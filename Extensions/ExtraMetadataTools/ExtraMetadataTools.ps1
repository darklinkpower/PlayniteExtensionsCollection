function GetGameMenuItems
{
    param(
        $menuArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  "Open extra metadata directory"
    $menuItem.FunctionName = "Invoke-DirectoryOpen"
    $menuItem.MenuSection = "Extra Metadata tools"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  "Download Steam logos for selected games"
    $menuItem2.FunctionName = "Get-SteamLogos"
    $menuItem2.MenuSection = "Extra Metadata tools|Logos"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  "Add logo from local file of selected game"
    $menuItem3.FunctionName = "Get-SteamLogosLocal"
    $menuItem3.MenuSection = "Extra Metadata tools|Logos"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem4.Description =  "Add logo from url of selected game"
    $menuItem4.FunctionName = "Get-SteamLogosUri"
    $menuItem4.MenuSection = "Extra Metadata tools|Logos"

    $menuItem5 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem5.Description =  "Convert icons to custom logos asset"
    $menuItem5.FunctionName = "Get-IconToLogoConvert"
    $menuItem5.MenuSection = "Extra Metadata tools|Logos"

    $menuItem6 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem6.Description =  "Delete logos of selected games"
    $menuItem6.FunctionName = "Remove-LogosSelectedGames"
    $menuItem6.MenuSection = "Extra Metadata tools|Logos"
    

    return $menuItem, $menuItem2, $menuItem3, $menuItem4, $menuItem5, $menuItem6
}

function OnApplicationStarted
{
    if ($PlayniteApi.ApplicationInfo.Mode -eq "Desktop")
    {
        $themesSubPath = "\Themes\Desktop\"
        $configurationFile = "config.json"
    }
    else
    {
        $themesSubPath = "\Themes\Fullscreen\"
        $configurationFile = "fullscreenConfig.json"
    }
    $playniteConfigPath = Join-Path $PlayniteApi.Paths.ConfigurationPath -ChildPath $configurationFile
    if (Test-Path $playniteConfigPath)
    {
        $playniteConfig = [System.IO.File]::ReadAllLines($playniteConfigPath) | ConvertFrom-Json
        $themeInUse = $playniteConfig.Theme
        $constantsPath = $PlayniteApi.Paths.ConfigurationPath + $themesSubPath + $themeInUse + "\Constants.xaml"
        if (!(Test-Path $constantsPath))
        {
            $resolvePathsWildcard = $PlayniteApi.Paths.ConfigurationPath + $themesSubPath + $themeInUse + "*"
            $resolvedPaths = Resolve-Path -Path $resolvePathsWildcard
            if ($resolvedPaths.Count -eq 1)
            {
                $constantsPath = $resolvedPaths[0].Path + "\Constants.xaml"
            }
        }
        if (Test-Path $constantsPath)
        {
            $configChanged = $false
            $constantsContent = [System.IO.File]::ReadAllLines($constantsPath)
            
            # Path value replacer
            $keyMatchRegex = "<sys:String x:Key=`"ExtraMetadataPath`">(.*?(?=<\/sys:String>))<\/sys:String>"
            $keyMatch = ([regex]$keyMatchRegex).Matches($constantsContent)
            if ($keyMatch.count -eq 1)
            {
                $extraMetadataOriginalValue = $keyMatch[0].Value
                $extraMetadataNewValue = "<sys:String x:Key=`"ExtraMetadataPath`">{0}</sys:String>" -f $PlayniteApi.Paths.ConfigurationPath
                if ($extraMetadataOriginalValue -ne $extraMetadataNewValue)
                {
                    $constantsContent = $constantsContent -replace [Regex]::Escape($extraMetadataOriginalValue), $extraMetadataNewValue
                    $__logger.Info("Extra Metadata Tools - Changed path from `"$extraMetadataOriginalValue`" to `"$extraMetadataNewValue`"")
                    $configChanged = $true
                }
            }

            # Bool value replacer
            $keyMatchRegex = "<sys:Boolean x:Key=`"UseAbsoluteExtraMetadataPath`">(.*?(?=<\/sys:Boolean>))<\/sys:Boolean>"
            $keyMatch = ([regex]$keyMatchRegex).Matches($constantsContent)
            if ($keyMatch.count -eq 1)
            {
                $extraMetadataOriginalValue = $keyMatch[0].Value
                $extraMetadataNewValue = "<sys:Boolean x:Key=`"UseAbsoluteExtraMetadataPath`">{0}</sys:Boolean>" -f "true"
                if ($extraMetadataOriginalValue -ne $extraMetadataNewValue)
                {
                    $constantsContent = $constantsContent -replace [Regex]::Escape($extraMetadataOriginalValue), $extraMetadataNewValue
                    $__logger.Info("Extra Metadata Tools - Changed bool string from `"$extraMetadataOriginalValue`" to `"$extraMetadataNewValue`"")
                    $configChanged = $true
                }
            }

            if ($configChanged -eq $true)
            {
                [System.IO.File]::WriteAllLines($constantsPath, $constantsContent)
                $PlayniteApi.Dialogs.ShowMessage("Extra Metadata configuration for `"$themeInUse`" updated.`nPlease restart Playnite to make changes take effect.", "Extra Metadata Tools");
            }
        }
    }
}

function Invoke-DirectoryOpen
{
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    foreach ($game in $gameDatabase) {
        $directory = Set-GameDirectory $game
        Invoke-Item $Directory
    }
    
}

function Set-GameDirectory
{
    param (
        $game
    )

    $directory = $PlayniteApi.Paths.ConfigurationPath + "\ExtraMetadata\" + "games\" + $game.Id
    if(!(Test-Path $directory))
    {
        # Store new Item in variable to fix function returning 2 times
        $newItem = New-Item -ItemType Directory -Path $directory -Force
    }
    return $directory
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
        $PlayniteApi.Dialogs.ShowMessage("Error downloading file `"$url`". Error: $errorMessage");
        return
    }
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
        $__logger.Info("Steam Trailers - Downloaded AppList")
    }
    else
    {
        exit
    }
}

function Get-SteamAppId
{
    param (
        $game
    )

    # Use GameId for Steam games
    if ([Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId) -eq "SteamLibrary")
    {
        return $game.GameId
    }
    else
    {
        # Look for Steam Store URL in links for other games
        foreach ($link in $game.Links) {
            switch -regex ($link.Url) {
                "https?://store.steampowered.com/app/(\d+)/?\w*/?" {
                return $matches[1]}
            }
        }
    }

    if (!$AppId)
    {
        # Get Steam AppList
        $appListPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'AppList.json'
        if (!(Test-Path $appListPath))
        {
            Get-SteamAppList -AppListPath $appListPath
        }

        # Try to search for AppId by searching in local Steam AppList database
        [object]$AppList = [System.IO.File]::ReadAllLines($appListPath) | ConvertFrom-Json
        $gameName = $game.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        foreach ($steamApp in $AppList) {
            if ($steamApp.name -eq $gameName) 
            {
                return $steamApp.appid
            }
        }
        if (!$AppId)
        {
            # Download Steam AppList if game was not found in local Steam AppList database and local Steam AppList database is older than 2 days
            $AppListLastWrite = (Get-Item $appListPath).LastWriteTime
            $TimeSpan = New-Timespan -days 2
            if (((Get-date) - $AppListLastWrite) -gt $TimeSpan)
            {
                Get-SteamAppList -AppListPath $appListPath
                [object]$AppList = [System.IO.File]::ReadAllLines($appListPath) | ConvertFrom-Json
                foreach ($SteamApp in $AppList) {
                    if ($SteamApp.name -eq $Gamename) 
                    {
                        return $SteamApp.appid
                    }
                }
            }
        }
    }
}

function Get-SteamLogos
{
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Platform.Name -eq "PC"}
    $logoUriTemplate = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/logo.png"
    $counter = 0
    foreach ($game in $gameDatabase) {
        $extraMetadataDirectory = Set-GameDirectory $game
        $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
        if (Test-Path $logoPath)
        {
            continue
        }

        $steamAppId = Get-SteamAppId $game
        if ($steamAppId)
        {
            $logoUri = $logoUriTemplate -f $steamAppId
            
            try {
                $webClient = New-Object System.Net.WebClient
                $webClient.Encoding = [System.Text.Encoding]::UTF8
                $webClient.DownloadFile($logoUri, $logoPath)
                $webClient.Dispose()
                Start-Sleep -Seconds 1
                $counter++
            } catch {
                $errorMessage = $_.Exception.Message
                $__logger.Info("Error downloading file `"$url`". Error: $errorMessage")
                $countErrors++
            }
        }
    }
    $results = "Downloaded logo of $counter games."
    if ($countErrors -gt 0)
    {
        $results += ". There were $countErrors errors, view Playnite log for details."
    }
    $PlayniteApi.Dialogs.ShowMessage($results, "Extra Metadata tools");
}

