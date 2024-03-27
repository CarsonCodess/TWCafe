using System.Collections.Generic;
using Sirenix.OdinInspector;
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
    public SerializedDictionary<int, Ingredient> items = new SerializedDictionary<int, Ingredient>();
    public List<Recipe> recipes = new List<Recipe>();
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
        var itemSoList = AssetDatabase.FindAssets("t:Ingredient");
        foreach (var item in itemSoList)
        {
            var path = AssetDatabase.GUIDToAssetPath(item);
            if(!items.ContainsKey(AssetDatabase.LoadAssetAtPath<Ingredient>(path).itemId))
                items.Add(AssetDatabase.LoadAssetAtPath<Ingredient>(path).itemId, AssetDatabase.LoadAssetAtPath<Ingredient>(path));
        }
        
        var recipeSoList = AssetDatabase.FindAssets("t:Recipe");
        foreach (var recipe in recipeSoList)
        {
            var path = AssetDatabase.GUIDToAssetPath(recipe);
            if(!recipes.Contains(AssetDatabase.LoadAssetAtPath<Recipe>(path)))
                recipes.Add(AssetDatabase.LoadAssetAtPath<Recipe>(path));
        }
#endif
    }

    public Ingredient GetItemObject(int itemId)
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
