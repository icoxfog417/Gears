Imports Microsoft.VisualBasic
Imports Gears.Validation.Marker

Namespace Gears.Validation.Validator

    ''' <summary>
    ''' 日付を検証するための属性
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GDate
        Inherits GMarkerDate

        Private _format As String = ""
        ''' <summary>日付フォーマットを指定する(指定がなくても一般的な書式はカバーされる)</summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Format() As String
            Get
                Return _format
            End Get
            Set(ByVal value As String)
                _format = value
            End Set
        End Property

        Protected Overrides Sub Validate()

            Dim dateFormat As New List(Of String)
            If _format = "" Then
                dateFormat.Add("yyyyMMdd")
                dateFormat.Add("yyyy/MM/dd")
                dateFormat.Add("yyyy-MM-dd")
            Else
                dateFormat.Add(_format)
            End If

            Try
                Dim d As DateTime
                If ValidateeValue <> "" Then
                    '通常パース検証
                    If _format = "" And DateTime.TryParse(ValidateeValue, d) Then 'フォーマット指定がある場合は行わない
                        IsValid = True
                    Else
                        d = DateTime.ParseExact(ValidateeValue, dateFormat.ToArray, _
                                                            System.Globalization.DateTimeFormatInfo.InvariantInfo,
                                                            System.Globalization.DateTimeStyles.None)
                    End If
                End If

            Catch ex As Exception
                IsValid = False
                ErrorMessage = "値は日付項目として不正です(入力値:" + ValidateeValue + " 書式：" + dateFormat(0) + ")"
            End Try

        End Sub

    End Class

End Namespace
