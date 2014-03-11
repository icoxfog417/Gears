Imports NUnit.Framework
Imports Gears
Imports Gears.Binder
Imports Gears.DataSource

Namespace GearsTest

    <TestFixture()>
    Public Class GBinderTemplateTest

        Dim outOfFilterEmp As String = "9997"
        Dim defaultEmp As String = "7369"
        Dim defaultArea As String = "A10" + GearsControl.VALUE_SEPARATOR + "40"
        Private Const mainConnection As String = "SQLiteConnect"

        <Test()>
        Public Sub TextBoxSet()
            Dim textEmp As TextBox = ControlBuilder.createControl("txtEMPNO")
            Dim textBox As TextBox = ControlBuilder.createControl("txtHOGE")
            Dim empData As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM V_EMP WHERE EMPNO = :pEmp ", SimpleDBA.makeParameters("pEmp", defaultEmp))

            Assert.IsFalse(empData Is Nothing)

            Dim gbind As New GBinderTemplate

            gbind.dataBind(textEmp, empData)    'リストコントロール/GridViewなどのバインドコントロール以外は、DataBindで値は付かないことを確認する
            Assert.IsTrue(String.IsNullOrEmpty(textEmp.Text))


            gbind.dataAttach(textEmp, empData)
            Assert.IsTrue(defaultEmp, textEmp.Text)

            gbind.dataAttach(textBox, empData) '名称からデータソース名が推定できないものはデータアタッチできない
            Assert.IsTrue(String.IsNullOrEmpty(textBox.Text))

            'setter/getter
            gbind.setValue(textBox, "hoge")
            Assert.AreEqual("hoge", textBox.Text)
            Assert.AreEqual("hoge", gbind.getValue(textBox))

        End Sub

        <Test()>
        Public Sub DropDownListSet()
            Dim dropDown As DropDownList = ControlBuilder.createControl("ddlDEPTNO")
            Dim empData As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM V_EMP WHERE EMPNO = :pEmp ", SimpleDBA.makeParameters("pEmp", defaultEmp))
            Dim deptData As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM DEPT ")

            Assert.IsFalse(deptData Is Nothing)

            Dim gbind As New GBinderTemplate

            '列不指定バインド
            gbind.dataBind(dropDown, deptData)
            For i As Integer = 0 To dropDown.Items.Count - 1
                Assert.AreEqual(dropDown.Items(i).Value, GearsSqlExecutor.getDataSetValue(0, deptData, i))
                Assert.AreEqual(dropDown.Items(i).Text, GearsSqlExecutor.getDataSetValue(1, deptData, i))
            Next

            'アタッチ
            gbind.dataAttach(dropDown, empData)

            Assert.AreEqual(dropDown.SelectedValue, GearsSqlExecutor.getDataSetValue("DNAME", empData))

            'setter/getter
            dropDown.ClearSelection()

            gbind.setValue(dropDown, GearsSqlExecutor.getDataSetValue("DNAME", empData))
            Assert.AreEqual(dropDown.SelectedValue, GearsSqlExecutor.getDataSetValue("DNAME", empData))
            Assert.AreEqual(dropDown.SelectedValue, gbind.getValue(dropDown))


        End Sub

        <Test()>
        Public Sub GridViewSet()

            '通常
            Dim grvView As GridView = ControlBuilder.createControl(Of GridView)("grvEMP").addKeys("EMPNO")
            Dim empData As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM V_EMP")
            Dim empRow As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM V_EMP WHERE EMPNO = :pEmp ", SimpleDBA.makeParameters("pEmp", defaultEmp))

            'キーなし
            Dim grvViewNoKey As GridView = ControlBuilder.createControl("grvEMP")

            '複数キー
            Dim grvArea As GridView = ControlBuilder.createControl(Of GridView)("grvAREA").addKeys("AREA", "DEPTNO")
            Dim areaData As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM AREA")
            Dim areaRow As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM AREA WHERE AREA = :pArea ", SimpleDBA.makeParameters("pArea", Split(defaultArea, GearsControl.VALUE_SEPARATOR)(0)))

            Dim gbind As New GBinderTemplate

            '無選択状態でのデータ取得
            Assert.AreEqual("", gbind.getValue(grvView))

            'バインド
            gbind.dataBind(grvViewNoKey, empData)
            gbind.dataBind(grvView, empData)
            Assert.AreEqual(empData.Rows.Count, grvView.Rows.Count)

            'アタッチはなし

            'セット
            gbind.setValue(grvView, GearsControl.serializeValue(empRow))
            Assert.AreEqual(defaultEmp, grvView.SelectedValue.ToString)
            Assert.AreEqual(defaultEmp, gbind.getValue(grvView))

            gbind.setValue(grvViewNoKey, GearsControl.serializeValue(empRow)) '何も起こらない
            Assert.IsTrue(String.IsNullOrEmpty(gbind.getValue(grvViewNoKey)))


            '複数キーの場合
            gbind.dataBind(grvArea, areaData)
            gbind.setValue(grvArea, GearsControl.serializeValue(areaRow))
            Assert.AreEqual(defaultArea, GearsControl.serializeValue(grvArea.SelectedDataKey))
            Assert.AreEqual(defaultArea, gbind.getValue(grvArea))

            'キー不完全
            areaRow = SimpleDBA.executeSql(mainConnection, "SELECT DEPTNO FROM AREA WHERE AREA = :pArea ", SimpleDBA.makeParameters("pArea", Split(defaultArea, GearsControl.VALUE_SEPARATOR)(0)))
            gbind.setValue(grvArea, GearsControl.serializeValue(areaRow)) '例外は出ないが選択は更新されない
            Assert.AreEqual(-1, grvArea.SelectedIndex)



        End Sub

        <Test()>
        Public Sub RadioButtonAndCheckBoxSet()

            Dim radio As RadioButton = ControlBuilder.createControl("rbtFILTER")
            Dim check As CheckBox = ControlBuilder.createControl("chbFILTER")

            Dim empRow As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM V_EMP WHERE EMPNO = :pEmp ", SimpleDBA.makeParameters("pEmp", defaultEmp))
            Dim outEmpRow As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM V_EMP WHERE EMPNO = :pEmp ", SimpleDBA.makeParameters("pEmp", outOfFilterEmp))
            Dim gbind As New GBinderTemplate

            gbind.dataAttach(radio, empRow)
            gbind.dataAttach(check, empRow)

            Assert.IsTrue(radio.Checked)
            Assert.IsTrue(check.Checked)

            gbind.dataAttach(radio, outEmpRow)
            gbind.dataAttach(check, outEmpRow)

            Assert.IsFalse(radio.Checked)
            Assert.IsFalse(check.Checked)


        End Sub

        <Test()>
        Public Sub RadioAndCheckBoxListSet()
            Dim empRow As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM V_EMP WHERE EMPNO = :pEmp ", SimpleDBA.makeParameters("pEmp", defaultEmp))
            Dim radio As RadioButtonList = ControlBuilder.createControl("rbtDEPTNO")
            Dim check As CheckBoxList = ControlBuilder.createControl("chlAREA")

            Dim dept As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM DEPT")
            Dim area As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM AREA")
            Dim gbind As New GBinderTemplate

            'バインド
            gbind.dataBind(radio, dept)
            gbind.dataBind(check, area)

            Assert.AreEqual(radio.Items.Count, dept.Rows.Count)
            Assert.AreEqual(area.Rows.Count, area.Rows.Count)

            'アタッチ
            gbind.dataAttach(radio, empRow)
            gbind.dataAttach(check, empRow)

            Assert.AreEqual(GearsSqlExecutor.getDataSetValue("DEPTNO", empRow), radio.SelectedValue)
            Assert.AreEqual(GearsSqlExecutor.getDataSetValue("AREA", empRow), check.SelectedValue)


        End Sub

        <Test()>
        Public Sub SharedBindTest()
            Dim list As DropDownList = ControlBuilder.createControl("ddlDEPT")
            Dim depts As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM DEPT")

            GBinderTemplate.dataBind(list, New DataSource.DEPTNO(mainConnection))

            Assert.AreEqual(depts.Rows.Count, list.Items.Count)

        End Sub

    End Class

End Namespace
