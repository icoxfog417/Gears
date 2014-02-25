Imports Microsoft.VisualBasic

Namespace Gears

    Public Class GearsRequestedActionInvalid
        Inherits GearsDataIntegrityException

        Public Sub New()
            MyBase.New("リクエストされた操作は実行されません")
        End Sub

        Public Sub New(ByVal ParamArray detail() As String)
            Me.New()
            addMsgDebug(detail)
        End Sub
    End Class

End Namespace
