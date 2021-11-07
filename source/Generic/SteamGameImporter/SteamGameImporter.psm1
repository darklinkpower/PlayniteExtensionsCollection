function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Game_Importer_MenuItemAddGamesIdUrlDescription")
    $menuItem1.FunctionName = "SteamGameImporter"
    $menuItem1.MenuSection = "@Steam Game Importer"
    
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Game_Importer_MenuItemAddGamesDepressurizerDescription")
    $menuItem2.FunctionName = "DepressurizerProfileImporter"
    $menuItem2.MenuSection = "@Steam Game Importer"

    return $menuItem1, $menuItem2
}

function DepressurizerProfileImporter
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Ger Depressurizer xml data
    $DepressurizerProfilePath = $PlayniteApi.Dialogs.SelectFile("Profiles|*.Profile")
    if ($DepressurizerProfilePath)
    {
        [xml]$DepressurizerXml = [System.IO.File]::ReadAllLines($DepressurizerProfilePath)
    }
    else
    {
        return
    }
    
    $steamPluginId = [guid]::Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab")
    $source = $PlayniteApi.Database.Sources.Add("Steam")
    $platform = $PlayniteApi.Database.Platforms.Add("PC (Windows)")
    $platformsList = [System.Collections.Generic.List[guid]]($platform.Id)
    $steamGamesInLibrary = Get-SteamGamesInLibrary

    # Create cache of Steam games in Database
    $steamGamesInLibrary = Get-SteamGamesInLibrary

    $addedGamesCount = 0

    foreach ($game in $DepressurizerXml.profile.games.game) {
        
        if ($null -ne $steamGamesInLibrary[$game.id])
        {
            continue
        }
        
        $exclusionItem = $PlayniteApi.Database.ImportExclusions | Where-Object {($_.LibraryId -eq $steamPluginId) -and ($_.GameId -eq $gameid)}
        if ($null -ne $exclusionItem)
        {
            $__logger.Info("Steam game with id $($game.id) is in exclusion list and will be skipped from Depressurizer import")
            continue
        }
        
        # Non game steam apps have an id inferior to 0 in Depressurizer
        if ([int]$game.id -lt 0)
        {
            continue
        }

        # Set game properties and save to database
        $newGame = New-Object "Playnite.SDK.Models.Game"
        $newGame.Name = $game.name
        $newGame.GameId = $game.id
        $newGame.SourceId = $Source.Id
        $newGame.PlatformIds = $platformsList
        $newGame.PluginId = $steamPluginId
        $PlayniteApi.Database.Games.Add($newGame)
        $addedGamesCount++
    }

    # Show dialogue with results
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Game_Importer_ResultsMessage") -f $addedGamesCount), "Steam Game Importer")
}

function Get-SteamGamesInLibrary
{
    $steamGamesInLibrary = @{}
    $steamPluginId = [guid]::Parse("cb91dfc9-b977-43bf-8e70-55f46e410fab")
    foreach ($game in $PlayniteApi.Database.Games) {
        if ($game.PluginId -ne $steamPluginId)
        {
            continue
        }

        # Use a try block for safety
        try {
            $steamGamesInLibrary.add($game.GameId, $game.Name)
        } catch {}
    }

    return $steamGamesInLibrary
}

function SteamGameImporter
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Input window for Steam Store URL or Steam AppId
    $UserInput = $PlayniteApi.Dialogs.SelectString([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Game_Importer_RequestInputSteamIdUrlMessage"), "Steam Game Importer", "")
    if (!$UserInput.SelectedString)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Game_Importer_InputNoValidAppIdsMessage"), "Steam Game Importer")
        return
    }
    
    $steamPluginId = [Playnite.SDK.BuiltinExtensions]::GetIdFromExtension([Playnite.SDK.BuiltinExtension]::SteamLibrary)
    $source = $PlayniteApi.Database.Sources.Add("Steam")
    $platform = $PlayniteApi.Database.Platforms.Add("PC (Windows)")
    $platformsList = [System.Collections.Generic.List[guid]]($platform.Id)
    $steamGamesInLibrary = Get-SteamGamesInLibrary

    [System.Collections.Generic.List[string]]$AppIds = @()
    [string]$TextInput = $UserInput.SelectedString		
    $addedGamesCount = 0

    # Verify if input was Steam Store URL
    $UrlRegex = "https?:\/\/store.steampowered.com\/app\/(\d+)"
    if ($TextInput -match $UrlRegex)
    {
        $UrlMatches = $TextInput | Select-String $UrlRegex -AllMatches | Select-Object -Unique
        if ($UrlMatches.Matches.count -ge 1)
        {
            foreach ($UrlMatch in $UrlMatches.Matches) {
                $AppIds.Add($UrlMatch.Groups[1].value)
            }
        }
    }
    # Verify if input was Steam Store AppId
    else
    {
        $TextInput = $TextInput -replace ' ',''
        $TextSplit = $TextInput.Split(',')
        foreach ($SplittedText in $TextSplit) {
            if ($SplittedText -Match "^\d+$")
            {
                $AppIds.Add($SplittedText)
            }
        }
    }
    # Verify if AppId was obtained
    if ($AppIds.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Game_Importer_InputNoValidAppIdsMessage"), "Steam Game Importer")
        return
    }

    $webClient = New-Object System.Net.WebClient
    $webClient.Encoding = [System.Text.Encoding]::UTF8
    foreach ($AppId in $AppIds) {
        # Skip game if it already exists in Planite game Database
        if ($null -ne $steamGamesInLibrary[$AppId])
        {
            continue
        }

        # Verify is obtained AppId is valid and get game name with SteamAPI
        try {
            $steamAPI = 'https://store.steampowered.com/api/appdetails?appids={0}' -f $AppId
            $downloadedString = $webClient.DownloadString($steamAPI)
            $json = $downloadedString | ConvertFrom-Json
            
            # Sleep time to prevent error 429
            Start-Sleep -Milliseconds 1200
            if ($json.$AppId.Success -eq "true")
            {
                $GameName = $json.$AppId.data.name
            }
            else
            {
                if (!$AddUnknownChoice)
                {
                    $AddUnknownChoice = $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Game_Importer_InvalidSteamIdWarningMessage") -f $AppId), "Steam Game Importer", 4)
                }
                if ($AddUnknownChoice -ne "Yes")
                {
                    continue
                }
                $GameName = "Unknown Steam Game"
            }
        } catch {
            $errorMessage = $_.Exception.Message
            $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Game_Importer_ResultsMessage") -f $AppId, $errorMessage), "Steam Game Importer")
            break
        }
        
        # Set game properties and save to database
        $NewGame = New-Object "Playnite.SDK.Models.Game"
        $NewGame.Name = $GameName
        $NewGame.GameId = $AppId
        $NewGame.SourceId = $Source.Id
        $NewGame.PlatformIds = $platformsList
        $NewGame.PluginId = $steamPluginId
        $PlayniteApi.Database.Games.Add($NewGame)
        $addedGamesCount++
        
        # Trigger download Metadata not available yet via SDK. https://github.com/JosefNemec/Playnite/issues/1870
    }
    
    # Show dialogue with results
    $webClient.Dispose()
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSteam_Game_Importer_ResultsMessage") -f $addedGamesCount), "Steam Game Importer")
}