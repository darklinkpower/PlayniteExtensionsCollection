function global:PCGamingWikiLinkOpen()
{
	#Set GameDatabase
	$GameDatabase = $PlayniteApi.MainView.SelectedGames

	foreach ($game in $GameDatabase) {
		if ("$($game.Source)" -eq "Steam" )
		{
			$url = 'https://pcgamingwiki.com/api/appid.php?appid=' + "$($game.GameId)"
			Start-Process $url
		}
		else
		{
			$url = 'http://pcgamingwiki.com/w/index.php?search=' + "$($game.name)"
			Start-Process $url
		}
	}
}