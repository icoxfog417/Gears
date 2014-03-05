Imports Microsoft.VisualBasic
Imports System.Reflection
Imports Gears.DataSource

Namespace Gears.Validation

    ''' ------------------------------------------------------------------------------
    ''' <summary>
    ''' 検証用メソッドのアノテーション
    ''' </summary>
    ''' ------------------------------------------------------------------------------
    <System.AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class ModelValidationMethod
        Inherits System.Attribute

        Public Const DEFAULT_ORDER As Integer = 999

        Private _falseAsAlert As Boolean = False
        Public Property FalseAsAlert() As Boolean
            Get
                Return _falseAsAlert
            End Get
            Set(ByVal value As Boolean)
                _falseAsAlert = value
            End Set
        End Property

        Private _onlyWhenTheseValueExist As String = ""
        Public Property OnlyWhenTheseValueExist() As String
            Get
                Return _onlyWhenTheseValueExist
            End Get
            Set(ByVal value As String)
                _onlyWhenTheseValueExist = value
            End Set
        End Property


        Private _order As Integer = DEFAULT_ORDER
        Public Property Order() As Integer
            Get
                Return _order
            End Get
            Set(ByVal value As Integer)
                _order = value
            End Set
        End Property

    End Class

    Public MustInherit Class AbsModelValidator

        Public Delegate Function ModelValidator(ByVal sqlb As SqlBuilder) As Boolean

        Private _validResults As New ValidationResults

        Public ReadOnly Property ValidResults As ValidationResults
            Get
                Return _validResults
            End Get
        End Property
        Public ReadOnly Property IsValid As Boolean
            Get
                Return _validResults.IsValid
            End Get
        End Property
        Public ReadOnly Property IsValidIgnoreAlert As Boolean
            Get
                Return _validResults.IsValidIgnoreAlert
            End Get
        End Property

        Private _errorMessage As String = "" 'メソッドでのメッセージ更新用(表には出ない)
        Public Property ErrorMessage As String
            Get
                Return _validResults.ErrorMessage
            End Get
            Protected Set(value As String)
                _errorMessage = value
            End Set
        End Property

        Private _errorSource As String = ""
        Public Property ErrorSource() As String
            Get
                Return _validResults.ErrorSource
            End Get
            Protected Set(value As String)
                _errorSource = value
            End Set
        End Property

        ''' ------------------------------------------------------------------------------
        ''' <summary>
        ''' 検証実行前処理
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' ------------------------------------------------------------------------------
        Public Overridable Sub setUpValidation(ByVal sqlb As SqlBuilder)
        End Sub

        ''' ------------------------------------------------------------------------------
        ''' <summary>
        ''' 検証実行後処理
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' ------------------------------------------------------------------------------
        Public Overridable Sub tearDownValidation(ByVal sqlb As SqlBuilder)
        End Sub

        ''' ------------------------------------------------------------------------------
        ''' <summary>
        ''' 'ModelValidationMethodアノテーションの付与された、返り値が BooleanのPublicメソッドを実行していく
        ''' </summary>
        ''' <param name="sqlb">バリデーション対象のSqlBuilder</param>
        ''' <param name="isStockError">致命的エラーがあっても続行してエラーをため続ける場合はTrue</param>
        ''' <returns></returns>
        ''' ------------------------------------------------------------------------------
        Public Function Validate(ByVal sqlb As SqlBuilder, Optional ByVal isStockError As Boolean = False) As ValidationResults
            'ModelValidationMethodを抽出
            Dim methods As List(Of ModelValidator) _
                = (From m As MethodInfo In Me.GetType.GetMethods
                  Let attr As ModelValidationMethod = Attribute.GetCustomAttribute(m, GetType(ModelValidationMethod))
                  Where attr IsNot Nothing
                  Order By attr.Order
                  Select CType([Delegate].CreateDelegate(GetType(ModelValidator), Me, m.Name), ModelValidator)).ToList

            Validate(sqlb, methods, isStockError)

            Return _validResults

        End Function
        ''' ------------------------------------------------------------------------------
        ''' <summary>
        ''' 対象のバリデーションを実行する
        ''' </summary>
        ''' <param name="validatee">バリデーション対象のSqlBuilder</param>
        ''' <param name="validates">バリデーションメソッドのリスト</param>
        ''' <param name="isStockError">致命的エラーがあっても続行してエラーをため続ける場合はTrue</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' <example>
        ''' <para>
        ''' <code>
        '''   Dim sb As SqlBuilder = dto.generateSqlBuilder
        '''   Dim hinValidator = New AstMasterHinmokuValidator(Master.ConnectionName)
        '''   Dim validates As New List(Of AbsModelValidator.ModelValidator)
        '''   '必要なバリデーションの追加
        '''   validates.Add(AddressOf hinValidator.isHinCodeValid)
        '''   validates.Add(AddressOf hinValidator.isNisgataNew)
        '''   Dim result As ValidationResults = hinValidator.Validate(sb, validates, True)
        ''' </code>
        ''' </para>
        ''' </example>
        ''' </remarks>
        ''' ------------------------------------------------------------------------------
        Public Function Validate(ByVal validatee As SqlBuilder, ByVal validates As List(Of ModelValidator), Optional ByVal isStockError As Boolean = False) As ValidationResults
            _validResults.Clear()

            '検証処理のセットアップ
            setUpValidation(validatee)

            For Each mv As ModelValidator In validates
                _errorMessage = ""
                _errorSource = ""

                'Dim attr As ModelValidationMethod = Attribute.GetCustomAttribute(mv.GetMethodInfo, GetType(ModelValidationMethod))
                Dim attr As ModelValidationMethod = Attribute.GetCustomAttribute(mv.Method, GetType(ModelValidationMethod))
                Dim isExecute As Boolean = True

                If attr IsNot Nothing Then
                    'ある項目が指定されていた場合のみバリデーションを行う、という指定について処理
                    If Not String.IsNullOrEmpty(attr.OnlyWhenTheseValueExist) Then
                        Dim mustNeedValues As String() = Split(attr.OnlyWhenTheseValueExist, ",")
                        For Each item As String In mustNeedValues
                            If getValidateeValue(validatee, item) Is Nothing Then
                                isExecute = False
                                Exit For
                            End If
                        Next
                    End If
                End If

                If isExecute Then
                    Dim result As Boolean = mv(validatee)

                    If result Then
                        _validResults.Add(ValidationResultType.Success, "", "")
                    Else
                        If attr IsNot Nothing AndAlso attr.FalseAsAlert Then
                            _validResults.Add(ValidationResultType.Alert, _errorMessage, _errorSource)
                        Else
                            _validResults.Add(ValidationResultType.Critical, _errorMessage, _errorSource)
                            If Not isStockError Then Exit For
                        End If
                    End If

                End If

            Next

            '検索処理の終了
            tearDownValidation(validatee)

            Return _validResults

        End Function

        ''' ------------------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' ------------------------------------------------------------------------------
        Public Function throwException() As GearsModelValidationException
            Dim ex As New GearsModelValidationException(_validResults)
            Throw ex
        End Function

        ''' ------------------------------------------------------------------------------
        ''' <summary>
        ''' 検証用値取得メソッド
        ''' </summary>
        ''' <param name="sqlb"></param>
        ''' <param name="colName"></param>
        ''' <param name="nothingAsSpace"></param>
        ''' <returns></returns>
        ''' ------------------------------------------------------------------------------
        Public Function getValidateeValue(ByVal sqlb As SqlBuilder, ByVal colName As String, Optional ByVal nothingAsSpace As Boolean = False) As String
            If sqlb.Selection(colName) Is Nothing Then
                If Not nothingAsSpace Then
                    Return Nothing
                Else
                    Return ""
                End If
            Else
                Return sqlb.Selection(colName).Value
            End If
        End Function

    End Class

End Namespace
