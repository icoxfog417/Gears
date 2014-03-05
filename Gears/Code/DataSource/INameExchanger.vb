Imports Microsoft.VisualBasic

Namespace Gears.DataSource

    ''' <summary>
    ''' 項目名を変換するためのクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface INameExchanger
        'View上で使用する項目名をDB上の列名に変換するメソッド
        Function changeItemToCol(ByVal item As String) As String

        'DB上で使用する列名をView上で使用する項目名に変換するメソッド
        Function changeColToItem(ByVal col As String) As String

    End Interface

End Namespace
