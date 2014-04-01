Imports System.Data.Common
Imports System.Configuration

Namespace GearsTest

    Public Class SimpleDBA

        '簡単なDB実行用ファンクション
        Public Shared Function executeSql(ByVal con As String, ByVal sql As String, Optional ByVal params As Dictionary(Of String, Object) = Nothing) As DataTable
            Dim dbprovider As DbProviderFactory = DbProviderFactories.GetFactory(ConfigurationManager.ConnectionStrings(con).ProviderName)
            Dim dbcon As DbConnection = dbprovider.CreateConnection
            dbcon.ConnectionString = ConfigurationManager.ConnectionStrings(con).ConnectionString

            'SQL解析
            Dim sqlParts As String() = Split(sql, " ")
            Dim isExecute As Boolean = True

            If Not sqlParts.Length > 0 Then
                Throw New Exception("SQL文が指定されていません")
            End If

            Select Case Trim(sqlParts(0).ToUpper)
                Case "SELECT"
                    isExecute = False
                Case "INSERT"
                Case "UPDATE"
                Case "DELETE"
            End Select

            'DBオープン
            dbcon.Open()
            Dim command As DbCommand = dbcon.CreateCommand
            command.CommandText = sql
            If Not params Is Nothing Then
                For Each item As KeyValuePair(Of String, Object) In params
                    Dim param As DbParameter = command.CreateParameter()
                    param.ParameterName = item.Key
                    param.Value = item.Value
                    command.Parameters.Add(param)
                Next
            End If


            '実行
            If isExecute Then
                Dim result As Integer = command.ExecuteNonQuery
                Return Nothing
            Else
                Dim result As New DataTable
                Dim reader As DbDataReader = command.ExecuteReader()
                result.Load(reader)
                Return result

            End If

        End Function

        Public Shared Function makeParameters(ParamArray params As String()) As Dictionary(Of String, Object)
            Dim dic As New Dictionary(Of String, Object)
            If params.Length > 0 And params.Length Mod 2 = 0 Then '0以上で偶数子の場合処理(param/valueの順で打ち込んでいく)

                For i As Integer = 0 To params.Length - 1 Step 2
                    dic.Add(params(i), params(i + 1))
                Next

            End If
            Return dic

        End Function

    End Class


End Namespace

