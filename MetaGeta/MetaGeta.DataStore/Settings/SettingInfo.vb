Imports System.Reflection
Imports System.Runtime.CompilerServices

Public Class SettingInfo
    Implements INotifyPropertyChanged

    Private ReadOnly m_TargetObject As Object
    Private ReadOnly m_PropertyInfo As PropertyInfo
    Private ReadOnly m_Metadata As SettingsAttribute

    Public Sub New(ByVal targetObject As Object, ByVal propertyInfo As PropertyInfo, ByVal metadata As SettingsAttribute)
        m_TargetObject = targetObject
        m_PropertyInfo = PropertyInfo
        m_Metadata = metadata
    End Sub

    Public ReadOnly Property SettingName() As String
        Get
            Return m_PropertyInfo.Name
        End Get
    End Property

    Public ReadOnly Property Metadata() As SettingsAttribute
        Get
            Return m_Metadata
        End Get
    End Property

    Public Property Value() As Object
        Get
            Return m_PropertyInfo.GetValue(m_TargetObject, Nothing)
        End Get
        Set(ByVal value As Object)
            m_PropertyInfo.SetValue(m_TargetObject, value, Nothing)
            OnPropertyChanged("IsDefault")
        End Set
    End Property

    Public ReadOnly Property IsDefault() As Boolean
        Get
            Return Object.Equals(Value, Metadata.DefaultValue)
        End Get
    End Property

    Public Function GetValueAsString() As String
        Return ValueToString(Value)
    End Function
    Public Function GetDefaultValueAsString() As String
        Return ValueToString(Metadata.DefaultValue)
    End Function

    Private Function ValueToString(ByVal val As Object) As String
        Select Case Metadata.Type
            Case SettingType.Directory, SettingType.ShortText, SettingType.File, SettingType.LongText
                Return CType(val, String)

            Case SettingType.Int
                Return CType(val, Integer).ToString("D")

            Case SettingType.Float
                Return CType(val, Double).ToString("R")

            Case Else
                Throw New Exception(String.Format("Unknown setting type: {0}.", Metadata.Type))
        End Select
    End Function

    Public Sub SetValueAsString(ByVal s As String)
        Select Case Metadata.Type
            Case SettingType.Directory, SettingType.ShortText, SettingType.File, SettingType.LongText
                Value = s

            Case SettingType.Int
                Value = Integer.Parse(s)

            Case SettingType.Float
                Value = Double.Parse(s)

            Case Else
                Throw New Exception(String.Format("Unknown setting type: {0}.", Metadata.Type))
        End Select
    End Sub

    Private Sub OnPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

End Class


Public Class SettingInfoCollection
    Implements IEnumerable(Of SettingInfo)

    Private ReadOnly m_TargetObject As Object
    Private ReadOnly m_Settings As List(Of SettingInfo)

    Public Sub New(ByVal targetObject As Object, ByVal settings As IEnumerable(Of SettingInfo))
        m_TargetObject = targetObject
        m_Settings = New List(Of SettingInfo)(settings)
    End Sub

    Public Function GetSetting(ByVal name As String) As SettingInfo
        Return m_Settings.First(Function(s) s.SettingName = name)
    End Function

    Public Shared Function GetSettings(ByVal plugin As IMGPluginBase) As SettingInfoCollection
        Dim infos = From p In plugin.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public) _
               Where p.IsDefined(Of SettingsAttribute)() _
               Select New SettingInfo(plugin, p, p.GetCustomAttribute(Of SettingsAttribute)())
        Return New SettingInfoCollection(plugin, infos)
    End Function

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of SettingInfo) Implements System.Collections.Generic.IEnumerable(Of SettingInfo).GetEnumerator
        Return m_Settings.GetEnumerator()
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class