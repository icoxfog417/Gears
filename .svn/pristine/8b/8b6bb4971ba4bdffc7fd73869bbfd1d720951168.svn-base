Imports Microsoft.VisualBasic

Namespace Gears

    Public Class GearsOptimisticLockCheckInvalid
        Inherits GearsDataIntegrityException

        Public Sub New()
            MyBase.New("他のユーザーにより更新されているため、更新出来ません。再読込みを行って下さい。")
        End Sub

        Public Sub New(ByVal ParamArray detail() As String)
            Me.New()
            addMsgDebug(detail)
        End Sub

    End Class

End Namespace
