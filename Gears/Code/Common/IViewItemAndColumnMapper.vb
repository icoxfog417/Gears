Imports Microsoft.VisualBasic

Namespace Gears

    Public Enum DBDataType
        T_DATE
        T_NUMBER
        T_STRING
    End Enum

    Public Interface IViewItemAndColumnMapper
        'View上で使用する項目名をDB上の列名に変換するメソッド
        Function changeItemToCol(ByVal item As String) As String

        'DB上で使用する列名をView上で使用する項目名に変換するメソッド
        Function changeColToItem(ByVal col As String) As String

        'データ型を取得するためのファンクション(getColAttrより切り出し)
        Function getDataType(ByVal col As String) As DBDataType

        'テキストを取得するためのファンクション(getColAttrより切り出し)
        Function getDataText(ByVal col As String) As String

        'DB上の項目属性を取得するためのメソッド(最終的にDB内にデータとして収まるため、このメソッド名で統一(getItemAttrは持たない))
        Function getColAttr(ByVal col As String, ByVal attrType As String) As String

    End Interface

End Namespace
