Imports Microsoft.VisualBasic
Imports Gears
Imports Gears.DataSource

Namespace DataSource

    Public Class EMP
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.DS("V_EMP"), SqlBuilder.DS("EMP"))
            If GExecutor.getDbServerType = DbServerType.OLEDB Then
                setViewAndTarget(SqlBuilder.DS("[EMP$]"))
            End If
        End Sub

    End Class

End Namespace
