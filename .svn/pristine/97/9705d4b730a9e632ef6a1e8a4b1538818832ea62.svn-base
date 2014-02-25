Imports Gears

Partial Class GearsSampleRelation
    Inherits GearsPage

    'テスト用テーブルからデータを取得するデータソース
    Private tableName As SqlDataSource = SqlBuilder.newDataSource("EMP")

    'Pageイベントハンドラ
    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        Dim dataSource As New GDSTemplate(Master.ConnectionName, tableName)

        'テスト３：フォーム用パネルの使用→関連に使用するコントロールを登録
        registerMyControl(pnlTest3__GFORM, dataSource)

        'テスト４：選択項目用パネルの使用→関連に使用するコントロールを登録
        registerMyControl(pnlTest4__GFILTER, dataSource)
        registerMyControl(grvData4, dataSource)

        'テスト５：収集した情報の確認
        registerMyControl(pnlTest5__1__GFILTER)
        registerMyControl(pnlTest5__1__GFORM)

        'ラジオボタン初期選択
        rblTest5SEX__1.SelectedIndex = 0
        rblTest5SEX__2.SelectedIndex = 0

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'コントロール初期化
        lblMsg.Text = ""

        'テスト２：関連登録
        addRelation(ddlCOMP_UNIT, ddlCOMP_GRP)

        'テスト３：関連登録
        addRelation(ddlEMPNO, pnlTest3__GFORM)
        addRelation(rblCOMP_UNIT__3, ddlCOMP_GRP__3)

        'テスト４：関連登録
        addRelation(ddlCOMP_UNIT__2, ddlCOMP_GRP__2)
        addRelation(pnlTest4__GFILTER, grvData4)

        'テスト５：関連登録
        addRelation(pnlTest5__1__GFILTER, pnlTest5__1__GFILTER)

    End Sub

    'テスト２
    Protected Sub ddlCOMP_UNIT_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlCOMP_UNIT.SelectedIndexChanged
        '自分の関連先に変更情報を通知
        executeBehavior(ddlCOMP_UNIT)
    End Sub

    'テスト３
    Protected Sub ddlEMPNO_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlEMPNO.SelectedIndexChanged
        executeBehavior(ddlEMPNO)
    End Sub

    'テスト４
    Protected Sub ddlCOMP_UNIT__2_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlCOMP_UNIT__2.SelectedIndexChanged
        executeBehavior(ddlCOMP_UNIT__2)
    End Sub

    Protected Sub btnClick__4_Click(sender As Object, e As System.EventArgs) Handles btnClick__4.Click
        executeBehavior(pnlTest4__GFILTER)
    End Sub

    'テスト５
    Protected Sub btnClick__5__1_Click(sender As Object, e As System.EventArgs) Handles btnClick__5__1.Click
        lblDTO__1__1.Text = makeSendMessage(ddlEMPNO__5__1).ToString.Replace(vbCrLf, "<br/>")

        Dim dto As GearsDTO = getSQLTypeSetDTO(ddlTest5SQLType.SelectedValue)
        lblDTO__1__2.Text = makeSendMessage(ddlEMPNO__5__1, dto).toString.Replace(vbCrLf, "<br/>")
    End Sub
    Protected Sub btnClick__5__2_Click(sender As Object, e As System.EventArgs) Handles btnClick__5__2.Click
        '不要なコントロールを登録
        setEscapesWhenSend(pnlTest5__1__GFILTER, pnlTest5__1__GFILTER, txtTest5TEXT.ID)

        lblDTO__2__1.Text = makeSendMessage(pnlTest5__1__GFILTER, pnlTest5__1__GFILTER).ToString.Replace(vbCrLf, "<br/>")

        Dim dto As GearsDTO = getSQLTypeSetDTO(ddlTest5SQLType.SelectedValue)
        lblDTO__2__2.Text = makeSendMessage(pnlTest5__1__GFILTER, pnlTest5__1__GFILTER, dto).toString.Replace(vbCrLf, "<br/>")
    End Sub
    Protected Sub btnClick__5__3_Click(sender As Object, e As System.EventArgs) Handles btnClick__5__3.Click
        '更新時のキー項目を設定
        getMyControl(ddlEMPNO__5__3.ID).setAskey()

        lblDTO__3__1.Text = makeSendMessage(pnlTest5__1__GFORM).ToString.Replace(vbCrLf, "<br/>")

        Dim dto As GearsDTO = getSQLTypeSetDTO(ddlTest5SQLType.SelectedValue)
        lblDTO__3__2.Text = makeSendMessage(pnlTest5__1__GFORM, dto).toString.Replace(vbCrLf, "<br/>")
    End Sub

    Private Function getSQLTypeSetDTO(type As String) As GearsDTO
        Dim dto As GearsDTO = New GearsDTO(ActionType.NONE)
        Select Case type
            Case "SELECT"
                dto.setAtype(ActionType.SEL)
            Case "UPDATE"
                dto.setAtype(ActionType.UPD)
            Case "INSERT"
                dto.setAtype(ActionType.INS)
            Case "DELETE"
                dto.setAtype(ActionType.DEL)
        End Select

        Return dto

    End Function

End Class
