Imports Microsoft.VisualBasic
Imports System.Configuration
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common

Namespace Gears.DataSource

    ''' <summary>
    ''' SqlBuilderを受け取り、データベースに対しSQLを実行する
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsSqlExecutor

        Private _factory As DbProviderFactory = Nothing
        Private _dbType As String = Nothing
        ''' <summary>データベース種別</summary>
        Public ReadOnly Property DbType As String
            Get
                Return _dbType
            End Get
        End Property

        Private _connectionString As String = Nothing
        ''' <summary>接続設定文字列</summary>
        Public ReadOnly Property ConnectionString As String
            Get
                Return _connectionString
            End Get
        End Property

        Private _dbServer As DbServerType = Nothing
        ''' <summary>データベース種別(Enum)</summary>
        Public ReadOnly Property DbServer As DbServerType
            Get
                Return _dbServer
            End Get
        End Property

        Private _connectionName As String = ""
        ''' <summary>接続文字列</summary>
        Public ReadOnly Property ConnectionName As String
            Get
                Return _connectionName
            End Get
        End Property

        Private _sqlElapsedTime As Long = 0
        ''' <summary>SQL実行時間(ミリ秒)を取得する</summary>
        Public Function SqlElapsedTime() As Long
            Return _sqlElapsedTime
        End Function

        Public Function SqlElapsedTime(ByVal scale As Long, Optional ByVal round As Integer = 3) As String
            Dim scaled As Decimal = _sqlElapsedTime / scale
            Return Math.Round(scaled, round).ToString
        End Function

        Private _connection As DbConnection = Nothing

        Public Sub New(ByVal conName As String)
            initConnection(conName)
        End Sub

        ''' <summary>
        ''' 接続文字列からデータベースの情報を取得し、設定する
        ''' </summary>
        ''' <param name="conName"></param>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' 設定ファイルから接続文字列に定義された接続設定を取得する
        ''' </summary>
        ''' <param name="conName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetConnectionString(ByVal conName As String) As ConnectionStringSettings
            If ConfigurationManager.ConnectionStrings(conName) Is Nothing Then
                Throw New GearsException("接続文字列 " + conName + " は定義されていません")
            Else
                Return ConfigurationManager.ConnectionStrings(conName)
            End If
        End Function

        ''' <summary>
        ''' SqlBuilderからDbCommandを作成する
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="asType"></param>
        ''' <param name="isNeedOrder"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' SqlBuilderからDbCommandを作成する
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="params"></param>
        ''' <param name="atype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' 件数を取得するためのSQLを生成する
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function createSelectCount(ByRef sql As SqlBuilder) As DbCommand
            Dim com As DbCommand = createSqlCommand(sql, ActionType.SEL, False)
            com.CommandText = "SELECT count(*) FROM ( " + com.CommandText + " ) weiVgnitnuoc "
            Return com

        End Function

        ''' <summary>
        ''' データのロード処理
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function load(ByVal sql As SqlBuilder) As DataTable

            Dim com As DbCommand = Nothing
            Dim resultSet As New DataTable()
            Dim gex As GearsSqlException = Nothing

            Try
                Dim sw As New Diagnostics.Stopwatch '実行時間を計測
                sw.Start()

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

                sw.Stop()
                _sqlElapsedTime = sw.ElapsedMilliseconds

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

        ''' <summary>
        ''' データ件数の取得処理
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function count(ByVal sql As SqlBuilder) As Integer
            Dim com As DbCommand = Nothing
            Dim gex As GearsSqlException = Nothing
            Dim resultCount As Integer = 0

            Try
                Dim sw As New Diagnostics.Stopwatch '実行時間を計測
                sw.Start()

                _connection.Open()
                com = createSelectCount(sql)
                resultCount = CType(com.ExecuteScalar(), Integer)

                sw.Stop()
                _sqlElapsedTime = sw.ElapsedMilliseconds

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

        ''' <summary>
        ''' データベースの実行処理
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <remarks></remarks>
        Public Sub execute(ByVal sql As SqlBuilder)
            If sql.Action <> ActionType.SEL Then 'selectは対象外

                Dim com As DbCommand = Nothing
                Dim gex As GearsSqlException = Nothing

                Try
                    Dim sw As New Diagnostics.Stopwatch '実行時間を計測
                    sw.Start()

                    _connection.Open()
                    com = createSqlCommand(sql)
                    com.ExecuteNonQuery()

                    sw.Stop()
                    _sqlElapsedTime = sw.ElapsedMilliseconds

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

        ''' <summary>
        ''' データベースへの実行処理<br/>
        ''' 受け取ったSqlBuilderの配列を、トランザクションで処理する
        ''' </summary>
        ''' <param name="sqlbs"></param>
        ''' <remarks></remarks>
        Public Sub execute(ByVal sqlbs As List(Of SqlBuilder))

            Dim com As DbCommand = Nothing
            Dim gex As GearsSqlException = Nothing
            Dim transaction As DbTransaction = Nothing
            Dim index As Integer = 0

            Try
                Dim sw As New Diagnostics.Stopwatch '実行時間を計測
                sw.Start()

                _connection.Open()
                transaction = _connection.BeginTransaction()

                For index = 0 To sqlbs.Count - 1
                    com = createSqlCommand(sqlbs(index))
                    com.Transaction = transaction
                    com.ExecuteNonQuery()
                Next

                transaction.Commit()

                sw.Stop() 'Open/Commitまでにかかる時間を計測(ラップを計測してもいいかもしれないが)
                _sqlElapsedTime = sw.ElapsedMilliseconds

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
