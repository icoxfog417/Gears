Imports Microsoft.VisualBasic

Namespace Gears
    ''' ------------------------------------------------------------------------------
    ''' <summary>
    ''' 検証結果の種別
    ''' </summary>
    ''' ------------------------------------------------------------------------------
    Public Enum ValidationResultType
        Critical
        Alert
        Success
    End Enum

    ''' ------------------------------------------------------------------------------
    ''' <summary>
    ''' 結果を評価するためのインナークラス
    ''' </summary>
    ''' ------------------------------------------------------------------------------
    Public Class ValidationResults

        Private results As New List(Of ValidationResult)

        Public Function GetFails() As List(Of ValidationResult)
            Dim fails As List(Of ValidationResult) _
                = (From x As ValidationResult In results Where x.ValidResult <> ValidationResultType.Success Select x).ToList
            Return fails
        End Function

        Public ReadOnly Property ErrorMessage As String
            Get
                Dim msg As String = ""
                For Each result As ValidationResult In results
                    If result.ValidResult = ValidationResultType.Critical Then
                        msg = result.ErrorMessage
                        Exit For
                    ElseIf result.ValidResult = ValidationResultType.Alert Then
                        '警告は一気に出す(何度もプロンプトで確認するのを防ぐため)
                        If String.IsNullOrEmpty(msg) Then
                            msg += result.ErrorMessage
                        Else
                            msg += vbCrLf + result.ErrorMessage
                        End If
                    End If
                Next

                Return msg

            End Get
        End Property

        Public ReadOnly Property ErrorSource As String
            Get

                Dim source As String = ""
                For Each result As ValidationResult In results
                    'Criticalがある場合最優先
                    If result.ValidResult = ValidationResultType.Critical Then
                        source = result.ErrorSource
                        Exit For
                    End If
                    If String.IsNullOrEmpty(source) AndAlso result.ValidResult = ValidationResultType.Alert Then
                        '最初の一つを設定
                        source = result.ErrorSource
                    End If
                Next
                Return source
            End Get
        End Property

        Public Function IsValid() As Boolean
            Dim result As Boolean = True

            For Each r As ValidationResult In results
                If r.ValidResult = ValidationResultType.Alert Or r.ValidResult = ValidationResultType.Critical Then
                    result = False
                End If
            Next
            Return result
        End Function

        Public Function IsValidIgnoreAlert() As Boolean
            Dim result As Boolean = True

            For Each r As ValidationResult In results
                If r.ValidResult = ValidationResultType.Critical Then
                    result = False
                End If
            Next
            Return result

        End Function

        Public Sub Add(ByVal vR As ValidationResultType, ByVal msg As String, ByVal source As String)
            Dim r As New ValidationResult(vR, msg, source)
            results.Add(r)
        End Sub
        Public Sub Clear()
            results.Clear()
        End Sub

    End Class

    ''' ------------------------------------------------------------------------------
    ''' <summary>
    ''' 結果格納用オブジェクト
    ''' </summary>
    ''' ------------------------------------------------------------------------------
    Public Class ValidationResult

        Private _validResult As ValidationResultType
        Public Property ValidResult() As ValidationResultType
            Get
                Return _validResult
            End Get
            Set(ByVal value As ValidationResultType)
                _validResult = value
            End Set
        End Property

        Private _errorMessage As String = ""
        Public Property ErrorMessage() As String
            Get
                Return _errorMessage
            End Get
            Set(ByVal value As String)
                _errorMessage = value
            End Set
        End Property

        Private _errorSource As String
        Public Property ErrorSource() As String
            Get
                Return _errorSource
            End Get
            Set(ByVal value As String)
                _errorSource = value
            End Set
        End Property

        Public Sub New()
        End Sub

        Public Sub New(ByVal vR As ValidationResultType, ByVal msg As String, ByVal source As String)
            _validResult = vR
            _errorMessage = msg
            _errorSource = source
        End Sub
    End Class

End Namespace
