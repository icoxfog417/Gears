Imports Microsoft.VisualBasic

Namespace Gears.Validation.Validator

    ''' <summary>
    ''' 必須の入力を検証する属性
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GRequired
        Inherits GearsAttribute

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
