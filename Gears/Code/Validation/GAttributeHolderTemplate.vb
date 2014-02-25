Imports Microsoft.VisualBasic

Namespace Gears

    Public Class GAttributeHolderTemplate
        Implements IAttributeHolder

        Public Sub New(ByVal v As String, ByVal attr As GearsAttribute)
            _validateeValue = v
            _gattributes = attr
            _gcssClass = _gattributes.CssClass
        End Sub

        Public Sub New(ByVal v As String, ByVal css As String)
            _validateeValue = v

            Dim attrCreator As New GearsAttributeCreator()
            attrCreator.createAttributesFromString(css)
            _gcssClass = attrCreator.getCssClass
            _gattributes = attrCreator.pack

        End Sub

        Private _gattributes As GearsAttribute = Nothing
        Public Property GAttribute As GearsAttribute Implements IAttributeHolder.GAttribute
            Get
                Return _gattributes
            End Get
            Set(value As GearsAttribute)
                If TypeOf _gattributes Is GearsAttributeContainer And TypeOf value Is GearsAttribute Then
                    CType(_gattributes, GearsAttributeContainer).addAttribute(value)
                Else
                    _gattributes = value
                End If
            End Set
        End Property

        Private _gcssClass As String = ""
        Public ReadOnly Property GCssClass As String Implements IAttributeHolder.GCssClass
            Get
                Return _gcssClass
            End Get
        End Property

        Public Function getValidatedMsg() As String Implements IAttributeHolder.getValidatedMsg
            Return _gattributes.ErrorMessage
        End Function

        Public Function isValidateOk() As Boolean Implements IAttributeHolder.isValidateOk
            Return _gattributes.isValidateOk(_validateeValue)
        End Function

        Private _validateeValue As String = ""
        Public ReadOnly Property validateeValue As String Implements IAttributeHolder.validateeValue
            Get
                Return _validateeValue
            End Get
        End Property

    End Class

End Namespace
