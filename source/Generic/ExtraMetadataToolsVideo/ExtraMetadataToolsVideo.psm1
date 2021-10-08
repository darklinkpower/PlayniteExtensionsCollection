function GetGameMenuItems
{
    param(
        $scriptGameMenuItemActionArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemGet-SteamVideoSdDescription")
    $menuItem.FunctionName = "Get-SteamVideoSd"
    $menuItem.MenuSection = "Extra Metadata tools|Video|Trailers"
   
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemGet-SteamVideoHdDescription")
    $menuItem2.FunctionName = "Get-SteamVideoHd"
    $menuItem2.MenuSection = "Extra Metadata tools|Video|Trailers"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemGet-SteamVideoMicroDescription")
    $menuItem3.FunctionName = "Get-SteamVideoMicro"
    $menuItem3.MenuSection = "Extra Metadata tools|Video|Microtrailers"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem4.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemSet-VideoManuallyTrailerDescription")
    $menuItem4.FunctionName = "Set-VideoManuallyTrailer"
    $menuItem4.MenuSection = "Extra Metadata tools|Video|Trailers"

    $menuItem5 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem5.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemSet-VideoManuallyMicroTrailerDescription")
    $menuItem5.FunctionName = "Set-VideoManuallyMicroTrailer"
    $menuItem5.MenuSection = "Extra Metadata tools|Video|Microtrailers"
    
    $menuItem6 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem6.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemGet-VideoMicrotrailerFromTrailerDescription")
    $menuItem6.FunctionName = "Get-VideoMicrotrailerFromTrailer"
    $menuItem6.MenuSection = "Extra Metadata tools|Video|Microtrailers"
    
    $menuItem7 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem7.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemRemove-VideoTrailerDescription")
    $menuItem7.FunctionName = "Remove-VideoTrailer"
    $menuItem7.MenuSection = "Extra Metadata tools|Video|Trailers"

    $menuItem8 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem8.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemRemove-VideoMicrotrailerDescription")
    $menuItem8.FunctionName = "Remove-VideoMicrotrailer"
    $menuItem8.MenuSection = "Extra Metadata tools|Video|Microtrailers"

    $menuItem9 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem9.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemSet-YouTubeVideoDescription")
    $menuItem9.FunctionName = "Set-YouTubeVideo"
    $menuItem9.MenuSection = "Extra Metadata tools|Video|Trailers"
	
    $menuItem10 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem10.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemInvoke-YoutubeSearchWindowDescription")
    $menuItem10.FunctionName = "Invoke-YoutubeSearchWindow"
    $menuItem10.MenuSection = "Extra Metadata tools|Video|Trailers"

    $menuItem11 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem11.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemUpdate-AssetsStatusGameDatabaseDescription")
    $menuItem11.FunctionName = "Update-AssetsStatusGameDatabase"
    $menuItem11.MenuSection = "Extra Metadata tools"

    $menuItem12 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem12.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemSet-YouTubeVideoId")
    $menuItem12.FunctionName = "Set-YouTubeVideoID"
    $menuItem12.MenuSection = "Extra Metadata tools|Video|Trailers"
    
    return $menuItem, $menuItem2, $menuItem3, $menuItem9, $menuItem10, $menuItem12, $menuItem4, $menuItem5, $menuItem6, $menuItem7, $menuItem8, $menuItem11
}

function Get-MandatorySettingsList
{
    $mandatorySettingsList = @{
        ffmpegPath = $null
        ffProbePath = $null
        youtubedlPath = $null
    }

    return $mandatorySettingsList
}

function Get-OptionalSettingsList
{
    $optionalSettingsList = @{
        youtubedlWindowStyle = "Minimized"
    }

    return $optionalSettingsList
}

function Get-Settings
{
    $mandatorySettingsList = Get-MandatorySettingsList
    $optionalSettingsList = Get-OptionalSettingsList
    $settingsObject = [PSCustomObject]@{}
    
    foreach ($setting in $mandatorySettingsList.GetEnumerator()) {
        $settingsObject | Add-Member -NotePropertyName $setting.Name -NotePropertyValue $setting.Value
    }
    
    foreach ($setting in $optionalSettingsList.GetEnumerator()) {
        $settingsObject | Add-Member -NotePropertyName $setting.Name -NotePropertyValue $setting.Value
    }

    $settingsStoragePath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'settings.json'
    if (Test-Path $settingsStoragePath)
    {
        $savedSettings = [System.IO.File]::ReadAllLines($settingsStoragePath) | ConvertFrom-Json
        foreach ($setting in $mandatorySettingsList.GetEnumerator()) {
            if ($savedSettings.($setting.Name))
            {
                $settingsObject.($setting.Name) = $savedSettings.($setting.Name)
            }
        }
        foreach ($setting in $optionalSettingsList.GetEnumerator()) {
            if ($savedSettings.($setting.Name))
            {
                $settingsObject.($setting.Name) = $savedSettings.($setting.Name)
            }
        }
    }

    return $settingsObject
}

function Save-Settings
{
    param (
        $settingsObject
    )
    
    $settingsStoragePath = Join-Path -Path $CurrentExtensionDataPath -ChildPath 'settings.json'
    $settingsJson = $settingsObject | ConvertTo-Json
    [System.IO.File]::WriteAllLines($settingsStoragePath, $settingsJson)
}

function Set-MandatorySettings
{
    $settings = Get-Settings

    # Setting: ffmpegPath
    if (![string]::IsNullOrEmpty($settings.ffmpegPath))
    {
        if (!(Test-Path $settings.ffmpegPath))
        {
            $__logger.Info(("ffmpeg executable not found in {0} and saved path was deleted." -f $ffmpegPath))
            $settings.ffmpegPath = $null
        }
    }

    if ($null -eq $settings.ffmpegPath)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_SelectFfmpegExecutableMessage"), "Extra Metadata Tools")
        $ffmpegPath = $PlayniteApi.Dialogs.SelectFile("ffmpeg executable|ffmpeg.exe")
        if ($ffmpegPath)
        {
            $settings.ffmpegPath = $ffmpegPath
            $__logger.Info(("Saved ffmpeg path: {0}" -f $settings.ffmpegPath))
        }
    }

    # Setting: ffprobePath
    if (![string]::IsNullOrEmpty($settings.ffProbePath))
    {
        if (!(Test-Path $settings.ffProbePath))
        {
            $__logger.Info(("ffProbePath executable not found in {0} and saved path was deleted." -f $settings.ffProbePath))
            $settings.ffprobePath = $null
        }
    }

    if ($null -eq $settings.ffprobePath)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_SelectFfprobeExecutableMessage"), "Extra Metadata Tools")
        $ffProbePath = $PlayniteApi.Dialogs.SelectFile("ffProbe executable|ffProbe.exe")
        if ($ffProbePath)
        {
            $settings.ffProbePath = $ffProbePath
            $__logger.Info(("Saved ffprobre path: {0}" -f $settings.ffProbePath))
        }
    }
    
    # Setting: youtubedlPath
    if (![string]::IsNullOrEmpty($settings.youtubedlPath))
    {
        if (!(Test-Path $settings.youtubedlPath))
        {
            $__logger.Info(("youtubedlPath executable not found in {0} and saved path was deleted." -f $settings.youtubedlPath))
            $settings.youtubedlPath = $null
        }
    }

    if ($null -eq $settings.youtubedlPath)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_SelectYoutubeDlExecutableMessage"), "Extra Metadata Tools")
        $youtubedlPath = $PlayniteApi.Dialogs.SelectFile("youtube-dl executable|youtube-dl.exe")
        if ($youtubedlPath)
        {
            $settings.youtubedlPath = $youtubedlPath
            $__logger.Info(("Saved youtube-dl path: {0}" -f $settings.youtubedlPath))
        }
    }

    Save-Settings $settings

    $mandatorySettingsList = Get-MandatorySettingsList
    foreach ($setting in $mandatorySettingsList.GetEnumerator())
    {
        if ([string]::IsNullOrEmpty($settings.($setting.Name)))
        {
            $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_SetupUnfinishedMessage"), "Extra Metadata Tools")
            exit
        }
    }
}

