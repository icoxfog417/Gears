Imports Microsoft.VisualBasic

Namespace Gears.Validation.Marker

    ''' <summary>
    ''' 日付属性を表すマーカー<br/>
    ''' これを継承する属性については、共通のCssClass gs-date を付与する
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class GMarkerDate
        Inherits GearsAttribute
        Implements IAttributeMarker

        ''' <summary>
        ''' 日付属性に共通するCSSClass
        ''' </summary>
        ''' <remarks></remarks>
        Protected Const DATE_CSS As String = "gs-date"

        Public Property DoCast As Boolean = False Implements IAttributeMarker.DoCast

        ''' <summary>
        ''' コンストラクタで数値属性共通の値をセット
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            CssClass = DATE_CSS
        End Sub

    End Class

End Namespace

