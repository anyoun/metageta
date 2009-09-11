// Copyright 2009 Will Thomas
// 
// This file is part of MetaGeta.
// 
// MetaGeta is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MetaGeta is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MetaGeta. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using MetaGeta.DataStore;

#endregion

namespace MetaGeta.TVShowPlugin
{
    [Serializable]
    public class EducatedGuessImporter : IMGTaggingPlugin, IMGPluginBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private MGDataStore m_DataStore;

        private long m_ID;

        public long ID
        {
            get { return m_ID; }
        }

        #region IMGPluginBase Members

        long IMGPluginBase.PluginID
        {
            get { return ID; }
        }


        public void Startup(MGDataStore dataStore, long id)
        {
            m_DataStore = dataStore;
        }


        public void Shutdown()
        {
        }

        public string FriendlyName
        {
            get { return "Educated Guess TV Show Importer"; }
        }

        public string UniqueName
        {
            get { return "EducatedGuessTVShowImporter"; }
        }

        public Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        public event PropertyChangedEventHandler SettingChanged;

        #endregion

        #region "From http://www.merriampark.com/ld.htm"

        private int Minimum(int a, int b, int c)
        {
            int mi = 0;

            mi = a;
            if (b < mi)
            {
                mi = b;
            }
            if (c < mi)
            {
                mi = c;
            }

            return mi;
        }

        //********************************
        //*** Compute Levenshtein Distance
        //********************************

