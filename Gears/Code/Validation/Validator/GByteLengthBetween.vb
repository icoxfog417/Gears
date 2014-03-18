Imports Microsoft.VisualBasic

Namespace Gears.Validation.Validator

    ''' <summary>
    ''' 項目長が指定されたバイト長の範囲に収まっているかを検証する属性
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GByteLengthBetween
        Inherits GByteLength

        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>最大長を指定し作成</summary>
        ''' <param name="len"></param>
        ''' <remarks></remarks>
        Public Sub New(len As Integer)
            MyBase.New(len)
        End Sub

        ''' <summary>最小～最大の範囲を指定して作成</summary>
        ''' <param name="min"></param>
        ''' <param name="max"></param>
        ''' <remarks></remarks>
        Public Sub New(min As Integer, max As Integer)
            MyBase.New()
            Length = max
            MinLength = min
        End Sub

        Private _minLength As Integer = -1
        ''' <summary>バイト長の最小値</summary>
        Public Property MinLength() As Integer
            Get
                Return _minLength
            End Get
            Set(ByVal value As Integer)
                _minLength = value
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

