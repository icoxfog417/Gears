Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Collections.Generic

Namespace Gears

    Public Class GDSTemplate
        Inherits GearsDataSource

        ''' <summary>
        ''' データを選択するためのビュー/テーブル名。
        ''' </summary>
        Protected ViewName As SqlDataSource = Nothing
        ''' <summary>
        ''' データの更新対象となるビュー/テーブル名。
        ''' </summary>
        Protected UpdateTarget As SqlDataSource = Nothing

        ''' <summary>
        ''' Select処理について、本当のSelectか、UpdateやInsert前の確認用Selectなのか判別する
        ''' </summary>
        Private inContext As ActionType = ActionType.NONE

        Public Sub New(ByVal conName As String, ByVal view As SqlDataSource, ByVal target As SqlDataSource)
            MyBase.New(conName)
            ViewName = view
            UpdateTarget = target
        End Sub
        Public Sub New(ByVal conName As String, ByVal singleTable As SqlDataSource)
            MyBase.New(conName)
            ViewName = singleTable
            UpdateTarget = singleTable
        End Sub

        '初期セットするview/targetが、join等を行う複雑なものである場合に対応。protectedとし、publicでの公開はしない(今のところ)
        Protected Sub New(ByVal conName As String)
            MyBase.New(conName)
        End Sub
        Protected Sub setView(ByVal view As SqlDataSource)
            ViewName = view
        End Sub
        Protected Sub setTarget(ByVal target As SqlDataSource)
            UpdateTarget = target
        End Sub
        Protected Sub setViewAndTarget(ByVal vt As SqlDataSource)
            ViewName = vt
            UpdateTarget = vt
        End Sub

        Public Overrides Function gSelect(ByRef sqlb As SqlBuilder) As System.Data.DataTable

            If inContext = ActionType.NONE Or inContext = ActionType.SEL Then
                GearsLogStack.setLog(GearsDTO.ActionToString(ActionType.SEL) + "処理を実行します", sqlb.confirmSql(ActionType.SEL))
            Else
                GearsLogStack.setLog(GearsDTO.ActionToString(inContext) + "処理実行前に、既存レコード確認のためSELECT処理を実行します", sqlb.confirmSql(ActionType.SEL))
            End If

            Return MyBase.gSelect(sqlb)

        End Function

        Public Overrides Function gSelectCount(ByRef sqlb As SqlBuilder) As Integer
            GearsLogStack.setLog(GearsDTO.ActionToString(ActionType.SEL) + "処理を実行します", sqlb.confirmSql(ActionType.SEL))
            Return MyBase.gSelectCount(sqlb)

        End Function

        Protected Overrides Sub setDataSource(ByRef sqlb As SqlBuilder)

            'デフォルトでセレクト用のデータソースを設定
            If sqlb Is Nothing OrElse sqlb.Action = ActionType.SEL Then
                sqlb.DataSource = ViewName
            Else
                sqlb.DataSource = UpdateTarget '更新用のデータソースを設定
            End If

        End Sub

        Protected Overrides Sub beforeExecute(ByRef sqlb As SqlBuilder)
            GearsLogStack.setLog("SQL 実行前のチェック処理を開始します")

            MyBase.beforeExecute(sqlb)

            GearsLogStack.setLog("チェック完了 " + GearsDTO.ActionToString(sqlb.Action) + "処理を実行します", sqlb.confirmSql(sqlb.Action))

        End Sub
        Protected Overrides Sub afterExecute(ByRef sqlb As SqlBuilder)
            GearsLogStack.setLog("SQL 実行後の処理を行います")
            MyBase.afterExecute(sqlb)
        End Sub

        Public Overrides Function makeExecute(ByRef dataFrom As GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = Nothing

            Dim whatType As GearsDTO = Nothing
            Dim result As Boolean = True
            Dim logStr As String = ""

            '事前チェック
            If Not dataFrom Is Nothing Then
                If dataFrom.getAtype = ActionType.NONE Or dataFrom.getAtype = ActionType.SEL Then
                    GearsLogStack.setLog("DTOに更新用処理タイプが設定されていません(設定されている更新タイプ：" + GearsDTO.ActionToString(dataFrom) + ")")
                    result = False
                End If
            Else
                GearsLogStack.setLog("DTOがNothingに設定されているため処理は行われません")
                result = False
            End If

            If Not result Then
                Return Nothing
            End If

            '更新タイプチェック
            Try
                whatType = confirmRecord(dataFrom)
                sqlb = makeSqlBuilder(whatType)
                GExecutor.setSql(sqlb)
            Catch ex As Exception
                Throw
            End Try

            Dim savePrefix As String = ""
            If dataFrom.Action = ActionType.SAVE Then
                savePrefix = "SAVE処理における"
            End If

            Dim validselections As Integer = sqlb.Selection.Where(Function(c) c.hasValue).Count
            Dim validfilters As Integer = sqlb.Filter.Where(Function(c) c.hasValue).Count

            Select Case whatType.Action
                Case ActionType.INS
                    If validselections = 0 Then
                        Throw New GearsDataIntegrityException(savePrefix + "INSERT定義が不完全です", "更新対象項目：未設定", sqlb.confirmSql(ActionType.INS))
                    End If

                Case ActionType.UPD
                    If Not sqlb.Selection.Count > 0 Or Not validfilters > 0 Then 'updateは空白にupdateすることもあるのでSelectについてはActiveCountを使わない
                        Throw New GearsDataIntegrityException(savePrefix + "UPDATE定義が不完全です", "更新対象項目：" + sqlb.Selection.Count.ToString + " 個 / Where句：" + validfilters.ToString + " 個", sqlb.confirmSql(ActionType.UPD))
                    End If
                Case ActionType.DEL
                    If validfilters > 0 Then
                        result = True
                    Else
                        Throw New GearsDataIntegrityException("WHERE区が設定されないDELETEです")
                    End If
                Case Else
                    GearsLogStack.setLog("処理区分：" + GearsDTO.ActionToString(whatType) + " DTOに処理タイプが設定されていないか、Nothingになっています ")

            End Select


            ''ログ出力
            'If dataFrom.getAtype = ActionType.SAVE Then
            '    GearsLogStack.setLog(GearsDTO.getAtypeString(dataFrom) + "処理を" + GearsDTO.getAtypeString(whatType) + "として実行します", GExecutor.toStringCommand(whatType.getAtype))
            'Else
            '    GearsLogStack.setLog(GearsDTO.getAtypeString(dataFrom) + "処理を実行します", GExecutor.toStringCommand(whatType.getAtype))
            'End If

            Return sqlb

        End Function

        Public Overridable Function confirmRecord(ByRef confirmData As GearsDTO) As GearsDTO
            '返り値用
            Dim data As GearsDTO = New GearsDTO(confirmData)

            'SQLBuilder用意
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)

            '処理用変数
            Dim isExist As Boolean = False  '更新対象データが存在するか
            Dim isLockCheckOk As Boolean = False    '更新前データが存在する場合、楽観的ロックの列の値を比較し、確認
            Dim isKeyEqualTarget As Boolean = True    'where区で設定された条件と更新データとして指定された値が等しいか否か
            Dim keySelection As List(Of SqlSelectItem) = sqlb.Selection.Where(Function(s) s.IsKey).ToList 'キー項目の列
            Dim keyFilter As List(Of SqlFilterItem) = sqlb.Filter.Where(Function(f) f.IsKey).ToList

            'ロックキーの確認
            Dim nowValue As Dictionary(Of String, Object) = Nothing 'DB上のロックキーの値
            Dim loadedValue As Dictionary(Of String, Object) = data.LockItem.ToDictionary(Function(lc) lc.Column, Function(lc) lc.Value) '画面にロードされたロックキーの値

            '処理開始：このメソッド内で発行するSelectは、確認用のSELECTであることを明示
            inContext = data.Action

            '処理対象レコードの確認 ----------------------------------------------------------------
            '更新項目にキーが含まれる場合->更新後のキーで存在確認、含まれない場合->フィルター値で存在確認
            If (Not keySelection Is Nothing AndAlso keySelection.Count > 0) Then '選択項目にキー有りの場合
                'フィルター側との値一致を確認
                For Each item As SqlSelectItem In keySelection
                    Dim filter As SqlFilterItem = sqlb.Filter(item.Column)
                    '例外処理
                    If String.IsNullOrEmpty(item.Value) Then
                        Throw New GearsDataIntegrityException("更新後のキーの値として空白が指定されているため、更新は行われません(項目:" + item.Column + "/値:" + item.Value + ")")
                    End If

                    If Not filter Is Nothing Then 'フィルター側にもキー有り
                        If filter.Value <> item.Value Then 'だが、値が異なる
                            isKeyEqualTarget = False

                            sqlb.removeFilter(item.Column)
                            sqlb.addFilter(item.filter) 'キー項目についてフィルタの値を変更(更新値にあわせる)
                            data.addFilter(item.filter) '後から追加したfilterで追加したものがSQLでは優先される
                        End If
                    Else
                        '必要なフィルター値を設定(通常、挿入処理の場合が該当)
                        'キーを更新したいが制約はかけない、というのは通常ありえない(必ず重複キーエラーになる)
                        sqlb.addFilter(item.filter)
                        data.addFilter(item.filter)
                    End If
                Next

            ElseIf (Not keyFilter Is Nothing AndAlso keyFilter.Count > 0) Then 'フィルター側にキー有りの場合(選択にはなし)
                '特に処理なし(そのままSELECTでOK)
            Else 'キー指定なし
                Throw New GearsDataIntegrityException("キー項目が設定されていません", "更新対象のテーブルのキーを表すGearsControlに対しsetAskey()を行うか、名称に__KEYを含めるかし、キー情報を設定してください")
            End If


            'データベース上の存在を確認
            Dim selResult As DataTable = gSelect(sqlb)

            '更新対象が存在するかの確認
            If selResult.Rows.Count > 0 Then 'select結果が0件以上であれば更新対象データ有り
                isExist = True

                'ロックキーが存在する場合、その一致を確認
                If loadedValue.Count > 0 Then
                    nowValue = getLockedCheckColValue()   'ロックキーの値を取得
                    If compareDic(loadedValue, nowValue) Then
                        isLockCheckOk = True
                    End If
                Else
                    GearsLogStack.setLog("ロックキーが指定されていないため、ロックチェックは実施されません")
                    isLockCheckOk = True    '存在しない場合、ロックはかけないのでOKにする
                End If
            End If

            '判定処理------------------------------------------------------------------------------
            If isExist Then '更新後のレコードが存在する(UPDATE想定)

                If Not isKeyEqualTarget Then '前後でキーが一致しない(キー変更更新)

                    If data.IsPermitOtherKeyUpdate Then 'キーの異なるレコードの更新を許可する
                        GearsLogStack.setLog("PermitOtherKeyUpdateオプションが指定されているため、キーの異なるデータについて強制的に更新を行います")
                        data.Action = (ActionType.UPD)
                    Else
                        '最後のカンマを除去してエラー出力
                        Throw New GearsTargetIsAlreadyExist("変更後のキーに合致するレコードが既に存在するため、更新は行われません (PermitOtherKeyUpdate:" + data.IsPermitOtherKeyUpdate.ToString + ")")
                    End If

                Else '前後でキーの一致する、通常のUPDATE

                    If isLockCheckOk Then   'ロック確認OK
                        data.Action = (ActionType.UPD)
                    Else
                        Throw New GearsOptimisticLockCheckInvalid("ロード時のロックキー:" + toStringDic(loadedValue) + ",現在のロックキー" + toStringDic(nowValue))
                    End If

                End If

            Else '更新後のレコードがない(INSERT想定)

                data.Action = ActionType.INS

            End If

            '結果出力------------------------------------------------------------------------------
            If confirmData.Action <> ActionType.SAVE Then
                Dim baseType As ActionType = confirmData.Action
                If confirmData.Action = ActionType.DEL Then
                    baseType = ActionType.UPD 'DELETEはUPDATEと同じ扱いにする
                End If

                If baseType <> data.Action Then '更新タイプが当初想定と異なる(全てエラー行き)
                    Dim gex As GearsRequestedActionInvalid = Nothing
                    If confirmData.Action = ActionType.DEL Then
                        gex = New GearsRequestedActionInvalid("削除対象レコードが存在しません")    'DELETEにもかかわらずINSERTと判断された場合、削除対象レコードがないことになる
                    Else
                        gex = New GearsRequestedActionInvalid(GearsDTO.ActionToString(confirmData) + " でリクエストされた処理は、データベースをチェックした結果 " + GearsDTO.ActionToString(data) + " と判断されました。")
                    End If
                    Throw gex
                Else
                    If confirmData.Action = ActionType.DEL Then 'delete = updateと判断していたため、元に戻す
                        data.Action = ActionType.DEL
                    End If
                End If
            End If

            '処理終了：確認用のSELECTであることを明示->終了
            inContext = ActionType.NONE

            Return data

        End Function
        Private Function compareDic(ByRef dic1 As Dictionary(Of String, Object), ByRef dic2 As Dictionary(Of String, Object)) As Boolean
            Dim isEqual As Boolean = True
            If dic1.Count = dic2.Count Then
                For Each item As KeyValuePair(Of String, Object) In dic1
                    If dic2.ContainsKey(item.Key) Then
                        If Not item.Value.Equals(dic2(item.Key)) Then
                            isEqual = False
                        End If
                    Else
                        isEqual = False
                    End If
                Next
            Else
                isEqual = False
            End If

            Return isEqual
        End Function
        Private Function toStringDic(ByRef dic As Dictionary(Of String, Object)) As String
            Dim str As String = ""
            For Each item As KeyValuePair(Of String, Object) In dic
                str += item.Key + ":" + If(item.Value IsNot Nothing, item.Value.ToString, "NULL") + "/"
            Next
            Return str
        End Function


    End Class

End Namespace

