Imports NUnit.Framework
Imports Gears
Imports Gears.Validation.Validator
Imports Gears.Validation.Marker

Namespace GearsTest

    <TestFixture()>
    Public Class GValidationTest

        <Test()>
        Public Sub isByteLengthOk()
            Dim validator As New GByteLength(3)

            Assert.IsTrue(validator.isValidateOk("123"))
            Assert.IsTrue(validator.isValidateOk("abc"))
            Assert.IsTrue(validator.isValidateOk("ほg"))
            Assert.IsTrue(validator.isValidateOk(""))
            Assert.IsFalse(validator.isValidateOk("1234"))
            Assert.IsFalse(validator.isValidateOk("abcd"))
            Assert.IsFalse(validator.isValidateOk("ほげ"))

        End Sub

        <Test()>
        Public Sub isByteLengthBetweenOk()

            Dim validator As New GByteLengthBetween(3)

            Assert.IsTrue(validator.isValidateOk("123"))
            Assert.IsTrue(validator.isValidateOk("ab"))
            Assert.IsTrue(validator.isValidateOk(""))

            validator.MinLength() = 2
            Assert.IsFalse(validator.isValidateOk(""))
            Assert.IsFalse(validator.isValidateOk("a"))
            Assert.IsTrue(validator.isValidateOk("123"))
            Assert.IsFalse(validator.isValidateOk("ほげ"))

        End Sub

        <Test()>
        Public Sub isCompareOk()
            Dim validator As New GCompare()

            Assert.IsTrue(validator.isValidateOk("abc", "abc", "="))
            Assert.IsFalse(validator.isValidateOk("abc", "xxx", "="))

            Assert.IsTrue(validator.isValidateOk("120", "121", "<"))
            Assert.IsFalse(validator.isValidateOk("120", "120", "<"))
            Assert.IsTrue(validator.isValidateOk("120.1", "120.521", "<"))
            Assert.IsTrue(validator.isValidateOk("a", "b", "<"))


            Assert.IsTrue(validator.isValidateOk("121", "120", ">"))
            Assert.IsFalse(validator.isValidateOk("120", "120", ">"))
            Assert.IsTrue(validator.isValidateOk("1124.1234", "1124.1233", ">"))
            Assert.IsTrue(validator.isValidateOk("ん", "あ", ">"))

            Assert.IsTrue(validator.isValidateOk("120", "121", "<="))
            Assert.IsTrue(validator.isValidateOk("120", "120", "<="))
            Assert.IsFalse(validator.isValidateOk("999.123", "120.521", "<="))
            Assert.IsFalse(validator.isValidateOk("ほいみん", "ほいみ", "<="))

            Assert.IsFalse(validator.isValidateOk("120", "121", ">="))
            Assert.IsTrue(validator.isValidateOk("120", "120", ">="))
            Assert.IsTrue(validator.isValidateOk("999.123", "120.521", ">="))
            Assert.IsFalse(validator.isValidateOk("シルバ", "ジルバ", ">="))


        End Sub

        <Test()>
        Public Sub isDateOk()

            Dim validator As New GDate()

            Assert.AreEqual("gs-date", validator.CssClass)
            Assert.IsTrue(validator.isValidateOk("20010101"))
            Assert.IsTrue(validator.isValidateOk("2001/1/1"))
            Assert.IsTrue(validator.isValidateOk("2001/01/01"))
            Assert.IsTrue(validator.isValidateOk("2001-12-31"))
            Assert.IsFalse(validator.isValidateOk("abc"))
            Assert.IsFalse(validator.isValidateOk("20011232"))

            validator.Format = "yyyy/MM/dd"
            Assert.IsFalse(validator.isValidateOk("2001/1/1"))

        End Sub

        <Test()>
        Public Sub isMatchOk()

            Dim validator As New GMatch("^B[0-9]{9}:\w+\.$")

            Assert.IsTrue(validator.isValidateOk("B123456789:山手商店."))
            Assert.IsFalse(validator.isValidateOk("A123456789:山手商店."))
            Assert.IsFalse(validator.isValidateOk("B123456789:山手商店"))

        End Sub

        <Test()>
        Public Sub isNumericOk()

            Dim validator As New GNumeric

            Assert.AreEqual("gs-number", validator.CssClass)
            Assert.IsTrue(validator.hasMarker(GetType(GMarkerNumeric)))

            Assert.IsTrue(validator.isValidateOk("123"))
            Assert.IsTrue(validator.isValidateOk("123.123"))
            Assert.IsFalse(validator.isValidateOk("+123.123"))
            Assert.IsFalse(validator.isValidateOk("-123.123"))

            validator.WithSign = True

            Assert.IsTrue(validator.isValidateOk("+123.123"))
            Assert.IsTrue(validator.isValidateOk("-123.123"))

            Assert.IsFalse(validator.isValidateOk("abc"))
            Assert.IsFalse(validator.isValidateOk("１２３"))
            Assert.IsFalse(validator.isValidateOk("123,456"))

        End Sub

        <Test()>
        Public Sub isNumberOk()

            Dim validator As New GNumber

            Assert.AreEqual("gs-number", validator.CssClass)
            Assert.IsTrue(validator.isValidateOk("123"))
            Assert.IsFalse(validator.isValidateOk("123.123"))
            Assert.IsFalse(validator.isValidateOk("+123.123"))
            Assert.IsFalse(validator.isValidateOk("-123.123"))

        End Sub

        <Test()>
        Public Sub isPeriodPositionOk()

            Dim validator As New GPeriodPositionOk(5, 3)

            Assert.AreEqual("gs-number", validator.CssClass)
            Assert.IsTrue(validator.isValidateOk("12345"))
            Assert.IsTrue(validator.isValidateOk("12345.12"))
            Assert.IsTrue(validator.isValidateOk("12345.123"))
            Assert.IsFalse(validator.isValidateOk("123456.123"))
            Assert.IsFalse(validator.isValidateOk("12345.1234"))
            Assert.IsTrue(validator.isValidateOk("1234.123"))
            Assert.IsTrue(validator.isValidateOk("123.12"))

            validator.AfterP() = 0
            Assert.IsTrue(validator.isValidateOk("12345"))
            Assert.IsFalse(validator.isValidateOk("12.1"))

        End Sub

        <Test()>
        Public Sub isRequiredOk()
            Dim validator As New GRequired

            Assert.IsTrue(validator.isValidateOk("x"))
            Assert.IsFalse(validator.isValidateOk(""))
            Assert.IsFalse(validator.isValidateOk("　"))
            Assert.IsFalse(validator.isValidateOk(" "))
            Assert.IsFalse(validator.isValidateOk(Nothing))

        End Sub

        <Test()>
        Public Sub isStartWithOk()
            Dim validator As New GStartWith("ABC")

            Assert.IsTrue(validator.isValidateOk(""))
            Assert.IsTrue(validator.isValidateOk("ABC"))
            Assert.IsTrue(validator.isValidateOk("ABCDEF"))

            validator.Prefix = "あいう"
            Assert.IsTrue(validator.isValidateOk("あいう"))
            Assert.IsTrue(validator.isValidateOk("あいうえお"))

            Assert.IsFalse(validator.isValidateOk("あい"))
            Assert.IsFalse(validator.isValidateOk("あ"))

        End Sub

    End Class


End Namespace
