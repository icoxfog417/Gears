Imports Gears

Namespace DataSource

    Public Class JOB
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.newDataSource("EMP"))
            If GExecutor.getDbServerType = DbServerType.OLEDB Then
                setViewAndTarget(SqlBuilder.newDataSource("[EMP$]"))
            End If

        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(SqlBuilder.newSelect("JOB").inGroup.ASC)
            sqlb.addSelection(SqlBuilder.newSelect("JOB").asName("JOB_TEXT"))

            Return sqlb

        End Function

    End Class

End Namespace
