Imports Gears
Imports System.Data.Common
Imports GSDataSource

Partial Class GearsSample
    Inherits GearsPage

    'ページ初期化イベント
    'ここで手動での登録が必要なコントロール(例：PanelやGridViewなど)の登録を行う
    '
    'ページ上のコントロールを、データソースクラスとのペアにして登録する処理はOnpreloadで実行される。
    'このため、コントロール登録前に行っておきたいような処理はPageInitに記述すること
    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        initMediator(Master.ConnectionName, Master.DsNameSpace)
        'データソースクラスの作成
        Dim empDataSourceClass As New EMP(Master.ConnectionName)

        '更新時、楽観的ロックに使用する列項目を設定
        empDataSourceClass.addLockCheckCol("UPD_YMD", LockType.UDATESTR)
        empDataSourceClass.addLockCheckCol("UPD_HMS", LockType.UTIMESTR)

        'コントロール/データソースクラスのペアを登録
        registerMyControl(pnlGFilter) 'データソースクラスが必要ない場合は設定しなくても問題なし
        registerMyControl(pnlGFORM, empDataSourceClass)
        registerMyControl(grvData, empDataSourceClass)

        'ハンドラ登録
        AddHandler COMP_UNIT__FIL.getControl(Of RadioButtonList).SelectedIndexChanged, AddressOf Me.rblCOMP_UNIT_SelectedIndexChanged

        '非同期更新設定
        Dim srmOnPage As ScriptManager = AjaxControlToolkit.ToolkitScriptManager.GetCurrent(Me)
        srmOnPage.RegisterAsyncPostBackControl(COMP_UNIT__FIL.getControl(Of RadioButtonList))

    End Sub


    'ページロードイベント
    'ここでは主にコントロール間の関連を登録しておく
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        '変数初期化
        lblLog.Text = ""
        lblMsg.Text = ""
        hdnMode.Value = "S"

        '関連を定義
        '事業部→部門の関連登録
        addRelation(COMP_UNIT__FIL.getControl, COMP_GRP__FIL.getControl)         '選択パネル用
        addRelation(hdnCOMP_UNIT, COMP_GRP__FORM.getControl(Of DropDownList))   'フォームパネル用

        '選択パネル→一覧
        addRelation(pnlGFilter, grvData)

        '一覧での選択→フォーム用パネル
        addRelation(grvData, pnlGFORM)

        'フォーム用パネル→フォーム用パネル(更新後、更新した値を自身に反映させる)
        addRelation(pnlGFORM, pnlGFORM)
        setEscapesWhenSend(pnlGFORM, pnlGFORM, "lblCOMP_UNIT_TXT__GCON") 'このコントロールは更新対象からはずしたい、という場合このように宣言

        If IsPostBack Then
            'ポストバック時、ログ記録を開始する。
            GearsLogStack.traceOn()
            GearsLogStack.setLog("リクエストされた処理のトレースを開始します。。。")
        End If
    End Sub
    Protected Sub Page_LoadComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LoadComplete
        If IsPostBack Then
            'ポストバック時、ログ記録を終了する。開始～終了までの間に実行された処理のログを出力する
            GearsLogStack.setLog("ログトレースを終了します")
            lblLog.Text = GearsLogStack.makeDisplayString 'ログの出力
            GearsLogStack.traceEnd()
        End If
    End Sub

    'コントロールイベント
    '事業部のラジオボタン選択
    Protected Sub rblCOMP_UNIT_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        executeBehavior(COMP_UNIT__FIL.getControl)
        udpArea.Update()
    End Sub
    '検索処理の実行
    Protected Sub btnSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSearch.Click
        'pnlGFilter内のコントロールの値で、関連先コントロール(GridView)のデータを制限
        getLogMsgDescription(executeBehavior(pnlGFilter))
    End Sub
    '一覧の中の行選択
    Protected Sub grvData_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles grvData.SelectedIndexChanged ', grvData__2.SelectedIndexChanged
        '選択行の値を使用し、データ更新を実行
        getLogMsgDescription(executeBehavior(grvData))
        hdnMode.Value = "U"

    End Sub
    Protected Sub btnUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUpdate.Click
        'フォームに入力された値でテーブルを更新
        '更新方法を、ActionType.SAVE(キーがあればUPDATE/なければINSERT)で指定
        getLogMsgDescription(executeBehavior(pnlGFORM, ActionType.SAVE), lblMsg)

        GearsLogStack.setLog("更新後のデータを表示します")

        executeBehavior(pnlGFilter)
        hdnMode.Value = "U"
    End Sub

    Protected Sub btnDelete_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        getLogMsgDescription(executeBehavior(pnlGFORM, ActionType.DEL), lblMsg)
        GearsLogStack.setLog("削除後のデータを表示します")

        executeBehavior(pnlGFilter)
        hdnMode.Value = "U"
    End Sub

End Class
