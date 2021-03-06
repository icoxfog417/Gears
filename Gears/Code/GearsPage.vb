﻿Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web
Imports System.Web.Security
Imports System.Text
Imports System.Reflection
Imports System.Globalization
Imports Gears.Validation
Imports Gears.DataSource
Imports Gears.Util
Imports Gears.JS

Namespace Gears

    ''' <summary>
    ''' Gearsフレームワークを使用する場合の、継承元となるページ
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class GearsPage
        Inherits Page

        ''' <summary>
        ''' ListItemのAttributeがPostBack時に消えてしまうため、これをViewStateに補完するためのキー<br/>
        ''' http://stackoverflow.com/questions/8157363/is-it-possible-to-maintain-added-attributes-on-a-listitem-while-posting-back
        ''' </summary>
        ''' <remarks></remarks>
        Public Const V_LIST_ATTRIBUTES As String = "GEARS_LIST_ATTRIBUTES"

        ''' <summary>画面ロードされた値をViewStateに保持しておくためのキー</summary>
        Public Const V_LOADED As String = "GEARS_VALUE_LOADED"

        ''' <summary>画面ロードされた、楽観ロック用の値をViewStateに保持しておくためのキー</summary>
        Public Const V_LOCKCOL As String = "GEARS_VALUE_FOR_LOCK"

        ''' <summary>リロード防止用のタイムスタンプをViewStateに保持するためのキー</summary>
        Public Const V_S_TIME_STAMP As String = "GEARS_TIME_STAMP"

        ''' <summary>
        ''' バリデーション情報が保管されたCssClassを保持する。<br/>
        ''' 初回以降は各属性のスタイルで書き換えられてしまうため、初回の値を保持
        ''' </summary>
        Public Const V_VALIDATORS As String = "GEARS_VALIDATORS"

        ''' <summary>権限を設定するための属性名</summary>
        Public Const A_ROLE_AUTH_ALLOW As String = "AUTHORIZATIONALLOW"

        ''' <summary>権限の有無によって切り替える属性(VISIBLE/ENABLEなど)</summary>
        Public Const A_ROLE_EVAL_ACTION As String = "ROLEEVALACTION"

        ''' <summary>グローバル化を行う対象を示す属性名</summary>
        Public Const A_GLOBALIZE_TARGET As String = "GGLOBALIZE"

        ''' <summary>
        ''' ログ出力を行うか否かの引数指定<br/>
        ''' gs_log_out=trueを設定することでログの出力が可能
        ''' </summary>
        Public Const Q_GEARS_IS_LOG_OUT As String = "gs_log_out"

        ''' <summary>警告のプロンプトを出すためのスクリプト名</summary>
        Const CS_ALERT_PROMPT_SCRIPT_NAME As String = "GearsAlertPromptScript"

        ''' <summary>警告を無視するか否かを設定したhiddenフィールド</summary>
        Const CS_ALERT_IS_IGNORE_FLG As String = "GearsYesAlertIgnore"

        ''' <summary>JavaScript変数の宣言を行うためのスクリプト名</summary>
        Const CS_JS_VARIABLE_BLOCK As String = "GearsJsObjectBlock"

        ''' <summary>ViewStateに値を設定する際の区切り文字</summary>
        Protected Const VIEW_STATE_SEPARATOR As String = "/"

        ''' <summary>デフォルトで使用するグローバルリソースファイル</summary>
        Protected Overridable Property GearsGlobalResource As String = "GearsResource"

        Private _isNeedJudgeReload As Boolean = True
        ''' <summary>リロードを判定するか否かのフラグ</summary>
        Public Property IsNeedJudgeReload() As Boolean
            Get
                Return _isNeedJudgeReload
            End Get
            Set(ByVal value As Boolean)
                _isNeedJudgeReload = value
            End Set
        End Property

        Private _isReload As Boolean = False
        ''' <summary>リクエストがリロードか否かの判定</summary>
        Public ReadOnly Property IsReload() As Boolean
            Get
                Return _isReload
            End Get
        End Property

        ''' <summary>コントロール間の関連を管理するクラス</summary>
        Protected GMediator As GearsMediator = Nothing

        ''' <summary>ログ</summary>GMediator
        Protected GLog As New Dictionary(Of String, GearsException)

        ''' <summary>
        ''' コントロールの管理を行うGearsMediatorを初期化する
        ''' </summary>
        ''' <param name="conName"></param>
        ''' <param name="dns"></param>
        ''' <remarks></remarks>
        Private Sub initMediator(ByVal conName As String, Optional ByVal dns As String = "")
            Dim pageConName As String = conName
            Dim pageDsNspace As String = dns
            Dim readProperty = Function(p As String) As String
                                   Dim result As String = p
                                   'マスターページプロパティを参照するものは、その値を取得
                                   If Not String.IsNullOrEmpty(p) AndAlso p.StartsWith("Master.") Then
                                       Dim prop As String = MasterProperty(Replace(p, "Master.", ""))
                                       If Not prop Is Nothing Then
                                           result = prop.ToString
                                       End If
                                   End If
                                   Return result
                               End Function

            'Config設定を読み込む
            Dim env As GearsSection = GearsSection.Read()
            Dim conInfo As String = Trim(env.DefaultConnection)
            Dim dnsInfo As String = Trim(env.DefaultNamespace)

            '設定
            If String.IsNullOrEmpty(pageConName) Then pageConName = conInfo
            pageConName = readProperty(pageConName)

            If String.IsNullOrEmpty(pageDsNspace) Then pageDsNspace = dnsInfo
            pageDsNspace = readProperty(pageDsNspace)

            '未作成か、設定値が異なる場合更新
            If GMediator Is Nothing OrElse (GMediator.ConnectionName <> pageConName Or GMediator.DsNamespace <> pageDsNspace) Then
                GMediator = New GearsMediator(pageConName, pageDsNspace)
            End If

        End Sub

        ''' <summary>
        ''' ページ初期化イベント
        ''' </summary>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Protected Overrides Sub OnInit(ByVal e As System.EventArgs)
            'GearsMediatorのインスタンス化
            initMediator(String.Empty, String.Empty)

            MyBase.OnInit(e)

            GIsAuth(Me) 'コントロールロード後、権限評価を実施する

        End Sub

        ''' <summary>
        ''' 画面上のコントロールをGearsControl化し、GearsMediatorの管理下に配置する
        ''' </summary>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Protected Overrides Sub OnPreLoad(ByVal e As System.EventArgs)
            MyBase.OnPreLoad(e)

            'リロード判定
            If IsNeedJudgeReload Then
                judgeIsReload()
            End If

            'コントロールへのデータ/アトリビュートのセット
            setUpPageControls()

            'ローカライゼーションを行う
            globalizeControl(Me)

            'ログ出力モードの場合トレースを開始する
            If IsLoggingMode() Then
                GearsLogStack.traceOn()
            End If

        End Sub

        ''' <summary>
        ''' ロード完了後イベント
        ''' </summary>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Protected Overrides Sub OnLoadComplete(e As System.EventArgs)
            MyBase.OnLoadComplete(e)

            'ログの書き出し
            If IsLoggingMode() Then
                '記録されたログを書き出し
                If Me.FindControl(Q_GEARS_IS_LOG_OUT) Is Nothing Then
                    Dim label As New Label
                    label.Text = GearsLogStack.makeDisplayString
                    Me.Controls.Add(label)
                Else
                    CType(Me.FindControl(Q_GEARS_IS_LOG_OUT), Label).Text = GearsLogStack.makeDisplayString
                End If

            ElseIf Not Me.FindControl(Q_GEARS_IS_LOG_OUT) Is Nothing Then
                Me.Controls.Remove(Me.FindControl(Q_GEARS_IS_LOG_OUT))
            End If

            GearsLogStack.traceEnd()

            'JavaScript変数定義スクリプトの書き出し
            JsVariableWrite()

        End Sub

        ''' <summary>
        ''' 発生したPostBackがリロードによるものか否かを判定する
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub judgeIsReload()

            Response.Cache.SetCacheability(HttpCacheability.NoCache)
            'IE以外の場合、nostoreも使用しないと駄目な模様 http://d.hatena.ne.jp/manymanytips/20110120/1295500136
            Response.Cache.SetNoStore()

            Dim dateNow As DateTime = Now
            Dim isAsync As Boolean = IsPageAsync()
            'ポストバック時、[F5]での二重登録防止判定 ※AsyncによるPostBackは除外する
            If Not isAsync Then

                If IsPostBack Then
                    'タイムスタンプが設定されていない場合
                    If Session(Request.FilePath & V_S_TIME_STAMP) Is Nothing Or ViewState(Request.FilePath & V_S_TIME_STAMP) Is Nothing Then
                        'ポストバックで遷移してきて、タイムスタンプが空の場合もリロードと判定する 
                        _isReload = True
                    Else

                        Dim dateSessionStamp As DateTime = DirectCast(Session(Request.FilePath & V_S_TIME_STAMP), DateTime)
                        Dim dateViewStateStamp As DateTime = DirectCast(ViewState(Request.FilePath & V_S_TIME_STAMP), DateTime)

                        '[F5]を押下したとき(リロード)のViewStateのタイムスタンプは、前回送信情報のまま
                        If dateSessionStamp <> dateViewStateStamp Then
                            _isReload = True
                        Else
                            _isReload = False
                        End If

                    End If
                End If

                'タイムスタンプを記録 ※非同期更新の場合はタイムスタンプの更新を行わない(ViewStateが更新されず、Sessionのみタイムスタンプが更新されてしまうため)
                Session(Request.FilePath & V_S_TIME_STAMP) = dateNow
                ViewState(Request.FilePath & V_S_TIME_STAMP) = dateNow

            End If

        End Sub

        ''' <summary>
        ''' ページ更新が非同期更新かどうかチェックする
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsPageAsync() As Boolean

            'TODO ToolkitScriptManagerの考慮が必要->ToolkitScriptManagerが使用されている場合、ScriptManager.GetCurrentでNothingが返る
            'これを回避するにはAjaxControlToolkit.ToolkitScriptManager.GetCurrent(Me)を使用する必要があるが、必ず使っているとも限らないのでアセンブリから読み込む必要あり
            Dim s As ScriptManager = ScriptManager.GetCurrent(Me)
            If s IsNot Nothing Then
                Return s.IsInAsyncPostBack()
            Else
                Return False
            End If

        End Function

        ''' <summary>
        ''' ログ出力モードか否かを判定
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsLoggingMode() As Boolean
            Dim isLogging As String = QueryValue(Q_GEARS_IS_LOG_OUT)
            Dim isDebug As Boolean = GearsSection.Read.isDebug
            If isDebug And (Not String.IsNullOrEmpty(isLogging) AndAlso (isLogging.ToLower = "true")) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' ページの初期化処理。<br/>
        ''' * ネーミングルールに沿うコントロールをGearsMediatorに登録する
        ''' * ViewStateに保持しておいた値から、ロード時の値をGearsControlにセットしておく
        ''' * CssClassから、GearsControlにバリデーション/スタイル表示のためのAttributeをロードする
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub setUpPageControls()

            If GMediator Is Nothing Then
                Throw New GearsException("ページの初期化が行われていません", "web.configのgears sectionの設定を確認して下さい")
            End If

            'コントロールを管理するGearsMediatorへコントロールを追加する
            '属性情報は、初回ロード以後は初回ロード時のCssClassを使用するため、自動ロードしない
            GMediator.addControls(Me, isAutoLoadAttr:=False)

            '初回ロード時のCSS
            Dim initialCss As Dictionary(Of String, String) = CType(ViewState(V_VALIDATORS), Dictionary(Of String, String))
            If initialCss Is Nothing Then
                initialCss = New Dictionary(Of String, String)
            End If

            '登録されたコントロールに対し、属性をセット
            For Each item As KeyValuePair(Of String, GearsControl) In GMediator.GControls

                '前回ロード時の値がある場合、過去ロード値をセット
                If Not reloadLoadedValue(item.Key) Is Nothing Then
                    item.Value.LoadedValue = reloadLoadedValue(item.Key)
                End If

                'データロード
                If Not IsPostBack Then
                    '初回、かつ入力用のコントロールについては事前にデータソースから値をロードする。
                    If GMediator.isInputControl(item.Value.Control) Then
                        item.Value.dataBind()
                    End If

                    'CSSからアトリビュートをロード
                    If TypeOf item.Value.Control Is WebControl Then
                        initialCss.Add(item.Key, CType(item.Value.Control, WebControl).CssClass)
                        item.Value.loadAttribute()
                        CType(item.Value.Control, WebControl).CssClass = item.Value.GCssClass 'CSSを書き換え
                    End If

                    'リストコントロールのAttributeが確保されない対応
                    saveListItemAttribute(item.Value.Control)
                Else
                    'Postbackの場合、基本はViewStateが担保されているのでロードは行わないが、
                    'ViewStateEnableがFalseでデータが消える場合、リロードをかける
                    If item.Value.Control.EnableViewState = False Then
                        item.Value.dataBind()
                    End If

                    '保管しておいた初期CSSからアトリビュートをロード/エラースタイルを消去
                    If TypeOf item.Value.Control Is WebControl AndAlso initialCss.ContainsKey(item.Key) Then
                        item.Value.loadAttribute(initialCss(item.Key))
                        CType(item.Value.Control, WebControl).CssClass = CType(item.Value.Control, WebControl).CssClass.Replace(" " + GearsAttribute.ERR_STYLE, "")
                    End If

                    'リストコントロールのAttributeが確保されない対応
                    loadListItemAttribute(item.Value.Control)
                End If

            Next

            '初期CSSを保管
            If ViewState(V_VALIDATORS) Is Nothing Then
                ViewState(V_VALIDATORS) = initialCss
            End If

        End Sub

        ''' <summary>
        ''' 指定コントロールについて、ローカライゼーションを行う
        ''' </summary>
        ''' <param name="target"></param>
        ''' <remarks></remarks>
        Protected Sub globalizeControl(ByVal target As Control)
            ControlSearcher.fetchControls(target, Sub(control As Control, ByRef dto As GearsDTO)
                                                      Dim itext As ITextControl = CType(control, ITextControl)
                                                      itext.Text = GGlobalize(itext.Text, GearsControl.getControlAttribute(control, A_GLOBALIZE_TARGET))
                                                  End Sub,
                                              Function(control As Control) As Boolean
                                                  Return TypeOf control Is ITextControl AndAlso Not String.IsNullOrEmpty(GearsControl.getControlAttribute(control, A_GLOBALIZE_TARGET))
                                              End Function)
        End Sub


        ''' <summary>
        ''' 登録済みのコントロールを取得する(id指定)
        ''' </summary>
        ''' <param name="id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GGet(ByVal id As String) As GearsControl
            Return GMediator.GControl(id)
        End Function

        ''' <summary>
        ''' 登録済みのコントロールを取得する(Controlから)
        ''' </summary>
        ''' <param name="con"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GGet(ByVal con As Control) As GearsControl
            If Not con Is Nothing Then
                Return GMediator.GControl(con)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 登録済みのコントロールのデータソースを取得する
        ''' </summary>
        ''' <param name="con"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GSource(ByVal con As Control) As GearsDataSource
            If Not GGet(con) Is Nothing Then
                Return GGet(con).DataSource
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 自動で登録されないコントロールを手動でGearsMediatorに登録する<br/>
        ''' ※データソースが推定されるケースは基本的に自動で行われるため、このメソッドにはisRepaceオプションは付与しない
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="isAutoLoadAttr"></param>
        ''' <remarks></remarks>
        Public Function GAdd(ByVal con As Control, Optional isAutoLoadAttr As Boolean = True) As gRuleExpression
            GMediator.addControl(con, isAutoLoadAttr)
            Return New gRuleExpression(con, GMediator)
        End Function

        ''' <summary>
        ''' 自動で登録されないコントロールを手動でGearsMediatorに登録する(データソース指定)<br/>
        ''' 先行して登録されているコントロールを明示的に入れ替えたい場合は、isReplaceをTrueに設定する
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="ds"></param>
        ''' <param name="isReplace"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GAdd(ByVal con As Control, ByVal ds As GearsDataSource, Optional ByVal isReplace As Boolean = False) As gRuleExpression
            Dim gcon As GearsControl = New GearsControl(con, ds)
            If isReplace Then
                GMediator.replaceControl(gcon)
            Else
                GMediator.addControl(gcon)
            End If
            Return New gRuleExpression(con, GMediator)
        End Function

        ''' <summary>
        ''' 指定されたIDのコントロールを取得する
        ''' </summary>
        ''' <param name="conid"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GFindControl(ByVal conid As String) As Control
            Return ControlSearcher.findControl(Me, conid)
        End Function

        ''' <summary>
        ''' コントロール間のルールを作成する汎用関数
        ''' </summary>
        ''' <param name="fromCon"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GRule(ByVal fromCon As Control) As gRuleExpression
            Return New gRuleExpression(fromCon, GMediator)
        End Function

        ''' <summary>
        ''' フォームを保存する（キーが一致すればUpdate/しなければInsertを行う）
        ''' </summary>
        ''' <param name="form"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GSave(ByVal form As Control) As Boolean
            Dim dto As New GearsDTO(ActionType.SAVE)
            Return GSend(form).ToMyself(dto)
        End Function

        ''' <summary>
        ''' フォームの値でデータベースを更新する
        ''' </summary>
        ''' <param name="form"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GUpdate(ByVal form As Control) As Boolean
            Dim dto As New GearsDTO(ActionType.UPD)
            Return GSend(form).ToMyself(dto)
        End Function

        ''' <summary>
        ''' フォームの値をデータベースに挿入する
        ''' </summary>
        ''' <param name="form"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GInsert(ByVal form As Control) As Boolean
            Dim dto As New GearsDTO(ActionType.INS)
            Return GSend(form).ToMyself(dto)
        End Function

        ''' <summary>
        ''' フォーム内のキー項目の値に合致するレコードをデータベースから削除する
        ''' </summary>
        ''' <param name="form"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GDelete(ByVal form As Control) As Boolean
            Dim dto As New GearsDTO(ActionType.DEL)
            Return GSend(form).ToMyself(dto)
        End Function

        ''' <summary>
        ''' フォーム内のキー項目の値で、データベースからレコードをロードし自身に設定する(フォームのリロード)<br/>
        ''' DTOを指定した場合、そのDTOでロードが行われる。コントロールにセットされた値は使用されないので注意。
        ''' </summary>
        ''' <param name="form"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GLoad(ByVal form As Control, Optional ByVal dto As GearsDTO = Nothing) As Boolean
            'Formコントロール内のキーを使用し、自身の値をリロードする
            Dim loadDto As New GearsDTO(dto)

            If dto Is Nothing Then 'DTOの指定がなかった場合、自身に設定された値を使用する
                Dim sqlb As SqlBuilder = GPack(form, ActionType.SEL).toSqlBuilder
                For Each f As SqlFilterItem In sqlb.Filter
                    loadDto.Filter.Add(f)
                Next
            End If

            Return GSend(loadDto).ToThe(form)

        End Function

        ''' <summary>
        ''' 自身の値で、関連する全てのコントロールに対しフィルタをかける
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GFilterBy(ByVal fromControl As Control, Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Dim fDto As GearsDTO = dto
            If fDto Is Nothing Then fDto = New GearsDTO(ActionType.SEL)
            Return GSend(fromControl).ToAll(fDto)
        End Function

        ''' <summary>
        ''' 自身の値で、指定したコントロールに対しフィルタをかける
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GFilterBy(ByVal fromControl As Control, ByVal toControl As Control, Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Dim fDto As GearsDTO = dto
            If fDto Is Nothing Then fDto = New GearsDTO(ActionType.SEL)
            Return GSend(fromControl).ToThe(toControl, fDto)
        End Function

        ''' <summary>
        ''' DTOの送信処理を記述する
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GSend(ByVal fromControl As Control) As gSendExpression
            Return New gSendExpression(fromControl, AddressOf Me.execute)
        End Function

        ''' <summary>
        ''' DTOの送信処理を記述する
        ''' </summary>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GSend(ByVal dto As GearsDTO) As gSendExpression
            Return New gSendExpression(dto, AddressOf Me.execute)
        End Function

        ''' <summary>
        ''' 自身のコントロール情報を収集しGearsDTOにまとめ、指定したActionをセットする
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="atype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GPack(ByVal fromControl As Control, Optional ByVal atype As ActionType = ActionType.SEL) As GearsDTO
            Dim dto As GearsDTO = New GearsDTO(atype)
            Return GPack(fromControl, Nothing, dto)
        End Function

        ''' <summary>
        ''' 与えられたDTOに自身のコントロール情報を追加する形で、GearsDTOを作成する
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GPack(ByVal fromControl As Control, ByVal dto As GearsDTO) As GearsDTO
            Return GPack(fromControl, Nothing, dto)
        End Function

        ''' <summary>
        ''' 指定した相手先に送るためのDTOを作成する
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="atype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GPack(ByVal fromControl As Control, ByVal toControl As Control, Optional ByVal atype As ActionType = ActionType.SEL) As GearsDTO
            Return GPack(fromControl, toControl, New GearsDTO(atype))
        End Function

        ''' <summary>
        ''' 与えられたDTOを元に指定した相手先に送るためのDTOを作成する
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="fromDto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function GPack(ByVal fromControl As Control, ByVal toControl As Control, ByVal fromDto As GearsDTO) As GearsDTO
            Return GMediator.makeDTO(fromControl, toControl, fromDto)

        End Function

        ''' <summary>
        ''' fromコントロールから与えられたDTOを元にDTOを作成し、toコントロールに送信する<br/>
        ''' </summary>
        ''' <param name="fromControl"></param>
        ''' <param name="toControl"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function execute(ByVal fromControl As Control, ByVal toControl As Control, ByVal dto As GearsDTO) As Boolean

            Dim result As Boolean = True
            GLog.Clear()

            'コントロールの登録チェック
            If fromControl IsNot Nothing AndAlso GMediator.GControl(fromControl) Is Nothing Then
                GearsLogStack.setLog(fromControl.ID + " はまだ登録されていません。GAddで登録する必要があります。")
                Return False
            End If

            '更新系処理の場合のチェック
            If Not dto Is Nothing AndAlso (dto.Action <> ActionType.SEL And dto.Action <> ActionType.NONE) Then
                'リロード
                If IsReload Then
                    GLog.Add(If(fromControl IsNot Nothing, fromControl.ID, "(Send Dto)"), New GearsException("画面のリフレッシュのため、更新処理は実行されません"))
                    dto.Action = ActionType.SEL
                End If

                'バリデーション
                If dto.Action <> ActionType.SEL Then
                    If Not GIsValid(fromControl) Then
                        GearsLogStack.setLog(fromControl.ID + " でのバリデーション処理でエラーが発生しました。")
                        Return False
                    End If
                End If

            End If

            'メイン処理実行
            Dim sender As GearsDTO = New GearsDTO(dto)
            If fromControl Is Nothing Then
                GMediator.lockDtoWhenSend(sender)
            ElseIf sender.Action <> ActionType.SEL Then
                reloadLockValue(fromControl).ForEach(Sub(f) sender.addFilter(f)) '楽観的ロックの選択を追加
            End If

            '警告無視フラグがある場合、その設定を行う処理
            If Not Request.Params(CS_ALERT_IS_IGNORE_FLG) Is Nothing AndAlso Request.Params(CS_ALERT_IS_IGNORE_FLG) = "1" Then
                sender.IsIgnoreAlert = True
            End If

            If fromControl IsNot Nothing Then
                GearsLogStack.setLog(fromControl.ID + " から送信情報を収集しました(DTO作成)。", sender.toString())
                If toControl Is Nothing OrElse fromControl.ID <> toControl.ID Then
                    result = GMediator.send(fromControl, toControl, sender)
                Else
                    result = GMediator.execute(fromControl, sender)
                End If
            Else
                GearsLogStack.setLog(toControl.ID + " への送信情報を収集しました(DTO作成)。", sender.toString())
                result = GMediator.execute(toControl, sender)
            End If

            '実行結果チェック
            If result Then
                GearsLogStack.setLog(If(fromControl IsNot Nothing, fromControl.ID, toControl.ID) + " の更新に成功しました。")
                '更新対象のロード時の値を保持 ※失敗した場合は、再処理のためロード時の値更新は行わない
                saveLoadedValue()
            Else

                '実行結果がNGであった場合、ログをセット
                For Each logitem As KeyValuePair(Of String, GearsException) In GMediator.GLog()
                    If Not GLog.ContainsKey(logitem.Key) Then
                        GLog.Add(logitem.Key, logitem.Value)
                    Else
                        GLog(logitem.Key) = logitem.Value
                    End If
                Next

                'モデルバリデーションの結果を評価する(エラー/警告など)
                result = evalModel(GLog)

            End If

            Return result

        End Function

        ''' <summary>
        ''' 登録済みコントロールのロード時の値をViewStateに保管する
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub saveLoadedValue()

            For Each gcon As KeyValuePair(Of String, GearsControl) In GMediator.GControls
                gcon.Value.LoadedValue = gcon.Value.getValue() '現時点の値をLoadedValueに移す

                saveLoadedValue(gcon.Key)

                If gcon.Value.IsFormAttribute Then
                    'ロック用項目がセットされている場合それも格納
                    saveLockValueIfExist(gcon.Value)
                End If
            Next

        End Sub

        ''' <summary>
        ''' ViewStateに指定されたコントロールの値を保存する
        ''' </summary>
        ''' <param name="conId"></param>
        ''' <remarks></remarks>
        Private Sub saveLoadedValue(ByVal conId As String)
            ViewState(V_LOADED + VIEW_STATE_SEPARATOR + conId) = GMediator.GControl(conId).getValue
        End Sub

        ''' <summary>
        ''' ロードされた値を取得する
        ''' </summary>
        ''' <param name="conId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function reloadLoadedValue(ByVal conId As String) As String
            Return ViewState(V_LOADED + VIEW_STATE_SEPARATOR + conId)
        End Function

        ''' <summary>
        ''' 楽観ロック用の値を取得し、ViewStateに保存する
        ''' </summary>
        ''' <param name="gcon"></param>
        ''' <remarks></remarks>
        Private Sub saveLockValueIfExist(ByVal gcon As GearsControl)
            If Not gcon.DataSource Is Nothing Then
                Dim lockValue As List(Of SqlFilterItem) = gcon.DataSource.getLockValue
                If lockValue.Count > 0 Then
                    For Each item As SqlFilterItem In lockValue
                        ViewState(V_LOCKCOL + VIEW_STATE_SEPARATOR + gcon.ControlID + VIEW_STATE_SEPARATOR + item.Column) = item.Value
                    Next
                End If

            End If
        End Sub

        ''' <summary>
        ''' ViewStateに保存しておいた楽観ロック用の値を取得する
        ''' </summary>
        ''' <param name="con"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function reloadLockValue(ByVal con As Control) As List(Of SqlFilterItem)
            Dim result As New List(Of SqlFilterItem)

            For Each key As String In ViewState.Keys
                If key.StartsWith(V_LOCKCOL + VIEW_STATE_SEPARATOR + con.ID + VIEW_STATE_SEPARATOR) Then
                    Dim keySplit() As String = key.Split(VIEW_STATE_SEPARATOR)
                    If keySplit.Length > 0 Then
                        If Not keySplit(2) Is Nothing Then '1:LOCKCOL/コントロールID/ロックカラム名
                            result.Add(New SqlFilterItem(keySplit(2)).eq(ViewState(key)))
                        End If
                    End If

                End If
            Next

            Return result

        End Function

        ''' <summary>
        ''' ListItemのAttributeが消える対策(save)
        ''' </summary>
        ''' <param name="con"></param>
        ''' <remarks></remarks>
        Private Sub saveListItemAttribute(ByVal con As Control)
            Dim liscon As ListControl
            If Not TypeOf con Is ListControl Then
                Exit Sub
            Else
                liscon = CType(con, ListControl)
            End If

            For Each item As ListItem In liscon.Items
                If item.Attributes.Count > 0 Then
                    For Each key As String In item.Attributes.Keys
                        'コントロールID/リストアイテムのキー/アトリビュートのキー を結合してキーを作成
                        ViewState(V_LIST_ATTRIBUTES + VIEW_STATE_SEPARATOR + con.ID + VIEW_STATE_SEPARATOR + item.Value + VIEW_STATE_SEPARATOR + key) = item.Attributes(key)
                    Next

                End If
            Next
        End Sub

        ''' <summary>
        ''' ListItemのAttributeが消える対策(load)
        ''' </summary>
        ''' <param name="con"></param>
        ''' <remarks></remarks>
        Private Sub loadListItemAttribute(ByVal con As Control)
            Dim liscon As ListControl
            If Not TypeOf con Is ListControl Then
                Exit Sub
            Else
                liscon = CType(con, ListControl)
            End If
            For Each key As String In ViewState.Keys
                If key.StartsWith(V_LIST_ATTRIBUTES + VIEW_STATE_SEPARATOR + con.ID + VIEW_STATE_SEPARATOR) Then 'リストアイテムのメンバ
                    Dim keySplit() As String = key.Split(VIEW_STATE_SEPARATOR)
                    If keySplit.Length > 0 Then
                        If Not liscon.Items.FindByValue(keySplit(2)) Is Nothing Then
                            liscon.Items.FindByValue(keySplit(2)).Attributes.Add(keySplit(3), ViewState(key))
                        End If
                    End If

                End If
            Next
        End Sub

        ''' <summary>
        ''' 登録されたコントロールのAttributeに基づき、バリデーション処理を実行する
        ''' </summary>
        ''' <param name="con"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GIsValid(Optional ByVal con As Control = Nothing) As Boolean
            Dim result As Boolean = True
            Dim target As Control = con
            GLog.Clear()

            If con Is Nothing Then
                target = Me
            End If

            'targetが子コントロールを保持している場合、子コントロールについてもバリデーションを行う
            If Not target Is Nothing AndAlso target.HasControls Then
                ControlSearcher.fetchControls(target, AddressOf Me.fetchEachControlValidate, AddressOf GMediator.isRegisteredControl)
            ElseIf GMediator.isRegisteredControl(target) Then
                fetchEachControlValidate(target, Nothing)
            End If

            If GLog.Count > 0 Then
                result = False
                'エラー項目にスタイル適用
                For Each item As KeyValuePair(Of String, GearsException) In GLog
                    If Not GMediator.GControl(item.Key) Is Nothing AndAlso _
                        TypeOf GMediator.GControl(item.Key).Control Is WebControl AndAlso _
                        TypeOf item.Value Is GearsValidationException Then

                        Dim wcon As WebControl = CType(GMediator.GControl(item.Key).Control, WebControl)
                        wcon.CssClass += " " + GearsAttribute.ERR_STYLE
                        Exit For '一件発見したら抜ける(エラーメッセージがそもそもそんなに表示できないので)
                    End If
                Next
            End If

            Return result

        End Function

        ''' <summary>
        ''' 登録済みの各コントロールに対しバリデーションを行う
        ''' </summary>
        ''' <param name="control"></param>
        ''' <param name="dto"></param>
        ''' <remarks></remarks>
        Private Sub fetchEachControlValidate(ByVal control As Control, ByRef dto As GearsDTO)
            If Not GMediator.GControl(control.ID) Is Nothing Then
                Dim gcon As GearsControl = GMediator.GControl(control.ID)
                If Not gcon.isValidateOk() Then
                    GLog.Add(control.ID, New GearsValidationException(gcon.getValidationError))
                End If
            End If

        End Sub

        ''' <summary>
        ''' モデルバリデーションの結果を評価する
        ''' </summary>
        ''' <param name="logs"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function evalModel(ByVal logs As Dictionary(Of String, GearsException)) As Boolean
            Dim result As Boolean = True

            'モデルバリデーションの検証
            Dim mv As GearsModelValidationException _
                = (From ex As KeyValuePair(Of String, GearsException) In logs
                        Where TypeOf ex.Value Is GearsModelValidationException
                        Select ex.Value).FirstOrDefault

            If Not mv Is Nothing Then 'モデルバリデーションでエラーが出ている場合

                If mv.Result.IsValidIgnoreAlert Then '警告のケース
                    Dim cs As ClientScriptManager = Me.ClientScript

                    '警告表示
                    If (Not cs.IsStartupScriptRegistered(Me.GetType, CS_ALERT_PROMPT_SCRIPT_NAME)) Then
                        Dim isAsync As Boolean = IsPageAsync()

                        Dim handler As Control = ControlSearcher.GetSubmitCausedControl(Me)

                        Dim alertMsg As String = "警告\n" + mv.Message.Replace(vbCrLf, "\n")
                        Dim scriptTag As New StringBuilder()
                        Dim unlock As String = " if(typeof gears.fn.unlock == 'function' ){ gears.fn.unlock(); } "
                        Dim releaseHidden As String = " gears.fn.removeElementById('" + CS_ALERT_IS_IGNORE_FLG + "'); "
                        scriptTag.Append("<script>")
                        scriptTag.Append("if(typeof gears.fn.lock == 'function' ){ gears.fn.lock(%TGT%); }")
                        scriptTag.Append("if(window.confirm('" + alertMsg + "')){")
                        scriptTag.Append(" document.getElementById('" + CS_ALERT_IS_IGNORE_FLG + "').value = '1'; ")
                        scriptTag.Append(cs.GetPostBackEventReference(handler, "") + ";")
                        scriptTag.Append(releaseHidden)
                        scriptTag.Append("}else{")
                        scriptTag.Append(unlock + releaseHidden)
                        scriptTag.Append("}")

                        scriptTag.Append("</script>")

                        If Not isAsync Then
                            cs.RegisterHiddenField(CS_ALERT_IS_IGNORE_FLG, "0")
                            cs.RegisterStartupScript(Me.GetType, CS_ALERT_PROMPT_SCRIPT_NAME, Replace(scriptTag.ToString, "%TGT%", ""), False)
                        Else
                            Dim con As Control = ControlSearcher.GetAsynchronousPostBackPanel(Me, handler)
                            If Not con Is Nothing Then
                                ScriptManager.RegisterHiddenField(con, CS_ALERT_IS_IGNORE_FLG, "0")
                                ScriptManager.RegisterStartupScript(con, con.GetType, CS_ALERT_PROMPT_SCRIPT_NAME, Replace(scriptTag.ToString, "%TGT%", "'" + con.ID + "'"), False)
                            End If

                        End If
                    End If
                    '警告のため、Logにエラーはあるが結果はTrueとする
                    result = True
                Else
                    result = False
                End If

                'エラー対象コントロールにスタイルを適用
                'TODO エラースタイルの付与/解除を簡単にできるよう関数化。エラースタイルの解除は初期化処理setUpPageControlsに依存しているため、要工夫
                Dim eSource As WebControl = (From con As KeyValuePair(Of String, GearsControl) In GMediator.GControls
                                             Where con.Value.DataSourceID = mv.Result.ErrorSource And TypeOf con.Value.Control Is WebControl
                                             Select con.Value.Control).FirstOrDefault

                If Not eSource Is Nothing Then
                    eSource.CssClass += " " + GearsAttribute.ERR_STYLE
                End If

            Else
                result = False 'エラー
            End If


            Return result

        End Function

        ''' <summary>
        ''' 与えられたControl領域に対し、権限の評価を行う
        ''' </summary>
        ''' <param name="con"></param>
        ''' <remarks></remarks>
        Public Sub GIsAuth(ByVal con As Control)
            'ロールベースコントロールについて、指定ロール以外の場合コントロールを非表示/非活性化
            ControlSearcher.fetchControls(con, AddressOf Me.fetchRoleBaseControl, AddressOf Me.isRoleBaseControl)

        End Sub

        ''' <summary>
        ''' 権限による表示/非表示などの切り替えを行う
        ''' </summary>
        ''' <param name="control"></param>
        ''' <param name="dto"></param>
        ''' <remarks></remarks>
        Private Sub fetchRoleBaseControl(ByVal control As Control, ByRef dto As GearsDTO)
            'ロールベースコントロールと判定されて来る
            Dim result As Boolean = False
            Dim evalType As String = "ENABLE"
            If TypeOf control Is WebControl Then
                Dim wControl = CType(control, WebControl)
                'ユーザーが指定ロールを保持しているかどうか判定する

                '許可されたロール
                Dim allowedRoleString As String = wControl.Attributes(A_ROLE_AUTH_ALLOW)
                Dim allowedRole As New List(Of String)(Split(allowedRoleString, ","))

                '保持しているロールを取得(User.IsInRoleだと逐一DBアクセスになる気がするのでそれを回避)
                Dim havingRole As New List(Of String)(Roles.GetRolesForUser(User.Identity.Name))

                '許可されたロールを保持しているか確認
                If allowedRole.Count > 0 Then
                    For Each role As String In allowedRole
                        If havingRole.IndexOf(role) > -1 Then
                            result = True
                            Exit For
                        End If
                    Next
                Else
                    result = True '制限無し
                End If

                'コントロールへの設定反映
                Dim evalTypeAttr = GearsControl.getControlAttribute(control, A_ROLE_EVAL_ACTION)
                If Not String.IsNullOrEmpty(evalTypeAttr) Then
                    evalType = evalTypeAttr.ToUpper
                End If

                Select Case evalType
                    Case "ENABLE"
                        wControl.Enabled = result
                    Case "VISIBLE"
                        wControl.Visible = result
                End Select

            End If

        End Sub

        ''' <summary>
        ''' ロール管理コントロールか否かを判定する
        ''' </summary>
        ''' <param name="control"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function isRoleBaseControl(ByVal control As Control) As Boolean
            Return Not String.IsNullOrEmpty(GearsControl.getControlAttribute(control, A_ROLE_AUTH_ALLOW))
        End Function

        ''' <summary>
        ''' エラーログをラベルに設定する
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="label"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function LogToLabel(ByVal result As Boolean, ByRef label As Label) As Boolean

            Dim emsg As String = ""
            If GLog.Count > 0 Then
                emsg = GLog.FirstLog.Message
                If IsLoggingMode() Then 'ログモードの場合詳細を出力
                    If Not GLog.FirstLog.InnerException Is Nothing Then
                        emsg += " (" + GLog.FirstLog.InnerException.Message + ")"
                    ElseIf Not String.IsNullOrEmpty(GLog.FirstLog.MessageDetail()) Then
                        emsg += " (" + GLog.FirstLog.MessageDetail() + ")"
                    End If
                End If
            End If
            Return LogToLabel(result, "処理は正常に行われました", emsg, label)

        End Function

        ''' <summary>
        ''' 指定されたラベルに対し、メッセージをセットします<br/>
        ''' labelのCssClassが、resultに応じ g-msg-success か g-msg-errorに変化します(それ以外のCssClassを指定していた場合はそれが消えることはありません)<br/>
        ''' result=trueの場合は引数で指定されたsuccessMsgがセットされます。<br/>
        ''' result=falseの場合は発生したエラーがGLogから取得され、そのメッセージがセットされます。<br/>
        ''' </summary>
        ''' <param name="label">メッセージをセットするラベル</param>
        ''' <param name="successMsg">成功時メッセージ</param>
        ''' <param name="result">処理結果のBoolean</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function LogToLabel(ByVal result As Boolean, ByVal successMsg As String, ByVal errorMsg As String, ByRef label As Label) As Boolean
            Dim css As String = label.CssClass

            '既存スタイルを消去
            css = RegularExpressions.Regex.Replace(css, "g-msg-success|g-msg-error|g-msg-warning", "")
            label.CssClass = Trim(css)

            'resultに応じメッセージをセット
            If result Then
                If GLog.Count = 0 Then
                    label.Text = successMsg
                    label.CssClass += " g-msg-success"
                Else
                    label.Text = errorMsg
                    label.CssClass += " g-msg-warning"
                End If
            Else
                label.Text = errorMsg
                label.CssClass += " g-msg-error"
            End If

            Return result

        End Function

        ''' <summary>
        ''' サーバーにインストールされたCultureとCurrentCultureが異なる場合、resourceKeyを使用しリソースファイルから値を取得する。<br/>
        ''' seeGlobal=Trueの場合、まずグローバルリソースを参照しなければローカルリソースを参照する。
        ''' </summary>
        ''' <param name="message"></param>
        ''' <param name="resourceKey"></param>
        ''' <param name="seeGlobal"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GGlobalize(ByVal message As String, ByVal resourceKey As String, Optional ByVal seeGlobal As Boolean = True) As Object
            Dim serverCulture As Globalization.CultureInfo = CultureInfo.InstalledUICulture
            Dim currentCulture As Globalization.CultureInfo = CultureInfo.CurrentCulture

            If serverCulture.Equals(currentCulture) Then
                Return message
            Else
                If seeGlobal Then
                    Return GetResource(resourceKey)
                Else
                    Return GetLocalResourceObject(resourceKey)
                End If
            End If

        End Function

        ''' <summary>
        ''' 与えられたリソースキーで、まずローカルリソースを参照しなければグローバルリソースを参照する。
        ''' </summary>
        ''' <param name="resourceKey"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetResource(ByVal resourceKey As String) As Object
            Dim resource As Object = GetLocalResourceObject(resourceKey)
            If resource Is Nothing Then
                resource = GetGlobalResourceObject(GearsGlobalResource, resourceKey)
            End If

            Return resource

        End Function

        ''' <summary>
        ''' クエリ引数を取得する
        ''' </summary>
        ''' <param name="keyname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function QueryValue(ByVal keyname As String) As String
            Dim value As String = HttpUtility.UrlDecode(Page.Request.QueryString.Get(keyname))
            If value Is Nothing Then
                Return ""
            Else
                Return value
            End If
        End Function

        ''' <summary>
        ''' クエリ引数の値をDictionary型で取得する
        ''' </summary>
        ''' <param name="isIgnoreBlank">空白の値を取得するか否か</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function QueryToDictionary(Optional ByVal isIgnoreBlank As Boolean = True) As Dictionary(Of String, List(Of String))
            Dim result As New Dictionary(Of String, List(Of String))

            For Each key As String In Page.Request.QueryString.Keys
                Dim values As String() = Page.Request.QueryString.GetValues(key)

                If Not values Is Nothing Then
                    Dim list As New List(Of String)
                    For Each i As String In values
                        If Not (isIgnoreBlank And String.IsNullOrEmpty(i)) Then
                            list.Add(HttpUtility.UrlDecode(i))
                        End If
                    Next
                    If list.Count > 0 Then
                        result.Add(key, list)
                    End If
                End If

            Next

            Return result

        End Function

        ''' <summary>
        ''' 親ページのプロパティを取得する
        ''' </summary>
        ''' <param name="propName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function MasterProperty(ByVal propName As String) As Object
            Dim masterType As Type = Master.GetType
            Dim mp As PropertyInfo = masterType.GetProperty(propName)

            If Not mp Is Nothing Then
                Return mp.GetValue(Master, Nothing)
            Else
                Return Nothing
            End If

        End Function

        ''' <summary>
        ''' GearsJsAttributeが指定されたプロパティを、ページのscriptブロックに書き込む<br/>
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub JsVariableWrite()

            Dim jsProps = From p As PropertyInfo In Me.GetType.GetProperties
                          Let jsAttr As JSVariableAttribute = Attribute.GetCustomAttribute(p, GetType(JSVariableAttribute))
                          Where jsAttr IsNot Nothing
                          Select p, jsAttr

            Dim cs As ClientScriptManager = Me.ClientScript

            If (Not cs.IsClientScriptBlockRegistered(Me.GetType, CS_JS_VARIABLE_BLOCK)) Then
                Dim script As New StringBuilder
                Dim vars As New Dictionary(Of String, String)
                Dim serializer As New Web.Script.Serialization.JavaScriptSerializer
                For Each item In jsProps
                    Dim name As String = If(String.IsNullOrEmpty(item.jsAttr.Name), item.p.Name, item.jsAttr.Name)
                    Dim value As String = serializer.Serialize(item.p.GetValue(Me, Nothing))
                    vars.Add(name, value)
                Next

                If vars.Count > 0 Then
                    script.Append("var gears; " + vbCrLf)
                    script.Append("if (typeof gears == ""undefined"") { gears = {}; } " + vbCrLf)
                    script.Append("if (typeof gears.v == ""undefined"") { gears.v = {}; } " + vbCrLf)
                    script.Append("(function(){ var GV = gears.v;" + vbCrLf)

                    For Each item As KeyValuePair(Of String, String) In vars
                        script.Append(" GV." + item.Key + " = " + item.Value + ";")
                    Next

                    script.Append("})() ")

                    cs.RegisterClientScriptBlock(Me.GetType, CS_JS_VARIABLE_BLOCK, script.ToString, True)

                End If
            End If

        End Sub

    End Class

End Namespace
