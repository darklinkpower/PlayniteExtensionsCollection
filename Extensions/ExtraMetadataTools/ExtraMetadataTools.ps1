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
    $menuItem2.Description =  "Download logos for selected PC games from Steam"
    $menuItem2.FunctionName = "Get-SteamLogos"
    $menuItem2.MenuSection = "Extra Metadata tools|Logos"
    
    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  "Download logos for selected games from SteamGridDB"
    $menuItem3.FunctionName = "Get-SgdbLogo"
    $menuItem3.MenuSection = "Extra Metadata tools|Logos"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem4.Description =  "Add logo from local file of selected game"
    $menuItem4.FunctionName = "Get-SteamLogosLocal"
    $menuItem4.MenuSection = "Extra Metadata tools|Logos"

    $menuItem5 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem5.Description =  "Add logo from url of selected game"
    $menuItem5.FunctionName = "Get-SteamLogosUri"
    $menuItem5.MenuSection = "Extra Metadata tools|Logos"

    $menuItem6 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem6.Description =  "Convert icons to custom logos asset"
    $menuItem6.FunctionName = "Get-IconToLogoConvert"
    $menuItem6.MenuSection = "Extra Metadata tools|Logos"

    $menuItem7 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem7.Description =  "Delete logos of selected games"
    $menuItem7.FunctionName = "Remove-LogosSelectedGames"
    $menuItem7.MenuSection = "Extra Metadata tools|Logos"

    $menuItem8 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem8.Description =  "Open themes directory"
    $menuItem8.FunctionName = "Invoke-ThemesDirectoryRootOpen"
    $menuItem8.MenuSection = "Extra Metadata tools|Themes"

    $menuItem9 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem9.Description =  "Set profile picture"
    $menuItem9.FunctionName = "Set-ProfilePicture"
    $menuItem9.MenuSection = "Extra Metadata tools|Themes"

    $menuItem10 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem10.Description =  "Set background music"
    $menuItem10.FunctionName = "Set-BackgroundMusic"
    $menuItem10.MenuSection = "Extra Metadata tools|Themes"

    $menuItem11 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem11.Description =  "Set background video"
    $menuItem11.FunctionName = "Set-BackgroundVideo"
    $menuItem11.MenuSection = "Extra Metadata tools|Themes"

    $menuItem12 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem12.Description =  "Set SteamGridDB API Key"
    $menuItem12.FunctionName = "Set-SgdbApiKey"
    $menuItem12.MenuSection = "Extra Metadata tools|Other"

    $menuItem13 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem13.Description =  "Update `"Logo missing`" tags of selected game(s)"
    $menuItem13.FunctionName = "Add-TagMissingLogo"
    $menuItem13.MenuSection = "Extra Metadata tools|Other"

    $menuItem14 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem14.Description =  "Download logo for selected game from Google"
    $menuItem14.FunctionName = "Get-GoogleLogo"
    $menuItem14.MenuSection = "Extra Metadata tools|Logos"

    return $menuItem, $menuItem2, $menuItem3, $menuItem14, $menuItem4, $menuItem5, $menuItem6, $menuItem7, $menuItem8, $menuItem9, $menuItem10, $menuItem11, $menuItem12, $menuItem13
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
                $PlayniteApi.Dialogs.ShowMessage("Extra Metadata configuration for the theme `"$themeInUse`" updated.`nPlease restart Playnite to make changes take effect.", "Extra Metadata Tools");
            }
        }
    }
}

function Get-GoogleResultsArray
{ 
    param (
        [string]$queryInput,
        [bool]$transparantImages
    )

    $query = [uri]::EscapeDataString($queryInput)
    $uri = ""
    if ($transparantImages)
    {
        $uri = "https://www.google.com/search?tbm=isch&client=firefox-b-d&source=lnt&q={0}&tbs=ic:trans" -f $query
    }
    else
    {
        $uri = "https://www.google.com/search?tbm=isch&client=firefox-b-d&source=lnt&q={0}" -f $query
    }
    $webViewSettings = New-Object "Playnite.SDK.WebViewSettings"
    $webViewSettings.CacheEnabled = $false
    $webViewSettings.JavaScriptEnabled = $true
    $webView = $PlayniteApi.WebViews.CreateOffscreenView($webViewSettings)
    $webView.NavigateAndWait($uri)
    $googleContent = $webView.GetPageSource()
    $googleContent = $googleContent -replace "\r\n?|\n", ""
    $regex = "\[""(https:\/\/encrypted-[^,]+?)"",\d+,\d+\],\[""(http.+?)"",(\d+),(\d+)\]"
    $regexmatch = ([regex]$regex).Matches($($googleContent))
    [System.Collections.ArrayList]$searchResults = @()
    foreach ($match in $RegexMatch)
    { 
        $json = ConvertFrom-Json("[" + $match.Value + "]")
        $searchResult = [PSCustomObject]@{
            Width  = [string]($json[1][1]) 
            Height  = [string]($json[1][2]) 
            ImageUrl = $json[1][0]
            ThumbUrl  = $json[0][0]
            Size = [string]($json[1][1])  + "x" + [string]($json[1][2])  
        }
        $searchResults.Add($searchResult) | Out-Null
    }
    $webView.Dispose()

    return $searchResults
}

function Get-GoogleLogo
{
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -gt 1)
    {
        $PlayniteApi.Dialogs.ShowMessage("More than one game is selected, please select only one game.", "Extra Metadata tools");
        return
    }

    # Load assemblies
    Add-Type -AssemblyName PresentationCore
    Add-Type -AssemblyName PresentationFramework

    # Set Xaml
    [xml]$Xaml = @"
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <Grid.Resources>
        <Style TargetType="TextBlock" BasedOn="{StaticResource BaseTextBlockStyle}" />
    </Grid.Resources>

    <Grid Margin="0,4,0,0">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>            
        </Grid.RowDefinitions>
        <DockPanel Grid.Row="0" Margin="15,5,10,5">        
            <CheckBox Name="CheckboxTransparant" IsChecked="True" Content="Transparant"
                DockPanel.Dock="Left" VerticalAlignment="Center" Margin="10,0,0,0"/>
        </DockPanel>
        <DockPanel Grid.Row="1" Margin="15,5,10,5">        
            <Grid DockPanel.Dock="Left">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                </Grid.ColumnDefinitions>
                <TextBox  Name="TextboxSearch" Grid.Column="0" HorizontalContentAlignment="Stretch"/>
                <Button  Grid.Column="1" Margin="10,0,0,0" Name="ButtonImageSearch" Content="Search" HorizontalAlignment="Right" IsDefault="True"/>
            </Grid>
        </DockPanel>        
        <ListBox Grid.Row="2" Name="ListBoxImages"
            ScrollViewer.HorizontalScrollBarVisibility="Disabled"
            BorderThickness="0"
            ScrollViewer.VerticalScrollBarVisibility="Auto">
            <ListBox.ItemsPanel>
                <ItemsPanelTemplate>
                    <WrapPanel />
                </ItemsPanelTemplate>
            </ListBox.ItemsPanel>
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <Border Margin="4" Background="#33000000">
                        <DockPanel ToolTip="{Binding ImageUrl}"
                                   ToolTipService.InitialShowDelay="2000">
                            <TextBlock DockPanel.Dock="Bottom" Text="{Binding Size, StringFormat={}{0}px}"
                                       Margin="0,3,0,0"
                                       HorizontalAlignment="Center" VerticalAlignment="Center" />
                            <Image Width="240" Height="180"
                                   Source="{Binding ThumbUrl, IsAsync=True}"
                                   DockPanel.Dock="Top"
                                   Stretch="Uniform" StretchDirection="Both" />
                        </DockPanel>
                    </Border>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
        <Button Grid.Row="3" Content="Download selected Logo" HorizontalAlignment="Center" Margin="0,20,0,10" Name="ButtonDownloadLogo" IsDefault="False"/>
    </Grid>
</Grid>
"@
    foreach ($game in $gameDatabase) 
    {
        # Load the xaml for controls
        $XMLReader = [System.Xml.XmlNodeReader]::New($Xaml)
        $XMLForm = [Windows.Markup.XamlReader]::Load($XMLReader)

        # Make variables for each control
        $Xaml.FirstChild.SelectNodes("//*[@Name]") | ForEach-Object {Set-Variable -Name $_.Name -Value $XMLForm.FindName($_.Name) }

        # Set items sources of controls
        $query = "{0} Logo" -f $game.Name
        $TextboxSearch.Text = $query
        $ListBoxImages.ItemsSource = Get-GoogleResultsArray $query $true

        # Set Window creation options
        $windowCreationOptions = New-Object Playnite.SDK.WindowCreationOptions
        $windowCreationOptions.ShowCloseButton = $true
        $windowCreationOptions.ShowMaximizeButton = $False
        $windowCreationOptions.ShowMinimizeButton = $False

        # Create window
        $window = $PlayniteApi.Dialogs.CreateWindow($windowCreationOptions)
        $window.Content = $XMLForm
        $window.Width = 830
        $window.Height = 600
        $window.Title = "Extra Metadata Tools - Google Logo Search"
        $window.WindowStartupLocation = "CenterScreen"

        # Handler for pressing "Search" button
        $ButtonImageSearch.Add_Click(
        {
            $ListBoxImages.ItemsSource = Get-GoogleResultsArray $TextboxSearch.Text $CheckboxTransparant.IsChecked
        })

        # Handler for pressing "Download selected video" button
        $ButtonDownloadLogo.Add_Click(
        {
            $logoUri = $ListBoxImages.SelectedValue.ImageUrl
            $window.Close()
            if (($logoUri) -and ($logoUri -ne ""))
            {
                $extraMetadataDirectory = Set-GameDirectory $game
                $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
                try {
                    $webClient = New-Object System.Net.WebClient
                    $webClient.DownloadFile($logoUri, $logoPath)
                    $webClient.Dispose()
                    $PlayniteApi.Dialogs.ShowMessage("Added logo file to `"$($game.name)`"", "Extra Metadata tools")
                } catch {
                    $errorMessage = $_.Exception.Message
                    $__logger.Info("Error downloading file `"$url`". Error: $errorMessage")
                    $PlayniteApi.Dialogs.ShowMessage("Error downloading file `"$url`". Error: $errorMessage")
                }
            }
        })

        # Show Window
        $window.ShowDialog()
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
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    return $directory
}

