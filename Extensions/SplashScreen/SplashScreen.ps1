function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Open video manager"
    $menuItem1.FunctionName = "Invoke-OpenVideoManagerWindow"
    $menuItem1.MenuSection = "@Splash Screen"
    
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "View settings"
    $menuItem2.FunctionName = "Invoke-ViewSettings"
    $menuItem2.MenuSection = "@Splash Screen"

    return $menuItem1, $menuItem2
}

function Invoke-ViewSettings
{
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
            <CheckBox Name="CBviewVideoDesktopMode" Margin="0,10,00,0"/>
            <CheckBox Name="CBcloseSplashScreenDesktopMode" Margin="0,10,00,0"/>
            <CheckBox Name="CBexecuteInFullscreenMode" Margin="0,20,0,0"/>
            <CheckBox Name="CBviewVideoFullscreenMode" Margin="0,10,0,0"/>
            <CheckBox Name="CBcloseSplashScreenFullscreenMode" Margin="0,10,00,0"/>
            <CheckBox Name="CBshowLogoInSplashscreen" Margin="0,20,0,0"/>
        </StackPanel>
    </DockPanel>
</Grid>
"@

    # Load the xaml for controls
    $XMLReader = [System.Xml.XmlNodeReader]::New($Xaml)
    $XMLForm = [Windows.Markup.XamlReader]::Load($XMLReader)

    # Make variables for each control
    $Xaml.FirstChild.SelectNodes("//*[@Name]") | ForEach-Object {Set-Variable -Name $_.Name -Value $XMLForm.FindName($_.Name) }

    # Set items sources of controls
    $CBexecuteInDesktopMode.Content = "Execute extension in Desktop Mode"
    $CBexecuteInDesktopMode.IsChecked = $settings.executeInDesktopMode

    $CBviewVideoDesktopMode.Content = "View intro videos in Desktop Mode"
    $CBviewVideoDesktopMode.IsChecked = $settings.viewVideoDesktopMode

    $CBcloseSplashScreenDesktopMode.Content = "Automatically close splashscreen in Desktop Mode (Hides desktop when game closes but disabling can cause issues)"
    $CBcloseSplashScreenDesktopMode.IsChecked = $settings.closeSplashScreenDesktopMode

    $CBexecuteInFullscreenMode.Content = "Execute extension in Fullscreen Mode"
    $CBexecuteInFullscreenMode.IsChecked = $settings.executeInFullscreenMode

    $CBviewVideoFullscreenMode.Content = "View intro videos in Fullscreen Mode"
    $CBviewVideoFullscreenMode.IsChecked = $settings.viewVideoFullscreenMode

    $CBcloseSplashScreenFullscreenMode.Content = "Automatically close splashscreen in Fullscreen Mode (Hides desktop when game closes but disabling can cause issues)"
    $CBcloseSplashScreenFullscreenMode.IsChecked = $settings.closeSplashScreenFullscreenMode

    $CBshowLogoInSplashscreen.Content = "Add game logo in splashscreen image if available"
    $CBshowLogoInSplashscreen.IsChecked = $settings.showLogoInSplashscreen

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
    $window.Height = 450
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
        $settings.viewVideoDesktopMode = $CBviewVideoDesktopMode.IsChecked
        $settings.closeSplashScreenDesktopMode = $CBcloseSplashScreenDesktopMode.IsChecked
        $settings.executeInFullscreenMode = $CBexecuteInFullscreenMode.IsChecked
        $settings.viewVideoFullscreenMode = $CBviewVideoFullscreenMode.IsChecked
        $settings.closeSplashScreenFullscreenMode = $CBcloseSplashScreenFullscreenMode.IsChecked
        $settings.showLogoInSplashscreen = $CBshowLogoInSplashscreen.IsChecked

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
        "viewVideoDesktopMode" = $false
        "closeSplashScreenDesktopMode" = $true
        "executeInFullscreenMode" = $true
        "viewVideoFullscreenMode" = $true
        "closeSplashScreenFullscreenMode" = $true
        "showLogoInSplashscreen" = $false
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
    [System.Collections.ArrayList]$platforms = @()
    $PlayniteApi.Database.Platforms | Sort-Object -Property "Name" | ForEach-Object {
        $platform = [PSCustomObject]@{
            Name = $_.Name
            Value = $_.Id.ToString()
        }
        $platforms.Add($platform) | Out-Null
    }

    [System.Collections.ArrayList]$sources = @()
    $PlayniteApi.Database.Sources | Sort-Object -Property "Name" | ForEach-Object {
        $source = [PSCustomObject]@{
            Name = $_.Name
            Value = $_.Id.ToString()
        }
        $sources.Add($source) | Out-Null
    }

    [System.Collections.ArrayList]$selectedGames = @()
    $PlayniteApi.MainView.SelectedGames | Sort-Object -Property "Name" | Select-Object -Unique | ForEach-Object {
        $game = [PSCustomObject]@{
            Name = $_.Name
            Value = $_.Id.ToString()
        }
        $selectedGames.Add($game) | Out-Null
    }

    [System.Collections.ArrayList]$playniteModes = @()
    $mode = [PSCustomObject]@{
        Name = "Desktop"
        Value = "Desktop"
    }
    $playniteModes.Add($mode) | Out-Null
    $mode = [PSCustomObject]@{
        Name = "Fullscreen"
        Value = "Fullscreen"
    }
    $playniteModes.Add($mode) | Out-Null

    $comboBoxCollectionSource = [ordered]@{
        "Games" = "Games"
        "Sources" = "Sources"
        "Plaforms" = "Plaforms"
        "Playnite Mode" = "Playnite Mode"
    }

    $comboBoxCollectionSource

    $extraMetadataDirectory = [System.IO.Path]::Combine($PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata")

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

    # Handler for ListBoxPlatforms
    $ListBoxSelectedCollection.Add_SelectionChanged(
    {
        $ButtonAddVideo.Visibility = "Visible"
        switch ($ComboBoxCollections.SelectedItem.Name) {
            "Games" {$collection = "games"}
            "Sources" {$collection = "sources"}
            "Plaforms" {$collection = "platforms"}
            "Playnite Mode" {$collection = "playnite"}
            default {$collection = "other"}
        }

        $videoPath = [System.IO.Path]::Combine($extraMetadataDirectory, $collection, $ListBoxSelectedCollection.SelectedItem.Value, "VideoIntro.mp4")
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
            $VideoPlayer.Stop()
            $VideoPlayer.Source = ""
        }
    })

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

    # Handler for pressing "Add Video" button
    $ButtonAddVideo.Add_Click(
    {
        $VideoPlayer.Stop()
        $VideoPlayer.Source = ""
        switch ($ComboBoxCollections.SelectedItem.Name) {
            "Games" {$collection = "games"}
            "Sources" {$collection = "sources"}
            "Plaforms" {$collection = "platforms"}
            "Playnite Mode" {$collection = "playnite"}
            default {$collection = "other"}
        }

        $videoPath = [System.IO.Path]::Combine($extraMetadataDirectory, $collection, $ListBoxSelectedCollection.SelectedItem.Value, "VideoIntro.mp4")
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
        $VideoPlayer.Stop()
        $VideoPlayer.Source = ""
        switch ($ComboBoxCollections.SelectedItem.Name) {
            "Games" {$collection = "games"}
            "Sources" {$collection = "sources"}
            "Plaforms" {$collection = "platforms"}
            "Playnite Mode" {$collection = "playnite"}
            default {$collection = "other"}
        }

        $videoPath = [System.IO.Path]::Combine($extraMetadataDirectory, $collection, $ListBoxSelectedCollection.SelectedItem.Value, "VideoIntro.mp4")
        Remove-IntroVideo $videoPath
        if(![System.IO.File]::Exists($videoPath))
        {
            $ButtonRemoveVideo.Visibility = "Hidden"
            $TextBlockVideoAvailable.Visibility = "Visible"
            $VideoPlayer.Stop()
            $VideoPlayer.Source = ""
        }
    })

    $window.Add_Closing(
    {
        $VideoPlayer.Stop()
        $VideoPlayer.Source = ""
    })

    $window.ShowDialog()
    $window = $null
    [System.GC]::Collect()
}

