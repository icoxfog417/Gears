Imports Microsoft.VisualBasic
Imports System.Data
Imports Gears

Namespace GSDataSource

    Public Class JOB
        Inherits GDSTemplate
        Public Sub New(ByVal conName As String)
            MyBase.New(conName, SqlBuilder.newDataSource("EMP"))

        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)
            sqlb.addSelection(SqlBuilder.newSelect("JOB").inGroup.ASC)
            sqlb.addSelection(SqlBuilder.newSelect("JOB").asName("JOB_TXT").inGroup)

            Return sqlb

        End Function


    End Class

End Namespace
