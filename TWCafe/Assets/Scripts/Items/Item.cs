using UnityEngine;

public enum FoodItemType
{
    Cook,
    Grill,
    Brew
}

[CreateAssetMenu(menuName = "Food Item")]
public class Item : ScriptableObject
{
    public int itemId;
    public FoodItemType foodType;
    public GameObject prefab;
}
