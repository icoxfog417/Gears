Imports Microsoft.VisualBasic
Imports System.Data

Namespace Gears.DataSource

    Public Interface IDataSource

        Function gSelect(ByVal data As GearsDTO) As DataTable
        Function gSelectPageBy(ByVal maximumRows As Integer, ByVal startRowIndex As Integer, ByVal data As GearsDTO) As DataTable
        Function gSelectCount(ByVal data As GearsDTO) As Integer

        Sub gInsert(ByVal data As GearsDTO)
        Sub gUpdate(ByVal data As GearsDTO)
        Sub gDelete(ByVal data As GearsDTO)

        Function gResultSet() As DataTable

    End Interface

End Namespace
