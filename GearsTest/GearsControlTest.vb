Imports NUnit.Framework
Imports Gears

Namespace GearsTest

    <TestFixture()>
    Public Class GearsControlTest

        <Test()>
        Public Sub getAsDateTime()

            '日付型のマーカーが設定されている場合のテスト
            Dim txtDATE As WebControl = ControlBuilder.createControl("txtDATE")
            txtDATE.CssClass = "gears-GDate_Format_yyyy-MM-dd_DoCast_True" '日付型のバリデーションで、書式はyyyy-MM-dd

            Dim gcon As New GearsControl(txtDATE, String.Empty)
            gcon.setValue("1000-01-01")

            Assert.AreEqual(New DateTime(1000, 1, 1), gcon.getAsObject)

            txtDATE.CssClass = "gears-GDate_Format_yyyy-MM-dd"
            gcon = New GearsControl(txtDATE, String.Empty)
            Assert.AreEqual("1000-01-01", gcon.getAsObject.ToString)

        End Sub

        <Test()>
        Public Sub setDateTime()

            '日付型のマーカーが設定されている場合のテスト
            Dim txtDATE As WebControl = ControlBuilder.createControl("txtDATE")
            txtDATE.CssClass = "gears-GDate_Format_yyyy-MM-dd" '日付型のバリデーションで、書式はyyyy-MM-dd

            Dim gcon As New GearsControl(txtDATE, String.Empty)
            Dim dateValue As New DateTime(1000, 1, 1)
            gcon.setValue(dateValue)

            Assert.AreEqual("1000-01-01", gcon.getValue)

            txtDATE.CssClass = "" 'フォーマット指定なし
            gcon = New GearsControl(txtDATE, String.Empty)
            gcon.setValue(dateValue)

            Assert.AreEqual(dateValue.ToString, gcon.getValue)

        End Sub

    End Class

End Namespace
