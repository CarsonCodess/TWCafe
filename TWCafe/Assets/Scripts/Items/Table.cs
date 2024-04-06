using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class Table : Interactable
{
    [Header("Table")]
    [SerializeField] private GameObject itemRenderer;
    [ReadOnly] public NetworkList<int> _items = new NetworkList<int>(Extensions.DefaultEmptyArray());
    private List<GameObject> _itemRenderers = new List<GameObject>();

    protected override void OnUpdate(Player player)
    {
        if (player != null && HasItem() && player.IsPressingInteract() && player.GetBaseItem() == 0)
        {
            foreach (var rend in _itemRenderers)
                Destroy(rend);
            _itemRenderers.Clear();

            player.Pickup(_items.ToList());
            SetItemServerRpc(Extensions.DefaultEmptyArray());
        }
        
        if (player != null && player.GetBaseItem() != 0 && player.IsPressingInteract())
        {
            if (!HasItem())
            {
                SetItemServerRpc(player.GetEntireItem().ToArray());
                SpawnItemModel(player);
                player.DropServerRpc();
            }
            else
            {
                var allItems = new List<int>(_items.ToList());
                allItems.AddRange(player.GetEntireItem());
                var canPlace = false;
                var allIngredients = allItems.Select(id => GameManager.Instance.GetItemObject(id)).ToList();
                foreach (var recipe in GameManager.Instance.recipes)
                {
                    var recipeIngredients = recipe.ingredients;
                    var allItemsMatch = allIngredients.All(ingredient => recipeIngredients.Contains(ingredient)); // Returns true if ALL of the ingredients in allIngredients are in recipeIngredients
                    var noExtraItems = allIngredients.Count(ingredient => recipeIngredients.Contains(ingredient)) == allIngredients.Count; // Checks if the amount of items that are in allIngredients AND recipeIngredients are equal to the number of allIngredients

                    if (allItemsMatch && noExtraItems)
                    {
                        canPlace = true;
                        break;
                    }
                }

                foreach (var playerItem in player.GetEntireItem()) // Checks if the player is trying to place an item that is already on the table
                {
                    if (_items.Contains(playerItem))
                    {
                        canPlace = false;
                        break;
                    }
                }
                
                if (canPlace)
                {
                    AddItemServerRpc(player.GetBaseItem());
                    SpawnItemModel(player);
                    player.DropServerRpc();
                }
            }
        }
    }

    private void SpawnItemModel(Player player)
    {
        foreach (var item in player.GetEntireItem())
        {
            var rend = Instantiate(itemRenderer, transform);
            rend.transform.localPosition = new Vector3(0f, 1f, 0f);
            var itemSo = GameManager.Instance.GetItemObject(item);
            rend.GetComponent<MeshFilter>().mesh = itemSo.mesh;
            rend.GetComponent<MeshRenderer>().material = itemSo.material;
            _itemRenderers.Add(rend);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (_itemRenderers.Count == 0 && _items[0] != 0)
        {
            foreach (var ingredient in _items)
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
    
    [ServerRpc(RequireOwnership = false)]
    private void SetItemServerRpc(int[] item)
    {
        _items.Clear();
        foreach (var i in item)
            _items.Add(i);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void AddItemServerRpc(int item)
    {
        _items.Add(item);
    }

    private bool HasItem()
    {
        return _items[0] != 0;
    }
}
