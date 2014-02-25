Imports Microsoft.VisualBasic
Imports Gears

Namespace DataSource

    Public Class DEPTNO
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.newDataSource("DEPT"))
            If GExecutor.getDbServerType = DbServerType.OLEDB Then
                setViewAndTarget(SqlBuilder.newDataSource("[DEPT$]"))
            End If
        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(SqlBuilder.newSelect("DEPTNO").ASC)
            sqlb.addSelection(SqlBuilder.newSelect("DNAME"))

            Return sqlb

        End Function


    End Class

End Namespace
