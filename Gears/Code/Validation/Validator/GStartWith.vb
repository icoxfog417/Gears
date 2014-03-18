Imports Microsoft.VisualBasic

Namespace Gears.Validation.Validator

    ''' <summary>
    ''' 指定された文字列で始まることを検証する属性
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GStartWith
        Inherits GearsAttribute

        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>開始文字列を指定</summary>
        ''' <param name="p"></param>
        ''' <remarks></remarks>
        Public Sub New(p As String)
            Prefix = p
        End Sub

        Private _prefix As String
        ''' <summary>開始文字列</summary>
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

