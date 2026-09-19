// What the next gameplay scene should do. Set by the main menu, read by GameManager.
public enum GameMode { Story = 0, Endless = 1 }

public static class GameState
{
    public static GameMode Mode = GameMode.Story;
    public static bool ContinueStory;       // true: resume the saved story night
    public static int ForcedNight = -1;     // used by "Restart night"

    public static void BeginStory(bool resume)
    {
        Mode = GameMode.Story;
        ContinueStory = resume;
        ForcedNight = -1;
        if (!resume) SaveSystem.ResetStory();
    }

    public static void BeginEndless()
    {
        Mode = GameMode.Endless;
        ContinueStory = false;
        ForcedNight = -1;
    }
}
