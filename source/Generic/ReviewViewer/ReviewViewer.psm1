function GetGameMenuItems
{
    param(
        $getGameMenuItemsArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCReview_Viewer_MenuItemYoutubeReviewDescription")
    $menuItem.FunctionName = "Invoke-ReviewViewer"
    $menuItem.MenuSection = "Video"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem2.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCReview_Viewer_MenuItemYoutubeTrailerDescription")
    $menuItem2.FunctionName = "Invoke-TrailerViewer"
    $menuItem2.MenuSection = "Video"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem3.Description =  [Playnite.SDK.ResourceProvider]::GetString("LOCReview_Viewer_MenuItemReviewViewerDescription")
    $menuItem3.FunctionName = "Invoke-ReviewReader"

    return $menuItem, $menuItem2, $menuItem3
}

function Invoke-ReviewViewer
{
    param(
        $scriptGameMenuItemActionArgs
    )

    Get-YoutubeVideoId "Review"
}
function Invoke-TrailerViewer
{
    param(
        $scriptMainMenuItemActionArgs
    )

    Get-YoutubeVideoId "Trailer"
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
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCReview_Viewer_GenericFileDownloadError") -f $url, $errorMessage))
        return
    }
}

function Get-YoutubeVideoId
{
    param (
        $videoType
    )


    $game = $PlayniteApi.MainView.SelectedGames | Select-Object -last 1

    $query = "{0}+{1}" -f [uri]::EscapeDataString($game.name),  $videoType
    $uri = "https://www.youtube.com/results?search_query={0}" -f $query
    $webContent = Get-DownloadString $uri
    if ($null -eq $webContent)
    {
        exit
    }

    $webContent -match '"videoId":"((.+?(?=")))"'
    if ($matches)
    {
        Invoke-YoutubeVideo $matches[1]
    }
    else
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCReview_Viewer_NoVideoFoundMessage")), $ExtensionName);
        exit
    }
}

function Invoke-YoutubeVideo
{
    param (
        [string] $videoId
    )


    $youtubeLink = "https://www.youtube-nocookie.com/embed/{0}" -f $videoId
    
    # Generate html
    $html = "
    <head>
        <title>Review Viewer</title>
        <meta http-equiv='refresh' content='0; url=$youtubeLink'>
    </head>
    <body style='margin:0'>
    </body>"

    $webView = $PlayniteApi.WebViews.CreateView(1280, 750)
    $webView.Navigate("data:text/html," + $html)
    $webView.OpenDialog()
    $webView.Dispose()
}

function Get-SteamReviews
{
    param (
        $steamAppId,
        $steamApiKey,
        $reviewType
    )

    $uri = "https://store.steampowered.com/appreviews/{0}?json=1&purchase_type=all&language=english&review_type={1}&playtime_filter_min=0&filter=summary" -f $steamAppId, $reviewType
    $reviews = Get-DownloadString $uri | ConvertFrom-Json

    [System.Collections.Generic.List[string]]$profileIds = @()
    $reviews.reviews | ForEach-Object {
        $profileIds.Add($_.Author.steamid)
    }

    if (![string]::IsNullOrEmpty($steamApiKey))
    {
        $playersSummariesUrl = "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}" -f $steamApiKey, ($profileIds -join ",%20")
        $playersSummaries = Get-DownloadString $playersSummariesUrl | ConvertFrom-Json
    }

    [System.Collections.Generic.List[System.Object]]$reviewsArray = @()
    foreach ($review in $reviews.reviews)
    {
        if ($null -ne $playersSummaries)
        {
            $playerSummary = $playersSummaries.response.players | Where-Object {$_.steamid -eq $review.Author.steamid}
            $authorName = $playerSummary.personaname
            $authorImage = $playerSummary.avatarmedium
        }
        else
        {
            $authorName = "Not available"
            $authorImage = [System.IO.Path]::Combine($CurrentExtensionInstallPath, "ProfilePictureUnknown.jpg")
        }

        $playTime =  [timespan]::FromMinutes($review.Author.playtime_forever).ToString("hh\:mm").Split(":")
        switch ($review.voted_up) {
            $true { $veredict = "Recommended" ; $reviewGroup = "Positive"}
            $false { $veredict = "Not recommended" ; $reviewGroup = "Negative"}
            Default {}
        }

        $review = [PSCustomObject]@{
            AuthorName = $authorName
            AuthorUrl = "https://steamcommunity.com/profiles/{0}" -f $review.Author.steamid
            AuthorImage = $authorImage
            ReviewDate = (Get-Date 01.01.1970).AddSeconds($review.timestamp_created).tostring("MM-dd-yyy")
            ReviewVeredict = $veredict
            ReviewGroup = $reviewGroup
            ReviewPlaytime = "{0} hours {1} minutes" -f $playTime[0], $playTime[1]
            ReviewText = $review.review -replace '\[[^\]]+\]', ''
            ReviewHelpfulness = "{0} people found this review helpful, {1} people found this review funny" -f $review.votes_up, $review.votes_funny
            ReviewUrl = "https://steamcommunity.com/profiles/{0}/recommended/{1}/" -f $review.Author.steamid, $steamAppId
        }
        $reviewsArray.Add($review) | Out-Null
    }

    return $reviewsArray
}

