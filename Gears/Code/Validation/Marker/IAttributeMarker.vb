Imports Microsoft.VisualBasic

Namespace Gears.Validation.Marker

    ''' <summary>
    ''' 属性をまとめるためのマーカーであることを示すインタフェース
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IAttributeMarker

        ''' <summary>データベース保存時、マーカーの属性に応じたキャストを実行するか否か</summary>
        Property DoCast As Boolean

    End Interface

End Namespace

