
Namespace Gears.DataSource

    ''' <summary>
    ''' リストアイテムのように、Key/Valueで構成されるデータソースのためのクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class AbsKeyValueSource
        Inherits GearsDataSource

        ''' <summary>キーとなる列</summary>
        Public Property KeyColumn() As SqlSelectItem

        ''' <summary>値(テキスト)となる列</summary>
        Public Property ValueColumn() As SqlSelectItem

        Public Sub New(ByVal conName As String, ByVal tableName As String, ByVal keyName As String, ByVal valueName As String)
            MyBase.New(conName, tableName)
            KeyColumn = SqlBuilder.S(keyName).ASC 'デフォルトキー順
            ValueColumn = SqlBuilder.S(valueName)
        End Sub

        Public Sub New(ByVal conName As String, ByVal table As SqlDataSource, ByVal key As SqlSelectItem, ByVal value As SqlSelectItem)
            MyBase.New(conName, table)
            KeyColumn = key
            ValueColumn = value

            '別名が設定されている場合、変換ルールを登録する
            If Not String.IsNullOrEmpty(KeyColumn.ColAlias) Then
                Mapper.addRule(KeyColumn.ColAlias, KeyColumn.Column)
            End If
            If Not String.IsNullOrEmpty(ValueColumn.ColAlias) Then
                Mapper.addRule(ValueColumn.ColAlias, KeyColumn.Column)
            End If

        End Sub

        Public Overrides Function makeSqlBuilder(ByVal data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(KeyColumn)
            sqlb.addSelection(ValueColumn)

            Return sqlb

        End Function

    End Class

End Namespace