function Set-GameDirectory
{
    param (
        $game
    )

    $directory = [System.IO.Path]::Combine($PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", $game.Id) 
    if(![System.IO.Directory]::Exists($directory))
    {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    return $directory
}

function Get-RequestStatusCode
{
    param (
        $url
    )
    
    try {
        $request = [System.Net.WebRequest]::Create($url)
        $request.Method = "HEAD"
        $response = $request.GetResponse()
        return $response.StatusCode
    } catch {
        $statusCode = $_.Exception.InnerException.Response.StatusCode
        $errorMessage = $_.Exception.Message
        $__logger.Info("Error connecting to server. Error: $errorMessage")
        if ($statusCode -ne 'NotFound')
        {
            $PlayniteApi.Dialogs.ShowMessage("Error connecting to server. Error: $errorMessage");
        }
        return $statusCode
    }
}

function Get-DownloadString
{
    param (
        [string]$url
    )
    
    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.Encoding = [System.Text.Encoding]::UTF8
        $DownloadedString = $webClient.DownloadString($url)
        $webClient.Dispose()
        return $DownloadedString
    } catch {
        $webClient.Dispose()
        $errorMessage = $_.Exception.Message
        $__logger.Info("Error downloading file `"$url`". Error: $errorMessage")
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_GenericDownloadErrorMessage") -f $url, $errorMessage)) | Out-Null
        return
    }
}

function Get-DownloadFile
{
    param (
        [string]$url,
        [string]$destinationPath
    )
    
    try {
        $webClient = [System.Net.WebClient]::new()
        $webClient.DownloadFile($url, $destinationPath)
        $webClient.Dispose()

        # In case an empty file was downloaded
        # Steam has video urls that don't exist but have 'OK' status code
        if ((Get-Item $destinationPath).length -eq 0)
        {
            $__logger.Info(("Downloaded file had 0 lenght: {0}." -f $url, $errorMessage))
            try {
                Remove-Item $destinationPath
            } catch {}
            return $false
        }

        $__logger.Info(("Downloaded {0} to {1}" -f $url, $destinationPath))
        return $true
    } catch {
        $webClient.Dispose()
        $errorMessage = $_.Exception.Message
        $__logger.Info(("Error downloading file: {0}. ErrorMessage: {1}" -f $url, $errorMessage))
        return $false
    }
}

function Get-SteamAppList
{
    param (
        $steamAppListPath
    )

    $uri = 'https://api.steampowered.com/ISteamApps/GetAppList/v2/'
    $steamAppList = Get-DownloadString $uri
    if ($null -ne $steamAppList)
    {
        [array]$appListContent = ($steamAppList | ConvertFrom-Json).applist.apps
        foreach ($steamApp in $appListContent) {
            $steamApp.name = $steamApp.name.ToLower() -replace '[^\p{L}\p{Nd}]', ''
        }
        ConvertTo-Json $appListContent -Depth 2 -Compress | Out-File -Encoding 'UTF8' -FilePath $steamAppListPath
        $__logger.Info("Downloaded AppList")
        $global:steamAppListDownloaded = $true
    }
    else
    {
        exit
    }
}

function Set-GlobalAppList
{
    param (
        [bool]$forceDownload
    )
    
    # Get Steam AppList
    $steamAppListPath = Join-Path -Path $env:TEMP -ChildPath 'SteamAppList.json'
    if (!(Test-Path $steamAppListPath) -or ($forceDownload -eq $true))
    {
        Get-SteamAppList $steamAppListPath
    }
    $global:steamAppList = @{}
    [object]$appListJson = [System.IO.File]::ReadAllLines($steamAppListPath) | ConvertFrom-Json
    foreach ($steamApp in $appListJson) {
        # Use a try block in case multple apps use the same name
        try {
            $steamAppList.add($steamApp.name, $steamApp.appid)
        } catch {}
    }

    $__logger.Info(("Global applist set from {0}" -f $steamAppListPath))
}

function Get-SteamAppId
{
    param (
        $game
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
    if ($null -ne $steamAppList[$gameName])
    {
        $appId = $steamAppList[$gameName].ToString()
        $__logger.Info(("Game: {0}, appId {1} found via AppList" -f $game.Name, $appId))
        return $appId
    }
    
    if ((!$appId) -and ($appListDownloaded -eq $false))
    {
        # Download Steam AppList if game was not found in local Steam AppList database and local Steam AppList database is older than 2 days
        $steamAppListPath = Join-Path -Path $env:TEMP -ChildPath 'SteamAppList.json'
        $AppListLastWrite = (Get-Item $steamAppListPath).LastWriteTime
        $timeSpan = New-Timespan -days 2
        if (((Get-date) - $AppListLastWrite) -gt $timeSpan)
        {
            Set-GlobalAppList $true
            if ($null -ne $steamAppList[$gameName])
            {
                $appId = $steamAppList[$gameName].ToString()
                $__logger.Info(("Game: {0}, appId {1} found via AppList" -f $game.Name, $appId))
                return $appId
            }
            return $null
        }
    }
}

function Get-SteamVideoUrl
{
    param (
        $game,
        $videoQuality
    )

    $appId = Get-SteamAppId $game

    if ($null -eq $appId)
    {
        $__logger.Info(("Couldn't obtain appId. Game: {0}" -f $game.Name))
        return $null
    }
    
    # Set Steam API url and download json file
    Start-Sleep -Milliseconds 1300
    $steamApiUrl = "https://store.steampowered.com/api/appdetails?appids={0}" -f $appId
    try {
        $json = Get-DownloadString $steamApiUrl | ConvertFrom-Json
    } catch {
        $errorMessage = $_.Exception.Message
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_ErrorDownloadGameInformationMessage") -f $errorMessage)) | Out-Null
        exit
    }

    # Check if json has 'movie' information
    if ($json.$appId.data.movies)
    {
        $videoId = $json.$AppId.data.movies[0].id
        if (($videoQuality -eq "480") -or ($videoQuality -eq "max"))
        {
            $videoUrl = $json.$AppId.data.movies[0].mp4.$VideoQuality -replace "\?t=\d+"
        }
        else
        {
            $videoUrl = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/microtrailer.mp4" -f $videoId
        }
        
        $__logger.Info(("Obtained video Url: {0}" -f $videoUrl))
        return $videoUrl
    }
    else
    {
        $__logger.Info(("No movie data. Url: {0}" -f $steamApiUrl))
        return $null
    }
}

function Set-SteamVideo
{
    param (
        [string]$videoQuality
    )
    
    $settings = Get-Settings
    if ($null -eq $steamAppList)
    {
        Set-GlobalAppList $false
    }
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    $global:steamAppListDownloaded = $false
    $videoSetCount = 0

    switch ($videoQuality) {
        "max" {$videoName = "VideoTrailer.mp4"}
        "480" {$videoName = "VideoTrailer.mp4"}
        "micro" {$videoName = "VideoMicrotrailer.mp4"}
        default {$videoName = "VideoTrailer.mp4"}
    }

    foreach ($game in $gameDatabase)
    {
        $extraMetadataDirectory = Set-GameDirectory $game
        $videoPath = Join-Path $extraMetadataDirectory -ChildPath $videoName
        if (Test-Path $videoPath)
        {
            continue
        }
        
        $videoTempPath = Join-Path $extraMetadataDirectory -ChildPath "VideoTemp.mp4"
        $videoUrl = Get-SteamVideoUrl -Game $game -VideoQuality $videoQuality
        if ($null -eq $videoUrl)
        {
            continue
        }
        $downloadSuccess = Get-DownloadFile $videoUrl $videoTempPath
        if ($downloadSuccess -eq $true)
        {
            $isConversionNeeded = Get-IsConversionNeeded $videoTempPath
            if ($isConversionNeeded -eq "invalidFile")
            {
                continue
            }
            elseif ($isConversionNeeded -eq "true")
            {
                $arguments = @("-y", "-i", "`"$videoTempPath`"", "-c:v", "libx264", "-c:a", "mp3", "-vf", "scale=trunc(iw/2)*2:trunc(ih/2)*2", "-pix_fmt", "yuv420p", "`"$videoPath`"")
                $__logger.Info(("Starting ffmpeg with arguments {0}" -f ($arguments -join ", ")))
                Start-Process -FilePath $settings.ffmpegPath -ArgumentList $arguments -Wait -WindowStyle Hidden
            }
            else
            {
                Move-Item $videoTempPath $videoPath -Force
                $__logger.Info(("Conversion is not needed for video and moved to {0}" -f $videoPath))
            }
            try {
                Remove-Item $videoTempPath -Force
            } catch {}
            if (Test-Path $videoPath)
            {
                $videoSetCount++
            }
        }
    }

    # Update assets status of collection
    Update-CollectionExtraAssetsStatus $gameDatabase $false
    $PlayniteApi.Dialogs.ShowMessage(("Done.`n`nSet video to {0} game(s)" -f $videoSetCount.ToString()), "Extra Metadata Tools")
}

