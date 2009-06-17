Imports System.ComponentModel
Imports System.Windows

Public Class DependancyObjectNotifyPropertyChanged
    Inherits DependencyObject
    Implements INotifyPropertyChanged

    Protected Overrides Sub OnPropertyChanged(ByVal e As System.Windows.DependencyPropertyChangedEventArgs)
        MyBase.OnPropertyChanged(e)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(e.Property.Name))
    End Sub

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class
