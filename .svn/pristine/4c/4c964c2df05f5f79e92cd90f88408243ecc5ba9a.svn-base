Imports Microsoft.VisualBasic
Imports System.Data

'GridViewを扱うクラス
Public Class GridViewUtility

    Private Const RECORD_T_COUNT_ATTR As String = "_TOTAL_COUNT"
    Private Const RECORD_S_COUNT_ATTR As String = "_SELECTED_COUNT"

    Private WithEvents gvRef As GridView

    'ObjectDataSource以外のDataSourceも対応したいところだが、select関係の処理/イベントが全て各DataSourceコントロールで異なり、
    'またそれをまとめているInterfaceも存在しないことから、とりあえずObjectDataSourceだけ対応する。
    Private WithEvents gvOdsDataSource As ObjectDataSource

    'GridViewの状態を表示するためのラベル
    Private gvStateLabel As Label

    Private hideColumns As New List(Of String)
    Private invalidRecords As New Dictionary(Of String, String)
    Private _invalidRecordCss As String = "ppp-table-invalid"
    Public Property InvalidRecordCss() As String
        Get
            Return _invalidRecordCss
        End Get
        Set(ByVal value As String)
            _invalidRecordCss = value
        End Set
    End Property

    Private _hideCss As String = "ppp-table-invisible"
    Public Property HideCss() As String
        Get
            Return _hideCss
        End Get
        Set(ByVal value As String)
            _hideCss = value
        End Set
    End Property


    Public Delegate Function fetchGrvRow(ByVal index As Integer, ByRef rowInfo As Dictionary(Of String, TableCell)) As String


    'コンストラクタ 対象のGridViewを保持(発生イベントを受け取れるように、WithEvents宣言)
    Public Sub New(ByRef gv As GridView)
        gvRef = gv
        setDataSource()
    End Sub
    Public Sub New(ByRef gv As GridView, ByRef lbl As Label)
        gvRef = gv
        setDataSource()

        gvStateLabel = lbl
    End Sub

    Public Sub setEventSourceGridView(ByRef gv As GridView)
        gvRef = gv
        setDataSource()
    End Sub

    Private Sub setDataSource()
        If Not gvRef Is Nothing AndAlso Not String.IsNullOrEmpty(gvRef.DataSourceID) Then

            Dim now As Control = gvRef.Parent
            While Not now Is Nothing
                If Not now.FindControl(gvRef.DataSourceID) Is Nothing Then
                    Dim targetDataSource As DataSourceControl = now.FindControl(gvRef.DataSourceID)
                    If TypeOf targetDataSource Is ObjectDataSource Then
                        gvOdsDataSource = CType(targetDataSource, ObjectDataSource)
                    End If

                    Exit While
                End If
                now = now.Parent
            End While
        End If

    End Sub


    '無効のスタイルを適用する場合の条件(列名と、どのような値の場合スタイルを適用するか)を設定
    Public Sub addInvalidRecordDef(column As String, value As String)
        If invalidRecords.ContainsKey(column) Then
            invalidRecords(column) = value
        Else
            invalidRecords.Add(column, value)
        End If

    End Sub
    Public Sub addHideColumn(ByVal colname As String)
        hideColumns.Add(colname)
    End Sub

    '列カラム名からGridView内の値を取得する
    Public Shared Function getGridViewValue(ByRef gv As GridView, ByVal colname As String, Optional ByVal row As Integer = 0) As String
        Dim cell As TableCell = getGridViewCell(gv, colname, row)
        If Not cell Is Nothing Then
            Return cell.Text
        Else
            Return Nothing
        End If

    End Function
    Public Shared Function getGridViewCell(ByRef gv As GridView, ByVal colname As String, Optional ByVal row As Integer = 0) As TableCell
        Dim index As Integer = getGridViewColumnIndex(gv, colname)

        If index > -1 Then
            Return gv.Rows(row).Cells(index)
        Else
            Return Nothing
        End If

    End Function
    'RowDataBoundなどで使用
    Public Shared Function getGridViewRowCell(ByRef gv As GridView, ByRef grvRow As GridViewRow, ByVal colname As String) As TableCell
        Dim index As Integer = getGridViewColumnIndex(gv, colname)

        If index > -1 Then
            Return grvRow.Cells(index)
        Else
            Return Nothing
        End If

    End Function
    Public Shared Function getGridViewColumnIndex(ByRef gv As GridView, ByVal colname As String) As Integer
        Dim index As Integer = 0
        Dim isExist As Boolean = False
        If Not gv.HeaderRow Is Nothing Then
            For Each cell As TableCell In gv.HeaderRow.Cells
                If cell.Text = colname Then
                    isExist = True
                    Exit For
                End If
                index += 1
            Next

            If isExist Then
                Return index
            Else
                Return -1
            End If
        Else
            Return -1
        End If

    End Function
    'BoundFiledのDataFieldからIndexを取得する。※GridViewはBoundFieldだけで出来ているわけではないので、汎用性は下がる。
    'ただ、同じ列名でDataFieldが異なる場合などに使い勝手有り
    Public Shared Function getGridViewColumnIndexByDataField(ByRef gv As GridView, ByVal datafield As String) As Integer
        Dim index As Integer = 0
        Dim isExist As Boolean = False
        Dim dfCollection As DataControlFieldCollection = gv.Columns

        If Not dfCollection Is Nothing Then
            For Each colField As DataControlField In dfCollection
                If TypeOf colField Is BoundField Then
                    Dim bf As BoundField = CType(colField, BoundField)
                    If bf.DataField = datafield Then
                        isExist = True
                        Exit For
                    End If
                End If
                index += 1
            Next

            If isExist Then
                Return index
            Else
                Return -1
            End If
        Else
            Return -1
        End If
    End Function


    'GridViewへの行追加
    Public Shared Sub addRow(ByRef gv As GridView, ByRef dic As Dictionary(Of String, String), Optional ByVal addPosition As Integer = -1)
        Dim newRow As New GridViewRow(-1, -1, DataControlRowType.DataRow, DataControlRowState.Normal)

        If Not gv.HeaderRow Is Nothing Then
            For Each hcell As TableCell In gv.HeaderRow.Cells
                Dim cell As New TableCell()
                If dic.ContainsKey(hcell.Text) Then
                    cell.Text = dic(hcell.Text)
                End If
                newRow.Cells.Add(cell)
            Next

            If newRow.Cells.Count = gv.HeaderRow.Cells.Count Then
                If addPosition > -1 Then
                    gv.Controls(0).Controls.AddAt(addPosition, newRow)
                Else
                    gv.Controls(0).Controls.AddAt(gv.Rows.Count + 1, newRow)
                End If

            End If
        End If

    End Sub
    Public Shared Sub addRow(ByRef gFrom As GridView, ByRef gTo As DataTable, Optional ByVal rowList As List(Of Integer) = Nothing)
        Dim paramList As New List(Of String)
        Dim indexList As New List(Of Integer)

        If rowList Is Nothing Then
            indexList.Add(0)
        Else
            indexList = rowList
        End If

        If Not gFrom Is Nothing And Not gTo Is Nothing Then
            'データ作成
            For Each index As Integer In indexList
                Dim newRow As DataRow = gTo.NewRow

                For i As Integer = 0 To gTo.Columns.Count - 1
                    Dim hname As String = gTo.Columns.Item(i).ColumnName
                    For j As Integer = 0 To gFrom.Columns.Count - 1
                        If TypeOf gFrom.Columns.Item(j) Is BoundField Then
                            If CType(gFrom.Columns.Item(j), BoundField).DataField = hname Then
                                Try
                                    newRow(i) = System.Convert.ChangeType(HttpUtility.HtmlDecode(gFrom.Rows(index).Cells(j).Text), gTo.Columns.Item(i).DataType)
                                Catch ex As Exception
                                    'キャスト不可であれば処理しない
                                End Try
                            End If
                        End If
                    Next

                Next
                Try
                    gTo.Rows.Add(newRow)
                Catch ex As Exception
                    '例外が発生した場合追加しない(主にキー重複を想定)
                End Try

            Next

        End If


    End Sub
    Public Shared Function makeTransferData(ByRef gFrom As GridView, ByRef gTo As GridView, Optional ByVal row As Integer = 0) As Dictionary(Of String, String)
        Dim dic As New Dictionary(Of String, String)

        If Not gTo.HeaderRow Is Nothing Then
            For Each hcell In gTo.HeaderRow.Cells
                If Not getGridViewValue(gFrom, hcell.Text, row) Is Nothing Then
                    dic.Add(hcell.Text, getGridViewValue(gFrom, hcell.Text, row))
                End If
            Next

        End If

        Return dic

    End Function

    '各行処理
    Public Function fetchEachRow(ByVal fetch As fetchGrvRow) As String
        Dim result As String = ""
        For i As Integer = 0 To gvRef.Rows.Count - 1
            Dim rowInfo As New Dictionary(Of String, TableCell)
            For Each hcell As TableCell In gvRef.HeaderRow.Cells
                If Not rowInfo.ContainsKey(hcell.Text) Then
                    rowInfo.Add(hcell.Text, getGridViewCell(gvRef, hcell.Text, i))
                End If
            Next
            result += fetch(i, rowInfo) '各行を処理するデリゲートを受け取る

        Next

        Return result

    End Function

    'データバインド時の処理
    Private Sub gvRef_DataBound(ByVal sender As Object, ByVal e As System.EventArgs) Handles gvRef.DataBound
        setInvisibleStyle(sender, e)
        setGridViewStateLabel(gvStateLabel)
    End Sub
    '行にデータがバインドされた時の処理
    Private Sub gvRef_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles gvRef.RowDataBound
        fixUrlEncoding(sender, e)
        setInvalidStyle(sender, e)
    End Sub
    'ページング時処理(イベントが存在しないとエラーになる)
    Protected Sub gvRef_PageIndexChanged(sender As Object, e As System.EventArgs) Handles gvRef.PageIndexChanged
    End Sub
    '件数・ページングの状態を表示するラベルを作成
    Public Sub setGridViewStateLabel(ByRef label As Label)
        If label Is Nothing Then
            Exit Sub
        End If

        label.Text = ""

        Dim startPoint As Integer = gvRef.PageIndex * gvRef.PageSize + 1
        Dim endPoint As Integer = startPoint - 1 '実レコード数をプラスするため、ラベル数値的には-1
		
        If Not String.IsNullOrEmpty(gvRef.Attributes(gvRef.ClientID + RECORD_S_COUNT_ATTR)) Then
            endPoint = endPoint + Integer.Parse(gvRef.Attributes(gvRef.ClientID + RECORD_S_COUNT_ATTR))
        Else
            endPoint = endPoint + gvRef.Rows.Count
        End If

        If endPoint > 0 Then
            label.Text = startPoint.ToString + " 件目 ～ " + endPoint.ToString + " 件目を表示"
        End If

        If Not String.IsNullOrEmpty(gvRef.Attributes(gvRef.ClientID + RECORD_T_COUNT_ATTR)) Then
            label.Text = gvRef.Attributes(gvRef.ClientID + RECORD_T_COUNT_ATTR) + " 件中 " + startPoint.ToString + " 件目 ～ " + endPoint.ToString + " 件目を表示"
        End If

    End Sub
    Private Sub gvOdsDataSource_Selected(sender As Object, e As System.Web.UI.WebControls.ObjectDataSourceStatusEventArgs) Handles gvOdsDataSource.Selected
        If TypeOf e.ReturnValue Is Integer Then 'データセットとカウントの取得が行われる
            gvRef.Attributes.Add(gvRef.ClientID + RECORD_T_COUNT_ATTR, e.ReturnValue.ToString)
        ElseIf TypeOf e.ReturnValue Is DataTable Then
            gvRef.Attributes.Add(gvRef.ClientID + RECORD_S_COUNT_ATTR, CType(e.ReturnValue, DataTable).Rows.Count.ToString)
        End If
    End Sub

    'GridView内のハイパーリンクをURLエンコードする
    Protected Sub fixUrlEncoding(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs)
        If gvRef Is Nothing Or e.Row.RowType <> DataControlRowType.DataRow Then
            Exit Sub
        End If

        Dim index As Integer = 0
        For Each col As DataControlField In gvRef.Columns
            If TypeOf col Is HyperLinkField Then
                Dim linkField As HyperLinkField = CType(gvRef.Columns(index), HyperLinkField)
                Dim td As TableCell = e.Row.Cells(index)
                If td.Controls.Count > 0 Then
                    If TypeOf td.Controls(0) Is HyperLink Then
                        Dim link As HyperLink = CType(e.Row.Cells(index).Controls(0), HyperLink)

                        If Not String.IsNullOrEmpty(linkField.DataNavigateUrlFormatString) Then
                            Dim encodedFields(linkField.DataNavigateUrlFields.Length - 1) As String
                            For j As Integer = 0 To encodedFields.Length - 1
                                Dim obj As Object = DataBinder.Eval(e.Row.DataItem, linkField.DataNavigateUrlFields(j))
                                encodedFields(j) = HttpUtility.UrlEncode(obj.ToString)
                            Next
                            link.NavigateUrl = String.Format(linkField.DataNavigateUrlFormatString, encodedFields)
                        End If

                    End If

                End If


            End If
            index += 1
        Next
    End Sub
    Protected Sub setInvalidStyle(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs)
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim drv As DataRowView = CType(e.Row.DataItem, DataRowView)
            Dim dr As DataRow = CType(drv.Row, DataRow)

            Dim gv As GridView = CType(sender, GridView)
            Dim isInvalid As Boolean = False


            For Each item As KeyValuePair(Of String, String) In invalidRecords

                If (item.Value Is Nothing And IsDBNull(dr(item.Key))) Or _
                   (Not item.Value Is Nothing And Not IsDBNull(dr(item.Key))) AndAlso item.Value = dr(item.Key) Then
                    For idx As Integer = 0 To e.Row.Cells.Count - 1
                        Dim cell As TableCell = e.Row.Cells(idx)

                        'ヘッダーテキストが非表示対象の場合は、CSSを非表示に設定し、そうでない場合は無効に設定する。
                        If gv.HeaderRow IsNot Nothing AndAlso hideColumns.Contains(gv.HeaderRow.Cells(idx).Text) Then
                            cell.CssClass = _hideCss
                        Else
                            cell.CssClass = _invalidRecordCss
                        End If
                    Next
                End If
            Next

        End If
    End Sub
    Protected Sub setInvisibleStyle(ByVal sender As Object, ByVal e As System.EventArgs)
        For Each target As String In hideColumns
            Dim colIndex As Integer = getGridViewColumnIndex(gvRef, target)
            If colIndex > -1 Then
                '列をＣＳＳで非表示にする。
                gvRef.HeaderRow.Cells(colIndex).CssClass = _hideCss
                gvRef.Columns(colIndex).ItemStyle.CssClass = _hideCss
				If gvRef.FooterRow IsNot Nothing Then
                    gvRef.FooterRow.Cells(colIndex).CssClass = _hideCss
                End If
                'If gvRef.Columns(colIndex).Visible Then
                '    gvRef.Columns(colIndex).Visible = False
                'End If
            End If
        Next
    End Sub

End Class
