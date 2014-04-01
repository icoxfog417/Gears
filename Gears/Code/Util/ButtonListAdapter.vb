Imports System.Web.UI.WebControls.Adapters

Namespace Gears.Util

    ''' <summary>
    ''' RadioButtonList/CheckboxListのレンダリング方法をカスタマイズするためのクラス<br/>
    ''' 本クラスを使用するためには、App_Browsersフォルダ内の.browserファイルでControlAdapterの指定を行う必要があります。<br/>
    ''' <a href="http://www.singingeels.com/Articles/How_To_Control_Adapters.aspx" target="_blank">How To: Control Adapters</a>を参考<br/>
    ''' この設定を行うことで、以下の処理が可能になります<br/>
    ''' <ul>
    '''   <li>ラジオボタン/チェックボックス本体への属性付与</li>
    '''   <li>jQuery等によるラジオボタン/チェックボックスの値の収集</li>
    ''' </ul>
    ''' 通常の場合、いくらListItemに属性を設定してもspanタグにつくのみでラジオボタン/チェックボックス本体には属性の付与ができませんでした。<br/>
    ''' これが、data-xxxx-ginputというスタイルで属性を付与すると、各inputにdata-xxxxの属性を与えることができるようになります<br/>
    ''' また、data-gnameにコントロールのIDが付与されるため、$("input[data-gname='rblList']")といった形でフォームを収集することができます。
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ButtonListAdapter
        Inherits WebControlAdapter

        Protected Overrides Sub Render(writer As HtmlTextWriter)

            If Me.Control Is Nothing OrElse Not TypeOf Me.Control Is IRepeatInfoUser Then
                MyBase.Render(writer)
                Return
            End If

            'レンダリング対象コントロール
            Dim targetControl As ListControl = CType(Me.Control, ListControl)

            '各リストに付与する属性
            Dim attrs As New Dictionary(Of String, String)
            Dim gattrs As New Dictionary(Of String, String)
            gattrs.Add("data-gname", targetControl.ID) 'セレクタで取得できるように、親IDの値を属性に付与(nameの代わり)
            For Each key As String In targetControl.Attributes.Keys
                If key.StartsWith("data-") And key.EndsWith("-ginput") Then 'data-xxx-ginputで終了するものは、inputの属性として付与する
                    gattrs.Add(key.Replace("-ginput", ""), targetControl.Attributes(key))
                Else
                    attrs.Add(key, targetControl.Attributes(key)) 'その他の属性
                End If
            Next

            'CSSの作成
            Dim css As String = ""
            Dim direction As RepeatDirection = RepeatDirection.Vertical
            Dim layout As RepeatLayout = RepeatLayout.Table 'tableタグは使用しないが、Flowの場合はdivで囲わない

            '縦横レイアウトを判定
            If TypeOf targetControl Is RadioButtonList Then
                direction = CType(targetControl, RadioButtonList).RepeatDirection
                layout = CType(targetControl, RadioButtonList).RepeatLayout
            ElseIf TypeOf targetControl Is CheckBoxList Then
                direction = CType(targetControl, CheckBoxList).RepeatDirection
                layout = CType(targetControl, CheckBoxList).RepeatLayout
            End If

            If Not targetControl.CssClass.Contains("gs-repeat-") Then 'レイアウト属性のCSSがない場合、作成
                If direction = RepeatDirection.Vertical Then
                    css = "gs-repeat-vertical"
                Else
                    css = "gs-repeat-horizontal"
                    If layout = RepeatLayout.Flow Then css += " flow"
                End If
            End If

            If targetControl.CssClass.Length > 0 Then
                css = targetControl.CssClass + If(targetControl.CssClass.Length > 0 And css.Length > 0, " ", "") + css
            End If

            'タグの書き出し開始
            writer.WriteBeginTag("div")

            'ID設定
            writer.WriteAttribute("ID", targetControl.ClientID)

            'CSS 設定
            If css.Length > 0 Then writer.WriteAttribute("class", css)

            '属性設定
            For Each attr As KeyValuePair(Of String, String) In attrs
                writer.WriteAttribute(attr.Key, attr.Value)
            Next

            writer.Write(">")

            Dim repeaterInfo As IRepeatInfoUser = CType(Me.Control, IRepeatInfoUser)

            For i As Integer = 0 To targetControl.Items.Count - 1
                If layout <> RepeatLayout.Flow Then
                    writer.WriteFullBeginTag("div")
                End If

                'この後書き出すinputタグのための属性を設定
                For Each item As KeyValuePair(Of String, String) In gattrs
                    writer.AddAttribute(item.Key, item.Value)
                Next
                repeaterInfo.RenderItem(ListItemType.Item, i, New RepeatInfo(), writer)

                If layout <> RepeatLayout.Flow Then
                    writer.WriteEndTag("div")
                End If
            Next

            writer.WriteEndTag("div")

        End Sub

    End Class


End Namespace
