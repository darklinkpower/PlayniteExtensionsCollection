function GetGameMenuItems
{
    param(
        $menuArgs
    )

    
    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem1.Description = "Open game(s) save directories"
    $menuItem1.FunctionName = "Invoke-OpenSaveDirectories"
    $menuItem1.MenuSection = "Game directories"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description = "Open game(s) configuration directories"
    $menuItem2.FunctionName = "Invoke-OpenConfigDirectories"
    $menuItem2.MenuSection = "Game directories"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description = "Refresh directories from PCGamingWiki"
    $menuItem3.FunctionName = "Invoke-RefreshDirectories"
    $menuItem3.MenuSection = "Game directories"

    return $menuItem1, $menuItem2, $menuItem3
}

function Invoke-RefreshDirectories
{
    $getDataSuccess = 0
    $getDataFailed = 0 
    $PlayniteApi.MainView.SelectedGames | ForEach-Object {
        $success = Get-GameDirectories $_
        if ($success -eq $true)
        {
            $getDataSuccess++
        }
        else
        {
            $getDataFailed++
        }
    }

    $PlayniteApi.Dialogs.ShowMessage(("Done.`n`nObtained data of {0} game(s).`nCould not obtain data of {1} game(s)." -f $getDataSuccess.ToString(), $getDataFailed.ToString()))
}

function Get-DownloadString
{
    param (
        $url
    )
    
    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.Encoding = [System.Text.Encoding]::UTF8
        $DownloadedString = $webClient.DownloadString($url)
        $webClient.Dispose()
        return $DownloadedString
    } catch {
        $errorMessage = $_.Exception.Message
        $__logger.Info("Error downloading file `"$url`". Error: $errorMessage")
        $PlayniteApi.Dialogs.ShowMessage("Error downloading file `"$url`". Error: $errorMessage")
        return
    }
}

function Get-PathFormatList
{  
    $list = @{
        "{{p|uid}}" = "<uid>"
        "{{p|steam}}" = "<steam>"
        "{{p|uplay}}" = "<uplay>"
        "{{p|username}}" = "<username>"
        "{{p|userprofile}}" = "<userprofile>"
        "{{p|userprofile\documents}}" = "<userprofile\documents>"
        "{{p|userprofile\appdata\locallow}}" = "<userprofile\appdata\locallow>"
        "{{p|appdata}}" = "<appdata>"
        "{{p|localappdata}}" = "<localappdata>"
        "{{p|public}}" =  "<public>"
        "{{p|allusersprofile}}" = "<allusersprofile>"
        "{{p|programdata}}" = "<programdata>"
        "{{p|windir}}" = "<windir>"
        "{{p|syswow64}}" = "<syswow64>"
        "{{p|game}}" = "<game>"
    }

    return $list
}

function Get-ReplaceList
{
    # Get Steam installation path
    $key = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::CurrentUser, [Microsoft.Win32.RegistryView]::Registry64)
    $regSubKey =  $Key.OpenSubKey("SOFTWARE\Valve\Steam")
    if ($null -ne $regSubKey)
    {
        $steamPath = $regSubKey.GetValue("SteamPath").Replace("/", "\")
    }
    if ($null -eq $steamPath)
    {
        $steamPath = 'C:\Program Files (x86)\Steam'
        $__logger.Warn("Could not find Steam registry value. Default path will be used.")
    }

    # Get Uplay installation path
    $key = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, [Microsoft.Win32.RegistryView]::Registry64)
    $regSubKey =  $Key.OpenSubKey("SOFTWARE\WOW6432Node\Ubisoft\Launcher")
    if ($null -ne $regSubKey)
    {
        $ubisoftPath = $regSubKey.GetValue("InstallDir").Replace("Ubisoft Game Launcher\", "Ubisoft Game Launcher")
    }
    if ($null -eq $ubisoftPath)
    {
        $ubisoftPath = 'C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher'
        $__logger.Warn("Could not find Ubisoft registry value. Default path will be used.")
    }
    
    $pathsConvertList = @{
        "<uid>" = "*"
        "<steam>" = $steamPath
        "<uplay>" = $ubisoftPath
        "<username>" = $env:USERNAME
        "<userprofile>" = $env:USERPROFILE
        "<userprofile\documents>" = [Environment]::GetFolderPath("MyDocuments")
        "<userprofile\appdata\locallow>" = [System.IO.Path]::Combine($env:USERPROFILE, "AppData", "LocalLow")
        "<appdata>" = $env:APPDATA
        "<localappdata>" = $env:LOCALAPPDATA
        "<public>" =  $env:PUBLIC
        "<allusersprofile>" = [Environment]::GetFolderPath("CommonApplicationData")
        "<programdata>" = [Environment]::GetFolderPath("CommonApplicationData")
        "<windir>" = [Environment]::GetFolderPath("System")
        "<syswow64>" = [Environment]::GetFolderPath("SystemX86")
        "<code>" = "*"
    }

    return $pathsConvertList
}