function Invoke-SetIntroVideoGames
{
    $videoSourcePath = $PlayniteApi.Dialogs.SelectFile("mp4|*.mp4")
    if ([string]::IsNullOrEmpty($videoSourcePath))
    {
        return
    }
    
    $count = 0
    foreach ($game in $PlayniteApi.MainView.SelectedGames)
    {
        $videoName = $game.Id + ".mp4"
        $videoDestinationPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath $videoName
        Copy-Item $videoSourcePath $videoDestinationPath -Force
        $count++
    }
    $PlayniteApi.Dialogs.ShowMessage("Intro video has been added to $count game(s)", "Splash Screen")
}

function Invoke-RemoveIntroVideoGames
{
    $count = 0
    foreach ($game in $PlayniteApi.MainView.SelectedGames)
    {
        $videoName = $game.Id + ".mp4"
        $videoPath = Join-Path -Path $CurrentExtensionDataPath -ChildPath $videoName
        if (Test-Path $videoPath)
        {
            Remove-Item $videoPath -Force
            $count++
        }
    }
    $PlayniteApi.Dialogs.ShowMessage("Intro video has been removed from $count game(s)", "Splash Screen")
}

function Get-SplashVideoPath
{
    param (
        [Playnite.SDK.Models.Game] $game
    )

    $videoTemplate = [System.IO.Path]::Combine($PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "{0}", "{1}", "VideoIntro.mp4")

    $splashVideo = $videoTemplate -f "games", $game.Id.ToString()
    if ([System.IO.File]::Exists($splashVideo))
    {
        return $splashVideo
    }

    if ($null -ne $game.Source)
    {
        $splashVideo = $videoTemplate -f "sources", $game.Source.Id.ToString()
        if ([System.IO.File]::Exists($splashVideo))
        {
            return $splashVideo
        }
    }
    
    if ($null -ne $game.Platform)
    {
        $splashVideo = $videoTemplate -f "platforms", $game.Platform.Id.ToString()
        if ([System.IO.File]::Exists($splashVideo))
        {
            return $splashVideo
        }
    }
    
    $splashVideo = $videoTemplate -f "playnite", $PlayniteApi.ApplicationInfo.Mode
    if ([System.IO.File]::Exists($splashVideo))
    {
        return $splashVideo
    }

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
            return $PlayniteApi.Database.GetFullFilePath($game.BackgroundImage)
        }
    }

    if ($game.Platform)
    {
        if ($game.Platform.Background)
        {
            return $PlayniteApi.Database.GetFullFilePath($game.Platform.Background)
        }
    }

    if ($PlayniteApi.ApplicationInfo.Mode -eq "Desktop")
    {
        return [System.IO.Path]::Combine($CurrentExtensionInstallPath, "SplashScreenDesktopMode.png")
    }
    else
    {
        return [System.IO.Path]::Combine($CurrentExtensionInstallPath, "SplashScreenFullscreenMode.png")
    }
}

