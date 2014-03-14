Imports Microsoft.VisualBasic


Namespace Gears.Util

    Public Interface IFormItem

        ReadOnly Property ControlId() As String
        Property ControlKind() As String

        Property LabelText() As String
        Property IsEditable() As Boolean
        Property Width() As Integer
        Property Height() As Integer

        Property CssClass() As String

        Function getControl() As WebControl

        Sub setValue(ByVal value As String)
        Function getValue() As String

    End Interface

End Namespace