function Get-SkipList
{
    $list = @(
        "{{p|hkcu}}",
        "{{p|hklm}}",
        "{{p|wow64}}"
    )
    
    return $list
}

function Get-DirectoriesFromContent
{
    param (
        [string]$downloadedString,
        [string]$type,
        [string]$gameLibraryPlugin
    )

    $formatList = Get-PathFormatList
    $skipList = Get-SkipList
    $regex = '{{{{Game data\/{0}\|[^\|]+\|.*?(?=}}}}\\n)' -f $type
    $savePathsMatches = [regex]::Matches($downloadedString, $regex)
    [System.Collections.Generic.List[string]]$savePaths = @()

    foreach ($savePath in $savePathsMatches) {
        $path = [regex]::Unescape($savePath.Value)
        if (($path -match ("^{{{{Game data\/{0}\|(macOS \()?OS X" -f $type)) -or ($savePath -match ("^{{{{Game data\/{0}\|Linux" -f $type)))
        {
            continue
        }
        elseif (($path -match ("^{{{{Game data\/{0}\|GOG.com" -f $type)) -and ($gameLibraryPlugin -ne "GogLibrary"))
        {
            continue
        }
        elseif (($path -match ("^{{{{Game data\/{0}\|Steam" -f $type)) -and ($gameLibraryPlugin -ne "SteamLibrary"))
        {
            continue
        }
        elseif (($path -match ("^{{{{Game data\/{0}\|Microsoft Store" -f $type)) -and ($gameLibraryPlugin -ne "XboxLibrary"))
        {
            continue
        }
        elseif (($path -match ("^{{{{Game data\/{0}\|Uplay" -f $type)) -and ($gameLibraryPlugin -ne "UplayLibrary"))
        {
            continue
        }
        elseif (($path -match ("^{{{{Game data\/{0}\|Epic Games Store" -f $type)) -and ($gameLibraryPlugin -ne "EpicLibrary"))
        {
            continue
        }

        $path = $path.Replace("\\", "\") -replace ("^{{{{Game data\/{0}\|[^|]+\|" -f $type) -replace '{{code\|[^}]+}}', "<code>"

        if ([string]::IsNullOrEmpty($path))
        {
            continue
        }
        
        foreach ($variable in $formatList.GetEnumerator()) {
            $path = $path -replace [regex]::Escape($variable.Name), $variable.Value
        }

        foreach ($item in ($path -replace " ?\| ?", "?").Split("?"))
        {
            foreach ($string in $skipList) {
                if ($item -match $string)
                {
                    continue
                }
            }
            $savePaths.Add($item)
        } 
    }

    return $savePaths
}

function Get-GameDirectories
{
    param (
        [Playnite.SDK.Models.Game]$game
    )

    $pathsStorePath = [System.IO.Path]::Combine($CurrentExtensionDataPath, ("{0}.json" -f $game.Id.ToString()))
    $gameLibraryPlugin = [Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId).ToString()
    if ($gameLibraryPlugin -eq 'SteamLibrary')
    {
        $uri = "https://www.pcgamingwiki.com/w/api.php?action=askargs&conditions=Steam+AppID::{0}&format=json" -f $game.GameId
    }
    elseif ($gameLibraryPlugin -eq 'GogLibrary')
    {
        $uri = "https://www.pcgamingwiki.com/w/api.php?action=askargs&conditions=GOGcom+ID::{0}&format=json" -f $game.GameId
    }
    else
    {
        $steamAppId = Get-SteamAppId $game
        if ($null -ne $steamAppId)
        {
            $uri = "https://www.pcgamingwiki.com/w/api.php?action=askargs&conditions=Steam+AppID::{0}&format=json" -f $steamAppId
        }
        else
        {
            return
        }
    }

    $json = Get-DownloadString $uri
    if ($null -eq $json)
    {
        return
    }

    try {
        $pageTitle = [uri]::EscapeDataString((($json | ConvertFrom-Json).query.results.PSObject.Properties | Select-Object -First 1).Value.fulltext)
        $uri = "https://www.pcgamingwiki.com/w/api.php?action=query&titles={0}&prop=revisions&rvprop=content&format=json" -f $pageTitle
        $downloadedString = Get-DownloadString $uri
    } catch {
        $PlayniteApi.Dialogs.ShowMessage(("Couldn't get PCGamingWiki page for {0}" -f $game.Name))
    }

    if ($downloadedString)
    {
        $saveDirectories = @(Get-DirectoriesFromContent $downloadedString "saves" $gameLibraryPlugin)
        $configDirectories = @(Get-DirectoriesFromContent $downloadedString "config" $gameLibraryPlugin)
    
        $directoriesObject = [PSCustomObject]@{
            SaveDirectories = $saveDirectories
            ConfigDirectories = $configDirectories
        }
    
        $directoriesObject | ConvertTo-Json | Out-File $pathsStorePath
        return $true
    }
    else
    {
        return $false
    }
}

function Invoke-OpenSaveDirectories
{
    Invoke-OpenDirectories "SaveDirectories"
}

function Invoke-OpenConfigDirectories
{
    Invoke-OpenDirectories "ConfigDirectories"
}



function Invoke-OpenDirectories
{
    param (
        [string]$directoryProperty
    )

    if (($PlayniteApi.MainView.SelectedGames | Where-Object {(($_.PluginId -ne "cb91dfc9-b977-43bf-8e70-55f46e410fab") -and ($_.PluginId -ne "aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e"))}).Count -gt 0)
    {
        Set-GlobalAppList $false
    }
    
    $pathsConvertList = Get-ReplaceList

    foreach ($game in $PlayniteApi.MainView.SelectedGames) {

        $pathsStorePath = [System.IO.Path]::Combine($CurrentExtensionDataPath, ("{0}.json" -f $game.Id.ToString()))
        if (![System.IO.File]::Exists($pathsStorePath))
        {
            Get-GameDirectories $game
        }
        if (![System.IO.File]::Exists($pathsStorePath))
        {
            $PlayniteApi.Dialogs.ShowMessage(("Couldn't obtain directories for game" -f $game.Name))
            continue
        }

        $storedPaths = ([System.IO.File]::ReadAllLines($pathsStorePath) | ConvertFrom-Json).$directoryProperty

        [System.Collections.Generic.List[string]]$saveItems = @()
        foreach ($storedPath in $storedPaths) {
            foreach ($variable in $pathsConvertList.GetEnumerator()) {
                $storedPath = $storedPath -replace [regex]::Escape($variable.Name), $variable.Value
            }
    
            if ($game.InstallDirectory)
            {
                $storedPath = $storedPath -replace '<game>', ($game.InstallDirectory -replace '\\$', "")
            }
            
            if ($storedPath -match "\*")
            {
                try {
                    foreach ($subItem in Get-ChildItem $storedPath) {
                        $saveItems.Add($subItem.FullName)
                    }
                } catch { }
            }
            else
            {
                $saveItems.Add($storedPath)
            }
        }
    
        [System.Collections.Generic.List[string]]$saveDirectories = @()
        foreach ($item in $saveItems) {
            if ([System.IO.Directory]::Exists($item))
            {
                $saveDirectories.Add($item)
            }
            elseif ([System.IO.File]::Exists($item))
            {
                $directory = [System.IO.Path]::GetDirectoryName($item)
                if ([System.IO.Directory]::Exists($directory))
                {
                    $saveDirectories.Add($directory)
                }
            }
        }

        if ($saveDirectories.count -eq 0)
        {
            $PlayniteApi.Dialogs.ShowMessage(("Directories not detected for {0}" -f $game.Name))
        }
        $saveDirectories | Select-Object -Unique | ForEach-Object {
            Start-Process $_
        }
    }
}

function Set-GlobalAppList
{
    param (
        [bool]$forceDownload
    )
    
    # Get Steam AppList
    $appListPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'AppList.json'
    if (!(Test-Path $appListPath) -or ($forceDownload -eq $true))
    {
        Get-SteamAppList -AppListPath $appListPath
    }
    $global:appList = @{}
    [object]$appListJson = [System.IO.File]::ReadAllLines($appListPath) | ConvertFrom-Json
    foreach ($steamApp in $appListJson) {
        # Use a try block in case multple apps use the same name
        try {
            $appList.add($steamApp.name, $steamApp.appid)
        } catch {}
    }

    $__logger.Info(("Global applist set from {0}" -f $appListPath))
}

function Get-SteamAppList
{
    param (
        $appListPath
    )

    $uri = 'https://api.steampowered.com/ISteamApps/GetAppList/v2/'
    $steamAppList = Get-DownloadString $uri
    if ($null -ne $steamAppList)
    {
        [array]$appListContent = ($steamAppList | ConvertFrom-Json).applist.apps
        foreach ($steamApp in $appListContent) {
            $steamApp.name = $steamApp.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        }
        ConvertTo-Json $appListContent -Depth 2 -Compress | Out-File -Encoding 'UTF8' -FilePath $appListPath
        $__logger.Info("Downloaded AppList")
        $global:appListDownloaded = $true
    }
    else
    {
        exit
    }
}

function Get-SteamAppId
{
    param (
        [Playnite.SDK.Models.Game]$game
    )

    $gamePlugin = [Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId).ToString()
    $__logger.Info(("Get-SteammAppId start. Game: {0}, Plugin: {1}" -f $game.Name, $gamePlugin))

    # Use GameId for Steam games
    if ($gamePlugin -eq "SteamLibrary")
    {
        $__logger.Info(("Game: {0}, appId {1} found via pluginId" -f $game.Name, $game.GameId))
        return $game.GameId
    }
    elseif ($null -ne $game.Links)
    {
        # Look for Steam Store URL in links for other games
        foreach ($link in $game.Links) {
            if ($link.Url -match "https?://store.steampowered.com/app/(\d+)/?")
            {
                $__logger.Info(("Game: {0}, appId {1} found via links" -f $game.Name, $link.Url))
                return $matches[1]
            }
        }
    }

    $gameName = $game.Name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
    if ($null -ne $appList[$gameName])
    {
        $appId = $appList[$gameName].ToString()
        $__logger.Info(("Game: {0}, appId {1} found via AppList" -f $game.Name, $appId))
        return $appId
    }
    
    if ((!$appId) -and ($appListDownloaded -eq $false))
    {
        # Download Steam AppList if game was not found in local Steam AppList database and local Steam AppList database is older than 2 days
        $appListPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'AppList.json'
        $AppListLastWrite = (Get-Item $appListPath).LastWriteTime
        $timeSpan = New-Timespan -days 2
        if (((Get-date) - $AppListLastWrite) -gt $timeSpan)
        {
            Set-GlobalAppList $true
            if ($null -ne $appList[$gameName])
            {
                $appId = $appList[$gameName].ToString()
                $__logger.Info(("Game: {0}, appId {1} found via AppList" -f $game.Name, $appId))
                return $appId
            }
            return $null
        }
    }
}