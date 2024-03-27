using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Food Item")]
public class Recipe : ScriptableObject
{
    public Ingredient baseIngredient;
    public List<Ingredient> ingredients;
}
