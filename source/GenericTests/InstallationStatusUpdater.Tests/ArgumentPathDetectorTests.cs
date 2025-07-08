using InstallationStatusUpdater.Application;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Tests
{
    [TestFixture]
    public class ArgumentPathDetectorTests
    {
        [Test]
        public void DetectsQuotedAndUnquotedPaths()
        {
            var result = StringPathsDetector.ExtractPathsFromArguments(@"""C:\Game\game.exe"" tools\run.bat --silent");
            Assert.That(result, Is.EqualTo(new[] { @"C:\Game\game.exe", @"tools\run.bat" }));
        }

        [TestCase(@"""C:\Games\My Game\game.exe""", new[] { @"C:\Games\My Game\game.exe" }, TestName = "QuotedPathWithSpaces")]
        [TestCase(@"""G:\Emulators\bin\emu.exe"" roms\game.rom", new[] { @"G:\Emulators\bin\emu.exe", @"roms\game.rom" }, TestName = "QuotedAndUnquotedMultiplePaths")]
        [TestCase(@"{InstallDir}\game.exe", new[] { @"{InstallDir}\game.exe" }, TestName = "PathWithInstallDirVariable")]
        [TestCase(@"tools\patch.bat", new[] { @"tools\patch.bat" }, TestName = "SimpleRelativePath")]
        [TestCase(@"""..\bin\launcher.bat""", new[] { @"..\bin\launcher.bat" }, TestName = "QuotedRelativePathWithParentDir")]
        [TestCase(@"""--runmode=admin""", new string[0], TestName = "QuotedFlagValueNoPaths")]
        [TestCase(@"-fullscreen -noinput", new string[0], TestName = "FlagsWithoutPaths")]
        [TestCase(@"C:/Games/Launcher.exe", new[] { @"C:\Games\Launcher.exe" }, TestName = "ForwardSlashPathNormalized")]
        [TestCase(@" C:/Games/Launcher.exe", new[] { @"C:\Games\Launcher.exe" }, TestName = "TrimsLeadingWhitespace")]
        [TestCase(@"C:/Games/Launcher.exe ", new[] { @"C:\Games\Launcher.exe" }, TestName = "TrimsTrailingWhitespace")]
        [TestCase(@"  C:/Games/Launcher.exe  ", new[] { @"C:\Games\Launcher.exe" }, TestName = "TrimsLeadingAndTrailingWhitespace")]
        [TestCase(@"./game", new[] { @".\game" }, TestName = "RelativeUnixStylePathNormalized")]
        [TestCase(@"""./local/game with space.exe""", new[] { @".\local\game with space.exe" }, TestName = "QuotedRelativePathWithSpaces")]
        [TestCase(@"""C:\path\to\app.exe"" ""C:\path\to\config.ini""", new[] { @"C:\path\to\app.exe", @"C:\path\to\config.ini" }, TestName = "MultipleQuotedAbsolutePaths")]
        [TestCase(@"", new string[0], TestName = "EmptyStringReturnsEmpty")]
        public void ExtractPathsFromArguments_CoversVariousCases(string args, string[] expected)
        {
            var result = StringPathsDetector.ExtractPathsFromArguments(args);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void HandlesNullGracefully()
        {
            var result = StringPathsDetector.ExtractPathsFromArguments(null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void HandlesUnmatchedQuotes()
        {
            var args = @"""C:\Games\UnfinishedPath.exe";
            var result = StringPathsDetector.ExtractPathsFromArguments(args);
            Assert.That(result, Is.EqualTo(new[] { @"C:\Games\UnfinishedPath.exe" }));
        }

        [Test]
        public void ExtractsPathsInsideFlags()
        {
            var args = @"--file=C:\Game.exe -outdir=tools/patch.bat";
            var result = StringPathsDetector.ExtractPathsFromArguments(args);
            Assert.That(result, Is.EqualTo(new[] { @"C:\Game.exe", @"tools\patch.bat" }));
        }

        [Test]
        public void ExtractPathsFromArguments_HandlesMultipleEqualSigns()
        {
            var args = @"--config=C:\path=with=equals\settings.ini";
            var result = StringPathsDetector.ExtractPathsFromArguments(args);
            Assert.That(result, Is.EqualTo(new[] { @"C:\path=with=equals\settings.ini" }));
        }

        [Test]
        public void ExtractPathsFromArguments_IgnoresFlagsThatResemblePaths()
        {
            var args = @"--fakepath=/dev/null -somethingelse";
            var result = StringPathsDetector.ExtractPathsFromArguments(args);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ExtractPathsFromArguments_PathInsideFlagValueWithQuotes()
        {
            var args = @"--file=""C:\Games\RealGame.exe""";
            var result = StringPathsDetector.ExtractPathsFromArguments(args);
            Assert.That(result, Is.EqualTo(new[] { @"C:\Games\RealGame.exe" }));
        }


    }
}
