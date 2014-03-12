Imports Microsoft.VisualBasic
Imports Gears
Imports Gears.DataSource

Namespace DataSource

    Public Class AREA
        Inherits GDSTemplate

        Public Sub New(ByVal conStr As String)
            MyBase.New(conStr, SqlBuilder.DS("AREA"))
            If GExecutor.getDbServerType = DbServerType.OLEDB Then
                setViewAndTarget(SqlBuilder.DS("[AREA$]"))
            End If

        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)

            sqlb.addSelection(SqlBuilder.S("AREA").ASC)
            sqlb.addSelection(SqlBuilder.S("AREANAME"))

            Return sqlb

        End Function

    End Class

End Namespace
