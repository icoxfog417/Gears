Imports Microsoft.VisualBasic
Imports System.Data
Imports Gears

Namespace GSDataSource

    Public Class EMPNO
        Inherits GDSTemplate
        Public Sub New(ByVal conName As String)
            MyBase.New(conName, SqlBuilder.newDataSource("EMP"))

        End Sub

        Public Overrides Function makeSqlBuilder(ByRef data As Gears.GearsDTO) As SqlBuilder
            Dim sqlb As SqlBuilder = MyBase.makeSqlBuilder(data)
            'sqlb.addSelection(SqlBuilder.newSelect("SAMPLE_EMPNO").inGroup().ASC)
            'sqlb.addSelection(SqlBuilder.newSelect("MAX(ENAME)").asName("ENAME"))
            sqlb.addSelection(SqlBuilder.newSelect("EMPNO"))
            sqlb.addSelection(SqlBuilder.newSelect("ENAME"))

            Return sqlb

        End Function


    End Class

End Namespace
