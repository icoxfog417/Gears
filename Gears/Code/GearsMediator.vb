Imports Microsoft.VisualBasic
Imports System.Collections.Generic

Namespace Gears

    Public Class GearsMediator

        Public Const GCON_TARGET As String = "GCON"
        Public Const DS_NAMESPACE As String = "DSNAMESPACE"
        Public Const DS_CONNECTION_NAME As String = "DSCONNECTIONNAME"
        Public Const RELATION_STORE_KEY As String = "RELATION_STORE_KEY"
        Public Const PARENT_CONTROL_KEY As String = "+PARENT_CONTROL_KEY+" 'controlとして設定不可能な文字列
        Public Const CONTROL_ID_SEPARATOR As String = "/"

        'デリゲート処理定義
        Public Delegate Sub fetchControl(ByRef control As Control, ByRef dto As GearsDTO)
        Public Delegate Function isFetchTgt(ByRef control As Control) As Boolean

        'プロパティ
        Private _dsNameSpace As String = ""
        Public Property DsNameSpace() As String
            Get
                Return _dsNameSpace
            End Get
            Set(ByVal value As String)
                _dsNameSpace = value
            End Set
        End Property
        Private _connectionName As String = ""
        Public Property ConnectionName() As String
            Get
                Return _connectionName
            End Get
            Set(ByVal value As String)
                _connectionName = value
            End Set
        End Property

        Private _gcontrols As New Dictionary(Of String, GearsControl)
        Public ReadOnly Property GControls() As Dictionary(Of String, GearsControl)
            Get
                Return _gcontrols
            End Get
        End Property
        Public Function GControl(ByVal id As String) As GearsControl
            If Not id Is Nothing AndAlso _gcontrols.ContainsKey(id) Then
                Return _gcontrols(id)
            Else
                Return Nothing
            End If
        End Function

        Private _relations As New Dictionary(Of String, List(Of String))
        Private _sendEscapes As New Dictionary(Of String, List(Of String))
        Private _receiveEscapes As New Dictionary(Of String, List(Of String))

        Private log As New Dictionary(Of String, GearsException)


        Public Sub New(ByVal con As String, Optional ByVal dsn As String = "")
            _connectionName = con
            _dsNameSpace = dsn
        End Sub

        'コントロールの探索
        '各コントロールに対する処理、及び対処か否かの判定処理を引数にして処理を行うメソッド
        '判定処理が単純な場合、また判定後のキャストなどがある場合処理が重複するので構成要検討(対象か否かの判定が"横断的関心"として有用ならこれもありか))
        Public Sub fetchControls(ByRef parent As Control, ByVal callback As fetchControl, ByVal isFetch As isFetchTgt, Optional ByRef dto As GearsDTO = Nothing)

            If parent.HasControls() Then
                Dim child As Control
                For Each child In parent.Controls()
                    If isFetch Is Nothing OrElse isFetch(child) Then '対象と判定されたもののみ、Fetchする(ない場合All True)
                        callback(child, dto)
                    End If
                    fetchControls(child, callback, isFetch, dto)
                Next
            End If

        End Sub

        Public Function addGControls(ByRef parent As Control, Optional isAutoLoadAttr As Boolean = True) As Boolean
            Dim result As Boolean = True
            For Each gcon As GearsControl In createGControls(parent, isAutoLoadAttr)
                If result And Not addGControl(gcon) Then
                    result = False
                End If
            Next

            Return result

        End Function
        Public Function addGControl(ByRef con As Control, Optional isAutoLoadAttr As Boolean = True) As Boolean

            Return addGControl(createGControl(con, isAutoLoadAttr))

        End Function
        Public Function addGControl(ByRef gcon As GearsControl) As Boolean
            If Not containsKeySafe(_gcontrols, gcon.ControlID) Then
                _gcontrols.Add(gcon.ControlID, gcon)
                Return True
            Else
                GearsLogStack.setLog(gcon.ControlID + " はすでにGearsControlとして追加されています")
                Return False
            End If
        End Function
        Public Function createGControl(ByRef con As Control, Optional isAutoLoadAttr As Boolean = True) As GearsControl

            Dim cn = ConnectionName
            Dim ds = DsNameSpace
            Dim addedControl As New List(Of String)

            If TypeOf con Is WebControl Then 'コントロールに名称空間/データソース接続の直接指定があればそちらを優先
                Dim wcon As WebControl = CType(con, WebControl)
                If Not String.IsNullOrEmpty(wcon.Attributes(DS_CONNECTION_NAME)) Then
                    cn = wcon.Attributes(DS_CONNECTION_NAME)
                End If
                If Not String.IsNullOrEmpty(wcon.Attributes(DS_NAMESPACE)) Then
                    ds = wcon.Attributes(DS_NAMESPACE)
                End If
            End If

            Dim gcon As GearsControl = New GearsControl(con, cn, ds, isAutoLoadAttr)

            Return gcon

        End Function
        Public Function createGControls(ByRef con As Control, Optional isAutoLoadAttr As Boolean = True) As List(Of GearsControl)
            Dim gcons As New List(Of GearsControl)

            fetchControls(con, Sub(ByRef control As Control, ByRef dto As GearsDTO)
                                   gcons.Add(createGControl(control, isAutoLoadAttr))
                               End Sub,
                      AddressOf Me.isTargetControl)

            Return gcons
        End Function
        Public Sub addRelation(ByRef conF As Control, ByRef conT As Control)
            Dim templateString As String = "{0} はまだフレームワークに登録されていません。addRelationを行う前に、registerMyControlを使用し、コントロールの登録を行ってください"
            If GControl(conF.ID) Is Nothing Then
                Throw New GearsException(String.Format(templateString, conF.ID))
            ElseIf GControl(conT.ID) Is Nothing Then
                Throw New GearsException(String.Format(templateString, conT.ID))
            Else
                If Not _relations.ContainsKey(conF.ID) Then
                    Dim newCon As New List(Of String)
                    newCon.Add(conT.ID)
                    _relations.Add(conF.ID, newCon)
                Else
                    _relations(conF.ID).Add(conT.ID)
                End If
            End If

        End Sub
        Public Sub addRelation(ByVal conF As String, ByVal conT As String)

            If Not GControl(conF) Is Nothing And Not GControl(conT) Is Nothing Then
                addRelation(GControl(conF).Control, GControl(conT).Control)
            End If

        End Sub

        Public Sub setEscapesWhenSend(ByRef fromControl As Control, ByRef toControl As Control, ByVal ParamArray escapes() As String)
            setEscapes(fromControl, toControl, escapes, isSend:=True)
        End Sub
        Public Sub setEscapesWhenReceive(ByRef fromControl As Control, ByRef toControl As Control, ByVal ParamArray escapes() As String)
            setEscapes(fromControl, toControl, escapes, isSend:=False)
        End Sub

        Public Sub resetEscapesWhenSend(ByRef fromControl As Control, ByRef toControl As Control)
            resetEscape(fromControl, toControl, isSend:=True)
        End Sub
        Public Sub resetEscapesWhenReceive(ByRef fromControl As Control, ByRef toControl As Control)
            resetEscape(fromControl, toControl, isSend:=False)
        End Sub
        Private Sub setEscapes(ByRef fromControl As Control, ByRef toControl As Control, ByVal escapes As String(), ByVal isSend As Boolean)
            Dim key As String = makeFromToKey(fromControl, toControl)
            Dim list As Dictionary(Of String, List(Of String))

            If isSend Then
                list = _sendEscapes
            Else
                list = _receiveEscapes
            End If

            Dim escapelist As New List(Of String)
            Dim isExist As Boolean = False

            If list.ContainsKey(key) Then
                escapelist = list(key)
                isExist = True
            End If

            For Each target As String In escapes
                escapelist.Add(target)
            Next

            If Not isExist Then
                list.Add(key, escapelist)
            End If

        End Sub
        Private Sub resetEscape(ByRef fromControl As Control, ByRef toControl As Control, ByVal isSend As Boolean)
            Dim key As String = makeFromToKey(fromControl, toControl)
            Dim list As Dictionary(Of String, List(Of String))

            If isSend Then
                list = _sendEscapes
            Else
                list = _receiveEscapes
            End If

            If list.ContainsKey(key) Then
                list.Remove(key)
            End If

        End Sub


        Public Function extractRelation(ByVal conid As String) As List(Of GearsControl)
            If _relations.ContainsKey(conid) Then
                Dim gconlist As New List(Of GearsControl)

                For Each id As String In _relations(conid)
                    If _gcontrols.ContainsKey(id) Then
                        gconlist.Add(_gcontrols(id))
                    End If
                Next

                Return gconlist
            Else
                Return Nothing
            End If
        End Function

        Private Function extractRootsInArea(ByVal controlArea As Control) As List(Of String)
            Dim hasParent As Boolean = True
            Dim inArea As Boolean = True
            Dim visitorControl As String = ""

            Dim controlsInArea As List(Of String) = getControlsInArea(controlArea)
            Dim rootControl As New List(Of String)

            For Each conId As String In controlsInArea

                Dim isRoot As Boolean = isRootNode(conId, controlsInArea)
                If isRoot Then
                    rootControl.Add(conId)
                End If

            Next

            Return rootControl

        End Function
        Private Function isRootNode(ByVal conId As String, Optional ByVal controlArea As List(Of String) = Nothing) As Boolean
            Dim hasParent As Boolean = False

            '指定範囲内に親(ただし同名でない)が存在する場合、ROOTでない
            For Each relation As KeyValuePair(Of String, List(Of String)) In _relations
                If relation.Value.Contains(conId) And relation.Key <> conId Then '指定コントロール子とするリレーションが存在する
                    If controlArea Is Nothing OrElse _
                        (Not controlArea Is Nothing And controlArea.Contains(relation.Key)) Then 'リレーションの親が指定範囲内に存在する
                        hasParent = True
                        Exit For
                    End If
                End If
            Next

            Return Not hasParent

        End Function

        Private Function getControlsInArea(ByVal controlArea As Control) As List(Of String)

            Dim controlInArea As New List(Of String)
            fetchControls(controlArea, Sub(ByRef control As Control, ByRef dto As GearsDTO)
                                           controlInArea.Add(control.ID)
                                       End Sub,
                                        Function(ByRef control As Control) As Boolean
                                            'コントロールが範囲内にあり、登録済み
                                            If Not control.ID Is Nothing AndAlso isRegisteredControl(control) AndAlso Not GFindControl(controlArea, control.ID) Is Nothing Then
                                                Return True
                                            Else
                                                Return False
                                            End If
                                        End Function)
            Return controlInArea

        End Function
        Public Function GFindControl(ByRef con As Control, ByVal conid As String) As Control
            Dim findout As Control = Nothing
            fetchControls(con,
                           Sub(ByRef control As Control, ByRef dto As GearsDTO)
                               findout = control
                           End Sub,
                           Function(ByRef control As Control) As Boolean
                               Return control.ID = conid
                           End Function)
            Return findout
        End Function


        Public Function makeSendMessage(ByRef fromControl As Control, ByRef toControl As Control, ByRef fromDto As GearsDTO) As GearsDTO

            If isRegisteredControl(fromControl) Then
                Dim fromToKey As String = makeFromToKey(fromControl, toControl)

                Dim message As GearsDTO = New GearsDTO(fromDto)
                message.addAttrInfo(RELATION_STORE_KEY, fromToKey)

                Dim parentInfo As New GearsControlInfo(PARENT_CONTROL_KEY, PARENT_CONTROL_KEY, "")
                parentInfo.IsFormAttribute = GControl(fromControl.ID).IsFormAttribute
                parentInfo.IsFilterAttribute = GControl(fromControl.ID).IsFilterAttribute
                message.addControlInfo(parentInfo)

                If TypeOf fromControl Is GridView Then 'サポート対象の特殊コントロール
                    fetchControlValue(fromControl, message)
                ElseIf fromControl.HasControls Then 'その他複合コントロール
                    fetchControls(fromControl, AddressOf Me.fetchControlValue, AddressOf Me.isRegisteredAsTarget, message)
                ElseIf isRegisteredAsTarget(fromControl) Then
                    fetchControlValue(fromControl, message)
                End If

                '開放
                message.removeControlInfo(PARENT_CONTROL_KEY)
                message.removeAttrInfo(RELATION_STORE_KEY)

                Return message
            Else
                Return Nothing
            End If
        End Function
        Private Sub fetchControlValue(ByRef control As Control, ByRef dto As GearsDTO)
            '除外対象のものは収集しない
            Dim isEscape As Boolean = False
            If Not dto.AttrInfo(RELATION_STORE_KEY) Is Nothing AndAlso _sendEscapes.ContainsKey(dto.AttrInfo(RELATION_STORE_KEY)) Then
                Dim escapeList As List(Of String) = _sendEscapes(dto.AttrInfo(RELATION_STORE_KEY))

                Dim now As Control = control
                While Not now Is Nothing '自身のルートがescape対象でないか確認する。(なお、いつかは親がNothingになるはず)
                    If escapeList.Contains(now.ID) Then
                        isEscape = True
                        Exit While
                    ElseIf TypeOf now Is Page Then 'ページコントロールは画面に1つしか存在しないため、Pageに達したら抜ける(無限ループの保険)
                        Exit While
                    End If
                    now = now.Parent
                End While

            End If

            If Not isEscape Then
                Dim conInfos As List(Of GearsControlInfo) = GControl(control.ID).createControlInfo
                If Not conInfos Is Nothing Then
                    For Each cf As GearsControlInfo In conInfos
                        If Not dto.ControlInfo(PARENT_CONTROL_KEY) Is Nothing Then
                            If dto.ControlInfo(PARENT_CONTROL_KEY).Item(0).IsFormAttribute Then
                                cf.IsFormAttribute = True
                            ElseIf dto.ControlInfo(PARENT_CONTROL_KEY).Item(0).IsFilterAttribute Then
                                cf.IsFilterAttribute = True
                            End If
                        End If
                        dto.addControlInfo(cf)
                    Next
                End If
            Else
                GearsLogStack.setLog(control.ID + " は除外対象として登録されているため、送信対象に含まれません")
            End If

        End Sub

        Public Function executeBehavior(ByRef fromControl As Control, ByRef toControl As Control, ByRef gto As GearsDTO, Optional ByVal isGatherInfo As Boolean = True) As Boolean
            Dim result As Boolean = True

            log.Clear()
            If _relations.ContainsKey(fromControl.ID) Then
                For Each conKey As String In _relations(fromControl.ID)
                    '対象コントロールの場合のみ処理
                    If Not toControl Is Nothing AndAlso conKey <> toControl.ID Then
                        Continue For
                    End If

                    '影響を及ぼす相手先の各コントロール(relations(controlid))に対して、値を通知
                    GearsLogStack.setLog(fromControl.ID + " から " + conKey + " への処理を開始します")

                    Dim gcon As GearsControl = GControl(conKey)
                    Dim sender As GearsDTO = Nothing
                    If Not isGatherInfo And Not gto Is Nothing Then
                        sender = New GearsDTO(gto)
                    Else
                        sender = makeSendMessage(fromControl, toControl, gto)
                    End If

                    Try
                        Dim bindResult As Boolean = gcon.dataBind(sender)
                        '配下のコントロールへの展開を開始(パネル型の場合)
                        If bindResult Then
                            '探索用リストを作成
                            Dim visitedList As New VisitedList(fromControl.ID, _
                                                               makeFromToKey(fromControl, GControl(conKey).Control), _
                                                               getControlsInArea(gcon.Control))
                            attachData(gcon, visitedList)
                        End If

                    Catch ex As GearsException
                        addItemSafe(log, New KeyValuePair(Of String, GearsException)(gcon.ControlID, ex))
                        GearsLogStack.setLog(ex)
                        result = False

                    Catch ex As Exception
                        Dim gex As New GearsException(gcon.ControlID + "の処理中に例外が発生しました", ex)
                        addItemSafe(log, New KeyValuePair(Of String, GearsException)(gcon.ControlID, gex))
                        GearsLogStack.setLog(gex)
                        result = False
                    End Try
                Next
            Else
                GearsLogStack.setLog(fromControl.ID + " に対し関連が登録されていないため、更新処理は行われません。")
            End If
            Return result

        End Function

        '指定コントロール内のリレーションをたどり、取得した値をセットしていく 例外はスローして外でキャッチ
        Private Sub attachData(ByRef gcon As GearsControl, ByRef visitedList As VisitedList, Optional ByRef ds As GearsDataSource = Nothing)
            Dim outerRelation As List(Of String) = getValueSafe(_relations, gcon.ControlID)
            Dim innerRelation As List(Of String) = extractRootsInArea(gcon.Control)
            Dim dsData As GearsDataSource = Nothing
            Dim isIgnore As Boolean = False

            If ds Is Nothing Then '初回実行
                dsData = gcon.getDataSource
                GearsLogStack.setLog("データアタッチの開始", gcon.ControlID + " のデータソース " + TypeName(gcon.getDataSource).ToUpper + " を配下のコントロールへアタッチします ", visitedList.ToString)
            Else
                If _receiveEscapes.ContainsKey(visitedList.Relation) AndAlso _receiveEscapes(visitedList.Relation).Contains(gcon.ControlID) Then
                    GearsLogStack.setLog("データアタッチ中", "コントロール " + gcon.ControlID + " は除外対象として設定されているため、アタッチは行われません(配下のコントロールも同様です) ", visitedList.ToString)
                    isIgnore = True
                Else
                    GearsLogStack.setLog("データアタッチ中", "データソース " + TypeName(ds).ToUpper + " を " + gcon.ControlID + "アタッチしています... ", visitedList.ToString)
                    gcon.dataAttach(ds)
                End If
                dsData = ds

            End If

            visitedList.Add(gcon.ControlID)
            If isIgnore Then '除外対象の場合、以降の処理をスキップ
                Exit Sub
            End If

            '内部リレーションの更新
            If Not innerRelation Is Nothing Then
                For Each rRoot As String In innerRelation
                    If Not visitedList.isVisited(rRoot) Then 'ルートノードで未到達
                        Dim childTree As New VisitedList(visitedList) '内部リレーションに、これまでの到達コントロールの履歴を渡す
                        childTree.Add(rRoot)
                        attachData(GControl(rRoot), childTree, dsData)
                    End If
                Next
            End If

            '外部リレーションへの伝達
            If Not outerRelation Is Nothing Then
                For Each rel As String In outerRelation
                    'ルートノード未到達かつターゲットコントロール(パネルやビューなどは連鎖リレーション解決の対象にしない)
                    If Not visitedList.isVisited(rel) And visitedList.isNeighbor(GControl(rel).ControlID) Then
                        GControl(rel).dataBind(makeSendMessage(gcon.Control, Nothing, Nothing))
                        attachData(GControl(rel), visitedList, dsData)
                    ElseIf Not visitedList.isNeighbor(GControl(rel).ControlID) Then
                        GearsLogStack.setLog("関連先 " + rel + " は、派生元 " + visitedList.Visiter + " に含まれないため、処理されません")

                    End If
                Next
            End If

        End Sub

        Public Function isTargetControl(ByRef control As Control) As Boolean
            Dim result As Boolean = False

            If Not control.ID Is Nothing Then
                If control.ID.Length >= 4 Then
                    Dim prefix As String = control.ID.Substring(0, 4)
                    '小文字3文字+大文字～で始まるかチェック(例：txtHOGEなど)
                    If System.Text.RegularExpressions.Regex.IsMatch(prefix, "^[a-z]{3}[A-Z]") Then
                        Dim isGconDeclare As Boolean = GearsControl.isIdAttributeExist(control.ID, GCON_TARGET)
                        If _
                            TypeOf control Is ListControl Or _
                            (TypeOf control Is RadioButton And Not TypeOf control.Parent Is RadioButtonList) Or _
                            (TypeOf control Is CheckBox And Not TypeOf control.Parent Is CheckBoxList) Or _
                            TypeOf control Is TextBox Or _
                            TypeOf control Is HiddenField Or _
                            (TypeOf control Is Label And isGconDeclare) Or _
                            (TypeOf control Is Literal And isGconDeclare) Then
                            result = True

                        End If

                    End If
                End If
            End If

            Return result

        End Function
        Public Function isRegisteredControl(ByRef control As Control) As Boolean
            If Not control Is Nothing AndAlso containsKeySafe(_gcontrols, control.ID) Then
                Return True
            Else
                Return False
            End If
        End Function
        Public Function isRegisteredAsTarget(ByRef control As Control) As Boolean
            If isRegisteredControl(control) Then
                If isTargetControl(GControl(control.ID).Control) Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Function

        Protected Function containsKeySafe(Of T, K)(ByVal dic As Dictionary(Of T, K), ByVal key As T) As Boolean
            Dim result As Boolean = False
            If Not getValueSafe(dic, key) Is Nothing Then
                result = True
            End If
            Return result
        End Function

        Protected Function addItemSafe(Of T, K)(ByVal dic As Dictionary(Of T, K), ByVal item As KeyValuePair(Of T, K)) As Boolean

            If Not containsKeySafe(dic, item.Key) Then
                dic.Add(item.Key, item.Value)
                Return True
            Else
                Return False
            End If

        End Function

        Protected Function getValueSafe(Of T, K)(ByVal dic As Dictionary(Of T, K), ByVal key As T) As K
            If Not key Is Nothing Then
                If Not dic Is Nothing AndAlso dic.ContainsKey(key) Then
                    Return dic(key)
                Else
                    Return Nothing
                End If
            Else
                Return Nothing

            End If
        End Function


        Public Overrides Function toString() As String
            Dim str As String = ""
            For Each item As KeyValuePair(Of String, List(Of String)) In _relations
                Dim temp As String = item.Key + " : "
                For Each relate As String In item.Value
                    temp += relate + ","
                Next
                temp += vbCrLf
                str += temp
            Next
            Return str

        End Function
        Private Function makeFromToKey(ByVal fromControl As Control, ByVal toControl As Control) As String
            Dim fromToKey As String = ""
            If Not toControl Is Nothing Then
                fromToKey = fromControl.ID + CONTROL_ID_SEPARATOR + toControl.ID
            Else
                fromToKey = fromControl.ID + CONTROL_ID_SEPARATOR + fromControl.ID 'デフォルト自分自身
            End If
            Return fromToKey

        End Function

        Public Function getLog() As Dictionary(Of String, GearsException)
            Return log
        End Function

        'コントロールアタッチ用 インナークラス
        Public Class VisitedList

            Private _visiter As String
            Public ReadOnly Property Visiter() As String
                Get
                    Return _visiter
                End Get
            End Property

            Private _relation As String
            Public Property Relation() As String
                Get
                    Return _relation
                End Get
                Set(ByVal value As String)
                    _relation = value
                End Set
            End Property

            Private _vlist As New List(Of String)
            Public ReadOnly Property VList() As List(Of String)
                Get
                    Return _vlist
                End Get
            End Property

            Private _neighbor As List(Of String)
            Protected ReadOnly Property Neighbor() As List(Of String)
                Get
                    Return _neighbor
                End Get
            End Property

            Public Sub Add(ByVal visit As String)
                _vlist.Add(visit)
            End Sub

            Public Function isVisited(ByVal visit As String) As Boolean
                Return _vlist.Contains(visit)
            End Function

            Public Function isNeighbor(ByVal target As String) As Boolean
                Return _neighbor.Contains(target)
            End Function

            Public Sub New(ByVal visiter As String, ByVal relation As String, Optional ByVal neighbor As List(Of String) = Nothing)
                _visiter = visiter
                _relation = relation
                If Not neighbor Is Nothing Then
                    _neighbor = neighbor
                End If

            End Sub

            Public Sub New(ByRef vl As VisitedList)
                _relation = vl.Relation
                _visiter = vl.Visiter
                _vlist = New List(Of String)(vl.VList)
                _neighbor = vl.Neighbor '共用

            End Sub


            Public Overrides Function ToString() As String
                Dim result As String = ""
                For i As Integer = 0 To VList.Count - 1
                    If i = 0 Then
                        result += VList(i)
                    Else
                        result += " > " + VList(i)
                    End If
                Next
                Return result
            End Function

        End Class


    End Class

End Namespace
