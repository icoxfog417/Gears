Imports Microsoft.VisualBasic
Imports System.Data
Namespace Gears

    Public Interface IDataBinder

        Function dataBind(ByRef con As Control, ByRef dset As DataTable) As Boolean
        Function isBindable(ByRef con As Control) As Boolean
        Function dataAttach(ByRef con As Control, ByRef dset As DataTable) As Boolean

        Function getValue(ByRef con As Control) As String
        Sub setValue(ByRef con As Control, ByVal value As String)


    End Interface

End Namespace
