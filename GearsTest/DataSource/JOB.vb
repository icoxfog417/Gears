Imports Gears
Imports Gears.DataSource

Namespace DataSource

    Public Class JOB
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.DS("EMP"))
        End Sub

        Public Overrides Function makeSqlBuilder(ByVal data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(SqlBuilder.S("JOB").inGroup.ASC)
            sqlb.addSelection(SqlBuilder.S("JOB").asName("JOB_TEXT"))

            Return sqlb

        End Function

    End Class

End Namespace
