Imports Gears.DataSource
Imports System.Data

Namespace Gears

    Public Class gRuleExpression
        Private _mediator As GearsMediator = Nothing
        Private _control As Control = Nothing
        Private _toControl As List(Of Control) = Nothing

        Public Sub New(ByVal con As Control, ByVal mediator As GearsMediator)
            _mediator = mediator
            _control = con
        End Sub

        Public Function Relate(ParamArray cons As Control()) As gRuleExpression
            Return Relate(cons)
        End Function

        Public Function Relate(ByVal cons As List(Of Control)) As gRuleExpression
            For Each con As Control In cons
                If _mediator.GControl(con.ID) Is Nothing Then '登録されていない場合、自動登録
                    _mediator.addControl(con)
                End If
                _mediator.addRelation(_control, con)
            Next
            _toControl = cons
            Return Me
        End Function

        Public Sub Except(ByVal toControl As Control, ParamArray excepts As String())
            _mediator.addExcept(_control, toControl, excepts)
        End Sub

        Public Sub Except(ParamArray excepts As String())
            If _toControl Is Nothing Then
                _mediator.addExcept(_control, _control, excepts)
            Else
                For Each tocon As Control In _toControl
                    _mediator.addExcept(_control, tocon, excepts)
                Next
            End If
        End Sub

    End Class

    Public Class gSendExpression
        Public Delegate Function ExecuteSend(ByVal fromControl As Control, ByVal toControl As Control, ByVal dto As GearsDTO) As Boolean
        Private _control As Control = Nothing
        Private _dto As GearsDTO = Nothing
        Private _executor As ExecuteSend = Nothing

        Public Sub New(ByVal con As Control, ByVal dto As GearsDTO, ByVal executor As ExecuteSend)
            _control = con
            _dto = dto
            _executor = executor
        End Sub

        Public Function ToThe(ByVal toControl As Control, Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Return _executor(_control, toControl, _dto)
        End Function

        Public Function ToAll(Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Return _executor(_control, Nothing, _dto)
        End Function

        Public Function ToMyself(Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Return _executor(_control, _control, dto)
        End Function

    End Class

    Public Class gSelectExpression
        Private _selection As New List(Of SqlSelectItem)
        Private _mediator As GearsMediator = Nothing

        Public Sub New(ByVal selection As List(Of SqlSelectItem), ByVal mediator As GearsMediator)
            _selection = selection
        End Sub

        Public Function From(ByVal fromControl As Control) As gSourceExpression
            Dim gs As New gSourceExpression(_mediator.GControl(fromControl.ID), _selection)
            Return gs
        End Function

        Public Function From(ByVal source As GearsDataSource) As gSourceExpression
            Dim gs As New gSourceExpression(source, _selection)
            Return gs
        End Function

        Public Function From(ByVal sourceName As String) As gSourceExpression
            Dim gs As New gSourceExpression(_mediator.ConnectionName, sourceName, _selection)
            Return gs
        End Function

    End Class

    Public Class gSourceExpression
        Private _sourceControl As GearsControl = Nothing
        Private _source As GearsDataSource = Nothing
        Private _filter As New List(Of SqlFilterItem)
        Private _selection As New List(Of SqlSelectItem)

        Public Sub New(ByVal gcon As GearsControl, ByVal selection As List(Of SqlSelectItem))
            _sourceControl = gcon
            _selection = selection
        End Sub

        Public Sub New(ByVal source As GearsDataSource, ByVal selection As List(Of SqlSelectItem))
            _source = source
            _selection = selection
        End Sub

        Public Sub New(ByVal conName As String, ByVal sourceName As String, ByVal selection As List(Of SqlSelectItem))
            _source = New GDSTemplate(conName, SqlBuilder.DS(sourceName))
            _selection = selection
        End Sub

        Public Function Where(ByVal dto As GearsDTO) As DataTable
            Return extract(dto)
        End Function

        Public Function Where(ByVal ParamArray filter As SqlFilterItem()) As DataTable
            _filter = filter.ToList
            Return extract()
        End Function

        Public Function Where(ByVal filter As List(Of SqlFilterItem)) As DataTable
            _filter = filter.ToList
            Return extract()
        End Function

        Private Function extract(Optional ByVal dto As GearsDTO = Nothing) As DataTable
            Dim ds As GearsDataSource = Nothing
            Dim myDto As GearsDTO = Nothing
            If dto IsNot Nothing Then
                myDto = New GearsDTO(dto)
            Else
                myDto = New GearsDTO(ActionType.SEL)
            End If

            If _selection IsNot Nothing Then _selection.ForEach(Sub(s) myDto.addSelection(s))
            If _filter IsNot Nothing Then _filter.ForEach(Sub(f) myDto.addFilter(f))

            If _sourceControl IsNot Nothing Then
                ds = _sourceControl.DataSource
            ElseIf _source IsNot Nothing Then
                ds = _source
            End If

            If ds IsNot Nothing Then
                Return ds.gSelect(myDto)
            Else
                Return Nothing
            End If

        End Function

    End Class

End Namespace
