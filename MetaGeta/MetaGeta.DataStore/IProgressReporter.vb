Public Interface IProgressReportCallback
    Sub SetItems(ByVal items As ICollection)
    Sub SetCurrentItem(ByVal index As Integer)
    Sub SetItemProgress(ByVal percent As Double)
End Interface
