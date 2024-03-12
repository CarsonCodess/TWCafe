using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pickup : NetworkBehaviour
{
    [SerializeField] private Item item;
    
    private PlayerController _player;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            _player = col.GetComponent<PlayerController>();
    }
    
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            _player = null;
    }

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && _player != null)
        {
            _player.Pickup(item.itemId);
            DespawnSelfServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnSelfServerRpc()
    {
        NetworkObject.Despawn();
    }
}
