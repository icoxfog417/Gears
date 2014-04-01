Imports NUnit.Framework
Imports Gears

Namespace GearsTest

    ''' <summary>
    ''' Expressionを使用した記述の検証
    ''' </summary>
    ''' <remarks></remarks>
    <TestFixture()>
    Public Class GearsExpressionTest

        <Test()>
        Public Sub RuleRelateExpression()
            Dim mediator As New GearsMediator(String.Empty)
            Dim wcon As New TextBox
            wcon.ID = "txtCOLUMN"
            Dim fcon As New TestFormItem("TXT", "FORMITEM") 'FormItemを使用した関連をチェック

            'mediatorにコントロールを登録しておく
            mediator.addControl(wcon)
            mediator.addControl(fcon)

            'wcon -> fconへの関連を登録
            Dim rExp As New gRuleExpression(wcon, mediator)
            rExp.Relate(fcon)
            Assert.AreEqual(1, mediator.Relation(wcon).Count)
            Assert.AreEqual(fcon.ControlId, mediator.Relation(wcon).First.ControlID)

            mediator.clearRelation()

            'fcon -> wcon への関連を登録(逆向き)
            rExp = New gRuleExpression(fcon, mediator)
            rExp.Relate(wcon)
            Assert.AreEqual(1, mediator.Relation(fcon).Count)
            Assert.AreEqual(wcon.ID, mediator.Relation(fcon).First.ControlID)

            mediator.clearRelation(fcon)

            Assert.AreEqual(0, mediator.Relation(fcon).Count)

        End Sub

        <Test()>
        Public Sub RuleExceptExpression()
            Dim mediator As New GearsMediator(String.Empty)
            Dim pnl As New Panel
            pnl.ID = "pnlFilter"
            Dim wcon1 As New TestFormItem("TXT", "ITEM1")
            Dim wcon2 As New TestFormItem("DDL", "ITEM2")
            pnl.Controls.Add(wcon1)
            pnl.Controls.Add(wcon2)

            Dim gview1 As Control = ControlBuilder.createControl("grvDATA1")
            Dim gview2 As Control = ControlBuilder.createControl("grvDATA2")

            'mediatorにコントロールを登録しておく(※実際はGearsPageで自動登録されるためこの面倒な作業は不要)
            mediator.addControls(pnl) 'パネル配下のコントロールをまとめて登録
            mediator.addControl(pnl)
            mediator.addControl(gview1)
            mediator.addControl(gview2)

            'Exceptを登録
            Dim rExp As New gRuleExpression(pnl, mediator)
            rExp.Relate(gview1, gview2).Except(wcon1) '一つだけ対象外
            rExp.When(gview2).Except(wcon1, wcon2)

            Assert.AreEqual(2, mediator.Relation(pnl).Count)

            Dim dto As GearsDTO = mediator.makeDTO(pnl, gview1, Nothing)
            Assert.AreEqual(1, dto.ControlInfo.Count) '一つだけ対象外

            dto = mediator.makeDTO(pnl, gview2, Nothing)
            Assert.AreEqual(0, dto.ControlInfo.Count) '全て対象外

        End Sub

        <Test()>
        Public Sub SendExpression()
            Dim sendDefine As String = ""
            Dim dummy As gSendExpression.ExecuteSend = Function(fromControl As Control, toControl As Control, dto As GearsDTO) As Boolean
                                                           sendDefine = If(fromControl Is Nothing, "-", fromControl.ID) + "/" + If(toControl Is Nothing, "-", toControl.ID)
                                                           If dto IsNot Nothing AndAlso Not String.IsNullOrEmpty(dto.AttrInfo("NAME")) Then
                                                               sendDefine += ":" + dto.AttrInfo("NAME").ToString
                                                           End If
                                                           Return True
                                                       End Function

            '送信用DTO
            Dim fdto As New GearsDTO(ActionType.SEL)
            fdto.addAttrInfo("NAME", "FROM")

            '実行用DTO
            Dim tdto As New GearsDTO(ActionType.SEL)
            tdto.addAttrInfo("NAME", "TO")

            '起点コントロール
            Dim fControl As Control = ControlBuilder.createControl("txtFROM")

            '相手コントロール
            Dim tControl As Control = ControlBuilder.createControl("ddlTO")

            '検証開始
            Dim sExp As New gSendExpression(fControl, dummy)
            sExp.ToAll(tdto) '全体に送信
            Assert.AreEqual(fControl.ID + "/-:TO", sendDefine) 'ALLの場合、対象がNothingとなる

            sExp.ToMyself() '自分自身に送信
            Assert.AreEqual(fControl.ID + "/" + fControl.ID, sendDefine)

            sExp.ToThe(tControl)
            Assert.AreEqual(fControl.ID + "/" + tControl.ID, sendDefine)

            sExp = New gSendExpression(fdto, dummy)
            sExp.ToThe(tControl)
            Assert.AreEqual("-/" + tControl.ID + ":FROM", sendDefine) 'TO対象にDTOを送信

            sExp.ToThe(tControl, tdto)
            Assert.AreEqual("-/" + tControl.ID + ":TO", sendDefine) '後からDTOを追加する場合、そちらが優先される

        End Sub

    End Class

End Namespace

