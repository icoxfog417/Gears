
Partial Class pppMaster
    Inherits System.Web.UI.MasterPage

    Private _connectionName As String = "SqLiteConnect"
    Public Property ConnectionName() As String
        Get
            Return _connectionName
        End Get
        Set(ByVal value As String)
            _connectionName = value
        End Set
    End Property

    Private _dsNameSpace As String = ""
    Public Property DsNameSpace() As String
        Get
            Return _dsNameSpace
        End Get
        Set(ByVal value As String)
            _dsNameSpace = value
        End Set
    End Property

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        'ブラウザにキャッシュさせない
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
    End Sub

End Class