function Get-IsConversionNeeded
{
    param (
        $videoPath
    )
    
    $videoInformation = Get-VideoInformation $videoPath

    if ($null -eq $videoInformation)
    {
        return "invalidFile"
    }

    if ($null -eq $videoInformation)
    {
        $__logger.Info(("File {0} is invalid, could not obtain video information" -f $videoPath))
        return "invalidFile"
    }
    if (($null -eq $videoInformation.width) -or ($null -eq $videoInformation.height))
    {
        $__logger.Info(("File {0} is invalid, could not obtain width and height information" -f $videoPath))
        return "invalidFile"
    }
    if ($null -eq $videoInformation.pix_fmt)
    {
        $__logger.Info(("File {0} is invalid. Couldn't obtain pixel format information" -f $videoPath))
        return "invalidFile"
    }
    if ($videoInformation.pix_fmt -eq "yuv444p")
    {
        $__logger.Info(("Conversion is needed for video {0}, color encoding is yuv444p" -f $videoPath))
        return "true"
    }
    if ([System.IO.Path]::GetExtension($videoPath) -ne ".mp4")
    {
        $__logger.Info(("Conversion is needed for video {0}, extension is not mp4" -f $videoPath))
        return "true"
    }
    return "false"
}

function Get-VideoInformation
{
    param (
        [string]$videoPath
    )

    $settings = Get-Settings

    # Set ErrorActionPreference in case source video is invalid and ffprobe throws an error
    $ErrorActionPreference = "SilentlyContinue"

    $arguments = @("-v", "error", "-select_streams", "v:0", "-show_entries", "stream=width,height, codec_name_name, pix_fmt, duration", "-of", "json", "`"$videoPath`"")
    
    $output = &$settings.ffProbePath $arguments | ConvertFrom-Json
    
    # Restore ErrorActionPreference
    $ErrorActionPreference = "Stop"
    
    if ($null -eq $output.streams)
    {
        $__logger.Info(("File {0} is invalid. Couldn't obtain any output with ffprobe" -f $videoPath))
        return $null
    }

    if ($output.streams.Count -eq 0)
    {
        $__logger.Info(("File {0} is invalid. Obtained number of streams was 0" -f $videoPath))
        return $null
    }

    $propertiesString = @(
        "Video Path: $videoPath"
    )

    $videoInformation = [PSCustomObject]@{}
    foreach ($property in $output.streams[0].PSObject.Properties)
    {
        $videoInformation | Add-Member -NotePropertyName $property.Name -NotePropertyValue $property.Value
        $propertiesString += ("{0}: {1}" -f $property.Name, $property.Value)
    }
    $__logger.Info(($propertiesString -join ", "))

    return $videoInformation
}

function Get-VideoMicrotrailerFromVideo
{
    param (
        [string]$videoSourcePath,
        [string]$videoDestinationPath
    )

    $settings = Get-Settings
    $videoInformation = Get-VideoInformation $videoSourcePath

    if ($null -eq $videoInformation)
    {
        return $null
    }

    if ($null -eq $videoInformation.pix_fmt)
    {
        $__logger.Info(("File {0} is invalid. Couldn't obtain pixel format information" -f $videoSourcePath))
        $PlayniteApi.Dialogs.ShowMessage(("File {0} is invalid. Couldn't obtain pixel format information" -f $videoSourcePath))
        return $null
    }
    $videoDuration = [System.Double]::Parse($videoInformation.duration, [CultureInfo]::InvariantCulture)
    if ($videoDuration -le 14)
    {
        $isConversionNeeded = Get-IsConversionNeeded $videoSourcePath
        if ($isConversionNeeded -eq "invalidFile")
        {
            $PlayniteApi.Dialogs.ShowMessage(("File {0} is invalid. Couldn't obtain video information" -f $videoSourcePath))
            return
        }
        elseif ($isConversionNeeded -eq "true")
        {
            # Convert
            $arguments = @("-y", "-i", "`"$videoSourcePath`"", "-c:v", "libx264", "-vf", "scale=trunc(iw/2)*2:trunc(ih/2)*2", "-pix_fmt", "yuv420p", "-an", "`"$videoDestinationPath`"")
            $__logger.Info(("Starting ffmpeg with arguments {0}" -f ($arguments -join ", ")))
            Start-Process -FilePath $settings.ffmpegPath -ArgumentList $arguments -Wait -WindowStyle Hidden
        }
        else
        {
            # Just copy stream without audio
            $arguments = @("-y", "-i", "`"$videoSourcePath`"", "-c:v", "copy", "-an", "`"$videoDestinationPath`"")
            $__logger.Info(("Starting ffmpeg with arguments {0}" -f ($arguments -join ", ")))
            Start-Process -FilePath $settings.ffmpegPath -ArgumentList $arguments -Wait -WindowStyle Hidden
        }
    }
    else
    {
        $rangeString = @()
        $clipDuration = 1
        $startPercentageVideo = @(
            15,
            25,
            35,
            45,
            55,
            65
        )
        foreach ($percentage in $startPercentageVideo) {
            [double]$clipStart = ($percentage * $videoDuration) / 100
            [double]$clipEnd = $clipStart + $clipDuration
            $rangeString += "between(t,{0:n2},{1:n2})" -f $clipStart.ToString([cultureinfo]::InvariantCulture), $clipEnd.ToString([cultureinfo]::InvariantCulture)
        }
        
        # Convert
        $selectString = "`"select='" + ($rangeString -join "+") + "', setpts=N/FRAME_RATE/TB" + ", scale=trunc(iw/2)*2:trunc(ih/2)*2`""
        $arguments = @("-y", "-i", "`"$videoSourcePath`"", "-vf", $selectString, "-c:v", "libx264", "-pix_fmt", "yuv420p", "-an", "`"$videoDestinationPath`"")
        $__logger.Info(("Starting ffmpeg with arguments {0}" -f ($arguments -join ", ")))
        Start-Process -FilePath $settings.ffmpegPath -ArgumentList $arguments -Wait -WindowStyle Hidden
    }
    return
}

