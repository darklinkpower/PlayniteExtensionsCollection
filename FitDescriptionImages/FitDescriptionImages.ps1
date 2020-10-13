function Edit-DescriptionStyle
{
    #Set GameDatabase
    $GameDatabase = $PlayniteApi.Database.Games | Where-Object {$_.description}
    
    # Set description style
    $Style = '<style>img{max-width:100%;}</style>'

    # Set counters
    $AddedStyle = 0
    foreach ($game in $GameDatabase) {

        $GameDescription = $($game.description) -replace '\s',''
        if ($GameDescription -match '^<style>img{max-width:100%;}<\/style>.*$')
        {
            continue
        }
        else
        {
            $game.description = $Style + "`n" + $($game.description)
            $PlayniteApi.Database.Games.Update($game)
            $AddedStyle++
        }
    }
    # Show finish dialogue with results
    $PlayniteApi.Dialogs.ShowMessage("Edited description style of $AddedStyle games", "Fit Description Images");
}