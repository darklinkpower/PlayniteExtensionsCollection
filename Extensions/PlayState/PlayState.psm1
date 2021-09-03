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
        if ($game.Features.name -contains $featureName)
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
        if ($game.Features.name -contains $featureName)
        {
            # Game in blacklist: remove PlayState blacklist feature id, increase count and log game
            $game.FeatureIds.Remove($featureIds)
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
        $OnGameStartedEventArgs
    )
    
    $game = $OnGameStartedEventArgs.Game

    $global:playstateLogPath = Join-Path -Path $PlayniteApi.Paths.ApplicationPath -ChildPath "PlayState.log"
    $global:DateFormat = 'yyyy/MM/dd HH:mm:ss:fff'
    
    $global:AhkScriptPath = Join-Path -Path $env:temp -ChildPath PlayState.ahk
    $global:AhkPidPath = Join-Path -Path $env:temp -ChildPath "PlayStatePID.txt"

    # See if AHK was not closed in the previous runtime
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
    
    "------------------------------------ $(Get-Date -Format $DateFormat) | INFO: PlayState 2.0 runtime started ------------------------------------" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    "$(Get-Date -Format $DateFormat) | INFO: Started OnGameStarted function" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    "$(Get-Date -Format $DateFormat) | INFO: Game launched: $($game.name). Plugin: $([Playnite.SDK.BuiltinExtensions]::GetExtensionFromId($game.PluginId))" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    
    $global:featureName = "PlayState blacklist"
    if ($game.Features.name -eq $featureName)
    {
        "$(Get-Date -Format $DateFormat) | INFO: $($game.name) is in PlayState blacklist. Extension execution stopped" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        exit
    }
    
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

    # Set Job arguments and start Job. The job is used to prevent locking the Playnite UI and allow game execution
    $arguments = @(
        $playstateLogPath, 
        $ahkScriptPath, 
        $ahkPidPath, 
        $game.Name,
        $game.InstallDirectory.ToLower(),
        $emulatorExecutablePath
    )

    Start-Job -Name "PlayState" -ArgumentList $arguments -ScriptBlock {

        $global:playstateLogPath = $args[0]
        $ahkScriptPath = $args[1]
        $ahkPidPath = $args[2]
        $gameName = $args[3]
        $gameDirectory = $args[4]
        $emulatorExecutablePath = $args[5]

        function Write-ToLog
        {
            param(
                [string] $message
            )

            "$(Get-Date -Format 'yyyy/MM/dd HH:mm:ss:fff') | INFO: $message" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        }

        function Get-ExclusionList
        {
            [System.Collections.Generic.List[String]]$exesExclusionList = @(
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
                "instal",
                "LangSelect.exe",
                "Language.exe",
                "*Launch*",
                "*loader",
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
                "patch",
                "planet_mapgen.exe",
                "Papyrus*.exe",
                "RADTools.exe",
                "readspr.exe",
                "register.exe",
                "SekiroFPSUnlocker*",
                "settings",
                "setup",
                "SCUEx64.exe",
                "synchronicity.exe",
                "syscheck.exe",
                "SystemSurvey.exe",
                "TES Construction Set.exe",
                "Texmod.exe",
                "uninstall",
                "UnityCrashHandler*",
                "x360ce",
                "*Unpack",
                "*UnX_Calibrate*",
                "update",
                "UnrealCEFSubProcess.exe",
                "url.exe",
                "versioned_json.exe",
                "vcredist*",
                "xtexconv.exe",
                "xwmaencode.exe",
                "Website.exe",
                "wide_on.exe"
            )

            return $exesExclusionList
        }
        function Get-GameProcessId
        {
            param(
                [string] $gameDirectory,
                [bool] $useExclusionList
            )


            $exesExclusionList = Get-ExclusionList
            $LoopDateLimit = (Get-Date).AddSeconds(90)
            $lookupPath = Join-Path $gameDirectory.TrimEnd('\') -ChildPath "\*"
            Write-ToLog "Starting process detection. Lookup path: $LookupPath. Use exclusion: $($useExclusionList.ToString())"
            while ((Get-Date) -lt $LoopDateLimit) {
                $activeProcesses = Get-CimInstance -Class Win32_Process | Where-Object {$_.Path -like $LookupPath}
                if ($activeProcesses.count -eq 0)
                {
                    Write-ToLog "Not detected active processes"
                    Start-Sleep -s 10
                    continue
                }

                $activeProcessesPaths = ($activeProcesses).Path -join ", "
                Write-ToLog "Detected $($activeProcesses.Count) processes that are active: $activeProcessesPaths"

                $activeProcessesList = [System.Collections.Generic.List[System.Object]]::new()
                if ($useExclusionList -eq $true)
                {
                    $activeProcesses | ForEach-Object {
                        $exclude = $false
                        foreach ($executable in $exesExclusionList) {
                            if ($_.Name -like $executable)
                            {
                                Write-ToLog "Process `"$($_.Name)`" excluded by `"$executable`""
                                continue
                            }
                        }
                        if ($exclude -eq $false)
                        {
                            $activeProcessesList.Add($_)
                        }
                    }
                    if ($activeProcessesList.count -eq 0)
                    {
                        Write-ToLog "No processes left after exclusion"
                        Start-Sleep -s 10
                        continue
                    }
                }
                else
                {
                    $activeProcesses | ForEach-Object {
                        $activeProcessesList.Add($_)
                    }
                }

                if ($activeProcessesList.count -gt 1)
                {
                    Write-ToLog "Multiple active processes"
                    <# Processes are sorted by CPU in case there are multiple process matches or 
                    game spawns multiple subprocess. From testing, the correct PID to suspend in 
                    these cases is the one with highest CPU usage and not the id of the Parent 
                    Process. An example of this is Cross Code.
                    To get CPU processing it is needed to use the Get-Process cmdlet 
                    There are a lot of properties that are not accesible due to 32 bits
                    so it will be tried to use <ProcessName>.exe #>

                    $getProcessActiveProcesses = Get-Process | Sort-Object -Property CPU -Descending
                    foreach ($process in $getProcessActiveProcesses) {
                        $processName = ($process.Name -replace "\.exe", "") + ".exe"
                        foreach ($activeProcess in $activeProcessesList) {
                            # Get-CimInstance processes Name is actually the module name
                            if ($processName -eq $activeProcess.Name)
                            {
                                Write-ToLog "Found and returning process Get-Process $($process.Id.ToString())"
                                Write-ToLog "Found process Get-CimInstance $($activeProcess.ProcessId.ToString())"
                                return $process.Id
                            }
                        }
                    }
                }

                if ($activeProcessesList.count -ge 1)
                {
                    Write-ToLog "Found process $($activeProcess.ProcessId.ToString())"
                    return $activeProcessesList[0].ProcessId
                }
                Start-Sleep -s 10
            }
        }

        # Detection method [1]: By emulator path
        $gameExecutable = $null
        $gameProcessId = 0
        if (![string]::IsNullOrEmpty($emulatorExecutablePath))
        {
            if (Test-Path $emulatorExecutablePath)
            {
                $gameExecutable = [System.IO.Path]::GetFileName($emulatorExecutablePath)
                Write-ToLog "Detection method [1]: Emulator executable found `"$($emulatorExecutablePath)`""
            }
        }

        if ($null -eq $gameExecutable)
        {
            $exesExclusionList = Get-ExclusionList
            $executablesAll = [System.IO.Directory]::GetFiles($gameDirectory, '*.exe', 'AllDirectories')
            $executablesExcluded = Get-ChildItem -path $executablesAll -Include $exesExclusionList
            $executablesNotExcluded = Get-ChildItem -path $executablesAll -Exclude $exesExclusionList
            
            Write-ToLog "Game directory: $gameDirectory"
            Write-ToLog "Executables excluded: $($executablesExcluded -join ", ")"
            Write-ToLog "Executables not excluded: $($executablesNotExcluded -join ", ")"
            
            if ($executablesNotExcluded.Count -gt 0)
            {
                Start-Sleep -Seconds 10
                $gameProcessId = Get-GameProcessId $gameDirectory $true
            }

            if (($null -eq $gameExecutable) -and ($executablesExcluded.Count -gt 110))
            {
                Start-Sleep -Seconds 10
                $gameProcessId = Get-GameProcessId $gameDirectory $false
            }
        }
        
        if (($null -ne $gameExecutable) -or ($gameProcessId -ne 0))
        {
            if ($gameProcessId -ne 0)
            {
                $process = $gameProcessId.ToString()
            }
            else
            {
                $process = $gameExecutable
            }

            $AhkScript = "Home::
                Pause::
                if (toggle := !toggle)
                {
                    Process_Suspend(`"$process`")
                    Process_Suspend(PID_or_Name)
                    {
                        PID := (InStr(PID_or_Name,`".`")) ? ProcExist(PID_or_Name) : PID_or_Name
                        h:=DllCall(`"OpenProcess`", `"uInt`", 0x1F0FFF, `"Int`", 0, `"Int`", pid)
                        If !h
                            Return -1
                        DllCall(`"ntdll.dll\NtSuspendProcess`", `"Int`", h)
                        DllCall(`"CloseHandle`", `"Int`", h)
                        SplashImage, , b FM18 fs12, Suspended, $gameName
                        Sleep, 1000
                        SplashImage, Off
                    }
                }
                else
                {
                    Process_Resume(`"$process`")
                    Process_Resume(PID_or_Name)
                    {
                        PID := (InStr(PID_or_Name,`".`")) ? ProcExist(PID_or_Name) : PID_or_Name
                        h:=DllCall(`"OpenProcess`", `"uInt`", 0x1F0FFF, `"Int`", 0, `"Int`", pid)
                        If !h
                            Return -1
                        DllCall(`"ntdll.dll\NtResumeProcess`", `"Int`", h)
                        DllCall(`"CloseHandle`", `"Int`", h)
                        SplashImage, , b FM18 fs12, Resumed, $gameName
                        Sleep, 1000
                        SplashImage, Off

                    }
                }

                ProcExist(PID_or_Name=`"`")
                {
                    Process, Exist, % (PID_or_Name=`"`") ? DllCall(`"GetCurrentProcessID`") : PID_or_Name
                    Return Errorlevel
                }

                return
            "

            $AhkScript | Out-File $env:temp\PlayState.ahk
            Write-ToLog "AutoHotKey script generated"
            
            $key = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, [Microsoft.Win32.RegistryView]::Registry64)
            $regSubKey =  $key.OpenSubKey("Software\AutoHotkey")
            if ($regSubKey)
            {
                $RegInstallDir = $regSubKey.GetValue("InstallDir")
                if ($RegInstallDir)
                {
                    $AhkExecutablePath = Join-Path -Path $RegInstallDir -ChildPath "AutoHotkeyU64.exe"
                    If (Test-Path $AhkExecutablePath) 
                    {
                        Write-ToLog "AutoHotkey executable detected in $($AhkExecutablePath)"
                        $global:app= Start-Process $AhkExecutablePath $ahkScriptPath -PassThru
                        Write-ToLog "AutoHotkey Script launched"
                        "$($app.Id)" | Out-File -Encoding 'UTF8' -FilePath $ahkPidPath
                        Write-ToLog "AutoHotkey PID $($app.Id) stored"
                    }
                    else
                    {
                        Write-ToLog "AutoHotKey executable not found in registry path"
                    }
                }
            }
            
            if (-not ($app))
            {
                Write-ToLog "AutoHotKey executable not found"
            }
        }
        else
        {
            Write-ToLog "Executable not found with any method"
        }
        
        Write-ToLog "Finished OnGameStarted function"
    }
}

function OnGameStopped
{
    param(
        $OnGameStoppedEventArgs
    )
    
    $game = $OnGameStoppedEventArgs.Game
    
    "$(Get-Date -Format $DateFormat) | INFO: Started OnGameStopped function" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
    if ($game.Features.Name -eq $featureName)
    {
        "$(Get-Date -Format $DateFormat) | INFO: $($game.name) is in PlayState blacklist. Extension execution stopped" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
        exit
    }
    
    
    if (Test-Path $env:temp\PlayState.ahk)
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
    
    
    "$(Get-Date -Format $DateFormat) | INFO: Finished OnGameStopped function" | Out-File -Encoding 'UTF8' -FilePath $playstateLogPath -Append
}