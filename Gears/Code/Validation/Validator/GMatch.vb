Imports Microsoft.VisualBasic

Namespace Gears.Validation.Validator

    ''' <summary>
    ''' 指定された正規表現の値と一致するか検証する属性
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GMatch
        Inherits GearsAttribute

        Public Sub New()

        End Sub

        Public Sub New(p As String)
            _pattern = p
        End Sub

        Private _pattern As String = ""
        ''' <summary>正規表現パターン</summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Pattern() As String
            Get
                Return _pattern
            End Get
            Set(ByVal value As String)
                _pattern = value
            End Set
        End Property

        Protected Overrides Sub Validate()
            initProperty()
            Dim regex As New System.Text.RegularExpressions.Regex(Pattern)
            Dim result As Boolean = regex.IsMatch(ValidateeValue)

            If Not result Then
                ErrorMessage = "値が指定されたパターンにマッチしません"
                IsValid = False
            End If

        End Sub

    End Class

End Namespace

