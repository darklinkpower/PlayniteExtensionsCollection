function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Open Tools Menu"
    $menuItem1.FunctionName = "OpenMenu"
    $menuItem1.MenuSection = "@Game Media Tools"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "See missing media statistics (All games)"
    $menuItem2.FunctionName = "MissingMediaStatsAll"
    $menuItem2.MenuSection = "@Game Media Tools"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem3.Description = "See missing media statistics (Selected games)"
    $menuItem3.FunctionName = "MissingMediaStatsSelection"
    $menuItem3.MenuSection = "@Game Media Tools"

    return $menuItem1, $menuItem2, $menuItem3
}

function GetGameMenuItems
{
    param(
        $menuArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  "Open Metadata Folder"
    $menuItem.FunctionName = "OpenMetadataFolder"

    return $menuItem
}

function OpenMetadataFolder
{
    # Set GameDatabase
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    
    foreach ($game in $gameDatabase) {
        # Set metadata folder directory
        $directory = Join-Path $PlayniteApi.Database.DatabasePath -ChildPath "Files" | Join-Path -ChildPath $game.Id
        
        # Verify if metadata folder exists and open
        if (Test-Path $directory)
        {
            Invoke-Item $directory
        }
    }
}

function Get-MissingMediaStats
{
    param (
        $gameDatabase,
        $selection
    )
    
    # Get information of missing media
    $gamesNoCover = ($gameDatabase | Where-Object {$null -eq $_.CoverImage}).count
    $gamesNoBackground = ($gameDatabase | Where-Object {$null -eq $_.BackgroundImage}).count
    $gamesNoIcon = ($gameDatabase | Where-Object {$null -eq $_.Icon}).count

    # Show results
    $results = "Missing media in $selection $($gameDatabase.Count):`n`nCovers: $gamesNoCover games`nBackground Images: $gamesNoBackground games`nIcons: $gamesNoIcon games"
    $__logger.Info("Game Media Tools (Missing Media) - $($results -replace "`n", ', ')")
    $PlayniteApi.Dialogs.ShowMessage($results, "Game Media Tools")
}

function MissingMediaStatsSelection
{
    
    # Get information of missing media
    $gameDatabase = $PlayniteApi.MainView.SelectedGames
    $selection = "selected games"
    if ($gameDatabase.count -ge 1)
    {
        Get-MissingMediaStats $gameDatabase $selection
    }
    else 
    {
        $PlayniteApi.Dialogs.ShowMessage("No games are selected", "Game Media Tools")
    }
}

function MissingMediaStatsAll
{
    
    # Get information of missing media
    $gameDatabase = $PlayniteApi.Database.Games
    $selection = "all games"
    Get-MissingMediaStats $gameDatabase $selection
}

function OpenMenu
{
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
        <StackPanel Orientation="Horizontal" Margin="0,0,0,15">
            <TextBlock TextWrapping="Wrap" Text="Game Selection:" VerticalAlignment="Center" MinWidth="140"/>
            <ComboBox Name="CbGameSelection" SelectedIndex="0" MinHeight="25" MinWidth="200" VerticalAlignment="Center" Margin="10,0,0,0">
                <ComboBoxItem Content="All games in database" HorizontalAlignment="Stretch"/>
                <ComboBoxItem Content="Selected Games" HorizontalAlignment="Stretch"/>
            </ComboBox>
        </StackPanel>

        <StackPanel Orientation="Horizontal" Margin="0,0,0,15">
            <TextBlock TextWrapping="Wrap" Text="Media type selection:" VerticalAlignment="Center" MinWidth="140"/>
            <ComboBox Name="CbMediaType" SelectedIndex="0" MinHeight="25" MinWidth="200" VerticalAlignment="Center" Margin="10,0,0,0">
                <ComboBoxItem Content="Cover Image" HorizontalAlignment="Stretch"/>
                <ComboBoxItem Content="Background Image" HorizontalAlignment="Stretch"/>
                <ComboBoxItem Content="Icon" HorizontalAlignment="Stretch"/>
            </ComboBox>
        </StackPanel>
        <TabControl Name="ControlTools" HorizontalAlignment="Left" MinHeight="220" Margin="0,0,0,15">
            <TabItem Header="Missing Media">
                <StackPanel>
                    <TextBlock Margin="0,15,0,15" TextWrapping="Wrap" Text="Description:" FontWeight="Bold"/>
                    <TextBlock Margin="0,0,0,15" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that are missing the selected media type."/>
                </StackPanel>
            </TabItem>
            <TabItem Header="Optimization">
                <StackPanel>
                    <TextBlock Margin="0,15,0,15" TextWrapping="Wrap" Text="Description:" FontWeight="Bold"/>
                    <TextBlock Margin="0,0,0,15" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that its media type image resolution is too big. These type of media will decrease performance in Playnite."/>
                </StackPanel>
            </TabItem>
            <TabItem Header="Aspect Ratio">
                <StackPanel>
                    <TextBlock Margin="0,15,0,15" TextWrapping="Wrap" Text="Description:" FontWeight="Bold"/>
                    <TextBlock Margin="0,0,0,15" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that its media type is not the entered aspect ratio."/>
                    <StackPanel Orientation="Horizontal" Margin="0,15,0,0">
                        <TextBlock Margin="0,0,0,15" TextWrapping="Wrap" Text="Enter aspect ratio:" VerticalAlignment="Center"/>
                        <TextBox Name="BoxAspectRatioWidth" Margin="10,0,0,15" MinHeight="25" TextWrapping="Wrap" Text="" VerticalAlignment="Top" Width="50"/>
                        <TextBlock Margin="10,0,0,15" TextWrapping="Wrap" Text=":" VerticalAlignment="Center" TextAlignment="Center"/>
                        <TextBox Name="BoxAspectRatioHeight" Margin="10,0,0,15" MinHeight="25" TextWrapping="Wrap" Text="" VerticalAlignment="Top" Width="50"/>
                    </StackPanel>
                </StackPanel>
            </TabItem>
            <TabItem Header="Resolution">
                <StackPanel>
                    <TextBlock Margin="0,15,0,15" TextWrapping="Wrap" Text="Description:" FontWeight="Bold"/>
                    <TextBlock Margin="0,0,0,15" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that its media type is not the entered resolution."/>
                    <TextBlock Text="Enter resolution in pixels (px)" HorizontalAlignment="Left" Margin="0,15,0,10" TextWrapping="Wrap"/>
                    <StackPanel Orientation="Horizontal">
                        <TextBlock TextWrapping="Wrap" Text="Width:" VerticalAlignment="Center"/>
                        <TextBox Name="BoxResolutionWidth" Margin="10,0,0,0" MinHeight="25" TextWrapping="Wrap" Width="50"/>
                        <TextBlock Margin="10,0,0,0" TextWrapping="Wrap" Text="Height:" VerticalAlignment="Center"/>
                        <TextBox Name="BoxResolutionHeight" Margin="10,0,0,0" MinHeight="25" TextWrapping="Wrap" Width="50"/>
                    </StackPanel>
                </StackPanel>
            </TabItem>
            <TabItem Header="Size">
                <StackPanel>
                    <TextBlock Margin="0,15,0,15" TextWrapping="Wrap" Text="Description:" FontWeight="Bold"/>
                    <TextBlock Margin="0,0,0,15" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that its media type is bigger than the entered size."/>
                    <StackPanel Orientation="Horizontal" Margin="0,15,0,0">
                        <TextBlock TextWrapping="Wrap" Text="Maximum size:" VerticalAlignment="Center"/>
                        <TextBox Name="BoxSize" Margin="10,0,0,0" MinHeight="25" TextWrapping="Wrap" Width="50"/>
                        <TextBlock Margin="10,0,0,0" TextWrapping="Wrap" Text="kb" VerticalAlignment="Center"/>
                    </StackPanel>
                </StackPanel>
            </TabItem>
            <TabItem Header="Extension">
                <StackPanel>
                    <TextBlock Margin="0,15,0,15" TextWrapping="Wrap" Text="Description:" FontWeight="Bold"/>
                    <TextBlock Margin="0,0,0,15" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that its media type is the entered extension"/>
                    <StackPanel Orientation="Horizontal" Margin="0,15,0,0">
                        <TextBlock TextWrapping="Wrap" Text="Image extension:" VerticalAlignment="Center"/>
                        <TextBox Name="BoxExtension" Margin="10,0,0,0" MinHeight="25" TextWrapping="Wrap" Width="50"/>
                    </StackPanel>
                </StackPanel>
            </TabItem>
        </TabControl>
        <Button Content="Update Tags" HorizontalAlignment="Center" Margin="0,0,0,15" Name="ButtonUpdateTags" IsDefault="True"/>
    </StackPanel>
</Grid>
"@

    # Load the xaml for controls
    $XMLReader = [System.Xml.XmlNodeReader]::New($Xaml)
    $XMLForm = [Windows.Markup.XamlReader]::Load($XMLReader)

    # Make variables for each control
    $Xaml.FirstChild.SelectNodes("//*[@Name]") | ForEach-Object {Set-Variable -Name $_.Name -Value $XMLForm.FindName($_.Name) }

    # Set Window creation options
    $windowCreationOptions = New-Object Playnite.SDK.WindowCreationOptions
    $windowCreationOptions.ShowCloseButton = $true
    $windowCreationOptions.ShowMaximizeButton = $False
    $windowCreationOptions.ShowMinimizeButton = $False

    # Create window
    $window = $PlayniteApi.Dialogs.CreateWindow($windowCreationOptions)
    $window.Content = $XMLForm
    $window.Width = 620
    $window.Height = 460
    $window.Title = "Game Media Tools"
    $window.WindowStartupLocation = "CenterScreen"

    # Handler for pressing "Add Tags" button
    $ButtonUpdateTags.Add_Click(
    {
        # Get the variables from the controls
        $gameSelection = $CbGameSelection.SelectedIndex
        $mediaTypeSelection = $CbMediaType.SelectedIndex
        $toolSelection = $ControlTools.SelectedIndex

        # Set GameDatabase
        switch ($gameSelection) {
            0 {
                $gameDatabase = $PlayniteApi.Database.Games
                $__logger.Info("Game Media Tools - Game Selection: `"AllGames`"")
            }
            1 {
                $gameDatabase = $PlayniteApi.MainView.SelectedGames
                $__logger.Info("Game Media Tools - Game Selection: `"SelectedGames`"")
            }
        }

        # Set Media Type
        switch ($mediaTypeSelection) {
            0 { 
                $mediaType = "Cover"
                $OptimizedSize = 1
                $__logger.Info("Game Media Tools - Media Selection: `"$mediaType`"")
            }
            1 {
                $mediaType = "Background"
                $OptimizedSize = 4
                $__logger.Info("Game Media Tools - Media Selection: `"$mediaType`"")
            }
            2 {
                $mediaType = "Icon"
                $OptimizedSize = 0.1
                $__logger.Info("Game Media Tools - Media Selection: `"$mediaType`"")
            }
        }

        # Set Tool
        switch ($toolSelection) {
            0 { # Tool #0: Missing Media

                $__logger.Info("Game Media Tools - Tool Selection: `"Missing Media`"")
                
                # Start Game Media Tools function
                $__logger.Info("Game Media Tools - Starting Function with parameters `"$mediaType`"")
                Invoke-GameMediaTools $gameDatabase $mediaType "" "" "" ""
            }
            1 { # Tool #1: Check Optimization
                
                $__logger.Info("Game Media Tools - Tool Selection: `"Check Optimization`"")
                
                # Set tag Name
                $TagTitle = "Optimization"
                $TagDescription = "not optimized"
                $tagName = "$TagTitle`: $mediaType $TagDescription"
                
                # Set function to determine tag operation
                $toolFunctionName = "ToolCheckOptimization"
                $additionalOperation = "GetDimensions"
                $extraParameters = @(
                    $OptimizedSize
                )
                
                # Start Game Media Tools function
                $__logger.Info("Game Media Tools - Starting Function with parameters `"$mediaType, $tagName, $toolFunctionName, $additionalOperation, $extraParameters`"")
                Invoke-GameMediaTools $gameDatabase $mediaType $tagName $toolFunctionName $additionalOperation $extraParameters
            }
            2 { # Tool #2: Image Aspect Ratio
                
                $__logger.Info("Game Media Tools - Tool Selection: `"Image Aspect Ratio`"")
                $toolTargetWidth = $BoxAspectRatioWidth.Text
                $toolTargetHeight = $BoxAspectRatioHeight.Text

                if (($toolTargetWidth -match "^\d+$") -and ($toolTargetHeight -match "^\d+$"))
                {
                    # Set tag Name
                    $TagTitle = "Aspect ratio"
                    $TagDescription = "not $toolTargetWidth`:$toolTargetHeight"
                    $tagName = "$TagTitle`: $mediaType $TagDescription"
                    
                    # Set function to determine tag operation
                    $toolFunctionName = "ToolAspectRatio"
                    $additionalOperation = "GetDimensions"
                    $toolTargetAspectRatio = $toolTargetWidth/$toolTargetHeight
                    $extraParameters = @(
                        $toolTargetAspectRatio
                    )
                    
                    # Start Game Media Tools function
                    $__logger.Info("Game Media Tools - Starting Function with parameters `"$mediaType, $tagName, $toolFunctionName, $additionalOperation, $extraParameters`"")
                    Invoke-GameMediaTools $gameDatabase $mediaType $tagName $toolFunctionName $additionalOperation $extraParameters
                }
                else
                {
                    $__logger.Info("Game Media Tools - Invalid Input `"$toolTargetWidth`", `"$toolTargetHeight`"")
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in Width and height Input boxes.", "Game Media Tools")
                }
            }
            3 { # Tool #3: Resolution
                
                $__logger.Info("Game Media Tools - Tool Selection: `"Resolution`"")
                $toolTargetWidth = $BoxResolutionWidth.Text
                $toolTargetHeight = $BoxResolutionHeight.Text

                if (($toolTargetWidth -match "^\d+$") -and ($toolTargetHeight -match "^\d+$"))
                {
                    # Set tag Name
                    $TagTitle = "Resolution"
                    $TagDescription = "not $toolTargetWidth`x$toolTargetHeight"
                    $tagName = "$TagTitle`: $mediaType $TagDescription"
                    
                    # Set function to determine tag operation
                    $toolFunctionName = "ToolImageResolution"
                    $additionalOperation = "GetDimensions"
                    $extraParameters = @(
                        $toolTargetWidth,
                        $toolTargetHeight
                    )

                    # Start Game Media Tools function
                    $__logger.Info("Game Media Tools - Starting Function with parameters `"$mediaType, $tagName, $toolFunctionName, $additionalOperation, $extraParameters`"")
                    Invoke-GameMediaTools $gameDatabase $mediaType $tagName $toolFunctionName $additionalOperation $extraParameters
                }
                else
                {
                    $__logger.Info("Game Media Tools - Invalid Input `"$toolTargetWidth`", `"$toolTargetHeight`"")
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in Width and height Input boxes.", "Game Media Tools")
                }
            }
            4 { # Tool #4: Image Size
                
                $__logger.Info("Game Media Tools - Tool Selection: `"Image Size`"")
                $toolTargetMaximumSize = $BoxSize.Text

                if ($toolTargetMaximumSize -match "^\d+$")
                {
                    # Set tag Name
                    $TagTitle = "Size"
                    $TagDescription = "bigger than $toolTargetMaximumSize`kb"
                    $tagName = "$TagTitle`: $mediaType $TagDescription"

                    # Tool Information
                    $toolFunctionName = "ToolImageSize"
                    $additionalOperation = "ImagePath"
                    $extraParameters = @(
                        $toolTargetMaximumSize
                    )

                    # Start Game Media Tools function
                    $__logger.Info("Game Media Tools - Starting Function with parameters `"$mediaType, $tagName, $toolFunctionName, $additionalOperation, $extraParameters`"")
                    Invoke-GameMediaTools $gameDatabase $mediaType $tagName $toolFunctionName $additionalOperation $extraParameters
                }
                else
                {
                    $__logger.Info("Game Media Tools - Invalid Input `"$toolTargetMaximumSize`"")
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in size input box.", "Game Media Tools")
                }
            }
            5 { # Tool #5: Extension
                
                $__logger.Info("Game Media Tools - Tool Selection: `"Extension`"")
                $toolTargetImageExtension = $BoxExtension.Text

                if ($toolTargetImageExtension -match "^.+$")
                {
                    # Set tag Name
                    $TagTitle = "Extension"
                    $TagDescription = "is $toolTargetImageExtension"
                    $tagName = "$TagTitle`: $mediaType $TagDescription"

                    # Tool Information
                    $toolFunctionName = "ToolImageExtension"
                    $additionalOperation = "ImagePath"
                    $extraParameters = @(
                        $toolTargetImageExtension
                    )
                    
                    # Start Game Media Tools function
                    $__logger.Info("Game Media Tools - Starting Function with parameters `"$mediaType, $tagName, $toolFunctionName, $additionalOperation, $extraParameters`"")
                    Invoke-GameMediaTools $gameDatabase $mediaType $tagName $toolFunctionName $additionalOperation $extraParameters
                }
                else
                {
                    $__logger.Info("Game Media Tools - Invalid Input `"$toolTargetImageExtension`"")
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in extension input box.", "Game Media Tools")
                }
            }
        }
    })

    # Show Window
    $__logger.Info("Game Media Tools - Opening Window.")
    $window.ShowDialog()
    $__logger.Info("Game Media Tools - Window closed.")
}

function Invoke-GameMediaTools
{
    param (
        $gameDatabase, 
        $mediaType,
        $tagName,
        $toolFunctionName,
        $additionalOperation,
        $extraParameters
    )
    
    # Create "No Media" tag
    $tagNoMediaName = "No Media: " + $mediaType
    $tagNoMedia = $PlayniteApi.Database.tags.Add($tagNoMediaName)
    $tagNoMediaIds = $tagNoMedia.Id

    # Create Tool tag
    if ($tagName -ne "")
    {
        $tagMatch = $PlayniteApi.Database.tags.Add($tagName)
        $ToolTagId = $tagMatch.Id
    }
    
    foreach ($game in $gameDatabase) {

        $global:imageFilePath = Get-ImagePath $game $mediaType
        if ($null -eq $imageFilePath)
        {
            Add-TagToGame $game $tagNoMediaIds
        }
        else
        {
            Remove-TagFromGame $game $tagNoMediaIds

            # Skip Game if media is of URL type
            if ($imageFilePath -match "^https?:\/\/")
            {
                continue
            }
            
            # Skip game if file path doesn't exist and delete property value
            if ([System.IO.File]::Exists($imageFilePath) -eq $false)
            {
                if ($mediaType -eq "Cover")
                {
                    $game.CoverImage = $null
                }
                elseif ($mediaType -eq "Background")
                {
                    $game.BackgroundImage = $null
                }
                elseif ($mediaType -eq "Icon")
                {
                    $game.Icon = $null
                }
                $__logger.Info("Game Media Tools - `"$($game.name)`" $mediaType doesn't exist in pointed path. Property value deleted.")
                continue
            }

            # Determine Tag Operation
            if ($additionalOperation -eq "GetDimensions")
            {
                $global:imageSuccess = $false
                Get-ImageDimensions $imageFilePath

                # Skip if couldn't get image information
                if ($imageSuccess -eq $false)
                {
                    continue
                }
                $tagOperation = &$toolFunctionName $imageWidth $imageHeight $extraParameters
            }
            elseif ($additionalOperation -eq "ImagePath")
            {
                $tagOperation = &$toolFunctionName $imageFilePath $extraParameters
            }
            else
            {
                continue
            }

            # Add or Remove tool tag
            if ($TagOperation -eq "RemoveTag")
            {
                Remove-TagFromGame $game $ToolTagId
            }
            elseif ($TagOperation -eq "AddTag")
            {
                Add-TagToGame $game $ToolTagId
            }
        }
    }
    
    # Generate results of missing media in selection
    $GamesNoMediaSelection = $gameDatabase | Where-Object {$_.TagIds -contains $tagNoMediaIds.Guid}
    $results = "Finished. Games in selection: $($gameDatabase.count)`n`nSelected Media: $mediaType`nGames missing selected media in selection: $($GamesNoMediaSelection.Count)"

    # Get information of games with missing media in all database and add to results
    $gamesNoCoverAll = ($PlayniteApi.Database.Games | Where-Object {$null -eq $_.CoverImage}).count
    $gamesNoBackgroundAll = ($PlayniteApi.Database.Games | Where-Object {$null -eq $_.BackgroundImage}).count
    $gamesNoIconAll = ($PlayniteApi.Database.Games | Where-Object {$null -eq $_.Icon}).count
    $results += "`n`nMissing Media in all database`nCovers: $gamesNoCoverAll games`nBackground Images: $gamesNoBackgroundAll games`nIcons: $gamesNoIconAll games"

    # Get information of tool Tag
    if ($tagName)
    {
        $GamesToolTagSelection = $gameDatabase | Where-Object {$_.TagIds -contains $ToolTagId.Guid}
        $GamesToolTagAll = $PlayniteApi.Database.Games | Where-Object {$_.TagIds -contains $ToolTagId.Guid}
        $__logger.Info("Game Media Tools - Games with tool tag `"$tagName`" at finish: Selection $($GamesToolTagSelection.Count), All $($GamesToolTagAll.Count)")
        $results += "`n`nTool tag name: $tagName`nGames with tag in selection: $($GamesToolTagSelection.Count)`nGames with tag in all games database: $($GamesToolTagAll.count)"
        
        # Remove tool tag from database if 0 games have it
        if (($GamesToolTagAll.count -eq 0) -and ($GamesToolTagSelection.count -eq 0))
        {
            $PlayniteApi.Database.Tags.Remove($ToolTagId)
            $__logger.Info("Game Media Tools - Removed tool tag `"$tagName`" from database")
        }
    }
    $__logger.Info("Game Media Tools - $($results -replace "`n", ', ')")
    $PlayniteApi.Dialogs.ShowMessage("$results", "Game Media Tools")
}

function Get-ImageDimensions
{
    param (
        $imageFilePath
    )

    try {
        Add-type -AssemblyName System.Drawing
        $image = New-Object System.Drawing.Bitmap $imageFilePath
        $global:imageHeight = $image.Height
        $global:imageWidth = $image.Width
        $image.Dispose()
        $global:imageSuccess = $true
    } catch {
        $global:imageSuccess = $false
        $errorMessage = $_.Exception.Message
        $__logger.Error("Game Media Tools - $($game.name): Error `"$errorMessage`" when processing image `"$imageFilePath`"")
    }
}

function Get-ImagePath
{
    param (
        $game, 
        $mediaType
    )

    # Verify selected media type, if game has it and get full file path
    if (($mediaType -eq "Cover") -and ($game.CoverImage))
    {
        return $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
    }
    elseif (($mediaType -eq "Background") -and ($game.BackgroundImage))
    {
        return $PlayniteApi.Database.GetFullFilePath($game.BackgroundImage)
    }
    elseif (($mediaType -eq "Icon") -and ($game.Icon))
    {
        return $PlayniteApi.Database.GetFullFilePath($game.Icon)
    }
    else
    {
        return $null
    }
}

function Add-TagToGame
{
    param (
        $game,
        $tagIds
    )

    # Check if game already doesn't have tag
    if ($game.tagIds -notcontains $tagIds)
    {
        # Add tag Id to game
        if ($game.tagIds)
        {
            $game.tagIds += $tagIds
        }
        else
        {
            # Fix in case game has null tagIds
            $game.tagIds = $tagIds
        }
        
        # Update game in database and increase no media count
        $PlayniteApi.Database.Games.Update($game)
    }
}

function Remove-TagFromGame
{
    param (
        $game,
        $tagIds
    )

    # Check if game already has tag and remove it
    if ($game.tagIds -contains $tagIds)
    {
        $game.tagIds.Remove($tagIds)
        $PlayniteApi.Database.Games.Update($game)
    }
}

function ToolCheckOptimization
{
    param (
        $imageWidth,
        $imageHeight,
        $extraParameters
    )
    # Tool #1: Check Optimization
    # Get extra parameters
    $OptimizedSize = $extraParameters[0]
    
    $imageMegaPixels = [math]::Round((($imageWidth * $imageHeight) / 1000000), 3)
    if ($imageMegaPixels -gt $OptimizedSize)
    {
        return "AddTag"
    }
    else
    {
        return "RemoveTag"
    }
}

function ToolAspectRatio
{
    param (
        $imageWidth,
        $imageHeight,
        $extraParameters
    )
    
    # Tool #2: Image Aspect Ratio
    # Get extra parameters
    $toolTargetAspectRatio = $extraParameters[0]
    
    $imageAspectRatio = $imageWidth/$imageHeight
    if ($imageAspectRatio -eq $toolTargetAspectRatio)
    {
        return "RemoveTag"
    }
    else
    {
        return "AddTag"
    }
}
function ToolImageResolution
{
    param (
        $imageWidth,
        $imageHeight,
        $extraParameters
    )
    
    # Tool #3: Resolution
    # Get extra parameters
    $toolTargetWidth = $extraParameters[0]
    $toolTargetHeight = $extraParameters[1]

    if (($toolTargetWidth -eq $imageWidth) -and ($toolTargetHeight -eq $imageHeight))
    {
        return "RemoveTag"
    }
    else
    {
        return "AddTag"
    }
}
function ToolImageSize
{
    param (
        $imageFilePath,
        $extraParameters
    )
    
    # Tool #4: Size
    # Get extra parameters
    $toolTargetMaximumSize = $extraParameters[0]

    $imageSize = (Get-Item $imageFilePath).length/1KB
    if ($imageSize -gt $toolTargetMaximumSize)
    {
        return "AddTag"
    }
    else
    {
        return "RemoveTag"
    }
}

function ToolImageExtension
{
    param (
        $imageFilePath,
        $extraParameters
    )

    # Tool #5: Extension
    # Get extra parameters
    $toolTargetImageExtension = $extraParameters[0]
    
    $imageExtension = [IO.Path]::GetExtension($imageFilePath) -replace '^\.', ''
    if ($imageExtension -eq $toolTargetImageExtension)
    {
        return "AddTag"
    }
    else
    {
        return "RemoveTag"
    }
}