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

        Private _dsNameSpace As String = ""
        ''' <summary>デフォルトで使用する名称空間</summary>
        Public Property DsNameSpace() As String
            Get
                Return _dsNameSpace
            End Get
            Set(ByVal value As String)
                _dsNameSpace = value
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
        ''' <param name="con"></param>
        ''' <param name="dsn"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal con As String, Optional ByVal dsn As String = "")
            _connectionName = con
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
                            End Function
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
            If Not _gcontrols.ContainsKey(gcon.ControlID) Then
                _gcontrols.Add(gcon.ControlID, gcon)
                Return gcon
            Else
                GearsLogStack.setLog(gcon.ControlID + " は既に追加されています。入れ替えたい場合はreplaceControlを使用してください")
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
        ''' <param name="conF"></param>
        ''' <param name="conT"></param>
        ''' <remarks></remarks>
        Public Sub addRelation(ByVal conF As String, ByVal conT As String)

            If Not GControl(conF) Is Nothing And Not GControl(conT) Is Nothing Then
                addRelation(GControl(conF).Control, GControl(conT).Control)
            End If

        End Sub

        Public Sub addRelation(ByVal conF As GearsControl, ByVal conT As GearsControl)
            addRelation(conF.ControlID, conT.ControlID)
        End Sub

        ''' <summary>
        ''' コントロール同士の関連を登録する
        ''' </summary>
        ''' <param name="conF"></param>
        ''' <param name="conT"></param>
        ''' <remarks></remarks>
        Public Sub addRelation(ByVal conF As Control, ByVal conT As Control)

            Dim templateString As String = "{0} はまだフレームワークに登録されていません。GMakeRuleを行う前に、GAddを使用し、コントロールの登録を行ってください"
            If GControl(conF.ID) Is Nothing Then
                Throw New GearsException(String.Format(templateString, conF.ID))
            ElseIf GControl(conT.ID) Is Nothing Then
                Throw New GearsException(String.Format(templateString, conT.ID))
            Else
                If Not _relations.ContainsKey(conF.ID) Then
                    _relations.Add(conF.ID, New List(Of String) From {conT.ID})
                Else
                    _relations(conF.ID).Add(conT.ID)
                End If
            End If

        End Sub

        ''' <summary>
        ''' 特定のコントロールからDTOを作成する際、除外するコントロールを指定する
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="escapes"></param>
        ''' <remarks></remarks>
        Public Sub addExcept(ByVal fromControl As Control, ByVal toControl As Control, ByVal escapes As List(Of String))
            Dim key As String = makeFromToKey(fromControl, toControl)
            If Not _excepts.ContainsKey(key) Then
                _excepts.Add(key, escapes)
            Else
                _excepts(key).AddRange(escapes)
            End If
        End Sub

        ''' <summary>
        ''' 特定のコントロールからDTOを作成する際、除外するコントロールを指定する(ParamArray渡し用)
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="escapes"></param>
        ''' <remarks></remarks>
        Public Sub addExcept(ByVal fromControl As Control, ByVal toControl As Control, ParamArray escapes As String())
            addExcept(fromControl, toControl, escapes.ToList)
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
            End If

            Dim gcon As GearsControl = New GearsControl(con, cn, ds, isAutoLoadAttr)

            Return gcon

        End Function

        ''' <summary>
        ''' 指定されたコントロール内にあり、かつ登録済みのコントロールをリスト化し返却する
        ''' </summary>
        ''' <param name="target"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function getRegisterdSubControlIds(ByVal target As Control) As List(Of String)

            Dim ids As New List(Of String)
            ControlSearcher.fetchControls(target, Sub(control As Control, ByRef dto As GearsDTO)
                                                      ids.Add(control.ID)
                                                  End Sub,
                                        Function(control As Control) As Boolean
                                            'コントロールが範囲内にあり、登録済み
                                            If Not control.ID Is Nothing AndAlso isRegisteredControl(control) AndAlso Not ControlSearcher.findControl(target, control.ID) Is Nothing Then
                                                Return True
                                            Else
                                                Return False
                                            End If
                                        End Function)
            Return ids

        End Function

        ''' <summary>
        ''' FROMからTOのコントロールへ処理を行う際のDTOを作成する。
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
                    ControlSearcher.fetchControls(fromControl, AddressOf Me.fetchControlInfo, AddressOf Me.isRegisteredAsInput, message)
                ElseIf isRegisteredAsInput(fromControl) Then
                    fetchControlInfo(fromControl, message)
                End If

                '開放
                message.removeAttrInfo(RELATION_STORE_KEY)

                'フォームコントロールの場合、フォーム属性を付与
                For Each info As KeyValuePair(Of String, List(Of GearsControlInfo)) In message.ControlInfo
                    For Each conInfo As GearsControlInfo In info.Value
                        If (GControl(fromControl.ID) IsNot Nothing AndAlso GControl(fromControl.ID).IsFormAttribute) Then '登録されているコントロール
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
        ''' 指定ControlをControlInfoに変換し、dto内に格納する<br/>
        ''' 除外指定が行われているか、DisplayOnlyであるコントロールの場合、これを対象としない
        ''' </summary>
        ''' <param name="control"></param>
        ''' <param name="dto"></param>
        ''' <remarks></remarks>
        Private Sub fetchControlInfo(ByVal control As Control, ByRef dto As GearsDTO)
            '除外対象のものは収集しない
            Dim isExcept As Boolean = False

            '表示専用のコントロールは、SELECT以外の場合除外する
            If GControl(control.ID) IsNot Nothing AndAlso (GControl(control.ID).IsDisplayOnly And dto.Action <> ActionType.SEL) Then isExcept = True

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
                Dim conInfos As List(Of GearsControlInfo) = GControl(control.ID).createControlInfo
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
        ''' この処理では、DTOは自分で作成し、自分自身のデータソースに適用する。
        ''' フォーム等で更新を行う場合はこのような処理となる
        ''' </summary>
        ''' <param name="control"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function execute(ByVal control As Control, ByVal dto As GearsDTO) As Boolean
            _log.Clear()
            Dim gcon As GearsControl = GControl(control.ID)
            Dim sender As GearsDTO = Nothing
            If Not String.IsNullOrEmpty(dto.AttrInfo(LOCK_WHEN_SEND_KEY)) Then
                sender = New GearsDTO(dto)
            Else
                sender = makeDTO(control, control, dto)
            End If

            _log = bindAndAttach(gcon, sender)

            Return _log.Count = 0 'ログが何もなければ成功

        End Function

        ''' <summary>
        ''' 関連する全てのコントロールに対し、自身の値で更新を行う
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="aType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function send(ByVal fromControl As Control, Optional ByVal aType As ActionType = ActionType.SEL) As Boolean
            Return send(fromControl, Nothing, New GearsDTO(aType))
        End Function

        ''' <summary>
        ''' 関連する全てのコントロールに対し、自身の値で更新を行う
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function send(ByVal fromControl As Control, ByVal dto As GearsDTO) As Boolean
            Return send(fromControl, Nothing, dto)
        End Function

        ''' <summary>
        ''' 関連する指定コントロールに対し、自身の値で更新を行う
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
        ''' 関連するコントロールに対し、自身の値で更新をかける
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function send(ByVal fromControl As Control, ByVal toControl As Control, ByVal dto As GearsDTO) As Boolean
            _log.Clear()

            Dim fcon As GearsControl = GControl(fromControl.ID)
            Dim tcons As New List(Of GearsControl)

            If toControl Is Nothing Then
                '指定がない場合、リレーションを使用する
                _relations(fromControl.ID).ForEach(Sub(r) tcons.Add(GControl(r)))
            Else
                '具体的な指定がある場合はそれを使用する(リレーションの有無は問わない)
                tcons = New List(Of GearsControl) From {GControl(toControl.ID)}
            End If

            If tcons IsNot Nothing Then
                For Each con As GearsControl In tcons

                    Dim sender As GearsDTO = Nothing
                    If Not String.IsNullOrEmpty(dto.AttrInfo(LOCK_WHEN_SEND_KEY)) Then
                        sender = New GearsDTO(dto)
                    Else
                        sender = makeDTO(fromControl, toControl, dto)
                    End If

                    Dim log As Dictionary(Of String, GearsException) = bindAndAttach(con, sender)
                    For Each lg As KeyValuePair(Of String, GearsException) In log
                        _log.Add(lg.Key, lg.Value)
                    Next
                Next

            Else
                GearsLogStack.setLog(fromControl.ID + "の関連先がないため、send処理は行われません")
            End If


            Return _log.Count = 0 'ログが何もなければ成功

        End Function

        Public Sub lockDtoWhenSend(ByRef dto As GearsDTO)
            dto.addAttrInfo(LOCK_WHEN_SEND_KEY, "X")
        End Sub

        Public Function makeRelationMap(Optional ByVal con As Control = Nothing) As List(Of RelationNode)

            Dim localRelation As Dictionary(Of String, List(Of String)) = _relations.ToDictionary(Function(i) i.Key, Function(i) i.Value)
            Dim result As List(Of RelationNode)

            If con IsNot Nothing Then
                '配下のコントロールも含めた関係リストを作成。配下のコントロールは宛先なしの関連として扱う
                Dim subControls As List(Of String) = getRegisterdSubControlIds(con)
                For Each scon As String In subControls
                    If Not localRelation.ContainsKey(scon) Then localRelation.Add(scon, Nothing)
                Next

                Dim root As RelationNode = RelationNode.makeTreeWithRoot(localRelation)
                result = root.getBranches(subControls)
            Else
                result = RelationNode.makeTree(localRelation)
            End If

            Return result

        End Function

        Private Function bindAndAttach(ByVal gcon As GearsControl, ByVal dto As GearsDTO) As Dictionary(Of String, GearsException)
            Dim log As New Dictionary(Of String, GearsException)

            Try

                Dim bindResult As Boolean = gcon.dataBind(dto)

                '配下のコントロールに対し値の反映を行う
                '関連先のコントロールに対し通知を行う
                If bindResult And gcon.Control.HasControls Then
                    Dim visitList As List(Of RelationNode) = makeRelationMap(gcon.Control)

                    For Each node As RelationNode In visitList
                        Dim ncon As GearsControl = GControl(node.Value)
                        ncon.dataAttach(gcon.DataSource)
                        If Not node.isLeaf Then '子がある場合、親から子へ情報を伝達、値を設定する
                            node.visitChildren(Function(nv As RelationNode) As String
                                                   Dim p As GearsControl = GControl(nv.Parent.Value) '子をたどっているため、親がないのはありえない
                                                   Dim c As GearsControl = GControl(nv.Value)
                                                   Dim ptoc As GearsDTO = makeDTO(p.Control, Nothing, Nothing)
                                                   c.dataBind(ptoc) '親の値を子に通知
                                                   c.dataAttach(gcon.DataSource) '大元のデータソースの値を設定
                                                   Return nv.Value
                                               End Function)
                        End If
                    Next

                End If

            Catch ex As GearsException
                log.Add(gcon.ControlID, ex)
                GearsLogStack.setLog(ex)
            Catch ex As Exception
                Dim gex As New GearsException(gcon.ControlID + "の処理中に例外が発生しました", ex)
                log.Add(gcon.ControlID, gex)
                GearsLogStack.setLog(gex)
            End Try

            Return log

        End Function

        Public Function isInputControl(ByVal control As Control) As Boolean
            Dim result As Boolean = False

            If isIdNamingMatch(control.ID) Then
                Dim isGconDeclare As Boolean = GearsControl.isIdAttributeExist(control.ID, GearsControl.ID_ATTR_GCON)
                If Not isGconDeclare Then GearsControl.isIdAttributeExist(control.ID, GearsControl.ID_ATTR_GDISP)

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

        Private Shared Function isIdNamingMatch(ByVal id As String) As Boolean
            Dim result As Boolean = False

            If Not String.IsNullOrEmpty(id) Then
                Dim prefix As String = id.Substring(0, 4)
                '小文字3文字+大文字～で始まるかチェック(例：txtHOGEなど)
                If System.Text.RegularExpressions.Regex.IsMatch(prefix, "^[a-z]{3}[A-Z]") Then
                    result = True
                End If
            End If

            Return result

        End Function

        Public Function isRegisteredControl(ByVal control As Control) As Boolean
            If (control IsNot Nothing AndAlso control.ID IsNot Nothing) AndAlso _gcontrols.ContainsKey(control.ID) Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Function isRegisteredAsInput(ByVal control As Control) As Boolean
            If isRegisteredControl(control) Then
                If isInputControl(GControl(control.ID).Control) Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
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

    End Class

End Namespace
