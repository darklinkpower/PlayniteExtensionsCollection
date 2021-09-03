function PCGamingWikiMissingInfo
{
    # Set paths
    $LocalMissingList = Join-Path -Path $env:temp -ChildPath "LocalML.txt"
    $ETagLocalMissingList = Join-Path -Path $env:temp -ChildPath "ETagLocalML.txt"
    
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames
    
    # Create "PCGW - Info missing" tag
    $tagNoInfoName = "PCGW - Info missing"
    $tagNoInfo = $PlayniteApi.Database.tags.Add($tagNoInfoName)
    [guid[]]$tagNoInfoIds = $tagNoInfo.Id
    
    # Set counters
    $CountTagAdded = 0
    $CountTagRemoved = 0
    
    # Download missing list information from Ludusavi Manifest
    $Uri = 'https://raw.githubusercontent.com/mtkennerly/ludusavi-manifest/master/data/missing.md'
    if (Test-Path $ETagLocalMissingList)
    {
        $ETag = Get-Content $ETagLocalMissingList
    }
    try {
        Invoke-WebRequest $Uri -Headers @{'If-None-Match' = "$ETag"} -OutFile $LocalMissingList
        (Invoke-WebRequest $Uri -Method Head).Headers.'ETag' | out-file $ETagLocalMissingList
        $__logger.Info("PC Gaming Wiki Missing Info - Missing list and Etag downloaded")
    } catch {
        $__logger.Info("PC Gaming Wiki Missing Info - Missing list not downloaded")
    }
    
    # Set missing list
    if (Test-Path $LocalMissingList)
    {
        $MissingList = [System.IO.File]::ReadAllLines($LocalMissingList)
    }
    else
    {
        Remove-Item -Path $ETagLocalMissingList -Force -ErrorAction 'SilentlyContinue'
        $PlayniteApi.Dialogs.ShowMessage("Missing List couldn't be downloaded and no local copy was found");
        exit
    }
    
    foreach ($game in $GameDatabase) {
        if ($null -eq $game.Platforms)
        {
            continue
        }
        else
        {
            $isTargetSpecification = $false
            foreach ($platform in $game.Platforms) {
                if ($null -eq $platform.SpecificationId)
                {
                    continue
                }
                if ($platform.SpecificationId -eq "pc_windows")
                {
                    $isTargetSpecification = $true
                    break
                }
            }
            if ($isTargetSpecification -eq $false)
            {
                continue
            }
        }
        
           # Check if game has "PCGW - Info missing" tag and remove it if it's no longer in missing list
        if ($game.tags.name -eq "$tagNoInfoName")
        {
            if  (($MissingList -match "\[$($game.name)\]").count -eq 0)
            {
                $game.tagIds.Remove("$tagNoInfoIds")
                $PlayniteApi.Database.Games.Update($game)
                $CountTagRemoved++
            }
        }
        
        # Check if game is in missing list
        elseif ($MissingList -match "\[$($game.name)\]")
        {
            # Add "PCGW - Info missing" tag Id to game
            if ($game.tagIds)
            {
                $game.tagIds += $tagNoInfoIds
            }
            else
            {
                # Fix in case game has null tagIds
                $game.tagIds = $tagNoInfoIds
            }
            
            # Update game in database and increase counters
            $PlayniteApi.Database.Games.Update($game)
            $CountTagAdded++
        }
    }
    # Show finish dialogue with results
    $PlayniteApi.Dialogs.ShowMessage("`"$tagNoInfoName`" tag added to $CountTagAdded games.`n`"$tagNoInfoName`" tag removed from $CountTagRemoved games.");
}