function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Open video manager"
    $menuItem1.FunctionName = "Invoke-OpenVideoManagerWindow"
    $menuItem1.MenuSection = "@Splash Screen"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "View settings"
    $menuItem2.FunctionName = "Invoke-ViewSettings"
    $menuItem2.MenuSection = "@Splash Screen"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem3.Description = "Add image splash screen skip feature to selected games"
    $menuItem3.FunctionName = "Add-ImageSkipFeature"
    $menuItem3.MenuSection = "@Splash Screen|Exclude functions"

    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem4.Description = "Remove image splash screen skip feature from selected games"
    $menuItem4.FunctionName = "Remove-ImageSkipFeature"
    $menuItem4.MenuSection = "@Splash Screen|Exclude functions"

    return $menuItem1, $menuItem2, $menuItem3, $menuItem4
}

function Invoke-ViewSettings
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $settings = Get-Settings

    # Load assemblies
    Add-Type -AssemblyName PresentationCore
    Add-Type -AssemblyName PresentationFramework
    
    # Set Xaml
    [xml]$Xaml = @"
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <Grid.Resources>
        <Style TargetType="TextBlock" BasedOn="{StaticResource BaseTextBlockStyle}" />
    </Grid.Resources>

    <DockPanel Margin="20">
        <DockPanel DockPanel.Dock="Bottom" LastChildFill="False">
            <Button Name="ButtonCancel" Margin="10,0,0,0" DockPanel.Dock="Right" VerticalAlignment="Bottom"/>
            <Button Name="ButtonSave" Margin="0,0,0,0" DockPanel.Dock="Right" VerticalAlignment="Bottom"/>
        </DockPanel>
        <StackPanel Grid.Row="0" DockPanel.Dock="Top">
            <CheckBox Name="CBexecuteInDesktopMode" Margin="0,10,0,0"/>
            <CheckBox Name="CBviewImageSplashscreenDesktopMode" Margin="0,10,0,0"/>
            <CheckBox Name="CBviewVideoDesktopMode" Margin="0,10,0,0"/>
            <CheckBox Name="CBcloseSplashScreenDesktopMode" Margin="0,10,0,0"/>
            <CheckBox Name="CBexecuteInFullscreenMode" Margin="0,20,0,0"/>
            <CheckBox Name="CBviewImageSplashscreenFullscreenMode" Margin="0,10,0,0"/>
            <CheckBox Name="CBviewVideoFullscreenMode" Margin="0,10,0,0"/>
            <CheckBox Name="CBcloseSplashScreenFullscreenMode" Margin="0,10,0,0"/>
            <CheckBox Name="CBshowLogoInSplashscreen" Margin="0,20,0,0"/>
            <DockPanel Margin="0,10,0,0">
                <TextBlock Name="TextBlockLogoPosition" DockPanel.Dock="Left" VerticalAlignment="Center"/>
                <ComboBox Name="ComboBoxLogoPosition" DockPanel.Dock="Left" Width="Auto" MinWidth="150" 
                        HorizontalAlignment="Left" VerticalAlignment="Center" DisplayMemberPath="Name" SelectedValuePath="Value" Margin="10,0,0,0"/>
            </DockPanel>
            <DockPanel Margin="0,10,0,0">
                <TextBlock Name="TextBlockLogoVerticalAlignment" VerticalAlignment="Center"/>
                <ComboBox Name="ComboBoxLogoVerticalAlignment" DockPanel.Dock="Left" Width="Auto" MinWidth="150" 
                        HorizontalAlignment="Left" VerticalAlignment="Center" DisplayMemberPath="Name" SelectedValuePath="Value" Margin="10,0,0,0"/>
            </DockPanel>
            <CheckBox Name="CBuseBlackSplashscreen" Margin="0,20,0,0"/>
        </StackPanel>
    </DockPanel>
