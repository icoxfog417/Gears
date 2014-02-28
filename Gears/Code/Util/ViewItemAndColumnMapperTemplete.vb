Imports Microsoft.VisualBasic

Namespace Gears

    Public Class ViewItemAndColumnMapperTemplete
        Implements IViewItemAndColumnMapper

        Protected Const TEXT_KEY As String = "TEXT_KEY"
        Protected Const DATA_TYPE_KEY As String = "DATA_TYPE_KEY"

        Private colToItem As New Dictionary(Of String, String)  'テーブル項目->項目名への変換
        Private itemToCol As New Dictionary(Of String, String) '項目名->テーブル項目への変換
        Private colAttr As New Dictionary(Of String, Dictionary(Of String, String))   '項目関連情報

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

        Public Sub addAttribute(ByVal key As String, ByVal attr As String, ByVal attrVal As String)
            Dim list As Dictionary(Of String, String) = getDictionarySafe(colAttr, key)
            If list Is Nothing Then
                Dim nlist As New Dictionary(Of String, String)
                nlist.Add(attr, attrVal)
                colAttr.Add(key, nlist)
            Else
                If list.ContainsKey(attr) Then
                    list(attr) = attrVal
                Else
                    list.Add(attr, attrVal)
                End If
            End If
        End Sub
        Public Function changeColToItem(ByVal col As String) As String Implements IViewItemAndColumnMapper.changeColToItem
            Return getDictionarySafe(colToItem, col)
        End Function

        Public Function changeItemToCol(ByVal item As String) As String Implements IViewItemAndColumnMapper.changeItemToCol
            Return getDictionarySafe(itemToCol, item)
        End Function
        Public Function getDataText(ByVal col As String) As String Implements IViewItemAndColumnMapper.getDataText
            If Not getColAttr(col, TEXT_KEY) Is Nothing Then
                Return getColAttr(col, TEXT_KEY)
            Else
                Return ""
            End If

        End Function
        Public Overridable Function getDataType(ByVal col As String) As DBDataType Implements IViewItemAndColumnMapper.getDataType
            Dim dataType As String = getColAttr(col, DATA_TYPE_KEY)
            Dim resultType As DBDataType = DBDataType.T_STRING

            If String.IsNullOrEmpty(dataType) Then
                dataType = ""
            End If

            If dataType.IndexOf("CHAR") > -1 Or dataType.IndexOf("STRING") > -1 Then
                If dataType.EndsWith("_YMD") Or dataType.EndsWith("_HMS") Then
                    resultType = DBDataType.T_DATE
                Else
                    resultType = DBDataType.T_STRING
                End If
            ElseIf dataType.IndexOf("NUMBER") > -1 Then
                resultType = DBDataType.T_NUMBER
            ElseIf dataType.IndexOf("DATE") > -1 Then
                resultType = DBDataType.T_DATE

            End If

            Return resultType

        End Function
        Public Sub setColText(ByVal col As String, ByVal text As String)
            addAttribute(col, TEXT_KEY, text)
        End Sub
        Public Sub setColDataType(ByVal col As String, ByVal type As String)
            addAttribute(col, DATA_TYPE_KEY, type)
        End Sub

        Public Function getColAttr(ByVal col As String, ByVal attrType As String) As String Implements IViewItemAndColumnMapper.getColAttr
            Dim list As Dictionary(Of String, String) = getDictionarySafe(colAttr, col)
            Return getDictionarySafe(list, attrType)
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
