using JastUsaLibrary.ProgramsHelper.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JastUsaLibrary.ProgramsHelper
{
    // Obtained from https://github.com/JosefNemec/Playnite
    public static class Programs
    {
        public static Program GetProgramData(string filePath)
        {
            var file = new FileInfo(filePath);
            if (file.Extension?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true)
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);
                var programName = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : Path.GetFileName(file.FullName);
                return new Program
                {
                    Path = file.FullName,
                    Icon = file.FullName,
                    WorkDir = Path.GetDirectoryName(file.FullName),
                    Name = programName
                };
            }
            else if (file.Extension?.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) == true)
            {
                var data = GetLnkShortcutData(file.FullName);
                var name = Path.GetFileName(file.Name);
                if (File.Exists(data.Path))
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(data.Path);
                    name = !string.IsNullOrEmpty(versionInfo.ProductName?.Trim()) ? versionInfo.ProductName : Path.GetFileName(file.FullName);
                }

                var program = new Program
                {
                    Path = data.Path,
                    WorkDir = data.WorkDir,
                    Arguments = data.Arguments,
                    Name = name
                };

                if (!data.Icon.IsNullOrEmpty())
                {
                    var reg = Regex.Match(data.Icon, @"^(.+),(\d+)$");
                    if (reg.Success)
                    {
                        program.Icon = reg.Groups[1].Value;
                        program.IconIndex = int.Parse(reg.Groups[2].Value);
                    }
                    else
                    {
                        program.Icon = data.Icon;
                    }
                }
                else
                {
                    program.Icon = data.Path;
                }

                return program;
            }
            else if (file.Extension?.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new Program
                {
                    Path = file.FullName,
                    Name = Path.GetFileName(file.FullName),
                    WorkDir = Path.GetDirectoryName(file.FullName)
                };
            }

            throw new NotSupportedException("Only exe, bat and lnk files are supported.");
        }

        public static Program GetLnkShortcutData(string lnkPath)
        {
            var shell = new IWshRuntimeLibrary.WshShell();
            var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(lnkPath);
            return new Program()
            {
                Path = link.TargetPath,
                Icon = link.IconLocation == ",0" ? link.TargetPath : link.IconLocation,
                Arguments = link.Arguments,
                WorkDir = link.WorkingDirectory,
                Name = link.FullName
            };
        }


    }
}