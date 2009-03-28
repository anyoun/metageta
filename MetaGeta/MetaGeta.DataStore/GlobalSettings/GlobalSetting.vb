Public Class GlobalSetting
    Implements INotifyPropertyChanged, IDataErrorInfo
    Private ReadOnly m_Name As String
    Private ReadOnly m_DefaultValue As String
    Private ReadOnly m_Type As GlobalSettingType

    Private m_Value As String

    Public Sub New(ByVal name As String, ByVal value As String, ByVal defaultValue As String, ByVal type As GlobalSettingType)
        m_Name = name

        m_DefaultValue = defaultValue
        m_Type = type

        If value = m_DefaultValue Then
            m_Value = Nothing
        Else
            m_Value = value
        End If
    End Sub

    Public ReadOnly Property Name() As String
        Get
            Return m_Name
        End Get
    End Property
    Public ReadOnly Property DefaultValue() As String
        Get
            Return m_DefaultValue
        End Get
    End Property
    Public ReadOnly Property Type() As GlobalSettingType
        Get
            Return m_Type
        End Get
    End Property
    Public Property Value() As String
        Get
            If m_Value Is Nothing Then
                Return m_DefaultValue
            Else
                Return m_Value
            End If
        End Get
        Set(ByVal value As String)
            If value <> m_Value Then
                If value = m_DefaultValue Then
                    m_Value = Nothing
                Else
                    m_Value = value
                End If
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Value"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsDefault"))
            End If
        End Set
    End Property
    Public ReadOnly Property ValueOrNull() As String
        Get
            Return m_Value
        End Get
    End Property
    Public ReadOnly Property IsDefault() As Boolean
        Get
            Return m_Value = Nothing
        End Get
    End Property


    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public ReadOnly Property [Error]() As String Implements IDataErrorInfo.Error
        Get
            Return Nothing
        End Get
    End Property

    Default Public ReadOnly Property Item(ByVal propertyName As String) As String Implements System.ComponentModel.IDataErrorInfo.Item
        Get
            If propertyName <> "Value" Then Return Nothing

            Select Case m_Type
                Case GlobalSettingType.Number
                    Dim v As Double
                    If Double.TryParse(Value, v) Then
                        Return Nothing
                    Else
                        Return String.Format("""{0}"" is not a number.", Value)
                    End If
                Case GlobalSettingType.Directory
                    Return If(Directory.Exists(Environment.ExpandEnvironmentVariables(Value)), Nothing, String.Format("Can't find directory ""{0}"".", Value))
                Case GlobalSettingType.File
                    Return If(File.Exists(Environment.ExpandEnvironmentVariables(Value)), Nothing, String.Format("Can't find file ""{0}"".", Value))
                Case GlobalSettingType.LongText
                    Return Nothing
                Case GlobalSettingType.ShortText
                    Return Nothing
                Case Else
                    Throw New Exception()
            End Select
        End Get
    End Property

    Public ReadOnly Property IsValid() As Boolean
        Get
            Return Item("Value") Is Nothing
        End Get
    End Property
End Class
