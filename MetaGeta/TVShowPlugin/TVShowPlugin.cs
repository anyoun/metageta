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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using MetaGeta.DataStore;
using MetaGeta.Utilities;
using System.Collections.ObjectModel;

#endregion

namespace MetaGeta.TVShowPlugin {
    [Serializable]
    public class EducatedGuessImporter : IMGTaggingPlugin, IMGPluginBase {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private MGDataStore m_DataStore;

        private long m_ID;

        public long ID {
            get { return m_ID; }
        }

        #region IMGPluginBase Members

        long IMGPluginBase.PluginID {
            get { return ID; }
        }


        public void Startup(MGDataStore dataStore, long id) {
            m_DataStore = dataStore;
        }


        public void Shutdown() { }

        public string FriendlyName {
            get { return "Educated Guess TV Show Importer"; }
        }

        public string UniqueName {
            get { return "EducatedGuessTVShowImporter"; }
        }

        public Version Version {
            get { return new Version(1, 0, 0, 0); }
        }

        public event PropertyChangedEventHandler SettingChanged;

        #endregion

        #region IMGTaggingPlugin Members

        public void Process(MGFile file, ProgressStatus reporter) {
            bool gotSeriesTitle = false;

            //var tags = new Dictionary<string, object>();

            string fullPath = file.FileName;
            string fileName = Path.GetFileNameWithoutExtension(fullPath);

            //now, find aliases before breaking apart into phrases
            for (int i = 0; i <= c_AliasFrom.Length - 1; i++) {
                //regular aliasing
                int idxFrom = fileName.IndexOf(c_AliasFrom[i]);
                int idxTo = fileName.IndexOf(c_AliasTo[i]);

                if (!string.IsNullOrEmpty(c_AliasFrom[i]) && c_AliasFrom[i] != " " && idxFrom != -1) {
                    fileName = fileName.Remove(idxFrom, c_AliasFrom[i].Length);
                    fileName = fileName.Insert(idxFrom, c_SeperatorChar.ToString());
                    file.Tags.Set(TVShowDataStoreTemplate.SeriesTitle, c_AliasTo[i]);
                    gotSeriesTitle = true;
                    //help for shows like 24
                } else {
                    if (idxTo != -1) {
                        fileName = fileName.Remove(idxTo, c_AliasTo[i].Length);
                        fileName = fileName.Insert(idxFrom, c_SeperatorChar.ToString());
                        file.Tags.Set(TVShowDataStoreTemplate.SeriesTitle, c_AliasTo[i]);
                        gotSeriesTitle = true;
                    }
                }
            }

            //Replace ignored strings with "-"
            var expr = new Regex(c_IgnoredStrings.Select(str => "(" + Regex.Escape(str) + ")").JoinToString("|"), RegexOptions.IgnoreCase);
            fileName = expr.Replace(fileName, c_SeperatorChar.ToString());

            //turn all underscores, periods, etc into spaces
            foreach (string s in c_StringsToConvertToSpaces) fileName = fileName.Replace(s, " ");

            //Replace CRC32
            fileName = Regex.Replace(fileName, @"\[[0-9A-Fa-f]{8}\]", c_SeperatorChar.ToString());

            var tokens = Tokenize(fileName);
            ParseTokens(tokens, file.Tags, gotSeriesTitle);
        }

        #endregion

        #region Constants

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
                                                                "h264",
                                                                "264",
                                                                "hdtv",
                                                                "1280x720",
                                                                "aac",
                                                                "5.1",
                                                                "720p",
                                                                "1080p",
                                                                "oav",
                                                                "ws",
                                                                "fs",
                                                                "pdtv",
                                                                "proper",
                                                                "tv",
                                                                "repack",
                                                                "rerip",
                                                                "real",
                                                                "internal",
                                                                "dubbed",
                                                                "subbed",
                                                                "dvd",
                                                            };

        private static readonly string[] c_IgnoredMovieStrings = {
                                                                   "dvd5",
                                                                   "dvd9",
                                                                   "se",
                                                                   "dc",
                                                                   "extended",
                                                                   "uncut",
                                                                   "remastered",
                                                                   "unrated",
                                                                   "theatrical",
                                                                   "retail",
                                                                   "ts",
                                                                   "cam",
                                                                   "vhs",
                                                                   "screener",
                                                               };

