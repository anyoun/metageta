<Serializable()> _
Public Class Dimension
    Private m_Items As New Dictionary(Of String, Bucket)

    Private m_Name As String

    Public Sub New(ByVal name As String)
        m_Name = name
    End Sub

    Public ReadOnly Property Name() As String
        Get
            Return m_Name
        End Get
    End Property

    Public ReadOnly Property Item(ByVal tagValue As String) As Bucket
        Get
            Dim b As Bucket = Nothing
            If Not m_Items.TryGetValue(tagValue, b) Then
                b = New Bucket(New MGTag(m_Name, tagValue))
                m_Items.Add(tagValue, b)
            End If
            Return b
        End Get
    End Property

    Public Sub IndexNewItem(ByVal file As MGFile)
        Me.Item(file.Tags.Item(m_Name).Value).Add(file)
    End Sub
End Class


Public Interface IDataStoreTemplate
    Function GetDimensionNames() As String()
End Interface