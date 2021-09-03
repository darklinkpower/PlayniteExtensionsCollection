function GetMainMenuItems
{
    param(
        $menuArgs
    )

    $menuItem1 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem1.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemAddToBlacklistDescription")
    $menuItem1.FunctionName = "Add-PlaystateBlacklist"
    $menuItem1.MenuSection = "@PlayState"

    $menuItem2 = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem2.Description = [Playnite.SDK.ResourceProvider]::GetString("LOCMenuItemRemoveFromBlacklistDescription")
    $menuItem2.FunctionName = "Remove-PlaystateBlacklist"
    $menuItem2.MenuSection = "@PlayState"
    
    return $menuItem1, $menuItem2
}

function Add-PlaystateBlacklist
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Set Log Path
    $playstateLogPath = Join-Path -Path $($PlayniteApi.Paths.ApplicationPath) -ChildPath "PlayState.log"
    
    # Log Startup
    "-------------------------- $(Get-Date -Format $DateFormat) | INFO: PlayState 2.0 Add game to blacklist runtime started --------------------------"  | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    
    # PlayState blacklist Feature
    $featureName = "PlayState blacklist"
    $feature = $PlayniteApi.Database.Features.Add($featureName)
    [guid[]]$featureIds = $feature.Id
    
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames
    
    # Set counters for added/already in list count
    $CountNotInList = 0
    
    # Start Execution for each game in the database
    foreach ($game in $GameDatabase) {
        if ($game.Features.name -contains "$featureName")
        {
            # Game in blacklist: increase count and log game
            "$(Get-Date -Format $DateFormat) | INFO: $($game.name) was already in PlayState blacklist"  | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        }
        else
        {
            # Game not in blacklist: add PlayState blacklist feature id, increase count and log game
            if ($game.FeatureIds) 
            {
                $game.FeatureIds += $featureIds
            } 
            else
            {
                # Fix in case game has null FeatureIds
                $game.FeatureIds = $featureIds
            }
            
            # Update game in database
            $PlayniteApi.Database.Games.Update($game)
            $CountNotInList++
            "$(Get-Date -Format $DateFormat) | INFO: $($game.name) was added to PlayState blacklist"  | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        }
    }
    
    # Show finish dialogue with number of games added and games that already were in blacklist
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCBlacklistAddedResultsMessage") -f $CountNotInList), "PlayState")
}

function Remove-PlaystateBlacklist
{
    param(
        $scriptMainMenuItemActionArgs
    )
    
    # Set Log Path
    $playstateLogPath = Join-Path -Path $($PlayniteApi.Paths.ApplicationPath) -ChildPath "PlayState.log"
    
    # Log Startup
    "------------------------ $(Get-Date -Format $DateFormat) | INFO: PlayState 1.7 Remove game from blacklist runtime started -----------------------"  | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append

    # PlayState blacklist Feature
    $featureName = "PlayState blacklist"
    $feature = $PlayniteApi.Database.Features.Add($featureName)
    [guid[]]$featureIds = $feature.Id
    
    # Set GameDatabase
    $GameDatabase = $PlayniteApi.MainView.SelectedGames
    
    # Set counters for removed/already in list count
    $CountInList = 0
    
    # Start Execution for each game in the database
    foreach ($game in $GameDatabase) {
        if ($game.Features.name -contains "$featureName")
        {
            # Game in blacklist: remove PlayState blacklist feature id, increase count and log game
            $game.FeatureIds.Remove("$featureIds")
            $PlayniteApi.Database.Games.Update($game)
            $CountInList++
            "$(Get-Date -Format $DateFormat) | INFO: $($game.name) was removed from PlayState blacklist"  | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        }
        else
        {
            # Game not in blacklist: increase count and log game
            "$(Get-Date -Format $DateFormat) | INFO: $($game.name) was already in PlayState blacklist"  | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        }
    }
    
    # Show finish dialogue with number of games added and games that already were in blacklist
    $PlayniteApi.Dialogs.ShowMessage(([Playnite.SDK.ResourceProvider]::GetString("LOCBlacklistRemovedResultsMessage") -f $CountInList), "PlayState")
}

