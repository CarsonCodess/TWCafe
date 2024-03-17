using Unity.Netcode;
using UnityEngine;

public class Pickup : Interactable
{
    [SerializeField] private Item item;

    protected override void Update()
    {
        base.Update();
        foreach (var player in players)
        {
            if (player.IsPressingInteract() && player.GetItem() == 0)
            {
                player.Pickup(item.itemId);
                DespawnSelfServerRpc();
                break;
            }
        }
    }

    protected override void OnUpdate(PlayerController player)
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnSelfServerRpc()
    {
        NetworkObject.Despawn();
    }
}
