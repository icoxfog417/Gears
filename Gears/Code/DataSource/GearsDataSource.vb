Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Collections.Generic
Imports Gears.Validation
Imports Gears.Util

Namespace Gears.DataSource

    ''' <summary>
    ''' データソースクラス作成用の抽象クラス
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class GearsDataSource
        Implements IDataSource

        ''' <summary>
        ''' データを選択するためのビュー/テーブル名。
        ''' </summary>
        Private _selectView As SqlDataSource = Nothing
        Public Property SelectView As SqlDataSource
            Get
                If _selectView IsNot Nothing Then
                    Return _selectView
                Else
                    Return TargetTable
                End If
            End Get
            Set(value As SqlDataSource)
                _selectView = value
            End Set
        End Property

        Private _targetTable As SqlDataSource = Nothing
        ''' <summary>
        ''' データの更新対象となるビュー/テーブル名。
        ''' </summary>
        Public Property TargetTable As SqlDataSource
            Get
                Return _targetTable
            End Get
            Set(value As SqlDataSource)
                _targetTable = value
                If _selectView Is Nothing Then
                    _selectView = _targetTable
                End If
            End Set
        End Property

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

        Private _connectionName As String = ""
        Public ReadOnly Property ConnectionName As String
            Get
                Return _connectionName
            End Get
        End Property

        Private _resultSet As New DataTable

        Public Sub New(ByVal conName As String)
            _connectionName = conName
        End Sub

        ''' <summary>
        ''' 選択/更新処理を実行する
        ''' </summary>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function execute(ByVal dto As GearsDTO) As DataTable

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

            Catch ex As GearsException
                Throw
            Catch ex As Exception
                Throw
            End Try

            Return _resultSet

        End Function

        ''' <summary>
        ''' 受け取ったDTOをSQL実行用のオブジェクトに変換する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function makeSqlBuilder(ByVal data As GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = Nothing
            Dim dbServer As DbServerType = SqlBuilder.GetDbServerType(ConnectionName)
            If Not data Is Nothing Then
                sqlb = data.toSqlBuilder()
                sqlb.DbServer = dbServer
            Else
                sqlb = New SqlBuilder(dbServer, ActionType.SEL)
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
        Protected MustOverride Sub setDataSource(ByVal sqlb As SqlBuilder)

        ''' <summary>
        ''' 実行用のSQLを作成するためのメソッド
        ''' </summary>
        ''' <param name="dataFrom"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function makeExecute(ByVal dataFrom As GearsDTO) As SqlBuilder
            Return makeSqlBuilder(dataFrom)
        End Function

        ''' <summary>
        ''' Selectを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gSelect(ByVal data As GearsDTO) As System.Data.DataTable Implements IDataSource.gSelect
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            Return gSelect(sqlb)
        End Function

        ''' <summary>
        ''' Selectを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function gSelect(ByVal sqlb As SqlBuilder) As System.Data.DataTable
            Try
                Dim executor As New GearsSqlExecutor(ConnectionName)
                _resultSet = executor.load(sqlb)
                convertResultSet(_resultSet, sqlb)
            Catch ex As Exception
                Throw
            End Try

            Return _resultSet

        End Function

        '項目名変換を行う
        Public Sub convertResultSet(ByRef dataSet As DataTable, ByVal sqlb As SqlBuilder)
            Dim conv As INameExchanger = sqlb.ItemColExchanger
            If Not conv Is Nothing AndAlso Not dataSet Is Nothing AndAlso dataSet.Columns.Count > 0 Then
                For Each col As DataColumn In dataSet.Columns

                    If Not String.IsNullOrEmpty(conv.changeColToItem(col.ColumnName)) Then
                        col.ColumnName = conv.changeColToItem(col.ColumnName)
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' ページサイズを指定したSelectを行う
        ''' </summary>
        ''' <param name="maximumRows"></param>
        ''' <param name="startRowIndex"></param>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gSelectPageBy(ByVal maximumRows As Integer, ByVal startRowIndex As Integer, ByVal data As GearsDTO) As DataTable Implements IDataSource.gSelectPageBy
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
        Public Function gSelectCount(ByVal data As GearsDTO) As Integer Implements IDataSource.gSelectCount
            Dim sqlb As SqlBuilder = makeSqlBuilder(data)
            Return gSelectCount(sqlb)
        End Function

        ''' <summary>
        ''' 件数の取得
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function gSelectCount(ByVal sqlb As SqlBuilder) As Integer
            Dim count As Integer = 0

            Try
                Dim executor As New GearsSqlExecutor(ConnectionName)
                count = executor.count(sqlb)
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
        Public Sub gInsert(ByVal data As GearsDTO) Implements IDataSource.gInsert
            Dim sqlb As SqlBuilder = makeExecute(data)
            gInsert(sqlb)
        End Sub

        ''' <summary>
        ''' Insertを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gInsert(ByVal sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        ''' <summary>
        ''' Updateを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gUpdate(ByVal data As GearsDTO) Implements IDataSource.gUpdate
            Dim sqlb As SqlBuilder = makeExecute(data)
            gUpdate(sqlb)
        End Sub

        ''' <summary>
        ''' Updateを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gUpdate(ByVal sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        ''' <summary>
        ''' Deleteを実行する
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gDelete(ByVal data As GearsDTO) Implements IDataSource.gDelete
            Dim sqlb As SqlBuilder = makeExecute(data)
            gDelete(sqlb)
        End Sub

        ''' <summary>
        ''' Deleteを実行する
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gDelete(ByVal sqlb As SqlBuilder)
            executeProcess(sqlb)
        End Sub

        ''' <summary>
        ''' Saveを実行する(一致するキーがある場合Update/なければInsert)
        ''' </summary>
        ''' <param name="data"></param>
        ''' <remarks></remarks>
        Public Sub gSave(ByVal data As GearsDTO)
            Dim sqlb As SqlBuilder = makeExecute(data)
            gSave(sqlb)
        End Sub

        ''' <summary>
        ''' Saveを実行する(一致するキーがある場合Update/なければInsert)
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Public Overridable Sub gSave(ByVal sqlb As SqlBuilder)
            If sqlb.Action = ActionType.INS Then
                gInsert(sqlb)
            ElseIf sqlb.Action = ActionType.UPD Then
                gUpdate(sqlb)
            Else
                'TODO INS/UPDの判断が行われていないケースの対処
            End If
        End Sub

        ''' <summary>
        ''' 更新系処理を行うメソッド
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Sub executeProcess(ByVal sqlb As SqlBuilder)
            beforeExecute(sqlb)

            Try
                Dim executor As New GearsSqlExecutor(ConnectionName)

                'Saveの場合、ActionTypeはINS/UPDのどちらかに編集されていることを前提とする
                executor.execute(sqlb)

                '実行結果を読み込む
                loadExecuted(sqlb)

            Catch ex As Exception
                Throw
            End Try

            afterExecute(sqlb)

        End Sub

        Protected Sub loadExecuted(ByVal sqlb As SqlBuilder)

            '実行後のデータを読み込む
            Dim sqlbForResult As New SqlBuilder(sqlb)
            sqlbForResult.DataSource = SelectView 'SELECTビューから、更新対象を読み込む

            If sqlb.Action = ActionType.INS Then
                'INSERTの場合フィルタ値がないため、設定値をフィルタ値に変換してセット
                For Each selection As SqlSelectItem In sqlb.Selection
                    If selection.IsKey And sqlbForResult.Filter(selection.Column) Is Nothing Then
                        sqlbForResult.addFilter(selection.toFilter)
                    End If
                Next
            End If

            '選択項目をクリア
            sqlbForResult.Selection.Clear()

            gSelect(sqlbForResult)

        End Sub

        ''' <summary>
        ''' 更新処理実行前に行われるトリガ処理
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <remarks></remarks>
        Protected Overridable Sub beforeExecute(ByVal sqlb As SqlBuilder)
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
        Protected Overridable Sub afterExecute(ByVal sqlb As SqlBuilder)
        End Sub

        ''' <summary>
        ''' 実行後の結果セットを取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function gResultSet() As System.Data.DataTable Implements IDataSource.gResultSet
            Return _resultSet
        End Function

        ''' <summary>
        ''' 結果セットから指定インデックス/行数の値を取得する
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="rowIndex"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Item(ByVal index As Integer, Optional ByVal rowIndex As Integer = 0) As Object
            Return DataSetReader.Item(_resultSet, index, rowIndex)
        End Function

        ''' <summary>
        ''' 結果セットから指定カラム/行数の値を取得する
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="rowIndex"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Item(ByVal index As String, Optional ByVal rowIndex As Integer = 0) As Object
            Return DataSetReader.Item(_resultSet, index, rowIndex)
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
            If _resultSet.Rows.Count > 0 Then
                For Each item As KeyValuePair(Of String, LockType) In LockCheckCol
                    Dim value As Object = DataSetReader.Item(_resultSet, item.Key)
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
        Protected Sub setLockCheckColValueToSql(ByVal sqlb As SqlBuilder)
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
