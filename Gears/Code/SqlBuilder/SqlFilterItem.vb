Imports Microsoft.VisualBasic

Namespace Gears

    'SQLのフィルタをまとめるグループ
    <Serializable()>
    Public Class SqlFilterGroup
        Private _name As String = ""
        Public ReadOnly Property Name() As String
            Get
                Return _name
            End Get
        End Property

        Private _isOrGroup As Boolean = True
        Public Function isAndGroup() As Boolean
            Return Not _isOrGroup
        End Function
        Public Function isOrGroup() As Boolean
            Return _isOrGroup
        End Function

        Public Sub New(ByVal groupName As String, Optional ByVal isOr As Boolean = True)
            _name = groupName
            _isOrGroup = isOr
        End Sub

    End Class


    'SQLのフィルター定義を管理するクラス
    <Serializable()>
    Public Class SqlFilterItem
        Inherits SqlItem

        Public Const TXT_EQUAL As String = "EQ"
        Public Const TXT_NEQUAL As String = "NEQ"
        Public Const TXT_LT As String = "LT"
        Public Const TXT_GT As String = "GT"
        Public Const TXT_LTEQ As String = "LTEQ"
        Public Const TXT_GTEQ As String = "GTEQ"
        Public Const TXT_LIKE As String = "LIKE"
        Public Const TXT_START_WITH As String = "START_WITH"
        Public Const TXT_END_WITH As String = "END_WITH"

        'プロパティ
        Private _operand As String = ""
        Public ReadOnly Property Operand() As String
            Get
                Return _operand
            End Get
        End Property

        Private _paramName As String = "" '実際にフィルタをかけるパラーメータ名(USERID = :pUserというSQLならpUserがパラメーター名)
        Public Property ParamName() As String
            Get
                Return _paramName
            End Get
            Set(ByVal value As String)
                _paramName = value
            End Set
        End Property

        Private _group As SqlFilterGroup = Nothing
        Public Property Group() As SqlFilterGroup
            Get
                Return _group
            End Get
            Set(ByVal value As SqlFilterGroup)
                _group = value
            End Set
        End Property

        Private _joinTarget As SqlFilterItem = Nothing
        Public ReadOnly Property JoinTarget() As SqlFilterItem
            Get
                Return _joinTarget
            End Get
        End Property

        Private _isNots As Boolean = False
        Public Property IsNots() As Boolean
            Get
                Return _isNots
            End Get
            Set(ByVal value As Boolean)
                _isNots = value
            End Set
        End Property

        'コンストラクタ
        Public Sub New(ByVal col As String, Optional ByVal px As String = "")
            MyBase.New()
            _column = col
            _prefix = px
        End Sub
        Public Sub New(ByVal sqlf As SqlFilterItem, Optional ByVal val As Object = Nothing)
            MyBase.New(sqlf, val)
            _operand = sqlf.Operand
            _paramName = sqlf.ParamName
            _group = sqlf.Group
            _joinTarget = sqlf.JoinTarget
            _isNots = sqlf.IsNots
        End Sub

        Public Function filterAs(ByVal filterType As String, ByVal value As Object, Optional ByVal isWrapWhenLike As Boolean = False) As SqlFilterItem
            Select Case filterType.ToUpper
                Case SqlFilterItem.TXT_EQUAL
                    Return eq(value)
                Case SqlFilterItem.TXT_NEQUAL
                    Return neq(value)
                Case SqlFilterItem.TXT_GT
                    Return gt(value)
                Case SqlFilterItem.TXT_GTEQ
                    Return gteq(value)
                Case SqlFilterItem.TXT_LT
                    Return lt(value)
                Case SqlFilterItem.TXT_LTEQ
                    Return lteq(value)
                Case SqlFilterItem.TXT_LIKE
                    Return likes(If(Not isWrapWhenLike, value, "%" + value + "%"))
                Case SqlFilterItem.TXT_START_WITH
                    Return likes(If(Not isWrapWhenLike, value, value + "%"))
                Case SqlFilterItem.TXT_END_WITH
                    Return likes(If(Not isWrapWhenLike, value, "%" + value))
                Case Else
                    Return Me 'Continue Method Chain
            End Select
        End Function

        'メソッド
        Public Function pf(ByVal pre As String) As SqlFilterItem
            _prefix = pre
            Return Me
        End Function
        Public Function pf(ByRef sds As SqlDataSource) As SqlFilterItem
            _prefix = sds.Suffix
            Return Me
        End Function

        Public Function limitBy(ByVal pn As String) As SqlFilterItem
            _paramName = pn
            Return Me
        End Function

        Private Sub setFormula(ByVal opr As String, ByVal value As Object)
            _operand = opr
            _value = value
        End Sub

        Public Function eq(ByVal value As Object) As SqlFilterItem
            _operand = "="
            _value = value
            Return Me
        End Function
        Public Function neq(ByVal value As Object) As SqlFilterItem
            _operand = "<>"
            _value = value
            Return Me
        End Function
        Public Function joinOn(ByVal target As SqlFilterItem) As SqlFilterItem
            _operand = "=="
            _joinTarget = target
            Return Me
        End Function

        Public Function lt(ByVal value As Object) As SqlFilterItem
            _operand = "<"
            _value = value
            Return Me
        End Function
        Public Function gt(ByVal value As Object) As SqlFilterItem
            _operand = ">"
            _value = value
            Return Me
        End Function

        Public Function lteq(ByVal value As Object) As SqlFilterItem
            _operand = "<="
            _value = value
            Return Me
        End Function
        Public Function gteq(ByVal value As Object) As SqlFilterItem
            _operand = ">="
            _value = value
            Return Me
        End Function
        Public Function likes(ByVal value As Object) As SqlFilterItem
            _operand = " LIKE "
            _value = value
            Return Me
        End Function

        Public Function inGroup(ByVal g As SqlFilterGroup) As SqlFilterItem
            _group = g
            Return Me
        End Function

        Public Function key() As SqlFilterItem
            IsKey() = True
            Return Me
        End Function
        Public Function nots() As SqlFilterItem
            _isNots = True
            Return Me
        End Function

        Public Overrides Function toString() As String
            Dim str As String = ""
            str += Column + " " + _operand + " " + If(Value IsNot Nothing, Value.ToString, "NULL")

            Return str

        End Function
    End Class

End Namespace