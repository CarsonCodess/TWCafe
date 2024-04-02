using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static Extensions;

public class Player : NetworkBehaviour
{
    [SerializeField] private Transform dropTarget;
    [SerializeField] private GameObject holdingItemModel;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private GameObject baseItemPrefab;
    
    private NetworkList<int> _equippedItem = new NetworkList<int>(DefaultEmptyList(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> _interacting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    private InputHandler _inputHandler;
    
    private void Awake()
    {
        if (TryGetComponent(out _inputHandler))
        {
            _inputHandler.OnDrop += DropAndSpawnItem;
            _inputHandler.OnThrow += Throw;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_inputHandler)
        {
            _inputHandler.OnDrop -= DropAndSpawnItem;
            _inputHandler.OnThrow -= Throw;
        }
    }

    private void Update()
    {
        holdingItemModel.SetActive(_equippedItem[0] != 0);
        if(!IsOwner)
            return;
        _interacting.Value = Keyboard.current.eKey.wasPressedThisFrame;
    }
    
    public int GetBaseItem()
    {
        return _equippedItem[0];
    }
    
    public List<int> GetEntireItem()
    {
        return _equippedItem.ToList();
    }
    
    public void Pickup(List<int> item)
    {
        if(!IsOwner)
            return;
        DOVirtual.Float(0f, 1f, 0.1f, _ => {}).OnComplete(() => { PickupItem(item); });
    }

    private void PickupItem(List<int> item)
    {
        _equippedItem.Clear();
        foreach (var id in item)
            _equippedItem.Add(id);
        holdingItemModel.SetActive(true);
        var itemSo = GameManager.Instance.GetItemObject(item[0]);
        holdingItemModel.GetComponent<MeshFilter>().mesh = itemSo.mesh;
        holdingItemModel.GetComponent<MeshRenderer>().material = itemSo.material;
    }

    public void DropAndSpawnItem()
    {
        if(_equippedItem[0] == 0 || !IsOwner)
            return;
        DropAndSpawnItemServerRpc();
    }

    [ServerRpc]
    private void DropAndSpawnItemServerRpc()
    {
        SpawnItem(dropTarget.position);
    }

    private GameObject SpawnItem(Vector3 pos)
    {
        var itemObject = Instantiate(baseItemPrefab, pos, Quaternion.identity);
        itemObject.GetComponent<NetworkObject>().Spawn();
        itemObject.GetComponent<Pickup>().Initialize(_equippedItem.ToList());
        Drop();
        return itemObject;
    }

    public void Throw()
    {
        if(_equippedItem[0] == 0 || !IsOwner)
            return;
        ThrowServerRpc();
    }

    [ServerRpc]
    private void ThrowServerRpc()
    {
        var itemObject = SpawnItem(holdingItemModel.transform.position);
        itemObject.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }
    
    public void Drop()
    {
        _equippedItem.Clear();
        _equippedItem.Add(0);
        holdingItemModel.SetActive(false);
    }

    public bool IsPressingInteract()
    {
        return _interacting.Value;
    }
}