function Get-SteamLogosLocal
{
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -gt 1)
    {
        $PlayniteApi.Dialogs.ShowMessage("More than one game is selected, please select only one game.", "Extra Metadata tools");
        return
    }

    foreach ($game in $gameDatabase) {
        $extraMetadataDirectory = Set-GameDirectory $game
        $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
        $logoPathLocal = $PlayniteApi.Dialogs.SelectFile("logo|*.png")
        Copy-Item $logoPathLocal -Destination $logoPath -Force
        $PlayniteApi.Dialogs.ShowMessage("Added logo file to `"$($game.name)`"", "Extra Metadata tools");
    }
}

function Get-SteamLogosUri
{
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -gt 1)
    {
        $PlayniteApi.Dialogs.ShowMessage("More than one game is selected, please select only one game.", "Extra Metadata tools");
        return
    }

    foreach ($game in $gameDatabase) {
        $extraMetadataDirectory = Set-GameDirectory $game
        $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
        $logoUriInput = $PlayniteApi.Dialogs.SelectString("Enter logo Url:", "Extra Metadata tools", "");
        
        # Check if input was entered
        if ($logoUriInput.result -eq "True")
        {
            $logoUri = $logoUriInput.Selectedstring
            try {
                $webClient = New-Object System.Net.WebClient
                $webClient.DownloadFile($logoUri, $logoPath)
                $webClient.Dispose()
                $PlayniteApi.Dialogs.ShowMessage("Added logo file to `"$($game.name)`"", "Extra Metadata tools");
            } catch {
                $errorMessage = $_.Exception.Message
                $__logger.Info("Error downloading file `"$url`". Error: $errorMessage")
                $PlayniteApi.Dialogs.ShowMessage("Error downloading file `"$url`". Error: $errorMessage");
            }
        }
    }
}

