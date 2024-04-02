using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Barrel : Interactable
{
    [FormerlySerializedAs("item")] [SerializeField] private Ingredient ingredient;

    protected override void OnUpdate(Player player)
    {
        if (player.IsPressingInteract() && player.GetBaseItem() == 0)
            player.Pickup(new List<int>{ingredient.itemId});
    }
}
