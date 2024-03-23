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

    protected override void OnUpdate(PlayerController player)
    {
        if(!IsHost)
            return;
        if (player != null && _itemCooking.Value == 0 && player.GetItem() > 0 && GetItemObject(player).foodType == type && player.IsPressingInteract())
        {
            indicator.SetActive(true);
            bar.SetActive(true);
            indicator.GetComponent<SpriteRenderer>().sprite =
                GameManager.Instance.GetItemObject(player.GetItem()).icon;
            PlayerSetServerRpc();
            SetItemServerRpc(player.GetItem());
            player.Drop();
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
                if (Mathf.Approximately(_chopProgress.Value, 1f) || _chopProgress.Value >= 1f)
                {
                    if (player.GetItem() == 0)
                    {
                        player.Pickup(_itemCooking.Value);
                        SetItemServerRpc(0);
                    }
                }
                else
                    DOVirtual.Float(_chopProgress.Value, _chopProgress.Value + 1f / GameManager.Instance.GetItemObject(_itemCooking.Value).chopAmount, 0.15f, SetProgressServerRpc);
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

    private Item GetItemObject(PlayerController player)
    {
        return GameManager.Instance.GetItemObject(player.GetItem());
    }
}
