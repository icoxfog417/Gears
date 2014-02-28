Imports Microsoft.VisualBasic

Namespace Gears

    Public Class GearsTargetIsAlreadyExist
        Inherits GearsDataIntegrityException

        Public Sub New()
            MyBase.New("更新後のキーに一致するレコードが既に存在します")
        End Sub

        Public Sub New(ByVal ParamArray detail() As String)
            Me.New()
            addMsgDebug(detail)
        End Sub


    End Class


End Namespace
