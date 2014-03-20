Imports Gears.DataSource
Imports System.Data

Namespace Gears

    ''' <summary>
    ''' Gearsにおける関連を作成するためのダミーオブジェクト(Expression)。<br/>
    ''' 下記のようにメソッドチェーンで定義を行うために使用する<br/>
    ''' <code>
    ''' GRule(someControl).Relate(otherControl).Except(thatControl)
    ''' </code>
    ''' </summary>
    ''' <remarks></remarks>
    Public Class gRuleExpression
        Private _mediator As GearsMediator = Nothing
        Private _control As Control = Nothing
        Private _toControl As List(Of Control) = Nothing

        ''' <summary>
        ''' 関連を定義するコントロールと、それを管理するGearsMediatorを受け取る
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="mediator"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal con As Control, ByVal mediator As GearsMediator)
            _mediator = mediator
            _control = con
        End Sub

        ''' <summary>
        ''' 関連を登録する
        ''' </summary>
        ''' <param name="cons"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Relate(ParamArray cons As Control()) As gRuleExpression
            Return Relate(cons.ToList)
        End Function

        ''' <summary>
        ''' 関連を登録する
        ''' </summary>
        ''' <param name="cons"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' 関連を定義する際、明示的に相手を指定する
        ''' </summary>
        ''' <param name="toControl"></param>
        ''' <remarks></remarks>
        Public Function [When](ByVal toControl As Control) As gRuleExpression
            _toControl = New List(Of Control) From {toControl}
            Return Me
        End Function

        ''' <summary>
        ''' 関連から除外するコントロールを指定する
        ''' </summary>
        ''' <param name="excepts"></param>
        ''' <remarks></remarks>
        Public Sub Except(ParamArray excepts As Control())
            If _toControl Is Nothing Then
                _mediator.addExcept(_control, _control, excepts.ToList)
            Else
                For Each tocon As Control In _toControl
                    _mediator.addExcept(_control, tocon, excepts.ToList)
                Next
            End If
        End Sub

    End Class

    ''' <summary>
    ''' GearsでのSend処理(DTOをターゲットとなるDataSourceに届ける)を記述するためのダミーオブジェクト(Expression)<br/>
    ''' 下記のようにメソッドチェーンで記述するために使用する<br/>
    ''' <code>
    ''' GSend(fromControl).ToThe(otherControl,dto)
    ''' </code>
    ''' </summary>
    ''' <remarks></remarks>
    Public Class gSendExpression
        Public Delegate Function ExecuteSend(ByVal fromControl As Control, ByVal toControl As Control, ByVal dto As GearsDTO) As Boolean
        Private _control As Control = Nothing
        Private _dto As GearsDTO = Nothing
        Private _executor As ExecuteSend = Nothing

        ''' <summary>
        ''' 起点となるコントロールと、処理定義を受け取る<br/>
        ''' 起点に指定したコントロールからDTOが作成され、送信対象(のDataSource)に送られる
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="executor"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal con As Control, ByVal executor As ExecuteSend)
            _control = con
            _executor = executor
        End Sub

        ''' <summary>
        ''' 送信対象のDTOを直接受け取る<br/>
        ''' コントロールからではなく、直接作成したDTOを相手に送りたい場合に使用する
        ''' </summary>
        ''' <param name="dto"></param>
        ''' <param name="executor"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal dto As GearsDTO, ByVal executor As ExecuteSend)
            _dto = dto
            _executor = executor
        End Sub

        ''' <summary>
        ''' 相手を指定せず、関連先のコントロール全体に対して送信する
        ''' </summary>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToAll(Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Return _executor(_control, Nothing, chooseDto(dto))
        End Function

        ''' <summary>
        ''' 相手を指定して送信する
        ''' </summary>
        ''' <param name="toControl"></param>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToThe(ByVal toControl As Control, Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Return _executor(_control, toControl, chooseDto(dto))
        End Function

        ''' <summary>
        ''' 自分自身に対して送信する
        ''' </summary>
        ''' <param name="dto"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToMyself(Optional ByVal dto As GearsDTO = Nothing) As Boolean
            Return _executor(_control, _control, chooseDto(dto))
        End Function

        Private Function chooseDto(ByVal dto As GearsDTO) As GearsDTO
            If dto IsNot Nothing Then
                Return dto
            Else
                Return _dto
            End If
        End Function

    End Class

    ''' <summary>
    ''' データ抽出処理を記述するためのダミーオブジェクト(Expression)。<br/>
    ''' 抽出条件を指定する
    ''' </summary>
    ''' <remarks></remarks>
    Public Class gSourceExpression
        Private _action As ActionType = ActionType.SEL
        Private _selection As New List(Of SqlSelectItem)
        Private _filter As New List(Of SqlFilterItem)
        Private _source As GearsDataSource = Nothing

        ''' <summary>
        ''' データソースからの抽出を行う
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="selection"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal source As GearsDataSource, action As ActionType, ByVal selection As List(Of SqlSelectItem))
            _action = action
            _source = source
            _selection = selection
        End Sub

        ''' <summary>
        ''' 抽出条件をSqlFilterItemの配列で受け取る
        ''' </summary>
        ''' <param name="filter"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Where(ByVal ParamArray filter As SqlFilterItem()) As DataTable
            _filter = filter.ToList
            Return execute()
        End Function

        ''' <summary>
        ''' 抽出条件をSqlFilterItemの配列で受け取る
        ''' </summary>
        ''' <param name="filter"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Where(ByVal filter As List(Of SqlFilterItem)) As DataTable
            _filter = filter.ToList
            Return execute()
        End Function

        ''' <summary>
        ''' 抽出条件をControlの配列で受け取る
        ''' </summary>
        ''' <param name="cons"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Where(ByVal cons As Control()) As DataTable
            Return Where(cons.ToList)
        End Function

        ''' <summary>
        ''' 抽出条件をControlの配列で受け取る
        ''' </summary>
        ''' <param name="cons"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Where(ByVal cons As List(Of Control)) As DataTable
            Dim filters As New List(Of SqlFilterItem)
            For Each con As Control In cons
                Dim cInfos As List(Of GearsControlInfo) = con.toGControl.toControlInfo
                filters.AddRange(cInfos.Select(Function(c) c.toFilter).ToList)
            Next
            Return Where(filters)
        End Function


        ''' <summary>
        ''' データ抽出処理の実体
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function execute() As DataTable
            Dim myDto As GearsDTO = New GearsDTO(_action)

            If _selection IsNot Nothing Then _selection.ForEach(Sub(s) myDto.addSelection(s))
            If _filter IsNot Nothing Then _filter.ForEach(Sub(f) myDto.addFilter(f))

            If _source IsNot Nothing Then
                Return _source.execute(myDto)
            Else
                Return Nothing
            End If

        End Function

    End Class


  


End Namespace
