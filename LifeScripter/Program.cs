using SadConsole.Configuration;

Settings.WindowTitle = "Life Scripter";

Builder gameStartup = new Builder()
    .SetScreenSize(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
    .SetStartingScreen<LifeScripter.Scenes.RootScreen>()
    .IsStartingScreenFocused(true)
    .ConfigureFonts(true)
    ;

Game.Create(gameStartup);
Game.Instance.Run();
Game.Instance.Dispose();