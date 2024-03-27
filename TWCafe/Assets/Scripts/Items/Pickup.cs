using System.Collections.Generic;
using Unity.Netcode;

public class Pickup : Interactable
{
    private List<int> _ingredients;

    protected override void Update()
    {
        base.Update();
        foreach (var player in players)
        {
            if (player.IsPressingInteract() && player.GetItem() == 0)
            {
                player.Pickup(_ingredients);
                DespawnSelfServerRpc();
                break;
            }
        }
    }

    public void SetIngredients(List<int> ingredients)
    {
        _ingredients = ingredients;
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
