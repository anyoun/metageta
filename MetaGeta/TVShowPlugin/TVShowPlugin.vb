Imports System.Text

<Serializable()> _
Public Class EducatedGuessImporter
    Implements IMGTaggingPlugin

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Public Sub New()
    End Sub

#Region "From http://www.merriampark.com/ld.htm"

    Private Function Minimum(ByVal a As Integer, ByVal b As Integer, ByVal c As Integer) As Integer
        Dim mi As Integer

        mi = a
        If b < mi Then
            mi = b
        End If
        If c < mi Then
            mi = c
        End If

        Minimum = mi

    End Function

    '********************************
    '*** Compute Levenshtein Distance
    '********************************

    Public Function LD(ByVal s As String, ByVal t As String) As Integer
        Dim m As Integer ' length of t
        Dim n As Integer ' length of s
        Dim i As Integer ' iterates through s
        Dim j As Integer ' iterates through t
        Dim s_i As String ' ith character of s
        Dim t_j As String ' jth character of t
        Dim cost As Integer ' cost

        ' Step 1

        n = Len(s)
        m = Len(t)
        If n = 0 Then
            LD = m
            Exit Function
        End If
        If m = 0 Then
            LD = n
            Exit Function
        End If
        'ReDim d(0 To n, 0 To m) As Integer
        Dim d(n, m) As Integer ' matrix

        ' Step 2

        For i = 0 To n
            d(i, 0) = i
        Next i

        For j = 0 To m
            d(0, j) = j
        Next j

        ' Step 3

        For i = 1 To n

            s_i = Mid$(s, i, 1)

            ' Step 4

            For j = 1 To m

                t_j = Mid$(t, j, 1)

                ' Step 5

                If s_i = t_j Then
                    cost = 0
                Else
                    cost = 1
                End If

                ' Step 6

                d(i, j) = Minimum(d(i - 1, j) + 1, d(i, j - 1) + 1, d(i - 1, j - 1) + cost)

            Next j

        Next i

        ' Step 7

        LD = d(n, m)
        Erase d

    End Function

