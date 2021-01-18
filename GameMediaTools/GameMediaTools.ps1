function global:GetMainMenuItems
{
    param($menuArgs)

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

function global:GetGameMenuItems
{
    param($menuArgs)

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  "Open Metadata Folder"
    $menuItem.FunctionName = "OpenMetadataFolder"

    return $menuItem
}

function OpenMetadataFolder
{
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames
    
    foreach ($game in $GameDatabase) {
        # Set metadata folder directory
        $Directory = Join-Path $PlayniteApi.Database.DatabasePath -ChildPath "Files" | Join-Path -ChildPath $($game.Id)
        
        # Verify if metadata folder exists and open
        if (Test-Path $Directory)
        {
            Invoke-Item $Directory
        }
    }
}

function Get-MissingMediaStats
{
    param (
        $GameDatabase,
        $Selection
    )
    
    # Get information of missing media
    $GamesNoCover = ($GameDatabase | Where-Object {$null -eq $_.CoverImage}).count
    $GamesNoBackground = ($GameDatabase | Where-Object {$null -eq $_.BackgroundImage}).count
    $GamesNoIcon = ($GameDatabase | Where-Object {$null -eq $_.Icon}).count

    # Show results
    $Results = "Missing media in $Selection ($($GameDatabase.Count)):`n`nCovers: $GamesNoCover games`nBackground Images: $GamesNoBackground games`nIcons: $GamesNoIcon games"
    $__logger.Info("Game Media Tools (Missing Media) - $($Results -replace "`n", ', ')")
    $PlayniteApi.Dialogs.ShowMessage("$Results", "Game Media Tools");
}

function MissingMediaStatsSelection
{
    
    # Get information of missing media
    $GameDatabase = $PlayniteApi.MainView.SelectedGames
    $Selection = "selected games"
    if ($GameDatabase.count -ge 1)
    {
        Get-MissingMediaStats $GameDatabase $Selection
    }
    else 
    {
        $PlayniteApi.Dialogs.ShowMessage("No games are selected", "Game Media Tools"); 
    }
}

function MissingMediaStatsAll
{
    
    # Get information of missing media
    $GameDatabase = $PlayniteApi.Database.Games
    $Selection = "all games"
    Get-MissingMediaStats $GameDatabase $Selection
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
    $WindowCreationOptions = New-Object Playnite.SDK.WindowCreationOptions
    $WindowCreationOptions.ShowCloseButton = $true
    $WindowCreationOptions.ShowMaximizeButton = $False
    $WindowCreationOptions.ShowMinimizeButton = $False

    # Create window
    $Window = $PlayniteApi.Dialogs.CreateWindow($WindowCreationOptions)
    $Window.Content = $XMLForm
    $Window.Width = 620
    $Window.Height = 460
    $Window.Title = "Game Media Tools"
    $Window.WindowStartupLocation = "CenterScreen"

    # Handler for pressing "Add Tags" button
    $ButtonUpdateTags.Add_Click(
    {
        # Get the variables from the controls
        $GameSelection = $CbGameSelection.SelectedIndex
        $MediaTypeSelection = $CbMediaType.SelectedIndex
        $ToolSelection = $ControlTools.SelectedIndex

        # Set GameDatabase
        switch ($GameSelection) {
            0 {
                $GameDatabase = $PlayniteApi.Database.Games
                $__logger.Info("Game Media Tools - Game Selection: `"AllGames`"")
            }
            1 {
                $GameDatabase = $PlayniteApi.MainView.SelectedGames
                $__logger.Info("Game Media Tools - Game Selection: `"SelectedGames`"")
            }
        }

        # Set Media Type
        switch ($MediaTypeSelection) {
            0 { 
                $MediaType = "Cover"
                $OptimizedSize = 1
                $__logger.Info("Game Media Tools - Media Selection: `"$MediaType`"")
            }
            1 {
                $MediaType = "Background"
                $OptimizedSize = 4
                $__logger.Info("Game Media Tools - Media Selection: `"$MediaType`"")
            }
            2 {
                $MediaType = "Icon"
                $OptimizedSize = 0.1
                $__logger.Info("Game Media Tools - Media Selection: `"$MediaType`"")
            }
        }

        # Set Tool
        switch ($ToolSelection) {
            0 { # Tool #0: Missing Media

                $__logger.Info("Game Media Tools - Tool Selection: `"Missing Media`"")
                
                # Start Game Media Tools function
                $__logger.Info("Game Media Tools - Starting Function with parameters `"$MediaType, $TagName, $ToolFunctionName, $AditionalOperation, $ExtraParameters`"")
                Invoke-GameMediaTools $GameDatabase $MediaType
            }
            1 { # Tool #1: Check Optimization
                
                $__logger.Info("Game Media Tools - Tool Selection: `"Check Optimization`"")
                
                # Set tag Name
                $TagTitle = "Optimization"
                $TagDescription = "not optimized"
                $TagName = "$TagTitle`: $MediaType $TagDescription"
                
                # Set function to determine tag operation
                $ToolFunctionName = "ToolCheckOptimization"
                $AditionalOperation = "GetDimensions"
                $ExtraParameters = @(
                    $OptimizedSize
                )
                
                # Start Game Media Tools function
                $__logger.Info("Game Media Tools - Starting Function with parameters `"$MediaType, $TagName, $ToolFunctionName, $AditionalOperation, $ExtraParameters`"")
                Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
            }
            2 { # Tool #2: Image Aspect Ratio
                
                $__logger.Info("Game Media Tools - Tool Selection: `"Image Aspect Ratio`"")
                $Width = $BoxAspectRatioWidth.Text
                $Height = $BoxAspectRatioHeight.Text

                if ( ($Width -match "^\d+$") -and ($Height -match "^\d+$") )
                {
                    # Set tag Name
                    $TagTitle = "Aspect ratio"
                    $TagDescription = "not $Width`:$height"
                    $TagName = "$TagTitle`: $MediaType $TagDescription"
                    
                    # Set function to determine tag operation
                    $ToolFunctionName = "ToolAspectRatio"
                    $AditionalOperation = "GetDimensions"
                    $ExtraParameters = @(
                        $Width,
                        $Height
                    )
                    
                    # Start Game Media Tools function
                    $__logger.Info("Game Media Tools - Starting Function with parameters `"$MediaType, $TagName, $ToolFunctionName, $AditionalOperation, $ExtraParameters`"")
                    Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
                }
                else
                {
                    $__logger.Info("Game Media Tools - Invalid Input `"$Width`", `"$Height`"")
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in Width and height Input boxes.", "Game Media Tools");
                }
            }
            3 { # Tool #3: Resolution
                
                $__logger.Info("Game Media Tools - Tool Selection: `"Resolution`"")
                $Width = $BoxResolutionWidth.Text
                $Height = $BoxResolutionHeight.Text

                if ( ($Width -match "^\d+$") -and ($Height -match "^\d+$") )
                {
                    # Set tag Name
                    $TagTitle = "Resolution"
                    $TagDescription = "not $Width`x$height"
                    $TagName = "$TagTitle`: $MediaType $TagDescription"
                    
                    # Set function to determine tag operation
                    $ToolFunctionName = "ToolImageResolution"
                    $AditionalOperation = "GetDimensions"
                    $ExtraParameters = @(
                        $Width,
                        $Height
                    )

                    # Start Game Media Tools function
                    $__logger.Info("Game Media Tools - Starting Function with parameters `"$MediaType, $TagName, $ToolFunctionName, $AditionalOperation, $ExtraParameters`"")
                    Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
                }
                else
                {
                    $__logger.Info("Game Media Tools - Invalid Input `"$Width`", `"$Height`"")
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in Width and height Input boxes.", "Game Media Tools");
                }
            }
            4 { # Tool #4: Image Size
                
                $__logger.Info("Game Media Tools - Tool Selection: `"Image Size`"")
                $MaxSize = $BoxSize.Text

                if ($MaxSize -match "^\d+$")
                {
                    # Set tag Name
                    $TagTitle = "Size"
                    $TagDescription = "bigger than $MaxSize`kb"
                    $TagName = "$TagTitle`: $MediaType $TagDescription"

                    # Tool Information
                    $ToolFunctionName = "ToolImageSize"
                    $AditionalOperation = "ImagePath"
                    $ExtraParameters = @(
                        $MaxSize
                    )

                    # Start Game Media Tools function
                    $__logger.Info("Game Media Tools - Starting Function with parameters `"$MediaType, $TagName, $ToolFunctionName, $AditionalOperation, $ExtraParameters`"")
                    Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
                }
                else
                {
                    $__logger.Info("Game Media Tools - Invalid Input `"$MaxSize`"")
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in size input box.", "Game Media Tools");
                }
            }
            5 { # Tool #5: Extension
                
                $__logger.Info("Game Media Tools - Tool Selection: `"Extension`"")
                $Extension = $BoxExtension.Text

                if ($Extension -match "^.+$")
                {
                    # Set tag Name
                    $TagTitle = "Extension"
                    $TagDescription = "is $Extension"
                    $TagName = "$TagTitle`: $MediaType $TagDescription"

                    # Tool Information
                    $ToolFunctionName = "ToolImageExtension"
                    $AditionalOperation = "ImagePath"
                    $ExtraParameters = @(
                        $Extension
                    )
                    
                    # Start Game Media Tools function
                    $__logger.Info("Game Media Tools - Starting Function with parameters `"$MediaType, $TagName, $ToolFunctionName, $AditionalOperation, $ExtraParameters`"")
                    Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
                }
                else
                {
                    $__logger.Info("Game Media Tools - Invalid Input `"$Extension`"")
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in extension input box.", "Game Media Tools");
                }
            }
        }
    })

    # Show Window
    $__logger.Info("Game Media Tools - Opening Window.")
    $Window.ShowDialog()
    $__logger.Info("Game Media Tools - Window closed.")
}

function Invoke-GameMediaTools
{
    param (
        $GameDatabase, 
        $MediaType,
        $TagName,
        $ToolFunctionName,
        $AditionalOperation,
        $ExtraParameters
    )
    
    # Create "No Media" tag
    $tagNoMediaName = "No Media: " + "$MediaType"
    $tagNoMedia = $PlayniteApi.Database.tags.Add($tagNoMediaName)
    $global:tagNoMediaIds = $tagNoMedia.Id

    # Create Tool tag
    if ($TagName)
    {
        $tagMatch = $PlayniteApi.Database.tags.Add($TagName)
        $global:ToolTagId = $tagMatch.Id
    }
    
    foreach ($Game in $GameDatabase) {
        # Get Image File path
        Get-ImagePath $game $MediaType

        # Verify if Image File path was obtained
        if ($ImageFilePath)
        {
            # Remove "No Media" tag
            Remove-TagFromGame $game $tagNoMediaIds

            # Skip Game if media is of URL type
            if ($ImageFilePath -match "https?:\/\/")
            {
                continue
            }
            
            # Skip game if file path doesn't exist and delete property value
            if ([System.IO.File]::Exists($ImageFilePath) -eq $false)
            {
                if ($MediaType -eq "Cover")
                {
                    $game.CoverImage = $null

                }
                elseif ($MediaType -eq "Background")
                {
                    $game.BackgroundImage = $null
                }
                elseif ($MediaType -eq "Icon")
                {
                    $game.Icon = $null
                }
                $__logger.Info("Game Media Tools - `"$($game.name)`" $MediaType doesn't exist in pointed path. Property value deleted.")
                continue
            }

            # Determine Tag Operation
            if ($AditionalOperation -eq "GetDimensions")
            {
                $global:ImageSuccess = $false
                Get-ImageDimensions $ImageFilePath

                # Skip if couldn't get image information
                if ($ImageSuccess -eq $false)
                {
                    continue
                }
                $tagOperation = &$ToolFunctionName $ImageWidth $ImageHeight $ExtraParameters
            }
            elseif ($AditionalOperation -eq "ImagePath")
            {
                $tagOperation = &$ToolFunctionName $ImageFilePath $ExtraParameters
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
        else
        {
            # Add "No Media Tag"
            Add-TagToGame $game $tagNoMediaIds
        }
    }
    
    # Generate results of missing media in selection
    $GamesNoMediaSelection = $GameDatabase | Where-Object {$_.TagIds -contains $tagNoMediaIds.Guid}
    $Results = "Finished. Games in selection: $($GameDatabase.count)`n`nSelected Media: $MediaType`nGames missing selected media in selection: $($GamesNoMediaSelection.Count)"

    # Get information of games with missing media in all database and add to results
    $GamesNoCoverAll = ($PlayniteApi.Database.Games | Where-Object {$null -eq $_.CoverImage}).count
    $GamesNoBackgroundAll = ($PlayniteApi.Database.Games | Where-Object {$null -eq $_.BackgroundImage}).count
    $GamesNoIconAll = ($PlayniteApi.Database.Games | Where-Object {$null -eq $_.Icon}).count
    $Results += "`n`nMissing Media in all database`nCovers: $GamesNoCoverAll games`nBackground Images: $GamesNoBackgroundAll games`nIcons: $GamesNoIconAll games"

    # Get information of tool Tag
    if ($TagName)
    {
        $GamesToolTagSelection = $GameDatabase | Where-Object {$_.TagIds -contains $ToolTagId.Guid}
        $GamesToolTagAll = $PlayniteApi.Database.Games | Where-Object {$_.TagIds -contains $ToolTagId.Guid}
        $__logger.Info("Game Media Tools - Games with tool tag `"$TagName`" at finish: Selection $($GamesToolTagSelection.Count), All $($GamesToolTagAll.Count)")

        # Add information to results
        $Results += "`n`nTool tag name: $TagName`nGames with tag in selection: $($GamesToolTagSelection.Count)`nGames with tag in all games database: $($GamesToolTagAll.count)"
        
        # Remove tool tag from database if 0 games have it
        if (($GamesToolTagAll.count -eq 0) -and ($GamesToolTagSelection.count -eq 0))
        {
            $PlayniteApi.Database.Tags.Remove($ToolTagId)
            $__logger.Info("Game Media Tools - Removed tool tag `"$TagName`" from database")
        }
    }

    # Show Results
    $__logger.Info("Game Media Tools - $($Results -replace "`n", ', ')")
    $PlayniteApi.Dialogs.ShowMessage("$Results", "Game Media Tools");
}

function Get-ImageDimensions
{
    param (
        $ImageFilePath
    )

    # Get image width and height
    try {
        Add-type -AssemblyName System.Drawing
        $Image = New-Object System.Drawing.Bitmap $ImageFilePath
        $global:ImageHeight = $image.Height
        $global:imageWidth = $image.Width
        $Image.Dispose()
        $global:ImageSuccess = $true
    } catch {
        $global:ImageSuccess = $false
        $ErrorMessage = $_.Exception.Message
        $__logger.Error("Game Media Tools - $($game.name): Error `"$ErrorMessage`" when processing image `"$ImageFilePath`"")
    }
}

function Get-ImagePath
{
    param (
        [object]$game, 
        [string]$MediaType
    )

    # Verify selected media type, if game has it and get full file path
    if ( ($MediaType -eq "Cover") -and ($game.CoverImage) )
    {
        $global:ImageFilePath = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
    }
    elseif ( ($MediaType -eq "Background") -and ($game.BackgroundImage) )
    {
        $global:ImageFilePath = $PlayniteApi.Database.GetFullFilePath($game.BackgroundImage)
    }
    elseif ( ($MediaType -eq "Icon") -and ($game.Icon) )
    {
        $global:ImageFilePath = $PlayniteApi.Database.GetFullFilePath($game.Icon)
    }
    else
    {
        $global:ImageFilePath = $null
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
        $ImageWidth,
        $ImageHeight,
        $ExtraParameters
    )
    # Tool #1: Check Optimization
    # Get extra parameters
    $OptimizedSize = $ExtraParameters[0]
    
    # Determine Tag Operation
    $ImageMegaPixels = [math]::Round((($ImageWidth * $ImageHeight) / 1000000), 3)
    if ($ImageMegaPixels -gt $OptimizedSize)
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
        $ImageWidth,
        $ImageHeight,
        $ExtraParameters
    )
    
    # Tool #2: Image Aspect Ratio
    # Get extra parameters
    $AspectRatio = $ExtraParameters[0]
    
    # Determine Tag Operation
    $ImageAspectRatio = $imageWidth/$imageHeight
    if ($ImageAspectRatio -eq $AspectRatio)
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
        $ImageWidth,
        $ImageHeight,
        $ExtraParameters
    )
    
    # Tool #3: Resolution
    # Get extra parameters
    $Width = $ExtraParameters[0]
    $Height = $ExtraParameters[1]

    if ( ($Width -eq $imageWidth) -and ($Height -eq $imageHeight) )
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
        $ImageFilePath,
        $ExtraParameters
    )
    
    # Tool #4: Size
    # Get extra parameters
    $MaxSize = $ExtraParameters[0]

    # Determine Tag Operation
    $ImageSize = (Get-Item $ImageFilePath).length/1KB
    if ($ImageSize -gt $MaxSize)
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
        $ImageFilePath,
        $ExtraParameters
    )

    # Tool #5: Extension
    # Get extra parameters
    $Extension = $ExtraParameters[0]
    
    # Determine Tag Operation
    $ImageExtension = [IO.Path]::GetExtension($ImageFilePath) -replace '^\.', ''
    if ($ImageExtension -eq $Extension)
    {
        return "AddTag"
    }
    else
    {
        return "RemoveTag"
    }
}