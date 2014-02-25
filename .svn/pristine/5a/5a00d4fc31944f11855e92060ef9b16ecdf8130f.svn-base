Imports NUnit.Framework
Imports System.Configuration
Imports Gears

Namespace GearsTest

    <TestFixture()>
    Public Class SqlBuilderTest

        <TestFixtureSetUp()>
        Public Sub setup()

        End Sub

        <TestFixtureTearDown()>
        Public Sub tearDown()

        End Sub

        Private Shared DbServers As DbServerType() = {DbServerType.Oracle, DbServerType.SQLServer, DbServerType.OLEDB}

        <Test(), TestCaseSource("DbServers")>
        Public Sub sqlSelectBasic(ds As DbServerType)
            Dim sqlbd = New SqlBuilder(ds, ActionType.SEL)
            sqlbd.IsMultiByte = True
            Dim answer As String = ""

            Select Case ds
                Case DbServerType.Oracle
                    answer = " SELECT t.""COL1"" AS ""COL1_IS_COLUMN1"", t.""COL2"",count(*) AS ""件数"" FROM SCHEMA.""TAB"" t WHERE t.""COL1"" = :p1 GROUP BY t.""COL1"",t.""COL2"" ORDER BY t.""COL1"" ASC "
                Case DbServerType.SQLServer, DbServerType.OLEDB
                    answer = " SELECT t.[COL1] AS [COL1_IS_COLUMN1], t.[COL2],count(*) AS [件数] FROM SCHEMA.[TAB] t WHERE t.[COL1] = @p1 GROUP BY t.[COL1],t.[COL2] ORDER BY t.[COL1] ASC "
            End Select

            sqlbd.addSelection(SqlBuilder.newSelect("COL1", "t").asName("COL1_IS_COLUMN1").inGroup.ASC)
            sqlbd.addSelection(SqlBuilder.newSelect("COL2", "t").inGroup)
            sqlbd.addSelection(SqlBuilder.newFunction("count(*)").asName("件数"))
            sqlbd.setDataSource(SqlBuilder.newDataSource("TAB", "t").inSchema("SCHEMA"))

            Dim filter As SqlFilterItem = SqlBuilder.newFilter("COL1", "t").eq("xxx")
            filter.ParamName = "p1"
            sqlbd.addFilter(filter)

            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)

            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlFilterOfNull()
            Dim sqlbd = New SqlBuilder(DbServerType.Oracle)
            Dim answer As String = "SELECT * FROM TAB WHERE COL1 IS NULL AND COL2 = :F1V0 "

            sqlbd.addFilter(SqlBuilder.newFilter("COL1").eq(Nothing))
            sqlbd.addFilter(SqlBuilder.newFilter("COL2").eq("1"))
            sqlbd.setDataSource(SqlBuilder.newDataSource("TAB"))

            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlFilterOfNot()
            Dim sqlbd = New SqlBuilder(DbServerType.Oracle)
            Dim answer As String = "SELECT * FROM TAB WHERE NOT COL1 IS NULL AND NOT COL2 = :F1V0 AND ( NOT COL3 = :F2V0 OR COL4 = :F3V0 ) "

            Dim group As New SqlFilterGroup("A")
            sqlbd.addFilter(SqlBuilder.newFilter("COL1").eq(Nothing).nots)
            sqlbd.addFilter(SqlBuilder.newFilter("COL2").eq("1").nots)
            sqlbd.addFilter(SqlBuilder.newFilter("COL3").eq("1").inGroup(group).nots)
            sqlbd.addFilter(SqlBuilder.newFilter("COL4").eq("1").inGroup(group))
            sqlbd.setDataSource(SqlBuilder.newDataSource("TAB"))

            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)

            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub


        <Test(), TestCaseSource("DbServers")>
        Public Sub sqlJoin(ds As DbServerType)
            Dim sqlbd = New SqlBuilder(ds)
            Dim answer1 As String = " SELECT * FROM TAB_A t1 INNER JOIN TAB_B t2 ON t1.JCOL1 = t2.JCOL2 AND t1.JCOL3 = t2.JCOL4 "
            Dim answer2 As String = " SELECT * FROM TAB_A t1 LEFT OUTER JOIN TAB_B t2 ON t1.JCOL1 = t2.JCOL1 LEFT OUTER JOIN TAB_C t3 ON t1.JCOL2 = t3.JCOL1 "
            Dim sql As String = ""

            'INNER JOIN
            Dim ds1 As SqlDataSource = SqlBuilder.newDataSource("TAB_A", "t1").innerJoin("TAB_B", "t2", SqlBuilder.newJoinFilter("JCOL1", "JCOL2"), _
                                                                                         SqlBuilder.newJoinFilter("JCOL3", "JCOL4"))
            sqlbd.setDataSource(ds1)
            sql = sqlbd.confirmSql(ActionType.SEL, True)

            Assert.AreEqual(trimAll(answer1), trimAll(sql))

            'LEFT OUTER JOIN
            Dim ds2 As SqlDataSource = SqlBuilder.newDataSource("TAB_A", "t1").leftOuterJoin( _
                "TAB_B", "t2", SqlBuilder.newJoinFilter("JCOL1", "JCOL1")).leftOuterJoin( _
                "TAB_C", "t3", SqlBuilder.newJoinFilter("JCOL2", "JCOL1"))

            sqlbd.setDataSource(ds2)
            sql = sqlbd.confirmSql(ActionType.SEL, True)
            Assert.AreEqual(trimAll(answer2), trimAll(sql))


        End Sub

        <Test(), TestCaseSource("DbServers")>
        Public Sub sqlFilterGrouping(ds As DbServerType)
            Dim sqlbd = New SqlBuilder(ds)
            Dim answer As String = " SELECT * FROM TAB_A WHERE (COL1 = %p%F0V0 AND COL2 = %p%F1V0) AND (COL3 = %p%F2V0 OR COL4 = %p%F3V0) AND ( COL5 = %p%F4V0 OR COL5 = %p%F4V1 ) AND COL6 = %p%F5V0 "
            Select Case ds
                Case DbServerType.Oracle
                    answer = answer.Replace("%p%", ":")
                Case DbServerType.SQLServer, DbServerType.OLEDB
                    answer = answer.Replace("%p%", "@")
            End Select

            Dim groupA As New SqlFilterGroup("A", False)
            Dim groupB As New SqlFilterGroup("B")
            sqlbd.setDataSource(SqlBuilder.newDataSource("TAB_A"))
            sqlbd.addFilter(SqlBuilder.newFilter("COL1").eq("1").inGroup(groupA))
            sqlbd.addFilter(SqlBuilder.newFilter("COL2").eq("2").inGroup(groupA))
            sqlbd.addFilter(SqlBuilder.newFilter("COL3").eq("3").inGroup(groupB))
            sqlbd.addFilter(SqlBuilder.newFilter("COL4").eq("4").inGroup(groupB))
            sqlbd.addFilter(SqlBuilder.newFilter("COL5").eq("5-1" + sqlbd.ValueSeparator + "5-2"))
            sqlbd.addFilter(SqlBuilder.newFilter("COL6").eq("6"))
            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)

            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlFilterAs()
            Dim filter As New SqlFilterItem("TST_COLUMN")
            filter.filterAs("eq", "X")
            Assert.AreEqual("=", filter.Operand)

            filter.filterAs("NEQ", "X")
            Assert.AreEqual("<>", filter.Operand)

            filter.filterAs("lt", "X")
            Assert.AreEqual("<", filter.Operand)

            filter.filterAs("Gt", "X")
            Assert.AreEqual(">", filter.Operand)

            filter.filterAs("ltEq", "X")
            Assert.AreEqual("<=", filter.Operand)

            filter.filterAs("GTEQ", "X")
            Assert.AreEqual(">=", filter.Operand)

            filter.filterAs("like", "X")
            Assert.AreEqual("LIKE", filter.Operand.Trim)

        End Sub


        Private Function trimAll(ByVal str As String) As String
            Return str.Replace(" ", "")

        End Function

    End Class


End Namespace
