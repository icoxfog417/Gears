Imports Microsoft.VisualBasic
Imports Gears.Validation
Imports Gears.Validation.Marker

Namespace Gears.Validation.Validator

    ''' <summary>
    ''' 整数(小数点なし)であるかを検証する属性
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GNumber
        Inherits GMarkerNumeric

        Protected Overrides Sub Validate()
            initProperty()
            Dim num As String = ValidateeValue

            If num <> "" Then '空白は処理対象外(検知したいならRequire)
                Dim regex As New System.Text.RegularExpressions.Regex("(^0$)|^[1-9][0-9]*$")
                Dim ret As Boolean = False
                ret = regex.IsMatch(num)
                If ret Then '保険
                    ret = Integer.TryParse(num, 0)
                End If

                If Not ret Then
                    ErrorMessage = "値が小数点無しの正の数ではありません(" + num + ")"
                    IsValid = False
                End If
            End If

        End Sub

    End Class

End Namespace
