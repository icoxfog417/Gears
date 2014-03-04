Imports Microsoft.VisualBasic

Namespace Gears

    ''' <summary>
    ''' ORDER の種類
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum OrderKind As Integer
        ASC
        DESC
        NON
    End Enum

    ''' <summary>
    ''' SQLの選択条件を管理するクラス
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    Public Class SqlSelectItem
        Inherits SqlItem

        Private _colAlias As String = ""
        ''' <summary>選択列の別名(AS)</summary>
        Public ReadOnly Property ColAlias() As String
            Get
                Return _colAlias
            End Get
        End Property

        Private _isGroupBy As Boolean = False
        ''' <summary>Group BY句の対象か否か</summary>
        Public Property IsGroupBy() As Boolean
            Get
                Return _isGroupBy
            End Get
            Set(ByVal value As Boolean)
                _isGroupBy = value
            End Set
        End Property

        Private _orderBy As OrderKind = OrderKind.NON
        ''' <summary>ORDER BY設定</summary>
        Public Property OrderBy() As OrderKind
            Get
                Return _orderBy
            End Get
            Set(ByVal value As OrderKind)
                _orderBy = value
            End Set
        End Property

        Private _isNoSelect As Boolean = False
        ''' <summary>SELECT選択の対象としない(ORDER BYのみに使いたいなど)</summary>
        Public Property IsNoSelect() As Boolean
            Get
                Return _isNoSelect
            End Get
            Set(ByVal value As Boolean)
                _isNoSelect = value
            End Set
        End Property

        Public Sub New(ByVal col As String, Optional ByVal px As String = "")
            MyBase.New()
            _column = col
            _prefix = px
        End Sub
        Public Sub New(ByVal sqls As SqlSelectItem, Optional ByVal val As Object = Nothing)
            MyBase.New(sqls, val)
            _colAlias = sqls.ColAlias
            _isGroupBy = sqls.IsGroupBy
            _orderBy = sqls.OrderBy
            _isNoSelect = sqls.IsNoSelect
        End Sub

        ''' <summary>
        ''' 別名を考慮したカラム名
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ColumnWithAlias() As String
            If _colAlias <> "" Then
                Return _colAlias
            Else
                Return _column
            End If
        End Function

        Public Function pf(ByVal pre As String) As SqlSelectItem
            MyBase.basePf(pre)
            Return Me
        End Function
        Public Function pf(ByRef sds As SqlDataSource) As SqlSelectItem
            MyBase.basePf(sds)
            Return Me
        End Function
        Public Function asKey() As SqlSelectItem
            MyBase.baseAsKey()
            Return Me
        End Function

        ''' <summary>別名を設定する</summary>
        ''' <param name="colalias"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function asName(ByVal colalias As String) As SqlSelectItem
            _colAlias = colalias
            Return Me
        End Function

        ''' <summary>Group by句に含む</summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function inGroup() As SqlSelectItem
            _isGroupBy = True
            Return Me
        End Function

        ''' <summary>昇順に設定</summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ASC() As SqlSelectItem
            _orderBy = OrderKind.ASC
            Return Me
        End Function

        ''' <summary>降順に設定</summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DESC() As SqlSelectItem
            _orderBy = OrderKind.DESC
            Return Me
        End Function

        ''' <summary>選択をしない</summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function asNoSelect() As SqlSelectItem
            _isNoSelect = True
            Return Me
        End Function

        ''' <summary>選択項目を条件項目に変換する</summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function toFilter() As SqlFilterItem
            Dim sf As SqlFilterItem = New SqlFilterItem(Me.Column)
            sf.pf(Me.Prefix)
            sf.eq(_value)
            sf.IsKey = Me.IsKey
            Return (sf)
        End Function

        ''' <summary>パラメータの値を設定する</summary>
        ''' <param name="val"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function setValue(ByVal val As Object) As SqlSelectItem
            _value = val
            Return Me
        End Function

        ''' <summary>
        ''' 値を保持しているか否かを判定する。IsNoSelectが設定された項目は値があってもFalseとなる
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function hasValue() As Boolean
            Dim result As Boolean = MyBase.hasValue
            If result And Not IsNoSelect() Then
                Return True
            Else
                Return False
            End If

        End Function

        Public Overrides Function toString() As String
            Dim str As String = ""
            str += Column
            If ColAlias <> "" Then
                str += " AS " + ColAlias
            End If
            If Value IsNot Nothing Then
                str += " -> value:" + Value.ToString
            End If

            Return str

        End Function

    End Class

End Namespace