        public int LD(string s, string t)
        {
            int m = 0;
            // length of t
            int n = 0;
            // length of s
            int i = 0;
            // iterates through s
            int j = 0;
            // iterates through t
            string s_i = null;
            // ith character of s
            string t_j = null;
            // jth character of t
            int cost = 0;
            // cost

            // Step 1
            n = s.Length;
            m = t.Length;
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            //ReDim d(0 To n, 0 To m) As Integer
            var d = new int[n + 1,m + 1];
            // matrix
            // Step 2
            for (i = 0; i <= n; i++)
            {
                d[i, 0] = i;
            }
            for (j = 0; j <= m; j++)
            {
                d[0, j] = j;
            }
            // Step 3
            for (i = 1; i <= n; i++)
            {
                s_i = s.Substring(i, 1);
                // Step 4
                for (j = 1; j <= m; j++)
                {
                    t_j = t.Substring(j, 1);
                    // Step 5
                    if (s_i == t_j)
                    {
                        cost = 0;
                    }
                    else
                    {
                        cost = 1;
                    }
                    // Step 6
                    d[i, j] = Minimum(d[i - 1, j] + 1, d[i, j - 1] + 1, d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        #endregion

        #region IMGTaggingPlugin Members

        public void Process(MGFile file, ProgressStatus reporter)
        {
            string s = null;
            bool gotSeriesTitle = false;
            int longestBracketedPhrase = 0;

            var tags = new Dictionary<string, string>();

            string fullPath = file.FileName;
            string fileName = Path.GetFileNameWithoutExtension(fullPath);

            //new algorithm: break up into 2 types of "phrases":
            //numbers & words and bracketed junk (encoder, CRC32)
            //a const array will hold the phrase delimiters
            //then work from there
            //recognize "ep", "episode", etc

            //turn all underscores, periods, etc into spaces
            foreach (string s_loopVariable in c_StringsToConvertToSpaces)
            {
                s = s_loopVariable;
                fileName = fileName.Replace(s, " ");
            }

            var brcktPhrases = new List<string>();
            brcktPhrases = getBracketedPhrases(ref fileName);

            //idetify CRC32
            bool IsHex = true;
            foreach (string s_loopVariable in brcktPhrases)
            {
                s = s_loopVariable;
                if (isCRC32(ref s))
                {
                    tags[TVShowDataStoreTemplate.CRC32] = s;
                    brcktPhrases.Remove(s);
                    break; // TODO: might not be correct. Was : Exit For
                }
            }

            //idetify encoder:
            longestBracketedPhrase = getLongest(ref brcktPhrases);
            if (!(longestBracketedPhrase == -1))
            {
                brcktPhrases[longestBracketedPhrase] = brcktPhrases[longestBracketedPhrase].Trim(' ');
                //info.encoder = brcktPhrases(longestStart)
                //Debug.WriteLine("Found Encoder: " & brcktPhrases(longestBracketedPhrase))
                tags[TVShowDataStoreTemplate.Group] = brcktPhrases[longestBracketedPhrase];
            }

            //now, find aliases before breaking apart into phrases
            for (int i = 0; i <= c_AliasFrom.Length - 1; i++)
            {
                //regular aliasing
                if (!string.IsNullOrEmpty(c_AliasFrom[i]) && c_AliasFrom[i] != " " && fileName.IndexOf(c_AliasFrom[i]) != -1)
                {
                    fileName = fileName.Remove(fileName.IndexOf(c_AliasFrom[i]), c_AliasFrom[i].Length);
                    tags["SeriesTitle"] = c_AliasTo[i];
                    gotSeriesTitle = true;
                    //help for shows like 24
                }
                else if (fileName.IndexOf(c_AliasTo[i]) != -1)
                {
                    fileName = fileName.Remove(fileName.IndexOf(c_AliasTo[i]), c_AliasTo[i].Length);
                    tags["SeriesTitle"] = c_AliasTo[i];
                    gotSeriesTitle = true;
                }
            }

            //bracketed junk is now all out of the way and replaced with "-"
            //now to build a similar array of the numbers

            List<Phrase> phrases = getNumberAndWordPhrases(fileName);
            //now do stuff
            processPhrases(phrases, tags, gotSeriesTitle);


            foreach (KeyValuePair<string, string> tag in tags)
            {
                file.SetTag(tag.Key, tag.Value);
            }
        }

        #endregion

        #region "Constants"

        private static readonly string[] c_AliasFrom = {
                                                           "The Venture Brothers",
                                                           "Battlestar Galactica"
                                                       };

        private static readonly string[] c_AliasTo = {
                                                         "The Venture Bros.",
                                                         "Battlestar Galactica (2003)"
                                                     };

        private static readonly string[] c_IgnoredStrings = {
                                                                "divx",
                                                                "x264",
                                                                "X264",
                                                                "h264",
                                                                "H264",
                                                                "264",
                                                                "hdtv",
                                                                "HDTV",
                                                                "1280x720",
                                                                "720p",
                                                                "1080p"
                                                            };

        private static readonly string[] c_StringsToConvertToSpaces = {
                                                                          ".",
                                                                          "_",
                                                                          "%20"
                                                                      };

        //Public intMaxLDForMatch As Integer = 3

        #endregion

        private void processPhrases(List<Phrase> phrases, Dictionary<string, string> tags, bool foundSeriesTitle)
        {
            //Capitalization no longer matters for magic words
            string[] episodeWords = {
                                        "e",
                                        "ep",
                                        "episode"
                                    };
            bool justFoundEpWord = false;
            bool foundEp = false;

            string[] seasonWords = {
                                       "s",
                                       "season"
                                   };
            bool justFoundSeWord = false;
            bool foundSe = false;

            string[] partWords = {
                                     "p",
                                     "pt",
                                     "part"
                                 };
            bool justFoundPaWord = false;
            bool foundPa = false;

            string[] versionWords = {
                                        "v",
                                        "ver",
                                        "version"
                                    };
            bool justFoundVerWord = false;
            bool foundVer = false;

            string s = null;
            int countOfNumbers = 0;
            int countOfWords = 0;
            countOfNumbers = 0;
            countOfWords = 0;
            bool gettingSeTi = false;
            bool gettingEpTi = false;
            //Dim foundSeriesTitle As Boolean = False

            //For x = 0 To ph.Count - 1
            foreach (Phrase p in phrases)
            {
                if (p is NumberPhrase)
                {
                    var np = (NumberPhrase) p;
                    if (justFoundEpWord)
                    {
                        tags[TVShowDataStoreTemplate.EpisodeNumber] = np.Value.ToString();
                        foundEp = true;
                    }
                    else if (justFoundSeWord)
                    {
                        tags[TVShowDataStoreTemplate.SeasonNumber] = np.Value.ToString();
                        foundSe = true;
                    }
                    else if (justFoundPaWord)
                    {
                        tags[TVShowDataStoreTemplate.PartNumber] = np.Value.ToString();
                        foundPa = true;
                    }
                    else if (justFoundVerWord)
                    {
                        //Ignore version for now
                        foundVer = true;
                        countOfNumbers -= 1;
                    }

                    countOfNumbers += 1;
                    gettingSeTi = false;
                    gettingEpTi = false;

                    justFoundEpWord = false;
                    justFoundSeWord = false;
                    justFoundPaWord = false;
                    justFoundVerWord = false;
                    if (!foundPa & countOfNumbers > 2)
                    {
                        tags[TVShowDataStoreTemplate.PartNumber] = np.Value.ToString();
                        foundPa = true;
                    }
                    else if (!foundSe & !foundPa & countOfNumbers > 1)
                    {
                        tags[TVShowDataStoreTemplate.SeasonNumber] = tags[TVShowDataStoreTemplate.EpisodeNumber];
                        foundSe = true;
                        tags[TVShowDataStoreTemplate.EpisodeNumber] = np.Value.ToString();
                        foundEp = true;
                    }
                    else if (!foundEp & countOfNumbers == 1)
                    {
                        if (np.Value > 100)
                        {
                            //probably be season then ep number concatted
                            tags[TVShowDataStoreTemplate.EpisodeNumber] = (np.Value%100).ToString();
                            foundEp = true;
                            tags[TVShowDataStoreTemplate.SeasonNumber] = (np.Value/100).ToString();
                            foundSe = true;
                        }
                        else
                        {
                            tags[TVShowDataStoreTemplate.EpisodeNumber] = np.Value.ToString();
                            foundEp = true;
                        }
                    }
                }
                else if (p is LetterPhrase)
                {
                    //Debug.Write(ph(x) & "!")
                    var lp = (LetterPhrase) p;

                    justFoundEpWord = false;
                    justFoundSeWord = false;
                    justFoundPaWord = false;
                    justFoundVerWord = false;

                    foreach (string s_loopVariable in episodeWords)
                    {
                        s = s_loopVariable;
                        if (string.Equals(lp.Value, s, StringComparison.CurrentCultureIgnoreCase))
                        {
                            justFoundEpWord = true;
                            gettingSeTi = false;
                            gettingEpTi = false;
                        }
                    }
                    foreach (string s_loopVariable in seasonWords)
                    {
                        s = s_loopVariable;
                        if (string.Equals(lp.Value, s, StringComparison.CurrentCultureIgnoreCase))
                        {
                            justFoundSeWord = true;
                            gettingSeTi = false;
                            gettingEpTi = false;
                        }
                    }
                    foreach (string s_loopVariable in partWords)
                    {
                        s = s_loopVariable;
                        if (string.Equals(lp.Value, s, StringComparison.CurrentCultureIgnoreCase))
                        {
                            justFoundPaWord = true;
                            gettingSeTi = false;
                            gettingEpTi = false;
                        }
                    }
                    foreach (string s_loopVariable in versionWords)
                    {
                        s = s_loopVariable;
                        if (string.Equals(lp.Value, s, StringComparison.CurrentCultureIgnoreCase))
                        {
                            justFoundVerWord = true;
                            gettingSeTi = false;
                            gettingEpTi = false;
                        }
                    }

                    if (gettingSeTi)
                    {
                        tags[TVShowDataStoreTemplate.SeriesTitle] = tags[TVShowDataStoreTemplate.SeriesTitle] + " " + lp.Value;
                    }
                    else if (gettingEpTi)
                    {
                        tags[TVShowDataStoreTemplate.EpisodeTitle] = tags[TVShowDataStoreTemplate.EpisodeTitle] + " " + lp.Value;
                    }
                    else if (countOfWords == 0 & !justFoundSeWord & !justFoundEpWord & !justFoundPaWord & !justFoundVerWord)
                    {
                        //Debug.WriteLine("SeriesTitle=" & ph(x))
                        tags[TVShowDataStoreTemplate.SeriesTitle] = lp.Value;
                        foundSeriesTitle = true;
                        gettingSeTi = true;
                    }
                    else if (countOfWords > 0 & !justFoundSeWord & !justFoundEpWord & !justFoundPaWord & !justFoundVerWord)
                    {
                        //Debug.WriteLine("EpisodeTitle=" & ph(x))
                        tags[TVShowDataStoreTemplate.EpisodeTitle] = lp.Value;
                        gettingEpTi = true;
                    }

                    countOfWords += 1;
                    //must be some other type - which means a break (bool to mark end of phrases)
                }
                else
                {
                    //Debug.Write("*")
                    gettingSeTi = false;
                    gettingEpTi = false;
                }
            }
            //stepping through ph with x
            //Debug.Write(ControlChars.NewLine)

            if (foundEp & !foundSe)
            {
                //Default to season = 1
                tags[TVShowDataStoreTemplate.SeasonNumber] = 1.ToString();
            }

            if (foundEp & !tags.ContainsKey(TVShowDataStoreTemplate.EpisodeTitle))
            {
                tags[TVShowDataStoreTemplate.EpisodeTitle] = string.Format("Episode {0}", tags[TVShowDataStoreTemplate.EpisodeNumber]);
            }
        }


        private List<Phrase> getNumberAndWordPhrases(string input)
        {
            foreach (string ignoredString in c_IgnoredStrings)
            {
                input = input.Replace(ignoredString, string.Empty);
            }

            var phrases = new List<Phrase>();
            Phrase currPhrase = new NothingPhrase();

            foreach (char ch in input)
            {
                if (isItLetter(ch))
                {
                    if (currPhrase is LetterPhrase)
                    {
                        //continuing current letter phrase
                        //Add it to the end
                        ((LetterPhrase) currPhrase).Contents.Add(ch);
                    }
                    else if (currPhrase is NumberPhrase)
                    {
                        //finished that number
                        phrases.Add(currPhrase);
                        //start new letter phrase
                        currPhrase = new LetterPhrase();
                        ((LetterPhrase) currPhrase).Contents.Add(ch);
                    }
                    else if (currPhrase is NothingPhrase)
                    {
                        //just starting a phrase
                        currPhrase = new LetterPhrase();
                        ((LetterPhrase) currPhrase).Contents.Add(ch);
                    }
                }
                else if (isItNumber(ch))
                {
                    if (currPhrase is LetterPhrase)
                    {
                        //ending current letter phrase
                        phrases.Add(currPhrase);
                        //start new number phrase
                        currPhrase = new NumberPhrase();
                        ((NumberPhrase) currPhrase).Contents.Add(ch);
                    }
                    else if (currPhrase is NumberPhrase)
                    {
                        //continuing that number
                        ((NumberPhrase) currPhrase).Contents.Add(ch);
                    }
                    else if (currPhrase is NothingPhrase)
                    {
                        //just starting a phrase
                        currPhrase = new NumberPhrase();
                        ((NumberPhrase) currPhrase).Contents.Add(ch);
                    }

                    //must be a phrase delimiter or just a space...
                }
                else
                {
                    if (currPhrase is LetterPhrase)
                    {
                        //ending current letter phrase
                        phrases.Add(currPhrase);
                        currPhrase = new NothingPhrase();
                    }
                    else if (currPhrase is NumberPhrase)
                    {
                        //finished that number
                        phrases.Add(currPhrase);
                        currPhrase = new NothingPhrase();
                    }
                    else if (currPhrase is NothingPhrase)
                    {
                        //just starting a phrase
                    }

                    //If input.Chars(x) <> " " Then
                    //    p.Add(New Boolean)
                    //End If
                }
            }
            //for through the entire string
            //Finish the last phrase
            phrases.Add(currPhrase);

            //Output the before and after to illustrate the spliting
            var sb = new StringBuilder();
            sb.Append(input);
            sb.Append(" -> ");
            foreach (Phrase p in phrases)
            {
                if (p is NumberPhrase)
                {
                    sb.Append(((NumberPhrase) p).Value + "#");
                }
                if (p is LetterPhrase)
                {
                    sb.Append(((LetterPhrase) p).Value + "!");
                }
                if (p is NothingPhrase)
                {
                    sb.Append("*");
                }
            }
            log.DebugFormat(sb.ToString());

            return phrases;
        }

        private List<string> getBracketedPhrases(ref string input)
        {
            //***will also remove stuff in brackets***
            //find anything in square brackets and put them into a temp array
            //then find the one with the most letters, and that's the encoder
            //also, remove those sections from strTemp so they'll be ignored later
            int x = 2;
            var intBrackets = new int[21];
            int y = 0;
            int z = 0;
            var tChars = new char[51];
            var brcktPhrases = new List<string>();
            string s = null;

            for (y = 0; y <= 19; y++)
            {
                intBrackets[y] = -1;
            }

            do
            {
                //Debug.WriteLine(CStr(x))
                intBrackets[x] = input.IndexOf("[", intBrackets[x - 2] + 1);
                if (intBrackets[x] < 0)
                {
                    intBrackets[x] = 0;
                    break; // TODO: might not be correct. Was : Exit Do
                }
                else
                {
                    intBrackets[x + 1] = input.IndexOf("]", intBrackets[x]);
                    if (intBrackets[x + 1] == -1)
                    {
                        intBrackets[x] = 0;
                        intBrackets[x + 1] = 0;
                        break; // TODO: might not be correct. Was : Exit Do
                    }
                    else
                    {
                        intBrackets[x + 1] = intBrackets[x + 1] - intBrackets[x] - 1;
                        x = x + 2;
                    }
                }
            } while (true);

            //put things btw brackets into the array of strings
            if (!(x == 2))
            {
                for (y = 2; y <= x - 2; y += 2)
                {
                    for (z = 0; z <= 50; z++)
                    {
                        tChars[z] = ' ';
                    }
                    for (z = intBrackets[y] + 1; z <= intBrackets[y + 1] + intBrackets[y]; z++)
                    {
                        tChars[z - intBrackets[y] - 1] = input[z];
                    }
                    //MsgBox(strTemp & Chr(13) & tChars)
                    //tChars(49) = tChars(0)
                    //brcktPhrases(y / 2 - 1) = tChars
                    s = new string(tChars);
                    s = s.TrimStart(' ');
                    s = s.TrimEnd(' ');
                    brcktPhrases.Add(s);
                }
                for (y = x - 2; y >= 2; y += -2)
                {
                    input = input.Remove(intBrackets[y], intBrackets[y + 1] + 2);
                    input = input.Insert(intBrackets[y], "-");
                }
                //info.encoder = tChars


                return brcktPhrases;
            }

            return new List<string>();
        }

        private int getLongest(ref List<string> myArray)
        {
            int longestStart = -1;
            int longestLen = 0;
            int y = 0;

            for (y = 0; y <= myArray.Count - 1; y++)
            {
                if (myArray[y].Length > longestLen)
                {
                    longestLen = myArray[y].Length;
                    longestStart = y;
                }
            }

            return longestStart;
        }

        private bool isItHex(char theChar)
        {
            switch (theChar)
            {
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '0':
                case 'A':
                case 'a':
                case 'B':
                case 'b':
                case 'C':
                case 'c':
                case 'D':
                case 'd':
                case 'e':
                case 'E':
                case 'F':
                case 'f':
                    return true;
                default:
                    return false;
            }
        }

        private bool isCRC32(ref string phrase)
        {
            if (phrase.Length != 8)
            {
                return false;
            }
            int i = 0;
            for (i = 0; i <= phrase.Length - 1; i++)
            {
                if (!isItHex(phrase[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool isItNumber(char thechar)
        {
            if (char.IsDigit(thechar))
            {
                return true;
            }
            return false;
        }

        private bool isItLetter(char thechar)
        {
            if (char.IsLetter(thechar) | thechar == '\'')
            {
                return true;
            }
            return false;
        }

        #region "Phrase Types"

        #region Nested type: LetterPhrase

        private class LetterPhrase : Phrase
        {
            private readonly List<char> mContents = new List<char>();

            public string Value
            {
                get { return new string(mContents.ToArray()); }
            }

            public List<char> Contents
            {
                get { return mContents; }
            }
        }

        #endregion

        #region Nested type: NothingPhrase

        private class NothingPhrase : Phrase
        {
            //no contents
        }

        #endregion

        #region Nested type: NumberPhrase

        private class NumberPhrase : Phrase
        {
            private readonly List<char> mContents = new List<char>();

            public int Value
            {
                get
                {
                    int x = 0;
                    if (int.TryParse(new string(mContents.ToArray()), out x))
                    {
                        return x;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public List<char> Contents
            {
                get { return mContents; }
            }
        }

        #endregion

        #region Nested type: Phrase

        private class Phrase
        {
            //nothing here
        }

        #endregion

        #endregion
    }
}