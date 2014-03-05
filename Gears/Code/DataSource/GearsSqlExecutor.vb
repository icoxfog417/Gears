Imports Microsoft.VisualBasic
Imports System.Configuration
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common

Namespace Gears.DataSource

    Public Class GearsSqlExecutor

        Private FACTORY As DbProviderFactory = Nothing
        Private DBTYPE As String = Nothing
        Private CONNECTION As String = Nothing
        Private DBSERVER As DbServerType = Nothing

        Private connectionName As String = ""
        Private SQL_CON As DbConnection = Nothing
        Private SQL_ADAPTER As DbDataAdapter = Nothing
        Private resultSet As DataTable = New DataTable()

        'コンストラクタ
        Public Sub New(ByVal conName As String)
            initConnection(conName)
        End Sub

        Private Sub initConnection(ByVal conName As String)

            '接続文字列の設定
            connectionName = conName
            DBTYPE = getDBType(conName)
            CONNECTION = getConnection(conName)
            DBSERVER = getDbServerType(DBTYPE)

            '各種オブジェクトの生成
            FACTORY = DbProviderFactories.GetFactory(DBTYPE)
            SQL_CON = FACTORY.CreateConnection()
            SQL_CON.ConnectionString = CONNECTION
            SQL_ADAPTER = FACTORY.CreateDataAdapter
            clearCommand()
        End Sub
        Private Sub clearCommand()

            SQL_ADAPTER.SelectCommand = Nothing
            SQL_ADAPTER.UpdateCommand = Nothing
            SQL_ADAPTER.InsertCommand = Nothing
            SQL_ADAPTER.DeleteCommand = Nothing

        End Sub

        Public Shared Function getDBType(ByVal conName As String) As String
            Dim tempType As String = ""
            tempType = ConfigurationManager.ConnectionStrings(conName).ProviderName.ToString()
            Return tempType
        End Function
        Public Function getDBType() As String
            Return DBTYPE
        End Function
        Public Shared Function getConnection(ByVal conName As String) As String
            Dim tempType As String = ""
            tempType = ConfigurationManager.ConnectionStrings(conName).ToString()
            Return tempType
        End Function

        Private Function getConnectionSection(ByVal conName As String) As ConnectionStringSettings
            Dim result As ConnectionStringSettings = ConfigurationManager.ConnectionStrings(conName)

            If result Is Nothing Then
                Dim directories As String() = conName.Split("/")
                If directories.Length > 1 Then
                    Dim conNameBare As String = directories(directories.Length - 1)
                    Dim openPath As String = ""
                    For i As Integer = 1 To directories.Length - 2
                        openPath += "/" + directories(i)
                        result = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(openPath).ConnectionStrings.ConnectionStrings(conNameBare)
                    Next
                End If
            End If

            Return result

        End Function

        Public Function getConnection() As String
            Return CONNECTION
        End Function
        Public Function getConnectionName() As String
            Return connectionName
        End Function
        Public Shared Function getDbServerType(ByVal dbType As String) As DbServerType
            Dim result As DbServerType = DbServerType.Oracle

            Select Case dbType
                Case "System.Data.SqlClient"
                    result = DbServerType.SQLServer
                Case "System.Data.OleDb"
                    result = DbServerType.OLEDB
                Case "System.Data.OracleClient"
                    result = DbServerType.Oracle
                Case "Oracle.DataAccess.Client"
                    result = DbServerType.Oracle
                Case "MySql.Data.MySqlClient"
                    result = DbServerType.MySQL
                Case "Devart.Data.PostgreSql"
                    result = DbServerType.PostgreSQL
                Case "System.Data.SQLite"
                    result = DbServerType.SQLite
            End Select
            Return result

        End Function
        Public Function getDbServerType() As DbServerType
            Return DBSERVER
        End Function


        'setter/getter
        Public Function getDataSet() As DataTable
            Return resultSet
        End Function
        Public Function getDataSetCount() As Integer
            If resultSet Is Nothing OrElse resultSet.Rows Is Nothing Then
                Return 0
            Else
                Return resultSet.Rows.Count
            End If
        End Function
        Public Function getDataSetValue(ByVal colindex As String, Optional ByVal rowIndex As Integer = 0) As Object
            Return getDataSetValue(Of String)(colindex, resultSet, rowIndex)

        End Function
        Public Function getDataSetValue(ByVal colindex As Integer, Optional ByVal rowIndex As Integer = 0) As Object
            Return getDataSetValue(Of Integer)(colindex, resultSet, rowIndex)

        End Function

        Public Shared Function getDataSetValue(ByVal colindex As String, ByRef dt As DataTable, Optional ByVal rowIndex As Integer = 0) As Object
            Return getDataSetValue(Of String)(colindex, dt, rowIndex)

        End Function
        Public Shared Function getDataSetValue(ByVal colindex As Integer, ByRef dt As DataTable, Optional ByVal rowIndex As Integer = 0) As Object
            Return getDataSetValue(Of Integer)(colindex, dt, rowIndex)

        End Function

        Private Shared Function getDataSetValue(Of T)(ByVal colindex As T, ByRef dt As DataTable, Optional ByVal rowIndex As Integer = 0) As Object
            Dim index As String = colindex.ToString
            Dim item As Object = Nothing

            Try
                If Not dt Is Nothing AndAlso Not dt.Rows Is Nothing Then

                    If TypeOf colindex Is Integer Then
                        item = dt.Rows(rowIndex).Item(CType(index, Integer))
                    Else
                        item = dt.Rows(rowIndex).Item(index)
                    End If

                End If

            Catch ex As Exception
                item = Nothing
            End Try

            Return item

        End Function
        Private Sub clearDataTable(ByRef dtab As DataTable)
            dtab.Clear()
            dtab.Columns.Clear()
        End Sub
        Private Function createBlankRow(ByRef dtab As DataTable) As DataRow
            If Not dtab Is Nothing Then
                Return dtab.NewRow()
            Else
                Return Nothing
            End If
        End Function

        'コマンドの設定
        Public Sub setSql(ByRef sql As SqlBuilder)
            '値初期化
            clearCommand()

            'SELECTは必須で設定する(更新後の結果セット取得のために必要)
            If sql.Action <> ActionType.INS Then
                setCommand(ActionType.SEL, createSqlCommand(sql, ActionType.SEL))
            Else
                'INSERTの場合フィルタ値がないため、設定値をフィルタ値に変換してセット
                Dim sqlbForInsert As New SqlBuilder(sql)
                For Each selection As SqlSelectItem In sql.Selection
                    If sqlbForInsert.Filter(selection.Column) Is Nothing Then
                        sqlbForInsert.addFilter(selection.toFilter)
                    End If
                Next
                setCommand(ActionType.SEL, createSqlCommand(sqlbForInsert, ActionType.SEL))
            End If

            'SELECT以外の更新系
            If sql.Action <> ActionType.SEL Then
                setCommand(sql.Action, createSqlCommand(sql))
            End If

        End Sub

        Public Function createSqlCommand(ByVal sql As SqlBuilder, Optional ByVal asType As ActionType = ActionType.NONE, Optional ByVal isNeedOrder As Boolean = True) As DbCommand
            Dim params As New Dictionary(Of String, Object)
            Dim sqlstr As String = ""
            Dim com As DbCommand = Nothing
            Dim aType As ActionType = sql.Action
            If asType <> ActionType.NONE Then
                aType = asType
            End If

            Select Case aType
                Case ActionType.SEL
                    sqlstr = sql.makeSelect(params, isNeedOrder)
                Case ActionType.UPD, ActionType.INS, ActionType.DEL
                    sqlstr = sql.makeSql(params, aType)
            End Select

            com = createSqlCommand(sqlstr, params, aType)
            If Not com Is Nothing Then
                If sql.CommandTimeout > -1 Then
                    com.CommandTimeout = sql.CommandTimeout
                End If
            End If

            Return com

        End Function

        Public Function createSqlCommand(ByVal sql As String, ByRef params As Dictionary(Of String, Object), ByVal atype As ActionType) As DbCommand
            Dim com As DbCommand = SQL_CON.CreateCommand

            com.CommandText = sql
            If Not params Is Nothing Then
                For Each item As KeyValuePair(Of String, Object) In params
                    Dim param As DbParameter = com.CreateParameter
                    param.ParameterName = item.Key
                    param.Value = item.Value
                    com.Parameters.Add(param)
                Next
            End If

            Return com

        End Function
        '各種コマンドの取得
        Public Function getCommand(ByVal at As ActionType) As DbCommand

            Select Case at
                Case ActionType.SEL
                    Return SQL_ADAPTER.SelectCommand
                Case ActionType.UPD
                    Return SQL_ADAPTER.UpdateCommand
                Case ActionType.INS
                    Return SQL_ADAPTER.InsertCommand
                Case ActionType.DEL
                    Return SQL_ADAPTER.DeleteCommand
                Case Else
                    Return (Nothing)
            End Select

        End Function

        '各種コマンドのセット
        Public Sub setCommand(ByVal at As ActionType, ByRef sc As DbCommand)
            Select Case at
                Case ActionType.SEL
                    SQL_ADAPTER.SelectCommand = sc
                Case ActionType.UPD
                    SQL_ADAPTER.UpdateCommand = sc
                Case ActionType.INS
                    SQL_ADAPTER.InsertCommand = sc
                Case ActionType.DEL
                    SQL_ADAPTER.DeleteCommand = sc
            End Select

        End Sub

        Public Sub setSqlForCounting(ByRef sql As SqlBuilder)
            Dim com As DbCommand = createSqlCommand(sql, ActionType.SEL, False)

            com.CommandText = "SELECT count(*) FROM ( " + com.CommandText + " ) weiVgnitnuoc "
            setCommand(ActionType.SEL, com)

        End Sub

        'データセットの取得処理
        Public Sub load(ByVal sql As SqlBuilder)
            setSql(sql)

            resultSet = New DataTable()
            Dim gex As GearsSqlException = Nothing

            Try
                SQL_CON.Open()
                SQL_ADAPTER.Fill(resultSet)
                formatResultSet(sql.ItemColExchanger)
            Catch ex As Exception
                gex = New GearsSqlException(ActionType.SEL, "データベースの読み込みに失敗しました " + toStringCommand(ActionType.SEL), ex)
                gex.addMsgDebug(ex.Message, toStringCommand(ActionType.SEL))
                clearDataTable(resultSet)
            Finally
                If Not SQL_CON Is Nothing Then
                    SQL_CON.Close()
                End If
            End Try

            If Not gex Is Nothing Then
                Throw gex
            End If

        End Sub
        Public Function count(ByVal sql As SqlBuilder) As Integer
            Dim counter As New DataTable
            Dim com As DbCommand = Nothing
            Dim resultCount As Integer = 0
            Dim gex As GearsSqlException = Nothing
            setSqlForCounting(sql)

            Try
                SQL_CON.Open()
                com = getCommand(ActionType.SEL)
                resultCount = CType(com.ExecuteScalar(), Integer)

            Catch ex As Exception
                gex = New GearsSqlException(ActionType.SEL, "データベースの読み込み(件数カウント)に失敗しました " + toStringCommand(ActionType.SEL), ex)
                gex.addMsgDebug(ex.Message, SQL_ADAPTER.SelectCommand.CommandText)
                Throw gex
            Finally
                If Not SQL_CON Is Nothing Then
                    SQL_CON.Close()
                End If
            End Try

            If Not gex Is Nothing Then
                Throw gex
            End If

            Return resultCount

        End Function
        Public Sub execute(ByVal sql As SqlBuilder)
            If sql.Action <> ActionType.SEL Then 'selectは対象外
                resultSet = New DataTable()

                Dim com As DbCommand = Nothing
                Dim executeType As ActionType = sql.Action
                Dim gex As GearsSqlException = Nothing

                setSql(sql)

                Try
                    SQL_CON.Open()
                    com = getCommand(executeType)
                    com.ExecuteNonQuery()
                    SQL_ADAPTER.Fill(resultSet) '更新後データをロード
                    formatResultSet(sql.ItemColExchanger)
                Catch ex As Exception
                    gex = New GearsSqlException(executeType, "データベースの更新に失敗しました ", ex)
                    gex.addMsgDebug(ex.Message, toStringCommand(executeType))
                    clearDataTable(resultSet)
                Finally
                    If Not SQL_CON Is Nothing Then
                        SQL_CON.Close()
                    End If

                End Try

                If Not gex Is Nothing Then
                    Throw gex
                End If

            End If
        End Sub
        Public Sub execute(ByVal sqlbs As List(Of SqlBuilder))
            resultSet = New DataTable()

            Dim com As DbCommand = Nothing
            Dim gex As GearsSqlException = Nothing

            Dim transaction As DbTransaction = Nothing
            Dim actionNow As ActionType
            Dim params As Dictionary(Of String, Object) = New Dictionary(Of String, Object)

            Try
                SQL_CON.Open()
                transaction = SQL_CON.BeginTransaction()

                For i As Integer = 0 To sqlbs.Count - 1
                    actionNow = sqlbs(i).Action

                    setSql(sqlbs(i))
                    com = getCommand(actionNow)

                    com.Transaction = transaction
                    com.ExecuteNonQuery()

                Next
                transaction.Commit()
            Catch ex As Exception
                Try
                    transaction.Rollback()
                Catch exWhenRollback As Exception
                    gex = New GearsSqlException(actionNow, "トランザクションのロールバックに失敗しました ", exWhenRollback)
                    gex.addMsgDebug(ex.Message, toStringCommand(actionNow))
                    clearDataTable(resultSet)
                End Try

                gex = New GearsSqlException(actionNow, "トランザクション処理に失敗しました ", ex)
                gex.addMsgDebug(ex.Message, toStringCommand(actionNow))
                clearDataTable(resultSet)

            Finally
                '初回(起点)の実行結果のみ保持(同じテーブルを更新するとは限らないため、全てを収めるのは不可能)
                Try
                    setSql(sqlbs(0))
                    com = getCommand(ActionType.SEL)
                    SQL_ADAPTER.Fill(resultSet) '更新後データをロード
                    formatResultSet(sqlbs(0).ItemColExchanger)

                    If Not SQL_CON Is Nothing Then
                        SQL_CON.Close()
                    End If
                Catch ex As Exception
                    gex = New GearsSqlException(ActionType.SEL, "更新後データの読み込み、コネクションのクローズに失敗しました " + toStringCommand(ActionType.SEL), ex)
                    gex.addMsgDebug(ex.Message, toStringCommand(ActionType.SEL))
                    clearDataTable(resultSet)
                End Try


            End Try

            If Not gex Is Nothing Then
                Throw gex
            End If

        End Sub

        '項目名変換を行う
        Private Sub formatResultSet(ByVal conv As INameExchanger)
            If Not conv Is Nothing AndAlso Not resultSet Is Nothing AndAlso resultSet.Columns.Count > 0 Then
                For Each col As DataColumn In resultSet.Columns

                    If Not String.IsNullOrEmpty(conv.changeColToItem(col.ColumnName)) Then
                        col.ColumnName = conv.changeColToItem(col.ColumnName)
                    End If
                Next
            End If
        End Sub

        'ユーティリティ
        Public Function toStringCommand(ByVal atype As ActionType) As String
            Dim str As String = ""
            Dim com As DbCommand = getCommand(atype)
            If Not com Is Nothing Then
                str += "SQL:" + com.CommandText + vbCrLf
                str += "PARAMS:"
                For Each param As DbParameter In com.Parameters
                    str += param.ParameterName + " = " + param.Value + vbCrLf
                Next
            End If

            Return str

        End Function



    End Class

End Namespace
