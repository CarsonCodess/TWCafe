using UnityEngine;

public class Barrel : Interactable
{
    [SerializeField] private Item item;
    
    protected override void OnUpdate(PlayerController player)
    {
        if (player.IsPressingInteract() && player.GetItem() == 0)
            player.Pickup(item.itemId);
    }
}
