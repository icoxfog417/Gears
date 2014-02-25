Imports Microsoft.VisualBasic
Imports System.Collections.Generic

Namespace Gears

    'Sqlbuilderと役割が一部重複するため、クラス設計要件等
	<Serializable()>
    Public Class GearsDTO

        '発生したイベントタイプ
        Private atype As ActionType = ActionType.NONE

        'コントロール情報
        Private controlInfo As Dictionary(Of String, List(Of GearsControlInfo)) = New Dictionary(Of String, List(Of GearsControlInfo))

        'デフォルトセット情報
        'SQLの選択項目
        Private selection As List(Of SqlSelectItem) = New List(Of SqlSelectItem)
        'SQLの条件項目
        Private filter As List(Of SqlFilterItem) = New List(Of SqlFilterItem)
        'ロック情報
        Private lockitem As List(Of SqlFilterItem) = New List(Of SqlFilterItem)
        'updateによる他キー項目の更新について、チェックを行うか否か(ホントはFalseがデフォルトであるべき？)
        Private _isPermitOtherKeyUpdate As String = True
        Public Property IsPermitOtherKeyUpdate() As String
            Get
                Return _isPermitOtherKeyUpdate
            End Get
            Set(ByVal value As String)
                _isPermitOtherKeyUpdate = value
            End Set
        End Property

        '警告が発生した場合無視するか
        Private _isIgnoreAlert As Boolean = False
        Public Property IsIgnoreAlert() As Boolean
            Get
                Return _isIgnoreAlert
            End Get
            Set(ByVal value As Boolean)
                _isIgnoreAlert = value
            End Set
        End Property

        'データベース実行のタイムアウト設定
        Private _commandTimeout As Integer = -1
        Public Property CommandTimeout() As Integer
            Get
                Return _commandTimeout
            End Get
            Set(ByVal value As Integer)
                _commandTimeout = value
            End Set
        End Property


        'リミット情報
        Private maximumRows As Integer = -1
        Private startRowIndex As Integer = -1

        'その他付加情報
        Private attrInfo As New Dictionary(Of String, String)

        'コンストラクタ
        Public Sub New(ByVal atype As ActionType)
            Me.atype = atype
        End Sub
        Public Sub New(ByRef gto As GearsDTO)
            If Not gto Is Nothing Then
                atype = gto.getAtype
                controlInfo = New Dictionary(Of String, List(Of GearsControlInfo))(gto.getControlInfo)
                selection = New List(Of SqlSelectItem)(gto.getSelection)
                filter = New List(Of SqlFilterItem)(gto.getFilter)
                lockitem = New List(Of SqlFilterItem)(gto.getLockItem)
                isPermitOtherKeyUpdate = gto.IsPermitOtherKeyUpdate
                IsIgnoreAlert = gto.IsIgnoreAlert
                CommandTimeout = gto.CommandTimeout
                maximumRows = gto.getMaximumRows
                startRowIndex = gto.getStartRowIndex
                attrInfo = New Dictionary(Of String, String)(gto.getAttrInfo)
            Else
                atype = ActionType.SEL 'デフォルト値
            End If

        End Sub

        'getter/setter
        Public Function getAtype() As ActionType
            Return atype
        End Function
        Public Sub setAtype(ByVal atype As ActionType)
            Me.atype = atype
        End Sub
        Public Function getControlInfo() As Dictionary(Of String, List(Of GearsControlInfo))
            Return controlInfo
        End Function
        Public Function getControlInfo(ByVal id As String) As List(Of GearsControlInfo)
            If (controlInfo.ContainsKey(id)) Then
                Return controlInfo(id)
            Else
                Return Nothing
            End If
        End Function
        Public Sub addControlInfo(ByRef conf As GearsControlInfo)
            If (controlInfo.ContainsKey(conf.ControlID)) Then
                controlInfo(conf.ControlID).Add(conf)
            Else
                Dim list As List(Of GearsControlInfo) = New List(Of GearsControlInfo)
                list.Add(conf)
                controlInfo.Add(conf.ControlID, list)
            End If
        End Sub
        Public Sub addControlInfo(ByRef con As GearsControl)
            Dim list As List(Of GearsControlInfo) = con.createControlInfo
            For Each conf As GearsControlInfo In list
                addControlInfo(conf)
            Next
        End Sub
        'Public Sub addControlInfo(ByVal id As String, ByRef conf As GearsControlInfo)
        '    If (controlInfo.ContainsKey(id)) Then
        '        controlInfo(id).Add(conf)
        '    Else
        '        Dim list As List(Of GearsControlInfo) = New List(Of GearsControlInfo)
        '        list.Add(conf)
        '        controlInfo.Add(id, list)
        '    End If
        'End Sub
        Public Sub removeControlInfo(ByVal id As String)
            If (controlInfo.ContainsKey(id)) Then
                controlInfo.Remove(id)
            End If
        End Sub

        Public Function getAttrInfo() As Dictionary(Of String, String)
            Return attrInfo
        End Function
        Public Function getAttrInfo(ByVal id As String) As String
            If attrInfo.ContainsKey(id) Then
                Return attrInfo(id)
            Else
                Return ""
            End If
        End Function
        Public Sub addAttrInfo(ByVal id As String, ByVal value As String)
            If attrInfo.ContainsKey(id) Then
                attrInfo(id) = value
            Else
                attrInfo.Add(id, value)
            End If
        End Sub
        Public Sub removeAttrInfo(ByVal id As String)
            If attrInfo.ContainsKey(id) Then
                attrInfo.Remove(id)
            End If
        End Sub

        Public Function getSelection() As List(Of SqlSelectItem)
            Return selection
        End Function
        Public Sub addSelection(ByVal item As SqlSelectItem)
            selection.Add(item)
        End Sub
        Public Function getFilter() As List(Of SqlFilterItem)
            Return filter
        End Function
        Public Sub addFilter(ByVal item As SqlFilterItem)
            filter.Add(item)
        End Sub
        Public Function getLockItem() As List(Of SqlFilterItem)
            Return lockitem
        End Function
        Public Function getLockItemList() As Dictionary(Of String, String)
            Dim result As New Dictionary(Of String, String)
            For Each item As SqlFilterItem In lockitem
                result.Add(item.Column, item.Value)
            Next
            Return result

        End Function
        Public Sub addLockItem(ByVal item As SqlFilterItem)
            lockitem.Add(item)
        End Sub
        Public Sub addLockItem(ByRef items As Dictionary(Of String, String))
            For Each item As KeyValuePair(Of String, String) In items
                lockitem.Add(SqlBuilder.newFilter(item.Key).eq(item.Value))
            Next
        End Sub
        Public Sub removeLockItem()
            lockitem.Clear()
        End Sub
        'Public Sub setPermitOtherKeyUpdate(isok As Boolean)
        '    isPermitOtherKeyUpdate = isok
        'End Sub
        'Public Function getPermitOtherKeyUpdate() As Boolean
        '    Return isPermitOtherKeyUpdate
        'End Function
        Public Function isSetLimit() As Boolean
            If maximumRows > -1 Or startRowIndex > -1 Then
                Return True
            Else
                Return False
            End If
        End Function
        Public Function getMaximumRows() As Integer
            Return maximumRows
        End Function
        Public Function getStartRowIndex() As Integer
            Return startRowIndex
        End Function
        Public Sub setLimit(startIndex As Integer, rowCount As Integer)
            startRowIndex = startIndex
            maximumRows = rowCount
        End Sub


        'SqlBuilder変換
        Public Function generateSqlBuilder() As SqlBuilder
            Dim sqlb As SqlBuilder = New SqlBuilder

            'アクションタイプの引継ぎ
            sqlb.setPredictiveType(atype)

            'プロパティの引継ぎ
            sqlb.IgnoreAlert = IsIgnoreAlert
            sqlb.CommandTimeout = CommandTimeout
            If isSetLimit() Then
                sqlb.limit(startRowIndex, maximumRows)
            End If

            'セレクタ/フィルタの設定
            For Each item As KeyValuePair(Of String, List(Of GearsControlInfo)) In controlInfo
                For Each ginfo As GearsControlInfo In item.Value
                    If ginfo.IsFormAttribute Then
                        If ginfo.IsKey Then
                            sqlb.addSelection(SqlBuilder.newSelect(ginfo.DataSourceID).setValue(ginfo.Value).key)
                            'sqlb.addFilter(SqlBuilder.newFilter(ginfo.getDataSourceId).eq(ginfo.getValue).key)
                            If ginfo.PastValue <> "" Then
                                sqlb.addFilter(SqlBuilder.newFilter(ginfo.DataSourceID).eq(ginfo.PastValue).key) '前回ロード時の値をフィルタ値として設定
                            Else
                                sqlb.addFilter(SqlBuilder.newFilter(ginfo.DataSourceID).eq(ginfo.Value).key) '前回ロード時の値がない場合、現在値をフィルタ値として設定
                            End If
                        Else
                            sqlb.addSelection(SqlBuilder.newSelect(ginfo.DataSourceID).setValue(ginfo.Value))
                        End If

                    Else 'フィルタパネル、もしくは何も設定がない場合はフィルタ値として判断する
                        If ginfo.IsKey Then
                            sqlb.addFilter(SqlBuilder.newFilter(ginfo.DataSourceID).eq(ginfo.Value).key)
                        Else
                            sqlb.addFilter(SqlBuilder.newFilter(ginfo.DataSourceID).eq(ginfo.Value))
                        End If
                    End If

                    'オペレーターの設定
                    If Not String.IsNullOrEmpty(ginfo.OperatorAttribute()) Then
                        Dim filter As SqlFilterItem = sqlb.getFilter(ginfo.DataSourceID, 0)
                        If Not filter Is Nothing AndAlso filter.Value <> "" Then
                            Select Case ginfo.OperatorAttribute()
                                Case SqlFilterItem.TXT_NEQUAL
                                    filter.neq(filter.Value)
                                Case SqlFilterItem.TXT_GT
                                    filter.gt(filter.Value)
                                Case SqlFilterItem.TXT_GTEQ
                                    filter.gteq(filter.Value)
                                Case SqlFilterItem.TXT_LT
                                    filter.lt(filter.Value)
                                Case SqlFilterItem.TXT_LTEQ
                                    filter.lteq(filter.Value)
                                Case SqlFilterItem.TXT_LIKE
                                    filter.likes("%" + filter.Value + "%")
                                Case SqlFilterItem.TXT_START_WITH
                                    filter.likes(filter.Value + "%")
                                Case SqlFilterItem.TXT_END_WITH
                                    filter.likes("%" + filter.Value)
                            End Select

                        End If
                    End If

                Next
            Next

            For Each item In selection
                If atype <> ActionType.SEL And (Not sqlb.getSelection(item.Column, 0) Is Nothing And item.IsKey) Then  'select以外でキーが複数存在することは出来ない
                    sqlb.changeSelection(New SqlSelectItem(item))
                Else
                    sqlb.addSelection(New SqlSelectItem(item))
                End If
            Next
            For Each item In filter
                If atype <> ActionType.SEL And (Not sqlb.getFilter(item.Column, 0) Is Nothing And item.IsKey) Then  'select以外でキーが複数存在することは出来ない
                    sqlb.changeFilter(New SqlFilterItem(item))
                Else
                    sqlb.addFilter(New SqlFilterItem(item))
                End If

            Next

            Return sqlb

        End Function

        Public Shared Function getAtypeString(ByRef dto As GearsDTO) As String
            If Not dto Is Nothing Then
                Return getAtypeString(dto.getAtype)
            Else
                Return " (dto Nothing) "
            End If

        End Function
        Public Shared Function getAtypeString(ByVal atype As ActionType) As String
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

        Public Overrides Function toString() As String
            Dim str As String = ""
            Dim sql As SqlBuilder = generateSqlBuilder()
            sql.setDataSource(SqlBuilder.newDataSource("dummy"))
            str += "更新タイプ：" + getAtypeString(atype) + vbCrLf
            If atype <> ActionType.SAVE Then
                str += "予想実行SQL(更新対象はdummyで表示)：" + vbCrLf + sql.confirmSql(atype)
            Else
                str += "予想実行SQL(更新対象はdummyで表示)：" + vbCrLf + sql.confirmSql(ActionType.UPD) + vbCrLf + sql.confirmSql(ActionType.INS)
            End If
            Return str

        End Function
        Public Function confirmSql(Optional ByVal ds As SqlDataSource = Nothing) As String
            Dim str As String = ""
            Dim sql As SqlBuilder = generateSqlBuilder()

            If ds Is Nothing Then
                sql.setDataSource(SqlBuilder.newDataSource("dummy"))
            Else
                sql.setDataSource(ds)
            End If

            Return sql.confirmSql(atype)

        End Function

    End Class

End Namespace
