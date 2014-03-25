Imports NUnit.Framework
Imports System.Data.Common
Imports System.Configuration
Imports Gears
Imports Gears.DataSource

Namespace GearsTest

    <TestFixture()>
    Public Class GearsDataSourceTest

        Private Const DefaultConnection As String = "SQLiteConnect"
        Private Const TestDataNumberIndex As String = "1"
        Private Const KeyForInsert As String = "1010"
        Private Const KeyForUpdate As String = "1011"
        Private Const KeyForDelete As String = "1012"
        Private Const KeyForSave As String = "1013"
        Private Const KeyForException As String = "1014"

        <TestFixtureSetUp()>
        Public Sub setup()
            SimpleDBA.executeSql(DefaultConnection, "DELETE FROM EMP WHERE EMPNO BETWEEN '" + TestDataNumberIndex + "000' AND '" + TestDataNumberIndex + "999' ")

            '1000～1003までのテストデータを作成
            Dim params As New Dictionary(Of String, Object)
            params.Add("F0", "1000")
            params.Add("F1", "DB TARO")
            params.Add("F2", "SALESMAN")
            params.Add("F3", "7698")
            params.Add("F4", "2013-07-01")
            params.Add("F5", "800")
            params.Add("F6", "GearsDataSourceTest")
            params.Add("F7", "10")
            params.Add("F8", "A2")
            params.Add("F9", DateTime.Now.ToString("YYYYMMDD"))
            params.Add("F10", DateTime.Now.ToString("HHmmss"))
            params.Add("F11", "DBUSR")

            SimpleDBA.executeSql(DefaultConnection, "INSERT INTO EMP(EMPNO,ENAME,JOB,MGR,HIREDATE,SAL,COMM,DEPTNO,AREA,UPD_YMD,UPD_HMS,UPD_USR) VALUES (:F0,:F1,:F2,:F3,:F4,:F5,:F6,:F7,:F8,:F9,:F10,:F11)", params)

            params("F0") = "1001"
            params("F1") = "DB SATOKO"
            params("F7") = "30" '異なるDEPTNOを割り当てる
            params("F8") = "A8"

            SimpleDBA.executeSql(DefaultConnection, "INSERT INTO EMP(EMPNO,ENAME,JOB,MGR,HIREDATE,SAL,COMM,DEPTNO,AREA,UPD_YMD,UPD_HMS,UPD_USR) VALUES (:F0,:F1,:F2,:F3,:F4,:F5,:F6,:F7,:F8,:F9,:F10,:F11)", params)

            params("F0") = "1002"
            params("F1") = "DB KOTO"
            params("F2") = "MANAGER"

            SimpleDBA.executeSql(DefaultConnection, "INSERT INTO EMP(EMPNO,ENAME,JOB,MGR,HIREDATE,SAL,COMM,DEPTNO,AREA,UPD_YMD,UPD_HMS,UPD_USR) VALUES (:F0,:F1,:F2,:F3,:F4,:F5,:F6,:F7,:F8,:F9,:F10,:F11)", params)

        End Sub

        <TestFixtureTearDown()>
        Public Sub tearDown()

            SimpleDBA.executeSql(DefaultConnection, "DELETE FROM EMP WHERE EMPNO BETWEEN '" + TestDataNumberIndex + "000' AND '" + TestDataNumberIndex + "999' ")

        End Sub

        <Test()>
        Public Sub gSelect()
            Dim ds As New DataSource.EMP(DefaultConnection)
            Dim selectDto As New GearsDTO(ActionType.SEL)

            '単純セレクト
            Dim dbData As DataTable = SimpleDBA.executeSql(DefaultConnection, "SELECT * FROM EMP")
            Dim DsData As DataTable = ds.execute(selectDto)
            Assert.AreEqual(dbData.Rows.Count, DsData.Rows.Count)
            Assert.AreEqual(dbData.Rows.Count, ds.gSelectCount(selectDto)) 'カウント検証

            '条件付
            Dim whereDto As New GearsDTO(ActionType.SEL)
            whereDto.addFilter(SqlBuilder.F("JOB").eq("SALESMAN"))
            dbData = SimpleDBA.executeSql(DefaultConnection, "SELECT * FROM EMP WHERE JOB = :pjob ", SimpleDBA.makeParameters("pjob", "SALESMAN"))
            DsData = ds.execute(whereDto)

            Assert.AreEqual(dbData.Rows.Count, DsData.Rows.Count)

        End Sub

        <Test()>
        Public Sub gSelectWithConvertor()
            Dim ds As New DataSource.EMP(DefaultConnection)

            '項目変換マッパー
            Dim mapper As New NameMapper()
            mapper.addRule("SHIGOTO", "JOB") '双方変換

            '普通にSELECTした結果
            Dim dbData As DataTable = SimpleDBA.executeSql(DefaultConnection, "SELECT * FROM V_EMP WHERE JOB = 'SALESMAN' AND ENAME LIKE 'DB%' AND EMPNO LIKE '" + TestDataNumberIndex + "%' ORDER BY EMPNO ")

            'マッパーをセットしたSELECT
            Dim sql As SqlBuilder = ds.makeSqlBuilder(New GearsDTO(ActionType.SEL))
            sql.addFilter(SqlBuilder.F("SHIGOTO").eq("SALESMAN"))
            sql.addSelection(SqlBuilder.S("EMPNO").ASC.asNoSelect)
            sql.Mapper = mapper

            Dim DsData As DataTable = ds.gSelect(sql) '変換のかかったデータテーブル
            Assert.AreEqual(dbData.Rows.Count, DsData.Rows.Count)

            Assert.AreEqual(dbData.Rows(0)("JOB"), DsData.Rows(0)("SHIGOTO")) '送信/読み取り双方での変換を確認

        End Sub

        <Test()>
        Public Sub gSelectPaging()
            Dim ds As New DataSource.EMP(DefaultConnection)
            Dim selectDto As New GearsDTO(ActionType.SEL)
            Dim dbData As DataTable = Nothing
            Dim dsData As DataTable = Nothing

            'ページング設定
            Dim start As Integer = 2
            Dim count As Integer = 1
            selectDto.setPaging(start, count)
            selectDto.Add(SqlBuilder.S("EMPNO").ASC.asNoSelect)
            selectDto.Add(SqlBuilder.F("EMPNO").likes("1%"))
            dsData = ds.execute(selectDto)

            '理論値
            dbData = SimpleDBA.executeSql(DefaultConnection, "SELECT * FROM EMP WHERE EMPNO LIKE '" + TestDataNumberIndex + "%' ORDER BY EMPNO LIMIT + " + count.ToString + " OFFSET " + start.ToString)

            Assert.AreEqual(count, dbData.Rows.Count)
            Assert.AreEqual(count, dsData.Rows.Count)

            For i As Integer = 0 To dbData.Rows.Count - 1
                Assert.AreEqual(dbData.Rows(i)("EMPNO"), dsData.Rows(i)("EMPNO"))
            Next

        End Sub

        <Test()>
        Public Sub gSelectCount()

            Dim ds As New DataSource.EMP(DefaultConnection)
            Dim selectDto As New GearsDTO(ActionType.SEL)
            Dim answerCount As DataTable = Nothing

            'ページング
            selectDto.addSelection(SqlBuilder.S("EMPNO").ASC)
            answerCount = SimpleDBA.executeSql(DefaultConnection, "SELECT COUNT(*) AS CNT FROM EMP ")

            Dim count As Integer = ds.gSelectCount(selectDto)

            Assert.AreEqual(count, CInt(answerCount.Rows(0).Item("CNT")))

        End Sub

        '<Test()>
        'Public Sub commandTimeout()
        '    Dim sourceSql As String = ""
        '    Dim loopCount As Integer = 400
        '    For i As Integer = 0 To loopCount
        '        sourceSql += " SELECT COMM,JOB,STDEV(SAL+MGR) AS SAL,STDEV(MGR-SAL) AS MGR FROM V_EMP GROUP BY COMM,JOB "
        '        If i < loopCount Then
        '            sourceSql += " UNION ALL "
        '        End If
        '    Next

        '    Dim sourceDs As New SqlDataSource("(SELECT COMM,JOB,STDEV(SAL+MGR) AS SAL,STDEV(MGR-SAL) AS MGR FROM ( " + sourceSql + " ) GROUP BY COMM,JOB)")

        '    Dim ds As New GearsDataSource(mainConnection, sourceDs)
        '    Dim selectDto As New GearsDTO(ActionType.SEL)
        '    selectDto.CommandTimeout = 1
        '    Dim sqlbuilder As SqlBuilder = ds.makeSqlBuilder(selectDto)

        '    Try
        '        ds.gSelect(selectDto)
        '    Catch ex As Exception
        '        Dim ss As String = "hoge"
        '    End Try


        'End Sub

        <Test()>
        Public Sub gExecuteInsert()
            '事前に消しておく
            deleteRow(KeyForInsert)

            'データソースの用意
            Dim ds As New DataSource.EMP(DefaultConnection)

            'INSERT
            Dim insertDto As New GearsDTO(ActionType.INS)
            insertDto.Add(SqlBuilder.S("EMPNO").setValue(KeyForInsert).asKey())
            insertDto.Add(SqlBuilder.S("ENAME").setValue("ランディージョンソン"))
            insertDto.Add(SqlBuilder.S("JOB").setValue("FREE"))
            insertDto.Action = ActionType.INS
            ds.execute(insertDto)

            Assert.IsTrue(compareDataSet(KeyForInsert, ds.gResultSet))
            Assert.IsFalse(String.IsNullOrEmpty(ds.Item("UPD_YMD").ToString)) '楽観ロックのカラムセットを確認(DataSource/EMPを参照)
            Assert.IsFalse(String.IsNullOrEmpty(ds.Item("UPD_HMS").ToString))

            '終わったら消す
            deleteRow(KeyForInsert)

        End Sub

        <Test()>
        Public Sub gExecuteUpdatet()
            '事前に作成しておく
            insertRow(KeyForUpdate)

            'データソースの用意
            Dim ds As New DataSource.EMP(DefaultConnection)

            'UPDATE
            Dim updateDto As New GearsDTO(ActionType.UPD)
            updateDto.Add(SqlBuilder.F("EMPNO").eq(KeyForUpdate).asKey())
            updateDto.Add(SqlBuilder.S("SAL").setValue("9999"))
            updateDto.Add(SqlBuilder.S("HIREDATE").setValue("9000-01-01")) 'DBごとに書式を設定する必要あり(SQLiteだとハイフンつなぎ)
            ds.execute(updateDto)

            Assert.AreEqual(1, ds.gResultSet.Rows.Count)
            Assert.AreEqual("9999", ds.Item("SAL").ToString)
            Assert.AreEqual("90000101", CDate(ds.Item("HIREDATE")).ToString("yyyyMMdd"))

            '削除
            deleteRow(KeyForUpdate)

        End Sub

        <Test()>
        Public Sub gDelete()
            '事前に作成しておく
            insertRow(KeyForDelete)

            'データソースの用意
            Dim ds As New DataSource.EMP(DefaultConnection)

            'DELETE
            Dim deleteDto As New GearsDTO(ActionType.DEL)
            deleteDto.Add(SqlBuilder.F("EMPNO").eq(KeyForDelete).asKey())
            ds.execute(deleteDto)

            Assert.AreEqual(0, SimpleDBA.executeSql(DefaultConnection, "SELECT 1 FROM EMP WHERE EMPNO = :emp", SimpleDBA.makeParameters("emp", KeyForDelete)).Rows.Count)

        End Sub

        <Test()>
        Public Sub gExecuteSaveInsert()
            '事前に消しておく
            deleteRow(KeyForSave)

            Dim ds As New DataSource.EMP(DefaultConnection)

            Dim saveDto As New GearsDTO(ActionType.SAVE)
            saveDto.addSelection(SqlBuilder.S("EMPNO").setValue(KeyForSave).asKey)
            saveDto.addSelection(SqlBuilder.S("ENAME").setValue("INSERT MAN"))
            saveDto.addSelection(SqlBuilder.S("JOB").setValue("TRAIN"))

            ds.execute(saveDto)

            Assert.IsTrue(compareDataSet(KeyForSave, ds.gResultSet))
            deleteRow(KeyForSave)

        End Sub

        <Test()>
        Public Sub gExecuteSaveUpdate()
            '事前に登録しておく
            insertRow(KeyForSave)

            Dim ds As New DataSource.EMP(DefaultConnection)

            Dim saveDto As New GearsDTO(ActionType.SAVE)
            saveDto.addFilter(SqlBuilder.F("EMPNO").eq(KeyForSave).asKey)
            saveDto.Add(SqlBuilder.S("SAL").setValue("9999"))
            saveDto.Add(SqlBuilder.S("HIREDATE").setValue("1000-01-01"))

            ds.execute(saveDto)

            Assert.IsTrue(compareDataSet(KeyForSave, ds.gResultSet))

            deleteRow(KeyForSave)

        End Sub

        <Test()>
        Public Sub ExceptionOptimisticLock()
            '事前に消しておく
            deleteRow(KeyForException)

            Dim ds As New DataSource.EMP(DefaultConnection)

            'レコードの挿入
            Dim saveDto As New GearsDTO(ActionType.SAVE)
            saveDto.addSelection(SqlBuilder.S("EMPNO").setValue(KeyForException).asKey)
            ds.execute(saveDto)

            '挿入したレコードのタイムスタンプとずれた楽観ロック設定を行う
            saveDto.Add(SqlBuilder.F("UPD_YMD").eq("99990101"))

            Assert.Throws(Of GearsOptimisticLockException)(Sub()
                                                               ds.execute(saveDto)
                                                           End Sub)


            deleteRow(KeyForException)

        End Sub

        <Test()>
        Public Sub ExceptionKeyUpdate()

            Dim ds As New DataSource.EMP(DefaultConnection)

            'レコードの挿入
            Dim updateDto As New GearsDTO(ActionType.UPD)
            updateDto.IsPermitOtherKeyUpdate = False 'キー更新を許可しない

            updateDto.addSelection(SqlBuilder.S("EMPNO").setValue(KeyForException).asKey)

            Assert.Throws(Of GearsSqlException)(Sub()
                                                    ds.execute(updateDto)
                                                End Sub)

        End Sub

        <Test()>
        Public Sub ExpressionSelectTest()

            Dim ds As New DataSource.EMP(DefaultConnection)
            Dim conNo As New TestFormItem("TXT", "EMPNO")
            Dim conName As Control = ControlBuilder.createControl("txtENAME")

            Dim result As DataTable = ds.gSelect({conNo, conName}).Where()
            Assert.AreEqual(2 + ds.TargetTable.LockCheckColumn.Count, result.Columns.Count)

            conNo.setValue("1000") 'デフォルトインサート済みのデータ

            result = ds.gSelect({conNo, conName}).Where({conNo})
            Assert.AreEqual(1, result.Rows.Count)

        End Sub

        <Test()>
        Public Sub ExpressionSaveTest()

            Dim ds As New DataSource.EMP(DefaultConnection)
            Dim conNo As New TestFormItem("TXT", "EMPNO")
            Dim conName As Control = ControlBuilder.createControl("txtENAME")

            conNo.setValue(KeyForSave)
            CType(conName, TextBox).Text = "EXPRESS"

            Dim result As DataTable = ds.gSave({conNo, conName}).Where(SqlBuilder.F("EMPNO").eq(KeyForSave).asKey)
            Assert.AreEqual(1, result.Rows.Count)
            Assert.AreEqual("EXPRESS", result.Rows(0)("ENAME"))

        End Sub

        Private Function getCount(ByVal key As String) As Integer
            Dim cnt As Integer = SimpleDBA.executeSql(DefaultConnection, "SELECT 1 FROM EMP WHERE EMPNO = :emp", SimpleDBA.makeParameters("emp", key)).Rows.Count
            Return cnt
        End Function

        Private Function compareDataSet(ByVal key As String, ByVal dataset As DataTable) As Boolean
            Dim answer As DataTable = SimpleDBA.executeSql(DefaultConnection, "SELECT * FROM V_EMP WHERE EMPNO = :emp", SimpleDBA.makeParameters("emp", key))

            Dim result As Boolean = True
            result = (answer.Columns.Count = dataset.Columns.Count)

            If result Then
                For Each column As DataColumn In answer.Columns
                    If answer.Rows(0)(column.ColumnName).ToString <> dataset.Rows(0)(column.ColumnName).ToString Then result = False
                Next
            End If

            Return result

        End Function

        Private Sub deleteRow(ByVal key As String)
            SimpleDBA.executeSql(DefaultConnection, "DELETE FROM EMP WHERE EMPNO = :emp", SimpleDBA.makeParameters("emp", key))
        End Sub

        Private Sub insertRow(ByVal key As String)
            SimpleDBA.executeSql(DefaultConnection, "INSERT INTO EMP(EMPNO) VALUES (:emp)", SimpleDBA.makeParameters("emp", key))
        End Sub

    End Class

End Namespace

