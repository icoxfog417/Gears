Imports Microsoft.VisualBasic
Imports System
Imports System.Web.UI.WebControls
Imports System.Collections
Imports System.Data

Namespace Gears

    Public Class GBinderTemplate
        Implements IDataBinder

        Private _keyField As String = ""
        Public Property KeyField() As String
            Get
                Return _keyField
            End Get
            Set(ByVal value As String)
                _keyField = value
            End Set
        End Property

        Private _valueField As String = ""
        Public Property ValueField() As String
            Get
                Return _valueField
            End Get
            Set(ByVal value As String)
                _valueField = value
            End Set
        End Property

        Public Shared Function dataBind(ByRef con As Control, ByRef dsClass As GDSTemplate, Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Dim db As New GBinderTemplate()
            Dim bindData As DataTable = Nothing

            If dto Is Nothing Then
                bindData = dsClass.gSelect(New GearsDTO(ActionType.SEL))
            Else
                bindData = dsClass.gSelect(dto)
            End If

            Return db.dataBind(con, bindData)

        End Function

        Public Function dataBind(ByRef con As Control, ByRef dset As System.Data.DataTable) As Boolean Implements IDataBinder.dataBind
            Dim result As Boolean = True
            Try
                If TypeOf con Is ListControl Then
                    result = listBind(CType(con, ListControl), dset)
                ElseIf TypeOf con Is CompositeDataBoundControl Then 'GridViewとかそのあたり
                    result = compositBind(CType(con, CompositeDataBoundControl), dset)

                Else
                    'dataBindという概念がないため処理しない(行うとすればattachのはず)
                    'Dim value As String = GearsSqlExecutor.getDataSetValue(GearsControl.getDataSourceId(con.ID), dset)
                    'If Not value Is Nothing Then
                    '    setValue(con, value)
                    'End If
                End If

            Catch ex As Exception
                Throw
            End Try

            Return result

        End Function

        Protected Overridable Function listBind(ByRef list As ListControl, ByRef dset As System.Data.DataTable) As Boolean

            Dim defFirst As ArrayList = New ArrayList
            Dim defEnd As ArrayList = New ArrayList
            Dim result As Boolean = True

            '既存リスト項目の削除
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
            For Each item As ListItem In defFirst
                list.Items.Add(item)
            Next
            For i As Integer = 0 To dset.Rows.Count - 1
                Dim key As String = ""
                Dim value As String = ""
                If _keyField <> "" And _valueField <> "" Then
                    key = GearsSqlExecutor.getDataSetValue(_keyField, dset, i)
                    value = GearsSqlExecutor.getDataSetValue(_valueField, dset, i)
                    If i = 0 Then
                        list.DataValueField = _keyField
                        list.DataTextField = _valueField
                    End If
                Else
                    key = GearsSqlExecutor.getDataSetValue(0, dset, i)
                    value = GearsSqlExecutor.getDataSetValue(1, dset, i)
                    If i = 0 Then
                        list.DataValueField = dset.Columns(0).ColumnName
                        list.DataTextField = dset.Columns(1).ColumnName
                    End If

                End If

                list.Items.Add(New ListItem(value, key))
            Next
            For Each item As ListItem In defEnd
                list.Items.Add(item)
            Next

            Return result

        End Function

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

        Public Function dataAttach(ByRef con As Control, ByRef dset As System.Data.DataTable) As Boolean Implements IDataBinder.dataAttach
            Dim result As Boolean = True
            Try
                If Not dset Is Nothing AndAlso dset.Rows.Count > 0 Then
                    If TypeOf con Is ListControl Then
                        result = listAttach(CType(con, ListControl), dset)
                    ElseIf TypeOf con Is GridView Then
                        For i As Integer = 0 To dset.Rows.Count - 1
                            setValue(con, GearsControl.serializeValue(dset, i))
                        Next
                    Else

                        Dim value As String = GearsSqlExecutor.getDataSetValue(GearsControl.getDataSourceid(con.ID), dset)
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
        Protected Overridable Function listAttach(ByRef list As ListControl, ByRef dset As System.Data.DataTable) As Boolean
            Dim result As Boolean = True
            For i = dset.Rows.Count - 1 To 0 Step -1
                'データソースIDに基づきバインドを行う(リストキー項目がデータソース名である保証はない)
                'Dim value As String = GearsSqlExecutor.getDataSetValue(list.DataValueField, dset, i)
                Dim value As String = GearsSqlExecutor.getDataSetValue(GearsControl.getDataSourceid(list.ID), dset, i)
                If Not value Is Nothing Then
                    If Not list.Items.FindByValue(value) Is Nothing Then
                        list.SelectedValue = value
                    Else
                        GearsLogStack.setLog(list.ID + " に値 " + value + " を設定しようとしましたが、リスト内に存在しないため、設定されません")

                    End If
                Else
                    GearsLogStack.setLog(list.ID + " のデータフィールド " + list.DataValueField + " がありません")

                End If
            Next

            Return result

        End Function
        

        Public Function getValue(ByRef con As Control) As String Implements IDataBinder.getValue
            Dim value As String = ""

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

        Public Sub setValue(ByRef con As Control, ByVal value As String) Implements IDataBinder.setValue
            Dim list As ArrayList = GearsControl.deSerializeValue(value)
            Dim valueStr As String = "" '単一値の場合

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
