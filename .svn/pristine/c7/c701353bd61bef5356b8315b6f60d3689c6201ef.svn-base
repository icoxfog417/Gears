Imports Gears

Partial Class GearsSampleValidation
    Inherits GearsPage

    Protected Sub Page_Init(sender As Object, e As System.EventArgs) Handles Me.Init

        Dim emp As New GSDataSource.EMP(Master.ConnectionName)
        emp.addLockCheckCol("UPD_YMD", LockType.UDATESTR)
        emp.addLockCheckCol("UPD_HMS", LockType.UTIMESTR)
        emp.ModelValidator = New EMPValidator(Master.ConnectionName)

        registerMyControl(pnlEMP_SAL__GFORM, emp)
        registerMyControl(pnlEMPAttr, emp)
    End Sub

    Protected Sub Page_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        lblVResult1.Text = ""

        addRelation(ddlEMPNO__KEY, pnlEMPAttr)
        addRelation(pnlEMP_SAL__GFORM, pnlEMP_SAL__GFORM)

    End Sub

    Protected Sub btnValidation1_Click(sender As Object, e As System.EventArgs) Handles btnValidation1.Click
        Dim result As Boolean = True
        result = MyBase.isValidateOk(pnlValidation1)
        If Not result AndAlso getLogCount() > 0 Then
            lblVResult1.Text = getLogMsgFirst().Message
            lblVResult1.CssClass = "ppp-msg error"
        Else
            lblVResult1.Text = "バリデーションＯＫ"
            lblVResult1.CssClass = "ppp-msg success"
        End If

    End Sub

    Protected Sub ddlEMPNO_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlEMPNO__KEY.SelectedIndexChanged
        executeBehavior(ddlEMPNO__KEY)
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As System.EventArgs) Handles btnSave.Click
        getLogMsgDescription(executeBehavior(pnlEMP_SAL__GFORM, ActionType.SAVE), lblMsgModelValid)
    End Sub

End Class
