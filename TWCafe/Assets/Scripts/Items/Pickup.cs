using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Pickup : Interactable
{
    [Header("Item")]
    [SerializeField] private GameObject itemRenderer;
    private List<int> _ingredients;
    private List<GameObject> _itemRenderers = new List<GameObject>();

    protected override void Update()
    {
        base.Update();
        foreach (var player in Players)
        {
            if (player.IsPressingInteract() && player.GetBaseItem() == 0)
            {
                player.Pickup(_ingredients);
                foreach (var rend in _itemRenderers)
                    Destroy(rend);
                _itemRenderers.Clear();
                DespawnSelfServerRpc();
                break;
            }
        }
    }

    public void Initialize(List<int> ingredients)
    {
        _ingredients = ingredients;
        foreach (var ingredient in ingredients)
        {
            var rend = Instantiate(itemRenderer, transform);
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
            var itemSo = GameManager.Instance.GetItemObject(ingredient);
            rend.GetComponent<MeshFilter>().mesh = itemSo.mesh;
            rend.GetComponent<MeshRenderer>().material = itemSo.material;
            _itemRenderers.Add(rend);
        }
    }

    protected override void OnUpdate(PlayerMovement player)
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnSelfServerRpc()
    {
        NetworkObject.Despawn();
    }
}
