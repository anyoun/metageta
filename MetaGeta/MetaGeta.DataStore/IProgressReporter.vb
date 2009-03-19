Public Interface IProgressReportCallback(Of T)
    Sub SetItems(ByVal items As IEnumerable(Of T))
    Sub SetCurrentItem(ByVal index As Integer)
    Sub SetItemProgress(ByVal percent As Double)
End Interface
