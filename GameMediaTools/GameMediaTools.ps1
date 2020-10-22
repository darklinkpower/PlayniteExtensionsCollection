function global:GetMainMenuItems
{
    param($menuArgs)

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Open Tools Menu"
    $menuItem1.FunctionName = "OpenMenu"
    $menuItem1.MenuSection = "@Game Media Tools"

    return $menuItem1
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
<Border Margin="40,50,273.6,0" VerticalAlignment="Top" Height="25" Width="180" HorizontalAlignment="Left">
    <TextBlock TextWrapping="Wrap" Text="Game Selection" VerticalAlignment="Center" Width="180"/>
</Border>
<ComboBox Name="CbGameSelection" SelectedIndex="0" HorizontalAlignment="Left" Margin="245,50,0,0" VerticalAlignment="Top" Height="25" Width="200">
    <ComboBoxItem Content="All games in database" HorizontalAlignment="Stretch"/>
    <ComboBoxItem Content="Selected Games" HorizontalAlignment="Stretch"/>
</ComboBox>
<Border Margin="40,90,273.6,0" VerticalAlignment="Top" Width="180" Height="25" HorizontalAlignment="Left">
    <TextBlock TextWrapping="Wrap" Text="Media type selection" VerticalAlignment="Center" Margin="0,4,-0.6,4.8" Width="180"/>
</Border>
<ComboBox Name="CbMediaType" SelectedIndex="0" HorizontalAlignment="Left" Margin="245,90,0,0" VerticalAlignment="Top" Width="200" Height="25">
    <ComboBoxItem Content="Cover Image" HorizontalAlignment="Stretch"/>
    <ComboBoxItem Content="Background Image" HorizontalAlignment="Stretch"/>
    <ComboBoxItem Content="Icon" HorizontalAlignment="Stretch"/>
</ComboBox>
<TabControl Name="ControlTools" HorizontalAlignment="Left" Height="185" Margin="40,143,34.6,44" VerticalAlignment="Top">
    <TabItem Header="Missing Media">
        <Grid>
            <TextBlock HorizontalAlignment="Left" Margin="17,20,0,0" TextWrapping="Wrap" Text="Description:" VerticalAlignment="Top" Height="20" FontWeight="Bold" Grid.RowSpan="2"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,40,0,0" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that are missing the selected media type." VerticalAlignment="Top" Width="303" Grid.ColumnSpan="2"/>
        </Grid>
    </TabItem>
    <TabItem Header="Optimization">
        <Grid>
            <TextBlock HorizontalAlignment="Left" Margin="17,20,0,0" TextWrapping="Wrap" Text="Description:" VerticalAlignment="Top" Height="20" FontWeight="Bold"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,40,0,0" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that its media type image resolution is too big. These type of media will decrease performance in Playnite." VerticalAlignment="Top" Width="303" Grid.ColumnSpan="2"/>
        </Grid>
    </TabItem>
    <TabItem Header="Aspect Ratio">
        <Grid>
            <TextBlock HorizontalAlignment="Left" Margin="17,20,0,0" TextWrapping="Wrap" Text="Description:" VerticalAlignment="Top" Height="20" FontWeight="Bold"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,40,0,0" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that its media type is not the entered aspect ratio." VerticalAlignment="Top" Width="303"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,90,0,0" TextWrapping="Wrap" Text="Enter aspect ratio" VerticalAlignment="Top" Width="260" Height="20"/>
            <TextBox Name="BoxAspectRatioWidth" HorizontalAlignment="Left" Height="20" Margin="140,90,0,0" TextWrapping="Wrap" Text="" VerticalAlignment="Top" Width="40"/>
            <TextBlock HorizontalAlignment="Left" Margin="180,90,0,0" TextWrapping="Wrap" Text=":" VerticalAlignment="Top" Width="20" Height="20" TextAlignment="Center"/>
            <TextBox Name="BoxAspectRatioHeight" HorizontalAlignment="Left" Height="20" Margin="200,90,0,0" TextWrapping="Wrap" Text="" VerticalAlignment="Top" Width="40"/>
        </Grid>
    </TabItem>
    <TabItem Header="Resolution">
        <Grid>
            <TextBlock HorizontalAlignment="Left" Margin="17,20,0,0" TextWrapping="Wrap" Text="Description:" VerticalAlignment="Top" Height="20" FontWeight="Bold"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,40,0,0" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that its media type is not the entered resolution." VerticalAlignment="Top" Width="303"/>
            <TextBox Name="BoxResolutionWidth" HorizontalAlignment="Left" Height="20" Margin="77,119,0,-11.8" TextWrapping="Wrap" VerticalAlignment="Top" Width="40"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,90,0,0" TextWrapping="Wrap" VerticalAlignment="Top" Width="260" Height="20"><Run Text="Enter "/><Run Text="resolution in pixels"/></TextBlock>
            <TextBox Name="BoxResolutionHeight" HorizontalAlignment="Left" Height="20" Margin="197,119,0,-11.8" TextWrapping="Wrap" Text="" VerticalAlignment="Top" Width="40"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,119,0,-11.8" TextWrapping="Wrap" VerticalAlignment="Top" Width="60" Height="20" Text="Width"/>
            <TextBlock HorizontalAlignment="Left" Margin="137,119,0,-11.8" TextWrapping="Wrap" VerticalAlignment="Top" Width="60" Height="20" Text="Height"/>
        </Grid>
    </TabItem>
    <TabItem Header="Size">
        <Grid>
            <TextBlock HorizontalAlignment="Left" Margin="17,20,0,0" TextWrapping="Wrap" Text="Description:" VerticalAlignment="Top" Height="20" FontWeight="Bold"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,40,0,0" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that its media type is bigger than the entered size." VerticalAlignment="Top" Width="303"/>
            <TextBox Name="BoxSize" HorizontalAlignment="Left" Height="20" Margin="17,120,0,-12.8" TextWrapping="Wrap" Text="" VerticalAlignment="Top" Width="40"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,90,0,0" TextWrapping="Wrap" Text="Enter maximum size" VerticalAlignment="Top" Width="260" Height="20"/>
            <TextBlock HorizontalAlignment="Left" Margin="62,120,0,-12.8" TextWrapping="Wrap" Text="kb" VerticalAlignment="Top" Width="35" Height="20"/>
        </Grid>
    </TabItem>
    <TabItem Header="Extension">
        <Grid>
            <TextBlock HorizontalAlignment="Left" Margin="17,20,0,0" TextWrapping="Wrap" Text="Description:" VerticalAlignment="Top" Height="20" FontWeight="Bold"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,40,0,0" TextWrapping="Wrap" Text="This tool will add a tag to all selected games that its media type is the entered extensionn" VerticalAlignment="Top" Width="303"/>
            <TextBox Name="BoxExtension" HorizontalAlignment="Left" Height="20" Margin="17,120,0,-12.8" TextWrapping="Wrap" Text="" VerticalAlignment="Top" Width="123"/>
            <TextBlock HorizontalAlignment="Left" Margin="17,90,0,0" TextWrapping="Wrap" VerticalAlignment="Top" Width="260" Height="20"><Run Text="Enter "/><Run Text="image extension"/></TextBlock>
        </Grid>
    </TabItem>
</TabControl>
<Button Content="Add Tags" HorizontalAlignment="Left" VerticalAlignment="Top" Margin="224,357,0,0" Name="ButtonAddTags" IsDefault="True"/>
</Grid>
"@

    # Load the xaml for controls
    $XMLReader = (New-Object System.Xml.XmlNodeReader $Xaml)
    $XMLForm = [Windows.Markup.XamlReader]::Load($XMLReader)

    # Make variables for each control
    # $Control = $XMLForm.FindName("CbGameSelection")
    $Xaml.FirstChild.SelectNodes("//*[@Name]") | ForEach-Object {Set-Variable -Name $_.Name -Value $XMLForm.FindName($_.Name) }

	# Set Window creation options
	$WindowCreationOptions = New-Object Playnite.SDK.WindowCreationOptions
    $WindowCreationOptions.ShowCloseButton = $true
    $WindowCreationOptions.ShowMaximizeButton = $False
	$WindowCreationOptions.ShowMinimizeButton = $False

	# Create window
	$global:Window = $PlayniteApi.Dialogs.CreateWindow($WindowCreationOptions)
	$global:Window.Content = $XMLForm
	$global:Window.Width = 560
	$global:Window.Height = 460
    $global:Window.Title = "Game Media Tools"
    
	# Set the owner so we can center it using startup location
	$global:Window.Owner = $PlayniteApi.Dialogs.GetCurrentAppWindow()
	$global:Window.WindowStartupLocation = [System.Windows.WindowStartupLocation]::CenterOwner
    $global:Window.ResizeMode = [System.Windows.ResizeMode]::NoResize
    
    # Handler for pressing "Add Tags" button
    $ButtonAddTags.Add_Click(
    {
        # Get the variables from the controls
        $GameSelection = $CbGameSelection.SelectedIndex 
        $MediaTypeSelection = $CbMediaType.SelectedIndex
        $ToolSelection = $ControlTools.SelectedIndex

        # Set GameDatabase
        switch ($GameSelection) {
            0 { $GameDatabase = $PlayniteApi.Database.Games }
            1 { $GameDatabase = $PlayniteApi.MainView.SelectedGames }
        }

        # Set Media Type
        switch ($MediaTypeSelection) {
            0 { 
                $MediaType = "Cover"
                $OptimizedSize = 1
            }
            1 {
                $MediaType = "Background"
                $OptimizedSize = 4
            }
            2 {
                $MediaType = "Icon"
                $OptimizedSize = 0.1
            }
        }

        # Set Tool
        switch ($ToolSelection) {
            0 { # Tool #0: Missing Media
                # Start Game Media Tools function
                Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
                $PlayniteApi.Dialogs.ShowMessage("Finished.", "Game Media Tools");
            }
            1 { # Tool #1: Check Optimization
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
                Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
                $PlayniteApi.Dialogs.ShowMessage("Finished.", "Game Media Tools");
            }
            2 { # Tool #2: Image Aspect Ratio
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
                    Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
                    $PlayniteApi.Dialogs.ShowMessage("Finished.", "Game Media Tools");
                }
                else
                {
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in Width and height Input boxes.", "Game Media Tools");
                }
            }
            3 { # Tool #3: Resolution
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
                    Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
                    $PlayniteApi.Dialogs.ShowMessage("Finished.", "Game Media Tools");
                }
                else
                {
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in Width and height Input boxes.", "Game Media Tools");
                }
            }
            4 { # Tool #4: Image Size
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
                    Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
                    $PlayniteApi.Dialogs.ShowMessage("Finished.", "Game Media Tools");
                }
                else
                {
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in size input box.", "Game Media Tools");
                }
            }
            5 { # Tool #5: Extension
                $Extension = $BoxExtension.Text
                if ($Extension -match "^.+$")
                {
                    # Set tag Name
                    $TagTitle = "ImgExt"
                    $TagDescription = "is $Extension"
                    $TagName = "$TagTitle`: $MediaType $TagDescription"

                    # Tool Information
                    $ToolFunctionName = "ToolImageExtension"
                    $AditionalOperation = "ImagePath"
                    $ExtraParameters = @(
                        $Extension
                    )
                    
                    # Start Game Media Tools function
                    Invoke-GameMediaTools $GameDatabase $MediaType $TagName $ToolFunctionName $AditionalOperation $ExtraParameters
                    $PlayniteApi.Dialogs.ShowMessage("Finished.", "Game Media Tools");
                }
                else
                {
                    $PlayniteApi.Dialogs.ShowMessage("Invalid Input in extension input box.", "Game Media Tools");
                }
            }
        }
    })

    # Show Window
    $global:Window.ShowDialog()

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
    
    # Set Counters
    $global:CountNoMedia = 0
    $global:CountNoMediaBefore = 0
    $global:CountAddedTag = 0
    $global:CountRemovedTag = 0

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
                &$ToolFunctionName $ImageWidth $ImageHeight $ExtraParameters
            }
            elseif ($AditionalOperation -eq "ImagePath")
            {
                &$ToolFunctionName $ImageFilePath $ExtraParameters
            }
            else
            {
                continue
            }

            # Set-ToolTagsToGame
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
        $__logger.Info("Game Media Tools - $($game.name): Error `"$ErrorMessage`" when processing image `"$ImageFilePath`"")
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
    else {
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
        $global:TagOperation = "AddTag"
    }
    else
    {
        $global:TagOperation = "RemoveTag"
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
        $global:TagOperation = "RemoveTag"
    }
    else
    {
        $global:TagOperation = "AddTag"
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
        $global:TagOperation = "RemoveTag"
    }
    else
    {
        $global:TagOperation = "AddTag"
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
        $global:TagOperation = "AddTag"
    }
    else
    {
        $global:TagOperation = "RemoveTag"
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
        $global:TagOperation = "AddTag"
    }
    else
    {
        $global:TagOperation = "RemoveTag"
    }
}