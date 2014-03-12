Imports NUnit.Framework
Imports System.Data.Common
Imports System.Configuration
Imports Gears
Imports Gears.DataSource

Namespace GearsTest

    <TestFixture()>
    Public Class GDSTemplateTest

        Private Const DefaultConnection As String = "SQLiteConnect"
        Private Const TestDataNumberIndex As String = "1"
        Private Const SaveKey As String = "1010"

        <TestFixtureSetUp()>
        Public Sub setup()
            SimpleDBA.executeSql(DefaultConnection, "DELETE FROM EMP WHERE EMPNO BETWEEN '" + TestDataNumberIndex + "000' AND '" + TestDataNumberIndex + "999' ")

            '8000～8001までのテストデータを作成
            Dim params As New Dictionary(Of String, String)
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
        Public Sub gSelectTest()
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
            Dim mapper As New NameExchangerTemplate()
            mapper.addRule("SHIGOTO", "JOB") '双方変換
            mapper.addRuleWhenToCol("ONAMAE", "ENAME") '送信時のみ
            mapper.addRuleWhenToItem("EMPNO", "SHAIN_NO") '読取時のみ

            '普通にSELECTした結果
            Dim dbData As DataTable = SimpleDBA.executeSql(DefaultConnection, "SELECT * FROM V_EMP WHERE JOB = 'SALESMAN' AND ENAME LIKE 'DB%' AND EMPNO LIKE '" + TestDataNumberIndex + "%' ORDER BY EMPNO ")

            'マッパーをセットしたSELECT
            Dim sql As SqlBuilder = ds.makeSqlBuilder(New GearsDTO(ActionType.SEL))
            sql.addFilter(SqlBuilder.F("SHIGOTO").eq("SALESMAN")) '送信時の変換を確認
            sql.addFilter(SqlBuilder.F("ONAMAE").likes("DB%")) '送信時の変換を確認
            sql.addFilter(SqlBuilder.F("EMPNO").likes("1%"))
            sql.addSelection(SqlBuilder.S("EMPNO").ASC.asNoSelect)
            sql.ItemColExchanger = mapper

            Dim DsData As DataTable = ds.gSelect(sql) '変換のかかったデータテーブル
            Assert.AreEqual(dbData.Rows.Count, DsData.Rows.Count)

            Assert.AreEqual(dbData.Rows(0)("JOB"), DsData.Rows(0)("SHIGOTO")) '送信/読み取り双方での変換を確認
            Assert.AreEqual(dbData.Rows(0)("ENAME"), DsData.Rows(0)("ENAME")) '送信時のみであることを確認
            Assert.AreEqual(dbData.Rows(0)("EMPNO"), DsData.Rows(0)("SHAIN_NO")) '読み取り時のみであることを確認

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

        '    Dim ds As New GDSTemplate(mainConnection, sourceDs)
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
        Public Sub gExecuteTest()
            '事前に消しておく
            SimpleDBA.executeSql(DefaultConnection, "DELETE FROM EMP WHERE EMPNO = '" + SaveKey + "'")

            '楽観ロックカラムの設定
            Dim ds As New DataSource.EMP(DefaultConnection)
            ds.addLockCheckCol("UPD_YMD", LockType.UDATESTR)
            ds.addLockCheckCol("UPD_HMS", LockType.UTIMESTR)
            ds.addLockCheckCol("UPD_USR", LockType.USER)

            'INSERT
            Dim insertDto As New GearsDTO(ActionType.INS)
            insertDto.addSelection(SqlBuilder.S("EMPNO").setValue(SaveKey).asKey())
            insertDto.addSelection(SqlBuilder.S("ENAME").setValue("ランディージョンソン"))
            insertDto.addSelection(SqlBuilder.S("JOB").setValue("FREE"))
            insertDto.Action = ActionType.INS
            ds.execute(insertDto)

            Assert.AreEqual(1, ds.gResultSet.Rows.Count)
            Assert.AreEqual(SaveKey, ds.Item("EMPNO").ToString)

            'UPDATE
            Dim updateDto As New GearsDTO(ActionType.UPD)
            updateDto.addSelection(SqlBuilder.S("SAL").setValue("9999"))
            updateDto.addSelection(SqlBuilder.S("HIREDATE").setValue("9000-01-01")) 'DBごとに書式を設定する必要あり(SQLiteだとハイフンつなぎ)
            updateDto.addFilter(SqlBuilder.F("EMPNO").eq(SaveKey).asKey())
            ds.execute(updateDto)

            Assert.AreEqual(1, ds.gResultSet.Rows.Count)
            Assert.AreEqual("9999", ds.Item("SAL").ToString)

            'DELETE
            updateDto.Action = ActionType.DEL
            ds.execute(updateDto)

            Assert.AreEqual(0, SimpleDBA.executeSql(DefaultConnection, "SELECT 1 FROM EMP WHERE EMPNO = :emp", SimpleDBA.makeParameters("emp", SaveKey)).Rows.Count)

        End Sub

        <Test()>
        Public Sub gSaveTest()
            Dim ds As New DataSource.EMP(DefaultConnection)
            ds.addLockCheckCol("UPD_YMD", LockType.UDATESTR)
            ds.addLockCheckCol("UPD_HMS", LockType.UTIMESTR)

            Dim saveDto As New GearsDTO(ActionType.SAVE)

            saveDto.addSelection(SqlBuilder.S("EMPNO").setValue(SaveKey).asKey)
            saveDto.addSelection(SqlBuilder.S("ENAME").setValue("山手五郎"))
            saveDto.addSelection(SqlBuilder.S("JOB").setValue("TRAIN"))

            'SAVE
            saveDto.Action = ActionType.SAVE 'INSERT判断
            ds.execute(saveDto)

            Assert.AreEqual(1, ds.gResultSet.Rows.Count)
            Assert.AreEqual(SaveKey, ds.Item("EMPNO").ToString)

            'ロックキーを設定
            Dim savedLock As List(Of SqlFilterItem) = ds.getLockCheckColValue
            saveDto.addLockItems(savedLock)

            saveDto.addSelection(SqlBuilder.S("COMM").setValue("アップ"))
            ds.execute(saveDto) 'UPDATE判断

            Assert.AreEqual(1, ds.gResultSet.Rows.Count)
            Assert.AreEqual("アップ", ds.Item("COMM"))

            'ロックエラー
            saveDto.removeLockItem()
            saveDto.addLockItems(savedLock)
            Try
                ds.execute(saveDto)
            Catch ex As Exception
                Assert.IsInstanceOf(GetType(GearsOptimisticLockException), ex)
            End Try

            'キー変更更新
            saveDto.addFilter(SqlBuilder.F("EMPNO").eq("9999")) 'キー9999をnewKey(1000)に変更して更新、という状況をエミュレート
            saveDto.IsPermitOtherKeyUpdate = False
            Try
                ds.execute(saveDto)
            Catch ex As Exception
                Assert.IsInstanceOf(GetType(GearsSqlException), ex)
            End Try

            saveDto.IsPermitOtherKeyUpdate = True
            saveDto.addSelection(SqlBuilder.S("SAL").setValue("10"))
            saveDto.removeLockItem()
            saveDto.addLockItems(ds.getLockCheckColValue)

            ds.execute(saveDto)

            Assert.AreEqual(1, ds.gResultSet.Rows.Count)
            Assert.AreEqual("10", ds.Item("SAL"))

            '削除しておく
            Dim clearDto As New GearsDTO(ActionType.DEL)
            clearDto.addFilter(SqlBuilder.F("EMPNO").eq(SaveKey).asKey())
            ds.execute(clearDto)

        End Sub




    End Class

End Namespace

