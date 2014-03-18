Imports Microsoft.VisualBasic

Namespace Gears.Validation

    ''' <summary>
    ''' 検証結果の種別
    ''' </summary>
    Public Enum ValidationResultType
        ''' <summary>エラー</summary>
        Critical
        ''' <summary>警告</summary>
        Alert
        ''' <summary>成功</summary>
        Success
    End Enum

    ''' <summary>
    ''' 検証結果を格納、評価するためのクラス
    ''' </summary>
    Public Class ValidationResults

        Private results As New List(Of ValidationResult)

        ''' <summary>
        ''' 成功以外のバリデーション結果を抽出する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetFails() As List(Of ValidationResult)
            Dim fails As List(Of ValidationResult) _
                = (From x As ValidationResult In results Where x.ValidResult <> ValidationResultType.Success Select x).ToList
            Return fails
        End Function

        ''' <summary>
        ''' エラーメッセージを取得する<br/>
        ''' エラーが複数ある場合は初回のものを取得。警告については、全て連結して一つの文字列にする<br/>
        ''' ※警告はこれを無視するかどうかプロンプトで確認するため、何度も確認をさせないよう一つにまとめる
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' エラーの発生源を取得する。複数のエラーが存在する場合は、最初の一つを取得する
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' バリデーションの判定結果を取得する(エラー/警告がある場合NG)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsValid() As Boolean
            Dim result As Boolean = True

            For Each r As ValidationResult In results
                If r.ValidResult = ValidationResultType.Alert Or r.ValidResult = ValidationResultType.Critical Then
                    result = False
                End If
            Next
            Return result
        End Function

        ''' <summary>
        ''' バリデーションの判定結果を取得する(警告を除外)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsValidIgnoreAlert() As Boolean
            Dim result As Boolean = True

            For Each r As ValidationResult In results
                If r.ValidResult = ValidationResultType.Critical Then
                    result = False
                End If
            Next
            Return result

        End Function

        ''' <summary>
        ''' バリデーション結果を追加する
        ''' </summary>
        ''' <param name="vResult"></param>
        ''' <param name="msg"></param>
        ''' <param name="source"></param>
        ''' <remarks></remarks>
        Public Sub Add(ByVal vResult As ValidationResultType, ByVal msg As String, ByVal source As String)
            Dim r As New ValidationResult(vResult, msg, source)
            results.Add(r)
        End Sub
        Public Sub Clear()
            results.Clear()
        End Sub

    End Class


    ''' <summary>
    ''' 検証結果オブジェクト
    ''' </summary>
    Public Class ValidationResult

        Private _validResult As ValidationResultType
        ''' <summary>バリデーション結果</summary>
        Public Property ValidResult() As ValidationResultType
            Get
                Return _validResult
            End Get
            Set(ByVal value As ValidationResultType)
                _validResult = value
            End Set
        End Property

        Private _errorMessage As String = ""
        ''' <summary>エラーメッセージ</summary>
        Public Property ErrorMessage() As String
            Get
                Return _errorMessage
            End Get
            Set(ByVal value As String)
                _errorMessage = value
            End Set
        End Property

        Private _errorSource As String
        ''' <summary>エラー発生源</summary>
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

        Public Sub New(ByVal vResult As ValidationResultType, ByVal msg As String, ByVal source As String)
            _validResult = vResult
            _errorMessage = msg
            _errorSource = source
        End Sub
    End Class

End Namespace
