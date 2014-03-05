Imports Microsoft.VisualBasic

Namespace Gears.Validation.Validator

    Public Class GMatch
        Inherits GearsAttribute

        Public Sub New()

        End Sub

        Public Sub New(p As String, Optional ByVal wm As Boolean = True)
            _pattern = p
            _whenMatch = wm
        End Sub

        Private _pattern As String = ""
        Public Property Pattern() As String
            Get
                Return _pattern
            End Get
            Set(ByVal value As String)
                _pattern = value
            End Set
        End Property

        Private _whenMatch As Boolean = True
        Public Property WhenMatch() As Boolean
            Get
                Return _whenMatch
            End Get
            Set(ByVal value As Boolean)
                _whenMatch = value
            End Set
        End Property


        Protected Overrides Sub Validate()
            initProperty()
            Dim regex As New System.Text.RegularExpressions.Regex(Pattern)
            Dim result As Boolean = regex.IsMatch(ValidateeValue)

            If _whenMatch = (Not result) Then
                ErrorMessage = "値が指定されたパターンにマッチしません"
                IsValid = False
            End If

        End Sub


    End Class
End Namespace

