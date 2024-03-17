using Unity.Netcode;
using UnityEngine;

public class FoodStation : Interactable
{
    [SerializeField] private GameObject indicator;
    [SerializeField] private FoodItemType type;
    [SerializeField] private float cookingTime;
    private NetworkVariable<int> _itemCooking = new NetworkVariable<int>();
    private float _timer;

    protected override void OnUpdate(PlayerController player)
    {
        if (player != null && _itemCooking.Value == 0 && player.GetItem() > 0 && GetItemObject(player).foodType == type && player.IsPressingInteract())
        {
            indicator.SetActive(true);
            SetItemServerRpc(player.GetItem());
            player.Drop();
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
                if (player != null && player.IsPressingInteract() && player.GetItem() == 0)
                {
                    _timer = 0f;
                    player.Pickup(_itemCooking.Value);
                    SetItemServerRpc(0);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetItemServerRpc(int item)
    {
        _itemCooking.Value = item;
    }

    private Item GetItemObject(PlayerController player)
    {
        return GameManager.Instance.GetItemObject(player.GetItem());
    }
}
