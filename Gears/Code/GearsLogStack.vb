Imports Microsoft.VisualBasic
Imports System.Web

Namespace Gears

    ''' <summary>
    ''' ログを記録するためのクラス<br/>
    ''' Sharedで履歴を管理し、各インスタンスをShared配列に格納する形でログを収集する
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsLogStack

        ''' <summary>
        ''' ログを格納した配列
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared _LogStack As New Dictionary(Of String, GearsLogStack)

        Private _isTrace As Boolean = False
        Private _logs As New List(Of GearsException)

        'インスタンス生成不可
        Private Sub New()
        End Sub

        ''' <summary>
        ''' ログをクリアする
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub clearLogStack()
            _LogStack.Clear()
        End Sub

        ''' <summary>
        ''' 現在位置でログトレースをオンにする<br/>
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub traceOn()
            Dim logStore As New GearsLogStack
            logStore.setTrace(True)

            If getLogStack() Is Nothing Then
                _LogStack.Add(getStoreKey(), logStore)
            End If

        End Sub

        ''' <summary>
        ''' トレースをオフにする
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub traceEnd()
            If Not getLogStack() Is Nothing Then
                _LogStack.Remove(getStoreKey())
            End If
        End Sub

        ''' <summary>
        ''' トレースがオンになっているか確認する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function IsTraceOn() As Boolean
            If Not getLogStackItem() Is Nothing Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' メッセージログを書き込む(通常のメッセージで、エラーとしては扱われない)
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <param name="msgDetail"></param>
        ''' <remarks></remarks>
        Public Shared Sub setLog(ByVal msg As String, ByVal ParamArray msgDetail() As String)
            Dim log As New GearsLog(msg, msgDetail)
            log.setLocalSource(2)
            setLog(log)
        End Sub

        ''' <summary>呼出階層を指定してメッセージを書き込み</summary>
        ''' <param name="msg"></param>
        ''' <param name="depth">通常は、呼出元メソッド>setLogメソッドで2階層。ログ出力用の関数などを使用している場合は、さらに+1する</param>
        ''' <param name="msgDetail"></param>
        ''' <remarks></remarks>
        Public Shared Sub setLog(ByVal msg As String, ByVal depth As Integer, ByVal ParamArray msgDetail() As String)
            Dim log As New GearsLog(msg, msgDetail)
            log.setLocalSource(depth)
            setLog(log)
        End Sub

        ''' <summary>
        ''' 例外からログを書き込む
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <remarks></remarks>
        Public Shared Sub setLog(ByRef msg As GearsException)
            Dim logStore As GearsLogStack = getLogStackItem()

            If Not logStore Is Nothing Then
                logStore.addLog(msg)
            End If

        End Sub

        ''' <summary>
        ''' ストアされたログを取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function getLogStack() As List(Of GearsException)
            If Not getLogStackItem() Is Nothing Then
                Return getLogStackItem().getLogs
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>
        ''' ログをHTMLで出力する<br/>
        ''' * gs-log-head:ログのタイトル
        ''' * gs-log-table:ログ出力を表示しているtableのスタイル
        ''' * gs-log-table-head":ログ出力を表示しているtableのヘッダのスタイル
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
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

                    tempStr += "<tr " + If(Not TypeOf item Is GearsLog, "class=""g-msg-error""", "") + ">" 'エラーの場合、スタイルを設定
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
                    tempStr += item.MessageDetail.Replace(vbCrLf, "<br/>") '改行コード変換
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

        ''' <summary>
        ''' トレースのON/OFFを設定する
        ''' </summary>
        ''' <param name="bool"></param>
        ''' <remarks></remarks>
        Private Sub setTrace(ByVal bool As Boolean)
            _isTrace = bool
        End Sub

        ''' <summary>
        ''' ログを書き込む
        ''' </summary>
        ''' <param name="log"></param>
        ''' <remarks></remarks>
        Private Sub addLog(ByRef log As GearsException)
            If _isTrace = True Then
                _logs.Add(log)
            End If
        End Sub

        ''' <summary>
        ''' ログを取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function getLogs() As List(Of GearsException)
            Return _logs
        End Function

        ''' <summary>
        ''' 自身のストアキーのオブジェクトを取り出す
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function getLogStackItem() As GearsLogStack
            Dim logStore As GearsLogStack = Nothing
            If _LogStack.ContainsKey(getStoreKey()) Then
                logStore = _LogStack(getStoreKey())
            End If

            Return logStore

        End Function

        ''' <summary>
        ''' 自身のストアキーを作成する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function getStoreKey() As String
            If Not HttpContext.Current Is Nothing Then
                Return HttpContext.Current.Request.Path
            Else
                Return "execute on local"
            End If

        End Function


    End Class

End Namespace
