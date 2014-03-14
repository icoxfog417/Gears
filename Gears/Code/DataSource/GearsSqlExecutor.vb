Imports Microsoft.VisualBasic
Imports System.Configuration
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common

Namespace Gears.DataSource

    Public Class GearsSqlExecutor

        Private _factory As DbProviderFactory = Nothing
        Private _dbType As String = Nothing
        Public ReadOnly Property DbType As String
            Get
                Return _dbType
            End Get
        End Property

        Private _connectionString As String = Nothing
        Public ReadOnly Property ConnectionString As String
            Get
                Return _ConnectionString
            End Get
        End Property

        Private _dbServer As DbServerType = Nothing
        Public ReadOnly Property DbServer As DbServerType
            Get
                Return _dbServer
            End Get
        End Property

        Private _connectionName As String = ""
        Public ReadOnly Property ConnectionName As String
            Get
                Return _connectionName
            End Get
        End Property

        Private _connection As DbConnection = Nothing

        'コンストラクタ
        Public Sub New(ByVal conName As String)
            initConnection(conName)
        End Sub

        Private Sub initConnection(ByVal conName As String)
            
            '各種情報をセット
            _connectionName = conName
            _dbType = GetConnectionString(conName).ProviderName.ToString()
            _connectionString = GetConnectionString(conName).ToString()
            _dbServer = SqlBuilder.GetDbServerType(conName)

            '接続を作成
            _factory = DbProviderFactories.GetFactory(_dbType)
            _connection = _factory.CreateConnection()
            _connection.ConnectionString = _connectionString

        End Sub

        Public Shared Function GetConnectionString(ByVal conName As String) As ConnectionStringSettings
            If ConfigurationManager.ConnectionStrings(conName) Is Nothing Then
                Throw New GearsException("接続文字列 " + conName + " は定義されていません")
            Else
                Return ConfigurationManager.ConnectionStrings(conName)
            End If
        End Function

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
            Dim com As DbCommand = _connection.CreateCommand

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

        Public Function createSelectCount(ByRef sql As SqlBuilder) As DbCommand
            Dim com As DbCommand = createSqlCommand(sql, ActionType.SEL, False)
            com.CommandText = "SELECT count(*) FROM ( " + com.CommandText + " ) weiVgnitnuoc "
            Return com

        End Function

        'データセットの取得処理
        Public Function load(ByVal sql As SqlBuilder) As DataTable

            Dim com As DbCommand = Nothing
            Dim resultSet As New DataTable()
            Dim gex As GearsSqlException = Nothing

            Try
                _connection.Open()
                com = createSqlCommand(sql, ActionType.SEL) 'SELECTのコマンドを明示
                Using reader As DbDataReader = com.ExecuteReader()
                    resultSet.Load(reader)
                End Using

                If resultSet IsNot Nothing AndAlso resultSet.Columns.Count > 0 Then
                    'DataReaderはデフォルトで全行を読み取り専用にしてしまうので、編集可能なように設定を変更
                    For Each col As DataColumn In resultSet.Columns
                        col.ReadOnly = False
                    Next
                End If

            Catch ex As Exception
                gex = New GearsSqlException(ActionType.SEL, "データベースの読み込みに失敗しました", ex)
                gex.addDetail(ex.Message, com.CommandText)
            Finally
                If Not _connection Is Nothing Then
                    _connection.Close()
                End If
            End Try

            If Not gex Is Nothing Then
                Throw gex
            Else
                Return resultSet
            End If
        End Function

        Public Function count(ByVal sql As SqlBuilder) As Integer
            Dim com As DbCommand = Nothing
            Dim gex As GearsSqlException = Nothing
            Dim resultCount As Integer = 0

            Try

                _connection.Open()
                com = createSelectCount(sql)
                resultCount = CType(com.ExecuteScalar(), Integer)

            Catch ex As Exception
                gex = New GearsSqlException(ActionType.SEL, "データベースの読み込み(件数カウント)に失敗しました", ex)
                gex.addDetail(ex.Message, com.CommandText)
                Throw gex
            Finally
                If Not _connection Is Nothing Then
                    _connection.Close()
                End If
            End Try

            If Not gex Is Nothing Then
                Throw gex
            End If

            Return resultCount

        End Function

        Public Sub execute(ByVal sql As SqlBuilder)
            If sql.Action <> ActionType.SEL Then 'selectは対象外

                Dim com As DbCommand = Nothing
                Dim gex As GearsSqlException = Nothing

                Try
                    _connection.Open()
                    com = createSqlCommand(sql)
                    com.ExecuteNonQuery()

                Catch ex As Exception
                    gex = New GearsSqlException(sql.Action, "データベースの更新に失敗しました ", ex)
                    gex.addDetail(ex.Message, com.CommandText)
                Finally
                    If Not _connection Is Nothing Then
                        _connection.Close()
                    End If

                End Try

                If Not gex Is Nothing Then
                    Throw gex
                End If

            End If
        End Sub

        Public Sub execute(ByVal sqlbs As List(Of SqlBuilder))

            Dim com As DbCommand = Nothing
            Dim gex As GearsSqlException = Nothing
            Dim transaction As DbTransaction = Nothing
            Dim index As Integer = 0

            Try
                _connection.Open()
                transaction = _connection.BeginTransaction()

                For index = 0 To sqlbs.Count - 1
                    com = createSqlCommand(sqlbs(index))
                    com.Transaction = transaction
                    com.ExecuteNonQuery()
                Next

                transaction.Commit()
            Catch ex As Exception
                Try
                    transaction.Rollback()
                Catch exWhenRollback As Exception
                    gex = New GearsSqlException(sqlbs(index).Action, "トランザクションのロールバックに失敗しました ", exWhenRollback)
                    gex.addDetail(ex.Message, com.CommandText)
                End Try

                gex = New GearsSqlException(sqlbs(index).Action, "トランザクション処理に失敗しました ", ex)
                gex.addDetail(ex.Message, com.CommandText)
            Finally
                If Not _connection Is Nothing Then
                    _connection.Close()
                End If
            End Try

            If Not gex Is Nothing Then
                Throw gex
            End If

        End Sub

    End Class

End Namespace
