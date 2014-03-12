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
        ''' フォーム用DTO(自身を更新するためのDTO)の検証
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeFormDTO()

            'コントロールの値を設定
            Dim conValue As New Dictionary(Of String, Object)
            conValue.Add("txtNAME", "MY_NAME")
            conValue.Add("ddlDEPTNO", "20")
            conValue.Add("ddlAREA", "A5")

            'DTOを指定しない場合
            Dim fDto As GearsDTO = makeExecuteDTO(Nothing, conValue)

            Assert.IsTrue(fDto IsNot Nothing)
            Assert.IsTrue(fDto.Action = ActionType.SEL) 'デフォルトは選択になる

            Assert.IsTrue(3, fDto.ControlInfo.Count) 'pnlGFORM内のフォームコントロールは計三個

            For Each cv As KeyValuePair(Of String, Object) In conValue
                Assert.AreEqual(cv.Value, fDto.ControlInfo(cv.Key).First.Value)
            Next

        End Sub

        Private Function makeExecuteDTO(ByVal dto As GearsDTO, ByVal controlValues As Dictionary(Of String, Object)) As GearsDTO

            'フォームコントロールの準備
            Dim conTree As New Dictionary(Of String, List(Of String))
            conTree.Add("pnlGFORM", New List(Of String) From {"lblNAME", "txtNAME", "ddlAREA", "ddlDEPTNO", "btnSave"})

            Dim root As Control = ControlBuilder.Build(RelationNode.makeTree(conTree))
            Dim mediator As New GearsMediator(DefaultConnection, DefaultNamespace)

            ControlBuilder.LoadControls(root, mediator)
            ControlBuilder.SetValues(root, controlValues)

            'コントロールの登録
            mediator.addControls(root) 'lblNAME,btnSaveは対象から外れるはず

            '関連を登録
            mediator.addRelation("ddlAREA", "ddlDEPTNO")


            'DTOを作成
            Dim result As GearsDTO = mediator.makeDTO(root, root, dto)

            Return result

        End Function


    End Class

End Namespace
