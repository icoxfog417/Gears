Imports Microsoft.VisualBasic

Namespace Gears.Validation.Validator

    Public Class GByteLengthBetween
        Inherits GByteLength

        Private _minLength As Integer = -1

        Public Sub New()
            MyBase.new()
        End Sub

        Public Sub New(len As Integer)
            MyBase.New(len)
        End Sub

        Public Sub New(min As Integer, max As Integer)
            MyBase.New()
            Length = max
            MinLength = min
        End Sub

        Public Property MinLength() As Integer
            Get
                Return _minLength
            End Get
            Set(ByVal value As Integer)
                _minLength = value
                CssClass = CSS_STYLE + "_" + Length.ToString + "_" + _minLength.ToString
            End Set
        End Property

        Protected Overrides Sub Validate()
            initProperty()
            Dim value = ValidateeValue
            Dim range = LenB(value)

            If _minLength < 0 Then 'MINの指定なし
                If Not (range <= Length) Then
                    ErrorMessage = "値の長さは " + Length.ToString + " 以下である必要があります"
                    IsValid = False
                End If
            Else
                If Not (MinLength <= range And range <= Length) Then
                    ErrorMessage = "値の長さは " + MinLength.ToString + " ～ " + Length.ToString + " の間である必要があります"
                    IsValid = False
                End If

            End If

        End Sub


    End Class

End Namespace

