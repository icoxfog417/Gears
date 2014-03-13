Imports Microsoft.VisualBasic

Namespace Gears.DataSource

    ''' <summary>
    ''' SQLの選択条件をグループ化するためのクラス<br/>
    ''' SqlFilterItemに設定することで、設定した条件をOR/ANDでまとめ括弧でくくることができる
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    Public Class SqlFilterGroup

        Private _name As String = ""
        ''' <summary>グループ名</summary>
        Public ReadOnly Property Name() As String
            Get
                Return _name
            End Get
        End Property

        Private _isOrGroup As Boolean = True
        ''' <summary>グループ内の条件をANDで結合する</summary>
        Public Function isAndGroup() As Boolean
            Return Not _isOrGroup
        End Function
        ''' <summary>グループ内の条件をORで結合する(デフォルトTrue)</summary>
        Public Function isOrGroup() As Boolean
            Return _isOrGroup
        End Function

        Public Sub New(ByVal groupName As String, Optional ByVal isOr As Boolean = True)
            _name = groupName
            _isOrGroup = isOr
        End Sub

        Public Shared Function newGroup(ByVal groupName As String, Optional ByVal isOr As Boolean = True) As SqlFilterGroup
            Return New SqlFilterGroup(groupName, isOr)
        End Function

        ''' <summary>
        ''' 与えられたSqlFilterItemに自身をグループとして設定する(任意引数指定)
        ''' </summary>
        ''' <param name="filters"></param>
        ''' <remarks></remarks>
        Public Sub grouping(ParamArray filters As SqlFilterItem())
            grouping(filters.ToList)
        End Sub

        ''' <summary>
        ''' 与えられたSqlFilterItemに自身をグループとして設定する(配列指定)
        ''' </summary>
        ''' <param name="filters"></param>
        ''' <remarks></remarks>
        Public Sub grouping(ByRef filters As List(Of SqlFilterItem))
            For Each f As SqlFilterItem In filters
                f.inGroup(Me)
            Next
        End Sub

    End Class


    ''' <summary>
    ''' SQLの抽出条件を管理するクラス
    ''' </summary>
    ''' <remarks></remarks>
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

        Private _operand As String = ""
        ''' <summary>比較演算子</summary>
        Public ReadOnly Property Operand() As String
            Get
                Return _operand
            End Get
        End Property

        Private _group As SqlFilterGroup = Nothing
        ''' <summary>グループ設定</summary>
        Public Property Group() As SqlFilterGroup
            Get
                Return _group
            End Get
            Set(ByVal value As SqlFilterGroup)
                _group = value
            End Set
        End Property

        Private _joinTarget As SqlFilterItem = Nothing
        ''' <summary>JOIN時の結合対象列</summary>
        Public ReadOnly Property JoinTarget() As SqlFilterItem
            Get
                Return _joinTarget
            End Get
        End Property

        Private _negation As Boolean = False
        ''' <summary>JOIN時の結合対象列</summary>
        Public Property Negation() As Boolean
            Get
                Return _negation
            End Get
            Set(ByVal value As Boolean)
                _negation = value
            End Set
        End Property

        Public Sub New(ByVal col As String, Optional ByVal px As String = "")
            MyBase.New()
            _column = col
            Prefix = px
        End Sub

        Public Sub New(ByVal sqlf As SqlFilterItem, Optional ByVal val As Object = Nothing)
            MyBase.New(sqlf, val)
            _operand = sqlf.Operand
            _group = sqlf.Group
            _joinTarget = sqlf.JoinTarget
            _negation = sqlf.Negation
        End Sub

        ''' <summary>文字列からSqlFilterItemを作成する</summary>
        ''' <param name="filterType"></param>
        ''' <param name="value"></param>
        ''' <param name="isWrapWhenLike">Likeの場合、%で囲うか否か</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
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

        Public Function pf(ByVal pre As String) As SqlFilterItem
            MyBase.basePf(pre)
            Return Me
        End Function
        Public Function pf(ByRef sds As SqlDataSource) As SqlFilterItem
            MyBase.basePf(sds)
            Return Me
        End Function
        Public Function asKey() As SqlFilterItem
            MyBase.baseAsKey()
            Return Me
        End Function

        ''' <summary>評価式を設定する(equal)</summary>
        Public Function eq(ByVal value As Object) As SqlFilterItem
            _operand = "="
            _value = value
            Return Me
        End Function

        ''' <summary>評価式を設定する(not equal)</summary>
        Public Function neq(ByVal value As Object) As SqlFilterItem
            _operand = "<>"
            _value = value
            Return Me
        End Function

        ''' <summary>表同士の結合式を設定する(equal)</summary>
        Public Function joinOn(ByVal target As SqlFilterItem) As SqlFilterItem
            _operand = "=="
            _joinTarget = target
            Return Me
        End Function

        ''' <summary>評価式を設定する(less than)</summary>
        Public Function lt(ByVal value As Object) As SqlFilterItem
            _operand = "<"
            _value = value
            Return Me
        End Function

        ''' <summary>評価式を設定する(grater than)</summary>
        Public Function gt(ByVal value As Object) As SqlFilterItem
            _operand = ">"
            _value = value
            Return Me
        End Function

        ''' <summary>評価式を設定する(less than or equal)</summary>
        Public Function lteq(ByVal value As Object) As SqlFilterItem
            _operand = "<="
            _value = value
            Return Me
        End Function

        ''' <summary>評価式を設定する(grater than or equal)</summary>
        Public Function gteq(ByVal value As Object) As SqlFilterItem
            _operand = ">="
            _value = value
            Return Me
        End Function

        ''' <summary>評価式を設定する(LIKE)</summary>
        Public Function likes(ByVal value As Object) As SqlFilterItem
            _operand = " LIKE "
            _value = value
            Return Me
        End Function

        ''' <summary>条件式のグループを設定する</summary>
        Public Function inGroup(ByVal g As SqlFilterGroup) As SqlFilterItem
            _group = g
            Return Me
        End Function

        ''' <summary>否定式を設定する</summary>
        Public Function nots() As SqlFilterItem
            _negation = True
            Return Me
        End Function

        Public Overrides Function toString() As String
            Dim str As String = ""
            str += Column + " " + _operand + " " + If(Value IsNot Nothing, Value.ToString, "NULL")

            Return str

        End Function
    End Class

End Namespace