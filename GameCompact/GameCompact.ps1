function global:GetMainMenuItems()
{
	param($menuArgs)

	$menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
	$menuItem1.Description = "Compact selected games with xpress8k algorithm"
	$menuItem1.FunctionName = "CompactGameXpress8k"
	$menuItem1.MenuSection = "@Game Compact"
	
	$menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
	$menuItem2.Description = "Compact selected games with xpress16k algorithm"
	$menuItem2.FunctionName = "CompactGameXpress16k"
	$menuItem2.MenuSection = "@Game Compact"
	
	return $menuItem1, $menuItem2
}

function global:CompactGameXpress8k()
{
	$exclude  = @(
	"*.7z",
	"*.aac",
	"*.avi",
	"*.ba",
	"*.br",
	"*.bz2",
	"*.bik",
	"*.pc_binkvid",
	"*.bk2",
	"*.bnk",
	"*.cab",
	"*.dl_",
	"*.docx",
	"*.flac",
	"*.flv",
	"*.gif",
	"*.gz",
	"*.jpeg",
	"*.jpg",
	"*.log",
	"*.lz4",
	"*.lzma",
	"*.lzx",
	"*.m2v",
	"*.m4v",
	"*.mkv",
	"*.mp2",
	"*.mp3",
	"*.mp4",
	"*.mpeg",
	"*.mpg",
	"*.ogg",
	"*.onepkg",
	"*.png",
	"*.pptx",
	"*.rar",
	"*.upk",
	"*.vob",
	"*.vssx",
	"*.vstx",
	"*.wem",
	"*.webm",
	"*.wma",
	"*.wmf",
	"*.wmv",
	"*.xap",
	"*.xnb",
	"*.xlsx",
	"*.xz",
	"*.zst",
	"*.zstd"
	)
	$Game = $PlayniteApi.MainView.SelectedGames
	$Game | ForEach-Object { 
		if ($($_.InstallDirectory))
		{
			$path = Join-Path "$($_.InstallDirectory)" -ChildPath "\*"
			$files = (Get-ChildItem -path $path -exclude $exclude -recurse | Select-Object -Expand name)
			compact /exe:xpress8k /c /s:"$($_.InstallDirectory)" /a /f /q /i $files | Out-File $env:temp\Compact_Game.txt
			$results = Get-Content $env:temp\Compact_Game.txt | Select-Object -Last 3
			$PlayniteApi.Dialogs.ShowMessage("$($_.name) compact with Xpress8k algorithm has finished. Results: $($results)");
			If (Test-Path $env:temp\Compact_Game.txt)
			{
				Remove-Item $env:temp\Compact_Game.txt
			}
		}
		else
		{
			$PlayniteApi.Dialogs.ShowMessage("$($_.name) is not installed. Can't compact game.");
		}
	}
}

function global:CompactGameXpress16k()
{
	$exclude  = @(
	"*.7z",
	"*.aac",
	"*.avi",
	"*.ba",
	"*.br",
	"*.bz2",
	"*.bik",
	"*.pc_binkvid",
	"*.bk2",
	"*.bnk",
	"*.cab",
	"*.dl_",
	"*.docx",
	"*.flac",
	"*.flv",
	"*.gif",
	"*.gz",
	"*.jpeg",
	"*.jpg",
	"*.log",
	"*.lz4",
	"*.lzma",
	"*.lzx",
	"*.m2v",
	"*.m4v",
	"*.mkv",
	"*.mp2",
	"*.mp3",
	"*.mp4",
	"*.mpeg",
	"*.mpg",
	"*.ogg",
	"*.onepkg",
	"*.png",
	"*.pptx",
	"*.rar",
	"*.upk",
	"*.vob",
	"*.vssx",
	"*.vstx",
	"*.wem",
	"*.webm",
	"*.wma",
	"*.wmf",
	"*.wmv",
	"*.xap",
	"*.xnb",
	"*.xlsx",
	"*.xz",
	"*.zst",
	"*.zstd"
	)
	$Game = $PlayniteApi.MainView.SelectedGames
	$Game | ForEach-Object { 
		if ($($_.InstallDirectory))
		{
			$path = Join-Path "$($_.InstallDirectory)" -ChildPath "\*"
			$files = (Get-ChildItem -path $path -exclude $exclude -recurse | Select-Object -Expand name)
			compact /exe:xpress16k /c /s:"$($_.InstallDirectory)" /a /f /q /i $files | Out-File $env:temp\Compact_Game.txt
			$results = Get-Content $env:temp\Compact_Game.txt | Select-Object -Last 3
			$PlayniteApi.Dialogs.ShowMessage("$($_.name) compact with Xpress16k algorithm has finished. Results: $($results)");
			If (Test-Path $env:temp\Compact_Game.txt)
			{
				Remove-Item $env:temp\Compact_Game.txt
			}
		}
		else
		{
			$PlayniteApi.Dialogs.ShowMessage("$($_.name) is not installed. Can't compact game.");
		}
	}
}