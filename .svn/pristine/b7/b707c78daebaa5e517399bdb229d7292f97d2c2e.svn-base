Imports Microsoft.VisualBasic

Namespace Gears

    Public Class GPeriodPositionOk
        Inherits GNumeric

        Private _beforep As Integer = 0
        Private _afterp As Integer = 0

        Public Sub New()
            CssClass = CSS_STYLE
        End Sub

        Public Sub New(ByVal beforep As Integer, ByVal afterp As Integer)
            MyBase.New()
            Me.BeforeP = beforep
            Me.AfterP = afterp
        End Sub

        Public Property BeforeP() As Integer
            Get
                Return _beforep
            End Get
            Set(ByVal value As Integer)
                _beforep = value
            End Set
        End Property

        Public Property AfterP() As Integer
            Get
                Return _afterp
            End Get
            Set(ByVal value As Integer)
                _afterp = value
            End Set
        End Property

        Protected Overrides Sub Validate()
            initProperty()
            Dim num As String = ValidateeValue

            If num <> "" Then

                Dim regex As New System.Text.RegularExpressions.Regex("^[+-]?\d{0," + _beforep.ToString + "}(\.\d{0," + _afterp.ToString + "})?$")
                Dim ret As Boolean = False
                ret = IsNumeric(num)

                If ret Then
                    ret = regex.IsMatch(num)
                    If Not ret Then
                        ErrorMessage = "数値の桁数が不正です(値：" + num + ",定義->整数部：" + _beforep.ToString + "桁/小数点以下桁数：" + _afterp.ToString + "桁)"
                        IsValid = False
                    End If
                Else
                    ErrorMessage = "値が数値ではありません(" + num + ")"
                    IsValid = False
                End If

            End If


        End Sub

    End Class

End Namespace
