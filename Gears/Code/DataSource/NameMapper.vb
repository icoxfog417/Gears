Imports Microsoft.VisualBasic

Namespace Gears.DataSource

    ''' <summary>
    ''' 項目変換を行うためのテンプレートとなる
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NameMapper
        Implements INameMapper

        Private columnToItem As New Dictionary(Of String, String)  'テーブル項目->項目名への変換
        Private itemToColumn As New Dictionary(Of String, String) '項目名->テーブル項目への変換

        ''' <summary>
        ''' 指定した画面項目名をデータベースの列項目名に変換するルールを登録する<br/>
        ''' </summary>
        ''' <param name="item"></param>
        ''' <param name="column"></param>
        ''' <remarks></remarks>
        Public Sub addRule(ByVal item As String, ByVal column As String) Implements INameMapper.addRule
            addDictionarySafe(itemToColumn, item, column)
            addDictionarySafe(columnToItem, column, item)
        End Sub

        ''' <summary>
        ''' 登録されたルールに基づき、列項目名を画面項目名に変換する
        ''' </summary>
        ''' <param name="column"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function changeColumnToItem(ByVal column As String) As String Implements INameMapper.changeColumnToItem
            Return getDictionarySafe(columnToItem, column)
        End Function

        ''' <summary>
        ''' 登録されたルールに基づき、画面項目名を列項目名に変換する
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function changeItemToColumn(ByVal item As String) As String Implements INameMapper.changeItemToColumn
            Return getDictionarySafe(itemToColumn, item)
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