        private static readonly string[] c_StringsToConvertToSpaces = {
                                                                          ".",
                                                                          "_",
                                                                          "%20"
                                                                      };

        private static readonly string[] c_EpisodeWords = {
                                                     "e",
                                                     "ep",
                                                     "episode"
                                                 };

        private static readonly string[] c_PartWords = {
                                                  "p",
                                                  "pt",
                                                  "part"
                                              };

        private static readonly string[] c_SeasonWords = {
                                                    "s",
                                                    "season"
                                                };

        private static readonly string[] c_VersionWords = {
                                                     "v",
                                                     "ver",
                                                     "version"
                                                 };

        private const char c_SeperatorChar = '|';

        #endregion

        private List<Token> Tokenize(string input) {
            var tokens = new List<Token>();
            Token currToken = null;

            foreach (char ch in input) {
                if (IsItLetter(ch)) {
                    currToken = PushAndNext(currToken, tokens, TokenType.Letter, false);
                } else if (IsItNumber(ch)) {
                    currToken = PushAndNext(currToken, tokens, TokenType.Number, false);
                } else if (IsItSeperator(ch)) {
                    currToken = PushAndNext(currToken, tokens, TokenType.Seperator, false);
                } else if (IsItOpenBracket(ch)) {
                    currToken = PushAndNext(currToken, tokens, TokenType.OpenBracket, true);
                } else if (IsItCloseBracket(ch)) {
                    currToken = PushAndNext(currToken, tokens, TokenType.CloseBracket, true);
                } else {
                    //Whitespace
                    currToken = PushAndNext(currToken, tokens, TokenType.WhiteSpace, false);
                }
                if (currToken != null)
                    currToken.Add(ch);
            }
            //for through the entire string
            //Finish the last phrase
            if (currToken != null)
                tokens.Add(currToken);

            //Output the before and after to illustrate the spliting
            var sb = new StringBuilder();
            sb.Append(input);
            sb.Append(" -> ");
            foreach (Token token in tokens) token.Print(sb);
            log.DebugFormat(sb.ToString());
            Console.WriteLine(sb);

            return tokens;
        }

        private static Token PushAndNext(Token currToken, List<Token> tokens, TokenType nextTokenType, bool isSingleCharToken) {
            if (currToken == null || currToken.TokenType != nextTokenType) {
                if (currToken != null)
                    tokens.Add(currToken);

                if (isSingleCharToken) {
                    tokens.Add(new Token(nextTokenType));
                    return null;
                } else {
                    return new Token(nextTokenType);
                }
            } else {
                return currToken;
            }
        }

