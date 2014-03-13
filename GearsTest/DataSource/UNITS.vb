Imports Microsoft.VisualBasic
Imports Gears
Imports Gears.DataSource

Namespace DataSource

    Public Class UNITS
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.DS("UNITS"))
        End Sub

        Public Overrides Function makeSqlBuilder(ByVal data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(SqlBuilder.S("UNITS").ASC)
            sqlb.addSelection(SqlBuilder.S("UNITSNAME"))

            Return sqlb

        End Function

    End Class

End Namespace
