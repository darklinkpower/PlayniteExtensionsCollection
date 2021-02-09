function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemAddGamesIdUrlDescription")
    $menuItem1.FunctionName = "SteamGameImporter"
    $menuItem1.MenuSection = "@Steam Game Importer"
    
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemAddGamesDepressurizerDescription")
    $menuItem2.FunctionName = "DepressurizerProfileImporter"
    $menuItem2.MenuSection = "@Steam Game Importer"

    return $menuItem1, $menuItem2
}

function DepressurizerProfileImporter
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Set Source
    $SourceName = "Steam"
    $Source = $PlayniteApi.Database.Sources.Add($SourceName)
    
    # Set Platform
    $PlatformName = "PC"
    $Platform = $PlayniteApi.Database.Platforms.Add($PlatformName)

    # Ger Depressurizer xml data
    $DepressurizerProfilePath = $PlayniteApi.Dialogs.SelectFile("Profiles|*.Profile")
    if ($DepressurizerProfilePath)
    {
        [xml]$DepressurizerXml = [System.IO.File]::ReadAllLines($DepressurizerProfilePath)
    }
    else
    {
        exit
    }
    
    # Create cache of Steam games in Database
    $SteamGames = $PlayniteApi.Database.Games | Where-Object {$_.source.name -eq "Steam"}
    [System.Collections.Generic.List[string]]$SteamGamesInDatabase = @()
    foreach ($game in $SteamGames) {
        $SteamGamesInDatabase.Add($($game.GameId))
    }

    $AddedGamesCount = 0

    foreach ($Game in $DepressurizerXml.profile.games.game) {
        
        # Convert Game.Id to int value type for value comparison
        [int]$GameId = $Game.id

        # Skip game if it already exists in Planite game Database
        if (($SteamGamesInDatabase -contains $Game.id) -or ($GameId -lt 0))
        {
            continue
        }
        else
        {
            # Set game properties and save to database
            $NewGame = New-Object "Playnite.SDK.Models.Game"
            $NewGame.Name = $Game.name
            $NewGame.GameId = $Game.id
            $NewGame.SourceId = $Source.Id
            $NewGame.PlatformId = $Platform.Id
            $NewGame.PluginId = "CB91DFC9-B977-43BF-8E70-55F46E410FAB"
            $PlayniteApi.Database.Games.Add($NewGame)
            $AddedGamesCount++
        }
    }
    # Show dialogue with results
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCResultsMessage") -f $AddedGamesCount), "Steam Game Importer")
}

function SteamGameImporter
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Set Source
    $SourceName = "Steam"
    $Source = $PlayniteApi.Database.Sources.Add($SourceName)
    
    # Set Platform
    $PlatformName = "PC"
    $Platform = $PlayniteApi.Database.Platforms.Add($PlatformName)

    # Create cache of Steam games in Database
    $SteamGames = $PlayniteApi.Database.Games | Where-Object {$_.source.name -eq "Steam"}
    [System.Collections.Generic.List[string]]$SteamGamesInDatabase = @()
    foreach ($game in $SteamGames) {
        $SteamGamesInDatabase.Add($($game.GameId))
    }

    # Set Regexes
    $UrlRegex = "https?:\/\/store.steampowered.com\/app\/(\d+)"
    
    # Input window for Steam Store URL or Steam AppId
    $UserInput = $PlayniteApi.Dialogs.SelectString([Playnite.SDK.ResourceProvider]::GetString("LOCRequestInputSteamIdUrlMessage"), "Steam Game Importer", "")
    if ($UserInput.SelectedString)
    {
        # Create AppIds Collection
        [System.Collections.Generic.List[string]]$AppIds = @()
        [string]$TextInput = $UserInput.SelectedString		
        $AddedGamesCount = 0

        # Verify if input was Steam Store URL
        if ($TextInput -match $UrlRegex)
        {
            $UrlMatches = $TextInput | Select-String $UrlRegex -AllMatches
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
        if ($AppIds.count -ge 1)
        {
            foreach ($AppId in $AppIds) {
                
                # Skip game if it already exists in Planite game Database
                if ($SteamGamesInDatabase -contains $AppId)
                {
                    continue
                }

                # Verify is obtained AppId is valid and get game name with SteamAPI
                try {
                    $steamAPI = 'https://store.steampowered.com/api/appdetails?appids={0}' -f $AppId
                    $webClient = New-Object System.Net.WebClient
                    $webClient.Encoding = [System.Text.Encoding]::UTF8
                    $downloadedString = $webClient.DownloadString($steamAPI)
                    $webClient.Dispose()
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
                            $AddUnknownChoice = $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCInvalidSteamIdWarningMessage") -f $AppId), "Steam Game Importer", 4)
                        }
                        if ($AddUnknownChoice -ne "Yes")
                        {
                            continue
                        }
                        $GameName = "Unknown Steam Game"
                    }
                } catch {
                    $errorMessage = $_.Exception.Message
                    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCResultsMessage") -f $AppId, $errorMessage), "Steam Game Importer")
                    break
                }
                
                # Set game properties and save to database
                $NewGame = New-Object "Playnite.SDK.Models.Game"
                $NewGame.Name = $GameName
                $NewGame.GameId = $AppId
                $NewGame.SourceId = $Source.Id
                $NewGame.PlatformId = $Platform.Id
                $NewGame.PluginId = "CB91DFC9-B977-43BF-8E70-55F46E410FAB"
                $PlayniteApi.Database.Games.Add($NewGame)
                $AddedGamesCount++
                
                # Trigger download Metadata not available yet via SDK. https://github.com/JosefNemec/Playnite/issues/1870
            }
        }	
        else
        {
            $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCInputNoValidAppIdsMessage"), "Steam Game Importer")
        }

        # Show dialogue with results
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCResultsMessage") -f $AddedGamesCount), "Steam Game Importer")
        
    }
}