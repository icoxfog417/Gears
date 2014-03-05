Imports Microsoft.VisualBasic
Imports System.Web.UI

Namespace Gears.Validation

    Public MustInherit Class GearsAttribute
        Implements IValidator

        Public Const ERR_STYLE As String = "gs-validation-error"
        Private _errorMessage As String = ""
        Private _valid As Boolean = True
        Private _cssClass As String = ""
        Private _validatee As String = ""

        Public Sub New()
        End Sub

        Public Property ErrorMessage As String Implements System.Web.UI.IValidator.ErrorMessage
            Get
                Return _errorMessage
            End Get
            Protected Set(value As String) 'setterは継承クラスのみに開放
                _errorMessage = value
            End Set
        End Property

        Public Property IsValid As Boolean Implements System.Web.UI.IValidator.IsValid
            Get
                Return _valid
            End Get
            Protected Set(value As Boolean) 'setterは継承クラスのみに開放
                _valid = value
            End Set
        End Property

        Protected ReadOnly Property ValidateeValue() As String
            Get
                Return _validatee
            End Get
        End Property


        Public Overridable Property CssClass() As String
            Get
                Return _cssClass
            End Get
            Protected Set(ByVal value As String)
                _cssClass = value
            End Set
        End Property

        Public Overridable Function hasMarker(marker As Type) As Boolean
            If Me.GetType.IsSubclassOf(marker) Then
                Return True
            Else
                Return False
            End If
        End Function

        '実装は継承先クラスにおまかせ　このメソッドはバリデーションの対象を明示しないため、予想しない動作をする可能性がある。
        'そのため、アクセスをProtectedにし公開範囲を限定する
        Protected MustOverride Sub Validate() Implements System.Web.UI.IValidator.Validate

        Public Function isValidateOk(validateeValue As String) As Boolean
            initProperty()
            _validatee = validateeValue
            Validate()
            Return IsValid

        End Function

        Protected Sub initProperty(Optional defaultResult As Boolean = True)
            _errorMessage = ""
            IsValid = defaultResult  'デフォルトTrueに初期化　実装時には注意

        End Sub

        Public Overridable Function ListUp() As List(Of GearsAttribute)
            Dim list As New List(Of GearsAttribute)
            list.Add(Me)
            Return list
        End Function

    End Class

End Namespace
