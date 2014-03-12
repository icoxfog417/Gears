Imports NUnit.Framework
Imports Gears
Imports GearsTest.ControlBuilder
Imports Gears.DataSource
Imports Gears.Util

Namespace GearsTest

    <TestFixture()>
    Public Class GearsRelationTest

        Private Const DefaultConnection As String = "SQLiteConnect"

        ''' <summary>
        ''' コントロールのリレーションを見て、ブランチを作成する
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeBranchByControl()

            'コントロールの準備
            Dim conTree As New Dictionary(Of String, List(Of String))
            conTree.Add("pnlGFORM", New List(Of String) From {"txtNAME", "ddlDEPTNO", "ddlAREA"})
            conTree.Add("grvDATA", Nothing)

            Dim root As Control = ControlBuilder.Build(RelationNode.makeTree(conTree))

            'Mediatorの準備
            Dim mediator As New GearsMediator(DefaultConnection)

            mediator.addControls(root) 'コントロールの登録
            mediator.addControl(ControlSearcher.findControl(root, "grvDATA"))
            mediator.addRelation("ddlDEPTNO", "ddlAREA") '組織->支店へフィルタをかけるリレーションを追加
            mediator.addRelation("txtNAME", "grvDATA") '営業員名->一覧表へフィルタをかけるリレーションを追加

            '関連を考慮したリレーションを作成
            Dim relMap As List(Of RelationNode) = mediator.makeRelationMap(root)
            relMap.ForEach(Sub(n) Console.WriteLine(n))

            'txtNAME->grvDATA,ddlAREA->ddlDEPTNOの2件になる
            Assert.AreEqual(2, relMap.Count)


        End Sub


        ''' <summary>
        ''' 指定されたノードの範囲でブランチを取得する<br/>
        ''' 例:A-B-Cというブランチがあり、Bが指定された場合B-Cを返却する
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeBySelectedNodes()

            'ブランチの準備
            Dim tree As New Dictionary(Of String, List(Of String))
            tree.Add("A", New List(Of String) From {"B", "C"})
            tree.Add("B", New List(Of String) From {"D"})
            tree.Add("C", Nothing)

            Dim node As RelationNode = RelationNode.makeTreeWithRoot(tree)

            Dim branches As List(Of RelationNode) = node.getBranches(New List(Of String) From {"B", "C"})
            branches.ForEach(Sub(n) Console.WriteLine(n))

            Assert.AreEqual(2, branches.Count) 'B-D,とCの2件

        End Sub

        ''' <summary>
        ''' 指定されたノードの範囲でブランチを取得する(複数ブランチ)
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeBySelectedNodesMulti()

            'ブランチの準備
            Dim tree As New Dictionary(Of String, List(Of String))
            tree.Add("A", New List(Of String) From {"B"})
            tree.Add("B", New List(Of String) From {"C", "D"})

            tree.Add("E", New List(Of String) From {"F"})
            tree.Add("F", New List(Of String) From {"G"})

            tree.Add("H", New List(Of String) From {"I"})

            Dim node As RelationNode = RelationNode.makeTreeWithRoot(tree)

            Dim branches As List(Of RelationNode) = node.getBranches(New List(Of String) From {"B", "C", "G", "H"})
            branches.ForEach(Sub(n)
                                 Console.WriteLine(n)
                                 Assert.IsTrue(New List(Of String)() From {"B", "G", "H"}.Contains(n.Value))
                             End Sub)

            Assert.AreEqual(3, branches.Count)

        End Sub

        ''' <summary>
        ''' 指定されたノードの範囲でブランチを取得する<br/>
        ''' 同じブランチのノードが指定された場合、よりルートに近い方が選択される
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeBySelectedSameBranchNodes()

            'ブランチの準備
            Dim tree As New Dictionary(Of String, List(Of String))
            tree.Add("A", New List(Of String) From {"B"})
            tree.Add("B", New List(Of String) From {"C"})
            tree.Add("C", Nothing)

            Dim node As RelationNode = RelationNode.makeTreeWithRoot(tree)

            'BとCではBの方がルートに近いため、Bが選択される
            Dim branches As List(Of RelationNode) = node.getBranches(New List(Of String) From {"B", "C"})
            branches.ForEach(Sub(n) Console.WriteLine(n))

            Assert.AreEqual(1, branches.Count)
            Assert.AreEqual("B", branches.First.Value)

        End Sub


        ''' <summary>
        ''' ブランチの連結が発生しないケースの検証
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeFlatBranch()

            'ブランチの準備
            Dim tree As New Dictionary(Of String, List(Of String))
            tree.Add("A", Nothing)
            tree.Add("B", New List(Of String) From {"B"})

            Dim branches As List(Of RelationNode) = RelationNode.makeTree(tree)
            branches.ForEach(Sub(n) Console.WriteLine(n))

            'ブランチの集約は発生しないため、渡したディクショナリとブランチ数のカウントは等しくなる
            Assert.AreEqual(tree.Count, branches.Count)

        End Sub

        ''' <summary>
        ''' ノードによるブランチの連結が発生するケースの検証<br/>
        ''' 単一のブランチに想定通り統合されることを確認する
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeMergedBranche()

            'ブランチの準備
            Dim tree As New Dictionary(Of String, List(Of String))
            tree.Add("A", New List(Of String) From {"B", "C"})
            tree.Add("B", New List(Of String) From {"D"})
            tree.Add("C", New List(Of String) From {"E", "F"})
            tree.Add("F", New List(Of String) From {"G"})

            Dim branches As List(Of RelationNode) = RelationNode.makeTree(tree)
            branches.ForEach(Sub(n) Console.WriteLine(n))

            'Aをルートとするブランチに統合されるため、ブランチ数は1本になる
            Assert.AreEqual(1, branches.Count)

            '各ノードにおける子要素の数が意図した通りか確認
            Assert.AreEqual(tree("B").Count, branches.First.findNode("B").Children.Count)
            Assert.AreEqual(tree("C").Count, branches.First.findNode("C").Children.Count)
            Assert.AreEqual(tree("F").Count, branches.First.findNode("F").Children.Count)

            '各ノードの親設定が適切であることを確認
            Assert.AreEqual("A", branches.First.findNode("B").Parent.Value)
            Assert.AreEqual("A", branches.First.findNode("C").Parent.Value)

            Assert.AreEqual("B", branches.First.findNode("D").Parent.Value)

            Assert.AreEqual("C", branches.First.findNode("E").Parent.Value)
            Assert.AreEqual("C", branches.First.findNode("F").Parent.Value)

        End Sub

        ''' <summary>
        ''' ノードによりブランチの統合が発生するケースの検証(複数)<br/>
        ''' 細かいブランチ内容の精査はmakeTreeWithMergeで行う。ここでは、最終的なブランチの数をチェックする
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeMergedBranches()

            Dim tree As New Dictionary(Of String, List(Of String))
            '一本目のブランチ
            tree.Add("A", New List(Of String) From {"B", "C"})
            tree.Add("B", New List(Of String) From {"D"})
            tree.Add("D", New List(Of String) From {"E", "F"})

            '二本目のブランチ
            tree.Add("X", New List(Of String) From {"Y"})
            tree.Add("Y", New List(Of String) From {"Z"})

            '三本目のブランチ
            tree.Add("0", New List(Of String) From {"1", "2", "3"})

            Dim branches As List(Of RelationNode) = RelationNode.makeTree(tree)
            branches = RelationNode.makeTree(tree)
            branches.ForEach(Sub(n) Console.WriteLine(n))

            'ブランチの数を確認
            Assert.AreEqual(3, branches.Count)

        End Sub

        ''' <summary>
        ''' ノートの循環が発生する場合の処理
        ''' </summary>
        ''' <remarks></remarks>
        <Test()>
        Public Sub makeCicleRelation()
            'ブランチの準備
            Dim tree As New Dictionary(Of String, List(Of String))
            tree.Add("A", New List(Of String) From {"B"})
            tree.Add("B", New List(Of String) From {"A"})

            Dim branches As List(Of RelationNode) = RelationNode.makeTree(tree)
            branches.ForEach(Sub(n) Console.WriteLine(n))

            'マージは行われず、単純に2つのブランチとなる
            Assert.AreEqual(tree.Count, branches.Count)

        End Sub

    End Class

End Namespace

