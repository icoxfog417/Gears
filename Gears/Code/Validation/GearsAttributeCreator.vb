Imports Microsoft.VisualBasic
Imports System.Reflection
Imports Gears.Validation.Validator

Namespace Gears.Validation

    ''' <summary>
    ''' 属性を作成するためのクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GearsAttributeCreator

        ''' <summary>属性を定義するCSSクラスの接頭辞</summary>
        Public Const CSS_HEAD As String = "gears-"

        ''' <summary>属性のコンテナ</summary>
        Private _ac As New GearsAttributeContainer

        ''' <summary>属性のスタイル</summary>
        Private cssClass As String = ""

        ''' <summary>属性の名称空間を明示的に指定(デフォルトはGears.Validation.Validatorから生成される)</summary>
        Public Property AttributeNamespace As String

        ''' <summary>数値型の属性を作成</summary>
        Public Function isNumeric() As GearsAttributeCreator
            _ac.addAttribute(New GNumeric)
            Return Me
        End Function

        ''' <summary>日付型の属性を作成</summary>
        Public Function isDate() As GearsAttributeCreator
            _ac.addAttribute(New GDate)
            Return Me
        End Function

        ''' <summary>必須の属性を作成</summary>
        Public Function isRequired() As GearsAttributeCreator
            _ac.addAttribute(New GRequired)
            Return Me
        End Function

        ''' <summary>項目のバイト長をチェックする属性を作成</summary>
        Public Function isLength(i As Integer) As GearsAttributeCreator
            _ac.addAttribute(New GByteLength(i))
            Return Me
        End Function

        ''' <summary>項目のバイト長間隔をチェックする属性を作成</summary>
        Public Function isLengthBetween(min As Integer, max As Integer) As GearsAttributeCreator
            _ac.addAttribute(New GByteLengthBetween(min, max))
            Return Me
        End Function

        ''' <summary>項目が特定文字で始まるかをチェックする属性を作成</summary>
        Public Function isStartWith(s As String) As GearsAttributeCreator
            _ac.addAttribute(New GStartWith(s))
            Return Me
        End Function

        ''' <summary>項目の整数部/小数点以下桁数をチェックする属性を作成</summary>
        Public Function isPeriodPositionOk(ByVal beforep As Integer, ByVal afterp As Integer) As GearsAttributeCreator
            _ac.addAttribute(New GPeriodPositionOk(beforep, afterp))
            Return Me
        End Function

        ''' <summary>指定正規表現への一致をチェックする属性を作成</summary>
        Public Function isMatch(ByVal pattern As String) As GearsAttributeCreator
            _ac.addAttribute(New GMatch(pattern))
            Return Me
        End Function

        ''' <summary>比較演算でのチェックをする属性を作成</summary>
        Public Function isCompare(ByVal expected As String, Optional ByVal operatorType As String = "=") As GearsAttributeCreator
            Dim attr As New GCompare()
            attr.Expected = expected
            attr.OperatorType = operatorType
            _ac.addAttribute(attr)
            Return Me
        End Function

        Public Sub New()
        End Sub

        ''' <summary>属性の名称空間を明示的に指定</summary>
        ''' <param name="attrNamespace"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal attrNamespace As String)
            AttributeNamespace = attrNamespace
        End Sub

        ''' <summary>
        ''' CSS文字列から属性を作成する<br/>
        ''' CSSはgears-属性クラス名_属性クラスのプロパティ_プロパティの値...という形式で構成される<br/>
        ''' 例:gears-GByteLengthBetween_MinLength_0_Length_18
        ''' </summary>
        ''' <param name="css"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function createAttributesFromString(css As String) As GearsAttributeCreator
            cssClass = css
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

        ''' <summary>
        ''' 単一のCSSクラス名から属性クラスを作成する
        ''' </summary>
        ''' <param name="cssClass"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
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
                    Dim attrType As Type = Type.GetType("Gears.Validation.Validator." + classStr)
                    Dim instance As Object = System.Activator.CreateInstance(attrType)

                    If instance Is Nothing AndAlso Not String.IsNullOrEmpty(AttributeNamespace) Then '代替名称空間で再トライ
                        attrType = Type.GetType(AttributeNamespace + "." + classStr)
                        instance = System.Activator.CreateInstance(attrType)
                    End If

                    If instance IsNot Nothing Then
                        For i As Integer = 1 To extractArgs.Length - 1 Step 2
                            Dim pi As PropertyInfo = attrType.GetProperty(extractArgs(i))
                            If Not pi Is Nothing Then
                                Dim t As Type = pi.PropertyType 'プロパティの型情報を取得
                                pi.SetValue(instance, System.Convert.ChangeType(extractArgs(i + 1), t), Nothing)
                            End If
                        Next

                        result = System.Convert.ChangeType(instance, attrType)

                    End If

                Catch ex As Exception
                    result = Nothing
                End Try

            End If
            Return result

        End Function

        ''' <summary>
        ''' 作成した属性コンテナを取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function pack() As GearsAttributeContainer
            Dim result As New GearsAttributeContainer(_ac)
            '初期化しない方が便利なケースがあるかもしれないが、混乱を避けるならこれがベスト
            'AttributeContainerのコピーは自己責任→内部のAttributeは参照渡しになっているので、コンテナ間のコピーは本当の意味でのコピーにならない。
            'これを回避するには全Attributeにコピーコンストラクタの実装を義務付ける必要があるが、コンストラクタのMustOverrideがどうやら不可能なようで、実装を強制することが難しい。
            'コピー用メソッドを外だししてMustOverrideにする手もあるが、ダウンキャストによる問題もあったりいろいろ面倒なので、とりあえずここまで

            clearCreator()
            Return result
        End Function

        ''' <summary>
        ''' 作成されたCssClassを取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getCssClass() As String
            If cssClass = "" Then
                Return _ac.CssClass
            Else
                Return cssClass
            End If
        End Function

        ''' <summary>
        ''' 作成処理を初期化する
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub clearCreator()
            _ac.clearAttributes()
            cssClass = ""
        End Sub

    End Class

End Namespace

