Imports Microsoft.VisualBasic
Imports Gears.Validation.Marker

Namespace Gears.Validation.Validator

    ''' <summary>
    ''' 値が単純に数値であるかを検証する属性
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GNumeric
        Inherits GMarkerNumeric

        Private _withSign As Boolean = False
        ''' <summary>+-の符号を認めるか否か</summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property WithSign() As Boolean
            Get
                Return _withSign
            End Get
            Set(ByVal value As Boolean)
                _withSign = value
            End Set
        End Property

        Protected Overrides Sub Validate()
            initProperty()
            Dim num As String = ValidateeValue

            If num <> "" Then '空白は処理対象外(検知したいならRequire)
                Dim regexStr As String = "^"
                If _withSign Then
                    regexStr = "^[+-]?"
                End If
                regexStr += "(0|[1-9][0-9]*)(\.[0-9]+)?$"

                Dim regex As New System.Text.RegularExpressions.Regex(regexStr)
                Dim ret As Boolean = False
                ret = regex.IsMatch(num)
                If ret Then '保険
                    ret = Double.TryParse(num, System.Globalization.NumberStyles.Number, Nothing, 0.0#)
                End If

                If Not ret Then
                    ErrorMessage = "値が数値ではありません(" + num + ")"
                    IsValid = False
                End If
            End If

        End Sub

    End Class

End Namespace
