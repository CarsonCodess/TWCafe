using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum GameType
{
    Singleplayer,
    Multiplayer
}

public class GameManager : Singleton<GameManager>
{
    public SerializedDictionary<int, Item> items = new SerializedDictionary<int, Item>();
    [SerializeField] private GameType gameType;
    [SerializeField] private int fps = 60;

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = fps;
    }

    [Button("Auto Fill Items")]
    public void PopulateItemsDictionary()
    {
#if UNITY_EDITOR
        var itemSoList = AssetDatabase.FindAssets("t:FoodItem");
        foreach (var item in itemSoList)
        {
            var path = AssetDatabase.GUIDToAssetPath(item);
            if(!items.ContainsKey(AssetDatabase.LoadAssetAtPath<Item>(path).itemId))
                items.Add(AssetDatabase.LoadAssetAtPath<Item>(path).itemId, AssetDatabase.LoadAssetAtPath<Item>(path));
        }
#endif
    }

    public Item GetItemObject(int itemId)
    {
        return items[itemId];
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
