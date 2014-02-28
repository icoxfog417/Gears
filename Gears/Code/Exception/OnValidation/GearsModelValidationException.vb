Imports Microsoft.VisualBasic
Imports Gears.AbsModelValidator

Namespace Gears

    Public Class GearsModelValidationException
        Inherits GearsException

        Private _result As ValidationResults = Nothing
        Public ReadOnly Property Result() As ValidationResults
            Get
                Return _result
            End Get
        End Property

        'コンストラクタ
        Public Sub New(ByVal result As ValidationResults)
            MyBase.new(result.ErrorMessage)
            _result = result
        End Sub

    End Class


End Namespace
