Imports Microsoft.VisualBasic

Namespace Gears.Validation.Validator

    ''' <summary>
    ''' 値の比較検証を行うための属性
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GCompare
        Inherits GearsAttribute

        Public Sub New()
            MyBase.new()
        End Sub

        Private _operatorType As String = "="
        ''' <summary>比較を行うためのオペレーター</summary>
        Public Property OperatorType() As String
            Get
                Return _operatorType
            End Get
            Set(ByVal value As String)
                _operatorType = value
            End Set
        End Property

        Private _expected As String = ""
        ''' <summary>期待される値</summary>
        Public Property Expected() As String
            Get
                Return _expected
            End Get
            Set(ByVal value As String)
                _expected = value
            End Set
        End Property

        ''' <summary>
        ''' 比較値・期待値・演算子を直接指定してバリデーションを行う
        ''' </summary>
        ''' <param name="leftValue"></param>
        ''' <param name="rightValue"></param>
        ''' <param name="opr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function isValidateOk(leftValue As String, rightValue As String, Optional ByVal opr As String = "=") As Boolean
            _operatorType = opr
            _expected = rightValue
            Return isValidateOk(leftValue)

        End Function


        Protected Overrides Sub Validate()
            If Not String.IsNullOrEmpty(ValidateeValue) Then '空白は処理対象外(検知したいならRequire)
                Dim result As Boolean = False
                Dim msg As String = ""
                Try
                    Select Case Trim(OperatorType)
                        Case "="
                            If compare(ValidateeValue, Expected, OperatorType) Then result = True Else msg = "と等しく"
                        Case "<"
                            If compare(ValidateeValue, Expected, OperatorType) Then result = True Else msg = "より小さく"
                        Case ">"
                            If compare(ValidateeValue, Expected, OperatorType) Then result = True Else msg = "より大きく"
                        Case "<="
                            If compare(ValidateeValue, Expected, OperatorType) Then result = True Else msg = "以下では"
                        Case ">="
                            If compare(ValidateeValue, Expected, OperatorType) Then result = True Else msg = "以上では"
                    End Select

                Catch ex As Exception
                    'キャストエラー対応
                    result = False
                End Try

                If Not result Then
                    ErrorMessage = "指定された値 " + ValidateeValue + " は、" + Expected + " " + msg + "ありません"
                    IsValid = False
                End If
            End If

        End Sub

        ''' <summary>比較を行うための関数。与えられた値が数値/日付に見える場合それを考慮し演算</summary>
        ''' <param name="leftValue"></param>
        ''' <param name="rightValue"></param>
        ''' <param name="ct"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
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
