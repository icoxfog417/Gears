Imports NUnit.Framework
Imports System.Data.Common
Imports System.Configuration
Imports Gears
Imports Gears.DataSource

Namespace GearsTest

    <TestFixture()>
    Public Class GDSTemplateTest

        Private Const mainConnection As String = "SQLiteConnect"
        Private Const newKey As String = "1000"

        <Test()>
        Public Sub gSelectTest()
            Dim ds As New DataSource.EMP(mainConnection)
            Dim selectDto As New GearsDTO(ActionType.SEL)

            '単純セレクト
            Dim dbData As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM EMP")
            Dim DsData As DataTable = ds.execute(selectDto)
            Assert.AreEqual(dbData.Rows.Count, DsData.Rows.Count)
            Assert.AreEqual(dbData.Rows.Count, ds.gSelectCount(selectDto)) 'カウント検証

            '条件付
            Dim whereDto As New GearsDTO(ActionType.SEL)
            whereDto.addFilter(SqlBuilder.F("JOB").eq("MANAGER"))
            dbData = SimpleDBA.executeSql(mainConnection, "SELECT * FROM EMP WHERE JOB = :manager ", SimpleDBA.makeParameters("manager", "MANAGER"))
            DsData = ds.execute(whereDto)

            Assert.AreEqual(dbData.Rows.Count, DsData.Rows.Count)

        End Sub

        <Test()>
        Public Sub gSelectWithConvertor()
            Dim ds As New DataSource.EMP(mainConnection)
            Dim dname As String = "SALES"
            Dim job As String = "SALESMAN"

            '項目変換マッパー
            Dim mapper As New NameExchangerTemplete()
            mapper.addRule("BUMON", "DNAME") '双方変換
            mapper.addRuleWhenToCol("SHIGOTO", "JOB") '送信時のみ
            mapper.addRuleWhenToItem("SHAIN_NO", "EMPNO") '読取時のみ

            '普通にSELECTした結果
            Dim dbData As DataTable = SimpleDBA.executeSql(mainConnection, "SELECT * FROM V_EMP WHERE DNAME = :pdname AND job = :pjob ORDER BY EMPNO ", SimpleDBA.makeParameters("pdname", dname, "pjob", job))

            'マッパーをセットしたSELECT
            Dim sql As SqlBuilder = ds.makeSqlBuilder(New GearsDTO(ActionType.SEL))
            sql.addFilter(SqlBuilder.F("BUMON").eq(dname))
            sql.addFilter(SqlBuilder.F("SHIGOTO").eq(job))
            sql.addFilter(SqlBuilder.F("EMPNO").gt(0))
            sql.addSelection(SqlBuilder.S("EMPNO").ASC.asNoSelect)
            sql.ItemColExchanger = mapper

            Dim DsData As DataTable = ds.gSelect(sql)
            Assert.AreEqual(dbData.Rows.Count, DsData.Rows.Count)

            Assert.AreEqual(dbData.Rows(0)("EMPNO"), DsData.Rows(0)("SHAIN_NO"))
            Assert.AreEqual(dbData.Rows(0)("JOB"), DsData.Rows(0)("JOB"))
            Assert.AreEqual(dbData.Rows(0)("DNAME"), DsData.Rows(0)("BUMON"))

        End Sub

        <Test()>
        Public Sub gSelectPaging()
            Dim ds As New DataSource.EMP(mainConnection)
            Dim selectDto As New GearsDTO(ActionType.SEL)
            Dim dbData As DataTable = Nothing
            Dim DsData As DataTable = Nothing

            'ページング
            Dim start As Integer = 5
            Dim count As Integer = 3
            selectDto.setPaging(5, 3)
            selectDto.addSelection(SqlBuilder.S("EMPNO").ASC.asNoSelect)
            dbData = SimpleDBA.executeSql(mainConnection, "SELECT * FROM EMP ORDER BY EMPNO LIMIT + " + count.ToString + " OFFSET " + start.ToString)
            DsData = ds.execute(selectDto)

            Assert.AreEqual(count, dbData.Rows.Count)
            Assert.AreEqual(count, DsData.Rows.Count)
            Assert.AreEqual(dbData.Rows.Count, ds.gSelectCount(selectDto)) 'カウント検証

            For i As Integer = 0 To dbData.Rows.Count - 1
                Assert.AreEqual(dbData.Rows(i)("EMPNO"), DsData.Rows(i)("EMPNO"))
            Next

        End Sub

        <Test()>
        Public Sub gSelectCount()

            Dim ds As New DataSource.EMP(mainConnection)
            Dim selectDto As New GearsDTO(ActionType.SEL)
            Dim answerCount As DataTable = Nothing

            'ページング
            selectDto.addSelection(SqlBuilder.S("EMPNO").ASC)
            answerCount = SimpleDBA.executeSql(mainConnection, "SELECT COUNT(*) AS CNT FROM EMP ")

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
            Dim ds As New DataSource.EMP(mainConnection)
            ds.addLockCheckCol("UPD_YMD", LockType.UDATESTR)
            ds.addLockCheckCol("UPD_HMS", LockType.UTIMESTR)
            ds.addLockCheckCol("UPD_USR", LockType.USER)

            'INSERT
            Dim insertDto As New GearsDTO(ActionType.INS)
            insertDto.addSelection(SqlBuilder.S("EMPNO").setValue(newKey).asKey())
            insertDto.addSelection(SqlBuilder.S("ENAME").setValue("ランディージョンソン"))
            insertDto.addSelection(SqlBuilder.S("JOB").setValue("FREE"))

            Try '存在しないキーをUPDATEしようとした場合、エラーになるはず
                insertDto.Action = ActionType.UPD
                ds.execute(insertDto)
            Catch ex As Exception
                Assert.IsInstanceOf(GetType(GearsSqlException), ex)
            End Try

            Try '存在しないキーをDELETEしようとした場合も同様
                insertDto.Action = ActionType.DEL
                ds.execute(insertDto)
            Catch ex As Exception
                Assert.IsInstanceOf(GetType(GearsSqlException), ex)
            End Try

            insertDto.Action = ActionType.INS
            ds.execute(insertDto)

            Assert.AreEqual(1, ds.gResultSet.Rows.Count)
            Assert.AreEqual(newKey, ds.Item("EMPNO"))

            'UPDATE
            Dim updateDto As New GearsDTO(ActionType.UPD)
            updateDto.addSelection(SqlBuilder.S("SAL").setValue("9999"))
            updateDto.addSelection(SqlBuilder.S("HIREDATE").setValue("9000-01-01")) 'DBごとに書式を設定する必要あり(SQLiteだとハイフンつなぎ)
            updateDto.addFilter(SqlBuilder.F("EMPNO").eq(newKey).asKey())
            ds.execute(updateDto)

            Assert.AreEqual(1, ds.gResultSet.Rows.Count)
            Assert.AreEqual("9999", ds.Item("SAL"))

            'DELETE
            updateDto.Action = ActionType.DEL
            ds.execute(updateDto)

            Assert.AreEqual(0, SimpleDBA.executeSql(mainConnection, "SELECT 1 FROM EMP WHERE EMPNO = :emp", SimpleDBA.makeParameters("emp", newKey)).Rows.Count)

        End Sub

        <Test()>
        Public Sub gSaveTest()
            Dim ds As New DataSource.EMP(mainConnection)
            ds.addLockCheckCol("UPD_YMD", LockType.UDATESTR)
            ds.addLockCheckCol("UPD_HMS", LockType.UTIMESTR)

            Dim saveDto As New GearsDTO(ActionType.SAVE)

            saveDto.addSelection(SqlBuilder.S("EMPNO").setValue(newKey).asKey)
            saveDto.addSelection(SqlBuilder.S("ENAME").setValue("山手五郎"))
            saveDto.addSelection(SqlBuilder.S("JOB").setValue("TRAIN"))

            'SAVE
            saveDto.Action = ActionType.SAVE 'INSERT判断
            ds.execute(saveDto)

            Assert.AreEqual(1, ds.gResultSet.Rows.Count)
            Assert.AreEqual(newKey, ds.Item("EMPNO"))

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
            clearDto.addFilter(SqlBuilder.F("EMPNO").eq(newKey).asKey())
            ds.execute(clearDto)

        End Sub




    End Class

End Namespace

