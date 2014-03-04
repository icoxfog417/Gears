Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Collections.Generic

Namespace Gears

    Public MustInherit Class GearsDataSource
        Implements IDataSource

        Protected GExecutor As GearsSqlExecutor = Nothing
        Protected LockCheckCol As New Dictionary(Of String, LockType)

        'テーブル/列名のマルチバイト対応を行うか否か。行う場合、列名などにエスケープ記号が付く(Oracleなら"",SqlServerなら[])
        Private _isMultiByte As Boolean = False
        Public Property IsMultiByte() As Boolean
            Get
                Return _isMultiByte
            End Get
            Set(ByVal value As Boolean)
                _isMultiByte = value
            End Set
        End Property

        'このデータソースを検証するためのバリデーションオブジェクト
        Private _modelValidator As AbsModelValidator = Nothing
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

        Public Function getConnectionName() As String
            Return GExecutor.getConnectionName()
        End Function

        '相手先のデータに合わせて何らかの処理を行うためのメソッド
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

        '画面データ情報->Sqlb実行用オブジェクトへの変換
        Public Overridable Function makeSqlBuilder(ByRef data As GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = Nothing
            If Not data Is Nothing Then
                sqlb = data.generateSqlBuilder()
                sqlb.DbServer = GExecutor.getDbServerType()
            Else
                sqlb = New SqlBuilder(GExecutor.getDbServerType(), ActionType.SEL)
            End If
            sqlb.IsMultiByte = _isMultiByte

            If sqlb.Selection.Where(Function(s) Not s.IsNoSelect).Count > 0 Then
                'SELECT * 指定の場合は追加しない(更新の場合は明示的に列が指定されるため0以上の列が指定されているはず)。
                setLockCheckColValueToSql(sqlb)
            End If

            'データソースの設定(下位クラスで実装必須)
            setDataSource(sqlb)

            Return sqlb

        End Function
        Protected MustOverride Sub setDataSource(ByRef sqlb As SqlBuilder)

        Public Overridable Function makeExecute(ByRef dataFrom As GearsDTO) As SqlBuilder
            Return makeSqlBuilder(dataFrom)
        End Function

        'SQL実行用メソッド
        Public Function gSelect(ByRef data As GearsDTO) As System.Data.DataTable Implements IDataSource.gSelect
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            Return gSelect(sqlb)
        End Function
        Public Overridable Function gSelect(ByRef sqlb As SqlBuilder) As System.Data.DataTable
            Try
                GExecutor.load(sqlb)
            Catch ex As Exception
                Throw
            End Try

            Return GExecutor.getDataSet

        End Function

        Public Function gSelectPageBy(ByVal maximumRows As Integer, ByVal startRowIndex As Integer, ByRef data As GearsDTO) As DataTable Implements IDataSource.gSelectPageBy
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            sqlb.setPaging(startRowIndex, maximumRows)
            Return gSelect(sqlb)

        End Function

        Public Function gSelectCount(ByRef data As GearsDTO) As Integer Implements IDataSource.gSelectCount
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            Return gSelectCount(sqlb)
        End Function
        Public Overridable Function gSelectCount(ByRef sqlb As SqlBuilder) As Integer
            Dim count As Integer = 0

            Try
                count = GExecutor.count(sqlb)
            Catch ex As Exception
                Throw
            End Try

            Return count
        End Function

        Public Sub gInsert(ByRef data As GearsDTO) Implements IDataSource.gInsert
            Dim sqlb As SqlBuilder = makeExecute(data)
            gInsert(sqlb)
        End Sub
        Public Overridable Sub gInsert(ByRef sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        Public Sub gUpdate(ByRef data As GearsDTO) Implements IDataSource.gUpdate
            Dim sqlb As SqlBuilder = makeExecute(data)
            gUpdate(sqlb)
        End Sub
        Public Overridable Sub gUpdate(ByRef sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        Public Sub gDelete(ByRef data As GearsDTO) Implements IDataSource.gDelete
            Dim sqlb As SqlBuilder = makeExecute(data)
            gDelete(sqlb)
        End Sub
        Public Overridable Sub gDelete(ByRef sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub
        Public Sub gSave(ByRef data As GearsDTO)
            Dim sqlb As SqlBuilder = makeExecute(data)
            gSave(sqlb)
        End Sub
        Public Overridable Sub gSave(ByRef sqlb As SqlBuilder)
            If sqlb.Action = ActionType.INS Then
                gInsert(sqlb)
            ElseIf sqlb.Action = ActionType.UPD Then
                gUpdate(sqlb)
            End If
        End Sub

        Protected Sub executeProcess(ByRef sqlb As SqlBuilder)
            beforeExecute(sqlb)

            Try
                GExecutor.execute(sqlb) 'Saveの場合、ActionTypeはINS/UPDのどちらかに編集されている
            Catch ex As Exception
                Throw
            End Try

            afterExecute(sqlb)

        End Sub
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
        Protected Overridable Sub afterExecute(ByRef sqlb As SqlBuilder)
        End Sub


        '現在のデータセットの個数を確認(更新結果の確認に使用可能)
        Public Function gResultCount() As Integer Implements IDataSource.gResultCount
            Return GExecutor.getDataSetCount
        End Function

        Public Function gResultSet() As System.Data.DataTable Implements IDataSource.gResultSet
            Return GExecutor.getDataSet
        End Function

        Public Function Item(Of T)(ByVal index As T, Optional ByVal rowIndex As Integer = 0) As String
            Return GExecutor.getDataSetValue(index, rowIndex)
        End Function

        'ロックオブジェクト用
        Public Function getLockCheckColCount() As Integer
            Return LockCheckCol.Count
        End Function
        Public Sub addLockCheckCol(ByVal colname As String, ByVal ltype As LockType)
            If LockCheckCol.ContainsKey(colname) Then
                LockCheckCol(colname) = ltype
            Else
                LockCheckCol.Add(colname, ltype)

            End If
        End Sub
        Protected Sub setLockCheckColValueToSql(ByRef sqlb As SqlBuilder)
            For Each item As KeyValuePair(Of String, LockType) In LockCheckCol
                If sqlb.Selection(item.Key) Is Nothing AndAlso Not getLockTypeValue(item.Value) Is Nothing Then
                    sqlb.addSelection(SqlBuilder.newSelect(item.Key).setValue(getLockTypeValue(item.Value)))
                End If
            Next
        End Sub
        Public Function getLockedCheckColValue() As Dictionary(Of String, Object)
            Dim param As New Dictionary(Of String, Object)
            If GExecutor.getDataSet.Rows.Count > 0 Then
                For Each item As KeyValuePair(Of String, LockType) In LockCheckCol
                    Dim value As Object = GearsSqlExecutor.getDataSetValue(item.Key, GExecutor.getDataSet)
                    If Not value Is Nothing OrElse Not String.IsNullOrEmpty(value.ToString) Then
                        param.Add(item.Key, value)
                    End If
                Next
            End If

            Return param

        End Function

        'その他
        Public Function getLockTypeValue(ByVal ltype As LockType) As String
            Dim val As String = ""
            Dim ndate As Date = Now()
            Select Case ltype
                Case LockType.UDATE '未実装
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
