using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pickup : NetworkBehaviour
{
    [SerializeField] private Item item;
    
    private List<PlayerController> _players = new List<PlayerController>();

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            _players.Add(col.GetComponent<PlayerController>());
    }
    
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            _players.Remove(col.GetComponent<PlayerController>());
    }

    private void Update()
    {
        foreach (var player in _players)
        {
            if (player.IsPressingInteract())
            {
                player.Pickup(item.itemId);
                DespawnSelfServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnSelfServerRpc()
    {
        NetworkObject.Despawn();
    }
}
