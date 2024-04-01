using UnityEngine;

public enum FoodItemType
{
    Cook,
    Grill,
    Brew
}

[CreateAssetMenu(menuName = "Food Item")]
public class Ingredient : ScriptableObject
{
    public int itemId;
    public Sprite icon;
    public FoodItemType foodType;
    public Mesh mesh;
    public Material material;
    public int chopAmount;
    public int cookingTime = 3;
    public int burnTime = 10;
}