function OnGameStarting
{
    param(
        $game
    )

    $settings = Get-Settings

    if (($PlayniteApi.ApplicationInfo.Mode -eq "Desktop") -and ($settings.executeInDesktopMode -eq $false))
    {
        return
    }
    elseif (($PlayniteApi.ApplicationInfo.Mode -eq "Fullscreen") -and ($settings.executeInFullscreenMode -eq $false))
    {
        return
    }

    $splashImage = Get-SplashImagePath $game

    $logoPath = ""
    if ($settings.showLogoInSplashscreen -eq $true)
    {
        $logoPath = [System.IO.Path]::Combine($PlayniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", $game.Id, "Logo.png")
    }
    
    $settings.closeSplashScreenDesktopMode

    switch ($PlayniteApi.ApplicationInfo.Mode.ToString()) {
        "Desktop" { $closeSplashScreenAutomatic = $settings.closeSplashScreenDesktopMode}
        Default { $closeSplashScreenAutomatic = $settings.closeSplashScreenFullscreenMode }
    }
    @($splashImage, $logoPath, $closeSplashScreenAutomatic) | ConvertTo-Json | Out-File (Join-Path $env:TEMP -ChildPath "SplashScreen.json")

    if ((($PlayniteApi.ApplicationInfo.Mode -eq "Desktop") -and ($settings.viewVideoDesktopMode -eq $true)) -or (($PlayniteApi.ApplicationInfo.Mode -eq "Fullscreen") -and ($settings.executeInFullscreenMode -eq $true)))
    {
        $splashVideo = Get-SplashVideoPath $game
        if ($null -ne $splashVideo)
        {
            Invoke-VideoSplashScreen $splashVideo
        }
        else
        {
            Invoke-ImageSplashScreen
        }
    }
    else
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
                        HorizontalAlignment="Center" VerticalAlignment="Center" Grid.Column="1" Grid.ColumnSpan="1">
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
                        return
                    }
                })
            }

            # Show Window
            $window.ShowDialog() | Out-Null
            $window = $null
            [System.GC]::Collect()
        }
    }

    # Sleep time to make sure splash screen shows in cases where game loads too fast
    Start-Sleep 2
}

function Invoke-VideoSplashScreen
{
    param (
        [string]$splashVideo
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

    $VideoPlayer.Volume = 100;
    [uri]$videoSource = $splashVideo
    $VideoPlayer.Source = $VideoSource;
    $VideoPlayer.Play()
    
    # Handler for video player
    $VideoPlayer.Add_MediaEnded({
        Invoke-ImageSplashScreen
        Start-Sleep -Milliseconds 300
        $window.Close()
    })

    # Show Window
    $window.ShowDialog() | Out-Null
    $window = $null
    [System.GC]::Collect()
}

function OnGameStopped
{
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