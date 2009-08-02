﻿Public Class DesignModeDataHelper
    Public Shared ReadOnly DesignTimeDataContextProperty As DependencyProperty = DependencyProperty.RegisterAttached("DesignTimeDataContext", _
                                                                                                                     GetType(Object), _
                                                                                                                     GetType(DesignModeDataHelper), _
                                                                                                                     New PropertyMetadata(AddressOf OnDesignTimeDataContextChanged))

    Private Shared Sub OnDesignTimeDataContextChanged(ByVal obj As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
        SetDesignTimeDataContext(obj, e.NewValue)
    End Sub

    Public Shared Sub SetDesignTimeDataContext(ByVal obj As DependencyObject, ByVal value As Object)
        Dim element = TryCast(obj, FrameworkElement)
 
        If element IsNot Nothing AndAlso DesignerProperties.GetIsInDesignMode(element) Then
            element.DataContext = value
        End If
    End Sub
    Public Shared Function GetDesignTimeDataContext(ByVal obj As DependencyObject) As Object
        Dim element = TryCast(obj, FrameworkElement)

        If element IsNot Nothing AndAlso DesignerProperties.GetIsInDesignMode(element) Then
            Return element.DataContext
        Else
            Return Nothing
        End If
    End Function
End Class
