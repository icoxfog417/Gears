Imports Microsoft.VisualBasic

Namespace Gears.DataSource

    ''' <summary>
    ''' 項目名を変換するためのクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface INameExchanger
        '画面上で使用する項目名をDB上の列名に変換するメソッド
        Function changeItemToColumn(ByVal item As String) As String

        'DB上で使用する列名を画面上で使用する項目名に変換するメソッド
        Function changeColumnToItem(ByVal column As String) As String

    End Interface

End Namespace
