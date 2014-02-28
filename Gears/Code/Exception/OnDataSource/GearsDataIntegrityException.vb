Imports Microsoft.VisualBasic

Namespace Gears

    Public Class GearsDataIntegrityException
        Inherits GearsException

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
    End Class

End Namespace
