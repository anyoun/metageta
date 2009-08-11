Public Class DataStoreBuilder
    Implements INotifyPropertyChanged, IDataErrorInfo

    Private m_Name As String, m_Description As String
    Private m_Template As IDataStoreTemplate
    Private m_DirectoriesToWatch As String, m_Extensions As String

    Public Sub New()
        m_Name = "TV Shows"
        m_DirectoriesToWatch = "F:\ipod\"
        m_Extensions = "mp4;avi;mkv"
    End Sub

    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(ByVal value As String)
            If m_Name <> value Then
                m_Name = value
                OnPropertyChanged("Name")
                OnPropertyChanged("IsValid")
            End If
        End Set
    End Property
    Public Property Description() As String
        Get
            Return m_Description
        End Get
        Set(ByVal value As String)
            If m_Description <> value Then
                m_Description = value
                OnPropertyChanged("Description")
                OnPropertyChanged("IsValid")
            End If
        End Set
    End Property
    Public Property Tempate() As IDataStoreTemplate
        Get
            Return m_Template
        End Get
        Set(ByVal value As IDataStoreTemplate)
            If m_Template IsNot value Then
                m_Template = value
                OnPropertyChanged("Template")
                OnPropertyChanged("IsValid")
            End If
        End Set
    End Property
    Public Property DirectoriesToWatch() As String
        Get
            Return m_DirectoriesToWatch
        End Get
        Set(ByVal value As String)
            If m_DirectoriesToWatch <> value Then
                m_DirectoriesToWatch = value
                OnPropertyChanged("DirectoriesToWatch")
                OnPropertyChanged("IsValid")
            End If
        End Set
    End Property
    Public Property Extensions() As String
        Get
            Return m_Extensions
        End Get
        Set(ByVal value As String)
            If m_Extensions <> value Then
                m_Extensions = value
                OnPropertyChanged("Extensions")
                OnPropertyChanged("IsValid")
            End If
        End Set
    End Property

    Private ReadOnly Property DirectoriesArray() As String()
        Get
            Return DirectoriesToWatch.Split(";"c)
        End Get
    End Property

    Private Sub OnPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public ReadOnly Property [Error]() As String Implements IDataErrorInfo.[Error]
        Get
            Return Nothing
        End Get
    End Property

    Default Public ReadOnly Property Item(ByVal propertyName As String) As String Implements IDataErrorInfo.Item
        Get
            Select Case propertyName
                Case "Name"
                    Return If(String.IsNullOrEmpty(Name), "Please enter a Name.", Nothing)
                Case "DirectoriesToWatch"
                    If DirectoriesArray.Length < 1 Then
                        Return "Please select at least one directory."
                    End If
                    Dim invalidDir = DirectoriesArray.FirstOrDefault(Function(s) Not Directory.Exists(s))
                    Return If(invalidDir Is Nothing, Nothing, String.Format("Can't find directory ""{0}"".", invalidDir))
                Case "Extensions"
                    If Extensions.Length < 1 Then
                        Return "Please select at least one extension."
                    Else
                        Return Nothing
                    End If
                Case Else
                    Return Nothing
            End Select
        End Get
    End Property

    Public ReadOnly Property IsValid() As Boolean
        Get
            Return Item("Name") Is Nothing AndAlso Item("DirectoriesToWatch") Is Nothing AndAlso Item("Extensions") Is Nothing
        End Get
    End Property
End Class