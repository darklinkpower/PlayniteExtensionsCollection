function GetMainMenuItems
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportSelectedDescription")
    $menuItem1.FunctionName = "Invoke-DateImporterSelected"
    $menuItem1.MenuSection = "@Date Importer"
    
    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportAllDescription")
    $menuItem2.FunctionName = "Invoke-DateImporterAll"
    $menuItem2.MenuSection = "@Date Importer"

    $menuItem3 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem3.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemSetDateManualDescription")
    $menuItem3.FunctionName = "Set-DatesFromInput"
    $menuItem3.MenuSection = "@Date Importer"
    
    $menuItem4 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem4.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportSelectedSteamDatesDescription")
    $menuItem4.FunctionName = "Invoke-SteamDateImporterSelected"
    $menuItem4.MenuSection = "@Date Importer|Steam"

    $menuItem5 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem5.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportAllSteamDatesDescription")
    $menuItem5.FunctionName = "Invoke-SteamDateImporterAll"
    $menuItem5.MenuSection = "@Date Importer|Steam"

    $menuItem6 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem6.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemExportSteamLicencesDescription")
    $menuItem6.FunctionName = "Export-SteamLicenses"
    $menuItem6.MenuSection = "@Date Importer|Steam"

    $menuItem7 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem7.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportSelectedGogDatesDescription")
    $menuItem7.FunctionName = "Invoke-GogDateImporterSelected"
    $menuItem7.MenuSection = "@Date Importer|GOG"

    $menuItem8 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem8.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportAllGogDatesDescription")
    $menuItem8.FunctionName = "Invoke-GogDateImporterAll"
    $menuItem8.MenuSection = "@Date Importer|GOG"

    $menuItem9 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem9.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemExportGogLicencesDescription")
    $menuItem9.FunctionName = "Export-GogLicenses"
    $menuItem9.MenuSection = "@Date Importer|GOG"

    $menuItem10 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem10.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportSelectedEpicDatesDescription")
    $menuItem10.FunctionName = "Invoke-EpicDateImporterSelected"
    $menuItem10.MenuSection = "@Date Importer|Epic"

    $menuItem11 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem11.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportAllEpicDatesDescription")
    $menuItem11.FunctionName = "Invoke-EpicDateImporterAll"
    $menuItem11.MenuSection = "@Date Importer|Epic"

    $menuItem12 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem12.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemExportEpicLicencesDescription")
    $menuItem12.FunctionName = "Export-EpicLicenses"
    $menuItem12.MenuSection = "@Date Importer|Epic"

    $menuItem13 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem13.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportSelectedOriginDatesDescription")
    $menuItem13.FunctionName = "Invoke-OriginDateImporterSelected"
    $menuItem13.MenuSection = "@Date Importer|Origin"

    $menuItem14 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem14.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportAllOriginDatesDescription")
    $menuItem14.FunctionName = "Invoke-OriginDateImporterAll"
    $menuItem14.MenuSection = "@Date Importer|Origin"

    $menuItem15 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem15.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemExportOriginLicencesDescription")
    $menuItem15.FunctionName = "Export-OriginLicenses"
    $menuItem15.MenuSection = "@Date Importer|Origin"

    return $menuItem1, $menuItem2, $menuItem3, $menuItem4, $menuItem5, $menuItem6, $menuItem7, $menuItem8, $menuItem9, $menuItem10, $menuItem11, $menuItem12, $menuItem13, $menuItem14, $menuItem15
}

function Invoke-DateImporterSelected
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    Invoke-SteamDateImporterSelected
    Invoke-GogDateImporterSelected
    Invoke-EpicDateImporterSelected
    Invoke-OriginDateImporterSelected
}

function Invoke-DateImporterAll
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    Invoke-SteamDateImporterAll
    Invoke-GogDateImporterAll
    Invoke-EpicDateImporterAll
    Invoke-OriginDateImporterAll
}

function Invoke-SteamDateImporterSelected
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.PluginId -eq "cb91dfc9-b977-43bf-8e70-55f46e410fab"}
    Add-SteamDates $gameDatabase
}

function Invoke-SteamDateImporterAll
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.PluginId -eq "cb91dfc9-b977-43bf-8e70-55f46e410fab"}
    Add-SteamDates $gameDatabase
}

function Invoke-GogDateImporterSelected
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.PluginId -eq "aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e"}
    Add-GogDates $gameDatabase
}

