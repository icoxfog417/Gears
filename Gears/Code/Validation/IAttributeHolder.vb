Imports Microsoft.VisualBasic

Namespace Gears.Validation

    Public Interface IAttributeHolder

        Property GAttribute As GearsAttribute
        ReadOnly Property validateeValue As String
        ReadOnly Property GCssClass As String
        Function isValidateOk() As Boolean
        Function getValidationError() As String

    End Interface

End Namespace
