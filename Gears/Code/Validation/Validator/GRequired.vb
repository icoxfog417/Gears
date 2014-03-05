Imports Microsoft.VisualBasic

Namespace Gears.Validation.Validator

    Public Class GRequired
        Inherits GearsAttribute

        Public Sub New()
            MyBase.new()
        End Sub

        Protected Overrides Sub Validate()
            initProperty()
            Dim value = ValidateeValue

            If String.IsNullOrWhiteSpace(value) Then
                IsValid = False
                ErrorMessage = "必須入力である項目の値が空白です"
            End If

        End Sub

    End Class

End Namespace
