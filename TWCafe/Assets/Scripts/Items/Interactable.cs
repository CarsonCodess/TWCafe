using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Interactable : NetworkBehaviour
{
    [Header("Interactable")]
    [SerializeField] protected bool highlight;
    [SerializeField] protected bool usePlayerDirection;
    [SerializeField, ShowIf("highlight")] protected float brightness = 2.5f;
    protected List<Renderer> MeshRenderers;

    protected List<Player> Players = new List<Player>();

    public void Awake()
    {
        GetRenderers();
    }

    protected void GetRenderers()
    {
        MeshRenderers = GetComponents<Renderer>().ToList();
        if (MeshRenderers == null || MeshRenderers.Count == 0)
            MeshRenderers = GetComponentsInChildren<Renderer>().ToList();
    }

    protected virtual void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            if (usePlayerDirection)
            {
                Physics.Raycast(col.transform.position, col.transform.forward, out var hit);
                var isFacingObject = hit.transform != null && hit.transform.gameObject == gameObject;
                if (isFacingObject)
                    Highlight(col.transform);
            }
            else
                Highlight(col.transform);
        }
    }

    private void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Player") && usePlayerDirection)
        {
            var isPlayerTracked = Players.Contains(col.GetComponent<Player>());
            Physics.Raycast(col.transform.position, col.transform.forward, out var hit);
            var isFacingObject = hit.transform != null && hit.transform.gameObject == gameObject;
            if (isFacingObject && !isPlayerTracked)
                Highlight(col.transform);
            else if (!isFacingObject && isPlayerTracked)
                Unhighlight(col.transform);
        }
    }
    
    protected virtual void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
            Unhighlight(col.transform);
    }

    protected void Highlight(Transform tr)
    {
        if (highlight)
        {
            foreach (var renderer in MeshRenderers)
                renderer.material.DOFloat(1f - brightness / 100, "_TextureImpact", 0.25f);
        }

        Players.Add(tr.GetComponent<Player>());
    }
    
    protected void Unhighlight(Transform tr)
    {
        if (highlight)
        {
            foreach (var renderer in MeshRenderers)
                renderer.material.DOFloat(1f, "_TextureImpact", 0.25f);
        }

        Players.Remove(tr.GetComponent<Player>());
    }

    protected virtual void Update()
    {
        foreach (var player in Players)
            OnUpdate(player);
    }

    protected abstract void OnUpdate(Player player);
}