function Get-SteamDbRating
{
    param (
        $totalPositiveReviews,
        $totalReviews
    )

    $average = $totalPositiveReviews / $totalReviews
    $steamDbRating = ($average - ( $average - 0.5 ) * ( [Math]::Pow(2,-([Math]::Log10( $totalReviews + 1 ))) ) ) * 100

    return $steamDbRating
}

function Get-SteamReviewsSummary
{
    param (
        $steamAppId
    )

    $uri = "https://store.steampowered.com/appreviews/{0}?json=1&purchase_type=all&language=all" -f $steamAppId
    $apiRequest = Get-DownloadString $uri 
    if ($null -eq $apiRequest)
    {
        return $null
    }
    $json = $apiRequest | ConvertFrom-Json
    if (($json.success -eq 2) -or ( $json.query_summary.total_reviews -eq 0))
    {
        $__logger.Warn("`"$($steamAppId)`" has been removed from Steam or doesn't have reviews")
        return $null
    }

    $usersReviews = [PSCustomObject]@{
        reviewsTotal = $json.query_summary.total_reviews
        reviewsPositive = $json.query_summary.total_positive
        reviewsNegative = $json.query_summary.total_negative
        reviewsAverage = ($json.query_summary.total_positive * 100)/$json.query_summary.total_reviews
    }

    $reviewsSummary = [PSCustomObject]@{
        criticReviews = $null
        userReviews = $usersReviews
    }

    return $reviewsSummary
}


function Invoke-ReviewReader
{   
    param(
        $scriptGameMenuItemActionArgs
    )
    
    # Set GameDatabase
    $game = $PlayniteApi.MainView.SelectedGames[-1]

    [System.Collections.ArrayList]$reviewSources = @()
    $reviewSource = [PSCustomObject]@{
        Name = "Steam"
        Value = "Steam"
    }
    $reviewSources.Add($reviewSource)

    [System.Collections.ArrayList]$steamReviewTypes = @()
    $reviewType = [PSCustomObject]@{
        Name = "All reviews"
        Value = "all"
    }
    $steamReviewTypes.Add($reviewType)
    $reviewType = [PSCustomObject]@{
        Name = "Only positive reviews"
        Value = "positive"
    }
    $steamReviewTypes.Add($reviewType)
    $reviewType = [PSCustomObject]@{
        Name = "Only negative reviews"
        Value = "negative"
    }
    $steamReviewTypes.Add($reviewType)

    $PlayniteSteamSettings = [System.IO.File]::ReadAllLines([System.IO.Path]::Combine($PlayniteApi.Paths.ConfigurationPath, "ExtensionsData", "cb91dfc9-b977-43bf-8e70-55f46e410fab", "config.json")) | ConvertFrom-Json
    $steamApiKey = ""
    if ($PlayniteSteamSettings.ApiKey)
    {
        $steamApiKey = $PlayniteSteamSettings.ApiKey
    }
    $steamAppId = Get-SteamAppId $game

    # Load assemblies
    Add-Type -AssemblyName PresentationCore
    Add-Type -AssemblyName PresentationFramework

    # Set Xaml
    [xml]$Xaml = @"
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <Grid.Resources>
        <Style TargetType="TextBlock" BasedOn="{StaticResource BaseTextBlockStyle}" />
    </Grid.Resources>
    <Grid Margin="20" >
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="250" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>
        <Grid Grid.Column="0">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"  />
                <RowDefinition Height="Auto" />
                <RowDefinition Height="*" />
                <RowDefinition Height="Auto" />
            </Grid.RowDefinitions>
            <Image Name="ImageGameCover" Grid.Row="0" Stretch="Uniform" MaxHeight="250" VerticalAlignment="Top" HorizontalAlignment="Left"/>
            <ListBox Name="ListBoxReviews" Grid.Row="0" DisplayMemberPath="AuthorName" Height="300" Visibility="Collapsed"/>
            <TextBlock Name="TextBoxGameName" Grid.Row="1" Margin="0,20,0,0" TextWrapping="NoWrap" TextTrimming="CharacterEllipsis" FontWeight="Bold"/>
            <StackPanel Grid.Row="2" Margin="0,20,0,0">
                <GridEx ColumnCount="2" StarColumns="1" RowCount="3" AutoLayoutColumns="2" VerticalAlignment="Top">
                    <TextBlock Text="{DynamicResource LOCCriticScore}" Margin="0,0,0,0"/>
                    <TextBlock Name="TextBoxCriticScore" Text="-" Margin="15,0,0,0"/>
                    <TextBlock Text="{DynamicResource LOCCommunityScore}" Margin="0,0,0,0"/>
                    <TextBlock Name="TextBoxCommunityScore" Text="-" Margin="15,0,0,0"/>
                    <TextBlock Text="{DynamicResource LOCUserScore}" Margin="0,0,0,0"/>
                    <TextBlock Name="TextBoxUserScore" Text="-" Margin="15,0,0,0"/>
                </GridEx>
                <Separator Margin="0,20,0,0" Height="1"/>
                <TextBlock Name="TextBoxSourceNameReviewsSection" Margin="0,10,0,0" TextWrapping="NoWrap" TextTrimming="CharacterEllipsis"/>
                <GridEx ColumnCount="2" Margin="0,20,0,0" StarColumns="1" RowCount="5" AutoLayoutColumns="2" VerticalAlignment="Top">
                    <TextBlock Text="{DynamicResource LOCReview_Viewer_TotalUserReviews}" Margin="0,0,0,0"/>
                    <TextBlock Name="SourceTotalUserReviews" Text="-" Margin="15,0,0,0"/>
                    <TextBlock Text="{DynamicResource LOCReview_Viewer_PositiveUserReviews}" Margin="0,0,0,0"/>
                    <TextBlock Name="SourcePositiveUserReviews" Text="-" Margin="15,0,0,0"/>
                    <TextBlock Text="{DynamicResource LOCReview_Viewer_NegativeUserReviews}" Margin="0,0,0,0"/>
                    <TextBlock Name="SourceNegativeUserReviews" Text="-" Margin="15,0,0,0"/>
                    <TextBlock Text="{DynamicResource LOCReview_Viewer_AverageUserScore}" Margin="0,0,0,0"/>
                    <TextBlock Name="SourceAverageUserScore" Text="-" Margin="15,0,0,0"/>
                    <TextBlock Text="{DynamicResource LOCReview_Viewer_SteamDbScore}" Margin="0,0,0,0"/>
                    <TextBlock Name="SourceSteamDbScore" Text="-" Margin="15,0,0,0"/>
                </GridEx>
            </StackPanel>
            <Button Name="ButtonInvokeYoutubeReview" Grid.Row="3" Margin="0,10,0,0" Width="Auto" VerticalAlignment="Bottom" Content="{DynamicResource LOCReview_Viewer_OpenReview}"/>
        </Grid>
        <Grid Grid.Column="1" Margin="10,0,0,0">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" />
                <RowDefinition Height="60" />
                <RowDefinition Height="*"/>
                <RowDefinition Height="50"/>
                <RowDefinition Height="60"/>
            </Grid.RowDefinitions>
            <StackPanel>
                <DockPanel LastChildFill="False">
                    <TextBlock Text="{DynamicResource LOCReview_Viewer_ReviewsSource}" FontWeight="Bold" Margin="0,0,0,0" VerticalAlignment="Center"/>
                    <ComboBox Name="CbReviewSources" DisplayMemberPath="Name" Margin="10,0,0,0" Width="Auto" MinWidth="130"/>
                    <TextBlock Text="{DynamicResource LOCReview_Viewer_ReviewsType}" FontWeight="Bold" Margin="10,0,0,0" VerticalAlignment="Center"/>
                    <ComboBox Name="CbReviewTypes" DisplayMemberPath="Name" Margin="10,0,0,0" Width="Auto" MinWidth="180"/>
                </DockPanel>
                <DockPanel Margin="0,10,0,0">
                </DockPanel>
            </StackPanel>
            <DockPanel Grid.Row="1" Background="#3A3E44" Margin="0,10,0,0">
                <Image Name="ImageAuthorImage" Cursor="Hand" Margin="5" Width="40" Stretch="Uniform" Source="{Binding ElementName=ListBoxReviews, Path=SelectedItem.AuthorImage}" VerticalAlignment="Center"/>
                <StackPanel VerticalAlignment="Center" Margin="15,0,0,0">
                    <DockPanel>
                        <TextBlock Text="Author:" FontWeight="Bold" Margin="0,0,0,0"/>
                        <TextBlock Name="TextBlockAuthor" Cursor="Hand" Text="{Binding ElementName=ListBoxReviews, Path=SelectedItem.AuthorName}" Margin="10,0,0,0"/>
                    </DockPanel>
                    <DockPanel>
                        <TextBlock Text="Review date:" FontWeight="Bold" Margin="0,0,0,0"/>
                        <TextBlock Text="{Binding ElementName=ListBoxReviews, Path=SelectedItem.ReviewDate}" Margin="10,0,0,0"/>
                    </DockPanel>
                </StackPanel>
            </DockPanel>
            <Grid Grid.Row="2" Margin="0,20,0,0">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="*" />
                </Grid.RowDefinitions>
                <DockPanel Grid.Row="0">
                    <TextBlock Text="{DynamicResource LOCReview_Viewer_Review}" FontWeight="Bold" Margin="0,0,0,0" VerticalAlignment="Center" DockPanel.Dock="Left"/>
                    <Button DockPanel.Dock="Right" Name="ButtonNextReview" Background="Transparent" BorderThickness="0" Cursor="Hand" Content="&#xEACA;" FontSize="40" FontFamily="{DynamicResource FontIcoFont}" HorizontalAlignment="Right" Margin="5,0,0,0" Padding="0,0,0,0"/>
                    <Button DockPanel.Dock="Right" Name="ButtonPreviousReview" Background="Transparent" BorderThickness="0" Cursor="Hand" Content="&#xEAC9;" FontSize="40" FontFamily="{DynamicResource FontIcoFont}" HorizontalAlignment="Right" Margin="10,0,0,0" Padding="0,0,0,0"/>
                    <TextBlock DockPanel.Dock="Right" Name="TextBlockReviewsCount" Margin="10,0,0,0" TextWrapping="NoWrap" FontWeight="Bold" VerticalAlignment="Center"/>
                    <Separator Margin="10,0,0,0" Height="1"/>
                </DockPanel>
                <ScrollViewer Name="ScrollReviewText" VerticalScrollBarVisibility="Auto" Margin="0,10,0,0" Grid.Row="1">
                    <TextBlock TextWrapping="Wrap" Text="{Binding ElementName=ListBoxReviews, Path=SelectedItem.ReviewText}"/>
                </ScrollViewer>
            </Grid>
            <DockPanel Grid.Row="3" Margin="0,20,0,0">
                <TextBlock DockPanel.Dock="Right" Name="TextBlockOpenReview" Cursor="Hand" TextWrapping="NoWrap" Text="{DynamicResource LOCReview_Viewer_OpenReview}" VerticalAlignment="Center"/>
                <TextBlock FontStyle="Italic" TextWrapping="NoWrap" Margin="0,0,20,0" Text="{Binding ElementName=ListBoxReviews, Path=SelectedItem.ReviewHelpfulness}" VerticalAlignment="Center"/>
            </DockPanel>
            <DockPanel Grid.Row="4" Background="#3A3E44" Margin="0,10,0,0">
                <Image Margin="5" Name="ImageIconReview" Width="40" Stretch="Uniform" VerticalAlignment="Center" Visibility="Hidden"/>
                <StackPanel VerticalAlignment="Center" Margin="15,0,0,0">
                    <DockPanel>
                        <TextBlock Text="Veredict:" FontWeight="Bold" Margin="0,0,0,0"/>
                        <TextBlock Text="{Binding ElementName=ListBoxReviews, Path=SelectedItem.ReviewVeredict}" Margin="10,0,0,0"/>
                    </DockPanel>
                    <DockPanel>
                        <TextBlock Text="Playtime:" FontWeight="Bold" Margin="0,0,0,0"/>
                        <TextBlock Text="{Binding ElementName=ListBoxReviews, Path=SelectedItem.ReviewPlaytime}" Margin="10,0,0,0"/>
                    </DockPanel>
                </StackPanel>
            </DockPanel>
        </Grid>
    </Grid>
</Grid>
"@

    # Load the xaml for controls
    $XMLReader = [System.Xml.XmlNodeReader]::New($Xaml)
    $XMLForm = [Windows.Markup.XamlReader]::Load($XMLReader)

    # Make variables for each control
    $Xaml.FirstChild.SelectNodes("//*[@Name]") | ForEach-Object {Set-Variable -Name $_.Name -Value $XMLForm.FindName($_.Name) }

    # Set items sources of controls
    $CbReviewSources.ItemsSource = $reviewSources

    if ($game.CoverImage)
    {
        if ($game.CoverImage -match "^http")
        {
            $ImageGameCover.Source = $game.CoverImage
        }
        else
        {
            $ImageGameCover.Source = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
        }
    }

    $TextBoxGameName.Text = $game.Name
    if ($game.CriticScore)
    {
        $TextBoxCriticScore.Text = $game.CriticScore.ToString()
    }
    if ($game.CommunityScore)
    {
        $TextBoxCommunityScore.Text = $game.CommunityScore.ToString()
    }
    if ($game.UserScore)
    {
        $TextBoxUserScore.Text = $game.UserScore.ToString()
    }

    # Set Window creation options
    $windowCreationOptions = New-Object Playnite.SDK.WindowCreationOptions
    $windowCreationOptions.ShowCloseButton = $true
    $windowCreationOptions.ShowMaximizeButton = $true
    $windowCreationOptions.ShowMinimizeButton = $false

    # Create window
    $window = $PlayniteApi.Dialogs.CreateWindow($windowCreationOptions)
    $window.Content = $XMLForm
    $window.Width = 1000
    $window.Height = 650
    $window.Title = "Review Viewer"
    $window.WindowStartupLocation = "CenterScreen"

    $ImageAuthorImage.Add_MouseLeftButtonDown(
    {
        if ($ListBoxReviews.SelectedItem.AuthorUrl)
        {
            Start-Process $ListBoxReviews.SelectedItem.AuthorUrl
        }
    })
    $TextBlockAuthor.Add_MouseLeftButtonDown(
    {
        if ($ListBoxReviews.SelectedItem.AuthorUrl)
        {
            Start-Process $ListBoxReviews.SelectedItem.AuthorUrl
        }
    })
    $TextBlockOpenReview.Add_MouseLeftButtonDown(
    {
        if ($ListBoxReviews.SelectedItem.ReviewUrl)
        {
            Start-Process $ListBoxReviews.SelectedItem.ReviewUrl
        }
    })

    # Handler for CbReviewSources
    $CbReviewSources.Add_SelectionChanged(
    {
        $reviewsSummary = $null
        if ($CbReviewSources.SelectedItem.Name -eq "Steam")
        {
            $CbReviewTypes.ItemsSource = $steamReviewTypes
            if ($null -ne $steamAppId)
            {
                $reviewsSummary = Get-SteamReviewsSummary $steamAppId
            }

        }

        if ($null -ne $reviewsSummary)
        {
            $SourceTotalUserReviews.Text = $reviewsSummary.userReviews.reviewsTotal.ToString('N0')
            $SourcePositiveUserReviews.Text = $reviewsSummary.userReviews.reviewsPositive.ToString('N0')
            $SourceNegativeUserReviews.Text = $reviewsSummary.userReviews.reviewsNegative.ToString('N0')
            $SourceAverageUserScore.Text = "{0} %" -f ([math]::Round($reviewsSummary.userReviews.reviewsAverage, 2)).ToString()
            $SourceSteamDbScore.Text = "{0} %" -f ([math]::Round((Get-SteamDbRating $reviewsSummary.userReviews.reviewsPositive $reviewsSummary.userReviews.reviewsTotal), 2)).ToString()
        }

        $TextBoxSourceNameReviewsSection.Text = $CbReviewSources.SelectedItem.Name
        if ($CbReviewTypes.Items.Count -gt 0)
        {
            $CbReviewTypes.SelectedIndex = 0
        }
    })

    # Handler for CbReviewSources
    $CbReviewTypes.Add_SelectionChanged(
    {
        if ($CbReviewTypes.SelectedItem)
        {
            if ($CbReviewSources.SelectedItem.Name -eq "Steam")
            {
                if ($steamAppId)
                {
                    $ListBoxReviews.ItemsSource = @(Get-SteamReviews $steamAppId $steamApiKey $CbReviewTypes.SelectedItem.Value)
                }
                else
                {
                    $ListBoxReviews.ItemsSource = $null
                    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCExtra_Metadata_tools_ThemeConstantsUpdatedMessage") -f $game.Name))
                }
            }

            if ($ListBoxReviews.Items.Count -ge 1)
            {
                $ListBoxReviews.SelectedIndex = 0
            }
        }
    })

    $CbReviewSources.SelectedIndex = 0

    # Handler for ListBoxReviews
    $ListBoxReviews.Add_SelectionChanged(
    {
        $ScrollReviewText.ScrollToTop()
        if ($null -ne $ListBoxReviews.SelectedItem.ReviewGroup)
        {
            $ImageIconReview.Visibility = "Visible"
        }
        else
        {
            $ImageIconReview.Visibility = "Collapsed"
        }

        switch ($ListBoxReviews.SelectedItem.ReviewGroup) {
            "Positive" { $ImageIconReview.Source = [System.IO.Path]::Combine($CurrentExtensionInstallPath, "IconReviewPositive.png") }
            "Mixed" { $ImageIconReview.Source = [System.IO.Path]::Combine($CurrentExtensionInstallPath, "IconReviewMixed.png") }
            "Negative" { $ImageIconReview.Source = [System.IO.Path]::Combine($CurrentExtensionInstallPath, "IconReviewNegative.png") }
            Default {}
        }

        $TextBlockReviewsCount.Text = "{0}/{1}" -f ($ListBoxReviews.SelectedIndex + 1), $ListBoxReviews.Items.Count
    })

    if ($ListBoxReviews.Items.Count -ge 1)
    {
        $ListBoxReviews.SelectedIndex = 0
        $TextBlockReviewsCount.Text = "{0}/{1}" -f ($ListBoxReviews.SelectedIndex + 1), $ListBoxReviews.Items.Count

        if ($null -ne $ListBoxReviews.SelectedItem.ReviewGroup)
        {
            $ImageIconReview.Visibility = "Visible"
        }
        else
        {
            $ImageIconReview.Visibility = "Collapsed"
        }
        switch ($ListBoxReviews.SelectedItem.ReviewGroup) {
            "Positive" { $ImageIconReview.Source = [System.IO.Path]::Combine($CurrentExtensionInstallPath, "IconReviewPositive.png") }
            "Mixed" { $ImageIconReview.Source = [System.IO.Path]::Combine($CurrentExtensionInstallPath, "IconReviewMixed.png") }
            "Negative" { $ImageIconReview.Source = [System.IO.Path]::Combine($CurrentExtensionInstallPath, "IconReviewNegative.png") }
            Default {}
        }
    }

    # Handler for pressing "Next Review" button
    $ButtonNextReview.Add_Click(
    {
        if ($ListBoxReviews.Items.Count -gt 1)
        {
            $ListBoxIndex = $ListBoxReviews.SelectedIndex
            if (($ListBoxReviews.Items.Count - 1) -eq $ListBoxIndex)
            {
                $ListBoxReviews.SelectedIndex = 0
            }
            else
            {
                $ListBoxReviews.SelectedIndex = $ListBoxIndex + 1
            }
        }
    })

    # Handler for pressing "Next Review" button
    $ButtonPreviousReview.Add_Click(
    {
        if ($ListBoxReviews.Items.Count -gt 1)
        {
            $ListBoxIndex = $ListBoxReviews.SelectedIndex
            if ($ListBoxReviews.SelectedIndex -eq 0)
            {
                $ListBoxReviews.SelectedIndex = $ListBoxReviews.Items.Count - 1
            }
            else
            {
                $ListBoxReviews.SelectedIndex = $ListBoxIndex - 1
            }
        }
    })

    $ButtonInvokeYoutubeReview.Add_Click(
    {
        Invoke-ReviewViewer
    })

    # Show Window
    $window.ShowDialog()
    $window = $null
    [System.GC]::Collect()
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

    if ($null -eq $steamAppList)
    {
        Set-GlobalAppList $false
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