using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VNDBNexus.Tests
{
    public class HtmlDescriptionCleanerTests
    {
        [Fact]
        public void Removes_FromLink_AtEnd()
        {
            string input = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.<br><br>[From <a href=\"https://example.com\">Example Source</a>]";
            string expected = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Removes_PartiallyFromLink_AtEnd()
        {
            string input = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.<br><br>[Partially taken from <a href=\"https://example.com\">Example Source</a>]";
            string expected = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Removes_FromLink_InMiddle()
        {
            string input = "Lorem ipsum dolor sit amet.<br>[From <a href=\"https://example.com\">Example Source</a>]<br>Consectetur adipiscing elit.";
            string expected = "Lorem ipsum dolor sit amet.<br>Consectetur adipiscing elit.";
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Removes_PartiallyFromLink_InMiddle()
        {
            string input = "Lorem ipsum dolor sit amet.<br>[Partially taken from <a href=\"https://example.com\">Example Source</a>]<br>Consectetur adipiscing elit.";
            string expected = "Lorem ipsum dolor sit amet.<br>Consectetur adipiscing elit.";
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Trims_Trailing_Brs()
        {
            string input = "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.<br><br><br>";
            string expected = "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Keeps_Inline_Brs()
        {
            string input = "Ut enim ad minim veniam.<br>Quis nostrud exercitation ullamco laboris.<br>Duis aute irure dolor.";
            string expected = input;
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Removes_Both_Trailing_Brs_And_FromLink()
        {
            string input = "Excepteur sint occaecat cupidatat non proident.<br><br>[From <a href=\"https://example.com\">Example Source</a>]<br><br>";
            string expected = "Excepteur sint occaecat cupidatat non proident.";
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Ignores_EmptyString()
        {
            Assert.Equal(string.Empty, HtmlDescriptionCleaner.CleanHtml(""));
        }

        [Fact]
        public void Ignores_WhitespaceOnly()
        {
            Assert.Equal(string.Empty, HtmlDescriptionCleaner.CleanHtml("   \t\n"));
        }

        [Fact]
        public void Handles_SelfClosingBrWithSpaces()
        {
            string input = "Lorem ipsum dolor sit amet.<br /><br /><br />";
            string expected = "Lorem ipsum dolor sit amet.";
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Removes_BracketedNote_WithNoLink()
        {
            string input = "Some content here.<br><br>[From Some Website]";
            string expected = "Some content here.";
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Removes_BracketedNote_WithLink_AndDifferentText()
        {
            string input = "Some content here.<br><br>[Edited from the blog <a href=\"https://example.com\">Some Website</a>]";
            string expected = "Some content here.";
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Removes_BracketedNote_TranslatedFrom()
        {
            string input = "Some content here.<br><br>[Translated from <a href=\"https://example.com\">Official Site</a>]";
            string expected = "Some content here.";
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Keeps_InlineSquareBrackets()
        {
            string input = "This is a funny moment. [laughs] It really happened!";
            string expected = input;
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Keeps_InlineBrackets_WithoutBrTag()
        {
            string input = "The character [Zero III] introduces the Nonary Game.";
            string expected = input;
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Keeps_Brackets_WhenNotPrecededByBr()
        {
            string input = "This story is [based on a true story].";
            string expected = input;
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Keeps_Brackets_WhenInsideSentenceWithBrsAround()
        {
            string input = "Line 1.<br>This is [very suspicious] behavior.<br>Line 3.";
            string expected = input;
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Keeps_Brackets_InsideHtmlTags()
        {
            string input = "<p>This is a paragraph with [inline info]</p>";
            string expected = input;
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }

        [Fact]
        public void Keeps_Brackets_WhenPartOfQuote()
        {
            string input = "He said, \"[This is not over yet]\" and walked away.";
            string expected = input;
            Assert.Equal(expected, HtmlDescriptionCleaner.CleanHtml(input));
        }
    }

}
