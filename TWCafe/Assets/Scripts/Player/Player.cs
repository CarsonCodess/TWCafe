using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static Extensions;

[HideMonoScript]
public class Player : NetworkBehaviour
{
    [Header("Holding")]
    [HorizontalLine(Thickness = 2, Padding = 15)]
    [SerializeField] private GameObject holdingItemParent;
    [Header("Dropping/Throwing")]
    [HorizontalLine(Thickness = 2, Padding = 15)]
    [SerializeField] private Transform dropTarget;
    [SerializeField] private float throwForce = 10f;
    [Header("Item Prefabs")]
    [HorizontalLine(Thickness = 2, Padding = 15)]
    [SerializeField] private GameObject baseItemPrefab;
    [SerializeField] private GameObject itemRendererPrefab;
    
    private NetworkList<int> _equippedItem = new NetworkList<int>(DefaultEmptyList(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> _interacting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    private InputHandler _inputHandler;
    
    private List<GameObject> _itemRenderers = new List<GameObject>();
    
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
        holdingItemParent.SetActive(_equippedItem[0] != 0);
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
        SetHoldingItemClientRpc(item);
    }

    [ClientRpc]
    private void SetHoldingItemClientRpc(int[] ingredients)
    {
        foreach (var ingredient in ingredients)
        {
            var rend = Instantiate(itemRendererPrefab, holdingItemParent.transform);
            rend.transform.localPosition = new Vector3(0f, -0.1f, -0.3f);
            rend.transform.localRotation = Quaternion.Euler(new Vector3(75f, 10f, -10f));
            var itemSo = GameManager.Instance.GetItemObject(ingredient);
            rend.GetComponent<MeshFilter>().mesh = itemSo.mesh;
            rend.GetComponent<MeshRenderer>().material = itemSo.material;
            _itemRenderers.Add(rend);
        }
    }

    [ClientRpc]
    private void ClearHoldingItemClientRpc()
    {
        foreach (var rend in _itemRenderers)
            Destroy(rend);
        _itemRenderers.Clear();
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
        var itemObject = SpawnItem(holdingItemParent.transform.position);
        itemObject.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void DropServerRpc()
    {
        ClearHoldingItemClientRpc();
        _equippedItem.Clear();
        _equippedItem.Add(0);
    }

    public bool IsPressingInteract()
    {
        return _interacting.Value;
    }
}
