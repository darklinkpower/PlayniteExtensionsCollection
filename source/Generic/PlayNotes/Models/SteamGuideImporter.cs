using HtmlAgilityPack;
using Playnite.SDK;
using ReverseMarkdown;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FlowHttp;

namespace PlayNotes.Models
{
    public class SteamGuideImporter : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private const string _guideLinkMatchPattern = @"^https:\/\/steamcommunity\.com\/sharedfiles\/filedetails\/\?id=\d+$";
        private const string _steamLinkFilterPrefix = @"https://steamcommunity.com/linkfilter/?url=";
        private readonly IPlayniteAPI _playniteApi;
        private readonly List<SteamHtmlTransformDefinition> _contentTransformElems;
        private readonly Dictionary<string, string> _unsupportedElemsRegexFormatters;
        private string _url = string.Empty;
        public string Url { get => _url; set => SetValue(ref _url, value); }

        private bool importSectionsSeparately = false;
        public bool ImportSectionsSeparately { get => importSectionsSeparately; set => SetValue(ref importSectionsSeparately, value); }

        private bool clearBaseNotes = false;
        public bool ClearBaseNotes { get => clearBaseNotes; set => SetValue(ref clearBaseNotes, value); }

        public SteamGuideImporter(IPlayniteAPI playniteApi)
        {
            _playniteApi = playniteApi;
            _contentTransformElems = new List<SteamHtmlTransformDefinition>()
            {
                new SteamHtmlTransformDefinition("span", "bb_strike", "strike"),
                new SteamHtmlTransformDefinition("div", "bb_h1", "h1"),
                new SteamHtmlTransformDefinition("div", "bb_h2", "h2"),
                new SteamHtmlTransformDefinition("div", "bb_h3", "h3"),
                new SteamHtmlTransformDefinition("div", "bb_h4", "h4"),
                new SteamHtmlTransformDefinition("div", "bb_h5", "h5")
            };

            _unsupportedElemsRegexFormatters = new Dictionary<string, string>
            {
                {@"<strike class=""bb_strike"">((.|\n)*?)</strike>", "~~$1~~" },
                {@"<u>((.|\n)*?)</u>", "*$1*" }
            };
        }

        public void ResetValues()
        {
            Url = string.Empty;
            ImportSectionsSeparately = false;
            ClearBaseNotes = false;
        }

        public bool ImportSteamGuide(MarkdownDatabaseItem databaseItem, CancellationToken cancelToken)
        {
            void ShowFailedMessage()
            {
                _playniteApi.Dialogs.ShowErrorMessage(
                        ResourceProvider.GetString("PlayNotes_SteamGuideImporterFailedObtainGuideMessage"),
                        ResourceProvider.GetString("PlayNotes_SteamGuideImporterLabel")
                    );
            }

            if (Url.IsNullOrWhiteSpace() ||
                !Regex.IsMatch(Url, _guideLinkMatchPattern))
            {
                _playniteApi.Dialogs.ShowErrorMessage(
                        ResourceProvider.GetString("PlayNotes_InvalidUrlMessage"),
                        ResourceProvider.GetString("PlayNotes_SteamGuideImporterLabel")
                    );
                return false;
            }

            var downloadResult = HttpRequestFactory.GetFlowHttpRequest().WithUrl(Url).DownloadString(cancelToken);
            if (downloadResult.IsCancelled)
            {
                return false;
            }

            if (!downloadResult.IsSuccess)
            {
                ShowFailedMessage();
                return false;
            }

            logger.Debug($"Steam guide url: {Url}");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(downloadResult.Content);
            var guideTitleDiv = doc.DocumentNode.SelectSingleNode("//div[@class='workshopItemTitle']");
            if (guideTitleDiv is null)
            {
                logger.Debug($"guideTitleDiv not found");
                ShowFailedMessage();
                return false;
            }

            var guideTitle = guideTitleDiv.InnerText.HtmlDecode().Trim();
            var guideContentDiv = doc.DocumentNode.SelectSingleNode("//div[@class='guide subSections']");
            if (guideContentDiv is null)
            {
                logger.Debug($"guideContentDiv not found");
                ShowFailedMessage();
                return false;
            }

            var subsectionDivs = guideContentDiv.SelectNodes(".//div[@class='subSection detailBox']");
            if (subsectionDivs is null)
            {
                logger.Debug($"subsectionDivs not found");
                ShowFailedMessage();
                return false;
            }

            var notes = new List<PlayNote>();
            var htmlToMarkdownConverter = new Converter();
            foreach (var subsectionDiv in subsectionDivs)
            {
                var innerHtml = subsectionDiv.InnerHtml;
                var subSectionTitleDiv = subsectionDiv.SelectSingleNode(".//div[@class='subSectionTitle']");
                var subSectionDescDiv = subsectionDiv.SelectSingleNode(".//div[@class='subSectionDesc']");
                if (subSectionTitleDiv is null || subSectionDescDiv is null)
                {
                    continue;
                }

                foreach (var childNode in subSectionDescDiv.ChildNodes)
                {
                    foreach (var transformElem in _contentTransformElems)
                    {
                        if (childNode.Name != transformElem.Name)
                        {
                            continue;
                        }

                        if (childNode.GetAttributeValue("class", string.Empty) != transformElem.OriginalClass)
                        {
                            continue;
                        }

                        // &gt;
                        childNode.Name = transformElem.NewName;
                        break;
                    }
                }

                var sectionTitle = subSectionTitleDiv.InnerText.HtmlDecode().Trim();
                var text = htmlToMarkdownConverter.Convert(subSectionDescDiv.OuterHtml.Trim());

                // Some elements not supported by the converter need to be manually converted
                foreach (var item in _unsupportedElemsRegexFormatters)
                {
                    text = Regex.Replace(text, item.Key, item.Value);
                }

                text = text.Replace(_steamLinkFilterPrefix, string.Empty);
                text = text.HtmlDecode(); // For some reason some html entities don't get decoded by the converter
                notes.Add(new PlayNote(sectionTitle, text));
            }

            if (!notes.HasItems())
            {
                logger.Debug($"Notes not found");
                ShowFailedMessage();
                return false;
            }

            if (ClearBaseNotes)
            {
                databaseItem.Notes.Clear();
            }

            if (ImportSectionsSeparately)
            {
                foreach (var note in notes)
                {
                    databaseItem.Notes.Add(note);
                }
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var note in notes)
                {
                    sb.Append(string.Format("# {0}", note.Title));
                    sb.Append("\n\n---\n\n");
                    sb.Append(string.Format(note.Text));
                    sb.Append("\n\n");
                }

                var newNote = new PlayNote(guideTitle, sb.ToString().Trim());
                databaseItem.Notes.Add(newNote);
            }

            return true;
        }
    }
}