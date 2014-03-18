Imports Microsoft.VisualBasic

Namespace Gears.Validation.Validator

    ''' <summary>
    ''' 整数部/小数部が定義長の範囲内であるか検証する属性
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GPeriodPositionOk
        Inherits GNumeric

        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' 小数点前/後の定義長を指定し作成する
        ''' </summary>
        ''' <param name="beforep"></param>
        ''' <param name="afterp"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal beforep As Integer, ByVal afterp As Integer)
            MyBase.New()
            Me.BeforeP = beforep
            Me.AfterP = afterp
        End Sub

        Private _beforep As Integer = 0
        ''' <summary>整数部の最大長</summary>
        Public Property BeforeP() As Integer
            Get
                Return _beforep
            End Get
            Set(ByVal value As Integer)
                _beforep = value
            End Set
        End Property

        Private _afterp As Integer = 0
        ''' <summary>小数点以下桁数の最大長</summary>
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
                ret = IsNumeric(num) 'そもそも数値かどうか判定

                If ret Then
                    ret = regex.IsMatch(num)
                    If Not ret Then
                        ErrorMessage = "数値の桁数が不正です(値：" + num + ",整数部：" + _beforep.ToString + "桁・小数点以下：" + _afterp.ToString + "桁の必要があります)"
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
