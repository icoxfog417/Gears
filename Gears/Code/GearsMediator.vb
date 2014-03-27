Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports Gears.DataSource
Imports Gears.Util

Namespace Gears

    ''' <summary>
    ''' コントロール間の関連を管理するクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsMediator

        Private _dsNamespace As String = ""
        ''' <summary>デフォルトで使用する名称空間</summary>
        Public Property DsNamespace() As String
            Get
                Return _dsNamespace
            End Get
            Set(ByVal value As String)
                _dsNamespace = value
            End Set
        End Property

        Private _connectionName As String = ""
        ''' <summary>デフォルトで使用する接続文字列</summary>
        Public Property ConnectionName() As String
            Get
                Return _connectionName
            End Get
            Set(ByVal value As String)
                _connectionName = value
            End Set
        End Property

        Private _gcontrols As New Dictionary(Of String, GearsControl)
        ''' <summary>画面コントロールをGearsControl化し登録したリスト</summary>
        Public ReadOnly Property GControls() As Dictionary(Of String, GearsControl)
            Get
                Return _gcontrols
            End Get
        End Property

        Public Function GControl(ByVal con As Control) As GearsControl
            Dim econ As Control = GearsControl.extractControl(con)
            If econ IsNot Nothing Then
                Return GControl(econ.ID)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>指定したIDのGearsControlを取得する</summary>
        Public Function GControl(ByVal id As String) As GearsControl
            If Not id Is Nothing AndAlso _gcontrols.ContainsKey(id) Then
                Return _gcontrols(id)
            Else
                Return Nothing
            End If
        End Function

        Private _relations As New Dictionary(Of String, List(Of String))
        ''' <summary>
        ''' 登録されたコントロール間の関連を取得する
        ''' </summary>
        ''' <param name="conid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Relation(ByVal conid As String) As List(Of GearsControl)
            Dim root As RelationNode = RelationNode.makeTreeWithRoot(_relations)
            Dim targetNode As RelationNode = root.findNode(conid)
            Dim result As New List(Of GearsControl)

            If targetNode IsNot Nothing Then
                targetNode.visitChildren(Function(n As RelationNode) As String
                                             If _gcontrols.ContainsKey(n.Value) Then result.Add(_gcontrols(n.Value))
                                             Return String.Empty
                                         End Function)
            End If

            Return result
        End Function

        Public Function Relation(ByVal con As Control) As List(Of GearsControl)
            Return Relation(GearsControl.extractControl(con).ID)
        End Function

        Private _log As New Dictionary(Of String, GearsException)
        ''' <summary>処理結果ログ</summary>
        Public ReadOnly Property GLog As Dictionary(Of String, GearsException)
            Get
                Return _log
            End Get
        End Property

        Private Const LOCK_WHEN_SEND_KEY As String = "+LOCK_WHEN_SEND+"

        ''' <summary>
        ''' makeDTOで使用する<br/>
        ''' Delegateで引数として渡すdto内にこのキーで送信元/送信先のコントロールを格納することで、除外対象として指定されたコントロールを特定する
        ''' </summary>
        ''' <remarks></remarks>
        Private Const RELATION_STORE_KEY As String = "+RELATION_STORE_KEY+"

        ''' <summary>
        ''' makeDTOで使用する<br/>
        ''' 除外対象を指定する際、fromとtoのコントロールIDを区切るためのセパレータ
        ''' </summary>
        ''' <remarks></remarks>
        Private Const CONTROL_ID_SEPARATOR As String = "/"

        Private _excepts As New Dictionary(Of String, List(Of String))

        ''' <summary>
        ''' デフォルトの接続文字列/名称空間を受け取りインスタンスを作成する
        ''' </summary>
        ''' <param name="conName"></param>
        ''' <param name="dsn"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal conName As String, Optional ByVal dsn As String = "")
            _connectionName = conName
            _dsNameSpace = dsn
        End Sub

        ''' <summary>
        ''' 与えられたコントロール配下のコントロールを自身に登録する
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <param name="isAutoLoadAttr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function addControls(ByVal parent As Control, Optional isAutoLoadAttr As Boolean = True) As List(Of GearsControl)
            Dim result As New List(Of GearsControl)

            '親コントロールを探索し、配下のコントロールのうち対象であるものを登録する
            ControlSearcher.fetchControls(parent, _
                            Sub(control As Control, ByRef dto As GearsDTO)
                                Dim gcon As GearsControl = createGControl(control, isAutoLoadAttr)

                                If Not isOtherTarget(control) Then
                                    addControl(gcon)
                                Else
                                    Select Case TypeOf control Is Control
                                        Case TypeOf control Is Panel
                                            If gcon.IsFormAttribute Or gcon.IsFilterAttribute Then
                                                addControl(gcon)
                                            End If
                                        Case TypeOf control Is CompositeDataBoundControl
                                            If gcon.DataSource IsNot Nothing Then
                                                addControl(gcon)
                                            End If
                                    End Select
                                End If
                            End Sub,
                            Function(control As Control) As Boolean
                                Return isInputControl(control) Or isOtherTarget(control)
                            End Function,
                            AddressOf Me.isFetchTarget
                        )

            Return result

        End Function

        ''' <summary>
        ''' 単一のコントロールを自身に登録する
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="isAutoLoadAttr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function addControl(ByVal con As Control, Optional isAutoLoadAttr As Boolean = True) As GearsControl
            Return addControl(createGControl(con, isAutoLoadAttr))
        End Function

        ''' <summary>
        ''' 単一のコントロールを自身に登録する
        ''' </summary>
        ''' <param name="gcon"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function addControl(ByVal gcon As GearsControl) As GearsControl
            If gcon IsNot Nothing AndAlso Not _gcontrols.ContainsKey(gcon.ControlID) Then
                _gcontrols.Add(gcon.ControlID, gcon)
                Return gcon
            Else
                If gcon Is Nothing Then
                    GearsLogStack.setLog("追加対象のGearsControlにNothingが設定されています")
                Else
                    GearsLogStack.setLog(gcon.ControlID + " は既に追加されています。入れ替えたい場合はreplaceControlを使用してください")
                End If
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 登録済みのコントロールを交換する
        ''' </summary>
        ''' <param name="gcon"></param>
        ''' <remarks></remarks>
        Public Sub replaceControl(ByVal gcon As GearsControl)
            If _gcontrols.ContainsKey(gcon.ControlID) Then
                _gcontrols(gcon.ControlID) = gcon
            End If
        End Sub

        ''' <summary>
        ''' コントロール同士の関連を登録する(文字列でIDを指定)
        ''' </summary>
        ''' <param name="fromControlId"></param>
        ''' <param name="toControlId"></param>
        ''' <remarks></remarks>
        Public Sub addRelation(ByVal fromControlId As String, ByVal toControlId As String)

            If Not GControl(fromControlId) Is Nothing And Not GControl(toControlId) Is Nothing Then
                addRelation(GControl(fromControlId).Control, GControl(toControlId).Control)
            End If

        End Sub

        Public Sub addRelation(ByVal fromControl As GearsControl, ByVal toControl As GearsControl)
            addRelation(fromControl.ControlID, toControl.ControlID)
        End Sub

        ''' <summary>
        ''' コントロール同士の関連を登録する
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <remarks></remarks>
        Public Sub addRelation(ByVal fromControl As Control, ByVal toControl As Control)

            Dim templateString As String = "{0} はまだGearsMediatorに登録されていません"
            If GControl(fromControl) Is Nothing Then
                Throw New GearsException(String.Format(templateString, fromControl.ID))
            ElseIf GControl(toControl) Is Nothing Then
                Throw New GearsException(String.Format(templateString, toControl.ID))
            ElseIf GControl(fromControl).ControlID = GControl(toControl).ControlID Then
                Throw New GearsException("自分自身への関連は登録できません(" + fromControl.ID + ")")
            Else
                Dim fcon As GearsControl = GControl(fromControl)
                If Not _relations.ContainsKey(fcon.ControlID) Then
                    _relations.Add(fcon.ControlID, New List(Of String) From {GControl(toControl).ControlID})
                Else
                    _relations(fcon.ControlID).Add(GControl(toControl).ControlID)
                End If
            End If

        End Sub

        ''' <summary>
        ''' リレーションを削除する
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub clearRelation(Optional ByVal con As Control = Nothing)
            If con Is Nothing Then
                _relations.Clear()
            Else
                If GControl(con) IsNot Nothing Then _relations.Remove(GControl(con).ControlID)
            End If
        End Sub

        ''' <summary>
        ''' 特定のコントロールからDTOを作成する際、除外するコントロールを指定する
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="excepts"></param>
        ''' <remarks></remarks>
        Public Sub addExcept(ByVal fromControl As Control, ByVal toControl As Control, ByVal excepts As List(Of Control))

            Dim exceptIds As New List(Of String)
            excepts.ForEach(Sub(c) If c IsNot Nothing AndAlso Not String.IsNullOrEmpty(c.ID) Then exceptIds.Add(GearsControl.extractControl(c).ID))
            addExcept(fromControl, toControl, exceptIds)

        End Sub

        ''' <summary>
        ''' 特定のコントロールからDTOを作成する際、除外するコントロールを指定する(除外を文字列指定)
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="excepts"></param>
        ''' <remarks></remarks>
        Public Sub addExcept(ByVal fromControl As Control, ByVal toControl As Control, ByVal excepts As List(Of String))
            Dim key As String = makeFromToKey(fromControl, toControl)

            If Not _excepts.ContainsKey(key) Then
                _excepts.Add(key, excepts)
            Else
                _excepts(key).AddRange(excepts)
            End If
        End Sub

        ''' <summary>
        ''' 特定のコントロールからDTOを作成する際、除外するコントロールを指定する(ParamArray渡し用)
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="excepts"></param>
        ''' <remarks></remarks>
        Public Sub addExcept(ByVal fromControl As Control, ByVal toControl As Control, ParamArray excepts As Control())
            addExcept(fromControl, toControl, excepts.ToList)
        End Sub

        ''' <summary>
        ''' 除外指定を解除する
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <remarks></remarks>
        Public Sub clearExcept(Optional ByVal fromControl As Control = Nothing, Optional ByVal toControl As Control = Nothing)
            If fromControl Is Nothing And toControl Is Nothing Then
                _excepts.Clear()
            ElseIf fromControl IsNot Nothing Then
                Dim key As String = makeFromToKey(fromControl, toControl)
                If _excepts.ContainsKey(key) Then
                    _excepts.Remove(key)
                End If
            End If

        End Sub

        ''' <summary>
        ''' 除外対象を管理するためのキーを作成する
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function makeFromToKey(ByVal fromControl As Control, ByVal toControl As Control) As String
            Dim fromToKey As String = ""
            Dim fCon As Control = GearsControl.extractControl(fromControl)
            If Not toControl Is Nothing Then
                fromToKey = fCon.ID + CONTROL_ID_SEPARATOR + GearsControl.extractControl(toControl).ID
            Else
                fromToKey = fCon.ID + CONTROL_ID_SEPARATOR + fCon.ID 'デフォルト自分自身
            End If
            Return fromToKey

        End Function

        ''' <summary>
        ''' 自身の持つデフォルトの接続文字列/名称空間を使用しGearsControlを作成する
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="isAutoLoadAttr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function createGControl(ByVal con As Control, Optional isAutoLoadAttr As Boolean = True) As GearsControl

            Dim cn = ConnectionName
            Dim ds = DsNameSpace
            Dim addedControl As New List(Of String)

            If TypeOf con Is WebControl Then 'コントロールに名称空間/データソース接続の直接指定があればそちらを優先
                Dim conset As String = GearsControl.getControlAttribute(con, GearsControl.ATTR_DS_CONNECTION_NAME)
                Dim dnsset As String = GearsControl.getControlAttribute(con, GearsControl.ATTR_DS_NAMESPACE)
                If Not String.IsNullOrEmpty(conset) Then
                    cn = conset
                End If
                If Not String.IsNullOrEmpty(dnsset) Then
                    ds = dnsset
                End If
            ElseIf TypeOf con Is IFormItem Then
                If Not String.IsNullOrEmpty(CType(con, IFormItem).ConnectionName) Then cn = CType(con, IFormItem).ConnectionName
                If Not String.IsNullOrEmpty(CType(con, IFormItem).DSNamespace) Then ds = CType(con, IFormItem).DSNamespace
            End If

            Dim gcon As GearsControl = New GearsControl(con, cn, ds, isAutoLoadAttr)

            Return gcon

        End Function

        ''' <summary>
        ''' 指定されたコントロール内にあり、かつ登録済みのコントロールをリスト化し返却する<br/>
        ''' なお、ここのコントロールにはIFormItemであるコントロールは含まない(内部のコントロールは取り出す)
        ''' </summary>
        ''' <param name="target"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function getRegisterdSubControlIds(ByVal target As Control) As List(Of String)

            Dim ids As New List(Of String)
            ControlSearcher.fetchControls(target,
                                          Sub(control As Control, ByRef dto As GearsDTO)
                                              ids.Add(control.ID)
                                          End Sub,
                                          Function(control As Control) As Boolean
                                              'コントロールが範囲内にあり、登録済み(登録済みターゲットのみ扱うため、isFetchは不要)
                                              If Not control.ID Is Nothing AndAlso isRegisteredControl(control) Then
                                                  Return True
                                              Else
                                                  Return False
                                              End If
                                          End Function)
            Return ids

        End Function

        ''' <summary>
        ''' fromのコントロール情報からDTOを作成し、toのコントロールに送信する。<br/>
        ''' toコントロールは受け取ったDTOを自身のデータソースクラスに渡し、データベースの抽出/更新処理を実行する<br/>
        ''' ※データベースに対しどのような処理が行われるかは、DTOのActionにより決定される
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl">Nothing可。除外対象を明示的に使用したい場合に指定</param>
        ''' <param name="fromDto">Nothing可。予め用意したDTOに追加したい場合に指定</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function makeDTO(ByVal fromControl As Control, ByVal toControl As Control, ByVal fromDto As GearsDTO) As GearsDTO

            If fromControl IsNot Nothing AndAlso Not String.IsNullOrEmpty(fromControl.ID) Then
                Dim fromToKey As String = makeFromToKey(fromControl, toControl)

                Dim message As GearsDTO = New GearsDTO(fromDto)
                message.addAttrInfo(RELATION_STORE_KEY, fromToKey) 'from->toで送りたくないコントロールが設定されている場合、その検索に使用

                If TypeOf fromControl Is GridView Then 'サポート対象の特殊コントロール
                    fetchControlInfo(fromControl, message)
                ElseIf fromControl.HasControls Then 'その他複合コントロール
                    ControlSearcher.fetchControls(fromControl, AddressOf Me.fetchControlInfo, AddressOf Me.isRegisteredAsInput, AddressOf Me.isFetchTarget, message)
                ElseIf isRegisteredAsInput(fromControl) Then
                    fetchControlInfo(fromControl, message)
                End If

                '開放
                message.removeAttrInfo(RELATION_STORE_KEY)

                'フォームコントロールの場合、フォーム属性を付与
                For Each info As KeyValuePair(Of String, List(Of GearsControlInfo)) In message.ControlInfo
                    For Each conInfo As GearsControlInfo In info.Value
                        If (GControl(fromControl) IsNot Nothing AndAlso GControl(fromControl).IsFormAttribute) Then '登録されているコントロール
                            conInfo.IsFormAttribute = True
                        ElseIf GearsControl.isIdAttributeExist(fromControl.ID, GearsControl.ID_ATTR_FORM) Then
                            conInfo.IsFormAttribute = True
                        End If
                    Next
                Next

                Return message
            Else
                Throw New GearsException("指定されたコントロールがNothingか、IDが設定されていません")
            End If
        End Function

        ''' <summary>
        ''' 指定ControlをControlInfoに変換し、DTOに格納する<br/>
        ''' 除外指定が行われているか、DisplayOnlyであるコントロールの場合、これを対象としない
        ''' </summary>
        ''' <param name="control"></param>
        ''' <param name="dto"></param>
        ''' <remarks></remarks>
        Private Sub fetchControlInfo(ByVal control As Control, ByRef dto As GearsDTO)

            Dim isExcept As Boolean = False

            '表示専用のコントロールは、SELECT以外の場合除外する
            If GControl(control) IsNot Nothing AndAlso (GControl(control).IsDisplayOnly And dto.Action <> ActionType.SEL) Then isExcept = True

            '除外対象である場合は、どんな場合でも除外する
            If Not isExcept AndAlso _excepts.ContainsKey(dto.AttrInfo(RELATION_STORE_KEY)) Then
                Dim excepts As List(Of String) = _excepts(dto.AttrInfo(RELATION_STORE_KEY))

                '自身の親がexcept対象であるか否かを確認する
                Dim now As Control = control
                While Not now Is Nothing
                    If excepts.Contains(now.ID) Then
                        isExcept = True
                        Exit While
                    ElseIf TypeOf now Is Page Then 'ページコントロールは画面に1つしか存在しないため、Pageに達したら抜ける(無限ループの保険)
                        Exit While
                    End If
                    now = now.Parent
                End While

            End If

            If Not isExcept Then
                Dim conInfos As List(Of GearsControlInfo) = GControl(control).toControlInfo
                For Each c As GearsControlInfo In conInfos
                    dto.addControlInfo(c)
                Next
            Else
                GearsLogStack.setLog(control.ID + " は除外対象として登録されているため、送信対象に含まれません")
            End If

        End Sub

        ''' <summary>
        ''' executeの引数省略版
        ''' </summary>
        ''' <param name="control"></param>
        ''' <param name="aType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function execute(ByVal control As Control, Optional ByVal aType As ActionType = ActionType.SAVE) As Boolean
            Return execute(control, New GearsDTO(aType))
        End Function

        ''' <summary>
        ''' 与えられたコントロールに対して、指定されたアクションを実行する<br/>
        ''' この処理では、自身から作成したDTOを自身のデータソースに送信する。
        ''' フォーム等で更新を行う場合はこのような処理となる
        ''' </summary>
        ''' <param name="control"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function execute(ByVal control As Control, ByVal dto As GearsDTO) As Boolean
            _log.Clear()
            Dim gcon As GearsControl = GControl(control)
            Dim sender As GearsDTO = Nothing
            If Not String.IsNullOrEmpty(dto.AttrInfo(LOCK_WHEN_SEND_KEY)) Then
                sender = New GearsDTO(dto)
            Else
                sender = makeDTO(control, control, dto)
            End If

            _log = bindAndAttach(gcon, gcon, sender)

            Return _log.Count = 0 'ログが何もなければ成功

        End Function

        ''' <summary>
        ''' 関連する全てのコントロールに対し、自身から生成したDTOを指定されたActionで送信する。
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="aType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function send(ByVal fromControl As Control, Optional ByVal aType As ActionType = ActionType.SEL) As Boolean
            Return send(fromControl, Nothing, New GearsDTO(aType))
        End Function

        ''' <summary>
        ''' 関連する全てのコントロールに対し、自身から生成したDTOを送信する。<br/>
        ''' DTOは、引数で指定されたDTOをベースとしそこに追加する形で作成する。
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function send(ByVal fromControl As Control, ByVal dto As GearsDTO) As Boolean
            Return send(fromControl, Nothing, dto)
        End Function

        ''' <summary>
        ''' 相手先を明示的に指定し、自身から生成したDTOを指定されたActionで送信する。
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="aType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function send(ByVal fromControl As Control, ByVal toControl As Control, Optional ByVal aType As ActionType = ActionType.SEL) As Boolean
            Return send(fromControl, toControl, New GearsDTO(aType))
        End Function

        ''' <summary>
        ''' 相手先を明示的に指定し、自身から生成したDTOを送信する。<br/>
        ''' DTOは、引数で指定されたDTOをベースとしそこに追加する形で作成する。
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function send(ByVal fromControl As Control, ByVal toControl As Control, ByVal dto As GearsDTO) As Boolean
            _log.Clear()

            Dim fcon As GearsControl = GControl(fromControl)
            Dim tcons As New List(Of GearsControl)

            If toControl Is Nothing Then
                '指定がない場合、リレーションを使用する
                If _relations.ContainsKey(fcon.ControlID) Then
                    _relations(fcon.ControlID).ForEach(Sub(r) tcons.Add(GControl(r)))
                End If
            Else
                '具体的な指定がある場合はそれを使用する(リレーションの有無は問わない)
                tcons = New List(Of GearsControl) From {GControl(toControl)}
            End If

            If tcons IsNot Nothing Then
                For Each con As GearsControl In tcons

                    Dim sender As GearsDTO = Nothing
                    If Not String.IsNullOrEmpty(dto.AttrInfo(LOCK_WHEN_SEND_KEY)) Then 'ロックがかけられている場合追加しない
                        sender = New GearsDTO(dto)
                    Else
                        sender = makeDTO(fromControl, toControl, dto)
                    End If

                    Dim log As Dictionary(Of String, GearsException) = bindAndAttach(fcon, con, sender)
                    For Each lg As KeyValuePair(Of String, GearsException) In log
                        _log.Add(lg.Key, lg.Value)
                    Next
                Next

            Else
                GearsLogStack.setLog(fromControl.ID + "の関連先がないため、send処理は行われません")
            End If


            Return _log.Count = 0 'ログが何もなければ成功

        End Function

        ''' <summary>
        ''' DTOをロックし、その後の処理で値が追加されないようにする
        ''' </summary>
        ''' <param name="dto"></param>
        ''' <remarks></remarks>
        Public Sub lockDtoWhenSend(ByRef dto As GearsDTO)
            dto.addAttrInfo(LOCK_WHEN_SEND_KEY, "X")
        End Sub

        ''' <summary>
        ''' 指定されたコントロール内にある処理対象コントロールと関連をリスト化する。<br/>
        ''' これを元にdataAttachを実行していく。
        ''' </summary>
        ''' <param name="con"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function makeRelationMap(Optional ByVal con As Control = Nothing, Optional ByVal fromcon As Control = Nothing) As List(Of RelationNode)

            Dim localRelation As Dictionary(Of String, List(Of String)) = _relations.ToDictionary(Function(i) i.Key, Function(i) i.Value)
            Dim result As List(Of RelationNode)

            'コントロールが指定されている場合、除外対象のリレーションの処理を行う
            If con IsNot Nothing Then
                Dim tmplocalRelation As New Dictionary(Of String, List(Of String))

                For Each item As KeyValuePair(Of String, List(Of String)) In localRelation
                    Dim newRelation As New List(Of String)
                    For Each rel As String In item.Value
                        Dim ignore As Boolean = False
                        '対象が指定コントロール(con)、または起点となったコントロール(fromcon)の場合、循環が発生するため処理しない
                        ignore = (rel = con.ID Or rel = If(fromcon IsNot Nothing, fromcon.ID, String.Empty))

                        'フィルタが起点となっている場合、値が一意となる保証がないためフォーム/フィルタのパネルは処理しない(visitListに入れない)
                        If Not ignore And fromcon IsNot Nothing Then
                            ignore = GControl(fromcon).IsFilterAttribute And (GControl(rel).IsFilterAttribute Or GControl(rel).IsFormAttribute)
                        End If

                        If Not ignore Then newRelation.Add(rel)
                    Next
                    tmplocalRelation.Add(item.Key, newRelation)
                Next
                localRelation = tmplocalRelation
            End If

            If con IsNot Nothing Then
                '配下のコントロールも含めた関係リストを作成。配下のコントロールは宛先なしの関連として扱う
                Dim nodes As New List(Of String)
                If con.HasControls Then
                    nodes = getRegisterdSubControlIds(con)
                    For Each scon As String In nodes
                        If Not localRelation.ContainsKey(scon) Then localRelation.Add(scon, Nothing)
                    Next
                Else
                    nodes.Add(con.ID)
                End If

                Dim root As RelationNode = RelationNode.makeTreeWithRoot(localRelation)
                result = root.getBranches(nodes)

            Else
                result = RelationNode.makeTree(localRelation)
            End If

            Return result

        End Function

        ''' <summary>
        ''' DTOの受信処理と、それにより更新された内容を関連/サブコントロールに対し送信する処理を行う。<br/>
        ''' 具体的には、更新されたDataSourceの結果セット(DataTable)の値を配下のコントロールへ反映するために、関連を考慮しながらdataBind/dataAttachを実行していく。
        ''' </summary>
        ''' <param name="gcon"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function bindAndAttach(ByVal fromControl As GearsControl, ByVal gcon As GearsControl, ByVal dto As GearsDTO) As Dictionary(Of String, GearsException)
            Dim log As New Dictionary(Of String, GearsException)
            Dim nodeInProcess As String = ""

            Try

                Dim bindResult As Boolean = gcon.dataBind(dto)

                '配下/関連先のコントロールに対し値の反映を行う
                If bindResult And (gcon.Control.HasControls Or _relations.ContainsKey(gcon.ControlID)) Then
                    Dim visitList As List(Of RelationNode) = makeRelationMap(gcon.Control, fromControl.Control)

                    For Each node As RelationNode In visitList
                        nodeInProcess = node.Value
                        Dim ncon As GearsControl = GControl(node.Value)

                        ncon.dataAttach(gcon.DataSource)
                        If Not node.isLeaf Then '子がある場合、親から子へ情報を伝達、値を設定する
                            node.visitChildren(Function(nv As RelationNode) As String
                                                   Dim p As GearsControl = GControl(nv.Parent.Value) '子をたどっているため、親がないのはありえない
                                                   Dim c As GearsControl = GControl(nv.Value)
                                                   Dim ptoc As GearsDTO = makeDTO(p.Control, c.Control, Nothing)
                                                   c.dataBind(ptoc) '親の値を子に通知
                                                   c.dataAttach(gcon.DataSource) '大元のデータソースの値を設定
                                                   Return nv.Value
                                               End Function)
                        End If
                    Next

                End If

            Catch ex As GearsException
                If Not String.IsNullOrEmpty(nodeInProcess) Then ex.addDetail("位置:" + nodeInProcess)
                log.Add(gcon.ControlID, ex)
                GearsLogStack.setLog(ex)
            Catch ex As Exception
                Dim gex As New GearsException(gcon.ControlID + "の処理中に例外が発生しました(位置:" + nodeInProcess + ")", ex)
                log.Add(gcon.ControlID, gex)
                GearsLogStack.setLog(gex)
            End Try

            Return log

        End Function

        ''' <summary>
        ''' 以後検索を行わないコントロールの判定を行う
        ''' </summary>
        ''' <param name="control"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function isFetchTarget(ByVal control As Control) As Boolean
            Dim ignore As Boolean = False
            '入力用コントロールとして登録された場合、以後配下のコントロールは対象としない(AjaxControlToolkit対策)
            ignore = isInputControl(control)

            'ユーザーコントロールは対象外とする。ただし、ページ元となるMasterPageと入力用コントロールを処理するIFormItem、対象を明示的に示すIGearsTargetを除く
            If Not ignore Then ignore = TypeOf control Is UserControl AndAlso Not (TypeOf control Is MasterPage Or TypeOf control Is IFormItem Or TypeOf control Is IGearsTarget)

            Return Not ignore

        End Function

        ''' <summary>
        ''' 入力用フォームコントロールか否かを判定する。ただし、ネーミングルールに沿わないものは除外する。
        ''' </summary>
        ''' <param name="control"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function isInputControl(ByVal control As Control) As Boolean
            Dim result As Boolean = False

            If isIdNamingMatch(control.ID) Then
                Dim isGconDeclare As Boolean = GearsControl.isIdAttributeExist(control.ID, GearsControl.ID_ATTR_GCON)
                If Not isGconDeclare Then isGconDeclare = GearsControl.isIdAttributeExist(control.ID, GearsControl.ID_ATTR_GDISP)

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

            Return result

        End Function

        ''' <summary>
        ''' 入力用コントロール以外に、対象の候補となるコントロールを判定する
        ''' </summary>
        ''' <param name="control"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function isOtherTarget(ByVal control As Control) As Boolean
            Dim result As Boolean = False

            If isIdNamingMatch(control.ID) Then
                If TypeOf control Is Panel Then
                    result = True
                ElseIf TypeOf control Is CompositeDataBoundControl Then
                    result = True
                End If
            End If

            Return result

        End Function

        ''' <summary>
        ''' コントロールIDのネーミングルールをチェックする
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function isIdNamingMatch(ByVal id As String) As Boolean
            Dim result As Boolean = False

            If Not String.IsNullOrEmpty(id) AndAlso id.Length >= 4 Then
                Dim prefix As String = id.Substring(0, 4)
                '小文字3文字+大文字～で始まるかチェック(例：txtHOGEなど)
                If System.Text.RegularExpressions.Regex.IsMatch(prefix, "^[a-z]{3}[A-Z]") Then
                    result = True
                End If
            End If

            Return result

        End Function

        ''' <summary>
        ''' GearsMediatorに登録済みのコントロールか否かを判定する
        ''' </summary>
        ''' <param name="control"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function isRegisteredControl(ByVal control As Control) As Boolean
            If (control IsNot Nothing AndAlso control.ID IsNot Nothing) AndAlso _gcontrols.ContainsKey(control.ID) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 登録済みかつ入力用フォームコントロールであることを判定する
        ''' </summary>
        ''' <param name="control"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function isRegisteredAsInput(ByVal control As Control) As Boolean
            If isRegisteredControl(control) Then
                If isInputControl(control) Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Function

        Public Overrides Function toString() As String
            Dim result As String = ConnectionName + "/" + DsNamespace
            Return result
        End Function

    End Class

End Namespace
