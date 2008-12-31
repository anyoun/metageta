<Serializable()> _
Public Class Bucket
    Inherits FileSet

    Private m_Tag As MGTag

    Public Sub New(ByVal tag As MGTag)
        m_Tag = tag
    End Sub

    Public ReadOnly Property Tag() As MGTag
        Get
            Return m_Tag
        End Get
    End Property

End Class
