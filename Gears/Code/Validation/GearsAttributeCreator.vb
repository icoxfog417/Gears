Imports Microsoft.VisualBasic
Imports System.Reflection

Namespace Gears

    Public Class GearsAttributeCreator

        Public Const CSS_HEAD As String = "gears-"
        Private _ac As New GearsAttributeContainer
        Private cssClass As String = ""

        '個別クラス設定用
        Public Function isNumeric() As GearsAttributeCreator
            _ac.addAttribute(New GNumeric)
            Return Me
        End Function
        Public Function isDate() As GearsAttributeCreator
            _ac.addAttribute(New GDate)
            Return Me
        End Function
        Public Function isRequired() As GearsAttributeCreator
            _ac.addAttribute(New GRequired)
            Return Me
        End Function
        Public Function isLength(i As Integer) As GearsAttributeCreator
            _ac.addAttribute(New GByteLength(i))
            Return Me
        End Function
        Public Function isLengthBetween(min As Integer, max As Integer) As GearsAttributeCreator
            _ac.addAttribute(New GByteLengthBetween(min, max))
            Return Me
        End Function
        Public Function isStartWith(s As String) As GearsAttributeCreator
            _ac.addAttribute(New GStartWith(s))
            Return Me
        End Function
        Public Function isPeriodPositionOk(ByVal beforep As Integer, ByVal afterp As Integer) As GearsAttributeCreator
            _ac.addAttribute(New GPeriodPositionOk(beforep, afterp))
            Return Me
        End Function
        Public Function isMatch(ByVal pattern As String, Optional ByVal whenMatch As Boolean = True) As GearsAttributeCreator
            _ac.addAttribute(New GMatch(pattern, whenMatch))
            Return Me
        End Function
        Public Function isComp(ByVal targetValue As String, Optional ByVal compType As String = "=") As GearsAttributeCreator
            Dim attr As New GComp()
            attr.TgtValue = targetValue
            attr.CompType = compType
            _ac.addAttribute(attr)
            Return Me
        End Function

        'Cssからの設定
        Public Function createAttributesFromString(cs As String) As GearsAttributeCreator
            cssClass = cs
            Dim splitedClass() As String = Split(cssClass, " ")

            For i As Integer = 0 To splitedClass.Length - 1
                Dim attrObj As GearsAttribute = createAttributeFromString(splitedClass(i))
                If Not attrObj Is Nothing Then
                    Dim targetCss As String = attrObj.CssClass
                    cssClass = cssClass.Replace(splitedClass(i), targetCss) 'Cssクラスを置き換え
                    _ac.addAttribute(attrObj)
                End If

            Next
            Return Me
        End Function

        Private Function createAttributeFromString(cssClass As String) As GearsAttribute
            Dim result As GearsAttribute = Nothing
            If Not cssClass Is Nothing AndAlso cssClass.StartsWith(CSS_HEAD) Then
                Dim extractClass() As String = Split(cssClass, CSS_HEAD)
                Dim classStr As String = extractClass(1)

                Dim extractArgs() As String = Split(classStr, "_")

                If extractArgs.Length > 1 Then
                    classStr = extractArgs(0)
                End If

                '動的クラス作成処理
                Try
                    Dim attrType As Type = Type.GetType("Gears." + classStr)
                    Dim instance As Object = System.Activator.CreateInstance(attrType)

                    For i As Integer = 1 To extractArgs.Length - 1 Step 2
                        Dim pi As PropertyInfo = attrType.GetProperty(extractArgs(i))
                        If Not pi Is Nothing Then
                            Dim t As Type = pi.PropertyType 'プロパティの型情報を取得
                            pi.SetValue(instance, System.Convert.ChangeType(extractArgs(i + 1), t), Nothing)
                        End If
                    Next

                    result = System.Convert.ChangeType(instance, attrType)

                Catch ex As Exception
                    result = Nothing
                End Try

            End If
            Return result

        End Function

        Public Function pack() As GearsAttributeContainer
            Dim result As New GearsAttributeContainer(_ac)
            '初期化しない方が便利なケースがあるかもしれないが、混乱を避けるならこれがベスト
            'AttributeContainerのコピーは自己責任→内部のAttributeは参照渡しになっているので、コンテナ間のコピーは本当の意味でのコピーにならない。
            'これを回避するには全Attributeにコピーコンストラクタの実装を義務付ける必要があるが、コンストラクタのMustOverrideがどうやら不可能なようで、実装を強制することが難しい。
            'コピー用メソッドを外だししてMustOverrideにする手もあるが、ダウンキャストによる問題もあったりいろいろ面倒なので、とりあえずここまで

            clearCreator()
            Return result
        End Function

        Public Function getCssClass() As String
            If cssClass = "" Then
                Return _ac.CssClass
            Else
                Return cssClass
            End If
        End Function

        Private Sub clearCreator()
            _ac.clearAttributes()
            cssClass = ""
        End Sub

    End Class

End Namespace