</Grid>
"@

    # Load the xaml for controls
    $XMLReader = [System.Xml.XmlNodeReader]::New($Xaml)
    $XMLForm = [Windows.Markup.XamlReader]::Load($XMLReader)

    # Make variables for each control
    $Xaml.FirstChild.SelectNodes("//*[@Name]") | ForEach-Object {Set-Variable -Name $_.Name -Value $XMLForm.FindName($_.Name) }

    # Control IsEnabled events
    $CBexecuteInDesktopMode.Add_Checked(
    {
        $CBviewImageSplashscreenDesktopMode.IsEnabled = $true
        $CBviewVideoDesktopMode.IsEnabled = $true
        $CBcloseSplashScreenDesktopMode.IsEnabled = $true
    })

    $CBexecuteInDesktopMode.Add_Unchecked(
    {
        $CBviewImageSplashscreenDesktopMode.IsEnabled = $false
        $CBviewVideoDesktopMode.IsEnabled = $false
        $CBcloseSplashScreenDesktopMode.IsEnabled = $false
    })

    $CBexecuteInFullscreenMode.Add_Checked(
    {
        $CBviewImageSplashscreenFullscreenMode.IsEnabled = $true
        $CBviewVideoFullscreenMode.IsEnabled = $true
        $CBcloseSplashScreenFullscreenMode.IsEnabled = $true
    })

    $CBexecuteInFullscreenMode.Add_Unchecked(
    {
        $CBviewImageSplashscreenFullscreenMode.IsEnabled = $false
        $CBviewVideoFullscreenMode.IsEnabled = $false
        $CBcloseSplashScreenFullscreenMode.IsEnabled = $false
    })

    $CBshowLogoInSplashscreen.Add_Checked(
    {
        $TextBlockLogoPosition.IsEnabled = $true
        $ComboBoxLogoPosition.IsEnabled = $true
        $TextBlockLogoVerticalAlignment.IsEnabled = $true
        $ComboBoxLogoVerticalAlignment.IsEnabled = $true
    })

    $CBshowLogoInSplashscreen.Add_Unchecked(
    {
        $TextBlockLogoPosition.IsEnabled = $false
        $ComboBoxLogoPosition.IsEnabled = $false
        $TextBlockLogoVerticalAlignment.IsEnabled = $false
        $ComboBoxLogoVerticalAlignment.IsEnabled = $false
    })

    # Set items sources of controls
    $CBexecuteInDesktopMode.Content = "Execute extension in Desktop Mode"
    $CBexecuteInDesktopMode.IsChecked = $true
    $CBexecuteInDesktopMode.IsChecked = $settings.executeInDesktopMode

    $CBviewImageSplashscreenDesktopMode.Content = "View splashscreen images in Desktop Mode"
    $CBviewImageSplashscreenDesktopMode.IsChecked = $settings.viewImageSplashscreenDesktopMode

    $CBviewVideoDesktopMode.Content = "View intro videos in Desktop Mode"
    $CBviewVideoDesktopMode.IsChecked = $settings.viewVideoDesktopMode

    $CBcloseSplashScreenDesktopMode.Content = "Automatically close splashscreen in Desktop Mode (Disabling hides desktop when game closes but can cause issues)"
    $CBcloseSplashScreenDesktopMode.IsChecked = $settings.closeSplashScreenDesktopMode

    $CBexecuteInFullscreenMode.Content = "Execute extension in Fullscreen Mode"
    $CBexecuteInFullscreenMode.IsChecked = $true
    $CBexecuteInFullscreenMode.IsChecked = $settings.executeInFullscreenMode

    $CBviewImageSplashscreenFullscreenMode.Content = "View splashscreen images in Fullscreen Mode"
    $CBviewImageSplashscreenFullscreenMode.IsChecked = $settings.viewImageSplashscreenFullscreenMode
    
    $CBviewVideoFullscreenMode.Content = "View intro videos in Fullscreen Mode"
    $CBviewVideoFullscreenMode.IsChecked = $settings.viewVideoFullscreenMode

    $CBcloseSplashScreenFullscreenMode.Content = "Automatically close splashscreen in Fullscreen Mode (Disabling hides desktop when game closes but can cause issues)"
    $CBcloseSplashScreenFullscreenMode.IsChecked = $settings.closeSplashScreenFullscreenMode

    $CBshowLogoInSplashscreen.Content = "Add game logo in splashscreen image if available"
    $CBshowLogoInSplashscreen.IsChecked = $true
    $CBshowLogoInSplashscreen.IsChecked = $settings.showLogoInSplashscreen

    $CBuseBlackSplashscreen.Content = "Use black splashscreen instead of the splashscreen image"
    $CBuseBlackSplashscreen.IsChecked = $settings.useBlackSplashscreen

    [System.Collections.ArrayList]$ComboBoxLogoPositionSource = @(
        [PSCustomObject]@{
            Name = "Center"
            Value = "Center"
        },
        [PSCustomObject]@{
            Name = "Left"
            Value = "Left"
        },
        [PSCustomObject]@{
            Name = "Right"
            Value = "Right"
        }
    )

    [System.Collections.ArrayList]$ComboBoxLogoVerticalAlignmentSource = @(
        [PSCustomObject]@{
            Name = "Top"
            Value = "Top"
        },
        [PSCustomObject]@{
            Name = "Center"
            Value = "Center"
        },
        [PSCustomObject]@{
            Name = "Bottom"
            Value = "Bottom"
        }
    )

    $TextBlockLogoPosition.Text = "Logo horizontal position:"
    $ComboBoxLogoPosition.ItemsSource = $ComboBoxLogoPositionSource
    $ComboBoxLogoPosition.SelectedValue = $settings.logoPosition

    $TextBlockLogoVerticalAlignment.Text = "Logo vertical position:"
    $ComboBoxLogoVerticalAlignment.ItemsSource = $ComboBoxLogoVerticalAlignmentSource
    $ComboBoxLogoVerticalAlignment.SelectedValue = $settings.logoVerticalAlignment

    $ButtonSave.Content = "Save"
    $ButtonCancel.Content = "Cancel"

    # Set Window creation options
    $windowCreationOptions = New-Object Playnite.SDK.WindowCreationOptions
    $windowCreationOptions.ShowCloseButton = $true
    $windowCreationOptions.ShowMaximizeButton = $False
    $windowCreationOptions.ShowMinimizeButton = $False

    # Create window
    $window = $PlayniteApi.Dialogs.CreateWindow($windowCreationOptions)
    $window.Content = $XMLForm
    $window.Width = 800
    $window.Height = 550
    $window.Title = "Splash Screen - Settings"
    $window.WindowStartupLocation = "CenterScreen"

    # Handler for pressing "Cancel" button
    $ButtonCancel.Add_Click(
    {
        $window.Close()
    })

    # Handler for pressing "Save" button
    $ButtonSave.Add_Click(
    {
        $settings.executeInDesktopMode = $CBexecuteInDesktopMode.IsChecked
        $settings.viewImageSplashscreenDesktopMode = $CBviewImageSplashscreenDesktopMode.IsChecked
        $settings.viewVideoDesktopMode = $CBviewVideoDesktopMode.IsChecked
        $settings.closeSplashScreenDesktopMode = $CBcloseSplashScreenDesktopMode.IsChecked
        $settings.executeInFullscreenMode = $CBexecuteInFullscreenMode.IsChecked
        $settings.viewImageSplashscreenFullscreenMode = $CBviewImageSplashscreenFullscreenMode.IsChecked
        $settings.viewVideoFullscreenMode = $CBviewVideoFullscreenMode.IsChecked
        $settings.closeSplashScreenFullscreenMode = $CBcloseSplashScreenFullscreenMode.IsChecked
        $settings.showLogoInSplashscreen = $CBshowLogoInSplashscreen.IsChecked
        $settings.logoPosition = $ComboBoxLogoPosition.SelectedValue
        $settings.logoVerticalAlignment = $ComboBoxLogoVerticalAlignment.SelectedValue
        $settings.useBlackSplashscreen = $CBuseBlackSplashscreen.IsChecked

        Save-Settings $settings
        $window.Close()
    })

    $window.ShowDialog()
    $window = $null
    [System.GC]::Collect()
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

