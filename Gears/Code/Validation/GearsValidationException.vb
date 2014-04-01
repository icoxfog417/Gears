Imports Microsoft.VisualBasic

Namespace Gears.Validation

    ''' <summary>
    ''' バリデーションの例外を表すクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsValidationException
        Inherits GearsException

        Public Sub New()
            MyBase.New()
        End Sub
        Public Sub New(message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)

        End Sub
        Public Sub New(message As String, ByVal ParamArray detail() As String)
            MyBase.New(message)
            addDetail(detail)

        End Sub
    End Class

End Namespace
