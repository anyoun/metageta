Public Interface IDataStoreTemplate
    Function GetDimensionNames() As String()
    Function GetColumnNames() As String()
    Function GetPluginTypeNames() As String()
    Function GetName() As String
End Interface


Public Class TemplateFinder
    Private Shared s_CachedTemplates As Dictionary(Of String, Type)

    Public Shared Function GetTemplateByName(ByVal name As String) As IDataStoreTemplate
        If s_CachedTemplates Is Nothing Then
            'Hard-coded for now, Unity later?
            s_CachedTemplates = New Dictionary(Of String, Type)
            s_CachedTemplates.Add("TVShow", GetType(TVShowDataStoreTemplate))
        End If
        Dim o = Activator.CreateInstance(s_CachedTemplates(name))
        Return CType(o, IDataStoreTemplate)
    End Function
End Class