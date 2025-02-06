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
        private bool clearBaseNotes = false;
        public bool ClearBaseNotes { get => clearBaseNotes; set => SetValue(ref clearBaseNotes, value); }
        private bool useOverride = false;
        public bool UseOverride { get => useOverride; set => SetValue(ref useOverride, value); }
        private string overrideFilePath;
        public string OverrideFilePath { get => overrideFilePath; set => SetValue(ref overrideFilePath, value); }
        private PlayNotesSettings _settings;

        public ExistingNotesImporter(IPlayniteAPI playniteApi, PlayNotesSettings settings)
        {
            _playniteApi = playniteApi;
            _settings = settings;
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

            string gameName = _playniteApi.MainView.SelectedGames.First().Name;
            gameName = Regex.Replace(gameName, @"[^a-zA-Z0-9\s]", "").Trim();

            string path;
            if (!UseOverride)
            {
                path = Path.Combine(_settings.ExistingNotesFolderPath, gameName + ".md").Trim();
            }
            else
            {
                path = OverrideFilePath;
            }

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
                Dictionary<string, string> sections = ExtractHeadingsAndSections(markdownContent, gameName);

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
        private static Dictionary<string, string> ExtractHeadingsAndSections(string markdown, string gameName)
        {
            Dictionary<string, string> sections = new Dictionary<string, string>();

            // Regex to match H1 and H2 headings and their content
            Regex regex = new Regex(@"^(#{1,2})\s+(.*?)\n([\s\S]*?)(?=\n#{1,2} |\z)", RegexOptions.Multiline);

            // Check for the first chunk of text before any headings
            string firstChunk = null;
            int firstHeadingIndex = markdown.IndexOf("#"); // Find where the first heading starts

            if (firstHeadingIndex == -1)
            {
                if (!string.IsNullOrEmpty(markdown))
                {
                    sections[gameName] = markdown.Trim();
                }
            }
            else
            {
                if (firstHeadingIndex > 0)
                {
                    firstChunk = markdown.Substring(0, firstHeadingIndex).Trim(); // Content before any heading
                    if (!string.IsNullOrEmpty(firstChunk))
                    {
                        if (!string.IsNullOrEmpty(firstChunk))
                        {
                            sections["First"] = firstChunk;
                        }
                    }
                }

                // Extract and store sections for H1 and H2 headings
                foreach (Match match in regex.Matches(markdown))
                {
                    string headingType = match.Groups[1].Value.Trim(); // # for H1, ## for H2
                    string heading = match.Groups[2].Value.Trim();
                    string content = match.Groups[3].Value.Trim();

                    if (!string.IsNullOrEmpty(content))
                    {
                        sections[heading] = content;
                    }
                }
            }

            return sections;
        }
    }
}