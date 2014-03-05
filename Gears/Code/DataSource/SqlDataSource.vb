Imports Microsoft.VisualBasic
Imports System.Collections.Generic

Namespace Gears.DataSource

    ''' <summary>
    ''' 結合条件種別
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum RelationKind As Integer
        LEFT_OUTER_JOIN
        INNER_JOIN
    End Enum

    ''' <summary>
    ''' SQLの実行対象(Table/View)を表すクラス<br/>
    ''' 標準クラスと名称が重複しているので、名称変更を検討
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    Public Class SqlDataSource

        Private _dataSource As String
        ''' <summary>
        ''' データソース名(テーブル/ビュー)
        ''' </summary>
        Public Property DataSource() As String
            Get
                Return _dataSource
            End Get
            Set(ByVal value As String)
                _dataSource = value
            End Set
        End Property

        Private _schema As String
        ''' <summary>スキーマ名称</summary>
        Public Property Schema() As String
            Get
                Return _schema
            End Get
            Set(ByVal value As String)
                _schema = value
            End Set
        End Property

        Private _suffix As String
        ''' <summary>
        ''' 後置詞<br/>
        ''' (SELECT * FROM tab a の aにあたる部分)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Suffix() As String
            Get
                Return _suffix
            End Get
            Set(ByVal value As String)
                _suffix = value
            End Set
        End Property

        Private _value As New Dictionary(Of String, Object)
        ''' <summary>
        ''' 特にパイプライン表関数などで、パラメーターを使用する場合に使用する値
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Value As Dictionary(Of String, Object)
            Get
                Return _value
            End Get
        End Property

        ''' <summary>
        ''' ディクショナリにSqlDataSourceに設定された値情報を読み込む
        ''' </summary>
        ''' <param name="params"></param>
        ''' <remarks></remarks>
        Public Sub readValues(ByRef params As Dictionary(Of String, Object))
            For Each item As KeyValuePair(Of String, Object) In _value
                params.Add(item.Key, item.Value)
            Next
        End Sub

        Private _joinTargets As List(Of SqlDataSource) = New List(Of SqlDataSource)
        ''' <summary>結合の相手先を取得する</summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property JoinTargets As List(Of SqlDataSource)
            Get
                Return _joinTargets
            End Get
        End Property

        Private _relations As Dictionary(Of String, RelationKind) = New Dictionary(Of String, RelationKind)
        ''' <summary>指定対象の結合種別を取得する</summary>
        ''' <param name="target">結合対象</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getRelation(ByVal target As String) As RelationKind
            If _relations.ContainsKey(target) Then
                Return _relations(target)
            Else
                Return Nothing
            End If
        End Function

        Private _joinKeys As Dictionary(Of String, List(Of SqlFilterItem)) = New Dictionary(Of String, List(Of SqlFilterItem))
        ''' <summary>指定対象の結合キーを取得する</summary>
        ''' <param name="target"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getJoinKey(ByVal target As String) As List(Of SqlFilterItem)
            If _joinKeys.ContainsKey(target) Then
                Return _joinKeys(target)
            Else
                Return Nothing
            End If
        End Function

        Public Sub New(ByVal ds As String, Optional ByVal sf As String = "")
            _dataSource = ds
            _suffix = sf
        End Sub
        Public Sub New(ByVal ds As SqlDataSource)
            Me._dataSource = ds.DataSource
            Me._schema = ds.Schema
            Me._suffix = ds.Suffix
            Me._value = New Dictionary(Of String, Object)(ds.Value)
            Me._joinTargets = New List(Of SqlDataSource)(ds._joinTargets)
            Me._relations = New Dictionary(Of String, RelationKind)(ds._relations)
            Me._joinKeys = New Dictionary(Of String, List(Of SqlFilterItem))(ds._joinKeys)
        End Sub

        ''' <summary>スキーマを指定する</summary>
        ''' <param name="strSchema"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function inSchema(ByVal strSchema As String) As SqlDataSource
            _schema = strSchema
            Return Me
        End Function

        ''' <summary>リレーションを持つか否か</summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function hasRelation() As Boolean
            If _joinTargets.Count = 0 Then
                Return False
            Else
                Return True
            End If
        End Function

        ''' <summary>パラメータの設定</summary>
        ''' <param name="pname"></param>
        ''' <param name="pval"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function setValue(ByVal pname As String, ByVal pval As Object) As SqlDataSource
            If Not String.IsNullOrEmpty(pname) And Not pval Is Nothing Then
                If _value.ContainsKey(pname) Then
                    _value(pname) = pval
                Else
                    _value.Add(pname, pval)
                End If
            End If
            Return Me
        End Function

        ''' <summary>パラメータの設定(SqlItemから)</summary>
        ''' <param name="item"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function setValue(ByVal item As SqlItem) As SqlDataSource
            setValue(item.Column, item.Value)
            Return Me
        End Function

        ''' <summary>左結合を行う</summary>
        ''' <param name="dsName">結合対象データソース</param>
        ''' <param name="sf">後置詞</param>
        ''' <param name="cr">結合カラム</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function leftOuterJoin(ByVal dsName As String, ByVal sf As String, ByVal ParamArray cr() As SqlFilterItem) As SqlDataSource
            Dim ds As New SqlDataSource(dsName, sf)
            Return leftOuterJoin(ds, cr)

        End Function

        ''' <summary>左結合を行う</summary>
        ''' <param name="sd">結合対象データソース</param>
        ''' <param name="cr">結合カラム</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function leftOuterJoin(ByRef sd As SqlDataSource, ByVal ParamArray cr() As SqlFilterItem) As SqlDataSource
            makeRelation(sd, cr)
            _relations.Add(sd.DataSource, RelationKind.LEFT_OUTER_JOIN)

            Return Me

        End Function

        ''' <summary>内部結合を行う</summary>
        ''' <param name="dsName">結合対象データソース</param>
        ''' <param name="sf">後置詞</param>
        ''' <param name="cr">結合カラム</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function innerJoin(ByVal dsName As String, ByVal sf As String, ByVal ParamArray cr() As SqlFilterItem) As SqlDataSource
            Dim ds As New SqlDataSource(dsName, sf)
            Return innerJoin(ds, cr)
        End Function

        ''' <summary>内部合を行う</summary>
        ''' <param name="sd">結合対象データソース</param>
        ''' <param name="cr">結合カラム</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function innerJoin(ByRef sd As SqlDataSource, ByVal ParamArray cr() As SqlFilterItem) As SqlDataSource
            _relations.Add(sd.DataSource, RelationKind.INNER_JOIN)
            makeRelation(sd, cr)
            Return Me

        End Function

        ''' <summary>
        ''' 結合のための設定を行う内部関数
        ''' </summary>
        ''' <param name="sd"></param>
        ''' <param name="cr"></param>
        ''' <remarks></remarks>
        Private Sub makeRelation(ByRef sd As SqlDataSource, ByVal ParamArray cr() As SqlFilterItem)
            _joinTargets.Add(sd)
            Dim list As List(Of SqlFilterItem) = New List(Of SqlFilterItem)(cr)
            For Each item As SqlFilterItem In list
                item.pf(Me) '結合元には自身のsuffixを付与
                item.JoinTarget.pf(sd) '結合相手は相手先のsuffixを付与
            Next
            _joinKeys.Add(sd.DataSource, list)

        End Sub

    End Class

End Namespace