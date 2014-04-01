Imports Gears.Util
Imports System.Runtime.CompilerServices
Imports Gears

Namespace GearsTest

    ''' <summary>
    ''' テスト用コントロールを組み立てるためのクラス
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ControlBuilder

        Private Sub New()
        End Sub

        Public Shared Function Build(ByVal nodes As List(Of RelationNode)) As Control
            Dim root As Control = createControl("pnlROOT")

            '子要素をコントロールとして追加していく
            For Each node As RelationNode In nodes
                root.Controls.Add(createControl(node.Value))
                If node.Children.Count > 0 Then
                    node.visitChildren(Function(n As RelationNode) As String
                                           Dim parent As Control = ControlSearcher.findControl(root, n.Parent.Value)
                                           If parent IsNot Nothing Then
                                               parent.Controls.Add(createControl(n.Value))
                                           Else
                                               root.Controls.Add(createControl(n.Value))
                                           End If
                                           Return n.Value
                                       End Function)
                End If
            Next

            Return root

        End Function

        Public Shared Sub LoadControls(ByVal root As Control, ByVal mediator As GearsMediator)
            Dim binder As New Gears.Binder.GearsDataBinder
            ControlSearcher.fetchControls(root,
                                          Sub(control As Control, ByRef dto As GearsDTO)
                                              mediator.addControl(control).dataBind()
                                          End Sub,
                                           Function(control As Control) As Boolean
                                               Return mediator.isInputControl(control)
                                           End Function)

        End Sub


        Public Shared Sub SetValues(ByVal root As Control, ByVal keyValue As Dictionary(Of String, Object))
            Dim binder As New Gears.Binder.GearsDataBinder
            ControlSearcher.fetchControls(root,
                                          Sub(control As Control, ByRef dto As GearsDTO)
                                              binder.setValue(control, keyValue(control.ID))
                                          End Sub,
                                           Function(control As Control) As Boolean
                                               Return control IsNot Nothing AndAlso Not String.IsNullOrEmpty(control.ID) AndAlso keyValue.ContainsKey(control.ID)
                                           End Function)

        End Sub

        Public Shared Function createControl(Of T As Control)(ByVal id As String) As T
            Return CType(createControl(id), T)
        End Function

        Public Shared Function createControl(ByVal id As String) As Control
            Dim prefix As String = id.Substring(0, 3).ToUpper
            Dim con As Control = Nothing

            Select Case prefix
                Case "TXT"
                    con = New TextBox
                Case "DDL"
                    con = New DropDownList
                Case "CHB"
                    con = New CheckBox
                Case "CBL"
                    con = New CheckBoxList
                Case "RBL"
                    con = New RadioButtonList
                Case "RBT"
                    con = New RadioButton
                Case "LBL"
                    con = New Label
                Case "LIT"
                    con = New Literal
                Case "HDN"
                    con = New HiddenField
                Case "PNL"
                    con = New Panel
                Case "GRV"
                    con = New GridView
                Case "BTN"
                    con = New Button
            End Select

            con.ID = id

            Return con

        End Function

    End Class

    Public Module ControlExtension

        <Extension()>
        Public Function addKeys(ByVal v As GridView, ByVal ParamArray keys As String()) As GridView
            If Not keys Is Nothing AndAlso keys.Length > 0 Then
                v.DataKeyNames = keys
            End If
            Return v
        End Function

        <Extension()>
        Public Function addAttributes(ByVal wcon As WebControl, ByVal ParamArray attributes As String()) As WebControl
            Dim result As New Hashtable
            If Not attributes Is Nothing AndAlso attributes.Length > 0 Then
                For Each attr As String In attributes
                    Dim kv As String() = attr.Split(":")
                    wcon.Attributes.Add(kv(0), kv(1))
                Next
            End If
            Return wcon
        End Function

    End Module

End Namespace
