Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Collections.Generic
Imports Gears.Validation

Namespace Gears.DataSource

    ''' <summary>
    ''' データソースクラス作成用の抽象クラス
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class GearsDataSource
        Implements IDataSource

        ''' <summary>
        ''' SQL実行用オブジェクト
        ''' </summary>
        ''' <remarks></remarks>
        Protected GExecutor As GearsSqlExecutor = Nothing

        ''' <summary>
        ''' 楽観ロックの定義
        ''' </summary>
        ''' <remarks></remarks>
        Protected LockCheckCol As New Dictionary(Of String, LockType)

        Private _isMultiByte As Boolean = False
        ''' <summary>
        ''' マルチバイト対応が必要なデータソースか否か(日本語名テーブルなど)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsMultiByte() As Boolean
            Get
                Return _isMultiByte
            End Get
            Set(ByVal value As Boolean)
                _isMultiByte = value
            End Set
        End Property

        Private _modelValidator As AbsModelValidator = Nothing
        ''' <summary>
        ''' データソースを検証するためのバリデーションオブジェクト
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ModelValidator() As AbsModelValidator
            Get
                Return _modelValidator
            End Get
            Set(ByVal value As AbsModelValidator)
                _modelValidator = value
            End Set
        End Property

        Public Sub New(ByVal conName As String)
            GExecutor = New GearsSqlExecutor(conName)
        End Sub

        ''' <summary>
        ''' 接続文字列を取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getConnectionName() As String
            Return GExecutor.getConnectionName()
        End Function

        ''' <summary>
        ''' 選択/更新処理を実行する
        ''' </summary>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function execute(ByRef dto As GearsDTO) As DataTable

            Try
                'データベース更新処理
                Select Case dto.Action
                    Case ActionType.DEL
                        gDelete(dto)
                    Case ActionType.INS
                        gInsert(dto)
                    Case ActionType.SAVE
                        gSave(dto)
                    Case ActionType.SEL
                        If dto.isPaging Then
                            gSelectPageBy(dto.RowsInPage(), dto.RowIndexOfPage, dto)
                        Else
                            gSelect(dto)
                        End If
                    Case ActionType.UPD
                        gUpdate(dto)
                End Select

            Catch ex As Exception
                Throw

            End Try

            Return GExecutor.getDataSet

        End Function

        ''' <summary>
        ''' 受け取ったDTOをSQL実行用のオブジェクトに変換する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function makeSqlBuilder(ByRef data As GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = Nothing
            If Not data Is Nothing Then
                sqlb = data.generateSqlBuilder()
                sqlb.DbServer = GExecutor.getDbServerType()
            Else
                sqlb = New SqlBuilder(GExecutor.getDbServerType(), ActionType.SEL)
            End If
            sqlb.IsMultiByte = _isMultiByte

            If LockCheckCol.Count > 0 And sqlb.Selection.Where(Function(s) Not s.IsNoSelect).Count > 0 Then
                '楽観ロックチェックの指定がある場合、楽観ロック用のカラムを設定する
                setLockCheckColValueToSql(sqlb)
            End If

            'データソースの設定(下位クラスで実装必須)
            setDataSource(sqlb)

            Return sqlb

        End Function

        ''' <summary>
        ''' SQLの実行対象を設定するメソッド
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected MustOverride Sub setDataSource(ByRef sqlb As SqlBuilder)

        ''' <summary>
        ''' 実行用のSQLを作成するためのメソッド
        ''' </summary>
        ''' <param name="dataFrom"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function makeExecute(ByRef dataFrom As GearsDTO) As SqlBuilder
            Return makeSqlBuilder(dataFrom)
        End Function

        ''' <summary>
        ''' Selectを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gSelect(ByRef data As GearsDTO) As System.Data.DataTable Implements IDataSource.gSelect
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            Return gSelect(sqlb)
        End Function

        ''' <summary>
        ''' Selectを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function gSelect(ByRef sqlb As SqlBuilder) As System.Data.DataTable
            Try
                GExecutor.load(sqlb)
            Catch ex As Exception
                Throw
            End Try

            Return GExecutor.getDataSet

        End Function

        ''' <summary>
        ''' ページサイズを指定したSelectを行う
        ''' </summary>
        ''' <param name="maximumRows"></param>
        ''' <param name="startRowIndex"></param>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gSelectPageBy(ByVal maximumRows As Integer, ByVal startRowIndex As Integer, ByRef data As GearsDTO) As DataTable Implements IDataSource.gSelectPageBy
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            sqlb.setPaging(startRowIndex, maximumRows)
            Return gSelect(sqlb)

        End Function

        ''' <summary>
        ''' 件数の取得
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gSelectCount(ByRef data As GearsDTO) As Integer Implements IDataSource.gSelectCount
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            Return gSelectCount(sqlb)
        End Function

        ''' <summary>
        ''' 件数の取得
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function gSelectCount(ByRef sqlb As SqlBuilder) As Integer
            Dim count As Integer = 0

            Try
                count = GExecutor.count(sqlb)
            Catch ex As Exception
                Throw
            End Try

            Return count
        End Function

        ''' <summary>
        ''' Insertを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gInsert(ByRef data As GearsDTO) Implements IDataSource.gInsert
            Dim sqlb As SqlBuilder = makeExecute(data)
            gInsert(sqlb)
        End Sub

        ''' <summary>
        ''' Insertを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gInsert(ByRef sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        ''' <summary>
        ''' Updateを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gUpdate(ByRef data As GearsDTO) Implements IDataSource.gUpdate
            Dim sqlb As SqlBuilder = makeExecute(data)
            gUpdate(sqlb)
        End Sub

        ''' <summary>
        ''' Updateを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gUpdate(ByRef sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        ''' <summary>
        ''' Deleteを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gDelete(ByRef data As GearsDTO) Implements IDataSource.gDelete
            Dim sqlb As SqlBuilder = makeExecute(data)
            gDelete(sqlb)
        End Sub

        ''' <summary>
        ''' Deleteを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gDelete(ByRef sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        ''' <summary>
        ''' Saveを実行する(一致するキーがある場合Update/なければInsert)
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gSave(ByRef data As GearsDTO)
            Dim sqlb As SqlBuilder = makeExecute(data)
            gSave(sqlb)
        End Sub

        ''' <summary>
        ''' Saveを実行する(一致するキーがある場合Update/なければInsert)
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gSave(ByRef sqlb As SqlBuilder)
            If sqlb.Action = ActionType.INS Then
                gInsert(sqlb)
            ElseIf sqlb.Action = ActionType.UPD Then
                gUpdate(sqlb)
            End If
        End Sub

        ''' <summary>
        ''' 更新系処理を行うメソッド
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Sub executeProcess(ByRef sqlb As SqlBuilder)
            beforeExecute(sqlb)

            Try
                GExecutor.execute(sqlb) 'Saveの場合、ActionTypeはINS/UPDのどちらかに編集されている
            Catch ex As Exception
                Throw
            End Try

            afterExecute(sqlb)

        End Sub

        ''' <summary>
        ''' 更新処理実行前に行われるトリガ処理
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Overridable Sub beforeExecute(ByRef sqlb As SqlBuilder)
            Dim result As Boolean = True

            If Not ModelValidator Is Nothing Then
                result = False
                ModelValidator.Validate(sqlb)

                If ModelValidator.IsValid Then
                    result = True
                ElseIf ModelValidator.IsValidIgnoreAlert AndAlso sqlb.IsIgnoreAlert Then
                    result = True
                Else
                    ModelValidator.throwException()
                End If
            End If


        End Sub

        ''' <summary>
        ''' 更新処理実行後に行われるトリガ処理
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Overridable Sub afterExecute(ByRef sqlb As SqlBuilder)
        End Sub

        ''' <summary>
        ''' 実行後の結果セットを取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gResultSet() As System.Data.DataTable Implements IDataSource.gResultSet
            Return GExecutor.getDataSet
        End Function

        ''' <summary>
        ''' 結果セットから指定インデックス/行数の値を取得する
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="rowIndex"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Item(ByVal index As Integer, Optional ByVal rowIndex As Integer = 0) As Object
            Return GExecutor.getDataSetValue(index, rowIndex)
        End Function

        ''' <summary>
        ''' 結果セットから指定カラム/行数の値を取得する
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="rowIndex"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Item(ByVal index As String, Optional ByVal rowIndex As Integer = 0) As Object
            Return GExecutor.getDataSetValue(index, rowIndex)
        End Function

        ''' <summary>
        ''' 楽観ロック用の列の設定
        ''' </summary>
        ''' <param name="colname"></param>
        ''' <param name="ltype"></param>
        ''' <remarks></remarks>
        Public Sub addLockCheckCol(ByVal colname As String, ByVal ltype As LockType)
            If LockCheckCol.ContainsKey(colname) Then
                LockCheckCol(colname) = ltype
            Else
                LockCheckCol.Add(colname, ltype)

            End If
        End Sub

        ''' <summary>
        ''' SQL実行結果から楽観ロック用列の値を取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getLockCheckColValue() As List(Of SqlFilterItem)
            Dim param As New List(Of SqlFilterItem)
            If GExecutor.getDataSet.Rows.Count > 0 Then
                For Each item As KeyValuePair(Of String, LockType) In LockCheckCol
                    Dim value As Object = GearsSqlExecutor.getDataSetValue(item.Key, GExecutor.getDataSet)
                    param.Add(New SqlFilterItem(item.Key).eq(value))
                Next
            End If

            Return param

        End Function

        ''' <summary>
        ''' SQLに楽観ロック列を更新する値をセットする
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Sub setLockCheckColValueToSql(ByRef sqlb As SqlBuilder)
            For Each item As KeyValuePair(Of String, LockType) In LockCheckCol
                '元々設定されていた場合はそちらを優先する
                If sqlb.Selection(item.Key) Is Nothing AndAlso Not getLockTypeValue(item.Value) Is Nothing Then
                    sqlb.addSelection(SqlBuilder.S(item.Key).setValue(getLockTypeValue(item.Value)))
                End If
            Next
        End Sub

        ''' <summary>
        ''' 楽観ロックの各タイプに応じた更新値の取得
        ''' </summary>
        ''' <param name="ltype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getLockTypeValue(ByVal ltype As LockType) As Object
            Dim val As Object = ""
            Dim ndate As Date = Now()
            Select Case ltype
                Case LockType.UDATE
                    val = ndate
                Case LockType.UDATESTR
                    val = ndate.ToString("yyyyMMdd")
                Case LockType.UTIMESTR
                    val = ndate.ToString("HHmmss")
                Case LockType.VNUM
                    val = ndate.ToString("yyyyMMddHHmmss")
                Case LockType.USER 'HttpContextはGearsDataSourceからかなり遠いが。。。
                    If Not HttpContext.Current Is Nothing AndAlso Not HttpContext.Current.User Is Nothing AndAlso Not HttpContext.Current.User.Identity Is Nothing Then
                        val = HttpContext.Current.User.Identity.Name
                    Else
                        val = Nothing
                    End If
            End Select

            Return val

        End Function

    End Class

End Namespace