function Get-Settings
{
    # Set default settings values
    $settings = @{
        "executeInDesktopMode" = $false
        "viewImageSplashscreenDesktopMode" = $true
        "viewVideoDesktopMode" = $false
        "closeSplashScreenDesktopMode" = $true
        "executeInFullscreenMode" = $true
        "viewImageSplashscreenFullscreenMode" = $true
        "viewVideoFullscreenMode" = $true
        "closeSplashScreenFullscreenMode" = $true
        "showLogoInSplashscreen" = $false
        "logoPosition" = "Center"
        "logoVerticalAlignment" = "Center"
        "useBlackSplashscreen" = $false
    }
    
    $settingsPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath "Settings.json"
    if ([System.IO.File]::Exists($settingsPath))
    {
        $savedSettings = [System.IO.File]::ReadAllLines($settingsPath) | ConvertFrom-Json
        foreach ($setting in $savedSettings.PSObject.Properties) {
            try {
                $settings.($setting.Name) = $setting.Value
            } catch {}
        }
    }

    return $settings
}

function Set-Video
{
    param (
        $videoDestinationPath
    )
    
    $videoSourcePath = $PlayniteApi.Dialogs.SelectFile("mp4|*.mp4")
    if ([string]::IsNullOrEmpty($videoSourcePath))
    {
        return
    }

    # In case source video is the same as target
    if ($videoSourcePath -eq $videoDestinationPath)
    {
        return
    }
    
    $directory = [System.IO.Path]::GetDirectoryName($videoDestinationPath)
    if (![System.IO.Directory]::Exists($directory))
    {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    
    Copy-Item $videoSourcePath $videoDestinationPath -Force
    $PlayniteApi.Dialogs.ShowMessage("Intro video has been added", "Splash Screen")
}

function Remove-IntroVideo
{
    param (
        $videoSourcePath
    )
    
    Remove-Item $videoSourcePath -Force
    $PlayniteApi.Dialogs.ShowMessage("Intro video has been removed.", "Splash Screen")
}

function Invoke-OpenVideoManagerWindow
{
    param(
        $scriptMainMenuItemActionArgs
    )

    $videoPathTemplate = [System.IO.Path]::Combine($PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "{0}", "{1}", "VideoIntro.mp4")

    [System.Collections.ArrayList]$selectedGames = @()
    $PlayniteApi.MainView.SelectedGames | Sort-Object -Property "Name" | Select-Object -Unique | ForEach-Object {
        $game = [PSCustomObject]@{
            Name = $_.Name
            Value = $videoPathTemplate -f "games", $_.Id.ToString()
        }
        $selectedGames.Add($game) | Out-Null
    }

    [System.Collections.ArrayList]$sources = @()
    $PlayniteApi.Database.Sources | Sort-Object -Property "Name" | ForEach-Object {
        $source = [PSCustomObject]@{
            Name = $_.Name
            Value = $videoPathTemplate -f "sources", $_.Id.ToString()
        }
        $sources.Add($source) | Out-Null
    }

    [System.Collections.ArrayList]$platforms = @()
    $PlayniteApi.Database.Platforms | Sort-Object -Property "Name" | ForEach-Object {
        $platform = [PSCustomObject]@{
            Name = $_.Name
            Value = $videoPathTemplate -f "platforms", $_.Id.ToString()
        }
        $platforms.Add($platform) | Out-Null
    }

    [System.Collections.ArrayList]$playniteModes = @()
    $mode = [PSCustomObject]@{
        Name = "Desktop"
        Value = $videoPathTemplate -f "playnite", "Desktop"
    }
    $playniteModes.Add($mode) | Out-Null
    $mode = [PSCustomObject]@{
        Name = "Fullscreen"
        Value = $videoPathTemplate -f "playnite", "Fullscreen"
    }
    $playniteModes.Add($mode) | Out-Null

    $comboBoxCollectionSource = [ordered]@{
        "Games" = "Selected games"
        "Sources" = "Sources"
        "Plaforms" = "Plaforms"
        "Playnite Mode" = "Playnite Mode"
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

    <StackPanel Margin="20">
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="300" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>
            <StackPanel Grid.Column="0">
                <TextBlock Text="Collection:" Margin="0,0,0,0"/>
                <ComboBox Name="ComboBoxCollections" DisplayMemberPath="Value" SelectedIndex="0" Margin="0,10,0,0"/>
                <ListBox Name="ListBoxSelectedCollection" Height="500" HorizontalContentAlignment="Stretch" DisplayMemberPath="Name" Margin="0,10,0,0"/>
            </StackPanel>
            <StackPanel Grid.Column="1" Margin="10,0,0,0">
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="255" />
                        <RowDefinition Height="*" />
                    </Grid.RowDefinitions>
                    <Grid Grid.Row="0" Background="Black">
                        <TextBlock Name="TextBlockVideoAvailable" Margin="0,0,0,0" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                        <MediaElement Name="VideoPlayer" Margin="0" MediaElementBehaviors.Repeat="True"/>
                    </Grid>
                    <StackPanel Grid.Row="1" Margin="0,10,0,0">
                        <DockPanel>
                            <Button Name="ButtonAddVideo" Visibility="Hidden" Content="Add video" Margin="0,0,0,0" HorizontalAlignment="Left"/>
                            <Button Name="ButtonRemoveVideo" Visibility="Hidden" Content="Remove video" Margin="10,0,0,0" HorizontalAlignment="Left"/>
                        </DockPanel>
                    </StackPanel>
                </Grid>
            </StackPanel>
        </Grid>
    </StackPanel>
</Grid>
"@

    # Load the xaml for controls
    $XMLReader = [System.Xml.XmlNodeReader]::New($Xaml)
    $XMLForm = [Windows.Markup.XamlReader]::Load($XMLReader)

    # Make variables for each control
    $Xaml.FirstChild.SelectNodes("//*[@Name]") | ForEach-Object {Set-Variable -Name $_.Name -Value $XMLForm.FindName($_.Name) }

    # Set items sources of controls
    $ListBoxSelectedCollection.ItemsSource = $selectedGames
    $ComboBoxCollections.ItemsSource = $comboBoxCollectionSource
    $TextBlockVideoAvailable.Text = "Video not available"
    
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
    $window.Title = "Splash Screen - Video manager"
    $window.WindowStartupLocation = "CenterScreen"

    # Handler for ComboBoxCollections
    $ComboBoxCollections.Add_SelectionChanged(
        {
            switch ($ComboBoxCollections.SelectedItem.Name) {
                "Games" {$ListBoxSelectedCollection.ItemsSource = $selectedGames}
                "Sources" {$ListBoxSelectedCollection.ItemsSource = $sources}
                "Plaforms" {$ListBoxSelectedCollection.ItemsSource = $platforms}
                "Playnite Mode" {$ListBoxSelectedCollection.ItemsSource = $playniteModes}
                default {}
            }
    
            $ButtonAddVideo.Visibility = "Hidden"
        })

    # Handler for ListBoxPlatforms
    $ListBoxSelectedCollection.Add_SelectionChanged(
    {
        $ButtonAddVideo.Visibility = "Visible"
        $videoPath = $ListBoxSelectedCollection.SelectedItem.Value
        if ([System.IO.File]::Exists($videoPath))
        {
            $ButtonRemoveVideo.Visibility = "Visible"
            $TextBlockVideoAvailable.Visibility = "Hidden"
            $VideoPlayer.Source = $videoPath
            $VideoPlayer.Play()
        }
        else
        {
            $ButtonRemoveVideo.Visibility = "Hidden"
            $TextBlockVideoAvailable.Visibility = "Visible"
            $VideoPlayer.Source = $null
        }
    })

    # Handler for pressing "Add Video" button
    $ButtonAddVideo.Add_Click(
    {
        $VideoPlayer.Source = $null
        $videoPath = $ListBoxSelectedCollection.SelectedItem.Value
        Set-Video $videoPath
        if ([System.IO.File]::Exists($videoPath))
        {
            $ButtonRemoveVideo.Visibility = "Visible"
            $TextBlockVideoAvailable.Visibility = "Hidden"
            $VideoPlayer.Source = $videoPath
            $VideoPlayer.Play()
        }
    })

    # Handler for pressing "Remove Video" button
    $ButtonRemoveVideo.Add_Click(
    {
        $VideoPlayer.Source = $null
        $videoPath = $ListBoxSelectedCollection.SelectedItem.Value
        Remove-IntroVideo $videoPath
        if(![System.IO.File]::Exists($videoPath))
        {
            $ButtonRemoveVideo.Visibility = "Hidden"
            $TextBlockVideoAvailable.Visibility = "Visible"
            $VideoPlayer.Source = $null
        }
    })

    $window.Add_Closing(
    {
        $VideoPlayer.Source = $null
    })

    $window.ShowDialog()
    $window = $null
    [System.GC]::Collect()
}

function Get-SplashVideoPath
{
    param (
        [Playnite.SDK.Models.Game] $game
    )

    $videoPathTemplate = [System.IO.Path]::Combine($PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "{0}", "{1}", "VideoIntro.mp4")

    $splashVideo = $videoPathTemplate -f "games", $game.Id.ToString()
    if ([System.IO.File]::Exists($splashVideo))
    {
        $__logger.Info(("Specific game video found in {0}." -f $splashVideo))
        return $splashVideo
    }

    if ($null -ne $game.Source)
    {
        $splashVideo = $videoPathTemplate -f "sources", $game.Source.Id.ToString()
        if ([System.IO.File]::Exists($splashVideo))
        {
            $__logger.Info(("Source video found in {0}." -f $splashVideo))
            return $splashVideo
        }
    }
    
    if ($null -ne $game.Platform)
    {
        $splashVideo = $videoPathTemplate -f "platforms", $game.Platform.Id.ToString()
        if ([System.IO.File]::Exists($splashVideo))
        {
            $__logger.Info(("Platform video found in {0}." -f $splashVideo))
            return $splashVideo
        }
    }
    
    $splashVideo = $videoPathTemplate -f "playnite", $PlayniteApi.ApplicationInfo.Mode
    if ([System.IO.File]::Exists($splashVideo))
    {
        $__logger.Info(("Playnite mode video found in {0}." -f $splashVideo))
        return $splashVideo
    }

    $__logger.Info(("Video not found"))
    return $null
}

function Get-SplashImagePath
{
    param (
        [Playnite.SDK.Models.Game] $game
    )

    if ($game.BackgroundImage)
    {
        if ($game.BackgroundImage -notmatch "^http")
        {
            $__logger.Info(("Found game background image"))
            return $PlayniteApi.Database.GetFullFilePath($game.BackgroundImage)
        }
    }

    if ($game.Platform)
    {
        if ($game.Platform.Background)
        {
            $__logger.Info(("Found platform background image"))
            return $PlayniteApi.Database.GetFullFilePath($game.Platform.Background)
        }
    }

    if ($PlayniteApi.ApplicationInfo.Mode -eq "Desktop")
    {
        $__logger.Info(("Using generic Desktop mode background image"))
        return [System.IO.Path]::Combine($CurrentExtensionInstallPath, "SplashScreenDesktopMode.png")
    }
    else
    {
        $__logger.Info(("Using generic Fullscreen mode background image"))
        return [System.IO.Path]::Combine($CurrentExtensionInstallPath, "SplashScreenFullscreenMode.png")
    }
}

function OnGameStarting
{
    param(
        $OnGameStartingEventArgs
    )

    $game = $OnGameStartingEventArgs
    $settings = Get-Settings
    
    if (($PlayniteApi.ApplicationInfo.Mode -eq "Desktop") -and ($settings.executeInDesktopMode -eq $false))
    {
        $__logger.Info(("Execution disabled for Desktop mode in settings" -f $game.Name))
        return
    }
    elseif (($PlayniteApi.ApplicationInfo.Mode -eq "Fullscreen") -and ($settings.executeInFullscreenMode -eq $false))
    {
        $__logger.Info(("Execution disabled for Fullscreen mode in settings" -f $game.Name))
        return
    }

    $__logger.Info(("Game: {0}" -f $game.Name))

    $skipSplashImage = $false
    if ((($PlayniteApi.ApplicationInfo.Mode -eq "Desktop") -and ($settings.viewImageSplashscreenDesktopMode -eq $false)) -or (($PlayniteApi.ApplicationInfo.Mode -eq "Fullscreen") -and ($settings.viewImageSplashscreenFullscreenMode -eq $false)))
    {
        $skipSplashImage = $true
        $__logger.Info(("Splashscreen image is disabled for {0} mode" -f $PlayniteApi.ApplicationInfo.Mode.ToString()))
    }
    elseif ($game.features)
    {
        foreach ($feature in $game.features) {
            if ($feature.Name -eq "[Splash Screen] Skip splash image")
            {
                $__logger.Info(("Game has exclude filter feature"))
                $skipSplashImage = $true
                break
            }
        }
    }

    if ($skipSplashImage -eq $false)
    {
        if ($settings.useBlackSplashscreen -eq $true)
        {
            $splashImage = [System.IO.Path]::Combine($CurrentExtensionInstallPath, "SplashScreenBlack.png")
        }
        else
        {
            $splashImage = Get-SplashImagePath $game
        }

        switch ($PlayniteApi.ApplicationInfo.Mode.ToString()) {
            "Desktop" { $closeSplashScreenAutomatic = $settings.closeSplashScreenDesktopMode}
            Default { $closeSplashScreenAutomatic = $settings.closeSplashScreenFullscreenMode }
        }
        
        $logoPath = ""
        if ($settings.showLogoInSplashscreen -eq $true)
        {
            $logoPath = [System.IO.Path]::Combine($PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", $game.Id, "Logo.png")
        }

        @($splashImage, $logoPath, $closeSplashScreenAutomatic, $settings.logoPosition, $settings.logoVerticalAlignment) | ConvertTo-Json | Out-File (Join-Path $env:TEMP -ChildPath "SplashScreen.json")
    }
    
    if ((($PlayniteApi.ApplicationInfo.Mode -eq "Desktop") -and ($settings.viewVideoDesktopMode -eq $true)) -or (($PlayniteApi.ApplicationInfo.Mode -eq "Fullscreen") -and ($settings.executeInFullscreenMode -eq $true)))
    {
        $splashVideo = Get-SplashVideoPath $game
        if ($null -ne $splashVideo)
        {
            Invoke-VideoSplashScreen $splashVideo $skipSplashImage
        }
        elseif ($skipSplashImage -eq $false)
        {
            Invoke-ImageSplashScreen
        }
    }
    elseif ($skipSplashImage -eq $false)
    {
        Invoke-ImageSplashScreen
    }
}

function Invoke-ImageSplashScreen
{
    Start-Job -Name "SplashScreen" -ScriptBlock{
        powershell.exe -WindowStyle Hidden {
            $splashPaths = Get-Content (Join-Path $env:TEMP -ChildPath "SplashScreen.json") | ConvertFrom-Json
            $splashImage = $splashPaths[0]
            $logoPath = $splashPaths[1]
            $closeSplashScreenAutomatic = $splashPaths[2]
            $logoPosition = $splashPaths[3]
            $logoVerticalAlignment = $splashPaths[4]
            
            # Load assemblies
            Add-Type -AssemblyName PresentationCore
            Add-Type -AssemblyName PresentationFramework

            # Set Xaml
            [xml]$XAML = @"
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    Title="PlayniteSplashScreenExtension" WindowStyle="None" ResizeMode="NoResize" WindowState="Maximized">
                <Grid Margin="0" Background="Black">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>
                    <Image Name="BackgroundImage" Stretch="UniformToFill"
                            HorizontalAlignment="Center" VerticalAlignment="Center" Grid.Column="0" Grid.ColumnSpan="3"/>
                    <Image Name="LogoImage" Source="" Stretch="Uniform"
                        HorizontalAlignment="Center" Grid.Column="0" Grid.ColumnSpan="1" Margin="20">
                        <Image.Effect>
                            <DropShadowEffect Direction="0" Color="#FF000000" ShadowDepth="0" BlurRadius="40" />
                        </Image.Effect>
                    </Image>
                    <Border Background="Black" Grid.Column="0" Grid.ColumnSpan="3">
                        <Border.Style>
                            <Style TargetType="Border">
                                <Setter Property="Opacity" Value="1.0" />
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding RelativeSource={RelativeSource Self}, Path=IsVisible}" Value="True">
                                        <DataTrigger.EnterActions>
                                            <BeginStoryboard>
                                                <Storyboard>
                                                    <DoubleAnimation Storyboard.TargetProperty="Opacity" To="0.0" Duration="0:0:1.9" />
                                                </Storyboard>
                                            </BeginStoryboard>
                                        </DataTrigger.EnterActions>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </Border.Style>
                    </Border>
                </Grid>
            </Window>
"@
        
            # Load the xaml for controls
            $XMLReader = [System.Xml.XmlNodeReader]::New($Xaml)
            $window = [Windows.Markup.XamlReader]::Load($XMLReader)

            # Make variables for each control
            $Xaml.FirstChild.SelectNodes("//*[@Name]") | ForEach-Object {Set-Variable -Name $_.Name -Value $window.FindName($_.Name) }

            $BackgroundImage.Source = $splashImage
            if ([System.IO.File]::Exists($logoPath))
            {
                $logoImage.Source = $logoPath
                $logoImage.VerticalAlignment = $logoVerticalAlignment
                switch ($logoPosition) {
                    "Left" { $logoImage.SetValue([Windows.Controls.Grid]::ColumnProperty, 0) }
                    "Center" { $logoImage.SetValue([Windows.Controls.Grid]::ColumnProperty, 1) }
                    "Right" { $logoImage.SetValue([Windows.Controls.Grid]::ColumnProperty, 2) }
                    Default {}
                }
            }

            if ($closeSplashScreenAutomatic -eq $true)
            {
                $timer = [System.Windows.Threading.DispatcherTimer]::new()
                $timer.Interval = New-TimeSpan -Seconds 3
                $timeSpanLimit = New-TimeSpan -Seconds 30
                $endTime = (Get-Date).Add($timeSpanLimit)
                $timer.Start()
                $timer.Add_Tick({
                    if ((Get-Date) -ge $endTime)
                    {
                        $window.Close()
                    }
                })
            }

            # Show Window
            $window.ShowDialog() | Out-Null
            $window = $null
            [System.GC]::Collect()
            exit
        }
    }

    # Sleep time to make sure splash screen shows in cases where game loads too fast
    Start-Sleep 2
}

function Invoke-VideoSplashScreen
{
    param (
        [string] $splashVideo,
        [bool] $skipSplashImage
    )

    # Load assemblies
    Add-Type -AssemblyName PresentationCore
    Add-Type -AssemblyName PresentationFramework

    # Set Xaml
    [xml]$XAML = @"
    <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            Title="PlayniteSplashScreenExtension" WindowStyle="None" ResizeMode="NoResize" WindowState="Maximized">
        <Grid Margin="0" Background="Black">
            <MediaElement Name="VideoPlayer" Margin="0" LoadedBehavior="Manual"/>
        </Grid>
    </Window>
"@
        
    # Load the xaml for controls
    $XMLReader = [System.Xml.XmlNodeReader]::New($Xaml)
    $window = [Windows.Markup.XamlReader]::Load($XMLReader)

    # Make variables for each control
    $Xaml.FirstChild.SelectNodes("//*[@Name]") | ForEach-Object {Set-Variable -Name $_.Name -Value $window.FindName($_.Name) }

    $VideoPlayer.Volume = 100
    [uri]$videoSource = $splashVideo
    $VideoPlayer.Source = $VideoSource
    $VideoPlayer.Play()
    
    # Handler for video player
    $VideoPlayer.Add_MediaEnded({
        $VideoPlayer.Source = $null
        if ($skipSplashImage -eq $false)
        {
            Invoke-ImageSplashScreen
            Start-Sleep -Milliseconds 300
        }
        $window.Close()
    })

    # Show Window
    $window.ShowDialog() | Out-Null
    $window = $null
    [System.GC]::Collect()
}

function OnGameStopped
{
    param(
        $OnGameStoppedEventArgs
    )
    
    # Close splash window in case game was closed before maximum time limit
    Start-Job {
        $splashsScreenWindow = Get-Process | Where-Object {$_.MainWindowTitle -eq "PlayniteSplashScreenExtension"}
        if ($null -ne $splashsScreenWindow)
        {
            try {
                $splashsScreenWindow.Kill()
            } catch { }
        }
    }
}

function Add-ImageSkipFeature
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $featureName = "[Splash Screen] Skip splash image"
    $feature = $PlayniteApi.Database.Features.Add($featureName)
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    
    $featureAdded = 0
    
    foreach ($game in $gameDatabase) {
        if ($game.FeatureIds) 
        {
            if ($game.FeatureIds -contains $feature.Id)
            {
                continue
            }
            $game.FeatureIds += $feature.Id
        }
        else
        {
            # Fix in case game has null Feature
            $game.FeatureIds = $feature.Id
        }
        
        $PlayniteApi.Database.Games.Update($game)
        $featureAdded++
        $__logger.Info(("Added `"{0}`" feature to `"{1}`"." -f $featureName, $game.name))
    }
    
    $PlayniteApi.Dialogs.ShowMessage(("Added `"{0}`" exclude feature to {1} game(s)" -f $featureName, $featureAdded.Count.ToString()), "Splash Screen")
}

function Remove-ImageSkipFeature
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $featureName = "[Splash Screen] Skip splash image"
    $feature = $PlayniteApi.Database.Features.Add($featureName)
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    
    $featureRemoved = 0
    
    foreach ($game in $GameDatabase) {
        if ($game.FeatureIds)
        {
            if ($game.FeatureIds -contains $feature.Id)
            {
                $game.FeatureIds.Remove($feature.Id)
                $PlayniteApi.Database.Games.Update($game)
                $featureRemoved++
                $__logger.Info(("Removed `"{0}`" feature from `"{1}`"." -f $featureName, $game.name))          
            }
        }
    }
    
    $PlayniteApi.Dialogs.ShowMessage(("Removed `"{0}`" exclude feature from {1} game(s)" -f $featureName, $featureRemoved.Count.ToString()), "Splash Screen");
}