function OnGameStarted
{
    param(
        $game
    )
    
    # Set Log Path and log date format
    $global:PlaystateLogPath = Join-Path -Path $($PlayniteApi.Paths.ApplicationPath) -ChildPath "PlayState.log"
    $global:DateFormat = 'yyyy/MM/dd HH:mm:ss:fff'
    
    # Set paths used by AutoHotKey
    $global:AhkScriptPath = Join-Path -Path $env:temp -ChildPath PlayState.ahk
    $global:AhkPidPath = Join-Path -Path $env:temp -ChildPath "PlayStatePID.txt"

    # Stop AutohotKey leftover process
    if (Test-Path $ahkPidPath)
    {
        $processId = Get-Content $ahkPidPath
        $processId = Get-Content $ahkPidPath
        Get-WmiObject -Class Win32_Process | Where-Object {$_.ProcessId -eq $processId} | ForEach-Object {
            try {
                $_.Terminate() | Out-Null
            } catch {
                continue
            }
        }
        
        "$(Get-Date -Format $DateFormat) | INFO: AutoHotKey process stopped" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        Remove-Item -Path $ahkPidPath -Force -ErrorAction 'SilentlyContinue'
        "$(Get-Date -Format $DateFormat) | INFO: Deleted $ahkPidPath" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    }

    # Check PlayState.log size and delete if bigger or equal than 50kb
    if (Test-Path $playstateLogPath)
    {
        if ($((Get-Item $playstateLogPath).length/1KB) -gt 50)
        {
            Remove-Item -Path $playstateLogPath -Force -ErrorAction 'SilentlyContinue'
            "$(Get-Date -Format $DateFormat) | INFO: Deleted PlayState.log since size was equal or bigger than 50kb " | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        }
    }
    
    # Script runtime start
    "------------------------------------ $(Get-Date -Format $DateFormat) | INFO: PlayState 2.0 runtime started ------------------------------------" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    "$(Get-Date -Format $DateFormat) | INFO: Started OnGameStarted function" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    "$(Get-Date -Format $DateFormat) | INFO: Game launched: $($game.name). Source: $($game.Source). Platform: $($game.Platform)." | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    
    # Check if game has PlayState blacklist Feature and stop execution if true
    $global:featureName = "PlayState blacklist"
    if ($game.Features.name -eq $featureName)
    {
        "$(Get-Date -Format $DateFormat) | INFO: $($game.name) is in PlayState blacklist. Extension execution stopped" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        exit
    }
    
    # Check if game has emulator profile
    $emulatorExecutablePath = [string]::Empty
    if ($null -ne $game.GameActions)
    {
        if ($game.GameActions[0].Type -eq [Playnite.SDK.Models.GameActionType]::Emulator)
        {
            if ($null -eq $game.GameActions[0].EmulatorId)
            {
                continue
            }
            $emulator = $PlayniteApi.Database.Emulators[$game.GameActions[0].EmulatorId]
            if ($null -ne $emulator.AllProfiles)
            {
                foreach ($profile in $emulator.AllProfiles) {
                    if ($profile.Id -eq $game.GameActions[0].EmulatorProfileId)
                    {
                        $emulatorExecutablePath = $profile.Executable
                    }
                    break
                }
            }
        }
    }

    # Set Job arguments and start Job
    $arguments = @(
        $playstateLogPath, 
        $ahkScriptPath, 
        $ahkPidPath, 
        $game.Name,
        $game.InstallDirectory,
        $emulatorExecutablePath
    )

    Start-Job -Name "PlayState" -ArgumentList $arguments -ScriptBlock {
    $DateFormat = 'yyyy/MM/dd HH:mm:ss:fff'
    
    # Set variables from arguments
    $playstateLogPath = $args[0]
    $ahkScriptPath = $args[1]
    $ahkPidPath = $args[2]
    $gameName = $args[3]
    $gameDirectory = $args[4]
    $emulatorExecutablePath = $args[5]

    # Detection method [1]: By emulator path
    if (![string]::IsNullOrEmpty($emulatorExecutablePath))
    {
        if (Test-Path $emulatorExecutablePath)
        {
            $gameExecutable = [System.IO.Path]::GetFileName($emulatorExecutablePath)
            "$(Get-Date -Format $DateFormat) | INFO: Detection method [1]: Emulator executable found `"$($emulatorExecutablePath)`"" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        }
    }

    if ($null -eq $gameExecutable)
    {
        # Set executables to ignore during process scan for PC games
        [System.Collections.Generic.List[String]]$ExclusionRules = @(
            "7z.exe",
            "7za.exe",
            "Archive.exe",
            "asset_*.exe",
            "anetdrop.exe",
            "Bat_To_Exe_Convertor.exe",
            "BsSndRpt.exe",
            "BootBoost.exe",
            "bootstrap.exe",
            "cabarc.exe",
            "CDKey.exe",
            "Cheat Engine.exe",
            "cheatengine*",
            "Civ2Map.exe",
            "*config*",
            "CLOSEPW.EXE",
            "*CrashDump*",
            "*CrashReport*",
            "crc32.exe",
            "CreationKit.exe",
            "CreatureUpload.exe",
            "EasyHook*.exe",
            "dgVoodooCpl.exe",
            "*dotNet*",
            "doc.exe",
            "*DXSETUP*",
            "dw.exe",
            "ENBInjector.exe",
            "HavokBehaviorPostProcess.exe",
            "*help*",
            "*instal*",
            "LangSelect.exe",
            "Language.exe",
            "*Launch*",
            "*loader*",
            "MapCreator.exe",
            "master_dat_fix_up.exe",
            "md5sum.exe",
            "MGEXEgui.exe",
            "modman.exe",
            "ModOrganizer.exe",
            "notepad++.exe",
            "notification_helper.exe",
            "oalinst.exe",
            "PalettestealerSuspender.exe",
            "pak*.exe",
            "*patch*",
            "planet_mapgen.exe",
            "Papyrus*.exe",
            "RADTools.exe",
            "readspr.exe",
            "register.exe",
            "SekiroFPSUnlocker*",
            "*settings*",
            "*setup*",
            "SCUEx64.exe",
            "synchronicity.exe",
            "syscheck.exe",
            "SystemSurvey.exe",
            "TES Construction Set.exe",
            "Texmod.exe",
            "*unins*",
            "*UnityCrashHandler*",
            "*x360ce*",
            "*Unpack*",
            "*UnX_Calibrate*",
            "*update*",
            "UnrealCEFSubProcess.exe",
            "url.exe",
            "*versioned_json.exe",
            "*vcredist*",
            "xtexconv.exe",
            "xwmaencode.exe",
            "Website.exe",
            "wide_on.exe"
        )
        
        # Set loops values
        $LoopRuntime = "60"
        $LoopRuntimeSecs = "10"
        $SleepTime = "40"

        # Generate executables lists: all executables in game directory, excluded and not excluded executables
        $ExecutablesAll = [System.IO.Directory]::GetFiles($gameDirectory, '*.exe', 'AllDirectories')
        $executablesExcluded = Get-ChildItem -path $ExecutablesAll -Include $ExclusionRules
        $ExecutablesNotExcluded = Get-ChildItem -path $ExecutablesAll -Exclude $ExclusionRules
        $LookupPath = Join-Path $gameDirectory -ChildPath "\*"

        # Log executables information
        "$(Get-Date -Format $DateFormat) | INFO: Game directory: $gameDirectory" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        "$(Get-Date -Format $DateFormat) | INFO: Executables excluded: $($executablesExcluded -join ", ")" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        "$(Get-Date -Format $DateFormat) | INFO: Executables not excluded: $($ExecutablesNotExcluded -join ", ")" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        
        # Detection method [2]: By not excluded executables count
        if ($ExecutablesNotExcluded.Count -eq 1)
        {
            $gameExecutable = $ExecutablesNotExcluded[0].name
            "$(Get-Date -Format $DateFormat) | INFO: Detection method [2]: Game executable found `"$($gameExecutable)`"" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        }
        else
        {
            # Sleeptime to prevent boostrap and launcher executables to be picked
            "$(Get-Date -Format $DateFormat) | INFO: $SleepTime seconds sleep time started" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
            Start-Sleep -s $SleepTime
            "$(Get-Date -Format $DateFormat) | INFO: $SleepTime seconds sleep time finished" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        }

        # Detection method [3]: By active processes with exclusions
        if ( (!$gameExecutable) -and ($ExecutablesNotExcluded.Count -gt 0) )
        {
            $LoopDateLimit = (Get-Date).AddSeconds( $LoopRuntime )
            "$(Get-Date -Format $DateFormat) | INFO: Started loop sequence with runtime of $($LoopRuntime) seconds and $($LoopRuntimeSecs) seconds between loops" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
            
            # Get active processes
            while ((!$gameExecutable) -and ((Get-Date) -lt $LoopDateLimit)) {
                $activeProcessesList = (Get-WmiObject -Class Win32_Process |  Where-Object {$_.path -like $LookupPath}).name | Select-Object -Unique
                "$(Get-Date -Format $DateFormat) | INFO: Detected processes that are active: $($activeProcessesList -join ", ")" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
                $gameExecutable = (Get-ChildItem -path $ExecutablesNotExcluded -include $activeProcessesList | Sort-Object -descending -property length)[0].name
                if (!$gameExecutable)
                {
                    Start-Sleep -s $LoopRuntimeSecs
                }
            }
            # Log found executable
            if ($gameExecutable)
            {
                "$(Get-Date -Format $DateFormat) | INFO: Detection method [3]: Game executable found `"$($gameExecutable)`"" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
            }
            else
            {
                "$(Get-Date -Format $DateFormat) | INFO: Detection method [3]: Game executable not found" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
            }
        }
        # Detection method [4]: By active processes with exclusions
        if ( (!$gameExecutable) -and ($executablesExcluded.Count -gt 0) )
        {
            $LoopDateLimit = (Get-Date).AddSeconds( $LoopRuntime )
            "$(Get-Date -Format $DateFormat) | INFO: Started loop sequence with maximum runtime time of $($LoopRuntime) seconds and $($LoopRuntimeSecs) seconds between loop to detect running process" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
            
            # Get active processes
            while ( (!$gameExecutable) -and ((Get-Date) -lt $LoopDateLimit)) {
                $activeProcessesList = (Get-WmiObject -Class Win32_Process |  Where-Object {$_.path -like $LookupPath}).name | Select-Object -Unique
                "$(Get-Date -Format $DateFormat) | INFO: Detected processes that are active: $($activeProcessesList -join ", ")" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
                $gameExecutable = (Get-ChildItem -path $executablesExcluded -include $activeProcessesList | Sort-Object -descending -property length)[0].name
                if (!$gameExecutable)
                {
                    Start-Sleep -s $LoopRuntimeSecs
                }
            }
            # Log found executable
            if ($gameExecutable)
            {
                "$(Get-Date -Format $DateFormat) | INFO: Detection method [4]: Game executable found `"$($gameExecutable)`"" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
            }
            else
            {
                "$(Get-Date -Format $DateFormat) | INFO: Detection method [4]: Game executable not found" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
            }
        }
    }
    
    # Check if executable was found and begin AutoHotKey executable search if true
    if ($gameExecutable)
    {
        # Generate AutoHotKey Script and save it
        $AhkScript = "Home::
            Pause::
            if (toggle := !toggle) {
            Process_Suspend(`"$gameExecutable`")
            Process_Suspend(PID_or_Name){
            PID := (InStr(PID_or_Name,`".`")) ? ProcExist(PID_or_Name) : PID_or_Name
            h:=DllCall(`"OpenProcess`", `"uInt`", 0x1F0FFF, `"Int`", 0, `"Int`", pid)
            If !h
                Return -1
            DllCall(`"ntdll.dll\NtSuspendProcess`", `"Int`", h)
            DllCall(`"CloseHandle`", `"Int`", h)
            SplashImage, , b FM18 fs12, Suspended, $($gameName)
            Sleep, 1000
            SplashImage, Off
            }
            } else {
            Process_Resume(`"$gameExecutable`")
            Process_Resume(PID_or_Name){
            PID := (InStr(PID_or_Name,`".`")) ? ProcExist(PID_or_Name) : PID_or_Name
            h:=DllCall(`"OpenProcess`", `"uInt`", 0x1F0FFF, `"Int`", 0, `"Int`", pid)
            If !h
                Return -1
            DllCall(`"ntdll.dll\NtResumeProcess`", `"Int`", h)
            DllCall(`"CloseHandle`", `"Int`", h)
            SplashImage, , b FM18 fs12, Resumed, $($gameName)
            Sleep, 1000
            SplashImage, Off
            }
            ProcExist(PID_or_Name=`"`"){
            Process, Exist, % (PID_or_Name=`"`") ? DllCall(`"GetCurrentProcessID`") : PID_or_Name
            Return Errorlevel
            }
            }
            return
        "
        $AhkScript | Out-File $env:temp\PlayState.ahk
        "$(Get-Date -Format $DateFormat) | INFO: AutoHotKey script generated" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        
        # Search for AutoHotKey registry key
        $Key = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, [Microsoft.Win32.RegistryView]::Registry64)
        $RegSubKey =  $Key.OpenSubKey("Software\AutoHotkey")
        $RegInstallDir = $RegSubKey.GetValue("InstallDir")
        if ($RegInstallDir)
        {
            $AhkExecutablePath = Join-Path -Path $RegInstallDir -ChildPath "AutoHotkeyU64.exe"
            If (Test-Path $AhkExecutablePath) 
            {
                "$(Get-Date -Format $DateFormat) | INFO: AutoHotkey executable detected in $($AhkExecutablePath)" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
                $global:app= Start-Process $AhkExecutablePath $ahkScriptPath -PassThru
                "$(Get-Date -Format $DateFormat) | INFO: AutoHotkey Script launched" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
                "$($app.Id)" | Out-File -Encoding 'UTF8' -FilePath $ahkPidPath
                "$(Get-Date -Format $DateFormat) | INFO: AutoHotkey PID $($app.Id) stored" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
            }
            else
            {
                "$(Get-Date -Format $DateFormat) | ERROR: AutoHotKey executable not found in registry path" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
            }
        }
        # Detect if AutoHotKey was launched
        if (-not ($app))
        {
            "$(Get-Date -Format $DateFormat) | ERROR: AutoHotKey executable not found" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        }
    }
    else
    {
        # Log executable not found with any method
        "$(Get-Date -Format $DateFormat) | ERROR: Executable not found with any method" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    }
    # Log function finish
    "$(Get-Date -Format $DateFormat) | INFO: Finished OnGameStarted function" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    }
}

function OnGameStopped
{
    param(
        $game
    )
    
    # Script runtime start Check if game has PlayState blacklist Feature and stop execution if true
    "$(Get-Date -Format $DateFormat) | INFO: Started OnGameStopped function" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    if ($game.Features.name -eq $featureName)
    {
        "$(Get-Date -Format $DateFormat) | INFO: $($game.name) is in PlayState blacklist. Extension execution stopped" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        exit
    }
    
    # Cleanup AutoHotKey and PID temporal fileS
    If (Test-Path $env:temp\PlayState.ahk)
    {
        Remove-Item $env:temp\PlayState.ahk -Force -ErrorAction 'SilentlyContinue'
        "$(Get-Date -Format $DateFormat) | INFO: AutoHotKey PlayState.ahk temporal file deleted" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    }
    if (Test-Path $ahkPidPath)
    {
        $processId = Get-Content $ahkPidPath
        Get-WmiObject -Class Win32_Process | Where-Object {$_.ProcessId -eq $processId} | ForEach-Object {
            try {
                $_.Terminate() | Out-Null
            } catch {
                continue
            }
        }
        
        "$(Get-Date -Format $DateFormat) | INFO: AutoHotKey process stopped" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        Remove-Item -Path $ahkPidPath -Force -ErrorAction 'SilentlyContinue'
        "$(Get-Date -Format $DateFormat) | INFO: Deleted $ahkPidPath" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    }
    
    # Log function finish
    "$(Get-Date -Format $DateFormat) | INFO: Finished OnGameStopped function" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
}