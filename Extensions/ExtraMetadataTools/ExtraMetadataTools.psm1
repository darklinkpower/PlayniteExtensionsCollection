function GetGameMenuItems
{
    param(
        $menuArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemInvokeExtraDirectoryDescription")
    $menuItem.FunctionName = "Invoke-DirectoryOpen"
    $menuItem.MenuSection = "Extra Metadata tools"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemGetSteamLogosDescription")
    $menuItem2.FunctionName = "Get-SteamLogos"
    $menuItem2.MenuSection = "Extra Metadata tools|Logos"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemSelectLocalLogoDescription")
    $menuItem3.FunctionName = "Get-SteamLogosLocal"
    $menuItem3.MenuSection = "Extra Metadata tools|Logos"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem4.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemGetUrlLogoDescription")
    $menuItem4.FunctionName = "Get-SteamLogosUri"
    $menuItem4.MenuSection = "Extra Metadata tools|Logos"

    $menuItem5 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem5.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemConvertIconsToLogosDescription")
    $menuItem5.FunctionName = "Get-IconToLogoConvert"
    $menuItem5.MenuSection = "Extra Metadata tools|Logos"

    $menuItem6 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem6.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemRemoveLogosSelectedGamesDescription")
    $menuItem6.FunctionName = "Remove-LogosSelectedGames"
    $menuItem6.MenuSection = "Extra Metadata tools|Logos"

    $menuItem7 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem7.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemInvokeThemesDirectoryDescription")
    $menuItem7.FunctionName = "Invoke-ThemesDirectoryRootOpen"
    $menuItem7.MenuSection = "Extra Metadata tools|Themes"

    $menuItem8 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem8.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemSetProfilePictureDescription")
    $menuItem8.FunctionName = "Set-ProfilePicture"
    $menuItem8.MenuSection = "Extra Metadata tools|Themes"

    $menuItem9 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem9.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemSetBackgroundMusicDescription")
    $menuItem9.FunctionName = "Set-BackgroundMusic"
    $menuItem9.MenuSection = "Extra Metadata tools|Themes"

    $menuItem10 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem10.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemSetBackgroundVideoDescription")
    $menuItem10.FunctionName = "Set-BackgroundVideo"
    $menuItem10.MenuSection = "Extra Metadata tools|Themes"
    
    return $menuItem, $menuItem2, $menuItem3, $menuItem4, $menuItem5, $menuItem6, $menuItem7, $menuItem8, $menuItem9, $menuItem10
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
        $manifestPath = $PlayniteApi.Paths.ConfigurationPath + $themesSubPath + $themeInUse + "\theme.yaml"
        if (!(Test-Path $constantsPath))
        {
            $resolvePathsWildcard = $PlayniteApi.Paths.ConfigurationPath + $themesSubPath + $themeInUse + "*"
            $resolvedPaths = Resolve-Path -Path $resolvePathsWildcard
            if ($resolvedPaths.Count -eq 1)
            {
                $constantsPath = $resolvedPaths[0].Path + "\Constants.xaml"
                $manifestPath = $resolvedPaths[0].Path + "\theme.yaml"
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
                if (Test-Path $manifestPath)
                {
                    $themeManifest = [System.IO.File]::ReadAllLines($manifestPath)
                    $regex = "^Name: ([^\n]+)"
                    $nameProperty = $themeManifest | Select-String -Pattern $regex
                    $themeInUse = $nameProperty -replace "Name: ", ""
                }
                $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCThemeConstantsUpdatedMessage") -f $themeInUse), "Extra Metadata Tools")
            }
        }
    }
}

function Invoke-DirectoryOpen
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
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

function Invoke-ThemesDirectoryRootOpen
{
    $directory = $PlayniteApi.Paths.ConfigurationPath + "\ExtraMetadata\Themes"
    if(!(Test-Path $directory))
    {
        # Store new Item in variable to fix function returning 2 times
        $newItem = New-Item -ItemType Directory -Path $directory -Force
    }
    Invoke-Item $directory
}

function Set-FullscreenThemesDirectory
{
    $directory = $PlayniteApi.Paths.ConfigurationPath + "\ExtraMetadata\Themes\Fullscreen\"
    if(!(Test-Path $directory))
    {
        # Store new Item in variable to fix function returning 2 times
        $newItem = New-Item -ItemType Directory -Path $directory -Force
    }
    return $directory
}

function Set-DesktopThemesDirectory
{
    $directory = $PlayniteApi.Paths.ConfigurationPath + "\ExtraMetadata\Themes\Desktop\"
    if(!(Test-Path $directory))
    {
        # Store new Item in variable to fix function returning 2 times
        $newItem = New-Item -ItemType Directory -Path $directory -Force
    }
    return $directory
}

