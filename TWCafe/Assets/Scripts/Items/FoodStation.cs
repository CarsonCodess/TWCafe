using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoodStation : NetworkBehaviour
{
    [SerializeField] private GameObject spawnPosition;
    [SerializeField] private GameObject indicator;
    [SerializeField] private FoodItemType type;
    [SerializeField] private float cookingTime;
    private NetworkVariable<int> _itemCooking = new NetworkVariable<int>();
    private PlayerController _player;
    private float _timer;

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            _player = col.GetComponent<PlayerController>();
            if (_player.GetItem() == 0)
                _player = null;
        }
    }
    
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            _player = null;
    }

    private void Update()
    {
        if (_player != null && _itemCooking.Value == 0 && _player.GetItem() > 0 && GetItemObject().foodType == type && _player.IsPressingInteract())
        {
            indicator.SetActive(true);
            AddItemServerRpc();
            _player.Drop();
        }
        
        if(_itemCooking.Value > 0 && !indicator.activeSelf)
            indicator.SetActive(true);
        else if(_itemCooking.Value <= 0)
            indicator.SetActive(false);

        if (_itemCooking.Value > 0)
        {
            _timer += Time.deltaTime;
            if (_timer >= cookingTime)
            {
                SpawnItemServerRpc();
                _timer = 0f;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddItemServerRpc()
    {
        _itemCooking.Value = _player.GetItem();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SpawnItemServerRpc()
    {
        var item = Instantiate(GameManager.Instance.GetItemObject(_itemCooking.Value).prefab,
            spawnPosition.transform.position, Quaternion.identity);
        item.GetComponent<NetworkObject>().Spawn();
        _itemCooking.Value = 0;
    }

    private Item GetItemObject()
    {
        return GameManager.Instance.GetItemObject(_player.GetItem());
    }
}
