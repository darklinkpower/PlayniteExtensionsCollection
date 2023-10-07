using DatabaseCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayNotes.Models
{
    public class MarkdownDatabaseItem : ObservableObject, IDatabaseItem<MarkdownDatabaseItem>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        private ObservableCollection<PlayNote> _notes { get; set; } = new ObservableCollection<PlayNote>();
        public ObservableCollection<PlayNote> Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged();
            }
        }

        public MarkdownDatabaseItem GetClone()
        {
            return new MarkdownDatabaseItem
            {
                Id = this.Id,
                Notes = Notes.Select(x => x.GetClone()).ToObservable()
            };
        }
    }

    public class PlayNote : ObservableObject
    {
        public Guid Id { get; set; }
        private string _title { get; set; }
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _text { get; set; }
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public PlayNote() // LiteDb requires a constructor with no parameters
        {
            Id = Guid.NewGuid();
            Title = string.Empty;
            Text = string.Empty;
        }

        public PlayNote(string title, string text)
        {
            Id = Guid.NewGuid();
            Title = title ?? string.Empty;
            Text = text ?? string.Empty;
        }

        private PlayNote(string title, string text, Guid id)
        {
            Title = title ?? string.Empty;
            Text = text ?? string.Empty;
            Id = id;
        }

        public PlayNote GetClone()
        {
            return new PlayNote(this.Title, this.Text, this.Id);
        }
    }
}