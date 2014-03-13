Imports Microsoft.VisualBasic
Imports Gears
Imports Gears.DataSource

Namespace DataSource

    Public Class DEPTNO
        Inherits GearsDataSource

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.DS("DEPT"))
        End Sub

        Public Overrides Function makeSqlBuilder(ByVal data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(SqlBuilder.S("DEPTNO").ASC)
            sqlb.addSelection(SqlBuilder.S("DNAME"))

            Return sqlb

        End Function


    End Class

End Namespace
