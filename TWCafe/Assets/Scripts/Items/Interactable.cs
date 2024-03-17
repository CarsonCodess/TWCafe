using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Interactable : NetworkBehaviour
{
    protected List<PlayerController> players = new List<PlayerController>();

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            players.Add(col.GetComponent<PlayerController>());
    }
    
    protected virtual void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            players.Remove(col.GetComponent<PlayerController>());
    }

    protected virtual void Update()
    {
        foreach (var player in players)
        {
            OnUpdate(player);
        }
    }

    protected abstract void OnUpdate(PlayerController player);
}
