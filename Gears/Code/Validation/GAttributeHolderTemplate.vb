Imports Microsoft.VisualBasic

Namespace Gears.Validation

    ''' <summary>
    ''' カスタム属性用のテンプレートクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GAttributeHolderTemplate
        Implements IAttributeHolder

        ''' <summary>バリデーション対象の値と属性を使用し作成する</summary>
        ''' <param name="v"></param>
        ''' <param name="attr"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal v As String, ByVal attr As GearsAttribute)
            _validateeValue = v
            _gattributes = attr
            _gcssClass = _gattributes.CssClass
        End Sub

        ''' <summary>バリデーション対象の値とCSSクラスから作成する(CSSから属性を動的に作成)</summary>
        ''' <param name="v"></param>
        ''' <param name="css"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal v As String, ByVal css As String)
            _validateeValue = v

            Dim attrCreator As New GearsAttributeCreator()
            attrCreator.createAttributesFromString(css)
            _gcssClass = attrCreator.getCssClass
            _gattributes = attrCreator.pack

        End Sub

        Private _gattributes As GearsAttribute = Nothing
        ''' <summary>属性を取得する</summary>
        Public Property GAttribute As GearsAttribute Implements IAttributeHolder.GAttribute
            Get
                Return _gattributes
            End Get
            Set(value As GearsAttribute)
                If TypeOf _gattributes Is GearsAttributeContainer And TypeOf value Is GearsAttribute Then
                    CType(_gattributes, GearsAttributeContainer).addAttribute(value)
                Else
                    _gattributes = value
                End If
            End Set
        End Property

        Private _gcssClass As String = ""
        ''' <summary>CSSクラスを取得する</summary>
        Public ReadOnly Property GCssClass As String Implements IAttributeHolder.GCssClass
            Get
                Return _gcssClass
            End Get
        End Property

        ''' <summary>
        ''' バリデーション結果を取得する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getValidationError() As String Implements IAttributeHolder.getValidationError
            Return _gattributes.ErrorMessage
        End Function

        ''' <summary>
        ''' バリデーションを実施する
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function isValidateOk() As Boolean Implements IAttributeHolder.isValidateOk
            Return _gattributes.isValidateOk(_validateeValue)
        End Function

        Private _validateeValue As String = ""
        ''' <summary>バリデーション対象の値を取得する</summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property validateeValue As String Implements IAttributeHolder.validateeValue
            Get
                Return _validateeValue
            End Get
        End Property

        ''' <summary>
        ''' バリデーション対象の値をセットする
        ''' </summary>
        ''' <param name="value"></param>
        ''' <remarks></remarks>
        Public Sub setValue(ByVal value As String)
            _validateeValue = value
        End Sub

    End Class

End Namespace
