Imports NUnit.Framework
Imports Gears
Imports GearsTest.ControlBuilder
Imports Gears.DataSource
Imports Gears.Util

Namespace GearsTest

    <TestFixture()>
    Public Class GearsMediatorTest

        Private Const DefaultConnection As String = "SQLiteConnect"
        Private Const DefaultNamespace As String = "DataSource"
        Private Const TestDataNumberIndex As String = "8"

        <TestFixtureSetUp()>
        Public Sub setup()
            SimpleDBA.executeSql(DefaultConnection, "DELETE FROM EMP WHERE EMPNO BETWEEN '" + TestDataNumberIndex + "000' AND '" + TestDataNumberIndex + "999' ")

            '8000～8001までのテストデータを作成
            Dim params As New Dictionary(Of String, Object)
            params.Add("F0", "8000")
            params.Add("F1", "TEST TARO")
            params.Add("F2", "ANALYST")
            params.Add("F3", "7566")
            params.Add("F4", DateTime.Now)
            params.Add("F5", "800")
            params.Add("F6", "GearsMediatorTest")
            params.Add("F7", "20")
            params.Add("F8", "A4")

            SimpleDBA.executeSql(DefaultConnection, "INSERT INTO EMP(EMPNO,ENAME,JOB,MGR,HIREDATE,SAL,COMM,DEPTNO,AREA) VALUES (:F0,:F1,:F2,:F3,:F4,:F5,:F6,:F7,:F8)", params)

            params("F0") = "8001"
            params("F1") = "TEST RYOTA"
            params("F7") = "30" '異なるDEPTNOを割り当てる
            params("F8") = "A9"

            SimpleDBA.executeSql(DefaultConnection, "INSERT INTO EMP(EMPNO,ENAME,JOB,MGR,HIREDATE,SAL,COMM,DEPTNO,AREA) VALUES (:F0,:F1,:F2,:F3,:F4,:F5,:F6,:F7,:F8)", params)

        End Sub

        <TestFixtureTearDown()>
        Public Sub tearDown()

            SimpleDBA.executeSql(DefaultConnection, "DELETE FROM EMP WHERE EMPNO BETWEEN '" + TestDataNumberIndex + "000' AND '" + TestDataNumberIndex + "999' ")

        End Sub

        <Test()>
        Public Sub updateAndAttach()
            Dim keyField As String = "hdnEMPNO__KEY"
            Dim formControl As String = "pnlEMP__GFORM"
            Dim dataKey As String = "8002"

            '事前にレコードを準備
            SimpleDBA.executeSql(DefaultConnection, "DELETE FROM EMP WHERE EMPNO = '" + dataKey + "'")
            SimpleDBA.executeSql(DefaultConnection, "INSERT INTO EMP(EMPNO) VALUES ('" + dataKey + "')")

            'コントロールの値を設定
            Dim conValue As New Dictionary(Of String, Object)
            conValue.Add(keyField, dataKey)
            conValue.Add("txtENAME", "TM" + DateTime.Now.ToString("YYMMDDHHmmss"))
            conValue.Add("ddlDEPTNO", "20")
            conValue.Add("ddlAREA", "A5")

            'フォームを準備
            Dim mediator As GearsMediator = setUpMediator("pnlEMP__GFORM", conValue)

            'SAVEする ※事前にレコードを用意しているのでUPDATE
            mediator.execute(mediator.GControl(formControl).Control)

            'SAVE後の値を取得
            Dim answer As DataTable = SimpleDBA.executeSql(DefaultConnection, "SELECT * FROM V_EMP WHERE EMPNO = " + conValue(keyField).ToString)

            '値を突合せ
            For Each gcon As KeyValuePair(Of String, GearsControl) In mediator.GControls
                If mediator.isInputControl(gcon.Value.Control) Then '入力コントロールの場合
                    '入力値との比較
                    Assert.AreEqual(conValue(gcon.Value.ControlID), answer.Rows(0)(gcon.Value.DataSourceID).ToString)
                    'フォームにセットされた値との比較
                    Assert.AreEqual(answer.Rows(0)(gcon.Value.DataSourceID).ToString, gcon.Value.getValue.ToString)
                End If
            Next

        End Sub

        <Test()>
        Public Sub insertAndAttach()
            Dim keyField As String = "hdnEMPNO__KEY"
            Dim formControl As String = "pnlEMP__GFORM"
            Dim dataKey As String = "8002"

            '事前に削除
            SimpleDBA.executeSql(DefaultConnection, "DELETE FROM EMP WHERE EMPNO = '" + dataKey + "'")

            'コントロールの値を設定
            Dim conValue As New Dictionary(Of String, Object)
            conValue.Add(keyField, dataKey)
            conValue.Add("txtENAME", "TM" + DateTime.Now.ToString("YYMMDDHHmmss"))
            conValue.Add("ddlDEPTNO", "20")
            conValue.Add("ddlAREA", "A5")

            'フォームを準備
            Dim mediator As GearsMediator = setUpMediator("pnlEMP__GFORM", conValue)

            'SAVEする ※この時点では、レコードが削除されているはずなのでINSERTされるはず
            mediator.execute(mediator.GControl(formControl).Control)

            'SAVE後の値を取得
            Dim answer As DataTable = SimpleDBA.executeSql(DefaultConnection, "SELECT * FROM V_EMP WHERE EMPNO = " + conValue(keyField).ToString)

            '値を突合せ
            For Each gcon As KeyValuePair(Of String, GearsControl) In mediator.GControls
                If mediator.isInputControl(gcon.Value.Control) Then '入力コントロールの場合
                    '入力値との比較
                    Assert.AreEqual(conValue(gcon.Value.ControlID), answer.Rows(0)(gcon.Value.DataSourceID).ToString)
                    'フォームにセットされた値との比較
                    Assert.AreEqual(answer.Rows(0)(gcon.Value.DataSourceID).ToString, gcon.Value.getValue.ToString)
                End If
            Next

        End Sub

        <Test()>
        Public Sub selectAndAttach()

            'コントロールの値を設定
            Dim keyField As String = "hdnEMPNO__KEY"
            Dim conValue As New Dictionary(Of String, Object)
            conValue.Add(keyField, "8000")

            'フォームを準備
            Dim mediator As GearsMediator = setUpMediator("pnlEMP__GFORM", conValue)
            mediator.addRelation(keyField, "pnlEMP__GFORM") 'キーからパネルにリレーションを登録

            'ロードされる値の理論値
            Dim answer As DataTable = SimpleDBA.executeSql(DefaultConnection, "SELECT * FROM V_EMP WHERE EMPNO = " + conValue(keyField).ToString)

            'フォームに値をロードする
            mediator.send(mediator.GControl(keyField).Control)

            '値を突合せ
            For Each gcon As KeyValuePair(Of String, GearsControl) In mediator.GControls
                If mediator.isInputControl(gcon.Value.Control) Then '入力コントロールの場合
                    Assert.AreEqual(answer.Rows(0)(gcon.Value.DataSourceID).ToString, gcon.Value.getValue.ToString)
                End If
            Next

            '再度、DEPTNO/AREAの関係が異なる値をロードする
            '理論値を更新
            answer = SimpleDBA.executeSql(DefaultConnection, "SELECT * FROM V_EMP WHERE EMPNO = 8001")

            'フォームに値を再ロードする
            mediator.GControl(keyField).setValue("8001")
            mediator.send(mediator.GControl(keyField).Control)

            '値を突合せ
            For Each gcon As KeyValuePair(Of String, GearsControl) In mediator.GControls
                If mediator.isInputControl(gcon.Value.Control) Then '入力コントロールの場合
                    Assert.AreEqual(answer.Rows(0)(gcon.Value.DataSourceID).ToString, gcon.Value.getValue.ToString)
                End If
            Next

        End Sub

        ''' <summary>
        ''' 検索用(フォーム以外)のDTOの検証
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeFilterDTO()

            'コントロールの値を設定
            Dim conValue As New Dictionary(Of String, Object)
            conValue.Add("hdnEMPNO__KEY", "8000")
            conValue.Add("txtENAME", "MY_NAME")
            conValue.Add("ddlDEPTNO", "20")
            conValue.Add("ddlAREA", "A5")

            '検索用パネルを準備
            Dim mediator As GearsMediator = setUpMediator("pnlGFilter", conValue)

            'フィルタパネルで選択された値でビューを更新
            Dim pnl As Control = mediator.GControl("pnlGFilter").Control
            Dim dto As GearsDTO = mediator.makeDTO(pnl, mediator.GControl("grvEMP").Control, Nothing)

            Dim sanswer As String = "SELECT * FROM V_EMP WHERE EMPNO =:F0 AND ENAME = :F1 AND DEPTNO =:F2 AND AREA = :F3"
            Dim sqlb As SqlBuilder = mediator.GControl("grvEMP").DataSource.makeSqlBuilder(dto)

            Console.WriteLine(sqlb.confirmSql(ActionType.SEL))
            Assert.IsTrue(SqlBuilderTest.compareWithoutSpace(sanswer, sqlb.confirmSql(ActionType.SEL, True)))

        End Sub

        ''' <summary>
        ''' フォーム用DTO(自身を更新するためのDTO)の検証
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeFormDTO()

            'コントロールの値を設定
            Dim conValue As New Dictionary(Of String, Object)
            conValue.Add("hdnEMPNO__KEY", "8000")
            conValue.Add("txtENAME", "MY_NAME")
            conValue.Add("ddlDEPTNO", "20")
            conValue.Add("ddlAREA", "A5")

            '更新フォームを準備
            Dim mediator As GearsMediator = setUpMediator("pnlGFORM", conValue)

            Dim form As Control = mediator.GControl("pnlGFORM").Control

            Dim fDto As GearsDTO = mediator.makeDTO(form, form, Nothing)
            Assert.IsTrue(fDto IsNot Nothing)
            Assert.IsTrue(fDto.Action = ActionType.SEL) 'デフォルトは選択になる

            'フォームに設定されている値が正しいか確認
            For Each cn As KeyValuePair(Of String, Object) In conValue
                Assert.AreEqual(cn.Value, fDto.ControlInfo(cn.Key).First.Value)
            Next

            '更新SQLの確認
            Dim fSql As SqlBuilder = fDto.toSqlBuilder(SqlBuilder.DS("TARGET"))

            Dim sanswer As String = "SELECT EMPNO,ENAME,DEPTNO,AREA FROM TARGET WHERE EMPNO = :F0"
            Dim uanswer As String = "UPDATE TARGET SET ENAME = :U0,DEPTNO = :U1,AREA = :U2 WHERE EMPNO = :F0"
            Dim ianswer As String = "INSERT INTO TARGET(EMPNO,ENAME,DEPTNO,AREA) VALUES (:N0,:N1,:N2,:N3)"
            Dim danswer As String = "DELETE FROM TARGET WHERE EMPNO = :F0"

            '各SQLの確認
            Console.WriteLine(fSql.confirmSql(ActionType.SEL))
            Assert.IsTrue(SqlBuilderTest.compareWithoutSpace(sanswer, fSql.confirmSql(ActionType.SEL, True)))

            Console.WriteLine(fSql.confirmSql(ActionType.UPD))
            SqlBuilderTest.compareWithoutSpace(uanswer, fSql.confirmSql(ActionType.UPD, True))

            Console.WriteLine(fSql.confirmSql(ActionType.INS))
            SqlBuilderTest.compareWithoutSpace(ianswer, fSql.confirmSql(ActionType.INS, True))

            Console.WriteLine(fSql.confirmSql(ActionType.DEL))
            SqlBuilderTest.compareWithoutSpace(ianswer, fSql.confirmSql(ActionType.DEL, True))

        End Sub

        Private Function setUpMediator(ByVal pnlId As String, ByVal controlValues As Dictionary(Of String, Object)) As GearsMediator

            'パネルコントロールの準備
            Dim conTree As New Dictionary(Of String, List(Of String))

            'フォーム部分
            conTree.Add(pnlId, New List(Of String) From {"lblENAME", "hdnEMPNO__KEY", "txtENAME", "ddlDEPTNO", "ddlAREA", "btnSave"})

            '一覧部分
            conTree.Add("pnlDisplay", New List(Of String) From {"grvEMP"})

            Dim root As Control = ControlBuilder.Build(RelationNode.makeTree(conTree))
            Dim mediator As New GearsMediator(DefaultConnection, DefaultNamespace)

            ControlBuilder.LoadControls(root, mediator)
            ControlBuilder.SetValues(root, controlValues)

            'コントロールの登録
            mediator.addControls(root) 'lblENAME,btnSaveは対象から外れるはず

            '関連を登録
            mediator.addRelation("ddlDEPTNO", "ddlAREA")

            Return mediator

        End Function


    End Class

End Namespace
