Imports Microsoft.VisualBasic
Imports System.Runtime.Serialization
Imports System.Web.UI

Namespace Gears.Binder

    Public Class GearsDataAttachException
        Inherits GearsDataBindException

        'コンストラクタ
        Public Sub New()
            MyBase.New()
        End Sub
        Public Sub New(ByRef con As Control, Optional ByVal innerException As Exception = Nothing)
            MyBase.New(con, "コントロール " + con.ID + " へのデータ展開処理に失敗しました", innerException)
            setSourceControl(con.ID)
        End Sub

    End Class

End Namespace
