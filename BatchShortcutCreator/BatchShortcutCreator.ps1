function global:GetMainMenuItems
{
    param($menuArgs)

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = "Create shortcut of selected games"
    $menuItem1.FunctionName = "BatchCreateShortcuts"
    $menuItem1.MenuSection = "@Batch Shortcut Creator"
    
    return $menuItem1
}

function global:BatchCreateShortcuts
{
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames
    
    # Check if there are selected games
    if ($GameDatabase.Count -eq 0)
    {
        # Show dialogue indicating there are not selected games
        $PlayniteApi.Dialogs.ShowMessage("No selected games");
        exit
    }
    
    # Show explorer to select shortcut save directory
    $Path = $PlayniteApi.Dialogs.SelectFolder()
    if ($Path)
    {
        $GameDatabase | ForEach-Object {$count = 0}{
        
        # Create Windows Explorer compatible game name
        $GameName = $($_.name).Split([IO.Path]::GetInvalidFileNameChars()) -join ''
        
        # Set shortcut path based on Windows Explorer compatible game name and url based on Id property
        $shortcut_path = $( "$($path)" + '\' + "$($GameName)" + '.url' )
        $url = $( 'URL=playnite://playnite/start/' + "$($_.Id)" )
        
        # Start .url file creation and increase count
        "[InternetShortcut]" | Out-File $shortcut_path
        $count++
        "IconIndex=0" | Out-File $shortcut_path -append

        # Check if game has an icon and if it's a *.ico compatible file. Else point to Playnite executable for icon
        if ($($_.Icon))
        {
            if ($($_.Icon) -like '*.ico')
            {
                $iconpath = $( 'IconFile=' + "$($PlayniteApi.Database.DatabasePath)" + '\files\' + "$($_.Icon)") | Out-File $shortcut_path -append
            }
        }
        if ($iconpath -eq $null)
        {
            $iconpath = $( 'IconFile=' + "$($PlayniteApi.Paths.ApplicationPath)" + '\Playnite.DesktopApp.exe' ) | Out-File $shortcut_path -append
        }
        
        # Save url to shortcut
        "$($url)" | Out-File $shortcut_path -append
        }
    }
    else
    {
        # Show dialogue if folder selection dialogue is canceled
        $PlayniteApi.Dialogs.ShowMessage("Folder selection dialogue was canceled. No shortcuts were created.");
        exit
    }

    # Show finish dialogue with shortcut creation count if at least one shortcut was created
    $PlayniteApi.Dialogs.ShowMessage("Shortcut created for $($count) selected games.");
}