        private void ParseTokens(List<Token> tokens, MGTagCollection tags, bool foundSeriesTitle) {
            //tokens = tokens.Where(t => t.TokenType != TokenType.WhiteSpace).ToList(); //Remove whitespace

            //Find CRC32
            var crcMatches = Pattern.Exact(Pattern.Single(TokenType.OpenBracket),
                //Pattern.Single(TokenType.Letter),
                                           Pattern.Single(new Regex("[0-9A-F]{8}")),
                                           Pattern.Single(TokenType.CloseBracket)).MatchAndRemove(tokens);
            if (crcMatches != null) {
                tags.Set(TVShowDataStoreTemplate.CRC32, crcMatches[1].GetContentsString());
            }

            //Match labels numbers
            int? episodeNum = MatchAndRemoveLabeledNumber(tokens, c_EpisodeWords);
            int? seasonNum = MatchAndRemoveLabeledNumber(tokens, c_SeasonWords);
            int? partNum = MatchAndRemoveLabeledNumber(tokens, c_PartWords);
            int? versionNum = MatchAndRemoveLabeledNumber(tokens, c_VersionWords);

            if (!episodeNum.HasValue && !seasonNum.HasValue) {
                //Try to match 01x03
                var matchedTokens = Pattern.Exact(Pattern.Single(TokenType.Number), Pattern.Any("x"), Pattern.Single(TokenType.Number)).MatchAndRemove(tokens);
                if (matchedTokens != null) {
                    seasonNum = int.Parse(matchedTokens[0].GetContentsString());
                    episodeNum = int.Parse(matchedTokens[2].GetContentsString());
                }
            }

            if (!episodeNum.HasValue) {
                var firstNumberToken = Pattern.Single(TokenType.Number).MatchAndRemove(tokens);
                var secondNumberToken = Pattern.Single(TokenType.Number).MatchAndRemove(tokens);
                //var numberTokens = tokens.Where(t => t.TokenType == TokenType.Number).ToArray();
                if (firstNumberToken != null && secondNumberToken == null) {
                    int num = int.Parse(firstNumberToken[0].GetContentsString());
                    if (num > 100) {
                        seasonNum = num / 100;
                        episodeNum = num % 100;
                    } else {
                        seasonNum = 1;
                        episodeNum = num;
                    }
                } else if (firstNumberToken != null && secondNumberToken != null) {
                    seasonNum = int.Parse(firstNumberToken[0].GetContentsString());
                    episodeNum = int.Parse(secondNumberToken[0].GetContentsString());
                } else {
                    //No numbers found at all
                }
            }

            if (episodeNum.HasValue && !seasonNum.HasValue)
                seasonNum = 1;

            if (seasonNum.HasValue) tags.Set(TVShowDataStoreTemplate.SeasonNumber, seasonNum.Value);
            if (episodeNum.HasValue) tags.Set(TVShowDataStoreTemplate.EpisodeNumber, episodeNum.Value);

            //Grab group
            var groupToken = Pattern.Exact(Pattern.Start(),
                                           Pattern.Single(TokenType.OpenBracket),
                                           Pattern.ZeroOrMore(Pattern.Not(Pattern.Single(TokenType.CloseBracket)))).MatchAndRemove(tokens);
            if (groupToken != null) {
                tags.Set(TVShowDataStoreTemplate.Group, groupToken.Select(t => t.ToString()).JoinToString(" "));
            }

            //The longest phrase must be series title
            bool includeNumbers = episodeNum.HasValue && seasonNum.HasValue;
            var phrases = TokensToPhrases(tokens, includeNumbers);
            //var longestPhrase = phrases.MaxItem(p => p.Tokens.Count);

            if (phrases.Count >= 1) tags.Set(TVShowDataStoreTemplate.SeriesTitle, phrases[0].Trim().GetContents());
            if (phrases.Count >= 2) tags.Set(TVShowDataStoreTemplate.EpisodeTitle, phrases[1].Trim().GetContents());
        }

        private static int? MatchAndRemoveLabeledNumber(List<Token> tokens, string[] labels) {
            var matched = Pattern.Exact(Pattern.Any(labels), Pattern.Single(TokenType.Number)).MatchAndRemove(tokens);
            if (matched == null)
                return null;
            return int.Parse(matched[1].GetContentsString());
        }

        private List<Phrase> TokensToPhrases(List<Token> tokens, bool includeNumbers) {
            var phrases = new List<Phrase>();
            var curr = new Phrase();

            for (int i = 0; i < tokens.Count; i++) {
                if (tokens[i].TokenType == TokenType.Letter
                    || tokens[i].TokenType == TokenType.WhiteSpace
                    //|| tokens[i].TokenType == TokenType.Seperator
                    || (includeNumbers && tokens[i].TokenType == TokenType.Number)) {
                    curr.Tokens.Add(tokens[i]);
                } else {
                    if (curr.Tokens.Count > 0) {
                        phrases.Add(curr);
                        curr = new Phrase();
                    }
                }
            }
            if (curr.Tokens.Count > 0)
                phrases.Add(curr);

            //Remove all phrases that are only white space
            phrases.RemoveAll(p => p.Tokens.All(t => t.TokenType == TokenType.WhiteSpace));
            return phrases;
        }


        #region Pattern matching

        private class Pattern : IPattern {
            private readonly List<IPattern> m_Elements = new List<IPattern>();

            private Pattern(IEnumerable<IPattern> elements) { m_Elements.AddRange(elements); }
            private Pattern(IPattern element) { m_Elements.Add(element); }

