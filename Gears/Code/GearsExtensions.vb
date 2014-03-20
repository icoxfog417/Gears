Imports Microsoft.VisualBasic
Imports System.Reflection

Namespace Gears

    ''' <summary>
    ''' GearsPage内で使用する拡張ユーティリティ
    ''' </summary>
    ''' <remarks></remarks>
    Public Module GearsPageExtension

        ''' <summary>最初のログを取得する</summary>
        <Runtime.CompilerServices.Extension()> _
        Public Function FirstLog(ByVal log As Dictionary(Of String, GearsException)) As GearsException
            If log IsNot Nothing AndAlso log.Count > 0 Then
                Return log.First.Value
            Else
                Return Nothing
            End If
        End Function

    End Module

    ''' <summary>
    ''' コントロール拡張
    ''' </summary>
    ''' <remarks></remarks>
    Public Module GearsControlExtension

        ''' <summary>ControlからGearsControlを作成する</summary>
        <Runtime.CompilerServices.Extension()> _
        Public Function toGControl(ByVal con As Control) As GearsControl
            Dim gcon As New GearsControl(GearsControl.extractControl(con), String.Empty)
            Return gcon
        End Function

        ''' <summary>ControlからGearsControlを作成する</summary>
        <Runtime.CompilerServices.Extension()> _
        Public Function toGControl(ByVal con As Control, ByVal conName As String, Optional ByVal dns As String = "") As GearsControl
            Dim gcon As New GearsControl(GearsControl.extractControl(con), conName, dns)
            Return gcon
        End Function


    End Module


End Namespace

