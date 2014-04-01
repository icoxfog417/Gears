Imports Microsoft.VisualBasic
Imports Gears.Validation

Namespace Gears.Validation

    ''' <summary>
    ''' モデルバリデーションのエラーを表す例外
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsModelValidationException
        Inherits GearsException

        Private _result As ValidationResults = Nothing
        Public ReadOnly Property Result() As ValidationResults
            Get
                Return _result
            End Get
        End Property

        Public Sub New(ByVal result As ValidationResults)
            MyBase.New(result.ErrorMessage)
            _result = result
        End Sub

    End Class


End Namespace