function Set-CommonThemesDirectory
{
    $directory = $PlayniteApi.Paths.ConfigurationPath + "\ExtraMetadata\Themes\Common\"
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
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCGenericFileDownloadError") -f $url, $errorMessage))
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
    param(
        $scriptGameMenuItemActionArgs
    )
    
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
            }
        }
    }
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCGetSteamLogosResultsMessage") -f $counter), "Extra Metadata tools");
}

function Get-SteamLogosLocal
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -gt 1)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCMoreThanOneGameSelectedMessage"), "Extra Metadata tools")
        return
    }

    foreach ($game in $gameDatabase) {
        $extraMetadataDirectory = Set-GameDirectory $game
        $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
        $logoPathLocal = $PlayniteApi.Dialogs.SelectFile("logo|*.png")
        if ([string]::IsNullOrEmpty($logoPathLocal))
        {
            return
        }
        Copy-Item $logoPathLocal -Destination $logoPath -Force
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSelectLogoResultsMessage") -f $game.Name), "Extra Metadata tools");
    }
}

function Get-SteamLogosUri
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -gt 1)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCMoreThanOneGameSelectedMessage"), "Extra Metadata tools")
        return
    }

    foreach ($game in $gameDatabase) {
        $extraMetadataDirectory = Set-GameDirectory $game
        $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
        $logoUriInput = $PlayniteApi.Dialogs.SelectString([Playnite.SDK.ResourceProvider]::GetString("LOCEnterUrlMessage"), "Extra Metadata tools", "")
        
        # Check if input was entered
        if ($logoUriInput.result -eq "True")
        {
            $logoUri = $logoUriInput.Selectedstring
            try {
                $webClient = New-Object System.Net.WebClient
                $webClient.DownloadFile($logoUri, $logoPath)
                $webClient.Dispose()
                $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCSelectLogoResultsMessage") -f $game.Name), "Extra Metadata tools")
            } catch {
                $errorMessage = $_.Exception.Message
                $__logger.Info("Error downloading file `"$url`". Error: $errorMessage")
                $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCGenericFileDownloadError") -f $url, $errorMessage), "Extra Metadata tools");
            }
        }
    }
}

function Get-IconToLogoConvert
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    $convertChoice = $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCConvertIconsChoiceMessage"), "Extra Metadata tools", 4)
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
    param(
        $scriptGameMenuItemActionArgs
    )
    
    $convertChoice = $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCRemoveLogosChoiceMessage"), "Extra Metadata tools", 4)
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
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCRemoveLogosResultsMessage") -f $removedLogos), "Extra Metadata tools")
}

function Set-ProfilePicture
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    $imageFile = $PlayniteApi.Dialogs.SelectImagefile()
    if ([string]::IsNullOrEmpty($imageFile))
    {
        return
    }
    $fileDestination = Set-CommonThemesDirectory | Join-Path -ChildPath "ProfilePicture.png"

    if ([System.IO.Path]::GetExtension($imageFile) -eq ".png")
    {
        Copy-Item $imageFile $fileDestination -Force
    }
    else
    {
        try {
            Add-Type -AssemblyName system.drawing
            $imageFormat = “System.Drawing.Imaging.ImageFormat” -as [type]
            $image = [drawing.image]::FromFile($imageFile)
            $image.Save($fileDestination, $imageFormat::png)
        } catch {
            $errorMessage = $_.Exception.Message
            $__logger.Info("Extra Metadata Tools - Error converting image file to png. Error: `"$errorMessage`"")
            $PlayniteApi.Dialogs.ShowErrorMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCImageConversionErrorMessage") -f $errorMessage), "Extra Metadata Tools")
            return
        }
    }
    $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCSetProfilePictureResultsMessage"), "Extra Metadata tools")
}

function Set-BackgroundMusic
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    $file = $PlayniteApi.Dialogs.SelectFile("mp3|*.mp3")
    if ([string]::IsNullOrEmpty($file))
    {
        return
    }
    $fileDestination = Set-FullscreenThemesDirectory | Join-Path -ChildPath "BackgroundMusic.mp3"
    Copy-Item $file $fileDestination -Force
    $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCSetBackgroundMusicResultsMessage"), "Extra Metadata tools")
}

function Set-BackgroundVideo
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    $file = $PlayniteApi.Dialogs.SelectFile("mp4|*.mp4")
    if ([string]::IsNullOrEmpty($file))
    {
        return
    }
    $fileDestination = Set-FullscreenThemesDirectory | Join-Path -ChildPath "BackgroundVideo.mp4"
    Copy-Item $file $fileDestination -Force
    $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCSetBackgroundVideoResultsMessage"), "Extra Metadata tools")
}

function Set-SgdbApiKey
{
    $sgdbApiKeyPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'sgdbApiKey.json'
    Start-Process "https://www.steamgriddb.com/profile/preferences"
    $userInput = $PlayniteApi.Dialogs.SelectString("Enter a valid SGDB API Key:", "Extra Metadata tools", "");
    if ($userInput.Result -eq $true)
    {
        @{'ApiKey'=$userInput.SelectedString} | ConvertTo-Json | Out-File $sgdbApiKeyPath
        return $userInput.SelectedString
    }
}

