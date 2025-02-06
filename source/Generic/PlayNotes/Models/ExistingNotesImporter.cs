using HtmlAgilityPack;
using Playnite.SDK;
using ReverseMarkdown;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Markdig;

namespace PlayNotes.Models
{
    public class ExistingNotesImporter : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _playniteApi;
        private readonly List<SteamHtmlTransformDefinition> _contentTransformElems;
        private readonly Dictionary<string, string> _unsupportedElemsRegexFormatters;
        private bool clearBaseNotes = false;
        public bool ClearBaseNotes { get => clearBaseNotes; set => SetValue(ref clearBaseNotes, value); }

        private PlayNotesSettings _settings;

        public ExistingNotesImporter(IPlayniteAPI playniteApi, PlayNotesSettings settings)
        {
            _playniteApi = playniteApi;
            _settings = settings;
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
            ClearBaseNotes = false;
        }

        public bool ImportExistingNotes(MarkdownDatabaseItem databaseItem, CancellationToken cancelToken)
        {
            if (_playniteApi.MainView.SelectedGames?.Any() != true)
            {
                return false;
            }

            string path = Path.Combine(_settings.ExistingNotesFolderPath, _playniteApi.MainView.SelectedGames.First().Name + ".md");
            path = path.Trim();

            if (string.IsNullOrEmpty(path) ||
                !File.Exists(path))
            {
                _playniteApi.Dialogs.ShowErrorMessage(
                    $"Could not find the notes file, {path}",
                    "Invalid file path");
                return false;
            }

            if (!Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                _playniteApi.Dialogs.ShowErrorMessage(
                    "Extension not supported",
                    $"Invalid file extension. Only markdown files are supported.");
                return false;
            }

            try
            {
                string markdownContent = File.ReadAllText(path);
                Dictionary<string, string> sections = ExtractH2Sections(markdownContent);

                var notes = new List<PlayNote>();
                foreach (var section in sections)
                {
                    notes.Add(new PlayNote(section.Key, section.Value));
                }

                if (!notes.HasItems())
                {
                    logger.Debug($"Notes not found");
                    _playniteApi.Dialogs.ShowErrorMessage(
                      "Could not import notes",
                      "Could not import notes");
                    return false;
                }

                if (ClearBaseNotes)
                {
                    databaseItem.Notes.Clear();
                }

                foreach (var note in notes)
                {
                    databaseItem.Notes.Add(note);
                }
            }
            catch (Exception ex)
            {
                logger.Debug($"Exception reading file: {path}, exception: {ex}");
                _playniteApi.Dialogs.ShowErrorMessage($"Error while reading file {path}");
                return false;
            }

            return true;
        }

        private static Dictionary<string, string> ExtractH2Sections(string markdown)
        {
            Dictionary<string, string> sections = new Dictionary<string, string>();

            // Regex to match H2 headings and their content
            Regex regex = new Regex(@"##\s+(.*?)\n([\s\S]*?)(?=\n## |\z)", RegexOptions.Multiline);

            // Extract and store sections
            foreach (Match match in regex.Matches(markdown))
            {
                string heading = match.Groups[1].Value.Trim();
                string content = match.Groups[2].Value.Trim();
                sections[heading] = content;
            }

            return sections;
        }
    }
}