Imports Microsoft.VisualBasic

Namespace Gears

    Public Class GearsMultipleTargetExist
        Inherits GearsDataIntegrityException

        Public Sub New()
            MyBase.New("更新対象が複数存在します。キーが設定されていない可能性があります。")
        End Sub

        Public Sub New(ByVal ParamArray detail() As String)
            Me.New()
            addMsgDebug(detail)
        End Sub
    End Class

End Namespace
