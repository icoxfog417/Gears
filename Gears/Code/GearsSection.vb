Imports Microsoft.VisualBasic

Namespace Gears

    Public Class GearsSection
        Inherits ConfigurationSection

        Public Const SectionName As String = "gears"

        <ConfigurationProperty("debug", DefaultValue:="false")>
        Public Property isDebug As Boolean
            Get
                Return CBool(Me("debug"))
            End Get
            Set(value As Boolean)
                Me("debug") = value
            End Set
        End Property

        <ConfigurationProperty("defaultConnection", DefaultValue:="Master.ConnectionName")>
        Public Property DefaultConnection As String
            Get
                Return CStr(Me("defaultConnection"))
            End Get
            Set(value As String)
                Me("defaultConnection") = value
            End Set
        End Property

        <ConfigurationProperty("defaultNamespace", DefaultValue:="Master.DsNamespace")>
        Public Property DefaultNamespace As String
            Get
                Return CStr(Me("defaultNamespace"))
            End Get
            Set(value As String)
                Me("defaultNamespace") = value
            End Set
        End Property

        Public Shared Function Read() As GearsSection
            Dim env As GearsSection = ConfigurationManager.GetSection(SectionName)
            Return env
        End Function

    End Class

End Namespace
