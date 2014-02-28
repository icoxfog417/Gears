Imports Microsoft.VisualBasic
Imports System.Web

Namespace Gears

    Public Class GearsLogStack

        Private Shared LogStack As New Dictionary(Of String, GearsLogStack)

        Private isTrace As Boolean = False
        Private logs As New List(Of GearsException)

        'インスタンス生成不可
        Private Sub New()
        End Sub

        'Shared
        Public Shared Sub clearLogStack()
            LogStack.Clear()
        End Sub
        Public Shared Sub traceOn()
            Dim logStore As New GearsLogStack
            logStore.setTrace(True)

            If getLogStack() Is Nothing Then
                LogStack.Add(getStoreKey(), logStore)
            End If

        End Sub
        Public Shared Sub traceEnd()
            If Not getLogStack() Is Nothing Then
                LogStack.Remove(getStoreKey())
            End If
        End Sub
        Public Shared Function IsTraceOn() As Boolean
            If Not getLogStackItem() Is Nothing Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Shared Sub setLog(ByVal msg As String, ByVal ParamArray msgDetail() As String)
            Dim logStore As GearsLogStack = getLogStackItem()

            If Not logStore Is Nothing Then

                Dim log As New GearsException(msg, msgDetail)
                log.setLocalSource(2)
                logStore.addLog(log)

            End If

        End Sub
        Public Shared Sub setLog(ByRef msg As GearsException)
            Dim logStore As GearsLogStack = getLogStackItem()

            If Not logStore Is Nothing Then
                logStore.addLog(msg)
            End If

        End Sub

        Public Shared Function getLogStack() As List(Of GearsException)
            If Not getLogStackItem() Is Nothing Then
                Return getLogStackItem().getLogs
            Else
                Return Nothing
            End If

        End Function

        Public Shared Function makeDisplayString() As String
            Dim logTemp As List(Of GearsException) = getLogStack()

            If Not logTemp Is Nothing Then
                Dim displayStr As String = ""
                Dim i As Integer = 1
                displayStr += "<h2 class=""gs-log-head"">ログ出力：リクエスト-> " + getStoreKey() + "</h2>"
                displayStr += "<table class=""gs-log-table"">"
                displayStr += "<tr class=""gs-log-table-head""><td>No</td><td>Location</td><td>Msg</td><td>Detail</td></tr>"

                For Each item As GearsException In logTemp
                    Dim tempStr As String = ""

                    tempStr += "<tr>"
                    'No
                    tempStr += "<td>" + i.ToString + "</td>"

                    'Location
                    Dim serverPath As String = HttpContext.Current.Request.PhysicalApplicationPath
                    If item.getLocalSource <> "" Then
                        tempStr += "<td>" + item.getLocalSource + "</td>" 'メソッド名の前で改行
                    ElseIf Not item.StackTrace Is Nothing Then
                        tempStr += "<td>" + item.StackTrace.Replace(serverPath, "").Replace(vbCrLf, "<br/>") + "</td>"
                    ElseIf Not item.InnerException Is Nothing Then
                        tempStr += "<td>" + item.InnerException.StackTrace.Replace(serverPath, "").Replace(vbCrLf, "<br/>") + "</td>"

                    End If

                    'Msg
                    tempStr += "<td>" + item.Message + "</td>"

                    'Detail
                    tempStr += "<td>"
                    tempStr += item.getMsgDebug.Replace(vbCrLf, "<br/>") '改行コード変換
                    tempStr += "<br/>"

                    tempStr += "</td></tr>"

                    displayStr += tempStr
                    i += 1

                Next

                displayStr += "</table>"

                Return displayStr

            Else
                Return ""
            End If

        End Function


        Private Sub setTrace(ByVal bool As Boolean)
            isTrace = bool
        End Sub
        Private Sub addLog(ByRef log As GearsException)
            If isTrace = True Then
                logs.Add(log)
            End If
        End Sub
        Private Function getLogs() As List(Of GearsException)
            Return logs
        End Function
        Private Shared Function getLogStackItem() As GearsLogStack
            Dim logStore As GearsLogStack = Nothing
            If LogStack.ContainsKey(getStoreKey()) Then
                logStore = LogStack(getStoreKey())
            End If

            Return logStore

        End Function
        Private Shared Function getStoreKey() As String
            If Not HttpContext.Current Is Nothing Then
                Return HttpContext.Current.Request.Path
            Else
                Return "execute on local"
            End If

        End Function


    End Class

End Namespace
