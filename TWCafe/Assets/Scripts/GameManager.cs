using UnityEngine;

public enum GameType
{
    Singleplayer,
    Multiplayer
}

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameType gameType;
    [SerializeField] private int fps = 60;

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = fps;
    }

    public GameType GetGameType()
    {
        return gameType;
    }

    public void SetGameType(GameType type)
    {
        gameType = type;
    }
}
