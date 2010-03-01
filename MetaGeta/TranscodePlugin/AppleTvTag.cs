using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagLib;
using TagLib.Mpeg4;

namespace TranscodePlugin {
    class AppleTvTag {
        private readonly AppleTag m_Tags;

        public AppleTvTag(TagLib.File f) {
            m_Tags = (TagLib.Mpeg4.AppleTag) f.GetTag(TagLib.TagTypes.Apple, true);
        }

        public string SeriesTitle {
            get { return GetText(SeriesTitleField); }
            set {
                m_Tags.Album = value;
                SetText(SeriesTitleField, value);
            }
        }
        public int SeasonNumber {
            get { return GetInt(SeasonNumberField); }
            set { SetInt(SeasonNumberField, value); }
        }
        public int EpisodeNumber {
            get { return GetInt(EpisodeNumberField); }
            set { SetInt(EpisodeNumberField, value); }
        }
        public int EpisodeID {
            get { return int.Parse(GetText(EpisodeIDField)); }
            set { SetText(EpisodeIDField, value.ToString()); }
        }

        public string EpisodeDescription {
            get { return GetText(EpisodeDescriptionField); }
            set { SetText(EpisodeDescriptionField, value); }
        }
        public string EpisodeTitle {
            get { return m_Tags.Title; }
            set { m_Tags.Title = value; }
        }

        public DateTimeOffset FirstAiredDate {
            get { return DateTimeOffset.ParseExact(GetText(DateField), "u", null); }
            set { SetText(DateField, value.ToString("u")); ; }
        }

        public void ClearEpisodeImages() {
            m_Tags.Pictures = null;
        }
        public void AddEpisodeImage(string path) {
            m_Tags.Pictures = m_Tags.Pictures.Concat(new[] { new Picture(path) }).ToArray();
        }
        public void AddEpisodeImage(byte[] data) {
            m_Tags.Pictures = m_Tags.Pictures.Concat(new[] { new Picture(new ByteVector(data)) }).ToArray();
        }

        public void SetAsTvShow() {
            SetInt(ItemTypeField, c_TvShowType);
            m_Tags.Genres = new[] { "TV Shows" };
        }

        private static ByteVector ToBytes(string s) {
            return ByteVector.FromString(s, StringType.UTF8, 255);
        }

        private int GetInt(ByteVector field) {
            return (int) m_Tags.DataBoxes(field).First().Data.ToUInt(true);
        }
        private void SetInt(ByteVector field, int value) {
            m_Tags.SetData(field, ByteVector.FromUInt((uint) value, true), (uint) AppleDataBox.FlagType.ContainsData);
        }
        private string GetText(ByteVector field) {
            return m_Tags.GetText(field).First();
        }
        private void SetText(ByteVector field, string value) {
            m_Tags.SetData(field, ByteVector.FromString(value, StringType.UTF8, 255), (uint) AppleDataBox.FlagType.ContainsText);
        }

        private static readonly ByteVector DateField = new ByteVector(169, 100, 97, 121); //©day
        private static readonly ByteVector SeriesTitleField = "tvsh";
        private static readonly ByteVector EpisodeIDField = "tven";
        private static readonly ByteVector EpisodeNumberField = "tves";
        private static readonly ByteVector SeasonNumberField = "tvsn";
        private static readonly ByteVector EpisodeDescriptionField = "desc";
        private static readonly ByteVector ItemTypeField = "stik";

        private const int c_TvShowType = 10;
    }
}
