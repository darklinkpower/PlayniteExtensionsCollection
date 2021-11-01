function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemSetProfilePictureDescription")
    $menuItem1.FunctionName = "Set-ProfilePicture"
    $menuItem1.MenuSection = "@Extra Metadata|Themes"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemDetectDeleteUnusedDataDescription")
    $menuItem2.FunctionName = "Invoke-DetectAndDeleteUnused"
    $menuItem2.MenuSection = "@Extra Metadata"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem3.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemSetProfilePictureDescription")
    $menuItem3.FunctionName = "Set-ProfilePicture"
    $menuItem3.MenuSection = "@Extra Metadata|Themes"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem4.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemSetBackgroundVideoDescription")
    $menuItem4.FunctionName = "Set-BackgroundVideo"
    $menuItem4.MenuSection = "@Extra Metadata|Themes"

    return $menuItem1, $menuItem2, $menuItem3, $menuItem4
}

function GetGameMenuItems
{
    param(
        $scriptGameMenuItemActionArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem1.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemSelectLocalLogoDescription")
    $menuItem1.FunctionName = "Get-SteamLogosLocal"
    $menuItem1.MenuSection = "Extra Metadata|Logos"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemGetLogosFromGoogleDescription")
    $menuItem2.FunctionName = "Get-GoogleLogo"
    $menuItem2.MenuSection = "Extra Metadata|Logos"
    
    return $menuItem1, $menuItem2
}

function Invoke-DetectAndDeleteUnused
{
    param(
        $scriptGameMenuItemActionArgs
    )

    $baseDirectory = [System.IO.Path]::Combine($PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata")
    
    $deletedCount = 0
    ### games ###
    $gamesRoot = [System.IO.Path]::Combine($baseDirectory, "games")
    if ([System.IO.Directory]::Exists($gamesRoot))
    {
        $gameIds = @{}
        foreach ($game in $PlayniteApi.Database.Games) {
            $gameIds.add($game.Id.ToString(), "")
        }
    
        foreach ($directory in Get-ChildItem $gamesRoot -Directory) {
            if ($null -eq $gameIds[$directory.Name])
            {
                Remove-Item $directory.FullName -Recurse
                $deletedCount++
            }
        }
    }

    ### platforms ###
    $platformsRoot = [System.IO.Path]::Combine($baseDirectory, "platforms")
    if ([System.IO.Directory]::Exists($platformsRoot))
    {
        $platformIds = @{}
        foreach ($platform in $PlayniteApi.Database.Platforms) {
            $platformIds.add($platform.Id.ToString(), "")
        }

        foreach ($directory in Get-ChildItem $platformsRoot -Directory) {
            if ($null -eq $platformIds[$directory.Name])
            {
                Remove-Item $directory.FullName -Recurse
                $deletedCount++
            }
        }
    }
    
    ### source ###
    $sourcesRoot = [System.IO.Path]::Combine($baseDirectory, "sources")
    if ([System.IO.Directory]::Exists($sourcesRoot))
    {
        $sourceIds = @{}
        foreach ($source in $PlayniteApi.Database.Sources) {
            $sourceIds.add($source.Id.ToString(), "")
        }
        
        foreach ($directory in Get-ChildItem $sourcesRoot -Directory)
        {
            if ($null -eq $sourceIds[$directory.Name])
            {
                Remove-Item $directory.FullName -Recurse
                $deletedCount++
            }
        }
    }

    $PlayniteApi.Dialogs.ShowMessage(
        ([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MenuItemDetectDeleteUnusedDataResultsMessage") -f $deletedCount), 
        "Extra Metadata Tools")
}

function OnApplicationStarted
{
    if ($PlayniteApi.ApplicationInfo.Mode -eq "Desktop")
    {
        return;
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
                $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_ThemeConstantsUpdatedMessage") -f $themeInUse), "Extra Metadata Tools")
            }
        }
    }
}

function Get-GoogleResultsArray
{
    param (
        [string]$queryInput,
        [bool]$transparentImages
    )

    $query = [uri]::EscapeDataString($queryInput)
    $uri = ""
    if ($transparentImages)
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
    $regexMatch = ([regex]$regex).Matches($googleContent)
    if ($null -ne $regexMatch)
    {
        $regexMatch = $regexMatch | Select-Object -First 30
    }
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
    param(
        $scriptGameMenuItemActionArgs
    )
    
    $gameDatabase = $scriptGameMenuItemActionArgs.Games
    if ($gameDatabase.count -gt 1)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MoreThanSingleGameSelectedMessage")), "Extra Metadata tools");
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
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <DockPanel Grid.Row="0" Margin="0">
            <Grid DockPanel.Dock="Left">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                </Grid.ColumnDefinitions>
                <TextBox  Name="TextboxSearch" Grid.Column="0" HorizontalContentAlignment="Stretch"/>
                <Button  Grid.Column="1" Margin="10,0,0,0" Name="ButtonImageSearch"
                        Content="Search" HorizontalAlignment="Right" IsDefault="True"/>
            </Grid>
        </DockPanel>
        <DockPanel Grid.Row="1" Margin="0,10,0,0">        
            <CheckBox Name="CheckboxTransparent" IsChecked="True"
                    DockPanel.Dock="Left" VerticalAlignment="Center"/>
        </DockPanel>
        <ListBox Grid.Row="2" Name="ListBoxImages" Margin="0,20,0,0"
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
        <Button Grid.Row="3" Content="Download selected Logo" HorizontalAlignment="Center" Margin="0,20,0,0" Name="ButtonDownloadLogo" IsDefault="False"/>
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
        $CheckboxTransparent.Content = "Search only for images with transparency"

        # Set Window creation options
        $windowCreationOptions = New-Object Playnite.SDK.WindowCreationOptions
        $windowCreationOptions.ShowCloseButton = $true
        $windowCreationOptions.ShowMaximizeButton = $False
        $windowCreationOptions.ShowMinimizeButton = $False

        # Create window
        $window = $PlayniteApi.Dialogs.CreateWindow($windowCreationOptions)
        $window.Content = $XMLForm
        $window.Width = 900
        $window.Height = 600
        $window.Title = "Extra Metadata Tools - Google Logo Search"
        $window.WindowStartupLocation = "CenterScreen"

        # Handler for pressing "Search" button
        $ButtonImageSearch.Add_Click(
        {
            $ListBoxImages.ItemsSource = Get-GoogleResultsArray $TextboxSearch.Text $CheckboxTransparent.IsChecked
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
                    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_AddedLogoMessage") -f $game.Name), "Extra Metadata tools")
                } catch {
                    $webClient.Dispose()
                    $errorMessage = $_.Exception.Message
                    $__logger.Info("Error downloading file `"$logoUri`". Error: $errorMessage")
                    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_GenericFileDownloadErrorMessage") -f $logoUri, $errorMessage), "Extra Metadata tools")
                }
            }
        })

        # Show Window
        $window.ShowDialog()
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

function Set-FullscreenThemesDirectory
{
    $directory = $PlayniteApi.Paths.ConfigurationPath + "\ExtraMetadata\Themes\Fullscreen\"
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

function Get-SteamLogosLocal
{
    param(
        $scriptGameMenuItemActionArgs
    )
    
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    if ($gameDatabase.count -gt 1)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_MoreThanOneGameSelectedMessage"), "Extra Metadata tools")
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
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_SelectLogoResultsMessage") -f $game.Name), "Extra Metadata tools");
    }
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
            $PlayniteApi.Dialogs.ShowErrorMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_ImageConversionErrorMessage") -f $errorMessage), "Extra Metadata Tools")
            return
        }
    }
    $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_SetProfilePictureResultsMessage"), "Extra Metadata tools")
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
    $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_SetBackgroundVideoResultsMessage"), "Extra Metadata tools")
}