Imports NUnit.Framework
Imports Gears

Namespace GearsTest

    <TestFixture()>
    Public Class ModelValidationTest

        <Test()>
        Public Sub testAlert()
            Dim validator As New ModelValidator
            Dim sqlb As New SqlBuilder(ActionType.SEL)
            sqlb.addSelection(SqlBuilder.newSelect("VALUE").setValue("1"))

            Assert.IsTrue(validator.Validate(sqlb).IsValidIgnoreAlert)

            validator.OrderConfirm.Sort()
            For i As Integer = 1 To validator.OrderConfirm.Count
                Assert.AreEqual(i, validator.OrderConfirm(i - 1))
            Next

            Assert.IsTrue(validator.IsEndWithTearDown)
            Assert.AreEqual("ABCDEF", validator.ErrorMessage.Replace(vbCrLf, ""))
            Assert.AreEqual("FOURTH1", validator.ErrorSource)

            sqlb.addSelection(SqlBuilder.newSelect("HOGE").setValue("X"))
            Assert.IsTrue(validator.Validate(sqlb).IsValidIgnoreAlert)

            Dim secondCount As Integer = (From v As Integer In validator.OrderConfirm
                                         Where v = 2
                                         Select v).Count

            Assert.AreEqual(2, secondCount)

        End Sub

        <Test()>
        Public Sub testError()

            Dim validator As New ModelValidator
            Dim sqlb As New SqlBuilder(ActionType.SEL)
            sqlb.addSelection(SqlBuilder.newSelect("VALUE").setValue("0"))

            Assert.IsFalse(validator.Validate(sqlb).IsValid)
            Assert.IsFalse(validator.Validate(sqlb).IsValidIgnoreAlert)
            Assert.AreEqual("CRITICAL", validator.ErrorMessage)
            Assert.AreEqual("FIFTH", validator.ErrorSource)

            validator.OrderConfirm.Sort()
            For i As Integer = 1 To validator.OrderConfirm.Count
                Assert.AreEqual(i, validator.OrderConfirm(i - 1))
            Next

            Assert.IsTrue(validator.IsEndWithTearDown)

        End Sub


    End Class


    '----------------------------------------------------------------------------------------
    '検証用クラス
    Public Class ModelValidator
        Inherits AbsModelValidator

        Private _orderConfirm As New List(Of Integer)
        Public ReadOnly Property OrderConfirm() As List(Of Integer)
            Get
                Return _orderConfirm
            End Get
        End Property

        Private _isEndWithTearDown As Boolean = False
        Public ReadOnly Property IsEndWithTearDown() As Boolean
            Get
                Return _isEndWithTearDown
            End Get
        End Property

        Public Overrides Sub setUpValidation(sqlb As Gears.SqlBuilder)
            MyBase.setUpValidation(sqlb)
            _orderConfirm.Clear()
        End Sub

        Public Overrides Sub tearDownValidation(sqlb As Gears.SqlBuilder)
            MyBase.tearDownValidation(sqlb)
            _isEndWithTearDown = True
        End Sub

        <ModelValidationMethod(FalseAsAlert:=True, Order:=1)>
        Public Function testFirst(ByVal sqlb As SqlBuilder) As Boolean
            _orderConfirm.Add(1)
            Return True
        End Function

        <ModelValidationMethod(FalseAsAlert:=True, OnlyWhenTheseValueExist:="VALUE", Order:=2)>
        Public Function testSecond(ByVal sqlb As SqlBuilder) As Boolean
            ErrorSource = "SECOND"
            _orderConfirm.Add(2)
            Return True
        End Function

        <ModelValidationMethod(FalseAsAlert:=True, OnlyWhenTheseValueExist:="VALUE,HOGE", Order:=2)>
        Public Function testSecondIgnored(ByVal sqlb As SqlBuilder) As Boolean
            ErrorSource = "SECOND"
            _orderConfirm.Add(2)
            Return True

        End Function

        <ModelValidationMethod(FalseAsAlert:=True, Order:=3)>
        Public Function testThird(ByVal sqlb As SqlBuilder) As Boolean
            _orderConfirm.Add(3)
            Return True
        End Function

        <ModelValidationMethod(FalseAsAlert:=True)>
        Public Function testFourthLeft(ByVal sqlb As SqlBuilder) As Boolean
            ErrorSource = "FOURTH1"
            ErrorMessage() = "ABC"
            Return False
        End Function

        <ModelValidationMethod(FalseAsAlert:=True)>
        Public Function testFourthRight(ByVal sqlb As SqlBuilder) As Boolean
            ErrorSource = "FOURTH2"
            ErrorMessage() = "DEF"
            Return False
        End Function

        <ModelValidationMethod()>
        Public Function testFifth(ByVal sqlb As SqlBuilder) As Boolean
            ErrorSource = "FIFTH"

            If getValidateeValue(sqlb, "VALUE") = "1" Then
                Return True
            Else
                ErrorMessage() = "CRITICAL"
                Return False
            End If

        End Function

        Public Function testSixth(ByVal sqlb As SqlBuilder) As Boolean 'アトリビュートがないので実行されないはず
            _orderConfirm.Add(6)
            Return False
        End Function


    End Class


End Namespace



