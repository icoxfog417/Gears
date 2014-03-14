
Namespace Gears.Util

    ''' <summary>
    ''' 関連を管理するためのクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class RelationNode
        Public Delegate Function nodeHandler(ByVal node As RelationNode) As String

        Public Property Parent As RelationNode = Nothing
        Private Sub setParent(ByVal node As RelationNode)
            Parent = node
        End Sub

        Public Property Children As New List(Of RelationNode)
        Public Sub addChild(ByRef node As RelationNode)
            node.setParent(Me)
            Children.Add(node)
        End Sub

        Public Property Value As String

        Public Sub New(ByVal value As String)
            Me.Value = value
        End Sub

        Public Sub New(ByVal value As String, ByVal children As List(Of String))
            Me.Value = value
            If children IsNot Nothing Then
                children.ForEach(Sub(c) addChild(New RelationNode(c)))
            End If
        End Sub

        Public Sub New(ByVal value As String, ByVal children As List(Of RelationNode))
            Me.Value = value
            If children IsNot Nothing Then
                Me.Children = children
            End If
        End Sub

        Public Function findNode(ByVal value As String) As RelationNode
            Dim targets As New List(Of RelationNode)

            Dim findOut As nodeHandler = Function(n As RelationNode) As String
                                             If n.Value = value Then targets.Add(n)
                                             Return "X"
                                         End Function

            '子を探索
            Dim childSearch As List(Of String) = visitChildren(findOut)

            '親を探索
            Dim parentSearch As List(Of String) = visitParents(findOut)

            If targets.Count > 0 Then
                Return targets.First
            Else
                Return Nothing
            End If

        End Function

        Public Function getDepth() As Integer
            Dim ps As List(Of String) = visitParents(Function(node As RelationNode) As String
                                                         Return "X"
                                                     End Function)
            Return ps.Count
        End Function

        Public Function visitParents(ByVal func As nodeHandler) As List(Of String)
            Dim result As New List(Of String)

            If Parent IsNot Nothing Then
                Dim ps As List(Of String) = Parent.visitParents(func)
                If ps.Count > 0 Then
                    result.AddRange(ps)
                End If
                result.Add(func(Parent))
            End If

            Return result

        End Function

        Public Function visitChildren(ByVal func As nodeHandler) As List(Of String)
            Dim result As New List(Of String)

            If Not isLeaf Then
                For Each c As RelationNode In Children
                    result.Add(func(c))
                    Dim cs As List(Of String) = c.visitChildren(func)
                    If cs.Count > 0 Then
                        result.AddRange(cs)
                    End If
                Next
            End If

            Return result

        End Function

        Public Function getBranches(ByVal nodes As List(Of String)) As List(Of RelationNode)
            Dim paths As New Dictionary(Of String, RelationNode)

            For Each node As String In nodes
                Dim n As RelationNode = findNode(node)
                If n IsNot Nothing Then
                    Dim elements As List(Of String) = n.visitParents(Function(p As RelationNode) p.Value)
                    elements.Add(n.Value)
                    Dim parentTxt As String = String.Join("/", elements)

                    If Not paths.ContainsKey(parentTxt) Then
                        paths.Add(parentTxt, n)
                    End If
                End If
            Next

            Dim keys As List(Of String) = paths.Keys.ToList
            Dim branchKeys As New List(Of String)

            keys.Sort() '昇順に並び替え

            Dim keyNow As String = ""
            For Each key In keys
                '初回、もしくは新規のルートであるパスの場合追加を行う
                '事前にソートをしているため、A,A/B.A/B/C,Dのようにルートに近い順に並び替えが行われている。
                '要素が含まれなくなったら、別のルートのブランチとなったと判断し追加を行う。
                'また、/を含まないルートについては無条件で追加を行う
                If String.IsNullOrEmpty(keyNow) Or (Not key.Contains("/") OrElse Not key.Contains(keyNow)) Then
                    branchKeys.Add(key)
                    keyNow = key
                End If
            Next

            Dim result As New List(Of RelationNode)
            For Each bkey As String In branchKeys
                result.Add(paths(bkey))
            Next

            Return result

        End Function

        Public Shared Function makeTreeWithRoot(ByVal relations As Dictionary(Of String, List(Of String))) As RelationNode
            Dim trees As List(Of RelationNode) = makeTree(relations)
            If trees.Count > 1 Then
                Dim root As New RelationNode("__ROOT__", trees)
                Return root
            Else
                Return trees.First
            End If
        End Function

        Public Shared Function makeTree(ByVal relations As Dictionary(Of String, List(Of String))) As List(Of RelationNode)
            Dim nodes As New List(Of RelationNode)
            Dim children As New List(Of RelationNode)
            Dim someoneChildren As New List(Of String)
            Dim nodeChecker As New Dictionary(Of String, Boolean)

            '事前準備作業
            For Each rel As KeyValuePair(Of String, List(Of String)) In relations
                '全ての子要素を洗い出す(いずれの子でもないルートノードを発見するため)
                If rel.Value IsNot Nothing Then
                    rel.Value.ForEach(Sub(c) If c IsNot Nothing AndAlso c <> rel.Key Then someoneChildren.Add(c))
                End If

                'ノードに追加したか否かのチェックリストを作成
                nodeChecker.Add(rel.Key, False)
            Next

            '親を収集する
            relations.Keys.ToList.ForEach(Sub(k)
                                              If someoneChildren.IndexOf(k) < 0 Then '子を持たないルートノード
                                                  Dim n As New RelationNode(k, relations(k))
                                                  nodes.Add(n)
                                                  nodeChecker(k) = True 'ノードに入れたためチェックをつける
                                                  children.AddRange(n.Children) 'ルートノードの子要素を収集
                                              End If
                                          End Sub)

            'ツリーを作る(全ノードを格納するまで実行)
            Dim depth As Integer = 1 '初回はルートノードの子要素から探索するため、1から
            While nodeChecker.Values.Where(Function(n) n = False).Count > 0
                '現時点の末端ノードのリストを作成し、それを親として持つ場合既存ノードに設定していく
                Dim lastChildren As New List(Of RelationNode)
                children.ForEach(Sub(c) lastChildren.Add(c))
                children.Clear()

                For Each c As RelationNode In lastChildren
                    '末端ノードと一致するリレーションがあり、まだ追加されていない
                    Dim childKey As String = nodeChecker.Keys.Where(Function(k) k = c.Value And Not nodeChecker(k)).FirstOrDefault

                    If Not String.IsNullOrEmpty(childKey) Then
                        If relations(childKey) IsNot Nothing Then '発見された場合で、子要素を持つ場合追加を行う
                            For Each ans As String In relations(childKey)
                                Dim n As New RelationNode(ans)
                                c.addChild(n)
                                children.Add(n)
                            Next
                        End If
                        nodeChecker(childKey) = True '発見されたためフラグを更新する
                    End If
                Next

                '次回の評価要素が0件となる場合、未追加の要素をそのまま追加し処理を抜ける
                If children.Count = 0 Then
                    '追加されていない要素があれば、そのまま追加し抜ける
                    For Each k As String In nodeChecker.Keys.ToList
                        If Not nodeChecker(k) Then
                            nodes.Add(New RelationNode(k, relations(k)))
                            nodeChecker(k) = True
                        End If
                    Next
                End If

            End While

            Return nodes

        End Function

        Public ReadOnly Property isLeaf As Boolean
            Get
                Return Children.Count = 0
            End Get
        End Property

        Public ReadOnly Property isRoot As Boolean
            Get
                Return Parent Is Nothing
            End Get
        End Property

        Public Overrides Function ToString() As String

            Dim text As String = ""
            Dim childStr As nodeHandler = Function(n As RelationNode) As String
                                              Return vbCrLf + New String("  ", n.getDepth()) + "|-" + n.Value
                                          End Function
            Dim parentStr As nodeHandler = Function(n As RelationNode) As String
                                               Return n.Value + "--"
                                           End Function

            '子を探索
            Dim childTxt As List(Of String) = visitChildren(childStr)

            '親を探索
            Dim parentTxt As List(Of String) = visitParents(parentStr)

            Return String.Join("", parentTxt) + "[" + Value + "]" + String.Join("", childTxt)

        End Function

    End Class

End Namespace
