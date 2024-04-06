using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChoppingStation : Interactable
{
    [SerializeField] private GameObject bar;
    [SerializeField] private Image barFill;
    [SerializeField] private GameObject indicator;
    [SerializeField] private FoodItemType type;
    private NetworkVariable<int> _itemCooking = new NetworkVariable<int>();
    private NetworkVariable<float> _chopProgress = new NetworkVariable<float>();

    protected override void OnUpdate(Player player)
    {
        // if(!IsHost)
        //     return;
        if (player != null && _itemCooking.Value == 0 && player.GetBaseItem() > 0 && GetItemObject(player).foodType == type && player.IsPressingInteract())
        {
            indicator.SetActive(true);
            bar.SetActive(true);
            indicator.GetComponent<SpriteRenderer>().sprite =
                GameManager.Instance.GetItemObject(player.GetBaseItem()).icon;
            PlayerSetServerRpc();
            SetItemServerRpc(player.GetBaseItem());
            player.DropServerRpc();
        }

        if (_itemCooking.Value > 0 && !indicator.activeSelf)
        {
            indicator.SetActive(true);
            bar.SetActive(true);
            indicator.GetComponent<SpriteRenderer>().sprite =
                GameManager.Instance.GetItemObject(_itemCooking.Value).icon;
        }
        else if (_itemCooking.Value <= 0)
        {
            indicator.SetActive(false);
            bar.SetActive(false);
        }

        if (_itemCooking.Value > 0)
        {
            if (player != null && player.IsPressingInteract())
            {
                if (_chopProgress.Value >= 1f)
                {
                    if (player.GetBaseItem() == 0)
                    {
                        player.Pickup(new List<int>{_itemCooking.Value});
                        SetItemServerRpc(0);
                    }
                }
                else
                    DOVirtual.Float(_chopProgress.Value, _chopProgress.Value + (1f - Time.deltaTime / 4) / GameManager.Instance.GetItemObject(_itemCooking.Value).chopAmount, 0.15f, SetProgressServerRpc);
            }
        }
        
        barFill.rectTransform.sizeDelta = new Vector2(-Mathf.Lerp(0, 2, 1 - _chopProgress.Value), barFill.rectTransform.sizeDelta.y);
        SetProgressServerRpc(_chopProgress.Value - Time.deltaTime / 4);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerSetServerRpc()
    {
        _chopProgress.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetItemServerRpc(int item)
    {
        _itemCooking.Value = item;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetProgressServerRpc(float progress)
    {
        _chopProgress.Value = math.max(0, progress);
    }

    private Ingredient GetItemObject(Player player)
    {
        return GameManager.Instance.GetItemObject(player.GetBaseItem());
    }
}
