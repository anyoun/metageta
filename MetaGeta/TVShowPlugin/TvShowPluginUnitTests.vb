Imports NUnit.Framework
Imports NUnit.Framework.SyntaxHelpers

<TestFixture()> _
Public Class TvShowPluginUnitTests
    Inherits AssertionHelper

    <Test()> Public Sub BasicFileNameParsing()
        Expect("Series Name - 1", seriesTitle:="Series Name", seasonNumber:="1", episodeNumber:="1")
        Expect("Series Name 1", seriesTitle:="Series Name", seasonNumber:="1", episodeNumber:="1")
        Expect("Series Name 2x1", seriesTitle:="Series Name", seasonNumber:="2", episodeNumber:="1")
        Expect("Series Name s2e1", seriesTitle:="Series Name", seasonNumber:="2", episodeNumber:="1")
        Expect("Series Name 201", seriesTitle:="Series Name", seasonNumber:="2", episodeNumber:="1")
        Expect("Series Name 1 v3", seriesTitle:="Series Name", seasonNumber:="1", episodeNumber:="1")
        Expect("Series.Name.4.3", seriesTitle:="Series Name", seasonNumber:="4", episodeNumber:="3")
        Expect("Series.Name.5.37.This.Is.The.Title", seriesTitle:="Series Name", seasonNumber:="5", episodeNumber:="37", episodeTitle:="This Is The Title")
    End Sub

    Private Overloads Sub Expect(ByVal fileName As String, _
                                        Optional ByVal seriesTitle As String = Nothing, _
                                        Optional ByVal seasonNumber As String = Nothing, _
                                        Optional ByVal episodeNumber As String = Nothing, _
                                        Optional ByVal episodeTitle As String = Nothing)

        Dim f As New MGFile()
        f.SetTag(MGFile.FileNameKey, New Uri("C:\" + fileName + ".avi").AbsoluteUri)
        Dim edu As New EducatedGuessImporter()
        edu.Process(f, New ProgressStatus())
        If seriesTitle IsNot Nothing Then Expect(f.GetTag(TVShowDataStoreTemplate.SeriesTitle), Iz.EqualTo(seriesTitle))
        If seasonNumber IsNot Nothing Then Expect(f.GetTag(TVShowDataStoreTemplate.SeasonNumber), Iz.EqualTo(seasonNumber))
        If episodeNumber IsNot Nothing Then Expect(f.GetTag(TVShowDataStoreTemplate.EpisodeNumber), Iz.EqualTo(episodeNumber))
        If episodeTitle IsNot Nothing Then Expect(f.GetTag(TVShowDataStoreTemplate.EpisodeTitle), Iz.EqualTo(episodeTitle))
    End Sub
End Class
