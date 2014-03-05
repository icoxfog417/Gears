Imports Microsoft.VisualBasic
Imports System.Runtime.Serialization
Imports System.Web.UI

Namespace Gears.Binder

    Public Class GearsDataBindException
        Inherits GearsException

        Protected Const MSG_CONTROL As String = "MSG_CONTROL"

        'コンストラクタ
        Public Sub New()
            MyBase.New()
        End Sub
        Public Sub New(ByRef con As Control, Optional ByVal innerException As Exception = Nothing)
            MyBase.New("コントロール " + con.ID + " のデータバインド処理に失敗しました", innerException)
            setSourceControl(con.ID)
        End Sub
        Public Sub New(ByRef con As Control, message As String, Optional ByVal innerException As Exception = Nothing)
            MyBase.New(message, innerException)
            setSourceControl(con.ID)
        End Sub

        'メソッド
        Public Function getSourceControl() As String
            Return getMsg(MSG_CONTROL)
        End Function
        Public Sub clearSourceControl()
            clearMsg(MSG_CONTROL)
        End Sub
        Public Sub setSourceControl(ByVal con As String)
            setMsg(MSG_CONTROL, con)
        End Sub


    End Class

End Namespace
