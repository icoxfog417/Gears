Imports Microsoft.VisualBasic
Imports System
Imports System.Web.UI.WebControls
Imports System.Collections
Imports System.Data
Imports Gears.DataSource
Imports Gears.Util

Namespace Gears.Binder

    ''' <summary>
    ''' コントロールに対するDataBind/値設定処理を行う
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsDataBinder
        Implements IDataBinder

        ''' <summary>
        ''' バインド対象か否かを判定する<br/>
        ''' 標準では、リストかGridViewのような複合コントロールを対象とする
        ''' </summary>
        ''' <param name="con"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function isBindable(ByRef con As Control) As Boolean Implements IDataBinder.isBindable

            If TypeOf con Is ListControl Or _
                TypeOf con Is CompositeDataBoundControl Then
                Return True
            Else
                Return False
            End If

        End Function

        ''' <summary>
        ''' データのバインド処理を行う<br/>
        ''' バインド対象データは、データソースクラスにdtoを渡した結果が使用される
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="dsClass"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function dataBind(ByRef con As Control, ByRef dsClass As GearsDataSource, Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Dim db As New GearsDataBinder()
            Dim bindData As DataTable = Nothing

            If dto Is Nothing Then
                bindData = dsClass.gSelect(New GearsDTO(ActionType.SEL))
            Else
                bindData = dsClass.gSelect(dto)
            End If

            Return db.dataBind(con, bindData)

        End Function

        ''' <summary>
        ''' データのバインド処理を行う
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="dset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function dataBind(ByRef con As Control, ByRef dset As System.Data.DataTable) As Boolean Implements IDataBinder.dataBind
            Dim result As Boolean = True
            Try
                If isBindable(con) Then
                    Select Case TypeOf con Is Control
                        Case TypeOf con Is ListControl
                            result = listBind(CType(con, ListControl), dset)
                        Case TypeOf con Is CompositeDataBoundControl
                            result = compositBind(CType(con, CompositeDataBoundControl), dset)
                    End Select
                End If

            Catch ex As Exception
                Throw
            End Try

            Return result

        End Function

        ''' <summary>
        ''' リスト型コントロールに対しバインド処理を行う
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="dset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Overridable Function listBind(ByRef list As ListControl, ByRef dset As System.Data.DataTable) As Boolean

            Dim defFirst As ArrayList = New ArrayList
            Dim defEnd As ArrayList = New ArrayList
            Dim result As Boolean = True

            '既存リスト項目を一旦削除する。ただし、デフォルトで表示する設定のものは残す
            For Each item As ListItem In list.Items
                If item.Attributes("Default") <> "" Then
                    If item.Attributes("Position") = "F" Then
                        defFirst.Add(item)
                    Else
                        defEnd.Add(item)
                    End If
                End If
            Next
            list.Items.Clear()

            'アイテム追加
            '先頭に追加するアイテムを設定
            For Each item As ListItem In defFirst
                list.Items.Add(item)
            Next

            If String.IsNullOrEmpty(list.DataValueField) Then list.DataValueField = dset.Columns(0).ColumnName
            If String.IsNullOrEmpty(list.DataTextField) Then list.DataTextField = dset.Columns(1).ColumnName
            For i As Integer = 0 To dset.Rows.Count - 1

                Dim key As String = DataSetReader.Item(dset, list.DataValueField, i).ToString
                Dim value As String = DataSetReader.Item(dset, list.DataTextField, i).ToString
                list.Items.Add(New ListItem(value, key))

            Next
            For Each item As ListItem In defEnd
                list.Items.Add(item)
            Next

            Return result

        End Function

        ''' <summary>
        ''' GridViewなどの複合データソースに対するバインド処理
        ''' </summary>
        ''' <param name="dbound"></param>
        ''' <param name="dset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Overridable Function compositBind(ByRef dbound As CompositeDataBoundControl, ByRef dset As System.Data.DataTable) As Boolean
            Dim result As Boolean = True
            Try
                dbound.DataSource = dset
                dbound.DataBind()

            Catch ex As Exception
                Dim gex As GearsDataBindException = New GearsDataBindException(dbound, ex)
                Throw gex
            End Try
            Return result
        End Function

        ''' <summary>
        ''' コントロールに値を設定する
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="dset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function dataAttach(ByRef con As Control, ByRef dset As System.Data.DataTable) As Boolean Implements IDataBinder.dataAttach
            Dim result As Boolean = True
            Try
                If Not dset Is Nothing AndAlso dset.Rows.Count > 0 Then
                    If TypeOf con Is ListControl Then
                        result = listAttach(CType(con, ListControl), dset)
                    ElseIf TypeOf con Is GridView Then
                        Dim key As String = GearsControl.serializeValue(dset, 0)
                        If String.IsNullOrEmpty(key) AndAlso CType(con, GridView).DataKeyNames IsNot Nothing Then
                            Dim row As DataRow = dset.Rows(0)
                            Dim keyValues As New ArrayList
                            For Each k As String In CType(con, GridView).DataKeyNames
                                keyValues.Add(row(k))
                            Next
                            key = GearsControl.serializeValue(keyValues)
                        End If
                        setValue(con, key)
                    Else

                        Dim value As Object = DataSetReader.Item(dset, GearsControl.extractDataSourceid(con.ID))
                        If Not value Is Nothing Then 'Nothing = データテーブルに該当項目がない
                            setValue(con, value)
                        End If
                    End If
                End If

            Catch ex As Exception
                Throw

            End Try

            Return result

        End Function

        ''' <summary>
        ''' リストコントロールに値をセットする
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="dset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Overridable Function listAttach(ByRef list As ListControl, ByRef dset As System.Data.DataTable) As Boolean
            Dim result As Boolean = True
            For i = dset.Rows.Count - 1 To 0 Step -1
                'データソースIDに基づきバインドを行う(リストキー項目がデータソース名である保証はない)
                Dim value As Object = DataSetReader.Item(dset, GearsControl.extractDataSourceid(list.ID), i)
                If Not value Is Nothing Then
                    If Not list.Items.FindByValue(value.ToString) Is Nothing Then
                        list.SelectedValue = value.ToString
                    Else
                        GearsLogStack.setLog(list.ID + " に値 " + value.ToString + " を設定しようとしましたが、リスト内に存在しないため、設定されません")
                    End If
                Else
                    GearsLogStack.setLog(list.ID + " のデータフィールド " + list.DataValueField + " がありません")
                End If
            Next

            Return result

        End Function

        ''' <summary>
        ''' コントロールから値を取得する
        ''' </summary>
        ''' <param name="con"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getValue(ByRef con As Control) As Object Implements IDataBinder.getValue
            Dim value As String = "" '文字列型で値を取得

            If TypeOf con Is ListControl Then
                value = GearsControl.serializeValue(CType(con, ListControl))
            ElseIf TypeOf con Is RadioButton Then
                Dim radio As RadioButton = CType(con, RadioButton)
                If radio.Checked Then
                    value = "1" 'True
                Else
                    value = "0" 'False
                End If
            ElseIf TypeOf con Is CheckBox Then
                Dim chk As CheckBox = CType(con, CheckBox)
                If chk.Checked Then
                    value = "1" 'True
                Else
                    value = "0" 'False
                End If
            ElseIf TypeOf con Is TextBox Then
                value = CType(con, TextBox).Text
            ElseIf TypeOf con Is HiddenField Then
                value = CType(con, HiddenField).Value.ToString
            ElseIf TypeOf con Is Label Then
                value = CType(con, Label).Text
            ElseIf TypeOf con Is Literal Then
                value = CType(con, Literal).Text
            ElseIf TypeOf con Is GridView Then
                If Not CType(con, GridView).DataKeyNames Is Nothing AndAlso CType(con, GridView).DataKeyNames.Length > 0 Then 'キー指定有り
                    value = GearsControl.serializeValue(CType(con, GridView).SelectedDataKey)
                End If
            End If

            Return value
        End Function

        ''' <summary>
        ''' コントロールに対して値をセットする
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="value"></param>
        ''' <remarks></remarks>
        Public Sub setValue(ByRef con As Control, ByVal value As Object) Implements IDataBinder.setValue
            Dim valueStr As String = If(value Is Nothing, "", value.ToString) '単一値の場合
            Dim list As ArrayList = GearsControl.deSerializeValue(valueStr)

            If Not list Is Nothing Then
                valueStr = list.Item(0).ToString
            End If

            If TypeOf con Is ListControl Then
                Dim lcon As ListControl = CType(con, ListControl)
                lcon.ClearSelection()
                If Not list Is Nothing Then
                    For Each items As Object In list
                        If Not lcon.Items.FindByValue(items.ToString) Is Nothing Then
                            lcon.Items.FindByValue(items.ToString).Selected = True
                        End If
                    Next
                End If
            ElseIf TypeOf con Is RadioButton Then
                Dim radio As RadioButton = CType(con, RadioButton)
                radio.Checked = False
                If Not list Is Nothing AndAlso valueStr = "1" Then
                    radio.Checked = True
                End If
            ElseIf TypeOf con Is CheckBox Then
                Dim chk As CheckBox = CType(con, CheckBox)
                chk.Checked = False
                If Not list Is Nothing AndAlso valueStr = "1" Then
                    chk.Checked = True
                End If
            ElseIf TypeOf con Is TextBox Then
                CType(con, TextBox).Text = valueStr
            ElseIf TypeOf con Is HiddenField Then
                CType(con, HiddenField).Value = valueStr
            ElseIf TypeOf con Is Label Then
                CType(con, Label).Text = valueStr
            ElseIf TypeOf con Is Literal Then
                CType(con, Literal).Text = valueStr
            ElseIf TypeOf con Is GridView Then
                Dim keys As String() = CType(con, GridView).DataKeyNames
                If keys.Length > 0 Then 'キー指定有り
                    Dim keyList As DataKeyArray = CType(con, GridView).DataKeys
                    Dim index As Integer = -1
                    For i As Integer = 0 To keyList.Count - 1
                        If String.Equals(GearsControl.serializeValue(keyList(i)), value) Then
                            index = i
                            Exit For
                        End If
                    Next

                    CType(con, GridView).SelectedIndex = index

                End If


            End If

        End Sub

    End Class

End Namespace
