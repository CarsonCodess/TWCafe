using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Barrel : Interactable
{
    [FormerlySerializedAs("item")] [SerializeField] private Ingredient ingredient;
    //[SerializeField] private SpriteRenderer label;

    private void Awake()
    {
        //label.sprite = item.icon;
        //label.gameObject.SetActive(true);
    }

    protected override void OnUpdate(PlayerController player)
    {
        if (player.IsPressingInteract() && player.GetItem() == 0)
            player.Pickup(new List<int>{ingredient.itemId});
    }
}
