using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CoverCollageMaker.Domain.Enums;

namespace CoverCollageMaker.Domain.ValueObjects
{
    public class CollageParameters
    {
        public int Rows { get; set; } = 5;
        public int Columns { get; set; } = 5;
        public int FinalWidth { get; set; } = 1000;
        public int FinalHeight { get; set; } = 1000;
        public int CellWidth { get; set; } = 200;
        public int CellHeight { get; set; } = 200;
        public int HorizontalSpacing { get; set; } = 0;
        public int VerticalSpacing { get; set; } = 0;
        public int Padding { get; set; } = 0;
        public Color BackgroundColor { get; set; } = Colors.Transparent;
        public string TextFontName { get; set; } = "Arial";
        public int TextFontSize { get; set; } = 12;
        public bool TextFontBold { get; set; } = false;
        public bool TextFontItalic { get; set; } = false;
        public Color TextColor { get; set; } = Colors.White;
        public TextInsertMethod TextInsertMethod { get; set; } = TextInsertMethod.Wrap;
        public TextHorizontalAlignment TextHorizontalAlignment { get; set; } = TextHorizontalAlignment.Center;
        public TextVerticalAlignment TextVerticalAlignment { get; set; } = TextVerticalAlignment.Center;
        public ImagesInsertMode ImagesInsertMode { get; set; } = ImagesInsertMode.Original;
        public ImagesInsertOrder ImagesInsertOrder { get; set; } = ImagesInsertOrder.Ascending;
        public bool ShowTexts { get; set; } = false;
        public bool TextAntiAliasing { get; set; } = true;

        public CollageParameters CreateClone()
        {
            return new CollageParameters
            {
                Rows = this.Rows,
                Columns = this.Columns,
                FinalWidth = this.FinalWidth,
                FinalHeight = this.FinalHeight,
                CellWidth = this.CellWidth,
                CellHeight = this.CellHeight,
                HorizontalSpacing = this.HorizontalSpacing,
                VerticalSpacing = this.VerticalSpacing,
                Padding = this.Padding,
                BackgroundColor = this.BackgroundColor,
                TextFontName = this.TextFontName,
                TextFontSize = this.TextFontSize,
                TextFontBold = this.TextFontBold,
                TextFontItalic = this.TextFontItalic,
                TextColor = this.TextColor,
                TextInsertMethod = this.TextInsertMethod,
                TextHorizontalAlignment = this.TextHorizontalAlignment,
                TextVerticalAlignment = this.TextVerticalAlignment,
                ImagesInsertMode = this.ImagesInsertMode,
                ImagesInsertOrder = this.ImagesInsertOrder,
                ShowTexts = this.ShowTexts,
                TextAntiAliasing = this.TextAntiAliasing
            };
        }
    }
}