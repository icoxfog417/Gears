Imports System
Imports System.Collections.Generic
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports Microsoft.VisualBasic
Imports System.Web
Imports System.Web.Security
Imports System.Text
Imports System.Reflection

Namespace Gears

    ''' <summary>
    ''' Gearsフレームワークを使用する場合の、継承元となるページ
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class GearsPage
        Inherits Page


        ''' <summary>
        ''' 使用するConnectionNameを設定するためのConfig<br/>
        ''' Master.xxxと設定することで、Masterページのプロパティに設定した値を参照させることができる<br/>
        ''' 例:Master.ConnectionNameと設定した場合、MasterページのConnectionNameプロパティの値が参照される
        ''' </summary>
        Public Const CONFIG_CONNECTION_NAME As String = "GearsConnection"

        ''' <summary>
        ''' 使用する名称空間を設定するためのConfig<br/>
        ''' Master.xxxと設定することで、Masterページのプロパティに設定した値を参照させることができる<br/>
        ''' </summary>
        Public Const CONFIG_DSNAMESPACE As String = "GearsDsNameSpace"

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
        Public Const ROLE_AUTH_ALLOW As String = "AUTHORIZATIONALLOW"

        ''' <summary>権限の有無によって切り替える属性(VISIBLE/ENABLEなど)</summary>
        Public Const ROLE_EVAL_ACTION As String = "ROLEEVALACTION"

        ''' <summary>
        ''' ログ出力を行うか否かの引数指定<br/>
        ''' gs_log_out=trueを設定することでログの出力が可能
        ''' </summary>
        Public Const GEARS_IS_LOG_OUT As String = "gs_log_out"

        ''' <summary>警告のプロンプトを出すためのスクリプト名</summary>
        Const ALERT_PROMPT_SCRIPT_NAME As String = "GearsAlertPromptScript"

        ''' <summary>警告を無視するか否かを設定したhiddenフィールド</summary>
        Const ALERT_IS_IGNORE_FLG As String = "GearsYesAlertIgnore"

        ''' <summary>ViewStateに値を設定する際の区切り文字</summary>
        Protected Const VIEW_STATE_SEPARATOR As String = "/"

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
        Protected GPageMediator As GearsMediator = Nothing

        ''' <summary>ログ</summary>
        Protected Log As New Dictionary(Of String, GearsException)

        ''' <summary>
        ''' コントロールの管理を行うGearsMediatorを初期化する<br/>
        ''' 個別に呼び出す場合はGPageInitをオーバーライドし、その中で呼び出すこと。
        ''' </summary>
        ''' <param name="conName"></param>
        ''' <param name="dns"></param>
        ''' <remarks></remarks>
        Protected Sub initMediator(ByVal conName As String, Optional ByVal dns As String = "")
            Dim pageConName As String = conName
            Dim pageDsNspace As String = dns
            Dim readProperty = Function(p As String) As String
                                   Dim result As String = p
                                   'マスターページプロパティを参照するものは、その値を取得
                                   If Not String.IsNullOrEmpty(p) AndAlso p.StartsWith("Master.") Then
                                       Dim prop As String = getMasterProperty(Replace(p, "Master.", ""))
                                       If Not prop Is Nothing Then
                                           result = prop.ToString
                                       End If
                                   End If
                                   Return result
                               End Function

            'Config設定を読み込む
            Dim conInfo As String = Trim(ConfigurationManager.AppSettings(CONFIG_CONNECTION_NAME))
            Dim dnsInfo As String = Trim(ConfigurationManager.AppSettings(CONFIG_DSNAMESPACE))

            '設定
            If String.IsNullOrEmpty(pageConName) Then pageConName = conInfo
            pageConName = readProperty(pageConName)

            If String.IsNullOrEmpty(pageDsNspace) Then pageDsNspace = dnsInfo
            pageDsNspace = readProperty(pageDsNspace)

            '未作成か、設定値が異なる場合更新
            If GPageMediator Is Nothing OrElse (GPageMediator.ConnectionName <> pageConName Or GPageMediator.DsNameSpace <> pageDsNspace) Then
                GPageMediator = New GearsMediator(pageConName, pageDsNspace)
            End If

        End Sub

        ''' <summary>
        ''' ページの初期化処理を行う<br/>
        ''' Overrideを行うことで、GearsMediator登録前(コントロール処理前)にカスタム処理を挿入することができる
        ''' </summary>
        ''' <remarks></remarks>
        Protected Overridable Sub GPageInit()
            initMediator(String.Empty, String.Empty)
        End Sub

        ''' <summary>
        ''' ページ初期化イベント<br/>
        ''' GPageInit(Override可)の実行を行う
        ''' </summary>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Protected Overrides Sub OnInit(ByVal e As System.EventArgs)
            GPageInit() 'GearsMediatorのインスタンス化を最優先で行う

            MyBase.OnInit(e)

            evaluateAutorization(Me) 'コントロールロード後、権限評価を実施する

        End Sub

        ''' <summary>
        ''' 与えられたControl領域について、権限の評価を行う
        ''' </summary>
        ''' <param name="con"></param>
        ''' <remarks></remarks>
        Public Sub evaluateAutorization(ByRef con As Control)
            'ロールベースコントロールについて、指定ロール以外の場合コントロールを非表示/非活性化
            GPageMediator.fetchControls(con, AddressOf Me.fetchRoleBaseControl, AddressOf Me.isRoleBaseControl)

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
                setIsReload()
            End If

            'コントロールへのデータ/アトリビュートのセット
            setUpPageControls()

            'ログオンの場合トレース開始
            If isLoggingMode() Then
                GearsLogStack.traceOn()
            End If

        End Sub

        Protected Overrides Sub OnLoadComplete(e As System.EventArgs)
            MyBase.OnLoadComplete(e)

            If isLoggingMode() Then
                If Me.FindControl(GEARS_IS_LOG_OUT) Is Nothing Then
                    Dim label As New Label
                    label.Text = GearsLogStack.makeDisplayString
                    Me.Controls.Add(label)
                Else
                    CType(Me.FindControl(GEARS_IS_LOG_OUT), Label).Text = GearsLogStack.makeDisplayString
                End If
                GearsLogStack.traceEnd()

            ElseIf Not Me.FindControl(GEARS_IS_LOG_OUT) Is Nothing Then
                Me.Controls.Remove(Me.FindControl(GEARS_IS_LOG_OUT))
                GearsLogStack.traceEnd()
            End If


        End Sub

        Private Sub setIsReload()

            Response.Cache.SetCacheability(HttpCacheability.NoCache)
            Response.Cache.SetNoStore() 'IE以外の場合、nostoreも使用しないと駄目な模様 http://d.hatena.ne.jp/manymanytips/20110120/1295500136

            Dim dateNow As DateTime = Now
            Dim isAsync As Boolean = ScriptManager.GetCurrent(Me).IsInAsyncPostBack()
            'ポストバック時、[F5]での二重登録防止判定 ※Asyncの場合はリロードがあり得ないため、判定を行わない
            If IsPostBack And Not isAsync Then
                'タイムスタンプが設定されていない場合(一度もNot PostBackでページがロードされていない場合=ポストバックで遷移してき)
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
            If Not isAsync Then
                Session(Request.FilePath & V_S_TIME_STAMP) = dateNow
                ViewState(Request.FilePath & V_S_TIME_STAMP) = dateNow
            End If

        End Sub

        Protected Sub setUpPageControls()

            If GPageMediator Is Nothing Then
                Throw New GearsException("ページの初期化が行われていません", "initMediatorを呼び出し接続文字列・名称空間を設定してください")
            End If

            'フォームコントロールの登録処理
            'Gearsフレームワークへのコントロールの登録
            GPageMediator.addGControls(Me, isAutoLoadAttr:=False)

            Dim initialCss As Dictionary(Of String, String) = CType(ViewState(V_VALIDATORS), Dictionary(Of String, String))
            If initialCss Is Nothing Then
                initialCss = New Dictionary(Of String, String)
            End If

            For Each item As KeyValuePair(Of String, GearsControl) In GPageMediator.GControls
                If GPageMediator.isTargetControl(item.Value.Control) Then

                    '前回ロード時の値がある場合、過去ロード値にセット
                    If Not reloadLoadedValue(item.Key) Is Nothing Then
                        item.Value.LoadedValue = reloadLoadedValue(item.Key)
                    End If

                    'データロード
                    If Not IsPostBack Then
                        '初回であれば、設定されたデータソースから値をロードする。
                        item.Value.dataBind()

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

                End If
            Next

            '初期CSSを保管
            If ViewState(V_VALIDATORS) Is Nothing Then
                ViewState(V_VALIDATORS) = initialCss
            End If

        End Sub

        Private Sub reloadControlData()

            For Each item As KeyValuePair(Of String, GearsControl) In GPageMediator.GControls
                If GPageMediator.isTargetControl(item.Value.Control) Then

                    'データロード
                    If IsPostBack Then
                        'Postbackの場合、基本はViewStateが担保されているのでロードは行わないが、
                        'ViewStateEnableがFalseでデータが消える場合、リロードをかける
                        If item.Value.Control.EnableViewState = False Then
                            item.Value.dataBind()
                        End If
                    End If
                End If
            Next

        End Sub

        Public Function isValidateOk(Optional ByVal con As Control = Nothing) As Boolean
            Dim result As Boolean = True
            Dim target As Control = Nothing
            Log.Clear()

            If con Is Nothing Then
                target = Me
            Else
                target = con
            End If

            If Not target Is Nothing AndAlso target.HasControls Then
                GPageMediator.fetchControls(target, AddressOf Me.fetchEachControlValidate, AddressOf GPageMediator.isRegisteredControl)
            ElseIf GPageMediator.isRegisteredControl(target) Then
                fetchEachControlValidate(target, Nothing)
            End If

            If Log.Count > 0 Then
                result = False
                'エラー項目にスタイル適用
                For Each item As KeyValuePair(Of String, GearsException) In Log
                    If Not GPageMediator.GControl(item.Key) Is Nothing AndAlso TypeOf GPageMediator.GControl(item.Key).Control Is WebControl AndAlso _
                        TypeOf item.Value Is GearsDataValidationException Then

                        Dim wcon As WebControl = CType(GPageMediator.GControl(item.Key).Control, WebControl)
                        wcon.CssClass += " " + GearsAttribute.ERR_STYLE
                        Exit For '一件発見したら抜ける(とりあえず)
                    End If
                Next
            End If

            Return result

        End Function
        Private Sub fetchEachControlValidate(ByRef control As Control, ByRef dto As GearsDTO)
            If Not GPageMediator.GControl(control.ID) Is Nothing Then
                Dim gcon As GearsControl = GPageMediator.GControl(control.ID)
                If Not gcon.isValidateOk() Then
                    Log.Add(control.ID, New GearsDataValidationException(gcon.getValidatedMsg))
                End If
            End If
        End Sub

        Public Function executeBehavior(ByRef fromControl As Control, Optional ByVal atype As ActionType = ActionType.SEL) As Boolean
            Dim dto As GearsDTO = New GearsDTO(atype)
            Return executeBehavior(fromControl, Nothing, dto)
        End Function
        Public Function executeBehavior(ByRef fromControl As Control, ByRef toControl As Control, Optional ByVal atype As ActionType = ActionType.SEL) As Boolean
            Dim dto As GearsDTO = New GearsDTO(atype)
            Return executeBehavior(fromControl, toControl, dto)
        End Function
        Public Function executeBehavior(ByRef fromControl As Control, ByRef dto As GearsDTO, Optional ByVal isGatherInfo As Boolean = True) As Boolean
            Return executeBehavior(fromControl, Nothing, dto, isGatherInfo)
        End Function
        Public Overridable Function executeBehavior(ByRef fromControl As Control, ByRef toControl As Control, ByRef dto As GearsDTO, Optional ByVal isGatherInfo As Boolean = True) As Boolean
            Dim result As Boolean = True
            Log.Clear()
            If Not dto Is Nothing AndAlso _
                  (dto.Action <> ActionType.SEL And dto.Action <> ActionType.NONE) Then
                If IsReload Then
                    Log.Add(fromControl.ID, New GearsException("画面のリフレッシュのみ行いました。更新処理は実行されていません。"))
                    dto.Action = ActionType.SEL
                End If
            End If

            'コントロールの登録チェック
            If Not GPageMediator.isRegisteredControl(fromControl) Then
                GearsLogStack.setLog(fromControl.ID + " はまだMyControlとして登録されていません。registerMyControlで登録する必要があります。")
                Return False
            End If

            'バリデーションチェック 
            If dto.Action <> ActionType.SEL Then '更新系の場合、バリデーション処理を実行
                If Not isValidateOk(fromControl) Then
                    GearsLogStack.setLog(fromControl.ID + " でのバリデーション処理でエラーが発生しました。")
                    Return False
                End If
            End If

            'メイン処理実行
            Dim sender As GearsDTO = Nothing

            If isGatherInfo Then
                sender = GPageMediator.makeSendMessage(fromControl, toControl, dto)
                sender.addLockItem(reloadLockValue(fromControl)) '楽観的ロックのチェックを追加
            Else
                sender = dto
            End If

            '警告無視フラグがある場合、その設定を行う処理
            If Not Request.Params(ALERT_IS_IGNORE_FLG) Is Nothing AndAlso Request.Params(ALERT_IS_IGNORE_FLG) = "1" Then
                sender.IsIgnoreAlert = True
            End If

            GearsLogStack.setLog(fromControl.ID + " の送信情報を収集しました(DTO作成)。", sender.toString())

            result = GPageMediator.executeBehavior(fromControl, toControl, sender, False) '自身で作っているためここではFalse

            '実行結果チェック
            If Not result Then
                For Each logitem As KeyValuePair(Of String, GearsException) In GPageMediator.getLog()
                    If Not Log.ContainsKey(logitem.Key) Then
                        Log.Add(logitem.Key, logitem.Value)
                    Else
                        Log(logitem.Key) = logitem.Value
                    End If
                Next

                result = evalModelValidation(Log)

            Else
                GearsLogStack.setLog(fromControl.ID + " での更新に成功しました。")
                '更新対象のロード時の値を保持 ※失敗した場合は、再処理のためロード時の値更新は行わない
                saveLoadedValue(fromControl, toControl)
            End If

            '更新系処理の場合、データの更新によるドロップダウンリストの項目増減がありうるため、実行(成否に関わらない)
            If dto.Action <> ActionType.SEL Then
                reloadControlData()
            End If

            Return result

        End Function

        Private Function evalModelValidation(ByRef logs As Dictionary(Of String, GearsException)) As Boolean
            Dim result As Boolean = True

            'モデルバリデーションの検証
            Dim mv As GearsModelValidationException _
                = (From ex As KeyValuePair(Of String, GearsException) In logs
                        Where TypeOf ex.Value Is GearsModelValidationException
                        Select ex.Value).FirstOrDefault

            If Not mv Is Nothing Then 'エラーなしだが警告有り

                If mv.Result.IsValidIgnoreAlert Then '警告のケース
                    Dim cs As ClientScriptManager = Me.ClientScript

                    '警告表示
                    If (Not cs.IsStartupScriptRegistered(Me.GetType, ALERT_PROMPT_SCRIPT_NAME)) Then
                        Dim isAsync As Boolean = ScriptManager.GetCurrent(Me).IsInAsyncPostBack

                        Dim handler As Control = GetSubmitCauseControl()

                        Dim alertMsg As String = "警告\n" + mv.Message.Replace(vbCrLf, "\n")
                        Dim scriptTag As New StringBuilder()
                        Dim unlock As String = " if(typeof gears.fn.unlock == 'function' ){ gears.fn.unlock(); } "
                        Dim releaseHidden As String = " gears.fn.removeElementById('" + ALERT_IS_IGNORE_FLG + "'); "
                        scriptTag.Append("<script>")
                        scriptTag.Append("if(typeof gears.fn.lock == 'function' ){ gears.fn.lock(%TGT%); }")
                        scriptTag.Append("if(window.confirm('" + alertMsg + "')){")
                        scriptTag.Append(" document.getElementById('" + ALERT_IS_IGNORE_FLG + "').value = '1'; ")
                        scriptTag.Append(cs.GetPostBackEventReference(handler, "") + ";")
                        scriptTag.Append(releaseHidden)
                        scriptTag.Append("}else{")
                        scriptTag.Append(unlock + releaseHidden)
                        scriptTag.Append("}")

                        scriptTag.Append("</script>")

                        If Not isAsync Then
                            cs.RegisterHiddenField(ALERT_IS_IGNORE_FLG, "0")
                            cs.RegisterStartupScript(Me.GetType, ALERT_PROMPT_SCRIPT_NAME, Replace(scriptTag.ToString, "%TGT%", ""), False)
                        Else
                            Dim con As Control = GetAsynchronousPostBackPanel(handler)
                            If Not con Is Nothing Then
                                ScriptManager.RegisterHiddenField(con, ALERT_IS_IGNORE_FLG, "0")
                                ScriptManager.RegisterStartupScript(con, con.GetType, ALERT_PROMPT_SCRIPT_NAME, Replace(scriptTag.ToString, "%TGT%", "'" + con.ID + "'"), False)
                            End If

                        End If
                    End If
                    '警告のため、Logにエラーはあるが結果はTrueとする
                    result = True
                Else
                    result = False
                End If

                'エラー対象コントロールにスタイルを適用
                Dim eSource As WebControl = (From con As KeyValuePair(Of String, GearsControl) In GPageMediator.GControls
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

        Public Function makeSendMessage(ByRef fromControl As Control, Optional ByVal atype As ActionType = ActionType.SEL) As GearsDTO
            Dim dto As GearsDTO = New GearsDTO(atype)
            Return makeSendMessage(fromControl, Nothing, dto)
        End Function
        Public Function makeSendMessage(ByRef fromControl As Control, ByRef dto As GearsDTO) As GearsDTO
            Return makeSendMessage(fromControl, Nothing, dto)
        End Function
        Public Function makeSendMessage(ByRef fromControl As Control, ByRef toControl As Control, Optional ByVal atype As ActionType = ActionType.SEL) As GearsDTO
            Return makeSendMessage(fromControl, toControl, New GearsDTO(atype))
        End Function
        Public Overridable Function makeSendMessage(ByRef fromControl As Control, ByRef toControl As Control, ByRef fromDto As GearsDTO) As GearsDTO
            Return GPageMediator.makeSendMessage(fromControl, toControl, fromDto)

        End Function
        'ロード時のデータ確保
        Protected Sub saveLoadedValue(ByRef fromControl As Control, ByRef toControl As Control)

            Dim list As List(Of GearsControl) = GPageMediator.extractRelation(fromControl.ID)
            If Not list Is Nothing Then
                For Each gcon As GearsControl In list
                    If Not toControl Is Nothing AndAlso toControl.ID <> gcon.ControlID Then
                        Continue For 'toControlの指定がある場合、それ以外は処理をスキップする
                    End If

                    If gcon.IsFormAttribute Then '対象項目の場合
                        'ロードした値を格納
                        GPageMediator.fetchControls(gcon.Control, AddressOf Me.fetchLoadedValue, AddressOf GPageMediator.isRegisteredAsTarget)
                        'ロック用項目がセットされている場合それも格納
                        saveLockValueIfExist(gcon)
                    End If
                Next
            End If

        End Sub
        Protected Sub fetchLoadedValue(ByRef control As Control, ByRef dto As GearsDTO)
            saveLoadedValue(control.ID)
        End Sub
        Protected Sub saveLoadedValue(ByVal conId As String)
            ViewState(V_LOADED + VIEW_STATE_SEPARATOR + conId) = GPageMediator.GControl(conId).getValue
        End Sub
        Protected Function reloadLoadedValue(ByVal conId As String) As String
            Return ViewState(V_LOADED + VIEW_STATE_SEPARATOR + conId)
        End Function

        'ロックチェック用カラムのデータ確保(GearsControlのデータソースを直接参照)
        Protected Sub saveLockValueIfExist(ByRef gcon As GearsControl)
            If Not gcon.DataSource Is Nothing Then
                If gcon.DataSource.getLockCheckColCount > 0 Then
                    Dim lockParams As Dictionary(Of String, Object) = gcon.DataSource.getLockedCheckColValue
                    For Each item As KeyValuePair(Of String, Object) In lockParams
                        ViewState(V_LOCKCOL + VIEW_STATE_SEPARATOR + gcon.ControlID + VIEW_STATE_SEPARATOR + item.Key) = item.Value
                    Next
                End If

            End If
        End Sub
        Protected Function reloadLockValue(ByRef con As Control) As Dictionary(Of String, Object)
            Dim result As New Dictionary(Of String, Object)

            For Each key As String In ViewState.Keys
                If key.StartsWith(V_LOCKCOL + VIEW_STATE_SEPARATOR + con.ID + VIEW_STATE_SEPARATOR) Then
                    Dim keySplit() As String = key.Split(VIEW_STATE_SEPARATOR)
                    If keySplit.Length > 0 Then
                        If Not keySplit(2) Is Nothing Then '1:LOCKCOL/コントロールID/ロックカラム名
                            result.Add(keySplit(2), ViewState(key))
                        End If
                    End If

                End If
            Next

            Return result

        End Function

        'ロールによってコントロールの活性/非活性を切り替える
        Private Sub fetchRoleBaseControl(ByRef control As Control, ByRef dto As GearsDTO)
            'ロールベースコントロールと判定されて来る
            Dim result As Boolean = False
            Dim evalType As String = "ENABLE"
            If TypeOf control Is WebControl Then
                Dim wControl = CType(control, WebControl)
                'ユーザーが指定ロールを保持しているかどうか判定する

                '許可されたロール
                Dim allowedRoleString As String = wControl.Attributes(ROLE_AUTH_ALLOW)
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
                If isControlHasAttribute(control, ROLE_EVAL_ACTION) Then
                    evalType = wControl.Attributes(ROLE_EVAL_ACTION).ToUpper
                End If

                Select Case evalType
                    Case "ENABLE"
                        wControl.Enabled = result
                    Case "VISIBLE"
                        wControl.Visible = result
                End Select

            End If

        End Sub

        'ロール管理コントロールの判定
        Protected Function isRoleBaseControl(ByRef control As Control) As Boolean
            Return isControlHasAttribute(control, ROLE_AUTH_ALLOW)
        End Function
        Protected Function isControlHasAttribute(ByRef con As Control, ByVal attr As String) As Boolean
            Dim result As Boolean = False
            If TypeOf con Is WebControl Then
                Dim wControl = CType(con, WebControl)
                'コントロールにアトリビュートが保持されているかどうか判定する
                Dim attrValue As String = wControl.Attributes(attr)
                If Not String.IsNullOrEmpty(attrValue) Then
                    result = True
                End If
            End If
            Return result
        End Function

        'ListItemのAttributeが消える対策
        Protected Sub saveListItemAttribute(ByRef con As Control)
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

        Private Sub loadListItemAttribute(ByRef con As Control)
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

        'ユーティリティメソッド
        Public Function getMyControl(ByVal id As String) As GearsControl
            Return GPageMediator.GControl(id)
        End Function
        Public Function getMyControl(ByRef con As Control) As GearsControl
            If Not con Is Nothing Then
                Return GPageMediator.GControl(con.ID)
            Else
                Return Nothing
            End If
        End Function

        Public Sub registerMyControl(ByRef con As Control, Optional isAutoLoadDS As Boolean = True, Optional isAutoLoadAttr As Boolean = True)
            GPageMediator.addGControl(con, isAutoLoadAttr)
        End Sub
        Public Sub registerMyControl(ByRef con As Control, ByRef ds As GearsDataSource)
            Dim gcon As GearsControl = New GearsControl(con, ds)
            GPageMediator.addGControl(gcon)
        End Sub

        Public Function GFindControl(ByVal conid As String) As Control
            Return GPageMediator.GFindControl(Me, conid)
        End Function

        Public Function setValueToControl(ByVal conid As String, ByVal value As String) As Boolean
            Dim con As Control = GFindControl(conid)
            Dim binder As New GBinderTemplate()

            If Not con Is Nothing Then
                binder.setValue(con, value)
                Return True
            Else
                Return False
            End If
        End Function

        Public Function GetSubmitCauseControl() As Control
            Dim result As Control = Nothing
            Dim controlName As String = Me.Request.Params("__EVENTTARGET")

            If Not String.IsNullOrEmpty(controlName) Then
                result = Me.FindControl(controlName)

            Else
                Dim imageControl As Control = Nothing

                For i As Integer = 0 To Me.Request.Form.Keys.Count - 1
                    Dim conInForm As Control = Me.FindControl(Me.Request.Form.Keys(i))
                    If TypeOf conInForm Is Button Then
                        result = conInForm
                    End If

                    If Me.Request.Form.Keys(i).EndsWith(".x") Or Me.Request.Form.Keys(i).EndsWith(".y") Then
                        imageControl = Me.FindControl(Me.Request.Form.Keys(i).Substring(0, Page.Request.Form.Keys(i).Length - 2))
                    End If
                Next

                If result Is Nothing Then
                    result = imageControl
                End If

            End If

            Return result

        End Function
        Public Function GetAsynchronousPostBackPanel(ByVal causedControl As Control) As Control
            If causedControl Is Nothing Then Return Nothing

            Dim con As Control = causedControl
            While Not con Is Nothing And Not TypeOf con Is UpdatePanel
                con = con.Parent
            End While
            If con Is Nothing Then
                GPageMediator.fetchControls(Me, Sub(ByRef control As Control, ByRef dto As GearsDTO)
                                                    For Each trigger As AsyncPostBackTrigger In CType(control, UpdatePanel).Triggers
                                                        If trigger.ControlID = causedControl.ID Then
                                                            con = control
                                                        End If
                                                    Next
                                                End Sub,
                                               Function(ByRef control As Control) As Boolean
                                                   If TypeOf control Is UpdatePanel Then Return True Else Return False
                                               End Function)

            End If

            Return con

        End Function

        Public Sub addRelation(ByRef conF As Control, ByRef conT As Control)
            GPageMediator.addRelation(conF, conT)
        End Sub
        Public Sub setEscapesWhenSend(ByRef fromCon As Control, ByRef toCon As Control, ByVal ParamArray escapes() As String)
            GPageMediator.setEscapesWhenSend(fromCon, toCon, escapes)
        End Sub
        Public Sub resetEscapesWhenSend(ByRef fromCon As Control, ByRef toCon As Control)
            GPageMediator.resetEscapesWhenSend(fromCon, toCon)
        End Sub

        Public Function getLogCount() As Integer
            Return Log.Count
        End Function
        Public Function getLogMsgFirst() As GearsException
            If Log.Count > 0 Then
                Return getLogMsg(0)
            Else
                Return Nothing
            End If
        End Function
        Public Function getLogMsg(ByVal index As Integer) As GearsException
            Dim logIndex As String = Nothing
            Dim logContent As GearsException = Nothing
            Dim i As Integer = 0

            For Each item As KeyValuePair(Of String, GearsException) In Log
                If index = i Then
                    logIndex = item.Key
                    logContent = item.Value
                    Exit For
                End If
                i += 1
            Next

            Return logContent

        End Function
        Public Sub getLogMsgDescription(ByVal result As Boolean, ByRef label As Label)
            Dim desc As KeyValuePair(Of String, String) = getLogMsgDescription(result)
            label.CssClass = desc.Key
            label.Text = desc.Value

        End Sub
        Public Overridable Function getLogMsgDescription(ByVal result As Boolean) As KeyValuePair(Of String, String)
            Dim desc As KeyValuePair(Of String, String) = Nothing
            Dim msg As String
            If Log.Count > 0 Then
                msg = getLogMsgFirst.Message
                If Not getLogMsgFirst.InnerException Is Nothing Then
                    msg += "　詳細：" + getLogMsgFirst.InnerException.Message
                ElseIf Not String.IsNullOrEmpty(getLogMsgFirst.getMsgDebug()) Then
                    msg += "　詳細：" + getLogMsgFirst.getMsgDebug()
                End If

                If result Then
                    msg = "【警告】" + msg
                    desc = New KeyValuePair(Of String, String)("g-msg-warning", msg)
                Else
                    msg = "【エラー】" + msg
                    desc = New KeyValuePair(Of String, String)("g-msg-error", msg)
                End If
            Else
                desc = New KeyValuePair(Of String, String)("g-msg-success", "【成功】処理は正常に実行されました")
            End If
            Return desc
        End Function

        Public Function getQueryString(ByVal keyname As String) As String
            Dim value As String = HttpUtility.UrlDecode(Page.Request.QueryString.Get(keyname))
            If value Is Nothing Then
                Return ""
            Else
                Return value
            End If
        End Function
        Public Function getQueryStrings(Optional ByVal isIgnoreBlank As Boolean = True) As Dictionary(Of String, List(Of String))
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
        Protected Function getMasterProperty(ByVal propName As String) As Object
            Dim masterType As Type = Master.GetType
            Dim masterProperty As PropertyInfo = masterType.GetProperty(propName)

            If Not masterProperty Is Nothing Then
                Return masterProperty.GetValue(Master, Nothing)
            Else
                Return Nothing
            End If

        End Function

        Public Function isLoggingMode() As Boolean
            Dim isLogging As String = getQueryString(GEARS_IS_LOG_OUT)
            If Not String.IsNullOrEmpty(isLogging) AndAlso (isLogging.ToLower = "true") Then
                Return True
            Else
                Return False
            End If
        End Function

    End Class

End Namespace