function Get-VideoMicrotrailerFromTrailer
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Set-MandatorySettings
    $gameDatabase = $PlayniteApi.MainView.SelectedGames

    $microtrailersCreated = 0
    foreach ($game in $gameDatabase)
    {
        $extraMetadataDirectory = Set-GameDirectory $game
        $videoPath = Join-Path $extraMetadataDirectory -ChildPath "VideoTrailer.mp4"
        $videoMicrotrailerPath = Join-Path $extraMetadataDirectory -ChildPath "VideoMicrotrailer.mp4"
        if (!(Test-Path $videoPath))
        {
            continue
        }

        Get-VideoMicrotrailerFromVideo $videoPath $videoMicrotrailerPath
        if (Test-Path $videoMicrotrailerPath)
        {
            $microtrailersCreated++
        }
    }

    # Update assets status of collection
    Update-CollectionExtraAssetsStatus $gameDatabase $false
    $PlayniteApi.Dialogs.ShowMessage(("Done.`n`nCreated {0} microtrailers from video trailers." -f $microtrailersCreated), "Extra Metadata Tools")
}

function Set-VideoManually
{
    param (
        [string]$videoQuality
    )
    
    $settings = Get-Settings
    
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -ne 1)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MoreThanSingleGameSelectedMessage")), "Extra Metadata tools");
        return
    }

    switch ($videoQuality) {
        "max" {$videoName = "VideoTrailer.mp4"}
        "480" {$videoName = "VideoTrailer.mp4"}
        "micro" {$videoName = "VideoMicroTrailer.mp4"}
        default {$videoName = "VideoTrailer.mp4"}
    }
    $game = $PlayniteApi.MainView.SelectedGames[0]

    $extraMetadataDirectory = Set-GameDirectory $game
    $videoPath = Join-Path $extraMetadataDirectory -ChildPath $videoName
    $videoTempPath = $PlayniteApi.Dialogs.SelectFile("Video file|*.mp4;*.avi;*.mkv;*.webm;*.flv;*.wmv;*.mov;*.m4v")
    if ([string]::IsNullOrEmpty($videoTempPath))
    {
        return
    }
    
    $isConversionNeeded = Get-IsConversionNeeded $videoTempPath
    if ($isConversionNeeded -eq "invalidFile")
    {
        $PlayniteApi.Dialogs.ShowMessage(("File {0} is not a valid video or is corrupted" -f $videoTempPath))
        return
    }
    elseif ($isConversionNeeded -eq "true")
    {
        $arguments = @("-y", "-i", "`"$videoTempPath`"", "-c:v", "libx264", "-c:a", "mp3", "-vf", "scale=trunc(iw/2)*2:trunc(ih/2)*2", "-pix_fmt", "yuv420p", "`"$videoPath`"")
        $__logger.Info(("Starting ffmpeg with arguments {0}" -f ($arguments -join ", ")))
        Start-Process -FilePath $settings.ffmpegPath -ArgumentList $arguments -Wait -WindowStyle Hidden
    }
    else
    {
        Copy-Item $videoTempPath $videoPath -Force
        $__logger.Info(("Conversion is not needed for video and copied to {0}" -f $videoPath))
    }
    if (Test-Path $videoPath)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_GenericFinishedMessage")), "Extra Metadata tools")
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_GenericErrorProcessingFileMessage")), "Extra Metadata tools")
    }

    # Update game assets status
    Update-GameExtraAssetsStatus $game $false
}

