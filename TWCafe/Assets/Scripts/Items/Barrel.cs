using System;
using UnityEngine;

public class Barrel : Interactable
{
    [SerializeField] private Item item;
    [SerializeField] private SpriteRenderer label;

    private void Awake()
    {
        label.sprite = item.icon;
        label.gameObject.SetActive(true);
    }

    protected override void OnUpdate(PlayerController player)
    {
        if (player.IsPressingInteract() && player.GetItem() == 0)
            player.Pickup(item.itemId);
    }
}
