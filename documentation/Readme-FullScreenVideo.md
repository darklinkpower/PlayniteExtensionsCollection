# Fullscreen Video Capability - Extra Metadata Loader

This update adds the ability to view game videos in a borderless, maximized fullscreen window within the Extra Metadata Loader extension for Playnite.

## ✨ Features

- **Multiple Triggers**: 
  - Click the new **Fullscreen (⛶)** button in the embedded video player's control bar.
  - **Double-click** anywhere on the video surface to rapidly pop it out.
- **State Preservation**: A completely seamless handoff. The video continues from its exact current position, volume level, and muted/unmuted state when switching between embedded and fullscreen modes.
- **Animated Transport Controls**: 
  - A bottom-aligned control bar containing full premium features.
  - **Animated Opacity**: The entire control bar, as well as the exit button, rests at an unobtrusive 15% opacity so it doesn't distract from the video. Hovering immediately triggers a smooth fade-in to 90% opacity (200ms in, 400ms out).
  - **Play/Pause Toggle**: Features a dedicated toggle button, but can also be triggered by hitting the **Spacebar** or with a **Single-click** anywhere on the video surface. 
  - **Timeline Slider**: A scrubbable timeline slider with live timestamp updates relative to the total duration.
  - **Volume & Mute Controls**: Includes a slider with a perceptually accurate (linear to quadratic) curve, a dedicated mute toggle button, and is also mapped to the **M key**.
- **Exit Methods**:
  - Press the **Escape** key.
  - **Double-click** the fullscreen video.
  - Click the **✕** overlay button in the top-right corner.
- **Auto-Looping**: Respects the "Repeat trailer videos" setting from the plugin configuration.

## 🛠️ Technical Implementation & Bug Fixes

This feature has been surgically ported to the stable `emlCrashFix2025` branch, avoiding regressions in the `master` repository (such as the broken Steam Video downloader). It includes robust fixes to the WPF `MediaElement` implementation:
- **Spacebar Focus Fix**: Explicitly set `Focusable="False"` on all interactive control buttons and sliders in the fullscreen window. This prevents mouse clicks from stealing keyboard focus, ensuring the Spacebar consistently acts as a global Play/Pause toggle rather than re-triggering the last clicked button.
- **Black Screen on Pause resolved**: When entering fullscreen while a video is paused, WPF natively fails to render the initial frame, presenting a black screen. A robust `fsPlayer.Pause()` injection during initialization forces the pipeline to immediately render the initial start position frame.
- **Stream Reset fixed**: Prevented an aggressive WPF bug that resets a manual stream back to `00:00` the very first time `Play()` is called from a Paused state.
- **Mute Syncing**: Fixed logic where the fullscreen player would ignore the embedded player's mute state. 
- **Compilation & Integration**: Cleanly injected the new windows into the existing `ExtraMetadataLoader.csproj` structure, ensuring zero compilation errors on the `emlCrashFix2025` foundation while adhering to the original author's strict coding guidelines (PascalCase, camelCase with underscores, etc.).

## 🧪 Installation & Testing Instructions

### 1. Build and Import

To test these changes, you need to compile the project and manually replace the extension files in your Playnite installation.

1.  **Build the Project**:
    Open a terminal in the project root and run:
    ```powershell
    msbuild source\Generic\ExtraMetadataLoader\ExtraMetadataLoader.csproj /p:Configuration=Debug /t:Build
    ```
    This will produce `ExtraMetadataLoader.dll` in `source\Generic\ExtraMetadataLoader\bin\Debug\`.

2.  **Locate Playnite Extensions**:
    Open Playnite, go to `Main Menu > About Playnite > User data directory`.
    Navigate to the `Extensions` folder (**not** `ExtensionsData`).
    Look for a folder named `ExtraMetadataLoader` or `705fdbca-e1fc-4004-b839-1d040b8b4429` (the Extra Metadata Loader GUID).

3.  **Replace Files**:
    - **Close Playnite** completely.
    - Copy the newly built `ExtraMetadataLoader.dll` from your build output to the extensions folder, overwriting the existing one.
    - Ensure the `Localization` and `Controls` folders (if applicable) are also synced if you made XAML changes that aren't embedded.

### 2. Verification Checklist

Follow these steps to verify the feature:

1.  **Launch Playnite**: Open a game that has a video trailer.
2.  **Toggle Button**: Hover over the video to reveal the control bar. Click the ⛶ button. The video should pop into fullscreen.
3.  **Double-Click**: Exit fullscreen, then double-click the video surface. It should enter fullscreen.
4.  **Exit Triggers**: While in fullscreen, verify that **Escape**, **Double-clicking**, and the **top-right X button** all return you to the Playnite interface.
5.  **State Restore**: Pause a video at `0:10`, enter fullscreen. It should be paused at `0:10`. Play it to `0:15`, exit fullscreen. It should be playing at `0:15` in the embedded player.
6.  **Volume & Mute Sync**: Mute the video in Playnite. Enter fullscreen. The video should be muted. Change the volume and un-mute in the fullscreen controls, hit Escape, and verify those changes persisted back to the embedded player.
