Imports Microsoft.VisualBasic
Imports System.Runtime.Serialization
Imports System.Web.UI

Namespace Gears.Binder

    ''' <summary>
    ''' コントロールへの値設定時の例外
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsDataAttachException
        Inherits GearsDataBindException

        'コンストラクタ
        Public Sub New()
            MyBase.New()
        End Sub
        Public Sub New(ByRef con As Control, Optional ByVal innerException As Exception = Nothing)
            MyBase.New(con, "コントロール " + con.ID + " への値設定に失敗しました", innerException)
            setSourceControl(con.ID)
        End Sub

    End Class

End Namespace
