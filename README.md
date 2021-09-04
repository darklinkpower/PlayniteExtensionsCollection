# Playnite Extensions Collection

Collection of extension scripts made for [Playnite](https://github.com/JosefNemec/Playnite).

**Note: The master branch contains changes for the upcoming Playnite 9 version. Playnite 8 compatible extensions are in [this branch.](https://github.com/darklinkpower/PlayniteExtensionsCollection/tree/pre-playnite9)**

## Download and installation

Download the packaged *.pext files from the forum thread of the wanted extension linked in the [Extensions section](#extensions) and see [Packaged extensions](https://github.com/JosefNemec/Playnite/wiki/Installing-scripts-and-plugins#packaged-extensions).

## Usage

Varies depending the extension functionality but in general. Refer to each extension thread in Playnite forums for the specific instructions.

## Extensions

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

### Importer for AniList

Playnite Forum Thread:https://playnite.link/forum/thread-395.html

<details>

<summary>Description</summary>

Made for personal use but maybe someone else finds it useful. It imports your lists from [MAL-Sync](https://malsync.moe/) for viewing in Playnite.

Features:
- Downloads Anime and Manga lists. "Platform" field is used to filter them
- Gets entry metadata
- Uses "Developer" field for authors in case of manga and for Studios in case of Anime. "Publishers" field is used for Producers for Anime.
- Gets completion status and the added entry uses the correspondant type in Playnite.
- Can overwrite completion status in existing entries in Playnite if changed in Anilist
- Play Action opens the entry AniList URL. A play action is also added for MyAnimeList if data is available.
- Adds links to stream or read imported entries, provided by MAL-Sync's API.

Notes:
- Your profile must be public
- It's suggested to use an exclusive Playnite installation for this extension to not saturate the database with entries, genres and specially tags.
- The extension can't update in any way information back to AniList. It was made in mind for just viewing in Playnite, while all the entries updating would be automatically made by [MAL-Sync](https://malsync.moe/) in your browser.
- Extension is currently limited in what it can do. I plan to rewrite it as a Library extension in the future.

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

### Links Sorter

Playnite Forum Thread: https://playnite.link/forum/thread-401.html

<details>

<summary>Description</summary>

Simple extension that sorts the links of selected or all games in database by URL in ascending order (From A to Z, 0 to 9)

Please be aware that it sorts using the URL and not the link names.

This is with the purpose of not having to sort the game links manually each time a new one is added.

</details>

### NVIDIA GeforceExperience GameStream Export

Playnite Forum Thread: https://playnite.link/forum/thread-252.html
<details>
<summary>Description</summary>

This extension will export your selected games to NVIDIA Geforce Experience GameStreaming games database, allowing you to run them via a NVIDIA Shield or in any [Moonlight Game Streaming](https://moonlight-stream.org/) supported device, while retaining all the benefits of Playnite and allowing it to still manage your games.

</details>

### NVIDIA Geforce NOW Enabler

Playnite Forum Thread: https://playnite.link/forum/thread-298.html

<details>

<summary>Description</summary>

This extension will check which of your games have been enabled for the NVIDIA Geforce NOW Service, add "NVIDIA Geforce NOW" in their features to easily check them and also add a Play Action to the games to launch them via the service.

Compatible with games from Epic, Origin, Steam and Uplay.

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

### Steam Mini

Playnite Forum Thread: https://playnite.link/forum/thread-434.html

<details>

<summary>Description</summary>

This extension will make Steam launch in a minimal mode with minimal RAM usage by disabling the embedded browser, which is the main culprit of high RAM usage by Steam

There are 2 versions of the extension:

* Whitelist: The extension will only execute for games marked as whitelisted by the extension functions.
* Blacklist: The extension will execute for all games except the ones marked as blacklisted by the extension functions.

Disabling the browser helps you reduce RAM usage but comes with drawbacks.

What will work:

* You can play games normally
* You can install games and see your game list in Steam
* You can access Steam settings normally

What won't work while in this mode:

* You can't uninstall games (Applies trying to Uninstall via Playnite as well).
* Steam Chat will be unavailable while playing a game when the overlay is enabled in settings.
* You can't use the Steam browser, so you can't access community pages, browse the Store or use the overlay browser.

Please make sure to understand this and don't ask for support when the drawbacks have been explained.

To access the missing functions you can still launch Steam normally and it is recommended to use the "Auto Close Clients" feature in Playnite to not interfere with the uninstall feature by only launching Steam in this mode when a game will be played.

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
