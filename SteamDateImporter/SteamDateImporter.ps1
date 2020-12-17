function global:GetMainMenuItems
{
    param($menuArgs)

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Import dates of selected games"
    $menuItem1.FunctionName = "Invoke-SteamDateImporterSelected"
    $menuItem1.MenuSection = "@Steam Date Importer"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = "Import dates of all games"
    $menuItem2.FunctionName = "Invoke-SteamDateImporterAll"
    $menuItem2.MenuSection = "@Steam Date Importer"
    
    return $menuItem1, $menuItem2
}

function Add-SteamDates
{
    param (
        $gameDatabase
    )
    
    # Create prefix strings to remove
    [System.Collections.Generic.List[string]]$RemoveStrings = @(
        
        # Regions
        " \(Latam\)",
        " \(LATAM\/IN\)",
        " \(LATAM\/RU\/CN\/IN\/TR\)",
        " Latam",
        " \(US\)",
        " \(US\/AU\)",
        " \(NA\)",
        " \(NA\+ROW\)",
        " \(ROW Launch\)",
        " \(ROW\)",
        " \(Rest of World\)",
        " ROW Release",
        " ROW",
        " \(RU\)",
        " RU",
        " \(South America\)",
        " SA",
        " \(WW\)",
        " WW Digital Distribution",
        " \(Key-only WW\)",
        " WW"

        # Release type
        " Collection Retail",
        " \(Retail\)",
        " Retail Key",
        " Retail Rtd",
        " - The Full Package Retail",
        " \(Digital Retail\)",
        " \[DIGITAL RETAIL\]",
        " - [Digital]",
        " [Digital]"
        " \(preorder\)",
        " \(Pre-Order\)",
        " \(pre-purchase\)",
        " Pre-Purchase",
        " \(Post-Launch\)",
        " Post-Launch",
        " CD key",
        " Retail",
        " Digital"

        # Free
        " - Free Giveaway",
        " - Free for 24 Hours",
        " - Free For A Limited Time!",
        " \(Free\)",
        " Free Giveaway",
        " Free"

        # Editions
        " Deluxe Edition",
        " Complete Edition",
        " Standard Edition",
        " Voiced Edition",
        " - Digital Edition of Light",
        "  Digital Edition",
        " Digital Distribution"
        " Day One Edition",
        " Enhanced Edition"
        " - Legacy Edition",
        " - Starter Pack",
        " - Starter Edition",
        " Special Edition",
        " Standard",
        " Launch",
        " Paper's Cut Edition",
        " - War Chest Edition",
        " - Special Steam Edition",
        ": Assassins of Kings Enhanced Edition",
        " - Beta",
        " for Beta Testing"

        # Other
        " \(Rebellion Store\)",
        " PROMO",
        " Gift Copy - Hades Purchase",
        " - Gift",
        " Gift",
        " Steam Store and Retail Key",
        " \(100% off week\)",
        " - Complimentary \(Opt In\)",
        " - Holiday Pack",
        " Bundle (Summer 2012)",
        ": REVENGEANCE",
        " Care Package",
        " Complete Season (Episodes 1-5)",
        " Deluxe - Includes OST and an exclusive Artbook",
        " Free On Demand",
        " 4-Pack",
        " 2 Pack",
        " Free Edition - Français + Italiano + 한국어 [Learn French + Italian + Korean]",
        ": 20 Year Celebration",
        ": Complete Story",
        " Free Access"
    )

    # Create prefix strings to remove that have dates
    [System.Collections.Generic.List[string]]$Months = @(
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Aug", "Sep", "Oct", "Nov", "Dec"
    )
    [System.Collections.Generic.List[string]]$Years = @(
        "2016", "2017", "2018", "2019", "2020", "2021"
    )
    foreach ($year in $Years) {
        foreach ($month in $months) {
            $LimitedFree = " Limited Free Promotional Package - " + "$month " + "$year"
            $RemoveStrings.Add($LimitedFree)
        }
    }

    # Use Webview to get licenses page content (Offscreen)
    $LicensesUrl = 'https://store.steampowered.com/account/licenses/?l=english'
    $webView = $PlayniteApi.WebViews.CreateOffscreenView()
    $webView.NavigateAndWait($LicensesUrl)
    $LicensesHtmlContent = $webView.GetPageSource()
    $webView.Close()

    # Use regex to get all licenses
    $regex = '(?:<td class="license_date_col">)(.*?(?=<\/td>))(?:<\/td>\s+<td>)(?:\s+<div class="free_license_remove_link">(?:[\s\S]*?(?=<\/div>))<\/div>)?(?:\s+)([^\t]+)'
    $LicenseMatches = ([regex]$regex).Matches($LicensesHtmlContent)
    if ($LicenseMatches.count -eq 0)
    {
        # Use Webview to log in
        $__logger.Info("Steam Date Importer - No licenses found in first try, WebView will be opened")
        $PlayniteApi.Dialogs.ShowMessage("A web browser window will be opened, please close the window after login in to Steam.", "Steam Date Importer");
        $LicensesUrl = 'https://store.steampowered.com/account/licenses/'
        $webView = $PlayniteApi.WebViews.CreateView(1020, 600)
        $webView.Navigate($LicensesUrl)
        $webView.OpenDialog()
        $webView.Close()

        # Use Webview to get licenses page content (Offscreen)
        $LicensesUrl = 'https://store.steampowered.com/account/licenses/?l=english'
        $webView = $PlayniteApi.WebViews.CreateOffscreenView()
        $webView.NavigateAndWait($LicensesUrl)
        $LicensesHtmlContent = $webView.GetPageSource()
        $webView.Close()
        $LicenseMatches = ([regex]$regex).Matches($LicensesHtmlContent)
        if ($LicenseMatches.count -eq 0)
        {
            $__logger.Info("Steam Date Importer - No licenses found in second try, not logged in or no licenses found")
            $PlayniteApi.Dialogs.ShowMessage("Not logged in or no licenses found", "Steam Date Importer")
            exit
        }
    }
    
    # Create collections of found licenses
    $__logger.Info("Steam Date Importer - Found $($LicenseMatches.count) licenses")
    [System.Collections.Generic.List[object]]$LicensesList = @()
    foreach ($LicenseMatch in $LicenseMatches) {
        
        #Create License name for matching 
        $LicenseNameMatch = $LicenseMatch.Groups[2].value
        foreach ($StringPattern in $RemoveStrings) {
            $LicenseNameMatch = $LicenseNameMatch -replace "($StringPattern)$", ''
        }
        $LicenseNameMatch = $LicenseNameMatch -replace "reg;", '' -replace "amp;", '' -replace "trade;", '' -replace "rsquo;", '' -replace "ndash;", ''
        $LicenseNameMatch = $LicenseNameMatch -replace "ü", 'u' -replace ' and ', '' -replace "Game of the Year", 'GOTY' -replace "(GOTY)$", 'GOTY Edition'
        
        # Create license object and add to collection
        $License = [PSCustomObject]@{
            LicenseName = $LicenseMatch.Groups[2].value
            LicenseNameMatch = $LicenseNameMatch -replace '[^\p{L}\p{Nd}]', ''
            LicenseDate = [datetime]$LicenseMatch.Groups[1].value
        }
        $LicensesList.Add($License)
    }

    # Licenses Export
    $LicenseExportChoice = $PlayniteApi.Dialogs.ShowMessage("Found $($LicenseMatches.count) licenses`nDo you want to export your found Steam licenses?", "Steam Date Importer", 4)
    if ($LicenseExportChoice -eq "Yes")
    {
        $LicenseExportPath = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
        if ($LicenseExportPath)
        {
            if ($LicenseExportPath -match ".csv$")
            {
                $LicensesList | Select-Object LicenseName, LicenseDate | ConvertTo-Csv -NoTypeInformation | Out-File $LicenseExportPath -Encoding 'UTF8'
            }
            else
            {
                $LicensesList | Select-Object LicenseName, LicenseDate | Format-Table -AutoSize | Out-File $LicenseExportPath -Encoding 'UTF8'
            }
            $__logger.Info("Steam Date Importer - Licenses exported to `"$LicenseExportPath`"")
            $PlayniteApi.Dialogs.ShowMessage("Licenses exported succesfully.", "Steam Date Importer");
        }
    }

    # Set GameDatabase and create modified games collection
    [System.Collections.Generic.List[object]]$GameDatesList = @()
    $CountNewDate = 0
    $CountMatchLicense = 0
    $CountNoLicense = 0

    $webView = $PlayniteApi.WebViews.CreateOffscreenView()
    $webView.Navigate("https://help.steampowered.com")
    $sessionIdCookie = $webView.GetCookies() | Where-Object {$_.Domain -eq "help.steampowered.com"} | Where-Object {$_.Name -eq "sessionid"}
    $sessionId = $sessionIdCookie.Value
    $helpTemplate = "https://help.steampowered.com/en/wizard/HelpWithGame/?appid={0}&sessionid={1}&wizard_ajax=1"
    $regexDate = '\\r\\n\\t\\t\\t\\t\\t\\t\\t&lt;span&gt;(.*?(?=&amp;))&amp;nbsp;'

    foreach ($game in $gameDatabase) {
        
        $LicenseDate = $null
        $GameNameMatch = $game.name -replace "ü", 'u' -replace ' and ', '' -replace "Game of the Year", 'GOTY' -replace "(GOTY)$", 'GOTY Edition'
        $GameNameMatch = $GameNameMatch -replace '[^\p{L}\p{Nd}]', ''
        $GameDateOld = $game.Added

        foreach ($License in $LicensesList) {
            if ($License.LicenseNameMatch -eq $GameNameMatch) 
            {
                $LicenseDate = $License.LicenseDate
                break
            }
        }
        if ($null -eq $LicenseDate)
        {
            $helpUrl = $helpTemplate -f $game.GameId, $sessionId
            $webView.NavigateAndWait($helpUrl)
            $DateMatch = ([regex]$regexDate).Matches($webView.GetPageSource())
            if ($DateMatch.Groups.Count -gt 0)
            {
                $LicenseDate = $DateMatch.groups[1].Value
                if ($LicenseDate -notmatch '\w{3} \d+, \d{4}')
                {
                    $LicenseDate = $LicenseDate + ", " + "$(Get-Date -Format yyyy)"
                }
            }
        }

        if ($LicenseDate)
        {
            $CountMatchLicense++
            $DateFound = "True"
            $LicenseDate = [datetime]$LicenseDate
            if ($game.Added -ne $LicenseDate)
            {
                $game.Added = $LicenseDate
                $PlayniteApi.Database.Games.Update($game)
                $GameDateNew = $game.Added
                $DateChanged = "True"
                $CountNewDate++
                $__logger.Info("Steam Date Importer - Changed date of `"$($game.name)`", Old date: `"$GameDateOld`", New date: `"$GameDateNew`"")
            }
            else
            {
                $DateChanged = "False"
                $GameDateNew = $null
            }
        }
        else
        {
            $CountNoLicense++
            $GameDateNew = $null
            $DateChanged = "False"
            $DateFound = "False"
        }
        
        $GameDates = [PSCustomObject]@{
            Name = $game.name
            OldDate = $GameDateOld
            NewDate = $GameDateNew
            DateChanged = $DateChanged
            DateFound = $DateFound
            LicenseDate = $LicenseDate
        }
        $GameDatesList.Add($GameDates)
    }

    # Show finish dialogue with results and ask if user wants to export results
    $__logger.Info("Steam Date Importer - Finished. Processed Steam games: $($GameDatabase.count). Games with date found: $CountMatchLicense`nGames without date found: $CountNoLicense. Games that had the added date changed: $CountNewDate")
    $ExportChoice = $PlayniteApi.Dialogs.ShowMessage("Processed Steam games: $($GameDatabase.count)`n`nGames with date found: $CountMatchLicense`nGames without date found: $CountNoLicense`nGames that had the added date changed: $CountNewDate`n`nDo you want to export results?", "Steam Date Importer", 4)
    if ($ExportChoice -eq "Yes")
    {
        $ExportPath = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
        if ($ExportPath)
        {
            if ($ExportPath -match "\.csv$")
            {
                $GameDatesList | Select-Object Name, OldDate, NewDate, DateChanged, DateFound, LicenseDate | Sort-Object -Property DateFound, Name | ConvertTo-Csv -NoTypeInformation | Out-File $ExportPath -Encoding 'UTF8'
            }
            else
            {
                $GameDatesList | Select-Object Name, OldDate, NewDate, DateChanged, DateFound, LicenseDate | Sort-Object -Property DateFound, Name | Format-Table -AutoSize | Out-File $ExportPath -Encoding 'UTF8'
            }
            $__logger.Info("Steam Date Importer - Results exported to `"$ExportPath`"")
            $PlayniteApi.Dialogs.ShowMessage("Results exported successfully.", "Steam Date Importer");
        }
    }
}

function Invoke-SteamDateImporterSelected
{
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.PluginId -eq "cb91dfc9-b977-43bf-8e70-55f46e410fab"}
    Add-SteamDates $gameDatabase
}

function Invoke-SteamDateImporterAll
{
    $gameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.PluginId -eq "cb91dfc9-b977-43bf-8e70-55f46e410fab"}
    Add-SteamDates $gameDatabase
}