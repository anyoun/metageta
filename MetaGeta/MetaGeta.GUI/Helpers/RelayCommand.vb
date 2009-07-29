Public Class RelayCommand(Of T)
    Implements ICommand

    Private ReadOnly m_Execute As Action(Of T)
    Private ReadOnly m_CanExecute As Predicate(Of T)
    Private ReadOnly m_GetParameter As Func(Of T)

    Public Sub New(ByVal execute As Action(Of T))
        Me.New(execute, Nothing)
    End Sub

    Public Sub New(ByVal execute As Action(Of T), ByVal canExecute As Predicate(Of T))
        Me.New(execute, canExecute, Nothing)
    End Sub

    Public Sub New(ByVal execute As Action(Of T), ByVal canExecute As Predicate(Of T), ByVal getParameter As Func(Of T))
        m_Execute = execute
        m_CanExecute = canExecute
        m_GetParameter = getParameter
    End Sub

    Public Function CanExecute(ByVal parameter As Object) As Boolean Implements System.Windows.Input.ICommand.CanExecute
        parameter = If(parameter Is Nothing, parameter, m_GetParameter())
        Return If(m_CanExecute Is Nothing, True, m_CanExecute(CType(parameter, T)))
    End Function

    Public Custom Event CanExecuteChanged As EventHandler Implements System.Windows.Input.ICommand.CanExecuteChanged
        AddHandler(ByVal value As EventHandler)
            AddHandler CommandManager.RequerySuggested, value
        End AddHandler
        RemoveHandler(ByVal value As EventHandler)
            RemoveHandler CommandManager.RequerySuggested, value
        End RemoveHandler
        RaiseEvent(ByVal sender As Object, ByVal e As System.EventArgs)

        End RaiseEvent
    End Event

    Public Sub Execute(ByVal parameter As Object) Implements System.Windows.Input.ICommand.Execute
        parameter = If(parameter Is Nothing, parameter, m_GetParameter())
        m_Execute(CType(parameter, T))
    End Sub
End Class
