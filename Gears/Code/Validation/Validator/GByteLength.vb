Imports Microsoft.VisualBasic

Namespace Gears

    Public Class GByteLength
        Inherits GearsAttribute

        Protected Const CSS_STYLE As String = "gs-vlength"
        Private _length As Integer = 0

        Public Sub New()
            MyBase.new()
            CssClass = CSS_STYLE
        End Sub

        Public Sub New(len As Integer)
            Me.New()
            Length = len
        End Sub

        Public Property Length() As Integer
            Get
                Return _length
            End Get
            Set(ByVal value As Integer)
                _length = value
                CssClass = CSS_STYLE + "_" + _length.ToString
            End Set
        End Property

        Protected Overrides Sub Validate()
            initProperty()
            Dim value = ValidateeValue

            If Not String.IsNullOrEmpty(value) Then
                If LenB(value) < Length Then
                    ErrorMessage = "値が短すぎます(設定長：" + Length.ToString + ")"
                    IsValid = False
                ElseIf LenB(value) > Length Then
                    ErrorMessage = "値が長すぎます(設定長：" + Length.ToString + ")"
                    IsValid = False
                End If
            Else
                IsValid = True '空文字の場合チェックしない
            End If

        End Sub

        'バイト数取得
        Protected Function LenB(ByVal st As String) As Integer
            If st Is Nothing Then
                Return 0
            Else
                Dim SJIS As System.Text.Encoding = System.Text.Encoding.GetEncoding("Shift_JIS")
                Return SJIS.GetByteCount(st)
            End If
        End Function

    End Class

End Namespace
