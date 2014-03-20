Imports Gears
Imports Gears.Util
Imports Gears.Binder

Namespace GearsTest

    Public Class TestFormItem
        Inherits UserControl
        Implements IFormItem

        Private _control As Control = Nothing

        Public ReadOnly Property ControlId As String Implements IFormItem.ControlId
            Get
                Return _control.ID
            End Get
        End Property

        Public Property ControlKind As String Implements IFormItem.ControlKind

        Public Function getControl() As WebControl Implements IFormItem.getControl
            Return _control
        End Function

        Public Function getValue() As String Implements IFormItem.getValue
            Return _control.toGControl.getValue
        End Function

        Public Property Height As Integer Implements IFormItem.Height
        Public Property Width As Integer Implements IFormItem.Width
        Public Property CssClass As String Implements IFormItem.CssClass

        Public Property IsEditable As Boolean Implements IFormItem.IsEditable
        Public Property LabelText As String Implements IFormItem.LabelText

        Public Property ConnectionName As String Implements IFormItem.ConnectionName
        Public Property DSNamespace As String Implements IFormItem.DSNamespace

        Public Sub setValue(value As String) Implements IFormItem.setValue
            _control.toGControl.setValue(value)
        End Sub

        Public Sub New(ByVal controlKind As String, ByVal itemName As String, Optional ByVal value As String = "")
            Dim id As String = controlKind.ToLower + itemName
            _control = ControlBuilder.createControl(id)
            Me.ID = itemName
            Me.Controls.Add(_control)
            setValue(value)
        End Sub

    End Class

End Namespace
