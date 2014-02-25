Namespace GearsTest

    'テスト用のコントロールを組み立てるためのクラス
    Public Class ControlBuilder
        Public Enum MoveDirection
            UPPER
            STAY
            UNDER
        End Enum
        Public Enum ControlType
            TXT
            DDL
            CHB
            CBL
            RBL
            RBT
            LBL
            LIT
            HDN
            PNL
        End Enum


        Private root As Control = Nothing
        Private nodeNow As Control = Nothing

        Public Sub New(ByRef webcon As Control)
            initRoot(webcon)
        End Sub

        Public Function initRoot(ByVal webcon As Control) As ControlBuilder
            root = webcon
            nodeNow = root
            Return Me
        End Function

        Public Function getNodeNow() As Control
            Return nodeNow
        End Function

        Public Function addNode(ByVal webcon As Control, Optional ByVal mv As MoveDirection = MoveDirection.STAY) As ControlBuilder
            nodeNow.Controls.Add(webcon)

            Select Case mv
                Case MoveDirection.UPPER
                    nodeNow = nodeNow.Parent
                    If nodeNow Is Nothing Then
                        nodeNow = root
                    End If
                Case MoveDirection.UNDER
                    nodeNow = webcon
            End Select

            Return Me

        End Function

        Public Function moveNode(ByVal mv As MoveDirection, Optional ByVal childIndex As Integer = 0) As ControlBuilder
            Select Case mv
                Case MoveDirection.UPPER
                    nodeNow = nodeNow.Parent
                    If nodeNow Is Nothing Then
                        nodeNow = root
                    End If
                Case MoveDirection.UNDER
                    If nodeNow.HasControls Then
                        nodeNow = nodeNow.Controls(childIndex)
                    End If
            End Select

            Return Me
        End Function

        Public Shared Function createControl(ByVal conType As ControlType, ByVal id As String, Optional ByRef attributes As Hashtable = Nothing) As Control
            Dim con As Control = Nothing

            Select Case conType
                Case ControlType.TXT
                    con = New TextBox
                Case ControlType.DDL
                    con = New DropDownList
                Case ControlType.CHB
                    con = New CheckBox
                Case ControlType.CBL
                    con = New CheckBoxList
                Case ControlType.RBL
                    con = New RadioButtonList
                Case ControlType.RBT
                    con = New RadioButton
                Case ControlType.LBL
                    con = New Label
                Case ControlType.LIT
                    con = New Literal
                Case ControlType.HDN
                    con = New HiddenField
                Case ControlType.PNL
                    con = New Panel
            End Select

            con.ID = id

            If Not attributes Is Nothing Then

                For Each key As String In attributes.Keys
                    If TypeOf con Is WebControl Then
                        CType(con, WebControl).Attributes.Add(key, attributes(key))
                    End If
                Next

            End If

            Return con

        End Function

        Public Shared Function createView(ByVal id As String, ByVal ParamArray keys As String())
            Dim v As New GridView
            v.ID = id
            If Not keys Is Nothing AndAlso keys.Length > 0 Then
                v.DataKeyNames = keys
            End If
            Return v
        End Function

        Public Shared Function makeAttribute(ByVal ParamArray keyValues As String()) As Hashtable
            Dim result As New Hashtable

            If Not keyValues Is Nothing AndAlso keyValues.Length > 0 Then
                For i As Integer = 0 To keyValues.Length - 1 Step 2
                    result(keyValues(i)) = keyValues(i + 1)
                Next
            End If

            Return result

        End Function

    End Class


End Namespace
