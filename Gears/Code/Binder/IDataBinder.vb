Imports Microsoft.VisualBasic
Imports System.Data

Namespace Gears.Binder

    ''' <summary>
    ''' データバインディング処理を実装するクラスのインタフェース
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IDataBinder

        ''' <summary>
        ''' データのバインド処理を行う
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="dset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function dataBind(ByRef con As Control, ByRef dset As DataTable) As Boolean

        ''' <summary>
        ''' データのバインド処理を行う
        ''' </summary>
        ''' <param name="gcon"></param>
        ''' <param name="dset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function dataBind(ByRef gcon As GearsControl, ByRef dset As DataTable) As Boolean

        ''' <summary>
        ''' バインド対象か否かの判定を行う
        ''' </summary>
        ''' <param name="con"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function isBindable(ByRef con As Control) As Boolean

        ''' <summary>
        ''' 値を設定する処理を行う
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="dset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function dataAttach(ByRef con As Control, ByRef dset As DataTable) As Boolean

        ''' <summary>
        ''' 値を設定する処理を行う
        ''' </summary>
        ''' <param name="gcon"></param>
        ''' <param name="dset"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function dataAttach(ByRef gcon As GearsControl, ByRef dset As DataTable) As Boolean

        ''' <summary>
        ''' コントロールから値を取得する処理
        ''' </summary>
        ''' <param name="con"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function getValue(ByRef con As Control) As Object

        ''' <summary>
        ''' コントロールに値をセットする処理
        ''' </summary>
        ''' <param name="con"></param>
        ''' <param name="value"></param>
        ''' <remarks></remarks>
        Sub setValue(ByRef con As Control, ByVal value As Object)


    End Interface

End Namespace
