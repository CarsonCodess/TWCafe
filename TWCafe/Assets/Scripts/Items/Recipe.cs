using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Recipe")]
public class Recipe : ScriptableObject
{
    public Ingredient baseIngredient;
    public List<Ingredient> ingredients;
}
