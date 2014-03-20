Imports NUnit.Framework
Imports Gears
Imports Gears.Binder
Imports Gears.DataSource
Imports Gears.Util

Namespace GearsTest

    <TestFixture()>
    Public Class GearsDataBinderTest

        <Test()>
        Public Sub TextBox()
            Dim gbind As New GearsDataBinder
            Dim textEmp As TextBox = ControlBuilder.createControl("txtEMPNO")
            Dim textBox As TextBox = ControlBuilder.createControl("txtHOGE")

            'テストデータを準備
            Dim table As New DataTable
            table.Columns.Add("EMPNO")
            table.Rows.Add("XXXXX")

            gbind.dataBind(textEmp, table)    'リストコントロール/GridViewなどのバインドコントロール以外は、DataBindで値は付かないことを確認する
            Assert.IsTrue(String.IsNullOrEmpty(textEmp.Text))

            gbind.dataAttach(textEmp, table)
            Assert.AreEqual(table.Rows(0)("EMPNO").ToString, textEmp.Text)

            gbind.dataAttach(textBox, table) '名称からデータソース名が推定できないものはデータアタッチできない
            Assert.IsTrue(String.IsNullOrEmpty(textBox.Text))

            'setter/getter
            gbind.setValue(textBox, "hoge")
            Assert.AreEqual("hoge", textBox.Text)
            Assert.AreEqual("hoge", gbind.getValue(textBox))

        End Sub

        <Test()>
        Public Sub DropDownList()
            Dim gbind As New GearsDataBinder
            Dim dropDown As DropDownList = ControlBuilder.createControl("ddlDEPTNO")

            'テストデータを準備
            Dim table As New DataTable
            table.Columns.Add("KEY")
            table.Columns.Add("VALUE")
            table.Columns.Add("COMMENT")

            table.Rows.Add("0", "DOMAIN0", "TOKYO")
            table.Rows.Add("1", "DOMAIN1", "OSAKA")

            '列不指定バインド
            gbind.dataBind(dropDown, table)
            For i As Integer = 0 To dropDown.Items.Count - 1
                Assert.AreEqual(dropDown.Items(i).Value, DataSetReader.Item(table, 0, i))
                Assert.AreEqual(dropDown.Items(i).Text, DataSetReader.Item(table, 1, i))
            Next

            'アタッチ
            Dim atTable As New DataTable
            atTable.Columns.Add("DEPTNO")
            atTable.Rows.Add("1")
            gbind.dataAttach(dropDown, atTable)

            Assert.AreEqual(DataSetReader.Item(atTable, "DEPTNO"), dropDown.SelectedValue)

            'setter/getter
            dropDown.ClearSelection()

            gbind.setValue(dropDown, "0")
            Assert.AreEqual(dropDown.SelectedItem.Text, DataSetReader.Item(table, "VALUE"))
            Assert.AreEqual(dropDown.SelectedValue, gbind.getValue(dropDown))


        End Sub

        <Test()>
        Public Sub GridView()
            Dim gbind As New GearsDataBinder

            '通常
            Dim grvView As GridView = ControlBuilder.createControl(Of GridView)("grvEMP").addKeys("EMPNO")

            'テストデータを準備
            Dim table As New DataTable
            table.Columns.Add("EMPNO")
            table.Columns.Add("EMP_TXT")
            table.Columns.Add("COMMENT")

            table.Rows.Add("0", "TARO", "TOKYO")
            table.Rows.Add("1", "RYOKO", "OSAKA")
            table.Rows.Add("2", "MIDORI", "NAGOYA")


            '無選択状態でのデータ取得
            Assert.AreEqual("", gbind.getValue(grvView))

            'バインド
            gbind.dataBind(grvView, table)
            Assert.AreEqual(table.Rows.Count, grvView.Rows.Count)

            Dim attable As New DataTable
            attable.Columns.Add("EMPNO")
            attable.Rows.Add("1")

            'セット
            gbind.dataAttach(grvView, attable)
            Assert.AreEqual(attable.Rows(0)("EMPNO"), grvView.SelectedValue.ToString)
            Assert.AreEqual(attable.Rows(0)("EMPNO"), gbind.getValue(grvView))

        End Sub

        <Test()>
        Public Sub GridViewMultiKey()
            Dim gbind As New GearsDataBinder

            '通常
            Dim grvView As GridView = ControlBuilder.createControl(Of GridView)("grvORDER").addKeys("NO", "DETAIL")

            'テストデータを準備
            Dim table As New DataTable
            table.Columns.Add("NO")
            table.Columns.Add("DETAIL")
            table.Columns.Add("MATERIAL")

            table.Rows.Add("1", "10", "POKKY")
            table.Rows.Add("1", "20", "URON")
            table.Rows.Add("2", "10", "BOX")

            'バインド
            gbind.dataBind(grvView, table)
            Assert.AreEqual(table.Rows.Count, grvView.Rows.Count)

            Dim attable As New DataTable
            attable.Columns.Add("NO")
            attable.Columns.Add("DETAIL")
            attable.Rows.Add("1", "20")

            'セット
            gbind.dataAttach(grvView, attable)
            Dim grvKey As New List(Of String)
            For i As Integer = 0 To grvView.SelectedDataKey.Values.Count - 1
                grvKey.Add(grvView.SelectedDataKey(i))
            Next

            Assert.AreEqual(String.Join(GearsControl.VALUE_SEPARATOR, grvKey), gbind.getValue(grvView))

        End Sub

        <Test()>
        Public Sub RadioButtonAndCheckBox()
            Dim gbind As New GearsDataBinder

            Dim radio As RadioButton = ControlBuilder.createControl("rbtFILTER")
            Dim check As CheckBox = ControlBuilder.createControl("chbFILTER")

            'テストデータを準備
            Dim table As New DataTable
            table.Columns.Add("FILTER")
            table.Rows.Add("1")

            gbind.dataAttach(radio, table)
            gbind.dataAttach(check, table)

            Assert.IsTrue(radio.Checked)
            Assert.IsTrue(check.Checked)

            table.Rows(0)("FILTER") = "0"
            gbind.dataAttach(radio, table)
            gbind.dataAttach(check, table)

            Assert.IsFalse(radio.Checked)
            Assert.IsFalse(check.Checked)


        End Sub

        <Test()>
        Public Sub RadioAndCheckBoxList()
            Dim gbind As New GearsDataBinder

            'テストデータを準備
            Dim table As New DataTable
            table.Columns.Add("ID")
            table.Columns.Add("TEXT")
            table.Rows.Add("1", "TOKYO")
            table.Rows.Add("2", "OSAKA")
            table.Rows.Add("3", "SHIKOKU")

            Dim radio As RadioButtonList = ControlBuilder.createControl("rblAREA")
            Dim check As CheckBoxList = ControlBuilder.createControl("cblAREA")


            'バインド
            gbind.dataBind(radio, table)
            gbind.dataBind(check, table)

            Assert.AreEqual(table.Rows.Count, radio.Items.Count)
            Assert.AreEqual(table.Rows.Count, check.Items.Count)

            'アタッチ
            Dim atTable As New DataTable
            atTable.Columns.Add("AREA")
            atTable.Rows.Add("2")

            gbind.dataAttach(radio, atTable)
            gbind.dataAttach(check, atTable)

            Assert.AreEqual(atTable.Rows(0)("AREA").ToString, radio.SelectedValue)
            Assert.AreEqual(atTable.Rows(0)("AREA").ToString, check.SelectedValue)


        End Sub

    End Class

End Namespace
