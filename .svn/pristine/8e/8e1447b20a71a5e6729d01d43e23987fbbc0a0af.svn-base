Imports _400_kakaku_menu

Partial Class _400_kakaku_menu
    Inherits System.Web.UI.Page

    'ページ初期処理
    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

        'ブラウザにキャッシュさせない()
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
    End Sub

    'ページロード
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'ページタイトルを消去
        Dim pnl As Panel = CType(Me.Page.Master.FindControl("pnlPageTitle"), Panel)
        pnl.Visible = False

    End Sub

    'ページエラー
    Protected Sub Page_Error(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Error

        '例外のErrorイベントはすべてこのPage_Errorイベントで処理する
        AddHandler [Error], AddressOf Me.Page_Error

        Server.Transfer("../home/error.aspx")

    End Sub


End Class
