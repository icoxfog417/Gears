Imports NUnit.Framework
Imports Gears
Imports GearsTest.ControlBuilder
Imports Gears.DataSource
Imports Gears.Util

Namespace GearsTest

    <TestFixture()>
    Public Class GearsMediatorTest

        Private Const DefaultConnection As String = "SQLiteConnect"
        Private Const DefaultNamespace As String = "DataSource"

        ''' <summary>
        ''' 検索用(フォーム以外)のDTOの検証
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeFilterDTO()

            'コントロールの値を設定
            Dim conValue As New Dictionary(Of String, Object)
            conValue.Add("hdnEMPNO__KEY", "1000")
            conValue.Add("txtNAME", "MY_NAME")
            conValue.Add("ddlAREA", "A5")
            conValue.Add("ddlDEPTNO", "20")

            '検索用パネルを準備
            Dim mediator As GearsMediator = setUpMediator("pnlGFilter", conValue)

            'フィルタパネルで選択された値でビューを更新
            Dim pnl As Control = mediator.GControl("pnlGFilter").Control
            Dim dto As GearsDTO = mediator.makeDTO(pnl, mediator.GControl("grvEMP").Control, Nothing)

            Dim sanswer As String = "SELECT * FROM V_EMP WHERE EMPNO =:F0 AND NAME = :F1 AND AREA =:F2 AND DEPTNO = :F3"
            Dim sqlb As SqlBuilder = mediator.GControl("grvEMP").DataSource.makeSqlBuilder(dto)

            Console.WriteLine(sqlb.confirmSql(ActionType.SEL))
            Assert.IsTrue(SqlBuilderTest.compareWithoutSpace(sanswer, sqlb.confirmSql(ActionType.SEL, True)))

        End Sub

        ''' <summary>
        ''' フォーム用DTO(自身を更新するためのDTO)の検証
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeFormDTO()

            'コントロールの値を設定
            Dim conValue As New Dictionary(Of String, Object)
            conValue.Add("hdnEMPNO__KEY", "1000")
            conValue.Add("txtNAME", "MY_NAME")
            conValue.Add("ddlAREA", "A5")
            conValue.Add("ddlDEPTNO", "20")

            '更新フォームを準備
            Dim mediator As GearsMediator = setUpMediator("pnlGFORM", conValue)

            Dim form As Control = mediator.GControl("pnlGFORM").Control

            Dim fDto As GearsDTO = mediator.makeDTO(form, form, Nothing)
            Assert.IsTrue(fDto IsNot Nothing)
            Assert.IsTrue(fDto.Action = ActionType.SEL) 'デフォルトは選択になる

            'フォームに設定されている値が正しいか確認
            For Each cn As KeyValuePair(Of String, Object) In conValue
                Assert.AreEqual(cn.Value, fDto.ControlInfo(cn.Key).First.Value)
            Next

            '更新SQLの確認
            Dim fSql As SqlBuilder = fDto.toSqlBuilder(SqlBuilder.DS("TARGET"))

            Dim sanswer As String = "SELECT EMPNO,NAME,AREA,DEPTNO FROM TARGET WHERE EMPNO = :F0"
            Dim uanswer As String = "UPDATE TARGET SET NAME = :U0,DEPTNO = :U1,AREA = :U2 WHERE EMPNO = :F0"
            Dim ianswer As String = "INSERT INTO TARGET(EMPNO,NAME,DEPTNO,AREA) VALUES (:N0,:N1,:N2,:N3)"
            Dim danswer As String = "DELETE FROM TARGET WHERE EMPNO = :F0"

            '各SQLの確認
            Console.WriteLine(fSql.confirmSql(ActionType.SEL))
            Assert.IsTrue(SqlBuilderTest.compareWithoutSpace(sanswer, fSql.confirmSql(ActionType.SEL, True)))

            Console.WriteLine(fSql.confirmSql(ActionType.UPD))
            SqlBuilderTest.compareWithoutSpace(uanswer, fSql.confirmSql(ActionType.UPD, True))

            Console.WriteLine(fSql.confirmSql(ActionType.INS))
            SqlBuilderTest.compareWithoutSpace(ianswer, fSql.confirmSql(ActionType.INS, True))

            Console.WriteLine(fSql.confirmSql(ActionType.DEL))
            SqlBuilderTest.compareWithoutSpace(ianswer, fSql.confirmSql(ActionType.DEL, True))

        End Sub

        Private Function setUpMediator(ByVal pnlId As String, ByVal controlValues As Dictionary(Of String, Object)) As GearsMediator

            'パネルコントロールの準備
            Dim conTree As New Dictionary(Of String, List(Of String))

            'フォーム部分
            conTree.Add(pnlId, New List(Of String) From {"lblNAME", "hdnEMPNO__KEY", "txtNAME", "ddlAREA", "ddlDEPTNO", "btnSave"})

            '一覧部分
            conTree.Add("pnlDisplay", New List(Of String) From {"grvEMP"})

            Dim root As Control = ControlBuilder.Build(RelationNode.makeTree(conTree))
            Dim mediator As New GearsMediator(DefaultConnection, DefaultNamespace)

            ControlBuilder.LoadControls(root, mediator)
            ControlBuilder.SetValues(root, controlValues)

            'コントロールの登録
            mediator.addControls(root) 'lblNAME,btnSaveは対象から外れるはず

            '関連を登録
            mediator.addRelation("ddlAREA", "ddlDEPTNO")

            Return mediator

        End Function


    End Class

End Namespace
