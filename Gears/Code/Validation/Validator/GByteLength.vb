Imports Microsoft.VisualBasic

Namespace Gears.Validation.Validator

    ''' <summary>
    ''' 項目長の検証を行うための属性
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GByteLength
        Inherits GearsAttribute

        Private _length As Integer = 0

        Public Sub New()
            MyBase.new()
        End Sub

        ''' <summary>
        ''' 項目長を指定して作成
        ''' </summary>
        ''' <param name="len"></param>
        ''' <remarks></remarks>
        Public Sub New(len As Integer)
            Me.New()
            Length = len
        End Sub

        ''' <summary>チェックするバイト長</summary>
        Public Property Length() As Integer
            Get
                Return _length
            End Get
            Set(ByVal value As Integer)
                _length = value
            End Set
        End Property

        Private _encode As String = "Shift_JIS"
        ''' <summary>バイト長を算出するためのエンコード(デフォルト Shift_JIS)</summary>
        Public Property Encode() As String
            Get
                Return _encode
            End Get
            Set(ByVal value As String)
                _encode = value
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

        ''' <summary>
        ''' バイト長を取得するための関数
        ''' </summary>
        ''' <param name="st"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function LenB(ByVal st As String) As Integer
            If st Is Nothing Then
                Return 0
            Else
                Dim coder As System.Text.Encoding = System.Text.Encoding.GetEncoding(_encode)
                Return coder.GetByteCount(st)
            End If
        End Function

    End Class

End Namespace
