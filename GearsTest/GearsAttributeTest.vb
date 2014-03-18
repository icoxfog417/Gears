Imports NUnit.Framework
Imports Gears
Imports Gears.Validation
Imports Gears.Validation.Validator

Namespace GearsTest

    <TestFixture()>
    Public Class GearsAttributeTest

        <Test()>
        Public Sub attributeCreateFromString()
            Dim attrc As New GearsAttributeCreator()
            Dim css As String = "class gears-GRequired"
            Dim attr As GearsAttribute = attrc.createAttributesFromString(css).pack

            Assert.AreNotEqual(attr, Nothing)
            Assert.IsInstanceOf(Of GRequired)(attr.ListUp(0))

        End Sub

        <Test()>
        Public Sub attributeCreateFromStringWithAttribute()
            Dim attrc As New GearsAttributeCreator()
            Dim css As String = "gears-GByteLength_Length_5 bluefont"
            Dim attr As GearsAttribute = attrc.createAttributesFromString(css).pack

            Assert.AreNotEqual(attr, Nothing)
            Assert.IsInstanceOf(Of GByteLength)(attr.ListUp(0))
            Assert.AreEqual(5, CType(attr.ListUp(0), GByteLength).Length)
            Assert.IsFalse(attr.ListUp(0).isValidateOk("aaaaaa"))
            Assert.IsTrue(attr.ListUp(0).isValidateOk("aaaaa"))

        End Sub

        <Test()>
        Public Sub attributeCreateFromStringMulti()

            Dim attrc As New GearsAttributeCreator()
            Dim css As String = "class gears-GPeriodPositionOk_BeforeP_5_AfterP_3 bluefont gears-GByteLength_Length_9 gears-GStartWith_Prefix_1"
            Dim attr As GearsAttribute = attrc.createAttributesFromString(css).pack

            Assert.AreEqual(3, attr.ListUp.Count)
            Assert.IsTrue(CType(attr.ListUp(0), GPeriodPositionOk).isValidateOk("23456.456"))
            Assert.IsFalse(CType(attr.ListUp(0), GPeriodPositionOk).isValidateOk("123456.456"))
            Assert.IsTrue(CType(attr.ListUp(1), GByteLength).isValidateOk("XXXXXXXXX"))
            Assert.IsFalse(CType(attr.ListUp(1), GByteLength).isValidateOk("X"))
            Assert.IsTrue(CType(attr.ListUp(2), GStartWith).isValidateOk("123"))
            Assert.IsFalse(CType(attr.ListUp(2), GStartWith).isValidateOk("234"))

            '全てのアトリビュートのチェックがかかる
            Assert.IsTrue(attr.isValidateOk("12345.456"))
            Assert.IsFalse(attr.isValidateOk("123454.56"))
            Assert.IsFalse(attr.isValidateOk("23456.456"))


        End Sub

    End Class


End Namespace