function Get-IconToLogoConvert
{
    $convertChoice = $PlayniteApi.Dialogs.ShowMessage("This function will convert the icons of the selected games to logos for use in compatible themes.
    `nThis is intended to use in case you have used a theme that has used the icons asset as a replacement for logos.
    `nFor safety it will only process icons that have the `".png`" file extension.
    `nChanges are not reversible.`n`nDo you wish to continue?", "Extra Metadata tools", 4)
    if ($convertChoice -ne "Yes")
    {
        return
    }

    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Icon}

    foreach ($game in $gameDatabase) {
        $iconPath = $PlayniteApi.Database.GetFullFilePath($game.Icon)
        $iconExtension = [System.IO.Path]::GetExtension($iconPath)
        if ($iconExtension -eq ".png")
        {
            if (Test-Path $iconPath)
            {
                $extraMetadataDirectory = Set-GameDirectory $game
                $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
                Move-Item $iconPath -Destination $logoPath -Force
                $game.Icon = $null
                $PlayniteApi.Database.Games.Update($game)
            }
        }
    }
}

function Remove-LogosSelectedGames
{
    param (
        $game
    )

    $convertChoice = $PlayniteApi.Dialogs.ShowMessage("This function will delete the logos for the selected games.
    `nChanges are not reversible.`n`nDo you wish to continue?", "Extra Metadata tools", 4)
    if ($convertChoice -ne "Yes")
    {
        return
    }

    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    $removedLogos = 0
    foreach ($game in $gameDatabase) {
        $extraMetadataDirectory = Set-GameDirectory $game
        $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
        if (Test-Path $logoPath)
        {
            Remove-Item $logoPath
            $removedLogos++
        }
    }
    $PlayniteApi.Dialogs.ShowMessage("Removed logos of $removedLogos games.", "Extra Metadata tools")
}