            public static Pattern Exact(params IPattern[] patterns) {
                return new Pattern(patterns);
            }

            public static Pattern Any(params TokenType[] types) {
                return new Pattern(new TokenTypePattern(types));
            }
            public static Pattern Any(params string[] exactTextList) {
                return new Pattern(new ExactTextPattern(exactTextList));
            }

            public static Pattern Single(TokenType type) {
                return new Pattern(new TokenTypePattern(type));
            }
            public static Pattern Single(string text) {
                return new Pattern(new ExactTextPattern(text));
            }
            public static Pattern Single(Regex expression) {
                return new Pattern(new ExactTextPattern(expression));
            }

            public static Pattern ZeroOrMore(IPattern condition) {
                return new Pattern(new StarPattern(condition));
            }
            public static Pattern Not(IPattern condition) {
                return new Pattern(new NotPattern(condition));
            }

            public static Pattern Start() {
                return new Pattern(new StartPattern());
            }

            public List<Token> MatchAndRemove(List<Token> tokens) {
                for (int i = 0; i < tokens.Count; i++) {
                    int matchingTokens;
                    if (IsMatch(tokens, i, out matchingTokens)) {
                        var range = tokens.GetRange(i, matchingTokens);
                        tokens.RemoveRange(i, matchingTokens);
                        tokens.Insert(i, new Token(TokenType.Seperator));
                        return range;
                    }
                }
                return null;
            }

            public bool IsMatch(IList<Token> tokens, int startIndex, out int matchingTokens) {
                matchingTokens = 0;
                for (int i = 0; i < m_Elements.Count; i++) {
                    if (matchingTokens + startIndex >= tokens.Count)
                        return false;
                    int matchCount;
                    if (!m_Elements[i].IsMatch(tokens, startIndex + matchingTokens, out matchCount))
                        return false;
                    matchingTokens += matchCount;
                }
                return true;
            }

            private class ExactTextPattern : IPattern {
                private readonly Regex m_Regex;

                public ExactTextPattern(string pattern) : this(new[] { pattern }) { }

                public ExactTextPattern(string[] pattern) {
                    m_Regex = new Regex(pattern.Select(str => "(" + Regex.Escape(str) + ")").JoinToString("|"), RegexOptions.IgnoreCase);
                }
                public ExactTextPattern(Regex expression) {
                    m_Regex = expression;
                }

                public bool IsMatch(IList<Token> tokens, int startIndex, out int matchingTokens) {
                    matchingTokens = 1;
                    return (m_Regex == null || m_Regex.IsMatch(tokens[startIndex].GetContentsString()));
                }
            }

            private class TokenTypePattern : IPattern {
                private readonly TokenType[] m_TokenTypes;

                public TokenTypePattern(params TokenType[] tokenTypes) {
                    m_TokenTypes = tokenTypes;
                }

                public bool IsMatch(IList<Token> tokens, int startIndex, out int matchingTokens) {
                    matchingTokens = 1;
                    return m_TokenTypes.Contains(tokens[startIndex].TokenType);
                }
            }

            private class StarPattern : IPattern {
                private readonly IPattern m_Condition;

                public StarPattern(IPattern condition) { m_Condition = condition; }

                public bool IsMatch(IList<Token> tokens, int startIndex, out int matchingTokens) {
                    matchingTokens = 0;
                    for (int i = startIndex; i < tokens.Count; i++) {
                        int matches;
                        bool matchedAnything = false;
                        while (i + matchingTokens < tokens.Count && m_Condition.IsMatch(tokens, i + matchingTokens, out matches)) {
                            if (matches == 0)
                                throw new Exception("StartPattern requires a pattern that eats tokens.");
                            matchingTokens += matches;
                            matchedAnything = true;
                        }
                        if (matchedAnything)
                            return true;
                    }
                    return false;
                }
            }

            private class NotPattern : IPattern {
                private readonly IPattern m_Condition;

                public NotPattern(IPattern condition) { m_Condition = condition; }

