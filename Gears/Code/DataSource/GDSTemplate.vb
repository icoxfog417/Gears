Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Collections.Generic

Namespace Gears.DataSource

    ''' <summary>
    ''' データソースクラスのテンプレート
    ''' </summary>
    ''' <remarks></remarks>
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

        ''' <summary>
        ''' 接続文字列と、データソースとなるテーブルを指定しインスタンスを作成する
        ''' </summary>
        ''' <param name="conName"></param>
        ''' <param name="singleTable"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal conName As String, ByVal singleTable As SqlDataSource)
            MyBase.New(conName)
            ViewName = singleTable
            UpdateTarget = singleTable
        End Sub

        ''' <summary>
        ''' 接続文字列と、データソースとなるテーブル、ビューを指定しインスタンスを作成する<br/>
        ''' 更新系処理にはテーブル、選択系処理にはビューが使用される
        ''' </summary>
        ''' <param name="conName"></param>
        ''' <param name="view"></param>
        ''' <param name="target"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal conName As String, ByVal view As SqlDataSource, ByVal target As SqlDataSource)
            MyBase.New(conName)
            ViewName = view
            UpdateTarget = target
        End Sub

        '初期セットするview/targetが、join等を行う複雑なものである場合に対応。protectedとし、publicでの公開はしない(今のところ)
        ''' <summary>
        ''' 内部のみに公開するコンストラクタ<br/>
        ''' 何等かのロジックでデータソースを設定する場合に使用
        ''' </summary>
        ''' <param name="conName"></param>
        ''' <remarks></remarks>
        Protected Sub New(ByVal conName As String)
            MyBase.New(conName)
        End Sub

        ''' <summary>
        ''' 選択用のビューを設定する
        ''' </summary>
        ''' <param name="view"></param>
        ''' <remarks></remarks>
        Protected Sub setView(ByVal view As SqlDataSource)
            ViewName = view
        End Sub

        ''' <summary>
        ''' 更新ターゲットを設定する
        ''' </summary>
        ''' <param name="target"></param>
        ''' <remarks></remarks>
        Protected Sub setTarget(ByVal target As SqlDataSource)
            UpdateTarget = target
        End Sub

        ''' <summary>
        ''' ビュー/ターゲット両方に設定を行う
        ''' </summary>
        ''' <param name="vt"></param>
        ''' <remarks></remarks>
        Protected Sub setViewAndTarget(ByVal vt As SqlDataSource)
            ViewName = vt
            UpdateTarget = vt
        End Sub

        ''' <summary>
        ''' 選択処理を行う
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function gSelect(ByRef sqlb As SqlBuilder) As System.Data.DataTable

            If inContext = ActionType.NONE Or inContext = ActionType.SEL Then
                GearsLogStack.setLog(GearsDTO.ActionToString(ActionType.SEL) + "処理を実行します", sqlb.confirmSql(ActionType.SEL))
            Else
                GearsLogStack.setLog(GearsDTO.ActionToString(inContext) + "処理実行前に、既存レコード確認のためSELECT処理を実行します", sqlb.confirmSql(ActionType.SEL))
            End If

            Return MyBase.gSelect(sqlb)

        End Function

        ''' <summary>
        ''' 件数取得用のSQLを実行
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function gSelectCount(ByRef sqlb As SqlBuilder) As Integer
            GearsLogStack.setLog(GearsDTO.ActionToString(ActionType.SEL) + "処理を実行します", sqlb.confirmSql(ActionType.SEL))
            Return MyBase.gSelectCount(sqlb)

        End Function

        ''' <summary>
        ''' SqlBuilderのアクションに応じてデータソースを設定する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Overrides Sub setDataSource(ByRef sqlb As SqlBuilder)

            'デフォルトでセレクト用のデータソースを設定
            If sqlb Is Nothing OrElse sqlb.Action = ActionType.SEL Then
                sqlb.DataSource = ViewName
            Else
                sqlb.DataSource = UpdateTarget '更新用のデータソースを設定
            End If

        End Sub

        ''' <summary>
        ''' SQL実行前のチェック処理
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Overrides Sub beforeExecute(ByRef sqlb As SqlBuilder)
            GearsLogStack.setLog("SQL 実行前のチェック処理を開始します")

            MyBase.beforeExecute(sqlb)

            GearsLogStack.setLog("チェック完了 " + GearsDTO.ActionToString(sqlb.Action) + "処理を実行します", sqlb.confirmSql(sqlb.Action))

        End Sub

        ''' <summary>
        ''' SQL実行後の処理
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Overrides Sub afterExecute(ByRef sqlb As SqlBuilder)
            GearsLogStack.setLog("SQL 実行後の処理を行います")
            MyBase.afterExecute(sqlb)
        End Sub

        ''' <summary>
        ''' 更新SQLを作成するための処理
        ''' </summary>
        ''' <param name="dataFrom"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function makeExecute(ByRef dataFrom As GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = Nothing

            Dim adjustedDTO As GearsDTO = Nothing
            Dim result As Boolean = True
            Dim logStr As String = ""

            '事前チェック
            If dataFrom Is Nothing Then
                GearsLogStack.setLog("DTOがNothingに設定されているため処理は行われません")
                result = False
            ElseIf dataFrom.Action = ActionType.NONE Or dataFrom.Action = ActionType.SEL Then
                GearsLogStack.setLog("DTOに更新用処理タイプが設定されていません(設定されている更新タイプ：" + GearsDTO.ActionToString(dataFrom) + ")")
                result = False
            End If

            If Not result Then
                Try
                    adjustedDTO = confirmRecord(dataFrom) 'Saveの場合INS/UPDに調整など
                    sqlb = makeSqlBuilder(adjustedDTO)
                    GExecutor.setSql(sqlb)
                Catch ex As Exception
                    Throw
                End Try
            End If

            Return sqlb

        End Function

        ''' <summary>
        ''' Saveの場合のInsert/Update判定、また楽観ロックのチェックを行う
        ''' </summary>
        ''' <param name="confirmData"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function confirmRecord(ByRef confirmData As GearsDTO) As GearsDTO
            '返り値用
            Dim adjustedDTO As GearsDTO = New GearsDTO(confirmData)

            'SQLBuilder用意
            Dim sqlb As SqlBuilder = makeSqlBuilder(adjustedDTO)

            '処理用変数
            Dim keySelection As List(Of SqlSelectItem) = sqlb.Selection.Where(Function(s) s.IsKey And s.hasValue).ToList 'キー項目の列
            Dim keyFilter As List(Of SqlFilterItem) = sqlb.Filter.Where(Function(f) f.IsKey And f.hasValue).ToList

            'SAVE/Updateの場合、既存レコードの確認を行う
            If adjustedDTO.Action = ActionType.UPD Or adjustedDTO.Action = ActionType.SAVE Then

                '既存レコードの確認を行うSQLを作成
                Dim selSqlb As New SqlBuilder(sqlb, False) 'フィルタ/セレクタを削除しコピー
                selSqlb.DataSource = Me.UpdateTarget 'ビューではなく、元テーブルに対しSELECTを行う(キー項目は必ず元テーブルに含まれるが、ビューはその限りではない)

                If keyFilter.Count > 0 Then 'キー選択がある場合、フィルタとしてそのまま追加
                    Dim isKeyUpdateOccur As Boolean = False
                    keyFilter.ForEach(Sub(f)
                                          selSqlb.addFilter(f)
                                          If Not f.Value = sqlb.Selection(f.Column).Value Then isKeyUpdateOccur = True
                                      End Sub)

                    'キーを更新するUpdateは、許可されている場合のみOK
                    If isKeyUpdateOccur And Not adjustedDTO.IsPermitOtherKeyUpdate Then
                        Throw New GearsSqlException("キーを更新する処理は許可されていません(PermitOtherKeyUpdate:False)")
                    End If

                ElseIf keySelection.Count > 0 Then 'キー選択がなく、キーの更新がある場合それをフィルタとして設定
                    keySelection.ForEach(Sub(s) selSqlb.addFilter(s.toFilter))
                Else
                    Throw New GearsSqlException("キー項目が設定されていないか空白です", "更新対象のテーブルのキーを表すGearsControlに対しsetAskey()を行うか、名称に__KEYを含めるかし、キー情報を設定してください")
                End If

                'SELECTを実行
                inContext = adjustedDTO.Action '発行するSelectが確認用のSELECTであることを明示
                Dim selResult As DataTable = gSelect(selSqlb)
                inContext = ActionType.NONE '終了

                '既存レコードを確認
                If selResult.Rows.Count > 0 Then 'select結果が0件以上であれば更新対象データ有り
                    Dim isLockCheckOk As Boolean = True
                    If adjustedDTO.Action = ActionType.SAVE Then adjustedDTO.Action = ActionType.UPD

                    'ロックキーが存在する場合、その一致を確認
                    If adjustedDTO.LockItem.Count > 0 Then
                        Dim lockValues As List(Of SqlFilterItem) = getLockCheckColValue()
                        For Each lc As SqlFilterItem In adjustedDTO.LockItem
                            If lockValues.Where(Function(f) f.Column = lc.Column And f.Value = lc.Value).Count = 0 Then isLockCheckOk = False
                        Next

                        If Not isLockCheckOk Then
                            Dim lockInfo As String = ""
                            lockValues.ForEach(Sub(f) lockInfo += f.Column + ":" + f.Value.ToString + " ")
                            Throw New GearsOptimisticLockException("他のユーザーにより更新されています:" + Trim(lockInfo))
                        End If
                    Else

                    End If
                Else
                    'ActionType=UPDの場合、更新対象がないことになるがエラーにはしない
                    If adjustedDTO.Action = ActionType.SAVE Then adjustedDTO.Action = ActionType.INS
                End If

            End If

            Return adjustedDTO

        End Function

    End Class

End Namespace

