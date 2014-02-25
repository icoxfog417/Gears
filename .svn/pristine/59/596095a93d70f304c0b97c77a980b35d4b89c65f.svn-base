Imports Microsoft.VisualBasic
Imports System.Collections.Generic

Namespace Gears

Public Enum RelationKind As Integer
    LEFT_OUTER_JOIN
    INNER_JOIN
End Enum

<Serializable()>
Public Class SqlDataSource

    'データソース元(普通、テーブル名)
    Private _dataSource As String
    Public Property DataSource() As String
        Get
            Return _dataSource
        End Get
        Set(ByVal value As String)
            _dataSource = value
        End Set
    End Property

    'スキーマ名称
    Private _schema As String
    Public Property Schema() As String
        Get
            Return _schema
        End Get
        Set(ByVal value As String)
            _schema = value
        End Set
    End Property

    '短縮名 (SELECT * FROM HOGETABLE a の aにあたる部分)
        Private _suffix As String
        Public Property Suffix() As String
            Get
                Return _suffix
            End Get
            Set(ByVal value As String)
                _suffix = value
            End Set
        End Property

    '特にパイプライン表関数などで、パラメーターを使用する場合に使用
    Private _value As New Dictionary(Of String, String)

    '結合の相手先
    Private rels As List(Of SqlDataSource) = New List(Of SqlDataSource)
    '結合種別
    Private relKind As Dictionary(Of String, RelationKind) = New Dictionary(Of String, RelationKind)
    '結合キー
        Private relKey As Dictionary(Of String, List(Of SqlFilterItem)) = New Dictionary(Of String, List(Of SqlFilterItem))

        Public Sub New(ByVal ds As String, Optional ByVal sf As String = "")
            _dataSource = ds
            _suffix = sf
        End Sub
        Public Sub New(ByVal ds As SqlDataSource)
            Me._dataSource = ds.DataSource
            Me._schema = ds.Schema
            Me._suffix = ds.Suffix
            Me._value = New Dictionary(Of String, String)(ds.getValue)
            Me.rels = New List(Of SqlDataSource)(ds.rels)
            Me.relKind = New Dictionary(Of String, RelationKind)(ds.relKind)
            Me.relKey = New Dictionary(Of String, List(Of SqlFilterItem))(ds.relKey)
        End Sub


    Public Function inSchema(ByVal strSchema As String) As SqlDataSource
        _schema = strSchema
        Return Me
    End Function
    Public Function hasRelation() As Boolean
        If rels.Count = 0 Then
            Return False
        Else
            Return True
        End If
    End Function
        'Public Function getDataSourceForSql(Optional ByVal isMulti As Boolean = False) As String
        '    Dim ds As String = ""
        '    If _schema <> "" Then
        '        ds += _schema + "."
        '    End If

        '    If _value.Count = 0 Then
        '        If isMulti Then
        '            ds += """" + _dataSource + """" 'Oracle用マルチバイト対策
        '        Else
        '            ds += _dataSource 'Oracle用マルチバイト対策
        '        End If
        '    Else
        '        ds += _dataSource '値指定がある場合、二重引用符をつけるとおかしくなる可能性大なのでつけない
        '    End If
        '    If _alias <> "" Then
        '        ds += " " + _alias
        '    End If
        '    Return ds

        'End Function

    Public Function getRels() As List(Of SqlDataSource)
        Return rels
    End Function
    Public Function getRelKind(ByVal relate As String) As RelationKind
        If relKind.ContainsKey(relate) Then
            Return relKind(relate)
        Else
            Return Nothing
        End If
    End Function
    Public Function getRelKey(ByVal relate As String) As List(Of SqlFilterItem)
        If relKey.ContainsKey(relate) Then
            Return relKey(relate)
        Else
            Return Nothing
        End If
    End Function
    Public Function getValue() As Dictionary(Of String, String)
        Return _value
    End Function
    Public Sub getValue(ByRef params As Dictionary(Of String, String))
        For Each item As KeyValuePair(Of String, String) In _value
            params.Add(item.Key, item.Value)
        Next
    End Sub
    Public Function setValue(ByVal pname As String, ByVal pval As String) As SqlDataSource
        If Not pname Is Nothing And Not pval Is Nothing And pname <> "" Then
            If _value.ContainsKey(pname) Then
                _value(pname) = pval
            Else
                _value.Add(pname, pval)
            End If
        End If

        Return Me
    End Function
    Public Function setValue(ByVal item As SqlItem) As SqlDataSource
        If Not item Is Nothing Then
            If item.Value <> "" Then
                If _value.ContainsKey(item.Column) Then
                    _value(item.Column) = item.Value
                Else
                    _value.Add(item.Column, item.Value)
                End If
            End If
        End If
        Return Me
    End Function
    Public Sub clearValue()
        _value.Clear()
    End Sub

        Public Function leftOuterJoin(ByVal dsName As String, ByVal sf As String, ByVal ParamArray cr() As SqlFilterItem) As SqlDataSource
            Dim ds As New SqlDataSource(dsName, sf)
            Return leftOuterJoin(ds, cr)

        End Function
        Public Function leftOuterJoin(ByRef sd As SqlDataSource, ByVal ParamArray cr() As SqlFilterItem) As SqlDataSource
            makeRelation(sd, cr)
            relKind.Add(sd.DataSource, RelationKind.LEFT_OUTER_JOIN)

            Return Me

        End Function

        Public Function innerJoin(ByVal dsName As String, ByVal sf As String, ByVal ParamArray cr() As SqlFilterItem) As SqlDataSource
            Dim ds As New SqlDataSource(dsName, sf)
            Return innerJoin(ds, cr)
        End Function

        Public Function innerJoin(ByRef sd As SqlDataSource, ByVal ParamArray cr() As SqlFilterItem) As SqlDataSource
            relKind.Add(sd.DataSource, RelationKind.INNER_JOIN)
            makeRelation(sd, cr)
            Return Me

        End Function

    Private Sub makeRelation(ByRef sd As SqlDataSource, ByVal ParamArray cr() As SqlFilterItem)
        rels.Add(sd)
            Dim list As List(Of SqlFilterItem) = New List(Of SqlFilterItem)(cr)
            For Each item As SqlFilterItem In list
                item.pf(Me)
                item.JoinTarget.pf(sd)
            Next
        relKey.Add(sd.DataSource, list)

    End Sub


End Class

End Namespace