function Invoke-GogDateImporterAll
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.PluginId -eq "aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e"}
    Add-GogDates $gameDatabase
}

function Invoke-EpicDateImporterSelected
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.PluginId -eq "00000002-dbd1-46c6-b5d0-b1ba559d10e4"}
    Add-EpicDates $gameDatabase
}

function Invoke-EpicDateImporterAll
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.PluginId -eq "00000002-dbd1-46c6-b5d0-b1ba559d10e4"}
    Add-EpicDates $gameDatabase
}

function Invoke-OriginDateImporterSelected
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames | Where-Object {$_.PluginId -eq "85dd7072-2f20-4e76-a007-41035e390724"}
    Add-OriginDates $gameDatabase
}

function Invoke-OriginDateImporterAll
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.PluginId -eq "85dd7072-2f20-4e76-a007-41035e390724"}
    Add-OriginDates $gameDatabase
}

function Get-JsonFromPageSource
{
    param (
        $pageSource
    )

    $json = $pageSource -replace '<html><head></head><body><pre style="word-wrap: break-word; white-space: pre-wrap;">', '' -replace '</pre></body></html>', '' | ConvertFrom-Json
    return $json
}

function Get-IsJsonValidFromPage
{
    param (
        $pageSource
    )

    try {
        $pageSource -replace '<html><head></head><body><pre style="word-wrap: break-word; white-space: pre-wrap;">', '' -replace '</pre></body></html>', '' | ConvertFrom-Json | Out-Null
        return $true
    } catch {
        return $false
    }    
}

function Get-LoginStatusViaJson
{
    param (
        $navigateUrl
    )

    $webView = $PlayniteApi.WebViews.CreateOffscreenView()
    $webView.NavigateAndWait($navigateUrl)
    $pageSource = $webView.GetPageSource()
    $webView.Dispose()

    $jsonValid = Get-IsJsonValidFromPage $pageSource

    if ($jsonValid -eq $false)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LoginNotifyMessage"), "$libraryName Date Importer")
        $webView = $PlayniteApi.WebViews.CreateView(1020, 600)
        $webView.Navigate($navigateUrl)
        $webView.OpenDialog()
        $webView.Dispose()

        $webView = $PlayniteApi.WebViews.CreateOffscreenView()
        $webView.NavigateAndWait($navigateUrl)
        $pageSource = $webView.GetPageSource()
        $webView.Dispose()
        $jsonValid = Get-IsJsonValidFromPage $pageSource

        if ($jsonValid -eq $false)
        {
            return $false
        }
        else
        {
            return $true
        }
    }
    else
    {
        return $true  
    }
}

function Get-LoginStatus
{
    param (
        $navigateUrl,
        $domain,
        $cookieName
    )

    $webView = $PlayniteApi.WebViews.CreateOffscreenView()
    $webView.Navigate($navigateUrl)
    $sessionIdCookie = $webView.GetCookies() | Where-Object {$_.Domain -eq $domain} | Where-Object {$_.Name -eq $cookieName}
    $webView.Dispose()
    if ($null -eq $sessionIdCookie)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LoginNotifyMessage"), "$libraryName Date Importer")
        $webView = $PlayniteApi.WebViews.CreateView(1020, 600)
        $webView.Navigate($navigateUrl)
        $webView.OpenDialog()
        $webView.Dispose()

        $webView = $PlayniteApi.WebViews.CreateOffscreenView()
        $webView.Navigate($navigateUrl)
        $sessionIdCookie = $webView.GetCookies() | Where-Object {$_.Domain -eq $domain} | Where-Object {$_.Name -eq $cookieName}
        $webView.Dispose()

        if ($null -eq $sessionIdCookie)
        {
            return $false
        }
        return $true
    }
    return $true
}