function Remove-DownloadedVideos
{ 
    param (
        [string]$videoQuality
    )
    
    switch ($videoQuality) {
        "max" {$videoName = "VideoTrailer.mp4"}
        "480" {$videoName = "VideoTrailer.mp4"}
        "micro" {$videoName = "VideoMicroTrailer.mp4"}
        default {$videoName = "VideoTrailer.mp4"}
    }
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    $deletedVideos = 0
    foreach ($game in $gameDatabase)
    {
        $extraMetadataDirectory = Set-GameDirectory $game
        $videoPath = Join-Path $extraMetadataDirectory -ChildPath $videoName
        if (Test-Path $videoPath)
        {
            try {
                Remove-Item $videoPath -Force
                $deletedVideos++
            } catch {
                $errorMessage = $_.Exception.Message
                $PlayniteApi.Dialogs.ShowMessage((("Game: {0}`nError deleting video: {1}`nError: {2}" + 
                "`n`nThe error could have been caused by the file being in use or currently playing on Playnite." +
                "`n`nThe game extra metadata directory will be opened, please delete the file manually when the video file is not in use" +
                "") -f $game.Name, $videoPath, $errorMessage),
                "Extra Metadata Tools")
                Start-Process $extraMetadataDirectory
                continue
            }
        }
    }

    # Update assets status of collection
    Update-CollectionExtraAssetsStatus $gameDatabase $false
    $PlayniteApi.Dialogs.ShowMessage(("Done.`n`nDeleted videos: {0}" -f $deletedVideos.ToString()), "Extra Metadata Tools")
}

