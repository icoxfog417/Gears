Imports Microsoft.VisualBasic
Imports Gears

Namespace DataSource

    Public Class EMP
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.newDataSource("V_EMP"), SqlBuilder.newDataSource("EMP"))
            If GExecutor.getDbServerType = DbServerType.OLEDB Then
                setViewAndTarget(SqlBuilder.newDataSource("[EMP$]"))
            End If
        End Sub

    End Class

End Namespace