#End Region

    Private m_DataStore As MGDataStore

    Private Shared ReadOnly c_AliasFrom As String() = {"The Venture Brothers"}
    Private Shared ReadOnly c_AliasTo As String() = {"The Venture Bros."}

    Public Const myniceName As String = "Educated Guess TV Show importer"
    'Public intMaxLDForMatch As Integer = 3
    Public chPhraseDelimiters() As Char = {"-"c}
    'Public myiHost As IMGHost
    'Public ds As New DSEducateGuessImporter

    Public Sub Import(ByVal File As System.Uri, ByVal Silent As Boolean)
        'Debug.WriteLine("Educated Guess " & filename)
        Dim s As String
        Dim gotSeriesTitle As Boolean = False
        Dim longestBracketedPhrase As Integer

        Dim tags As New Generic.Dictionary(Of String, MGTag)

        Dim fileName As String = System.IO.Path.GetFileNameWithoutExtension(File.ToString)
        Dim fileSize As Long = (New IO.FileInfo(fileName)).Length


        'new algorithm: break up into 2 types of "phrases":
        'numbers & words and bracketed junk (encoder, CRC32)
        'a const array will hold the phrase delimiters
        'then work from there
        'recognize "ep", "episode", etc

        'turn all underscores and periods into spaces
        fileName = fileName.Replace("_", " ")
        fileName = fileName.Replace(".", " ")

        Dim brcktPhrases As New Generic.List(Of String)
        brcktPhrases = getBracketedPhrases(fileName)

        'idetify CRC32
        Dim IsHex As Boolean = True
        For Each s In brcktPhrases
            If isCRC32(s) Then
                sett(tags, TVShowDataStoreTemplate.CRC32, s)
                brcktPhrases.Remove(s)
                Exit For
            End If
        Next

        'idetify encoder:
        longestBracketedPhrase = getLongest(brcktPhrases)
        If Not longestBracketedPhrase = -1 Then
            brcktPhrases(longestBracketedPhrase) = brcktPhrases(longestBracketedPhrase).Trim(" "c)
            'info.encoder = brcktPhrases(longestStart)
            'Debug.WriteLine("Found Encoder: " & brcktPhrases(longestBracketedPhrase))
            sett(tags, TVShowDataStoreTemplate.Group, brcktPhrases(longestBracketedPhrase))
        End If

        'now, find aliases before breaking apart into phrases
        For i As Integer = 0 To c_AliasFrom.Count
            If c_AliasFrom(i) <> "" AndAlso c_AliasFrom(i) <> " " AndAlso fileName.IndexOf(c_AliasFrom(i)) <> -1 Then 'regular aliasing
                fileName = fileName.Remove(fileName.IndexOf(c_AliasFrom(i)), c_AliasFrom(i).Length)
                sett(tags, "SeriesTitle", c_AliasTo(i))
                gotSeriesTitle = True
            ElseIf fileName.IndexOf(c_AliasTo(i)) <> -1 Then 'help for shows like 24
                fileName = fileName.Remove(fileName.IndexOf(c_AliasTo(i)), c_AliasTo(i).Length)
                sett(tags, "SeriesTitle", c_AliasTo(i))
                gotSeriesTitle = True
            End If
        Next

        'Debug.WriteLine("Ready to continue with EducatedGuess using " & fileName)
        'bracketed junk is now all out of the way and replaced with "-"
        'now to build a similar array of the numbers

        Dim phrases As Generic.List(Of Phrase) = getNumberAndWordPhrases(fileName)
        'now do stuff
        'Debug.WriteLine("hi")
        tags = processPhrases(phrases, tags, gotSeriesTitle)

        'now autocorrect
        'If tags.ContainsKey("SeriesTitle") Then
        '    Dim title As String = tags("SeriesTitle").Value
        '    Dim t, d As String
        '    Dim dist As Double
        '    For Each ar In ds.aliases()
        '        t = title.ToLower.Trim(" ")
        '        d = ar.Dest.ToLower.Trim(" ")
        '        If t.Length <> 0 AndAlso d.Length <> 0 Then
        '            dist = LD(t, d) / (t.Length + d.Length)
        '            If dist < ds.AutocorrectDistance(0).distance Then
        '                sett(tags, "SeriesTitle", ar.Dest)
        '            End If
        '        End If
        '    Next
        'End If

        'Where should this episode go?
        'Dim EpisodeItem As MGItem

        ''Get item

        'If EpisodeItem Is Nothing Then
        '    'Create a new episode item
        '    EpisodeItem = myiHost.CreateNewItem(Me)
        '    'Copy all the tags over
        '    For Each tag As MGMetaText In tags.Values
        '        EpisodeItem.Tags.Add(tag.Name, tag)
        '    Next

        '    'Add this datafile
        '    'If there is part info, should be "part x"
        '    EpisodeItem.Files.Add("Video", New MGDataFile(File, EpisodeItem.ID, fileSize))
        '    myiHost.ChangeItem(Me, EpisodeItem)
        'Else
        '    'Already exists - multipart episode?
        '    'Add this datafile and any new info and move on
        '    'Should handle multiple parts here

        '    'Create a new item, etc

        '    For Each tag As MGMetaText In tags.Values
        '        If Not EpisodeItem.Tags.ContainsKey(tag.Name) Then
        '            EpisodeItem.Tags.Add(tag.Name, tag)
        '        End If
        '    Next
        '    myiHost.ChangeItem(Me, EpisodeItem)
        'End If


    End Sub

    Private Function processPhrases(ByRef phrases As Generic.List(Of Phrase), ByRef tags As Generic.Dictionary(Of String, MGTag), ByRef foundSeriesTitle As Boolean) As Generic.Dictionary(Of String, MGTag)
        'Capitalization no longer matters for magic words
        Dim episodeWords() As String = {"e", "ep", "episode"}
        Dim justFoundEpWord As Boolean = False
        Dim foundEp As Boolean = False

        Dim seasonWords() As String = {"s", "season"}
        Dim justFoundSeWord As Boolean = False
        Dim foundSe As Boolean = False

        Dim partWords() As String = {"p", "pt", "part"}
        Dim justFoundPaWord As Boolean = False
        Dim foundPa As Boolean = False

        Dim s As String
        Dim countOfNumbers, countOfWords As Integer
        countOfNumbers = 0
        countOfWords = 0
        Dim gettingSeTi As Boolean = False
        Dim gettingEpTi As Boolean = False
        'Dim foundSeriesTitle As Boolean = False

        'For x = 0 To ph.Count - 1
        For Each p As Phrase In phrases
            If TypeOf p Is NumberPhrase Then
                Dim np As NumberPhrase = CType(p, NumberPhrase)
                If justFoundEpWord Then
                    sett(tags, TVShowDataStoreTemplate.EpisodeNumber, np.Value.ToString())
                    foundEp = True
                ElseIf justFoundSeWord Then
                    sett(tags, TVShowDataStoreTemplate.SeasonNumber, np.Value.ToString())
                    foundSe = True
                ElseIf justFoundPaWord Then
                    sett(tags, TVShowDataStoreTemplate.PartNumber, np.Value.ToString())
                    foundPa = True
                End If

                countOfNumbers += 1
                gettingSeTi = False
                gettingEpTi = False

                justFoundEpWord = False
                justFoundSeWord = False
                justFoundPaWord = False
                If Not foundPa And countOfNumbers > 2 Then
                    'tags.setTag("SeasonNumber", tags.getTag("EpisodeNumber"), False)
                    'tags.setTag("EpisodeNumber", ph(x), False)
                    sett(tags, TVShowDataStoreTemplate.PartNumber, np.Value.ToString())
                ElseIf Not foundSe And Not foundPa And countOfNumbers > 1 Then
                    sett(tags, TVShowDataStoreTemplate.SeasonNumber, tags(TVShowDataStoreTemplate.EpisodeNumber).Value)
                    sett(tags, TVShowDataStoreTemplate.EpisodeNumber, np.Value.ToString())
                ElseIf Not foundEp And countOfNumbers = 1 Then
                    If np.Value > 100 Then
                        'probably be season then ep number concatted
                        sett(tags, TVShowDataStoreTemplate.EpisodeNumber, (np.Value Mod 100).ToString)
                        sett(tags, TVShowDataStoreTemplate.SeasonNumber, Int(np.Value / 100).ToString)
                    Else
                        sett(tags, TVShowDataStoreTemplate.EpisodeNumber, np.Value.ToString())
                    End If

                End If

            ElseIf TypeOf p Is LetterPhrase Then
                'Debug.Write(ph(x) & "!")
                Dim lp As LetterPhrase = CType(p, LetterPhrase)

                justFoundEpWord = False
                justFoundSeWord = False
                justFoundPaWord = False

                For Each s In episodeWords
                    If lp.Value.ToLower = s.ToLower Then
                        justFoundEpWord = True
                        gettingSeTi = False
                        gettingEpTi = False
                    End If
                Next
                For Each s In seasonWords
                    If lp.Value.ToLower = s.ToLower Then
                        justFoundSeWord = True
                        gettingSeTi = False
                        gettingEpTi = False
                    End If
                Next
                For Each s In partWords
                    If lp.Value.ToLower = s.ToLower Then
                        justFoundPaWord = True
                        gettingSeTi = False
                        gettingEpTi = False
                    End If
                Next

                If gettingSeTi Then
                    sett(tags, TVShowDataStoreTemplate.SeriesTitle, tags(TVShowDataStoreTemplate.SeriesTitle).Value & " " & lp.Value)
                ElseIf gettingEpTi Then
                    sett(tags, TVShowDataStoreTemplate.EpisodeTitle, tags(TVShowDataStoreTemplate.EpisodeTitle).Value & " " & lp.Value)
                ElseIf countOfWords = 0 And Not justFoundSeWord And Not justFoundEpWord Then
                    'Debug.WriteLine("SeriesTitle=" & ph(x))
                    sett(tags, TVShowDataStoreTemplate.SeriesTitle, lp.Value)
                    foundSeriesTitle = True
                    gettingSeTi = True
                ElseIf countOfWords > 0 And Not justFoundSeWord And Not justFoundEpWord Then
                    'Debug.WriteLine("EpisodeTitle=" & ph(x))
                    sett(tags, TVShowDataStoreTemplate.EpisodeTitle, lp.Value)
                    gettingEpTi = True
                End If

                countOfWords += 1
            Else 'must be some other type - which means a break (bool to mark end of phrases)
                'Debug.Write("*")
                gettingSeTi = False
                gettingEpTi = False
            End If

        Next 'stepping through ph with x
        'Debug.Write(ControlChars.NewLine)

        If Not tags.ContainsKey(TVShowDataStoreTemplate.EpisodeTitle) _
            And tags.ContainsKey(TVShowDataStoreTemplate.EpisodeNumber) Then
            sett(tags, TVShowDataStoreTemplate.EpisodeTitle, String.Format("Episode {0}", tags(TVShowDataStoreTemplate.EpisodeNumber).Value))
        End If

        If Not foundSeriesTitle Then
            'Debug.WriteLine("Breaking...")
        End If

        Return tags

    End Function

    Private Function getNumberAndWordPhrases(ByVal input As String) As Generic.List(Of Phrase)
        Dim phrases As New Generic.List(Of Phrase)
        Dim currPhrase As Phrase = New NothingPhrase

        For Each ignoredString As String In c_IgnoredStrings
            input = input.Replace(ignoredString, String.Empty)
        Next

        For Each ch As Char In input

            If isItLetter(ch) Then
                If TypeOf currPhrase Is LetterPhrase Then
                    'continuing current letter phrase
                    'Add it to the end
                    CType(currPhrase, LetterPhrase).Contents.Add(ch)
                ElseIf TypeOf currPhrase Is NumberPhrase Then
                    'finished that number
                    phrases.Add(currPhrase)
                    'start new letter phrase
                    currPhrase = New LetterPhrase
                    CType(currPhrase, LetterPhrase).Contents.Add(ch)
                ElseIf TypeOf currPhrase Is NothingPhrase Then
                    'just starting a phrase
                    currPhrase = New LetterPhrase
                    CType(currPhrase, LetterPhrase).Contents.Add(ch)
                End If

            ElseIf isItNumber(ch) Then
                If TypeOf currPhrase Is LetterPhrase Then
                    'ending current letter phrase
                    phrases.Add(currPhrase)
                    'start new number phrase
                    currPhrase = New NumberPhrase
                    CType(currPhrase, NumberPhrase).Contents.Add(ch)
                ElseIf TypeOf currPhrase Is NumberPhrase Then
                    'continuing that number
                    CType(currPhrase, NumberPhrase).Contents.Add(ch)
                ElseIf TypeOf currPhrase Is NothingPhrase Then
                    'just starting a phrase
                    currPhrase = New NumberPhrase
                    CType(currPhrase, NumberPhrase).Contents.Add(ch)
                End If

            Else 'must be a phrase delimiter or just a space...
                If TypeOf currPhrase Is LetterPhrase Then
                    'ending current letter phrase
                    phrases.Add(currPhrase)
                    currPhrase = New NothingPhrase
                ElseIf TypeOf currPhrase Is NumberPhrase Then
                    'finished that number
                    phrases.Add(currPhrase)
                    currPhrase = New NothingPhrase
                ElseIf TypeOf currPhrase Is NothingPhrase Then
                    'just starting a phrase
                End If

                'If input.Chars(x) <> " " Then
                '    p.Add(New Boolean)
                'End If
            End If

        Next 'for through the entire string
        'Finish the last phrase
        phrases.Add(currPhrase)

        'Output the before and after to illustrate the spliting
        Dim sb As New StringBuilder()
        sb.Append(input)
        sb.Append(" -> ")
        For Each p As Phrase In phrases
            'Debug.Write(p(x) & "(" & p(x).GetType.FullName & ")")
            If TypeOf p Is NumberPhrase Then
                sb.Append(CType(p, NumberPhrase).Value & "#")
            End If
            If TypeOf p Is LetterPhrase Then
                sb.Append(CType(p, LetterPhrase).Value & "!")
            End If
            If TypeOf p Is NothingPhrase Then
                sb.Append("*")
            End If
        Next
        log.DebugFormat(sb.ToString())


        Return phrases

    End Function

    Private Function getBracketedPhrases(ByRef input As String) As Generic.List(Of String)
        '***will also remove stuff in brackets***
        'find anything in square brackets and put them into a temp array
        'then find the one with the most letters, and that's the encoder
        'also, remove those sections from strTemp so they'll be ignored later
        Dim x As Integer = 2
        Dim intBrackets(20) As Integer
        Dim y As Integer
        Dim z As Integer
        Dim tChars(50) As Char
        Dim brcktPhrases As New Generic.List(Of String)
        Dim s As String

        For y = 0 To 19
            intBrackets(y) = -1
        Next

        Do
            'Debug.WriteLine(CStr(x))
            intBrackets(x) = input.IndexOf("[", intBrackets(x - 2) + 1)
            If intBrackets(x) < 0 Then
                intBrackets(x) = 0
                Exit Do
            Else
                intBrackets(x + 1) = input.IndexOf("]", intBrackets(x))
                If intBrackets(x + 1) = -1 Then
                    intBrackets(x) = 0
                    intBrackets(x + 1) = 0
                    Exit Do
                Else
                    intBrackets(x + 1) = intBrackets(x + 1) - intBrackets(x) - 1
                    x = x + 2
                End If
            End If
        Loop

        'put things btw brackets into the array of strings
        If Not x = 2 Then
            For y = 2 To x - 2 Step 2
                For z = 0 To 50
                    tChars(z) = " "c
                Next
                For z = intBrackets(y) + 1 To intBrackets(y + 1) + intBrackets(y)
                    tChars(z - intBrackets(y) - 1) = input.Chars(z)
                Next
                'MsgBox(strTemp & Chr(13) & tChars)
                'tChars(49) = tChars(0)
                'brcktPhrases(y / 2 - 1) = tChars
                s = New String(tChars)
                s = s.TrimStart(" "c)
                s = s.TrimEnd(" "c)
                brcktPhrases.Add(s)

            Next
            For y = x - 2 To 2 Step -2
                input = input.Remove(intBrackets(y), intBrackets(y + 1) + 2)
                input = input.Insert(intBrackets(y), "-")
            Next
            'info.encoder = tChars


            Return brcktPhrases
        End If

        Return New Generic.List(Of String)
    End Function

    Private Function getLongest(ByRef myArray As Generic.List(Of String)) As Integer
        Dim longestStart As Integer = -1
        Dim longestLen As Integer = 0
        Dim y As Integer

        For y = 0 To myArray.Count - 1

            If myArray(y).Length > longestLen Then
                longestLen = myArray(y).Length
                longestStart = y
            End If
        Next

        Return longestStart
    End Function

    Private Function isItHex(ByVal theChar As Char) As Boolean
        Select Case theChar
            Case "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c, "0"c, "A"c, "a"c, "B"c, "b"c, "C"c, "c"c, "D"c, "d"c, "e"c, "E"c, "F"c, "f"c
                Return True
            Case Else
                Return False
        End Select
    End Function
    Private Function isCRC32(ByRef phrase As String) As Boolean
        If phrase.Length <> 8 Then
            Return False
        End If
        Dim i As Integer
        For i = 0 To phrase.Length - 1
            If Not isItHex(phrase.Chars(i)) Then
                Return False
            End If
        Next
        'Debug.WriteLine("Found CRC32: " & phrase)
        Return True
    End Function
    Private Function isItNumber(ByVal thechar As Char) As Boolean
        If Char.IsDigit(thechar) Then
            Return True
        End If
        Return False
    End Function
    Private Function isItLetter(ByVal thechar As Char) As Boolean
        If Char.IsLetter(thechar) Or thechar = "'" Then
            Return True
        End If
        Return False
    End Function

    Private Function readANumber(ByVal charray() As Char) As Integer
        Dim y As Integer
        Dim x As Integer
        Dim intTemp As Integer
        Dim intArrayTemp(50) As Integer
        Dim index As Integer
        index = 0
        y = 0
        intTemp = 0

        If charray.Length < 1 Then
            Return 0
        End If
        'read the number
        While Char.IsDigit(charray(index))
            intArrayTemp(y) = Val(charray(index))
            'Debug.Write(CStr(intArrayTemp(y)))
            index = index + 1
            y = y + 1
            If y >= charray.Length Then
                Exit While
            End If
        End While

        'turn the individual numbers into a single number
        For x = 1 To y
            intTemp = CInt(intTemp + (intArrayTemp(y - x) * 10 ^ (x - 1)))
        Next

        Return intTemp

    End Function


    'Public Sub initialize(ByRef host As MetaGetaInterfaces.IMGHost, ByRef strmPrefs As System.IO.StreamReader) Implements MetaGetaInterfaces.IMGImporter.initialize
    '    myiHost = host
    '    If strmPrefs.Peek <> -1 Then
    '        ds.ReadXml(strmPrefs)
    '    End If
    '    Try
    '        ds.AutocorrectDistance(0).distance += 0
    '    Catch ex As Exception
    '        ds.AutocorrectDistance.AddAutocorrectDistanceRow(0.1)
    '    End Try
    'End Sub

    'Public Sub openPropertiesWindow() Implements MetaGetaInterfaces.IMGImporter.openPropertiesWindow
    '    Dim props As New EducatedGuessImporterProperties
    '    props.aliases = New ArrayList
    '    Dim al As CAlias
    '    Dim ar As DSEducateGuessImporter.aliasesRow
    '    For Each ar In ds.aliases
    '        al = New CAlias
    '        al.Dest = ar.Dest
    '        al.From = ar.From
    '        props.aliases.Add(al)
    '    Next
    '    props.udDistance.Value() = ds.AutocorrectDistance(0).distance

    '    If props.ShowDialog() = Windows.Forms.DialogResult.OK Then
    '        ds.AutocorrectDistance(0).distance = props.udDistance.Value
    '        ds.aliases.Clear()
    '        For Each al In props.aliases
    '            ds.aliases.AddaliasesRow(al.From, al.Dest)
    '        Next
    '    Else
    '        'nothing
    '    End If


    'End Sub

    Private Sub sett(ByRef tags As Generic.Dictionary(Of String, MGTag), ByRef name As String, ByRef value As String)
        If tags.ContainsKey(name) Then
            tags(name).Value = value
        Else
            tags.Add(name, New MGTag(name, value))
        End If
    End Sub

    Private Class Phrase
        'nothing here
    End Class
    Private Class LetterPhrase
        Inherits Phrase
        Private mContents As New Generic.List(Of Char)
        Public ReadOnly Property Value() As String
            Get
                Return New String(mContents.ToArray)
            End Get
        End Property
        Public ReadOnly Property Contents() As Generic.List(Of Char)
            Get
                Return mContents
            End Get
        End Property
    End Class
    Private Class NumberPhrase
        Inherits Phrase
        Private mContents As New Generic.List(Of Char)
        Public ReadOnly Property Value() As Integer
            Get
                Dim x As Integer
                If Integer.TryParse(New String(mContents.ToArray), x) Then
                    Return x
                Else
                    Return 0
                End If
            End Get
        End Property
        Public ReadOnly Property Contents() As Generic.List(Of Char)
            Get
                Return mContents
            End Get
        End Property
    End Class
    Private Class NothingPhrase
        Inherits Phrase
        'no contents
    End Class

    Public Sub Startup(ByVal dataStore As MGDataStore) Implements IMGTaggingPlugin.Startup
        m_DataStore = dataStore

    End Sub

    Public Sub Process() Implements IMGTaggingPlugin.Process

        For Each file As MGFile In m_DataStore
            Dim s As String
            Dim gotSeriesTitle As Boolean = False
            Dim longestBracketedPhrase As Integer

            Dim tags As New Generic.Dictionary(Of String, MGTag)

            Dim fullPath As String = file.Path.LocalPath
            Dim fileName As String = System.IO.Path.GetFileNameWithoutExtension(fullPath)
            Dim fileSize As Long = 20 '(New IO.FileInfo(fullPath)).Length


            'new algorithm: break up into 2 types of "phrases":
            'numbers & words and bracketed junk (encoder, CRC32)
            'a const array will hold the phrase delimiters
            'then work from there
            'recognize "ep", "episode", etc

            'turn all underscores and periods into spaces
            fileName = fileName.Replace("_", " ")
            fileName = fileName.Replace(".", " ")

            Dim brcktPhrases As New Generic.List(Of String)
            brcktPhrases = getBracketedPhrases(fileName)

            'idetify CRC32
            Dim IsHex As Boolean = True
            For Each s In brcktPhrases
                If isCRC32(s) Then
                    sett(tags, TVShowDataStoreTemplate.CRC32, s)
                    brcktPhrases.Remove(s)
                    Exit For
                End If
            Next

            'idetify encoder:
            longestBracketedPhrase = getLongest(brcktPhrases)
            If Not longestBracketedPhrase = -1 Then
                brcktPhrases(longestBracketedPhrase) = brcktPhrases(longestBracketedPhrase).Trim(" "c)
                'info.encoder = brcktPhrases(longestStart)
                'Debug.WriteLine("Found Encoder: " & brcktPhrases(longestBracketedPhrase))
                sett(tags, TVShowDataStoreTemplate.Group, brcktPhrases(longestBracketedPhrase))
            End If

            'now, find aliases before breaking apart into phrases
            For i As Integer = 0 To c_AliasFrom.Count - 1
                If c_AliasFrom(i) <> "" AndAlso c_AliasFrom(i) <> " " AndAlso fileName.IndexOf(c_AliasFrom(i)) <> -1 Then 'regular aliasing
                    fileName = fileName.Remove(fileName.IndexOf(c_AliasFrom(i)), c_AliasFrom(i).Length)
                    sett(tags, "SeriesTitle", c_AliasTo(i))
                    gotSeriesTitle = True
                ElseIf fileName.IndexOf(c_AliasTo(i)) <> -1 Then 'help for shows like 24
                    fileName = fileName.Remove(fileName.IndexOf(c_AliasTo(i)), c_AliasTo(i).Length)
                    sett(tags, "SeriesTitle", c_AliasTo(i))
                    gotSeriesTitle = True
                End If
            Next

            'bracketed junk is now all out of the way and replaced with "-"
            'now to build a similar array of the numbers

            Dim phrases As Generic.List(Of Phrase) = getNumberAndWordPhrases(fileName)
            'now do stuff
            tags = processPhrases(phrases, tags, gotSeriesTitle)


            For Each tag As MGTag In tags.Values
                file.Tags.SetTag(tag)
            Next
        Next
    End Sub

    Public Sub Shutdown() Implements IMGTaggingPlugin.Shutdown

    End Sub

#Region "Constants"
    Private Shared ReadOnly c_IgnoredStrings As String() = {"divx", "x264", "h264", "264", "1280x720", "720p", "1080p"}
#End Region
End Class