function Set-YouTubeVideoID
{
    param(
        $scriptGameMenuItemActionArgs
    )

    $result = $PlayniteApi.Dialogs.SelectString([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_EnterYoutubeIdMessage"), "Extra Metadata Tools", "")

    if ($result.Result)
    {
        if ($result.SelectedString -ne "")
        {
            Set-YouTubeVideoManual $result.SelectedString
        }
    }
}

function Set-YouTubeVideo
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Set-MandatorySettings
    $settings = Get-Settings
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    $videoSetCount = 0

    foreach ($game in $gameDatabase)
    {
        $search = "`"ytsearch1:{0} {1}`"" -f $game.Name, "trailer"
        if ($null -ne $game.Platforms)
        {
            if ([string]::IsNullOrEmpty($game.Platforms[0].Name) -eq $false)
            {
                $search = "`"ytsearch1:{0} {1} {2}`"" -f $game.Name, $game.Platforms[0].Name, "trailer"
            }
        }

        $extraMetadataDirectory = Set-GameDirectory $game
        $videoPath = Join-Path $extraMetadataDirectory -ChildPath "VideoTrailer.mp4"
        $videoTempPath = Join-Path $extraMetadataDirectory -ChildPath "VideoTemp.mp4"
        $youtubedl = $settings.youtubedlPath

        if (Test-Path $videoPath)
        {
            continue
        }
        if (Test-Path $videoTempPath)
        {
            try {
                Remove-Item $videoTempPath -Force
            } catch {}
        }
        
        $trailerdownloadparams = @{
            'FilePath'     = $youtubedl
            'ArgumentList' = '-o ' + "`"$videoTempPath`"", '-f "mp4"', $search
            'Wait'         = $true
            'PassThru'     = $true
        }
        $proc = Start-Process @trailerdownloadparams -WindowStyle $settings.youtubedlWindowStyle
        
        if (Test-Path $videoTempPath -PathType leaf)
        {
            $isConversionNeeded = Get-IsConversionNeeded $videoTempPath
            if ($isConversionNeeded -eq "invalidFile")
            {
                continue
            }
            elseif ($isConversionNeeded -eq "true")
            {
                $arguments = @("-y", "-i", "`"$videoTempPath`"", "-c:v", "libx264", "-c:a", "mp3", "-vf", "scale=trunc(iw/2)*2:trunc(ih/2)*2", "-pix_fmt", "yuv420p", "`"$videoPath`"")
                $__logger.Info(("Starting ffmpeg with arguments {0}" -f ($arguments -join ", ")))
                Start-Process -FilePath $settings.ffmpegPath -ArgumentList $arguments -Wait -WindowStyle Hidden
            }
            else
            {
                Move-Item $videoTempPath $videoPath -Force
                $__logger.Info(("Conversion is not needed for video and moved to {0}" -f $videoPath))
            }
            try {
                Remove-Item $videoTempPath -Force
            } catch {}
            if (Test-Path $videoPath)
            {
                $videoSetCount++
            }
        }
    }

    # Update assets status of collection
    Update-CollectionExtraAssetsStatus $gameDatabase $false

    $PlayniteApi.Dialogs.ShowMessage(("Done.`n`nSet video to {0} game(s)" -f $videoSetCount.ToString()), "Extra Metadata Tools")
}

function Get-YoutubeResultsArray
{
    param (
        [string]$queryInput
    )

    $query = [uri]::EscapeDataString($queryInput)
    $uri = "https://www.youtube.com/results?search_query={0}&sp=EgQQARgB" -f $query
    $webContent = Get-DownloadString $uri
    $webContent -match 'var ytInitialData = ((.*?(?=(;<\/script>))))' | Out-Null

    [System.Collections.ArrayList]$searchResults = @()
    if ($matches)
    {
        $json = $matches[1] | ConvertFrom-Json
        $searchItems = $json.contents.twoColumnSearchResultsRenderer.primaryContents.sectionListRenderer.contents.itemSectionRenderer.contents | Select-Object -First 12
        foreach ($searchItem in $searchItems)
        {
            if ($null -eq $searchItem.videoRenderer)
            {
                continue
            }
    
            $thumbnailUrl = ($searchItem.videoRenderer.thumbnail.thumbnails | Sort-Object -Property width)[0].url
            $searchResult = [PSCustomObject]@{
                Name = $searchItem.videoRenderer.title.runs.text
                Value = $searchItem.videoRenderer.videoId
                Lenght = $searchItem.videoRenderer.lengthText.simpleText
                ChannelName = $searchItem.videoRenderer.ownerText.runs.text
                Thumbnail = $thumbnailUrl
            }
            $searchResults.Add($searchResult) | Out-Null
        }
    }

    return $searchResults
}

function Invoke-TempFilesCleanup
{
    Get-ChildItem -Path $CurrentExtensionDataPath -File | Where-Object {$_.Name -like "*.jpg"} | ForEach-Object {
        try {
            Remove-Item $_.FullName
        } catch {  

        }
    }
}

function Invoke-YoutubeSearchWindow
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Set-MandatorySettings
    
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -ne 1)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MoreThanSingleGameSelectedMessage")), "Extra Metadata tools");
        return
    }
    $game = $gameDatabase[0]

    # Load assemblies
    Add-Type -AssemblyName PresentationCore
    Add-Type -AssemblyName PresentationFramework

    # Set Xaml
    [xml]$Xaml = @"
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <Grid.Resources>
        <Style TargetType="TextBlock" BasedOn="{StaticResource BaseTextBlockStyle}" />
    </Grid.Resources>

    <StackPanel Margin="20">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBox Name="TextboxSearch" Grid.Column="0" HorizontalContentAlignment="Stretch"/>
            <Button Name="ButtonVideoSearch" Grid.Column="1" Content="Search" HorizontalAlignment="Right" Margin="10,0,0,0" IsDefault="True"/>
        </Grid>
        <TextBlock Text="Video list:" Margin="0,20,0,0"/>
        <ListBox Name="ListBoxVideos" Height="500" HorizontalContentAlignment="Stretch">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <DockPanel>
                        <Image Margin="3" Height="80" Source="{Binding Thumbnail}"/>
                        <StackPanel Margin="5,0,0,0">
                            <TextBlock Margin="3" Text="{Binding Name}"/>
                            <DockPanel Margin="3">
                                <TextBlock Margin="0" Text="{Binding ChannelName}" DockPanel.Dock="Left"/>
                                <TextBlock Margin="15,0,0,0" Text="{Binding Lenght}"/>
                            </DockPanel>
                        </StackPanel>
                    </DockPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
        <Button Content="Download selected video" HorizontalAlignment="Center" Margin="0,20,0,0" Name="ButtonDownloadVideo" IsDefault="False"/>
    </StackPanel>
