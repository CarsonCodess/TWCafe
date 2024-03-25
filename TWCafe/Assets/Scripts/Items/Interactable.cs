using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public abstract class Interactable : NetworkBehaviour
{
    [SerializeField] protected bool highlight;
    [SerializeField] protected bool usePlayerDirection;
    [SerializeField, ShowIf("highlight")] protected Renderer meshRenderer;
    [SerializeField, ShowIf("highlight")] protected float brightness = 10;
    
    protected List<PlayerController> players = new List<PlayerController>();

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
                    players.Add(col.GetComponent<PlayerController>());
                }
            }
            else
            {
                if (highlight)
                    meshRenderer.material.DOFloat(1f - brightness / 100, "_TextureImpact", 0.25f);
                players.Add(col.GetComponent<PlayerController>());
            }
        }
    }

    private void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Player") && usePlayerDirection)
        {
            var playerController = col.GetComponent<PlayerController>();
            var isPlayerTracked = players.Contains(playerController);
            Physics.Raycast(col.transform.position, col.transform.forward, out var hit);
            var isFacingObject = hit.transform != null && hit.transform.gameObject == gameObject;
            if (isFacingObject && !isPlayerTracked)
            {
                if (highlight)
                        meshRenderer.material.DOFloat(1f - brightness / 100, "_TextureImpact", 0.25f);
                players.Add(playerController);
            }
            else if (!isFacingObject && isPlayerTracked)
            {
                if (highlight)
                        meshRenderer.material.DOFloat(1f, "_TextureImpact", 0.25f);
                players.Remove(playerController);
            }
        }
    }
    
    protected virtual void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (highlight)
                    meshRenderer.material.DOFloat(1f, "_TextureImpact", 0.25f);
            players.Remove(col.GetComponent<PlayerController>());
        }
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
