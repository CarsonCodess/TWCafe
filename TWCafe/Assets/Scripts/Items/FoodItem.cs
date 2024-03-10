using UnityEngine;

public enum FoodItemType
{
    Cook,
    Grill,
    Brew
}

[CreateAssetMenu(menuName = "Food Item")]
public class FoodItem : ScriptableObject
{
    public FoodItemType foodType;
}