</Grid>
"@

    # Load the xaml for controls
    $XMLReader = [System.Xml.XmlNodeReader]::New($Xaml)
    $XMLForm = [Windows.Markup.XamlReader]::Load($XMLReader)

    # Make variables for each control
    $Xaml.FirstChild.SelectNodes("//*[@Name]") | ForEach-Object {Set-Variable -Name $_.Name -Value $XMLForm.FindName($_.Name) }

    # Set items sources of controls
    $query = "{0} Trailer" -f $game.Name
    if ($null -ne $game.Platforms)
    {
        if ([string]::IsNullOrEmpty($game.Platforms[0].Name) -eq $false)
        {
            $query = "{0} {1} Trailer" -f $game.Name, $game.Platforms[0].Name
        }
    }
    $TextboxSearch.Text = $query
    $ListBoxVideos.ItemsSource = Get-YoutubeResultsArray $query

    # Set Window creation options
    $windowCreationOptions = New-Object Playnite.SDK.WindowCreationOptions
    $windowCreationOptions.ShowCloseButton = $true
    $windowCreationOptions.ShowMaximizeButton = $False
    $windowCreationOptions.ShowMinimizeButton = $False

    # Create window
    $window = $PlayniteApi.Dialogs.CreateWindow($windowCreationOptions)
    $window.Content = $XMLForm
    $window.Width = 800
    $window.Height = 700
    $window.Title = "Extra Metadata Tools Video"
    $window.WindowStartupLocation = "CenterScreen"

    # Handler for pressing "Search" button
    $ButtonVideoSearch.Add_Click(
    {
        $ListBoxVideos.ItemsSource = Get-YoutubeResultsArray $TextboxSearch.Text
    })

    # Handler for pressing "Download selected video" button
    $ButtonDownloadVideo.Add_Click(
    {
        $YouTubeID = $ListBoxVideos.SelectedValue.Value
        $window.Close()
        Set-YouTubeVideoManual $YouTubeID
    })
    
    # Show Window
    $window.ShowDialog()
}

function Set-YouTubeVideoManual
{
    param (
        [string]$YouTubeID
    )
    
    $settings = Get-Settings
    
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -ne 1)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MoreThanSingleGameSelectedMessage")), "Extra Metadata tools");
        return
    }

    $game = $gameDatabase[0]

    $extraMetadataDirectory = Set-GameDirectory $game
    $videoPath = Join-Path $extraMetadataDirectory -ChildPath "VideoTrailer.mp4"
    $videoTempPath = Join-Path $extraMetadataDirectory -ChildPath "VideoTemp.mp4"
    $youtubedl = $settings.youtubedlPath
    $search = '"' + "https://www.youtube.com/watch?v=" + $YouTubeID + '"'
    
    $trailerdownloadparams = @{
        'FilePath'     = $youtubedl
        'ArgumentList' = '-v -o ' + "`"$videoTempPath`"", '-f "mp4"', $search
        'Wait'         = $true
        'PassThru'     = $true
    }
    $proc = Start-Process @trailerdownloadparams -WindowStyle $settings.youtubedlWindowStyle
    
    if (Test-Path $videoTempPath -PathType leaf)
    {
        $isConversionNeeded = Get-IsConversionNeeded $videoTempPath
        if ($isConversionNeeded -eq "invalidFile")
        {
            $__logger.Info(("File {0} is not a valid video or is corrupted" -f $videoTempPath))
            $PlayniteApi.Dialogs.ShowMessage(("File {0} is not a valid video or is corrupted" -f $videoTempPath))
            return
        }
        elseif ($isConversionNeeded -eq "true")
        {
            $arguments = @("-y", "-i", "`"$videoTempPath`"", "-c:v", "libx264", "-c:a", "mp3", "-vf", "scale=trunc(iw/2)*2:trunc(ih/2)*2", "-pix_fmt", "yuv420p"," `"$videoPath`"")
            $__logger.Info(("Starting ffmpeg with arguments {0}" -f ($arguments -join ", ")))
            Start-Process -FilePath $settings.ffmpegPath -ArgumentList $arguments -Wait -WindowStyle Hidden
        }
        else
        {
            Move-Item $videoTempPath $videoPath -Force
            $__logger.Info(("Conversion is not needed for video and moved to {0}" -f $videoPath))
        }
        try {
            Remove-Item $videoTempPath -Force
        } catch {}
        if (Test-Path $videoPath)
        {
            $videoSetCount++
        }
    }
    if (Test-Path $videoPath)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_GenericFinishedMessage")), "Extra Metadata tools")
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_GenericErrorProcessingFileMessage")), "Extra Metadata tools")
    }

    # Update assets status
    Update-GameExtraAssetsStatus $game $false
}

function Remove-VideoTrailer
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Set-MandatorySettings
    Remove-DownloadedVideos "max"
}

function Remove-VideoMicrotrailer
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Set-MandatorySettings
    Remove-DownloadedVideos "micro"
}

function Set-VideoManuallyTrailer
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Set-MandatorySettings
    Set-VideoManually "max"
}

