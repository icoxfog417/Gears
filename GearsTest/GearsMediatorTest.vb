Imports NUnit.Framework
Imports Gears
Imports GearsTest.ControlBuilder
Imports Gears.DataSource
Imports Gears.Util

Namespace GearsTest

    <TestFixture()>
    Public Class GearsMediatorTest

        Private mediator As GearsMediator = Nothing
        Private Shared DbServers As DbServerType() = {DbServerType.Oracle, DbServerType.SQLServer, DbServerType.OLEDB, DbServerType.SQLite}
        Private Shared TestEmps As String() = {"7566", "7654", "7934"}
        Private Const mainConnection As String = "SQLiteConnect"
        Private Const groupNamespace As String = "DataSource.Groups"

        Protected SamplePage As Page = Nothing
        Private targetControlCount As Integer = 0
        Private exceptionInForm As Integer = 0
        Protected RelationPage As Page = Nothing
        Private Const FILTER_PANEL As String = "pnlGFILTER"
        Private Const FORM_PANEL As String = "pnlGFORM"
        Private Const TABLE_VIEW As String = "grvEMP"

        <SetUp()>
        Public Sub Setup()
            SamplePage = New Page
            RelationPage = New Page
            targetControlCount = 0
            exceptionInForm = 0
            Dim builder As New ControlBuilder(SamplePage)

            'ページ作成
            With builder.addNode(ControlBuilder.createControl(ControlType.PNL, FILTER_PANEL), MoveDirection.UNDER)
                .addNode(ControlBuilder.createControl(ControlType.TXT, "txtEMPNO"))
                .addNode(ControlBuilder.createControl(ControlType.DDL, "ddlDEPTNO"))
                .addNode(ControlBuilder.createControl(ControlType.CBL, "cblJOB"))
                .addNode(ControlBuilder.createControl(ControlType.TXT, "txtEMPNO")) '同名IDコントロール
                targetControlCount += .getNodeNow.Controls.Count - 1 '同名コントロール分マイナス
                .moveNode(MoveDirection.UPPER)
            End With
            With builder.addNode(ControlBuilder.createControl(ControlType.PNL, FORM_PANEL), MoveDirection.UNDER)
                .addNode(ControlBuilder.createControl(ControlType.TXT, "txtEMPNO__FORM"))
                .addNode(ControlBuilder.createControl(ControlType.DDL, "ddlDEPTNO__FORM"))
                .addNode(ControlBuilder.createControl(ControlType.CBL, "cblJOB__FORM"))
                .addNode(ControlBuilder.createControl(ControlType.TXT, "txttxtEMPNO__FORM")) '名称ルール外コントロール
                .addNode(ControlBuilder.createControl(ControlType.TXT, "txEMPNO__FORM")) '名称ルール外コントロール
                .addNode(ControlBuilder.createControl(ControlType.TXT, "TXTEMPNO__FORM")) '名称ルール外コントロール
                targetControlCount += .getNodeNow.Controls.Count - 3 '名称ルール外コントロール分-3
                .moveNode(MoveDirection.UPPER)
            End With
            With builder.addNode(ControlBuilder.createControl(ControlType.PNL, "pnlList"))
                .addNode(ControlBuilder.createView(TABLE_VIEW, "EMPNO"))
            End With
            builder.addNode(New HiddenField())

            'リレーション検証用ページ
            Dim dsAttribute As Hashtable = ControlBuilder.makeAttribute(GearsControl.DS_NAMESPACE, "DataSource")
            builder.initRoot(RelationPage)

            With builder.addNode(ControlBuilder.createControl(ControlType.PNL, FORM_PANEL), MoveDirection.UNDER)
                .addNode(ControlBuilder.createControl(ControlType.TXT, "txtEMPNO"))
                .addNode(ControlBuilder.createControl(ControlType.DDL, "ddlDEPTNO", dsAttribute))
                With builder.addNode(ControlBuilder.createControl(ControlType.PNL, "pnlDeptInfos"), MoveDirection.UNDER)
                    .addNode(ControlBuilder.createControl(ControlType.DDL, "ddlAREA"))
                    .addNode(ControlBuilder.createControl(ControlType.DDL, "ddlGROUPN"))
                    .addNode(ControlBuilder.createControl(ControlType.DDL, "ddlUNITS"))
                    .addNode(ControlBuilder.createControl(ControlType.LBL, "lblHOGEN"))
                    exceptionInForm = 1     'lblHOGENは名称ルール外なので、カウントされない
                    .moveNode(MoveDirection.UPPER)
                End With
                .addNode(ControlBuilder.createControl(ControlType.TXT, "txtENAME"))
                .addNode(ControlBuilder.createControl(ControlType.LBL, "lblJOB__GCON"))
                .addNode(ControlBuilder.createControl(ControlType.TXT, "txtSAL"))
                .moveNode(MoveDirection.UPPER)
            End With

            builder.addNode(ControlBuilder.createControl(ControlType.DDL, "ddlDEPTNO__FIL", dsAttribute)) 'フィルター外のコントロール

            With builder.addNode(ControlBuilder.createControl(ControlType.PNL, FILTER_PANEL), MoveDirection.UNDER)
                .addNode(ControlBuilder.createControl(ControlType.TXT, "txtEMPNO__FIL"))
                .addNode(ControlBuilder.createControl(ControlType.DDL, "ddlGROUPN__FIL"))
                .addNode(ControlBuilder.createControl(ControlType.DDL, "ddlUNITS__FIL"))
                .addNode(ControlBuilder.createControl(ControlType.TXT, "txtFILTER__FIL"))
                .moveNode(MoveDirection.UPPER)
            End With

            With builder.addNode(ControlBuilder.createControl(ControlType.PNL, "pnlList"), MoveDirection.UNDER)
                .addNode(ControlBuilder.createView(TABLE_VIEW, "EMPNO"))
                .moveNode(MoveDirection.UPPER)
            End With

            'なお、データの構成としては以下の通りとなっている
            'EMP        従業員データ DEPTNO 含む
            'DEPTNO     所属
            'AREA   <- DEPTNO   地域     DEPTNOにより制約される
            'GROUPN <- DEPTNO   グループ DEPTNOにより制約される
            'UNITS  <- GROUPN   組織     GROUPNにより制約される

        End Sub

        <TearDown()>
        Public Sub TearDown()
            SamplePage = Nothing
            RelationPage = Nothing
            mediator = Nothing '開放
        End Sub

        Private Sub initMediator(ds As DbServerType)
            'Set Mediator
            Select Case ds
                Case DbServerType.Oracle
                    mediator = New GearsMediator("OracleConnect", "DataSource")
                Case DbServerType.SQLServer
                    mediator = New GearsMediator("SqlSConnect", "DataSource")
                Case DbServerType.OLEDB
                    mediator = New GearsMediator("OLEConnect", "DataSource")
                Case DbServerType.SQLite
                    mediator = New GearsMediator("SQLiteConnect", "DataSource")
            End Select
        End Sub

        Private Sub makeTitle(ByVal title As String)

            Console.WriteLine("-------------------------------------------------------------------")
            Console.WriteLine(title)
            Console.WriteLine("-------------------------------------------------------------------")

        End Sub

        Private Sub registerControls(ByRef con As Control, Optional ByVal ds As DbServerType = DbServerType.Oracle, Optional ByVal ns As String = "")
            initMediator(ds)
            If ns <> "" Then
                mediator.DsNameSpace = ns
            End If
            mediator.addControls(con)
        End Sub

        <Test(), TestCaseSource("DbServers")>
        Public Sub addControlsTest(ds As DbServerType)
            registerControls(SamplePage, ds)

            makeTitle("addControlsTest")
            For Each item As KeyValuePair(Of String, GearsControl) In mediator.GControls
                Console.WriteLine(item.Key)
            Next
            Assert.AreEqual(targetControlCount, mediator.GControls.Count)

        End Sub

        <Test()>
        Public Sub addRelationTest()
            registerControls(SamplePage, DbServerType.SQLite)

            '存在しないコントロールの登録
            Assert.Throws(Of GearsException)(Sub()
                                                 mediator.addRelation(New TextBox, New DropDownList())
                                             End Sub)

            '普通の登録
            mediator.addRelation(FILTER_PANEL, TABLE_VIEW)
            mediator.addRelation(FORM_PANEL, FORM_PANEL)

        End Sub

        <Test()>
        Public Sub controlListupTest()
            registerControls(RelationPage, DbServerType.SQLite)

            Dim form As New GearsControl(RelationPage.FindControl(FORM_PANEL), New DataSource.EMP(mediator.ConnectionName))
            mediator.addControl(form)

            Dim filter As New GearsControl(RelationPage.FindControl(FILTER_PANEL), mediator.ConnectionName)
            mediator.addControl(filter)


            'リレーションを登録
            'ROOT:ddlDEPTNO -> ddlAREA, ( ddlGROUPN -> ddlUNITS )
            mediator.addRelation("ddlDEPTNO", "ddlAREA")
            mediator.addRelation("ddlDEPTNO", "ddlGROUPN")
            mediator.addRelation("ddlGROUPN", "ddlUNITS")
            mediator.addRelation(FORM_PANEL, FORM_PANEL) '自己参照
            mediator.addRelation("txtEMPNO", FORM_PANEL) 'リレーション循環(FORM -> txtEMPNO -> FORM ->つづく)

            'ROOT:ddlDEPTNO__FIL -> ddlGROUPN__FIL -> ddlUNITS__FIL
            mediator.addRelation("ddlDEPTNO__FIL", "ddlGROUPN__FIL")
            mediator.addRelation("ddlGROUPN__FIL", "ddlUNITS__FIL")
            mediator.addRelation(FILTER_PANEL, FILTER_PANEL) '自己参照

            '検証対象がプライベートメソッドのため、リフレクションを使用し呼び出し準備
            Dim gmdType As Type = mediator.GetType

            'あるコントロールに含まれるコントロールを取得する
            Dim getControlsInArea As Reflection.MethodInfo = gmdType.GetMethod("getControlsInArea", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
            'リレーションのルートノード(A->Bでリレーションがある場合、Aに相当)であるかの判定
            Dim isRootNode As Reflection.MethodInfo = gmdType.GetMethod("isRootNode", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
            'あるコントロールに含まれる、リレーションルートを取得する
            Dim extractRootsInArea As Reflection.MethodInfo = gmdType.GetMethod("extractRootsInArea", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)

            Dim controls As List(Of String) = getControlsInArea.Invoke(mediator, New Object() {RelationPage.FindControl(FORM_PANEL)})
            Assert.AreEqual(listupControls(RelationPage.FindControl(FORM_PANEL)).Count - exceptionInForm, controls.Count) 'パネル内のコントロール数に等しい

            Assert.IsFalse(CType(isRootNode.Invoke(mediator, New Object() {"ddlUNITS", controls}), Boolean))
            Assert.IsTrue(CType(isRootNode.Invoke(mediator, New Object() {"ddlDEPTNO", controls}), Boolean))
            Assert.IsFalse(CType(isRootNode.Invoke(mediator, New Object() {FORM_PANEL, controls}), Boolean))

            controls = getControlsInArea.Invoke(mediator, New Object() {RelationPage.FindControl(FILTER_PANEL)})
            Assert.AreEqual(listupControls(RelationPage.FindControl(FILTER_PANEL)).Count, controls.Count)
            Assert.IsFalse(CType(isRootNode.Invoke(mediator, New Object() {"ddlGROUPN__FIL", Nothing}), Boolean))
            Assert.IsTrue(CType(isRootNode.Invoke(mediator, New Object() {"ddlGROUPN__FIL", controls}), Boolean)) '親のddlDEPTNO_FILは対象範囲外なので親になる

            'controls = getControlsInArea.Invoke(mediator, New Object() {RelationPage.FindControl(FILTER_PANEL)})
            Assert.IsTrue(CType(isRootNode.Invoke(mediator, New Object() {FILTER_PANEL, Nothing}), Boolean))

            Dim rootList As List(Of String) = extractRootsInArea.Invoke(mediator, New Object() {RelationPage})
            '最終的にルートになっているものををカウント 1:ddlDEPTNO/2:txtEMPNO/3:ddlDEPTNO__FIL/4:FILTER_PANEL/5:txtENAME/6:lblJOB__GCON/7:txtSAL/8:txtEMPNO__FIL/9:txtFILTER__FIL
            Assert.AreEqual(9, rootList.Count)

            rootList = extractRootsInArea.Invoke(mediator, New Object() {RelationPage.FindControl(FILTER_PANEL)})
            '最終的にルートになっているものををカウント 1:txtEMPNO__FIL/2:ddlGROUPN__FIL/3:txtFILTER__FIL
            Assert.AreEqual(3, rootList.Count)

        End Sub

        <Test(), TestCaseSource("TestEmps")>
        Public Sub executeBehaviorSelectTest(ByVal testEmp As String)
            registerControls(RelationPage, DbServerType.SQLite, groupNamespace)

            Dim empTable As New DataSource.EMP(mediator.ConnectionName)
            Dim empRow As New DataSource.EMP(mediator.ConnectionName)

            Dim form As New GearsControl(RelationPage.FindControl(FORM_PANEL), empRow)
            Dim filter As New GearsControl(RelationPage.FindControl(FILTER_PANEL), mediator.ConnectionName)
            Dim view As New GearsControl(RelationPage.FindControl(TABLE_VIEW), empTable)

            mediator.addControl(form)
            mediator.addControl(filter)
            mediator.addControl(view)

            mediator.addRelation("ddlDEPTNO", "ddlAREA")
            mediator.addRelation("ddlDEPTNO", "ddlGROUPN")
            mediator.addRelation("ddlGROUPN", "ddlUNITS")

            mediator.addRelation(FILTER_PANEL, TABLE_VIEW) '選択->リスト
            mediator.addRelation(TABLE_VIEW, FORM_PANEL) 'リスト->フォーム
            mediator.addRelation(FORM_PANEL, FORM_PANEL) 'フォーム->更新用
            mediator.addRelation("txtEMPNO", FORM_PANEL) 'リレーション循環検証用(FORM -> txtEMPNO -> FORM ->つづく)

            '理論値の算出
            Dim empsDto As New GearsDTO(ActionType.SEL)
            empsDto.addFilter(SqlBuilder.newFilter("FILTER").eq("1"))
            Dim empsCount As Integer = empTable.gSelectCount(empsDto)

            Dim empNo As String = testEmp
            Dim empDto As New GearsDTO(ActionType.SEL)
            empDto.addFilter(SqlBuilder.newFilter("EMPNO").eq(empNo))
            Dim empData As DataTable = empRow.gSelect(empDto)

            Dim empName As String = GearsSqlExecutor.getDataSetValue("ENAME", empData)
            Dim deptnoValue As String = GearsSqlExecutor.getDataSetValue("DEPTNO", empData)
            Dim areaValue As String = GearsSqlExecutor.getDataSetValue("AREA", empData)

            Dim deptDto As New GearsDTO(ActionType.SEL)
            deptDto.addFilter(SqlBuilder.newFilter("DEPTNO").eq(deptnoValue))
            Dim deptnoCount As Integer = mediator.GControl("ddlDEPTNO").DataSource.gSelectCount(New GearsDTO(ActionType.SEL))
            Dim areaCount As Integer = mediator.GControl("ddlAREA").DataSource.gSelectCount(deptDto)
            Dim groupnCount As Integer = mediator.GControl("ddlGROUPN").DataSource.gSelectCount(deptDto)

            Dim groupnTopValue As String = GearsSqlExecutor.getDataSetValue("GROUPN", mediator.GControl("ddlGROUPN").DataSource.gSelect(deptDto))
            Dim groupnDto As New GearsDTO(ActionType.SEL)
            groupnDto.addFilter(SqlBuilder.newFilter("GROUPN").eq(groupnTopValue))
            Dim unitCount As Integer = mediator.GControl("ddlUNITS").DataSource.gSelectCount(groupnDto)

            '初期化(GPageの処理に相当)
            initGControls(mediator)

            '処理対象外コントロールの設定（これらのコントロールの値は送信されない）
            mediator.addExcept(filter.Control, Nothing, "ddlGROUPN__FIL", "ddlUNITS__FIL")
            ''処理対象外コントロールの設定（これらのコントロールの値は受信されない）
            'mediator.setEscapesWhenReceive(view.Control, form.Control, "txtENAME")

            '選択から一覧を出力
            mediator.GControl("txtFILTER__FIL").setValue("1")
            mediator.send(mediator.GControl(FILTER_PANEL).Control, ActionType.SEL)

            '一覧をセレクト
            Dim viewControl As GridView = CType(view.Control, GridView)
            Assert.AreEqual(empsCount, viewControl.Rows.Count)

            Dim indexInView As Integer = 0
            For i As Integer = 0 To viewControl.DataKeys.Count - 1
                If viewControl.DataKeys(i).Value = empNo Then
                    Exit For
                End If
                indexInView += 1
            Next

            Assert.AreNotEqual(indexInView, viewControl.Rows.Count)

            CType(view.Control, GridView).SelectedIndex = indexInView
            Assert.AreEqual("", mediator.GControl("txtEMPNO").getValue) 'この時点では当然空白

            '一覧からフォームへ
            mediator.execute(mediator.GControl(TABLE_VIEW).Control)

            '反映された値をチェック
            Assert.AreEqual(empNo, mediator.GControl("txtEMPNO").getValue)
            Assert.AreEqual("", mediator.GControl("txtENAME").getValue) 'setEscapesWhenReceiveで設定しているため、空白のはず

            Assert.AreEqual(deptnoValue, mediator.GControl("ddlDEPTNO").getValue)
            Assert.AreEqual(deptnoCount, mediator.GControl("ddlDEPTNO").DataSource.gResultSet.Rows.Count)

            Assert.AreEqual(areaValue, mediator.GControl("ddlAREA").getValue)
            Assert.AreEqual(areaCount, mediator.GControl("ddlAREA").DataSource.gResultSet.Rows.Count)

            Assert.AreEqual(groupnCount, mediator.GControl("ddlGROUPN").DataSource.gResultSet.Rows.Count)
            Assert.AreEqual(unitCount, mediator.GControl("ddlUNITS").DataSource.gResultSet.Rows.Count)


        End Sub

        <Test()>
        Public Sub executeBehaviorWhenNothing()
            registerControls(RelationPage, DbServerType.SQLite, groupNamespace)

            Dim empTable As New DataSource.EMP(mediator.ConnectionName)
            Dim empRow As New DataSource.EMP(mediator.ConnectionName)
            Dim notExistEmp As String = "1000"

            Dim form As New GearsControl(RelationPage.FindControl(FORM_PANEL), empRow)

            mediator.addControl(form)

            mediator.addRelation("ddlDEPTNO", "ddlAREA")
            mediator.addRelation("ddlDEPTNO", "ddlGROUPN")
            mediator.addRelation("ddlGROUPN", "ddlUNITS")

            mediator.addRelation("txtEMPNO", FORM_PANEL)
            mediator.addRelation(FORM_PANEL, FORM_PANEL)

            '初期化(GPageの処理に相当)
            initGControls(mediator)

            mediator.GControl("txtEMPNO").setValue(notExistEmp)
            mediator.send(mediator.GControl("txtEMPNO").Control)

            Assert.AreEqual(notExistEmp, mediator.GControl("txtEMPNO").getValue) '値はロードされず、残る

            '項目限定
            mediator.GControl("txtEMPNO").setValue(TestEmps(0))
            mediator.send(mediator.GControl("txtEMPNO").Control)
            Dim firstName As String = mediator.GControl("txtEMPNO").getValue
            Dim firstSAL As String = mediator.GControl("txtSAL").getValue

            Dim selectDto As New GearsDTO(ActionType.SEL)
            selectDto.addSelection(SqlBuilder.newSelect("ENAME"))
            mediator.GControl("txtEMPNO").setValue(TestEmps(1))
            mediator.send(mediator.GControl("txtEMPNO").Control, Nothing, selectDto)

            Assert.AreNotEqual(firstName, mediator.GControl("txtENAME").getValue)
            Assert.AreEqual(firstSAL, mediator.GControl("txtSAL").getValue) '取得されていない項目はそのまま

        End Sub

        <Test(), TestCaseSource("TestEmps")>
        Public Sub executeBehaviorExecuteTest(ByVal testEmp As String)
            registerControls(RelationPage, DbServerType.SQLite, groupNamespace)

            Dim empRow As New DataSource.EMP(mediator.ConnectionName)
            '楽観ロックのカラムを設定
            empRow.addLockCheckCol("UPD_YMD", LockType.UDATESTR)
            empRow.addLockCheckCol("UPD_HMS", LockType.UTIMESTR)
            Dim form As New GearsControl(RelationPage.FindControl(FORM_PANEL), empRow)

            mediator.addControl(form)

            mediator.addRelation("ddlDEPTNO", "ddlAREA")
            mediator.addRelation("ddlDEPTNO", "ddlGROUPN")
            mediator.addRelation("ddlGROUPN", "ddlUNITS")
            mediator.addRelation(FORM_PANEL, FORM_PANEL)
            mediator.addRelation("txtEMPNO", FORM_PANEL)

            'キー設定
            mediator.GControl("txtEMPNO").IsKey = True

            '初期化処理
            initGControls(mediator)

            '除外対象設定
            mediator.addExcept(form.Control, form.Control, "pnlDeptInfos")

            'データをロード
            mediator.GControl("txtEMPNO").setValue(testEmp)
            mediator.send(mediator.GControl("txtEMPNO").Control, form.Control, New GearsDTO(ActionType.SEL))

            Assert.IsFalse(String.IsNullOrEmpty(mediator.GControl("txtENAME").getValue))

            '更新 ------------------------------------------------------------
            Dim originalName As String = mediator.GControl("txtENAME").getValue
            Dim changedValue As String = "HOGEN"
            mediator.GControl("txtENAME").setValue(changedValue)

            Dim executeDto As New GearsDTO(ActionType.SAVE)
            Dim lockedValue As List(Of SqlFilterItem) = form.DataSource.getLockCheckColValue
            executeDto.addLockItems(lockedValue) 'データロード時のロック値をセット
            mediator.send(form.Control, form.Control, executeDto)

            Assert.AreEqual(changedValue, GearsSqlExecutor.getDataSetValue("ENAME", form.DataSource.gResultSet))
            '元に戻す(ロック考慮なし)
            mediator.GControl("txtENAME").setValue(originalName)
            mediator.send(form.Control, form.Control, New GearsDTO(ActionType.SAVE))

            'ロックエラー発生(上記で更新を行っているため、ロード時のキーではエラーになるはず)
            mediator.GControl("txtENAME").setValue(originalName)
            mediator.send(form.Control, form.Control, executeDto)

            Assert.AreEqual(mediator.Log.Count, 1)
            Assert.IsInstanceOf(GetType(GearsOptimisticLockException), mediator.Log.Values(0))

            '挿入(キーの値だけ変えて更新) -----------------------------------------
            Dim newEmpno As String = "8888"
            mediator.GControl("txtEMPNO").LoadedValue = mediator.GControl("txtEMPNO").getValue '更新前の値を保持
            mediator.GControl("txtEMPNO").setValue(newEmpno)
            mediator.send(form.Control, form.Control, executeDto)

            Assert.AreEqual(newEmpno, GearsSqlExecutor.getDataSetValue("EMPNO", form.DataSource.gResultSet))

            '更新(キー変更更新OFFの場合) ------------------------------------------
            executeDto.IsPermitOtherKeyUpdate = False
            mediator.GControl("txtEMPNO").LoadedValue = testEmp '更新前の値を保持
            mediator.GControl("txtENAME").setValue(changedValue)
            mediator.send(form.Control, form.Control, executeDto)
            Assert.AreEqual(mediator.Log.Count, 1)
            Assert.IsInstanceOf(GetType(GearsSqlException), mediator.Log.Values(0))

            'キー更新OKで実行
            executeDto.IsPermitOtherKeyUpdate = True
            mediator.send(form.Control, form.Control, executeDto)
            Assert.AreEqual(changedValue, GearsSqlExecutor.getDataSetValue("ENAME", form.DataSource.gResultSet))

            '削除処理 ------------------------------------------------------------
            Dim deleteDto As New GearsDTO(ActionType.DEL)
            deleteDto.addLockItems(lockedValue) 'データロード時のロック値をセット

            '異なるキー更新はエラー
            deleteDto.IsPermitOtherKeyUpdate = False
            mediator.send(form.Control, form.Control, deleteDto)
            Assert.AreEqual(mediator.Log.Count, 1)
            Assert.IsInstanceOf(GetType(GearsSqlException), mediator.Log.Values(0))

            deleteDto.IsPermitOtherKeyUpdate = True
            deleteDto.removeLockItem()
            deleteDto.addLockItems(form.DataSource.getLockCheckColValue)
            mediator.send(form.Control, form.Control, deleteDto)

            Dim confirmDto As New GearsDTO(ActionType.SEL)
            confirmDto.addFilter(SqlBuilder.newFilter("EMPNO").eq(newEmpno))
            Assert.AreEqual(0, empRow.gSelectCount(confirmDto))

        End Sub

        <Test()>
        Public Sub makeSendMessageTest()
            registerControls(RelationPage, DbServerType.SQLite, groupNamespace)

            Dim form As New GearsControl(RelationPage.FindControl(FORM_PANEL), New DataSource.EMP(mediator.ConnectionName))
            mediator.addControl(form)

            Dim grv As New GridView
            grv.ID = "grvList"
            grv.DataKeyNames = New String() {"EMPNO"}
            Dim empno As New BoundField
            empno.DataField = "EMPNO"
            grv.Columns.Add(empno)

            Dim grvCon As New GearsControl(grv, New DataSource.EMP(mediator.ConnectionName))
            mediator.addControl(grvCon)

            Dim dto As GearsDTO = mediator.makeSendMessage(grv, Nothing, Nothing)

            Assert.AreNotEqual(dto, Nothing)

        End Sub

        Private Sub initGControls(ByRef m As GearsMediator)
            For Each gcon As KeyValuePair(Of String, GearsControl) In m.GControls
                If m.isTargetControl(gcon.Value.Control) Then
                    gcon.Value.dataBind()
                End If
            Next
        End Sub

        Private Function listupControls(ByVal con As Control, Optional ByVal isContainPanel As Boolean = False) As List(Of Control)
            Dim list As New List(Of Control)

            If con.HasControls Then
                If isContainPanel Then
                    list.Add(con)
                End If
                For Each child As Control In con.Controls
                    list.AddRange(listupControls(child, isContainPanel))
                Next
            Else
                list.Add(con)
            End If

            Return list

        End Function

        <Test()>
        Public Sub makeTreeTest()

            '単一要素
            Dim tree As New Dictionary(Of String, List(Of String))
            tree.Add("A", Nothing)
            tree.Add("B", New List(Of String) From {"B"})

            Dim treeNode As List(Of RelationNode) = RelationNode.makeTree(tree)
            treeNode.ForEach(Sub(n) Console.WriteLine(n))

            Console.WriteLine("--------------------------")

            '階層要素
            tree.Clear()
            tree.Add("A", New List(Of String) From {"B", "C"})
            tree.Add("B", New List(Of String) From {"D"})

            treeNode = RelationNode.makeTree(tree)
            treeNode.ForEach(Sub(n) Console.WriteLine(n))

            Console.WriteLine("--------------------------")

            '複数階層
            tree.Clear()
            tree.Add("A", New List(Of String) From {"B", "C"})
            tree.Add("B", New List(Of String) From {"D"})
            tree.Add("D", New List(Of String) From {"E", "F"})
            tree.Add("G", New List(Of String) From {"A"})

            tree.Add("H", New List(Of String) From {"I"})
            tree.Add("J", New List(Of String) From {"K"})
            tree.Add("K", New List(Of String) From {"L", "M", "N"})

            treeNode = RelationNode.makeTree(tree)
            treeNode.ForEach(Sub(n) Console.WriteLine(n))

            Console.WriteLine("--------------------------")

            'ループ
            tree.Clear()
            tree.Add("A", New List(Of String) From {"B"})
            tree.Add("B", New List(Of String) From {"A"})

            treeNode = RelationNode.makeTree(tree)
            treeNode.ForEach(Sub(n) Console.WriteLine(n))

        End Sub

    End Class

End Namespace
