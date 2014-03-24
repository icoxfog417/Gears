Imports Microsoft.VisualBasic

Namespace Gears.DataSource

    ''' <summary>
    ''' 項目名を変換するためのクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface INameMapper

        ''' <summary>変換ルールの登録を行う</summary>
        ''' <param name="item"></param>
        ''' <param name="column"></param>
        Sub addRule(ByVal item As String, ByVal column As String)

        ''' <summary>項目名をDB上の列名に変換するメソッド</summary>
        Function changeItemToColumn(ByVal item As String) As String

        ''' <summary>DB上の列名を項目名に変換するメソッド</summary>
        Function changeColumnToItem(ByVal column As String) As String

    End Interface

End Namespace
