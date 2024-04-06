using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class Pickup : Interactable
{
    [Header("Item")]
    [SerializeField] private GameObject itemRenderer;
    [ReadOnly] public NetworkList<int> _ingredients = new NetworkList<int>();
    private List<GameObject> _itemRenderers = new List<GameObject>();

    protected override void Update()
    {
        base.Update();
        if (_itemRenderers.Count == 0)
        {
            foreach (var ingredient in _ingredients)
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
    }
    
    public void Initialize(List<int> ingredients)
    {
        foreach (var ingredient in ingredients)
        {
            _ingredients.Add(ingredient);
            var rend = Instantiate(itemRenderer, transform);
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
            var itemSo = GameManager.Instance.GetItemObject(ingredient);
            rend.GetComponent<MeshFilter>().mesh = itemSo.mesh;
            rend.GetComponent<MeshRenderer>().material = itemSo.material;
            _itemRenderers.Add(rend);
        }
    }

    protected override void OnUpdate(Player player)
    {
        if (player.IsPressingInteract() && player.GetBaseItem() == 0)
        {
            player.Pickup(_ingredients.ToList());
            foreach (var rend in _itemRenderers)
                Destroy(rend);
            _itemRenderers.Clear();
            DespawnSelfServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnSelfServerRpc()
    {
        NetworkObject.Despawn();
    }
}