function Export-Licenses
{
    param (
        $libraryName,
        $LicensesList
    )
    
    $LicenseExportChoice = $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesExportChoiceMessage") -f $LicensesList.count, $libraryName), "$libraryName Date Importer", 4)
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
            $__logger.Info("$libraryName Date Importer - Licenses exported to `"$LicenseExportPath`"")
            $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesExportSuccessMessage"), "$libraryName Date Importer")
        }
    }
}

function Export-Results
{
    param (
        $libraryName,
        $gamedatabase,
        $countMatchLicense,
        $countNoLicense,
        $CountNewDate,
        $gameDatesList
    )
    
    # Show finish dialogue with results and ask if user wants to export results
    $__logger.Info("$libraryName Date Importer - Finished. Processed Steam games: $($gameDatabase.count). Games with date found: $countMatchLicense`nGames without date found: $countNoLicense. Games that had the added date changed: $countNewDate")
    $ExportChoice = $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_ResultsExportChoiceMessage") -f $libraryName, $gameDatabase.count, $countMatchLicense, $countNoLicense, $countNewDate), "$libraryName Date Importer", 4)
    if ($ExportChoice -eq "Yes")
    {
        $ExportPath = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
        if ($ExportPath)
        {
            if ($ExportPath -match "\.csv$")
            {
                $gameDatesList | Select-Object Name, OldDate, NewDate, DateChanged, DateFound, LicenseDate | Sort-Object -Property DateFound, Name | ConvertTo-Csv -NoTypeInformation | Out-File $ExportPath -Encoding 'UTF8'
            }
            else
            {
                $gameDatesList | Select-Object Name, OldDate, NewDate, DateChanged, DateFound, LicenseDate | Sort-Object -Property DateFound, Name | Format-Table -AutoSize | Out-File $ExportPath -Encoding 'UTF8'
            }
            $__logger.Info("Steam Date Importer - Results exported to `"$ExportPath`"")
            $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_ResultsExportSuccessMessage"), "Steam Date Importer")
        }
    }
}

function Set-DatesFromInput
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $gameDatabase = $PlayniteApi.MainView.SelectedGames

    $dateInput = $PlayniteApi.Dialogs.SelectString([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_ResultsDateInputMessage"), "Date Importer", "")
    if ($dateInput.Result -eq $false)
    {
        exit
    }

    try {
        $date = $dateInput.SelectedString -replace '/', '-'
        $date = [datetime]::parseexact($date, 'dd-MM-yyyy HH:mm', $null)
    } catch {
        $PlayniteApi.Dialogs.ShowErrorMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_ResultsDateInputBadFormatMessage") -f $dateInput.SelectedString), "Date Importer")
        return
    }

    $dateModified = 0
    foreach ($game in $gameDatabase){
        if ($game.Added -ne $dateInput)
        {
            $gameDateOld = $game.Added
            $game.Added = $date
            $PlayniteApi.Database.Games.Update($Game)
            $gameDateNew = $game.Added
            $__logger.Info("Date Importer - Changed date of `"$($game.name)`", Old date: `"$gameDateOld`", New date: `"$gameDateNew`"")
            $dateModified++
        }
    }
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_AddedDateModifiedResultsMessage") -f $dateModified), "Date Importer")
}

function Set-DatesFromLicenses
{
    param (
        $libraryName,
        $LicensesList,
        $gameDatabase
    )

    if ($libraryName -eq "Steam")
    {
        $webView = $PlayniteApi.WebViews.CreateOffscreenView()
        $webView.Navigate("https://help.steampowered.com")
        $cookie = $webView.GetCookies() | Where-Object {$_.Domain -eq "help.steampowered.com"} | Where-Object {$_.Name -eq "steamLoginSecure"}
        $isLoggedOnSteamHelp = $true
        $webView.Dispose()
        if ($null -eq $cookie)
        {
            $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LoginNotifyMessage"), "$libraryName Date Importer")
            $webView = $PlayniteApi.WebViews.CreateView(1020, 600)
            $webView.Navigate("https://help.steampowered.com/")
            $webView.OpenDialog()
            $webView.Dispose()

            $webView = $PlayniteApi.WebViews.CreateOffscreenView()
            $webView.Navigate("https://help.steampowered.com")
            $cookie = $webView.GetCookies() | Where-Object {$_.Domain -eq "help.steampowered.com"} | Where-Object {$_.Name -eq "steamLoginSecure"}
            $webView.Dispose()
            if ($null -eq $cookie)
            {
                $isLoggedOnSteamHelp = $false
            }
        }
        $webView = $PlayniteApi.WebViews.CreateOffscreenView()
        $webView.Navigate("https://help.steampowered.com")
        $sessionIdCookie = $webView.GetCookies() | Where-Object {$_.Domain -eq "help.steampowered.com"} | Where-Object {$_.Name -eq "sessionid"}
        $sessionId = $sessionIdCookie.Value
        $helpTemplate = "https://help.steampowered.com/en/wizard/HelpWithGame/?appid={0}&sessionid={1}&wizard_ajax=1"
        $regexDate = 'class=\\"LineItemRow\\"&gt;\\r\\n\\t\\t\\t\\t\\t\\t\\t&lt;span&gt;(.*?(?=&amp;))'
    }

    # Counters and export list
    [System.Collections.Generic.List[object]]$GameDatesList = @()
    $CountNewDate = 0
    $CountMatchLicense = 0
    $CountNoLicense = 0

    foreach ($game in $gameDatabase) {
        $gameDateOld = $game.Added
        $gameDateNew = $null
        $dateChanged = "False"
        $dateFound = "False"
        $licenseDate = $null

        $gameNameMatch = $game.name -replace '[^\p{L}\p{Nd}]', ''
        foreach ($license in $LicensesList) {
            if ($license.LicenseNameMatch -eq $gameNameMatch)
            {
                $licenseDate = $license.LicenseDate
                break
            }
        }

        if (($null -eq $LicenseDate) -and ($libraryName -eq "Steam") -and ($isLoggedOnSteamHelp -eq $true))
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

        if ($licenseDate)
        {
            $countMatchLicense++
            $dateFound = "True"
            $licenseDate = [datetime]$licenseDate
            if ($game.Added -ne $licenseDate)
            {
                $game.Added = $licenseDate
                $PlayniteApi.Database.Games.Update($game)
                $gameDateNew = $game.Added
                $dateChanged = "True"
                $countNewDate++
                $__logger.Info("$libraryName Date Importer - Changed date of `"$($game.name)`", Old date: `"$gameDateOld`", New date: `"$gameDateNew`"")
            }
        }
        else
        {
            $countNoLicense++
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

    if ($libraryName -eq "Steam")
    {
        $webView.Dispose()
    }

    Export-Results $libraryName $gamedatabase $countMatchLicense $countNoLicense $CountNewDate $gameDatesList
}

function Add-EpicDates
{
    param (
        $gameDatabase
    )

    if ($gameDatabase.count -eq 0)
    {
        return
    }

    $libraryName = "Epic"
    $LicensesList = Get-EpicLicenses
    if ($LicensesList.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesNotFoundMessage"), "$libraryName Date Importer")
        return
    }
    Set-DatesFromLicenses $libraryName $LicensesList $gameDatabase
}

function Export-EpicLicenses
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $libraryName = "Epic"
    
    $LicensesList = Get-EpicLicenses
    if ($LicensesList.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesNotFoundMessage"), "$libraryName Date Importer")
        return
    }
    Export-Licenses $libraryName $LicensesList
}

function Get-EpicLicenses
{
    param (
        $libraryName
    )
    
    [System.Collections.Generic.List[object]]$LicensesList = @()
    $apiTemplate = "https://www.epicgames.com/account/v2/payment/ajaxGetOrderHistory?page={0}&lastCreatedAt={1}"
    $loginStatusNavigateUrl = $apiTemplate -f "0", [DateTime]::UtcNow.ToString('u') -replace " ", "T"
    $loginStatus = Get-LoginStatusViaJson $loginStatusNavigateUrl
    if ($loginStatus -eq $false)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_UsetNotLoggedMessage") -f $libraryName), "$libraryName Date Importer")
        return $LicensesList
    }

	$CreatedAt = [DateTime]::UtcNow.ToString('u') -replace " ", "T"
    $webView = $PlayniteApi.WebViews.CreateOffscreenView()
    for ($i = 0; $true; $i++) {
		$apiUrl = $apiTemplate -f $i, $CreatedAt
        $webView.NavigateAndWait($apiUrl)
        $json = Get-JsonFromPageSource $webView.GetPageSource()
        if ($json.orders.Count -gt 0)
        {
            foreach ($order in $json.orders) {
                $date = (Get-Date 01.01.1970).AddSeconds($($order.createdAtMillis -replace ".{3}$"))
                $CreatedAt = $date.ToString('u') -replace " ", "T"

                foreach ($item in $order.items) {
                    $product = [PSCustomObject]@{
                        LicenseName = $item.description
                        LicenseNameMatch = [System.Web.HttpUtility]::HtmlDecode($($item.description.ToLower())) -replace '[^\p{L}\p{Nd}]', ''
                        LicenseDate = $date
                    }
                    $LicensesList.Add($product)
                }
            }
        }
        else
        {
            break
        }
    }
    
    $webView.Dispose()
    return $LicensesList
}

function Add-GogDates
{
    param (
        $gameDatabase
    )

    if ($gameDatabase.count -eq 0)
    {
        return
    }

    $libraryName = "GOG"
    $LicensesList = Get-GOGLicenses
    if ($LicensesList.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesNotFoundMessage"), "$libraryName Date Importer")
        return
    }

    Set-DatesFromLicenses $libraryName $LicensesList $gameDatabase
}

function Export-GogLicenses
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $libraryName = "GOG"
    $LicensesList = Get-GogLicenses $libraryName
    if ($LicensesList.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesNotFoundMessage"), "$libraryName Date Importer")
        return
    }
    Export-Licenses $libraryName $LicensesList
}

function Get-GogLicenses
{
    param (
        $libraryName
    )
    
    [System.Collections.Generic.List[object]]$LicensesList = @()
    $apiTemplate = "https://www.gog.com/account/settings/orders/data?canceled=0&completed=1&in_progress=1&not_redeemed=1&page={0}&pending=1&redeemed=1"
    $loginStatusNavigateUrl = $apiTemplate -f "0"
    $loginStatus = Get-LoginStatusViaJson $loginStatusNavigateUrl
    if ($loginStatus -eq $false)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_UsetNotLoggedMessage") -f $libraryName), "$libraryName Date Importer")
        return $LicensesList
    }
    
    $webView = $PlayniteApi.WebViews.CreateOffscreenView()
    for ($i = 0; $true; $i++) {
        $apiUrl = $apiTemplate -f $i
        $webView.NavigateAndWait($apiUrl)
        $json = Get-JsonFromPageSource $webView.GetPageSource()
        if ($json.orders.Count -gt 0)
        {
            foreach ($order in $json.orders) {
                $date = (Get-Date 01.01.1970).AddSeconds($order.date)

                foreach ($product in $order.products) {
                    $product = [PSCustomObject]@{
                        LicenseName = $product.title
                        LicenseNameMatch = $product.title.ToLower() -replace '[^\p{L}\p{Nd}]', ''
                        LicenseDate = $date
                    }
                    $LicensesList.Add($product)
                }
            }
        }
        else
        {
           break
        }
    }
    
    $webView.Dispose()
    return $LicensesList
}

function Add-SteamDates
{
    param (
        $gameDatabase
    )
    
    if ($gameDatabase.count -eq 0)
    {
        return
    }

    $libraryName = "Steam"
    
    $LicensesList = Get-SteamLicenses
    if ($LicensesList.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesNotFoundMessage"), "$libraryName Date Importer")
        return
    }

    Set-DatesFromLicenses $libraryName $LicensesList $gameDatabase
}

function Export-SteamLicenses
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $libraryName = "Steam"
    $LicensesList = Get-SteamLicenses
    if ($LicensesList.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesNotFoundMessage"), "$libraryName Date Importer")
        return
    }
    Export-Licenses $libraryName $LicensesList
}

function Get-SteamLicenses
{
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
        "2016", "2017", "2018", "2019", "2020", "2021, 2022"
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
    $webView.Dispose()

    # Use regex to get all licenses
    $regex = '(?:<td class="license_date_col">)(.*?(?=<\/td>))(?:<\/td>\s+<td>)(?:\s+<div class="free_license_remove_link">(?:[\s\S]*?(?=<\/div>))<\/div>)?(?:\s+)([^\t]+)'
    $LicenseMatches = ([regex]$regex).Matches($LicensesHtmlContent)
    if ($LicenseMatches.count -eq 0)
    {
        # Use Webview to log in
        $__logger.Info("Steam Date Importer - No licenses found in first try, WebView will be opened")
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LoginNotifyMessage"), "Steam Date Importer")
        $LicensesUrl = 'https://store.steampowered.com/account/licenses/'
        $webView = $PlayniteApi.WebViews.CreateView(1020, 600)
        $webView.Navigate($LicensesUrl)
        $webView.OpenDialog()
        $webView.Dispose()

        # Use Webview to get licenses page content (Offscreen)
        $LicensesUrl = 'https://store.steampowered.com/account/licenses/?l=english'
        $webView = $PlayniteApi.WebViews.CreateOffscreenView()
        $webView.NavigateAndWait($LicensesUrl)
        $LicensesHtmlContent = $webView.GetPageSource()
        $webView.Dispose()

        $LicenseMatches = ([regex]$regex).Matches($LicensesHtmlContent)
        if ($LicenseMatches.count -eq 0)
        {
            $__logger.Info("Steam Date Importer - No licenses found in second try, not logged in or no licenses found")
            $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesNotFoundMessage"), "Steam Date Importer")
            return
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
    return $LicensesList
}

function Export-OriginLicenses
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    $libraryName = "Origin"
    
    $LicensesList = Get-OriginLicenses $libraryName
    if ($LicensesList.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesNotFoundMessage"), "$libraryName Date Importer")
        return
    }
    Export-Licenses $libraryName $LicensesList
}

function Add-OriginDates
{
    param (
        $gameDatabase
    )

    if ($gameDatabase.count -eq 0)
    {
        return
    }

    $libraryName = "Origin"
    $LicensesList = Get-OriginLicenses $libraryName
    if ($LicensesList.count -eq 0)
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_LicensesNotFoundMessage"), "$libraryName Date Importer")
        return
    }

    Set-DatesFromLicenses $libraryName $LicensesList $gameDatabase
}

function Get-OriginLicenses
{
    param (
        $libraryName
    )
    
    [System.Collections.Generic.List[object]]$LicensesList = @()

    $authUrl = "https://accounts.ea.com/connect/auth?client_id=ORIGIN_JS_SDK&response_type=token&redirect_uri=nucleus:rest&prompt=none"
    $loginStatus = Get-LoginStatusViaJson $authUrl
    if ($loginStatus -eq $false)
    {
        $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_UsetNotLoggedMessage") -f $libraryName), "$libraryName Date Importer")
        return $LicensesList
    }

    $webView = $PlayniteApi.WebViews.CreateOffscreenView()
    $webView.NavigateAndWait($authUrl)
    $pageSource = $webView.GetPageSource()
    $webView.Dispose()

    $jsonValid = Get-IsJsonValidFromPage $pageSource

    if ($jsonValid -eq $false) 
    {
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportOriginInvalidJson"), "$libraryName Date Importer")
        return $LicensesList
    }

    $authJson = Get-JsonFromPageSource $pageSource
    $token = $authJson.access_token


    $idUrl = "https://gateway.ea.com/proxy/identity/pids/me"
    $headers = @{
        Authorization = "Bearer {0}" -f $token
    }
    $idResponse = $null
    try {
        $idResponse = Invoke-RestMethod -Method "GET" -Uri $idUrl -Headers $headers -ContentType "application/json" -UseBasicParsing
    } catch {
        $errorMessage = $_.Exception.Message
        $__logger.Info("Error retrieving user id from Origin. Uri: $idUrl,  Error: $errorMessage")
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportOriginErrorMessage"), "$libraryName Date Importer")
        return $LicensesList
    }
    $id = $idResponse.pid.pidId


    $licenseTemplate = "https://api1.origin.com/ecommerce2/consolidatedentitlements/{0}?machine_hash=1"
    $licenseUrl = $licenseTemplate -f $id 
    $headers = @{
        authtoken = $token
    }
    $xml = $null
    try {
        $xml = Invoke-RestMethod -Method "GET" -Uri $licenseUrl -Headers $headers -ContentType "application/xml" -UseBasicParsing
    } catch {
        $errorMessage = $_.Exception.Message
        $__logger.Info("Error retrieving license data from Origin. Uri: $licenseUrl,  Error: $errorMessage")
        $PlayniteApi.Dialogs.ShowMessage([Playnite.SDK.ResourceProvider]::GetString("LOCDate_Importer_MenuItemImportOriginErrorMessage"), "$libraryName Date Importer")
        return $LicensesList
    }


    $licenses = Select-Xml $xml -XPath '/entitlements/entitlement'
    foreach ($license in $licenses) {
        $type = $license.Node.offer.offerType.ToLower() -replace " ", ""
        if ($($type -eq "basegame") -or $($type -eq "demo")) 
        {
            $date = Get-Date $license.Node.grantDate
            $product = [PSCustomObject]@{
                LicenseName = $license.Node.offer.localizableAttributes.displayName
                LicenseNameMatch = $license.Node.offer.localizableAttributes.displayName.ToLower() -replace '[^\p{L}\p{Nd}]', ''
                LicenseDate = $date
            }
            $LicensesList.Add($product)
        }
    }
    
    return $LicensesList
}