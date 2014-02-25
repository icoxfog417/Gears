Imports Microsoft.VisualBasic

Namespace Gears

    Public Class GComp
        Inherits GearsAttribute

        Public Sub New()
            MyBase.new()
        End Sub

        Private _compType As String = "="
        Public Property CompType() As String
            Get
                Return _compType
            End Get
            Set(ByVal value As String)
                _compType = value
            End Set
        End Property

        Private _tgtValue As String = ""
        Public Property TgtValue() As String
            Get
                Return _tgtValue
            End Get
            Set(ByVal value As String)
                _tgtValue = value
            End Set
        End Property

        Public Overloads Function isValidateOk(leftValue As String, rightValue As String, Optional ByVal ct As String = "=") As Boolean
            _compType = ct
            _tgtValue = rightValue
            Return isValidateOk(leftValue)

        End Function


        Protected Overrides Sub Validate()
            If Not String.IsNullOrEmpty(ValidateeValue) Then '空白は処理対象外(検知したいならRequire)
                Dim result As Boolean = False
                Dim msg As String = ""
                Try
                    Select Case Trim(CompType)
                        Case "="
                            If compare(ValidateeValue, TgtValue, CompType) Then result = True Else msg = "と等しく"
                        Case "<"
                            If compare(ValidateeValue, TgtValue, CompType) Then result = True Else msg = "より小さく"
                        Case ">"
                            If compare(ValidateeValue, TgtValue, CompType) Then result = True Else msg = "より大きく"
                        Case "<="
                            If compare(ValidateeValue, TgtValue, CompType) Then result = True Else msg = "以下では"
                        Case ">="
                            If compare(ValidateeValue, TgtValue, CompType) Then result = True Else msg = "以上では"
                    End Select

                Catch ex As Exception
                    'キャストエラー対応
                    result = False
                End Try

                If Not result Then
                    ErrorMessage = "指定された値 " + ValidateeValue + " は、" + TgtValue + " " + msg + "ありません"
                    IsValid = False
                End If
            End If

        End Sub

        Private Function compare(ByVal leftValue As String, ByVal rightValue As String, ByVal ct As String) As Boolean

            Dim numValue As Double = 0
            Dim dateValue As DateTime = Nothing
            Dim isNumber As Boolean = Double.TryParse(leftValue, numValue)
            Dim isDate As Boolean = DateTime.TryParse(leftValue, dateValue)
            Dim result As Integer = 0

            If isNumber Then
                result = numValue.CompareTo(Double.Parse(rightValue))

            ElseIf isDate Then
                result = dateValue.CompareTo(DateTime.Parse(rightValue))
            Else
                result = leftValue.CompareTo(rightValue)
            End If

            Select Case Trim(ct)
                Case "="
                    If result = 0 Then Return True Else Return False
                Case "<"
                    If result < 0 Then Return True Else Return False
                Case ">"
                    If result > 0 Then Return True Else Return False
                Case "<="
                    If result <= 0 Then Return True Else Return False
                Case ">="
                    If result >= 0 Then Return True Else Return False
                Case Else
                    Return False
            End Select

        End Function


    End Class

End Namespace
