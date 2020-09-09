# Playnite Script Extensions

Collection of extension scripts made for [Playnite](https://github.com/JosefNemec/Playnite).

## Download and installation

Option 1: [Click here](https://github.com/darklinkpower/PlayniteScriptExtensions/archive/master.zip) to download the whole repository and extract the wanted extensions in your Playnite Extensions folder depending if you are using a Portable or Standard installation.
See for [Portable version](https://github.com/JosefNemec/Playnite/wiki/Installing-scripts-and-plugins#portable-version).
See for [Standard version](https://github.com/JosefNemec/Playnite/wiki/Installing-scripts-and-plugins#standard-version).

Option 2: Download the packaged *.pext files from the forum thread of the wanted extension linked in the [Extensions section](#extensions) and see [Packaged extensions](https://github.com/JosefNemec/Playnite/wiki/Installing-scripts-and-plugins#packaged-extensions).

## Usage

Varies depending the extension functionality but in general. Refer to each extension thread in Playnite forums for the specific instructions.

## Extensions

### Batch Shortcut Creator

Playnite Forum Thread: https://playnite.link/forum/thread-251.html

<details>

<summary>Description</summary>

This extension will create shortcuts in batch for your selected games in your selection of choice. It mainly serves as a workaround until [Playnite's Issue #856](https://github.com/JosefNemec/Playnite/issues/856) is done, although it can serve other purposes.

</details>

### Fit Description Images 

Playnite Forum Thread: https://playnite.link/forum/thread-373.html

<details>

<summary>Description</summary>

This extension will apply the following style to your games's description to make images fit when viewing them in details:

`<style>img{max-width:100%;}</style>`

</details>

### GameCompact

Playnite Forum Thread: https://playnite.link/forum/thread-241.html

<details>

<summary>Description</summary>

This extension will compact the currently selected game(s) using the [compact](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/compact) windows command.

By default the extension will ignore files with the following extension since they compact barely anything if at all and will only make the compact function much slower:

`*.7z, *.aac, *.avi, *.ba, *.br, *.bz2, *.bik, *.pc_binkvid, *.bk2, *.bnk, *.cab, *.dl_, *.docx, *.flac, *.flv, *.gif, *.gz, *.jpeg, *.jpg, *.log, *.lz4, *.lzma, *.lzx, *.m2v, *.m4v, *.mkv, *.mp2, *.mp3, *.mp4, *.mpeg, *.mpg, *.ogg, *.onepkg, *.png, *.pptx, *.rar, *.upk, *.vob, *.vssx, *.vstx, *.wem, *.webm, *.wma, *.wmf, *.wmv, *.xap, *.xnb, *.xlsx, *.xz, *.zst, *.zstd`

</details>

### Game Media Tools

Playnite Forum Thread: https://playnite.link/forum/thread-313.html

<details>

<summary>Description</summary>

This extension is intended as a library mantaining tool and to make it easier to handle game media in your library.
It works with the currently available game media options: Covers images, background images and icons.

It currently has the following functions:

1. Missing Media: Detect games that are missing any of the media available.
2. Image aspect ratio: Enter an arbitrary aspect ratio and detect if the selected media is different in the processed games.
3. Image resolution: Enter an arbitrary resolution and detect if the selected media is different in the processed games.
4. Image extension: Enter an arbitrary file extension and detect if the selected games match.
5. Image size: Enter an arbitrary size in kb and detect if the selected games are bigger than that.
6. Open metadata folder: Open Metadata folder of selected games.

After processing, you'll see a dialogue window with the results and games will have a tag added if necessary for easy filtering with Playnite to afterwards manage the games.

</details>

### Game Region Filler

Playnite Forum Thread: https://playnite.link/forum/thread-369.html
<details>
<summary>Description</summary>

This extension will fill the region field based on the file name of your game.

</details>

### Image Cache Size Saver

Playnite Forum Thread: https://playnite.link/forum/thread-372.html

<details>

<summary>Description</summary>

This extension will save a considerable amount of space by processing the images in you Playnite's cache. This cache can use many GBs of space depending on your installation size, being the main culprit animated images that get automatically downloaded for your game's description. This extension will process all your images and only save their first frame to considerably reduce image sizes, and also process all other images in the cache. There won't be any difference to the user after using the extension's function.

Only images that have less size after being processed will overwrite your current images to provide the best savings in your cache and the list of processed images will be saved to not try to process them again when the extension is run afterwards.

The extension uses ImageMagick to do the image processing and it's required to download it in any location. ImageMagick can be downloaded here (See the "Windows Binary Release" section): https://imagemagick.org/script/download.php

</details>

### Installation Status Updater

Playnite Forum Thread: https://playnite.link/forum/thread-316.html

<details>

<summary>Description</summary>

This extension has two main functions:

1. Installation Status Updater: Check all the games in your library that have an executable, rom or ISO path and will do the following:
   * Installed games: check if the game file is still there and if not, mark the game as uninstalled.
   * Uninstalled games: check if the game file is now there and if true, mark the game as installed.
   
   The game scan is done when Playnite starts and also manually by selecting the function in the extensions menu. It has the function to export changes when ran manually.

2. Installation Path Updater: Modify the pointed install path of selected games for cases when the game image/file has been moved from the pointed location in Playnite. After using this function, it will be checked if the game executable, rom or ISO exists in the new location and update the games installation status. It has the function to export changes.

</details>

### NVIDIA GeforceExperience GameStream Export

Playnite Forum Thread: https://playnite.link/forum/thread-252.html
<details>
<summary>Description</summary>

This extension will export your selected games to NVIDIA Geforce Experience GameStreaming games database, allowing you to run them via a NVIDIA Shield or in any [Moonlight Game Streaming](https://moonlight-stream.org/) supported device, while retaining all the benefits of Playnite and allowing it to still manage your games.

</details>

### NVIDIA Geforce NOW Compatibility Checker

Playnite Forum Thread: https://playnite.link/forum/thread-298.html

<details>

<summary>Description</summary>

This extension will check which of your games has been enabled for the NVIDIA Geforce NOW Service and add "NVIDIA Geforce NOW" in their features to easily check them. Compatible with games from Epic, Origin, Steam and Uplay.

</details>

### PlayState

Playnite Forum Thread: https://playnite.link/forum/thread-225.html

<details>

<summary>Description</summary>

This extension will let you suspend and resume your game at any moment. This gives a the benefit of pausing your game at any time and also to free your CPU and GPU usage when you are not playing, effectively acting as if you closed the game when you activate the script. See the screenshots for comparison.

It's required to have [AutoHotkey](https://www.autohotkey.com/) installed to make use of this extension.

</details>

### Search Collection

Playnite Forum Thread: https://playnite.link/forum/thread-163.html

<details>

<summary>Description</summary>

This extension will search the currently selected game(s) on different websites in your web browser.

</details>

### Steam Date Importer

Playnite Forum Thread: https://playnite.link/forum/thread-376.html

<details>

<summary>Description</summary>

This extension will obtain the date of when the steam games in your account where bought by obtaining them from the license date. This is to have better reference inside Playnite of when all your games were bought, instead of just when they were imported into your Playnite library.

The dates are obtained by parsing the data found in the licenses page when logged into Steam.

The extension has the following functionalities:
1. Export obtained Steam license data (License name and date).
2. Replace "Added date" information in your games from the one found in Steam.
3. Export extension results, including the game names with their new added dates.

</details>

### SteamDB Rating

Playnite Forum Thread: https://playnite.link/forum/thread-320.html
<details>
<summary>Description</summary>

This extension will get the SteamDB rating-like score and put it in the game community Score. The problem with the Steam review scores is that they can be innacurate on certain games with a low number of reviews and this extension is an attempt to fix this and show more accurate information. For more information about SteamDB's algorithm see [Introducing Steam Database's new rating algorithm](https://steamdb.info/blog/steamdb-rating/)

The extension is compatible with Steam games and games that have a Steam Store link.

</details>

### Steam Game Importer

Playnite Forum Thread: https://playnite.link/forum/thread-300.html

<details>

<summary>Description</summary>

This extension will add a steam game to your library. You need to input either a valid steam id or url. It can also import all your games not currently imported in Playnite. This extension is mainly intended as a workaround for [Playnite's Issue #910.](https://github.com/JosefNemec/Playnite/issues/910)

</details>

### Steam Trailers

Playnite Forum Thread: https://playnite.link/forum/thread-242.html

<details>

<summary>Description</summary>

This extension will search for trailers for your games in your web browser or in a Playnite Window. It also works for non-Steam games.

There are 2 versions of the extension:

* Playnite Version: Opens the video in a Playnite window.
* Web Browser Version: Opens the video in your Web Browser (Only available in forums).

</details>

### Twitch Add Link

Playnite Forum Thread: https://playnite.link/forum/thread-364.html

<details>

<summary>Description</summary>

This extension will add a Twitch Link to your selected games.

There are two functions:

1. Twitch - Add Twitch link to selected games (Automatic): will search for Twitch Links of games and only add the ones found automatically without user input.
2. Twitch - Add Twitch link to selected games (Manual): will search for Twitch Links of games, add the ones found automatically and request the user to enter the correct Urls when not automatically found.

</details>

## Contributing

If you see any way to improve any of the extensions, feel free to send a PR.


## Questions, suggestions and Issues

Please open a [new Issue](https://github.com/darklinkpower/PlayniteScriptExtensions/issues)
