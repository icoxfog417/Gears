﻿Imports Gears.Util


Namespace Gears.DataSource

    ''' <summary>
    ''' SQL実行に必要なパラメータを管理するコンテナ
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    Public Class SqlItemContainer

        ''' <summary>イベントタイプ</summary>
        Public Property Action As ActionType = ActionType.NONE

        ''' <summary>SQLの選択項目</summary>
        Private _selection As New List(Of SqlSelectItem)

        ''' <summary>SQLの選択項目</summary>
        Public Function Selection() As List(Of SqlSelectItem)
            Return _selection
        End Function

        ''' <summary>指定したSQLの選択項目を取得する</summary>
        Public Function Selection(ByVal column As String) As SqlSelectItem
            Return _selection.Where(Function(x) x.Column = column).FirstOrDefault
        End Function

        ''' <summary>指定したSQLの選択項目を取得する</summary>
        Public Function Selection(ByVal index As Integer) As SqlSelectItem
            Return _selection(index)
        End Function

        ''' <summary>SQLの条件項目</summary>
        Private _filter As New List(Of SqlFilterItem)
        ''' <summary>SQLの条件項目</summary>
        Public Function Filter() As List(Of SqlFilterItem)
            Return _filter
        End Function
        ''' <summary>指定したSQLの条件項目を取得する</summary>
        Public Function Filter(ByVal column As String) As SqlFilterItem
            Return _filter.Where(Function(x) x.Column = column).FirstOrDefault
        End Function
        ''' <summary>指定したSQLの条件項目を取得する</summary>
        Public Function Filter(ByVal index As Integer) As SqlFilterItem
            Return _filter(index)
        End Function

        ''' <summary>Updateでキーの更新を許可するか否か</summary>
        Public Property IsPermitOtherKeyUpdate() As Boolean = True

        ''' <summary>警告が発生した場合無視するか否か</summary>
        Public Property IsIgnoreAlert() As Boolean = False

        ''' <summary>マルチバイト対応の要否</summary>
        Public Property IsMultiByte() As Boolean = False

        ''' <summary>データベース実行のタイムアウト設定</summary>
        Public Property CommandTimeout() As Integer = -1

        ''' <summary>ページング対応:最大行数</summary>
        Private _rowsInPage As Integer = -1

        ''' <summary>ページング対応:開始位置</summary>
        Private _rowIndexOfPage As Integer = -1

        ''' <summary>その他付加情報</summary>
        Private _attrInfo As New Dictionary(Of String, String)

        ''' <summary>その他属性情報の取得</summary>
        Public Function AttrInfo() As Dictionary(Of String, String)
            Return _attrInfo
        End Function

        ''' <summary>その他属性情報の取得(ID指定)</summary>
        Public Function AttrInfo(ByVal id As String) As String
            If _attrInfo.ContainsKey(id) Then
                Return _attrInfo(id)
            Else
                Return ""
            End If
        End Function

        Public Sub New()
        End Sub

        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="atype"></param>
        Public Sub New(ByVal atype As ActionType)
            Me._Action = atype
        End Sub

        ''' <summary>
        ''' コピーコンストラクタ
        ''' </summary>
        ''' <param name="scon"></param>
        ''' <remarks></remarks>
        Public Sub New(ByRef scon As SqlItemContainer, Optional ByVal withSelectionAndFilter As Boolean = True)
            If Not scon Is Nothing Then
                _Action = scon.Action
                If withSelectionAndFilter Then
                    _selection = New List(Of SqlSelectItem)(scon.Selection)
                    _filter = New List(Of SqlFilterItem)(scon.Filter)
                End If
                IsPermitOtherKeyUpdate = scon.IsPermitOtherKeyUpdate
                IsIgnoreAlert = scon.IsIgnoreAlert
                IsMultiByte = scon.IsMultiByte
                CommandTimeout = scon.CommandTimeout
                _rowsInPage = scon.RowsInPage
                _rowIndexOfPage = scon.RowIndexOfPage
                _attrInfo = New Dictionary(Of String, String)(scon._attrInfo)
            Else
                Action = ActionType.SEL 'デフォルト値
            End If

        End Sub

        ''' <summary>その他属性情報の追加</summary>
        Public Sub addAttrInfo(ByVal id As String, ByVal value As String)
            If _attrInfo.ContainsKey(id) Then
                _attrInfo(id) = value
            Else
                _attrInfo.Add(id, value)
            End If
        End Sub

        ''' <summary>その他属性情報の削除</summary>
        Public Sub removeAttrInfo(ByVal id As String)
            If _attrInfo.ContainsKey(id) Then
                _attrInfo.Remove(id)
            End If
        End Sub

        ''' <summary>SqlItemを追加する汎用処理</summary>
        Public Sub Add(ByVal ParamArray items As SqlSelectItem())
            addSelection(items.ToList)
        End Sub

        ''' <summary>SqlItemを追加する汎用処理</summary>
        Public Sub Add(ByVal ParamArray items As SqlFilterItem())
            addFilter(items.ToList)
        End Sub

        ''' <summary>SqlItemを追加する汎用処理</summary>
        Public Sub Add(ByVal items As List(Of SqlSelectItem))
            addSelection(items)
        End Sub

        ''' <summary>SqlItemを追加する汎用処理</summary>
        Public Sub Add(ByVal items As List(Of SqlFilterItem))
            addFilter(items)
        End Sub

        ''' <summary>選択情報の追加</summary>
        Public Sub addSelection(ByVal ParamArray items As SqlSelectItem())
            addSelection(items.ToList)
        End Sub

        ''' <summary>選択情報の追加(リスト)</summary>
        Public Sub addSelection(ByVal items As List(Of SqlSelectItem))
            items.ForEach(Sub(s) _selection.Add(s))
        End Sub

        ''' <summary>コントロールによる選択情報の追加(任意引数)</summary>
        Public Sub addSelection(ByVal ParamArray cons As Control())
            addSelection(cons.ToList)
        End Sub

        ''' <summary>コントロールによる選択情報の追加(リスト)</summary>
        Public Sub addSelection(ByVal cons As List(Of Control))
            For Each con As Control In cons
                addSelection(con.toGControl().toControlInfo().Select(Function(c) c.toSelection).ToList)
            Next
        End Sub

        ''' <summary>選択情報の削除</summary>
        Public Sub removeSelection(ByVal column As String)
            _selection.RemoveAll(Function(s) s.Column = column)
        End Sub

        ''' <summary>条件情報の追加</summary>
        Public Sub addFilter(ByVal ParamArray items As SqlFilterItem())
            addFilter(items.ToList)
        End Sub

        ''' <summary>条件情報の追加(リスト)</summary>
        Public Sub addFilter(ByVal items As List(Of SqlFilterItem))
            items.ForEach(Sub(f) _filter.Add(f))
        End Sub

        ''' <summary>コントロールによる条件情報の追加(任意引数)</summary>
        Public Sub addFilter(ByVal ParamArray cons As Control())
            addFilter(cons.ToList)
        End Sub

        ''' <summary>コントロールによる条件情報の追加(リスト)</summary>
        Public Sub addFilter(ByVal cons As List(Of Control))
            For Each con As Control In cons
                addFilter(con.toGControl().toControlInfo().Select(Function(c) c.toFilter).ToList)
            Next
        End Sub

        ''' <summary>条件情報の削除</summary>
        Public Sub removeFilter(ByVal column As String)
            _filter.RemoveAll(Function(s) s.Column = column)
        End Sub

        ''' <summary>ページング用項目が設定されているか否か</summary>
        Public Function isPaging() As Boolean
            If _rowsInPage > -1 Or _rowIndexOfPage > -1 Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>行数の取得</summary>
        Public Function RowsInPage() As Integer
            Return _rowsInPage
        End Function

        ''' <summary>開始行の取得</summary>
        Public Function RowIndexOfPage() As Integer
            Return _rowIndexOfPage
        End Function

        ''' <summary>開始行の取得</summary>
        Public Sub setPaging(ByVal index As Integer, ByVal size As Integer)
            _rowIndexOfPage = index
            _rowsInPage = size
        End Sub

        ''' <summary>Actionのテキストを取得する</summary>
        Public Shared Function ActionToString(ByRef dto As SqlItemContainer) As String
            If Not dto Is Nothing Then
                Return ActionToString(dto.Action)
            Else
                Return " (dto Nothing) "
            End If

        End Function

        ''' <summary>Actionのテキストを取得する</summary>
        Public Shared Function ActionToString(ByVal atype As ActionType) As String
            Dim str As String = ""
            Select Case atype
                Case ActionType.SEL
                    str = "SELECT"
                Case ActionType.INS
                    str = "INSERT"
                Case ActionType.UPD
                    str = "UPDATE"
                Case ActionType.DEL
                    str = "DELETE"
                Case ActionType.SAVE
                    str = "SAVE"
                Case Else
                    str = " - "
            End Select

            Return str

        End Function

    End Class

End Namespace

