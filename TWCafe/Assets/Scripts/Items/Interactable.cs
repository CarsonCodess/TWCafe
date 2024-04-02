using System;
using System.Collections.Generic;
using DG.Tweening;
using ExternPropertyAttributes;
using Unity.Netcode;
using UnityEngine;

public abstract class Interactable : NetworkBehaviour
{
    [Header("Interactable")]
    [SerializeField] protected bool highlight;
    [SerializeField] protected bool usePlayerDirection;
    [SerializeField, ShowIf("highlight")] protected Renderer meshRenderer;
    [SerializeField, ShowIf("highlight")] protected float brightness = 10;
    
    protected List<Player> Players = new List<Player>();

    protected virtual void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (usePlayerDirection)
            {
                Physics.Raycast(col.transform.position, col.transform.forward, out var hit);
                var isFacingObject = hit.transform != null && hit.transform.gameObject == gameObject;
                if (isFacingObject)
                {
                    if (highlight)
                        meshRenderer.material.DOFloat(1f - brightness / 100, "_TextureImpact", 0.25f);
                    Players.Add(col.GetComponent<Player>());
                }
            }
            else
            {
                if (highlight)
                    meshRenderer.material.DOFloat(1f - brightness / 100, "_TextureImpact", 0.25f);
                Players.Add(col.GetComponent<Player>());
            }
        }
    }

    private void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Player") && usePlayerDirection)
        {
            var playerController = col.GetComponent<Player>();
            var isPlayerTracked = Players.Contains(playerController);
            Physics.Raycast(col.transform.position, col.transform.forward, out var hit);
            var isFacingObject = hit.transform != null && hit.transform.gameObject == gameObject;
            if (isFacingObject && !isPlayerTracked)
            {
                if (highlight)
                        meshRenderer.material.DOFloat(1f - brightness / 100, "_TextureImpact", 0.25f);
                Players.Add(playerController);
            }
            else if (!isFacingObject && isPlayerTracked)
            {
                if (highlight)
                        meshRenderer.material.DOFloat(1f, "_TextureImpact", 0.25f);
                Players.Remove(playerController);
            }
        }
    }
    
    protected virtual void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (highlight)
                    meshRenderer.material.DOFloat(1f, "_TextureImpact", 0.25f);
            Players.Remove(col.GetComponent<Player>());
        }
    }

    protected virtual void Update()
    {
        foreach (var player in Players)
            OnUpdate(player);
    }

    protected abstract void OnUpdate(Player player);
}
