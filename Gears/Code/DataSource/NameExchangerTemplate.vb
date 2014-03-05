Imports Microsoft.VisualBasic

Namespace Gears.DataSource

    ''' <summary>
    ''' 項目変換を行うためのクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NameExchangerTemplete
        Implements INameExchanger

        Private colToItem As New Dictionary(Of String, String)  'テーブル項目->項目名への変換
        Private itemToCol As New Dictionary(Of String, String) '項目名->テーブル項目への変換

        Public Sub addRule(ByVal viewItem As String, ByVal toCol As String)
            addDictionarySafe(itemToCol, viewItem, toCol)
            addDictionarySafe(colToItem, toCol, viewItem)
        End Sub

        Public Sub addRuleWhenToCol(ByVal fromItem As String, ByVal toCol As String)
            addDictionarySafe(itemToCol, fromItem, toCol)
        End Sub
        Public Sub addRuleWhenToItem(ByVal fromCol As String, ByVal toItem As String)
            addDictionarySafe(colToItem, fromCol, toItem)
        End Sub

        Public Function changeColToItem(ByVal col As String) As String Implements INameExchanger.changeColToItem
            Return getDictionarySafe(colToItem, col)
        End Function

        Public Function changeItemToCol(ByVal item As String) As String Implements INameExchanger.changeItemToCol
            Return getDictionarySafe(itemToCol, item)
        End Function

        Protected Function getDictionarySafe(Of T, K)(ByRef dic As Dictionary(Of T, K), ByVal id As T) As K
            If Not id Is Nothing And Not dic Is Nothing Then
                If dic.ContainsKey(id) Then
                    Return dic(id)
                Else
                    Return Nothing
                End If

            Else
                Return Nothing
            End If

        End Function

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
