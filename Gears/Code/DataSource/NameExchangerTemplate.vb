Imports Microsoft.VisualBasic

Namespace Gears.DataSource

    ''' <summary>
    ''' 項目変換を行うためのテンプレートとなる
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NameExchangerTemplate
        Implements INameExchanger

        Private colToItem As New Dictionary(Of String, String)  'テーブル項目->項目名への変換
        Private itemToCol As New Dictionary(Of String, String) '項目名->テーブル項目への変換

        ''' <summary>
        ''' 指定した画面項目名をデータベースの列項目名に変換するルールを登録する<br/>
        ''' </summary>
        ''' <param name="itemName"></param>
        ''' <param name="toCol"></param>
        ''' <remarks></remarks>
        Public Sub addRule(ByVal itemName As String, ByVal toCol As String)
            addDictionarySafe(itemToCol, itemName, toCol)
            addDictionarySafe(colToItem, toCol, itemName)
        End Sub

        ''' <summary>
        ''' 指定した画面項目名をデータベースの列項目名に変換するルールを登録する<br/>
        ''' SQLを実行するときは変換をかけるが、結果セット(DataTable)には変換をかけない
        ''' </summary>
        ''' <param name="fromItem"></param>
        ''' <param name="toCol"></param>
        ''' <remarks></remarks>
        Public Sub addRuleWhenToCol(ByVal fromItem As String, ByVal toCol As String)
            addDictionarySafe(itemToCol, fromItem, toCol)
        End Sub

        ''' <summary>
        ''' 指定したデータベースの列項目名を画面項目名に変換するルールを登録する<br/>
        ''' 結果セット(DataTable)を読み込む際にのみ変換をかける
        ''' </summary>
        ''' <param name="fromCol"></param>
        ''' <param name="toItem"></param>
        ''' <remarks></remarks>
        Public Sub addRuleWhenToItem(ByVal fromCol As String, ByVal toItem As String)
            addDictionarySafe(colToItem, fromCol, toItem)
        End Sub

        ''' <summary>
        ''' 登録されたルールに基づき、列項目名を画面項目名に変換する
        ''' </summary>
        ''' <param name="col"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function changeColToItem(ByVal col As String) As String Implements INameExchanger.changeColToItem
            Return getDictionarySafe(colToItem, col)
        End Function

        ''' <summary>
        ''' 登録されたルールに基づき、画面項目名を列項目名に変換する
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function changeItemToCol(ByVal item As String) As String Implements INameExchanger.changeItemToCol
            Return getDictionarySafe(itemToCol, item)
        End Function

        ''' <summary>ディクショナリ/キーがNothingの場合も考慮し値を読み出す</summary>
        Protected Function getDictionarySafe(Of T, K)(ByRef dic As Dictionary(Of T, K), ByVal key As T) As K
            If Not key Is Nothing And Not dic Is Nothing Then
                If dic.ContainsKey(key) Then
                    Return dic(key)
                Else
                    Return Nothing
                End If

            Else
                Return Nothing
            End If

        End Function

        ''' <summary>ディクショナリ/キーがNothingの場合も考慮し値を追加する</summary>
        Protected Sub addDictionarySafe(Of T, K)(ByRef dic As Dictionary(Of T, K), ByVal key As T, ByVal val As K)
            If Not dic Is Nothing And Not key Is Nothing Then
                If dic.ContainsKey(key) Then
                    dic(key) = val
                Else
                    dic.Add(key, val)
                End If
            End If
        End Sub

    End Class

End Namespace