                public bool IsMatch(IList<Token> tokens, int startIndex, out int matchingTokens) {
                    matchingTokens = 1;
                    int ignored;
                    return !m_Condition.IsMatch(tokens, startIndex, out ignored);
                }
            }

            private class StartPattern : IPattern {
                public bool IsMatch(IList<Token> tokens, int startIndex, out int matchingTokens) {
                    matchingTokens = 0;
                    return startIndex == 0;
                }
            }
        }

        private interface IPattern {
            bool IsMatch(IList<Token> tokens, int startIndex, out int matchingTokens);
        }

        #endregion

        #region Character types

        private static bool IsItNumber(char thechar) {
            return char.IsDigit(thechar);
        }
        private static bool IsItLetter(char thechar) {
            return char.IsLetter(thechar) || thechar == '\'';
        }
        private static bool IsItWhiteSpace(char ch) {
            return char.IsWhiteSpace(ch);
        }
        private static bool IsItSeperator(char ch) {
            const char emDash = '—';
            const char enDash = '-';
            //return ch == enDash;
            return ch == c_SeperatorChar;
        }
        private static bool IsItOpenBracket(char ch) {
            return ch == '[' || ch == '(';
        }
        private static bool IsItCloseBracket(char ch) {
            return ch == ']' || ch == ')';
        }

        #endregion

        #region Tokens

        #region Nested type: Token

        private class Token {
            private readonly List<char> m_Contents = new List<char>();
            private readonly TokenType m_TokenType;
            private string m_CachedString;

            public Token(TokenType tokenType) {
                m_TokenType = tokenType;
            }

            public void Add(char c) {
                m_Contents.Add(c);
                m_CachedString = null;
            }

            public TokenType TokenType {
                get { return m_TokenType; }
            }

            public void Print(StringBuilder sb) {
                switch (TokenType) {
                    case TokenType.Seperator:
                        sb.Append("|");
                        break;
                    case TokenType.Number:
                        sb.Append(GetContentsString());
                        sb.Append("#");
                        break;
                    case TokenType.Letter:
                        sb.Append(GetContentsString());
                        sb.Append("!");
                        break;
                    case TokenType.WhiteSpace:
                        //sb.Append(GetContentsString());
                        sb.Append("_");
                        break;
                    case TokenType.OpenBracket:
                        sb.Append("{");
                        break;
                    case TokenType.CloseBracket:
                        sb.Append("}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public string GetContentsString() {
                if (m_CachedString == null)
                    m_CachedString = new string(m_Contents.ToArray());
                return m_CachedString;
            }

            public override string ToString() {
                switch (m_TokenType) {
                    case TokenType.Seperator:
                        return c_SeperatorChar.ToString();
                    case TokenType.WhiteSpace:
                    case TokenType.Number:
                    case TokenType.Letter:
                    case TokenType.OpenBracket:
                    case TokenType.CloseBracket:
                        return GetContentsString();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private class Phrase {
            private readonly List<Token> m_Tokens = new List<Token>();

            public List<Token> Tokens {
                get { return m_Tokens; }
            }

            public Phrase Trim() {
                var p = new Phrase();
                p.m_Tokens.AddRange(m_Tokens.SkipWhile(t => t.TokenType == TokenType.WhiteSpace));
                for (int i = p.m_Tokens.Count - 1; i >= 0; i--) {
                    if (p.m_Tokens[i].TokenType == TokenType.WhiteSpace)
                        p.m_Tokens.RemoveAt(i);
                    else
                        break;
                }
                //for (int i = 0; i < p.m_Tokens.Count - 1; i++) {
                //    if (p.m_Tokens[i].TokenType == TokenType.Seperator && p.m_Tokens[i + 1].TokenType == TokenType.Seperator)
                //        p.m_Tokens.RemoveAt(i);
                //}
                return p;
            }

            public string GetContents() {
                return m_Tokens.JoinToString().Trim(' ', '-');
            }
        }

        #endregion

        #region Nested type: TokenType

        private enum TokenType {
            Seperator = 0,
            WhiteSpace = 1,
            Number = 2,
            Letter = 3,
            OpenBracket = 4,
            CloseBracket = 5,
        }

        #endregion

        #endregion
    }
}