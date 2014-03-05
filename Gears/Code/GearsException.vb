Imports Microsoft.VisualBasic
Imports System.Diagnostics
Imports System.Collections.Generic
Imports System.Runtime.Serialization

Namespace Gears

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    Public Enum ExceptionLevel
        Critical
        Alert
    End Enum

    Public Class GearsException
        Inherits System.Exception

        Protected Const MSG_DEBUG As String = "MSG_DEBUG"
        Private localSource As String = ""

        'コンストラクタ
        Public Sub New()
            MyBase.New()
        End Sub
        Public Sub New(message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)

        End Sub
        Public Sub New(message As String, ByVal ParamArray detail() As String)
            MyBase.New(message)
            addMsgDebug(detail)

        End Sub

        'メソッド
        <Obsolete("Messageプロパティを使用してください")> _
        Public Function getMsg() As String
            Return Message
        End Function
        <Obsolete("メソッドgetMsgDebugを使用してください")> _
        Public Function getMsgDetailFirst() As String
            Return getMsgDebug()
        End Function
        Public Function getLocalSource() As String
            Return localSource
        End Function
        Public Sub setLocalSource(Optional ByVal depth As Integer = 1)
            Dim localDepth As Integer = depth '1だと呼び出し元。2だとさらにその・・・と続く
            Dim st As New StackTrace
            Dim className As String = st.GetFrame(localDepth).GetMethod.ReflectedType.Name
            Dim methodName As String = st.GetFrame(localDepth).GetMethod.Name

            localSource = className + "@" + methodName
        End Sub
        Public Function getMsgDebug() As String
            Return getMsg(MSG_DEBUG)
        End Function
        Public Sub clearMsgDebug()
            clearMsg(MSG_DEBUG)
        End Sub
        Public Sub addMsgDebug(ByVal ParamArray msg() As String)
            addMsg(MSG_DEBUG, msg)
        End Sub

        Public Overrides Function toString() As String
            Return Message + vbCrLf + getMsgDebug()
        End Function

        Protected Function getMsg(ByVal kind As String) As String
            Dim result As String = ""
            If Not Data(kind) Is Nothing Then
                If TypeOf Data(kind) Is List(Of String) Then
                    For Each item As String In CType(Data(kind), List(Of String))
                        result += item + vbCrLf
                    Next
                Else
                    result = Data(kind)
                End If

            End If

            Return result
        End Function
        Protected Sub addMsg(ByVal kind As String, ByVal ParamArray msg() As String)
            If Data(kind) Is Nothing Then
                Data(kind) = New List(Of String)
            End If
            For i As Integer = 0 To msg.Length - 1
                Data(kind).add(msg(i))
            Next
        End Sub
        Protected Sub setMsg(ByVal kind As String, ByVal msg As String)
            If Not TypeOf Data(kind) Is List(Of String) Then
                Data(kind) = msg
            Else
                clearMsg(kind)
                addMsg(msg)
            End If
        End Sub
        Protected Sub clearMsg(ByVal kind As String)
            If Not Data(kind) Is Nothing Then
                If TypeOf Data(kind) Is List(Of String) Then
                    CType(Data(kind), List(Of String)).Clear()
                Else
                    Data(kind) = ""
                End If

            End If
        End Sub

    End Class

End Namespace
