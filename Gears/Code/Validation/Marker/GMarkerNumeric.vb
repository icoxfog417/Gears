Imports Microsoft.VisualBasic

Namespace Gears.Validation.Marker

    ''' <summary>
    ''' 数値属性を表すマーカー<br/>
    ''' これを継承する属性については、共通のCssClass gs-number を付与する
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class GMarkerNumeric
        Inherits GearsAttribute
        Implements IAttributeMarker

        ''' <summary>
        ''' 数値属性に共通するCSSClass
        ''' </summary>
        ''' <remarks></remarks>
        Protected Const NUMBER_CSS As String = "gs-number"

        ''' <summary>
        ''' コンストラクタで数値属性共通の値をセット
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            CssClass = NUMBER_CSS
        End Sub

        Public Property DoCast As Boolean = False Implements IAttributeMarker.DoCast

    End Class

End Namespace