function Invoke-ThemesDirectoryRootOpen
{
    $directory = $PlayniteApi.Paths.ConfigurationPath + "\ExtraMetadata\Themes"
    if(!(Test-Path $directory))
    {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    Invoke-Item $directory
}

function Set-FullscreenThemesDirectory
{
    $directory = $PlayniteApi.Paths.ConfigurationPath + "\ExtraMetadata\Themes\Fullscreen\"
    if(!(Test-Path $directory))
    {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    return $directory
}

function Set-DesktopThemesDirectory
{
    $directory = $PlayniteApi.Paths.ConfigurationPath + "\ExtraMetadata\Themes\Desktop\"
    if(!(Test-Path $directory))
    {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    return $directory
}

function Set-CommonThemesDirectory
{
    $directory = $PlayniteApi.Paths.ConfigurationPath + "\ExtraMetadata\Themes\Common\"
    if(!(Test-Path $directory))
    {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
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
        if ([string]::IsNullOrEmpty($logoPathLocal))
        {
            return
        }
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

function Set-ProfilePicture
{
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
            $PlayniteApi.Dialogs.ShowMessage("Error converting image file to png. Error: `"$errorMessage`"", "Extra Metadata Tools");
            return
        }
    }
    $PlayniteApi.Dialogs.ShowMessage("Profile picture set.", "Extra Metadata tools")
}

function Set-BackgroundMusic
{
    $file = $PlayniteApi.Dialogs.SelectFile("mp3|*.mp3")
    if ([string]::IsNullOrEmpty($file))
    {
        return
    }
    $fileDestination = Set-FullscreenThemesDirectory | Join-Path -ChildPath "BackgroundMusic.mp3"
    Copy-Item $file $fileDestination -Force
    $PlayniteApi.Dialogs.ShowMessage("Background music set.", "Extra Metadata tools")
}

function Set-BackgroundVideo
{
    $file = $PlayniteApi.Dialogs.SelectFile("mp4|*.mp4")
    if ([string]::IsNullOrEmpty($file))
    {
        return
    }
    $fileDestination = Set-FullscreenThemesDirectory | Join-Path -ChildPath "BackgroundVideo.mp4"
    Copy-Item $file $fileDestination -Force
    $PlayniteApi.Dialogs.ShowMessage("Background video set.", "Extra Metadata tools")
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

function Get-SgdbRequestUrl
{
    param (
        $game,
        $sgdbApiKey
    )

    switch ([Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId)) {
        "SteamLibrary" { $platformEnum = "steam" ; break }
        <# Don't use SGDB platform enums since a lot of games are not associated with the Platforms IDs
        "OriginLibrary" { $platformEnum = "origin" ; break }
        "EpicLibrary" { $platformEnum = "egs" ; break }
        "BattleNetLibrary" { $platformEnum = "bnet" ; break }
        "UplayLibrary" { $platformEnum = "uplay" ; break } #>
        Default { $platformEnum = $null ; break }
    }

    if ($null -ne $platformEnum)
    {
        $requestUri = "https://www.steamgriddb.com/api/v2/logos/{0}/{1}" -f $platformEnum, $game.GameId
        return $requestUri
    }
    else
    {
        if ($game.Platform.Name -eq "PC")
        {
            $steamAppId = Get-SteamAppId $game
            if ($null -ne $steamAppId)
            {
                $requestUri = "https://www.steamgriddb.com/api/v2/logos/steam/{0}" -f $steamAppId
                return $requestUri
            }
        }

        try {
            $requestUri = "https://www.steamgriddb.com/api/v2/search/autocomplete/{0}" -f [uri]::EscapeDataString($game.name)
            $webClient = New-Object System.Net.WebClient
            $webClient.Encoding = [System.Text.Encoding]::UTF8
            $webClient.Headers.Add("Authorization", "Bearer $sgdbApiKey")
            $sgdbSearchRequest = $webClient.DownloadString($requestUri) | ConvertFrom-Json
            $webClient.Dispose()
        } catch {
            $webClient.Dispose()
            $errorMessage = $_.Exception.Message
            $errorCode = $_.Exception.InnerException.Response.StatusCode
            if ($errorCode -eq "Unauthorized")
            {
                # 401 Status Code Error handling
                $__logger.Info("Error in SteamGridDB API Request. Configured SteamGridDB API Key is invalid.")
                $PlayniteApi.Dialogs.ShowErrorMessage("Error in SteamGridDB API Request.`nConfigured SteamGridDB API Key is invalid.", "Extra Metadata Tools") | Out-Null
                exit
            }
            else
            {
                $__logger.Info("Error in SteamGridDB API Request `"$requestUri`". Error: $errorMessage")
                return $null
            }
        }

        if ($sgdbSearchRequest.data.Count -gt 0)
        {
            $requestUri = "https://www.steamgriddb.com/api/v2/logos/game/{0}" -f $sgdbSearchRequest.data[0].id
            return $requestUri
        }
        else
        {
            return $null
        }
    }
}

function Get-SgdbLogo
{
    $sgdbApiKey = Get-SgdbApiKey
    if ([string]::IsNullOrEmpty($sgdbApiKey))
    {
        $PlayniteApi.Dialogs.ShowMessage("Couldn't get SteamGridDB API Key. Please configure it before continuing.", "Extra Metadata tools")
        return
    }
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    $downloadedLogos = 0
    foreach ($game in $gameDatabase) {
        $extraMetadataDirectory = Set-GameDirectory $game
        $logoPath = Join-Path $extraMetadataDirectory -ChildPath "Logo.png"
        if (Test-Path $logoPath)
        {
            continue
        }
        $requestUri = Get-SgdbRequestUrl $game $sgdbApiKey
        if ([string]::IsNullOrEmpty($requestUri))
        {
            continue
        }
        
        try {
            $webClient = New-Object System.Net.WebClient
            $webClient.Encoding = [System.Text.Encoding]::UTF8
            $webClient.Headers.Add("Authorization", "Bearer $sgdbApiKey")
            $sgdbRequest = $webClient.DownloadString($requestUri) | ConvertFrom-Json
            $webClient.Dispose()
        } catch {
            $webClient.Dispose()
            $errorMessage = $_.Exception.Message
            $errorCode = $_.Exception.InnerException.Response.StatusCode
            if ($errorCode -eq "NotFound")
            {
                # 404 Status Code Error handling
                continue
            }
            elseif ($errorCode -eq "Unauthorized")
            {
                # 401 Status Code Error handling
                $__logger.Info("Error in SteamGridDB API Request. Configured SteamGridDB API Key is invalid.")
                $PlayniteApi.Dialogs.ShowErrorMessage("Error in SteamGridDB API Request.`nConfigured SteamGridDB API Key is invalid.", "Extra Metadata Tools")
                break
            }
            else
            {
                $__logger.Info("Error in SteamGridDB API Request `"$requestUri`". Error: $errorMessage")
                $PlayniteApi.Dialogs.ShowErrorMessage("Error in SteamGridDB API Request `"$requestUri`". Error: $errorMessage", "Extra Metadata Tools")
                break
            }
        }

        if ($sgdbRequest.data.Count -gt 0)
        {
            if ([string]::IsNullOrEmpty($sgdbRequest.data[0].url))
            {
                continue
            }
            else
            {
                try {
                    Start-Sleep -Seconds 1
                    $url = $sgdbRequest.data[0].url
                    $webClient = New-Object System.Net.WebClient
                    $webClient.Encoding = [System.Text.Encoding]::UTF8
                    $webClient.DownloadFile($url, $logoPath)
                    $webClient.Dispose()
                    $downloadedLogos++
                } catch {
                    $webClient.Dispose()
                    $errorMessage = $_.Exception.Message
                    $__logger.Info("Error downloading file `"$url`" file from SteamGridDB. Error: $errorMessage")
                    continue
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