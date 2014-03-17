Imports Microsoft.VisualBasic
Imports System.Diagnostics
Imports System.Collections.Generic
Imports System.Runtime.Serialization

Namespace Gears

    ''' <summary>
    ''' エラーのレベルを表すEnum型
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum ExceptionLevel
        ''' <summary>通常のエラー</summary>
        Critical
        ''' <summary>警告</summary>
        Alert
    End Enum

    ''' <summary>
    ''' Gearsフレームワーク内の例外を管轄するクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsException
        Inherits System.Exception

        ''' <summary>詳細情報を格納するためのキー</summary>
        ''' <remarks></remarks>
        Protected Const MSG_DETAIL As String = "__MSG_DETAIL__"

        ''' <summary>例外の発生源をセット</summary>
        ''' <remarks></remarks>
        Private localSource As String = ""

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)

        End Sub

        ''' <summary>
        ''' 詳細情報を付与した例外を作成する<br/>
        ''' 詳細情報は文字列のListで管理され、表示時は改行で区切られて出力される。
        ''' </summary>
        ''' <param name="message"></param>
        ''' <param name="detail">詳細情報</param>
        ''' <remarks></remarks>
        Public Sub New(message As String, ByVal ParamArray detail() As String)
            MyBase.New(message)
            addDetail(detail)

        End Sub

        ''' <summary>
        ''' 発生位置を取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getLocalSource() As String
            Return localSource
        End Function

        ''' <summary>
        ''' 発生位置を設定する
        ''' </summary>
        ''' <param name="depth">setLocalSourceが呼び出された箇所=例外の発生源であるため、1がデフォルト値</param>
        ''' <remarks></remarks>
        Public Sub setLocalSource(Optional ByVal depth As Integer = 1)
            Dim localDepth As Integer = depth '1だと呼び出し元。2だとさらにその・・・と続く
            Dim st As New StackTrace
            Dim className As String = st.GetFrame(localDepth).GetMethod.ReflectedType.Name
            Dim methodName As String = st.GetFrame(localDepth).GetMethod.Name

            localSource = className + "@" + methodName
        End Sub

        ''' <summary>
        ''' 詳細情報を表示する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function MessageDetail() As String
            Return toStringDetail(MSG_DETAIL)
        End Function

        ''' <summary>
        ''' 詳細情報を追加する
        ''' </summary>
        ''' <param name="msg"></param>
        ''' <remarks></remarks>
        Public Sub addDetail(ByVal ParamArray msg() As String)
            addDetail(MSG_DETAIL, msg)
        End Sub

        ''' <summary>
        ''' 詳細情報をクリアする
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub clearDetail()
            clearDetail(MSG_DETAIL)
        End Sub

        Public Overrides Function toString() As String
            Return Message + vbCrLf + MessageDetail()
        End Function

        ''' <summary>
        ''' 指定されたキーの詳細情報を文字列化する
        ''' </summary>
        ''' <param name="detailKey"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function toStringDetail(ByVal detailKey As String) As String
            Dim result As String = ""
            If Not Data(detailKey) Is Nothing Then
                If TypeOf Data(detailKey) Is List(Of String) Then
                    For Each item As String In CType(Data(detailKey), List(Of String))
                        result += item + vbCrLf
                    Next
                Else
                    result = Data(detailKey)
                End If

            End If

            Return result
        End Function

        ''' <summary>
        ''' 指定されたキーの詳細情報を追加する
        ''' </summary>
        ''' <param name="detailKey"></param>
        ''' <param name="msg"></param>
        ''' <remarks></remarks>
        Protected Sub addDetail(ByVal detailKey As String, ByVal ParamArray msg() As String)
            If Data(detailKey) Is Nothing Then
                Data(detailKey) = New List(Of String)
            End If
            For i As Integer = 0 To msg.Length - 1
                Data(detailKey).add(msg(i))
            Next
        End Sub

        ''' <summary>
        ''' 指定されたキーの詳細情報をクリアする
        ''' </summary>
        ''' <param name="detailKey"></param>
        ''' <remarks></remarks>
        Protected Sub clearDetail(ByVal detailKey As String)
            If Not Data(detailKey) Is Nothing Then
                If TypeOf Data(detailKey) Is List(Of String) Then
                    CType(Data(detailKey), List(Of String)).Clear()
                Else
                    Data(detailKey) = ""
                End If

            End If
        End Sub

    End Class

End Namespace
