Public MustInherit Class DesignOrRuntimeDataContext(Of T)
    Inherits Freezable

    Private m_Data As T = Nothing

    Public ReadOnly Property IsInDesignMode() As Boolean
        Get
            Return DesignerProperties.GetIsInDesignMode(New DependencyObject())
        End Get
    End Property
    Public ReadOnly Property IsInRuntimeMode() As Boolean
        Get
            Return Not IsInDesignMode
        End Get
    End Property

    Public ReadOnly Property Data() As T
        Get
            If m_Data Is Nothing Then
                If IsInDesignMode Then
                    m_Data = CreateDesigntimeData()
                Else
                    m_Data = CreateRuntimeData()
                End If
            End If
            Return m_Data
        End Get
    End Property

    Protected MustOverride Function CreateRuntimeData() As T
    Protected MustOverride Function CreateDesigntimeData() As T
End Class