function Get-SgdbApiKey
{
    $sgdbApiKeyPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'sgdbApiKey.json'
    if (Test-Path $sgdbApiKeyPath)
    {
        $sgdbApiKey = ([System.IO.File]::ReadAllLines($sgdbApiKeyPath) | ConvertFrom-Json).ApiKey
    }
    else
    {
        $sgdbApiKey = Set-SgdbApiKey
    }
    if ([string]::IsNullOrEmpty($sgdbApiKey))
    {
        if (Test-Path $sgdbApiKeyPath)
        {
            Remove-Item $sgdbApiKeyPath -Force
        }
        return $null
    }
    else
    {
        return $sgdbApiKey
    }
}
function Get-SgdbLogo
{
    $sgdbApiKey = Get-SgdbApiKey
    if ([string]::IsNullOrEmpty($sgdbApiKey))
    {
        $PlayniteApi.Dialogs.ShowMessage("Couldn't get SGDB API Key. Please configure it before continuing.", "Extra Metadata tools")
        return
    }
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.Platform.Name -eq "PC"}
    $downloadedLogos = 0
    foreach ($game in $gameDatabase) {
        $extraMetadataDirectory = Set-GameDirectory $game
        $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
        if (Test-Path $logoPath)
        {
            continue
        }

        switch ([Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId)) {
            "SteamLibrary" { $platformEnum = "steam" ; break}
            "OriginLibrary" { $platformEnum = "origin" ; break}
            "EpicLibrary" { $platformEnum = "egs" ; break}
            "BattleNetLibrary" { $platformEnum = "bnet" ; break}
            "UplayLibrary" { $platformEnum = "uplay" ; break }
            Default { $platformEnum = $null ; break }
        }

        if ($null -eq $platformEnum)
        {
            $steamAppId = Get-SteamAppId $game
            if ($null -ne $steamAppId)
            {
                $requestUri = "https://www.steamgriddb.com/api/v2/logos/steam/{0}" -f $steamAppId
            }
            else
            {
                continue
            }
        }
        else
        {
            $requestUri = "https://www.steamgriddb.com/api/v2/logos/{0}/{1}" -f $platformEnum, $game.GameId
        }
        
        try {
            $headers = @{'Authorization'="Bearer $sgdbApiKey"}
            $sgdbRequest = Invoke-WebRequest -Uri $requestUri -Headers $headers | ConvertFrom-Json
        } catch {
            $errorMessage = $_.Exception.Message
            $__logger.Info("Error in SGDB API Request. Error: $errorMessage")
            $PlayniteApi.Dialogs.ShowErrorMessage("Error in SteamGridDB API Request. Verify that the API key is correct. Error: $errorMessage")
            break
        }
        $headers = @{'Authorization'="Bearer $sgdbApiKey"}
        $sgdbRequest = Invoke-WebRequest -Uri $requestUri -Headers $headers | ConvertFrom-Json

        if ($sgdbRequest.data.Count -gt 0)
        {
            if ([string]::IsNullOrEmpty($sgdbRequest.data[0].url))
            {
                continue
            }
            else
            {
                try {
                    $webClient = New-Object System.Net.WebClient
                    $webClient.Encoding = [System.Text.Encoding]::UTF8
                    $webClient.DownloadFile($sgdbRequest.data[0].url, $logoPath)
                    $webClient.Dispose()
                    Start-Sleep -Seconds 1
                    $downloadedLogos++
                } catch {
                    $errorMessage = $_.Exception.Message
                    $__logger.Info("Error downloading file `"$url`". Error: $errorMessage")
                    $PlayniteApi.Dialogs.ShowErrorMessage("Error downloading file from SteamGridDB. Error: $errorMessage")
                    break
                }
            }
        }
    }
    $PlayniteApi.Dialogs.ShowMessage("Downloaded $downloadedLogos logos from SteamGridDB.", "Extra Metadata tools")
}

function Add-TagMissingLogo
{
    $tag = $PlayniteApi.Database.Tags.Add("Logo missing")
    $missingLogos = 0
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    foreach ($game in $gameDatabase) {
        $extraMetadataDirectory = Set-GameDirectory $game
        $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
        if (!(Test-Path $logoPath))
        {
            if ($game.tagIds -notcontains $tag.Id)
            {
                if ($game.tagIds)
                {
                    $game.tagIds += $tag.Id
                }
                else
                {
                    # Fix in case game has null tagIds
                    $game.tagIds = $tag.Id
                }
                $PlayniteApi.Database.Games.Update($game)
                $missingLogos++
            }
        }
        elseif ($game.tagIds -contains $tag.Id)
        {
            $game.tagIds.Remove($tag.Id)
            $PlayniteApi.Database.Games.Update($game)
        }
    }
    $PlayniteApi.Dialogs.ShowMessage("Done.`nMissing logo in $missingLogos game(s).", "Extra Metadata tools")
}