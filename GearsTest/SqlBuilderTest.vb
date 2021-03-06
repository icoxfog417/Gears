﻿Imports NUnit.Framework
Imports System.Configuration
Imports Gears
Imports Gears.DataSource

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

            sqlbd.addSelection(SqlBuilder.S("COL1").pf("t").asName("COL1_IS_COLUMN1").inGroup.ASC)
            sqlbd.addSelection(SqlBuilder.S("COL2").pf("t").inGroup)
            sqlbd.addSelection(SqlBuilder.S("count(*)").asNoFormat.asName("件数"))
            sqlbd.DataSource = (SqlBuilder.DS("TAB", "t").inSchema("SCHEMA"))

            Dim filter As SqlFilterItem = SqlBuilder.F("COL1", "p1").pf("t").eq("xxx")
            sqlbd.addFilter(filter)

            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlFilterSameColumn()
            Dim sqlbd = New SqlBuilder(DbServerType.Oracle)
            Dim answer As String = "SELECT * FROM TAB WHERE ( COL1 = :F0 OR COL1 = :F1 )"

            sqlbd.addFilter(SqlBuilder.F("COL1").eq("1"))
            sqlbd.addFilter(SqlBuilder.F("COL1").eq("2"))
            sqlbd.DataSource = (SqlBuilder.DS("TAB"))

            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlFilterNull()
            Dim sqlbd = New SqlBuilder(DbServerType.Oracle)
            Dim answer As String = "SELECT * FROM TAB WHERE COL1 IS NULL AND COL2 = :F1 "

            sqlbd.addFilter(SqlBuilder.F("COL1").eq(Nothing))
            sqlbd.addFilter(SqlBuilder.F("COL2").eq("1"))
            sqlbd.DataSource = (SqlBuilder.DS("TAB"))

            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlFilterEmpty()
            Dim sqlbd = New SqlBuilder(DbServerType.Oracle)
            Dim answer As String = "SELECT * FROM TAB "

            sqlbd.Add(SqlFilterGroup.Ors(SqlBuilder.F("COL1").eq(""), SqlBuilder.F("COL2").eq("")))
            sqlbd.DataSource = (SqlBuilder.DS("TAB"))

            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlFilterGroupEmpty()
            Dim sqlbd = New SqlBuilder(DbServerType.Oracle)
            Dim answer As String = "SELECT * FROM TAB "

            sqlbd.addFilter(SqlBuilder.F("COL1").eq(""))
            sqlbd.addFilter(SqlBuilder.F("COL2").eq(""))
            sqlbd.DataSource = (SqlBuilder.DS("TAB"))

            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlFilterNot()
            Dim sqlbd = New SqlBuilder(DbServerType.Oracle)
            Dim answer As String = "SELECT * FROM TAB WHERE (NOT COL1 IS NULL AND NOT COL2 = :F1) AND ( NOT COL3 = :G1F0 OR COL4 = :G1F1 ) "

            sqlbd.addFilter(SqlBuilder.F("COL1").eq(Nothing).nots)
            sqlbd.addFilter(SqlBuilder.F("COL2").eq("1").nots)
            sqlbd.Add(SqlFilterGroup.Ors(SqlBuilder.F("COL3").eq("1").nots, SqlBuilder.F("COL4").eq("1")))
            sqlbd.DataSource = (SqlBuilder.DS("TAB"))

            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlFilterGrouping()
            Dim sqlbd = New SqlBuilder(DbServerType.Oracle)
            Dim answer As String = "SELECT * FROM TAB WHERE (NOT COL1 IS NULL AND COL2 = :G0F1) AND ( COL3 >= :G1F0 OR COL4 = :G1F1 ) "

            sqlbd.Add(SqlFilterGroup.Ands(SqlBuilder.F("COL1").eq(Nothing).nots, SqlBuilder.F("COL2").eq("1")))
            sqlbd.Add(SqlFilterGroup.Ors(SqlBuilder.F("COL3").gteq("1"), SqlBuilder.F("COL4").eq("1")))
            sqlbd.DataSource = (SqlBuilder.DS("TAB"))

            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test(), TestCaseSource("DbServers")>
        Public Sub sqlJoin(ds As DbServerType)
            Dim sqlbd = New SqlBuilder(ds)
            Dim answer1 As String = " SELECT * FROM TAB_A t1 INNER JOIN TAB_B t2 ON t1.JCOL1 = t2.JCOL2 AND t1.JCOL3 = t2.JCOL4 "
            Dim answer2 As String = " SELECT * FROM TAB_A t1 LEFT OUTER JOIN TAB_B t2 ON t1.JCOL1 = t2.JCOL1 LEFT OUTER JOIN TAB_C t3 ON t1.JCOL2 = t3.JCOL1 "
            Dim sql As String = ""

            'INNER JOIN
            Dim ds1 As SqlDataSource = SqlBuilder.DS("TAB_A", "t1").innerJoin("TAB_B", "t2", SqlBuilder.F("JCOL1").joinOn("JCOL2"), _
                                                                                         SqlBuilder.F("JCOL3").joinOn("JCOL4"))
            sqlbd.DataSource = (ds1)
            sql = sqlbd.confirmSql(ActionType.SEL, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer1), trimAll(sql))

            'LEFT OUTER JOIN
            Dim ds2 As SqlDataSource = SqlBuilder.DS("TAB_A", "t1").leftOuterJoin( _
                "TAB_B", "t2", SqlBuilder.F("JCOL1").joinOn("JCOL1")).leftOuterJoin( _
                "TAB_C", "t3", SqlBuilder.F("JCOL2").joinOn("JCOL1"))

            sqlbd.DataSource = (ds2)
            sql = sqlbd.confirmSql(ActionType.SEL, True)

            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer2), trimAll(sql))


        End Sub

        <Test()>
        Public Sub sqlUpdate()
            Dim sqlb = New SqlBuilder(DbServerType.Oracle)
            sqlb.DataSource = (SqlBuilder.DS("TARGET"))

            Dim answer As String = "UPDATE TARGET SET COL1 = :U0 , COL2 = :U1 WHERE COL1 = :F0 AND COL2 = :F1 "

            sqlb.Add(SqlBuilder.S("COL1").setValue("V1"))
            sqlb.Add(SqlBuilder.S("COL2").setValue("V2"))
            sqlb.Add(SqlBuilder.F("COL1").eq("K1"))
            sqlb.Add(SqlBuilder.F("COL2").eq("K2"))

            Dim sql As String = sqlb.confirmSql(ActionType.UPD, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlInsert()
            Dim sqlb = New SqlBuilder(DbServerType.Oracle)
            sqlb.DataSource = (SqlBuilder.DS("TARGET"))

            Dim answer As String = "INSERT INTO TARGET(COL1,COL2) VALUES(:N0,:N1) "

            sqlb.Add(SqlBuilder.S("COL1").setValue("V1"))
            sqlb.Add(SqlBuilder.S("COL2").setValue("V2"))
            sqlb.Add(SqlBuilder.F("COL1").eq("K1")) 'フィルタの設定はINSERTに影響を与えない
            sqlb.Add(SqlBuilder.F("COL2").eq("K2"))

            Dim sql As String = sqlb.confirmSql(ActionType.INS, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub sqlDelete()
            Dim sqlb = New SqlBuilder(DbServerType.Oracle)
            sqlb.DataSource = (SqlBuilder.DS("TARGET"))

            Dim answer As String = "DELETE FROM TARGET WHERE ( COL1 = :F0V0 OR COL1 = :F0V1 ) AND COL2 = :F1 "

            sqlb.ValueSeparator = ","
            sqlb.Add(SqlBuilder.F("COL1").eq("K1,K2"))
            sqlb.Add(SqlBuilder.F("COL2").eq("K3"))

            Dim sql As String = sqlb.confirmSql(ActionType.DEL, True)
            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test(), TestCaseSource("DbServers")>
        Public Sub makeFilterGroup(ds As DbServerType)
            Dim sqlbd = New SqlBuilder(ds)
            Dim answer As String = " SELECT * FROM TAB_A WHERE (COL1 = %p%G0F0 AND COL2 = %p%G0F1) AND (COL3 = %p%G1F0 OR COL4 = %p%G1F1) AND (( COL5 = %p%F0V0 OR COL5 = %p%F0V1 ) AND COL6 = %p%F1) "
            Select Case ds
                Case DbServerType.Oracle
                    answer = answer.Replace("%p%", ":")
                Case DbServerType.SQLServer, DbServerType.OLEDB
                    answer = answer.Replace("%p%", "@")
            End Select

            Dim groupA As New SqlFilterGroup("A", False)
            Dim groupB As New SqlFilterGroup("B")
            sqlbd.DataSource = (SqlBuilder.DS("TAB_A"))
            sqlbd.addFilter(SqlBuilder.F("COL1").eq("1").inGroup(groupA))
            sqlbd.addFilter(SqlBuilder.F("COL2").eq("2").inGroup(groupA))
            sqlbd.addFilter(SqlBuilder.F("COL3").eq("3").inGroup(groupB))
            sqlbd.addFilter(SqlBuilder.F("COL4").eq("4").inGroup(groupB))
            sqlbd.addFilter(SqlBuilder.F("COL5").eq("5-1" + sqlbd.ValueSeparator + "5-2"))
            sqlbd.addFilter(SqlBuilder.F("COL6").eq("6"))
            Dim sql As String = sqlbd.confirmSql(ActionType.SEL, True)

            Console.WriteLine(sql)
            Assert.AreEqual(trimAll(answer), trimAll(sql))

        End Sub

        <Test()>
        Public Sub makeFilterAs()
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

            'Likeにおける%ラッパー
            filter.filterAs("like", "X", True)
            Assert.AreEqual("%X%", filter.Value.ToString)

            filter.filterAs("like", "", True) '空白の場合は%でくくらない
            Assert.AreEqual("", filter.Value.ToString)

        End Sub

       
        Public Shared Function compareWithoutSpace(ByVal left As String, ByVal right As String) As Boolean
            Return trimAll(left) = trimAll(right)
        End Function

        Private Shared Function trimAll(ByVal str As String) As String
            Return str.Replace(" ", "")

        End Function

    End Class


End Namespace
