Imports Microsoft.VisualBasic

Namespace Gears

Public Enum OrderKind As Integer
    ASC
    DESC
    NON
End Enum

<Serializable()>
Public Class SqlSelectItem
    Inherits SqlItem

    'プロパティ
    Private _colAlias As String = ""
    Public ReadOnly Property ColAlias() As String
        Get
            Return _colAlias
        End Get
        End Property

    Private _isGroupBy As Boolean = False
    Public Property IsGroupBy() As Boolean
        Get
            Return _isGroupBy
        End Get
        Set(ByVal value As Boolean)
            _isGroupBy = value
        End Set
    End Property

    Private _orderBy As OrderKind = OrderKind.NON
    Public Property OrderBy() As OrderKind
        Get
            Return _orderBy
        End Get
        Set(ByVal value As OrderKind)
            _orderBy = value
        End Set
    End Property

        Private _noSelect As Boolean = False
        Public Property NoSelect() As Boolean
            Get
                Return _noSelect
            End Get
            Set(ByVal value As Boolean)
                _noSelect = value
            End Set
        End Property

    'コンストラクタ
        Public Sub New(ByVal col As String, Optional ByVal px As String = "")
            MyBase.New()
            _column = col
            _prefix = px
        End Sub
        Public Sub New(ByVal sqls As SqlSelectItem, Optional ByVal val As String = "")
            MyBase.New(sqls, val)
            _colAlias = sqls.ColAlias
            _isGroupBy = sqls.IsGroupBy
            _orderBy = sqls.OrderBy
            _noSelect = sqls.NoSelect
        End Sub

    'メソッド
    Public Function getColText() As String
        If _colAlias <> "" Then
            Return _colAlias
        Else
            Return _column
        End If
    End Function
        'Public Function getColAliasForSql(Optional ByVal ismulti As Boolean = False) As String
        '    Dim resultAlias As String = _colAlias
        '    If ismulti Then
        '        resultAlias = """" + resultAlias + """"
        '    End If

        '    Return resultAlias

        'End Function

    Public Function pf(ByVal pre As String) As SqlSelectItem
        _prefix = pre
        Return Me
    End Function
    Public Function pf(ByRef sds As SqlDataSource) As SqlSelectItem
            _prefix = sds.Suffix
        Return Me
    End Function

    Public Function asName(ByVal talias As String) As SqlSelectItem
        _colAlias = talias
        Return Me
    End Function

    Public Function key() As SqlSelectItem
        IsKey() = True
        Return Me
    End Function

    Public Function inGroup() As SqlSelectItem
        _isGroupBy = True
        Return Me
    End Function

    Public Function ASC() As SqlSelectItem
        _orderBy = OrderKind.ASC
        Return Me
    End Function

    Public Function DESC() As SqlSelectItem
        _orderBy = OrderKind.DESC
        Return Me
        End Function

        Public Function isNoSelect() As SqlSelectItem
            _noSelect = True
            Return Me
        End Function

    Public Function filter() As SqlFilterItem
            Dim sf As SqlFilterItem = New SqlFilterItem(Me.Column)
            sf.pf(Me.Prefix)
            sf.eq(_value)
            sf.IsKey = Me.IsKey
        Return(sf)
    End Function

    Public Function setValue(ByVal val As String) As SqlSelectItem
        _value = val
        Return Me
    End Function

        Public Overrides Function isActive() As Boolean
            Dim result As Boolean = MyBase.isActive
            If result And Not NoSelect() Then
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
            If Value <> "" Then
                str += " -> value:" + Value
            End If

            Return str

        End Function

    End Class

End Namespace