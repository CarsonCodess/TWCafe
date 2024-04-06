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
    
    private NetworkList<int> _equippedItem = new NetworkList<int>(DefaultEmptyList(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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
        DOVirtual.Float(0f, 1f, 0.1f, _ => {}).OnComplete(() => { PickupItemServerRpc(item.ToArray()); });
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickupItemServerRpc(int[] item)
    {
        _equippedItem.Clear();
        foreach (var id in item)
            _equippedItem.Add(id);
        holdingItemModel.SetActive(true);
        SetHoldingItemClientRpc(item[0]);
    }

    [ClientRpc]
    private void SetHoldingItemClientRpc(int item)
    {
        var itemSo = GameManager.Instance.GetItemObject(item);
        holdingItemModel.GetComponent<MeshFilter>().mesh = itemSo.mesh;
        holdingItemModel.GetComponent<MeshRenderer>().material = itemSo.material;
    }

    public void DropAndSpawnItem()
    {
        if(GetBaseItem() == 0 || !IsOwner)
            return;
        DropAndSpawnItemServerRpc();
        DropServerRpc();
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
        return itemObject;
    }

    public void Throw()
    {
        if(_equippedItem[0] == 0 || !IsOwner)
            return;
        ThrowServerRpc();
        DropServerRpc();
    }

    [ServerRpc]
    private void ThrowServerRpc()
    {
        var itemObject = SpawnItem(holdingItemModel.transform.position);
        itemObject.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void DropServerRpc()
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