function Set-VideoManuallyMicrotrailer
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Set-MandatorySettings
    Set-VideoManually "micro"
}

function Get-SteamVideoSd
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Set-MandatorySettings
    Set-SteamVideo "480"
}

function Get-SteamVideoHd
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Set-MandatorySettings
    Set-SteamVideo "max"
}

function Get-SteamVideoMicro
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Set-MandatorySettings
    Set-SteamVideo "micro"
}

function Add-Tag
{
    param (
        [Playnite.SDK.Models.Game] $game,
        [Playnite.SDK.Models.Tag] $tag
    )

    if ($game.tagIds -notcontains $tag.Id)
    {
        if ($game.tagIds)
        {
            $game.tagIds.Add($tag.Id)
        }
        else
        {
            # Fix in case game has null tagIds
            $game.tagIds = $tag.Id
        }
        $PlayniteApi.Database.Games.Update($game)
        return $true
    }
    return $false
}

function Remove-Tag
{
    param (
        [Playnite.SDK.Models.Game] $game,
        [Playnite.SDK.Models.Tag] $tag
    )

    if ($game.tagIds -contains $tag.Id)
    {
        $game.tagIds.Remove($tag.Id)
        $PlayniteApi.Database.Games.Update($game)
        return $true
    }
    return $false
}

function Get-MissingAssetTags
{
    [System.Collections.Generic.List[[Playnite.SDK.Models.Tag]]]$tagsList = @(
        $PlayniteApi.Database.Tags.Add("[EMT] Video Trailer missing"),
        $PlayniteApi.Database.Tags.Add("[EMT] Video Microtrailer missing"),
        $PlayniteApi.Database.Tags.Add("[EMT] Logo missing")
    )

    return $tagsList
}

function Update-GameExtraAssetsStatus
{
    param (
        [Playnite.SDK.Models.Game] $game,
        [bool] $showResultsDialog
    )

    $tags = Get-MissingAssetTags
    $resultsString = "Finished.`nGame: {0}`nStatus.`n"
    foreach ($tag in $tags) {
        switch ($tag.Name) {
            "[EMT] Video Trailer missing" {$assetName = "VideoTrailer.mp4"; $resultsDescription = "`nGame has trailer: {0}"}
            "[EMT] Video Microtrailer missing" {$assetName = "VideoMicrotrailer.mp4"; $resultsDescription = "`nGame has microtrailer: {0}"}
            "[EMT] Logo missing" {$assetName = "Logo.png"; $resultsDescription = "`nGame has logo: {0}"}
            Default {continue}
        }

        $extraMetadataDirectory = Set-GameDirectory $game
        $assetPath = [System.IO.Path]::Combine($extraMetadataDirectory, $assetName)
        if ([System.IO.File]::Exists($assetPath))
        {
            Remove-Tag $game $Tag
        }
        else
        {
            Add-Tag $game $Tag
        }

        if ([System.IO.File]::Exists($assetPath))
        {
            $resultsString += $resultsDescription -f "Yes"
        }
        else
        {
            $resultsString += $resultsDescription -f "No"
        }
    }

    if ($showResultsDialog -eq $true)
    {
        $PlayniteApi.Dialogs.ShowMessage($resultsString, "Extra Metadata tools")
    }
}

function Update-CollectionExtraAssetsStatus
{
    param (
        [System.Collections.Generic.List[[Playnite.SDK.Models.Game]]] $gameCollection,
        [bool] $showResultsDialog
    )
    
    $tags = Get-MissingAssetTags

    foreach ($tag in $tags) {
        switch ($tag.Name) {
            "[EMT] Video Trailer missing" {$assetName = "VideoTrailer.mp4"}
            "[EMT] Video Microtrailer missing" {$assetName = "VideoMicrotrailer.mp4"}
            "[EMT] Logo missing" {$assetName = "Logo.png"}
            Default {continue}
        }
        foreach ($game in $gameCollection) {
            $extraMetadataDirectory = Set-GameDirectory $game
            $assetPath = [System.IO.Path]::Combine($extraMetadataDirectory, $assetName)
            if ([System.IO.File]::Exists($assetPath))
            {
                if ($game.tagIds -contains $tag.Id)
                {
                    $game.tagIds.Remove($tag.Id)
                    $PlayniteApi.Database.Games.Update($game)
                }
            }
            elseif ($game.tagIds -notcontains $tag.Id)
            {
                if ($game.tagIds)
                {
                    $game.tagIds.Add($tag.Id)
                }
                else
                {
                    # Fix in case game has null tagIds
                    $game.tagIds = $tag.Id
                }
                $PlayniteApi.Database.Games.Update($game)
            }
        }
    }

    if ($showResultsDialog -eq $true)
    {
        $resultsString = "Finished.`nTotal games: {0}`n`n" -f $gameCollection.Count
        
        foreach ($tag in $tags) {
            $tagBaseName = $tag.Name.Replace("[EMT] ", "")
            $resultsString += ("{0}: {1}" -f $tagBaseName, ($gameCollection | Where-Object {$_.TagIds -notcontains $tag.Id}).Count).Replace("missing", "exists")
            $resultsString += "`n"
            $resultsString += ("{0}: {1}" -f $tagBaseName, ($gameCollection | Where-Object {$_.TagIds -contains $tag.Id}).Count)
            $resultsString += "`n"
        }
        $PlayniteApi.Dialogs.ShowMessage($resultsString, "Extra Metadata tools")
    }
}

function Update-AssetsStatusGameDatabase
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    Update-CollectionExtraAssetsStatus $PlayniteApi.Database.Games $true
}

function OnLibraryUpdated
{
    Update-CollectionExtraAssetsStatus $PlayniteApi.Database.Games $false
}