Imports Microsoft.VisualBasic

Namespace Gears
    Public Class GStartWith
        Inherits GearsAttribute

        Public Sub New()

        End Sub

        Public Sub New(p As String)
            Prefix = p
        End Sub

        Private _prefix As String
        Public Property Prefix() As String
            Get
                Return _prefix
            End Get
            Set(ByVal value As String)
                _prefix = value
            End Set
        End Property

        Protected Overrides Sub Validate()

            If Not String.IsNullOrEmpty(ValidateeValue) And Not ValidateeValue.StartsWith(Prefix) Then
                IsValid = False
                ErrorMessage = "値は" + Prefix.ToString + " で始まる必要があります"
            End If

        End Sub


    End Class
End Namespace

