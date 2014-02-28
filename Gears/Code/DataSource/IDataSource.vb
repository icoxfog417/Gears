Imports Microsoft.VisualBasic
Imports System.Data

Namespace Gears

    Public Interface IDataSource

        Function gSelect(ByRef data As GearsDTO) As DataTable
        Function gSelectPageBy(ByVal maximumRows As Integer, ByVal startRowIndex As Integer, ByRef data As GearsDTO) As DataTable
        Function gSelectCount(ByRef data As GearsDTO) As Integer

        Sub gInsert(ByRef data As GearsDTO)
        Sub gUpdate(ByRef data As GearsDTO)
        Sub gDelete(ByRef data As GearsDTO)

        Function gResultCount() As Integer
        Function gResultSet() As DataTable

    End Interface

End Namespace
