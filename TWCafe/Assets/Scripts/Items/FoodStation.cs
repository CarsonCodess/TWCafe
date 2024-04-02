using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FoodStation : Interactable
{
    [SerializeField] private GameObject bar;
    [SerializeField] private Image barFill;
    [SerializeField] private Image burnBarFill;
    [SerializeField] private GameObject indicator;
    [SerializeField] private FoodItemType type;
    private NetworkVariable<int> _itemCooking = new NetworkVariable<int>();
    private float _cookTimer;
    private bool _isCooked;
    private bool _isBurnt;

    protected override void OnUpdate(Player player)
    {
        if (_itemCooking.Value > 0 && player != null && player.IsPressingInteract() && player.GetBaseItem() == 0)
        {
            _cookTimer = 0f;
            player.Pickup(new List<int>{_itemCooking.Value});
            SetItemServerRpc(0);
        }
        
        if (player != null && _itemCooking.Value == 0 && player.GetBaseItem() > 0 && GetItemObject(player).foodType == type && player.IsPressingInteract())
        {
            indicator.SetActive(true);
            bar.SetActive(true);
            SetCookBar(0);
            SetBurnBar(0);
            _isCooked = false;
            _isBurnt = false;
            SetItemServerRpc(player.GetBaseItem());
            indicator.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.GetItemObject(player.GetBaseItem()).icon;
            player.Drop();
        }
    }

    protected override void Update()
    {
        base.Update();
        if (_itemCooking.Value > 0 && !indicator.activeSelf)
        {
            indicator.SetActive(true);
            bar.SetActive(true);
        }
        else if (_itemCooking.Value <= 0)
        {
            indicator.SetActive(false);
            bar.SetActive(false);
        }

        if (_itemCooking.Value > 0)
        {
            _cookTimer += Time.deltaTime;
            if (IsBurning())
            {
                var item = GameManager.Instance.GetItemObject(_itemCooking.Value);
                var lerpValue = _cookTimer / item.burnTime;
                SetBurnBar(lerpValue);
            }
            
            if(_cookTimer >= GameManager.Instance.GetItemObject(_itemCooking.Value).burnTime)
            {
                _isCooked = false;
                _isBurnt = true;
            }

            if (IsCooking())
            {
                var lerpValue = _cookTimer / GameManager.Instance.GetItemObject(_itemCooking.Value).cookingTime;
                SetCookBar(lerpValue);
            }

            if (_cookTimer >= GameManager.Instance.GetItemObject(_itemCooking.Value).cookingTime && !_isCooked && !_isBurnt)
            {
                _isCooked = true;
                _cookTimer = 0f;
            }
        }
    }

    private void SetCookBar(float value)
    {
        barFill.rectTransform.sizeDelta = new Vector2(-Mathf.Lerp(0, 2, 1 - value), barFill.rectTransform.sizeDelta.y);
        barFill.rectTransform.anchoredPosition = new Vector2(-Mathf.Lerp(0, 2, 1 - value) / 2, barFill.rectTransform.anchoredPosition.y);
    }
    
    private void SetBurnBar(float value)
    {
        burnBarFill.rectTransform.sizeDelta = new Vector2(-Mathf.Lerp(0, 2, 1 - value), barFill.rectTransform.sizeDelta.y);
        burnBarFill.rectTransform.anchoredPosition = new Vector2(-Mathf.Lerp(0, 2, 1 - value) / 2, barFill.rectTransform.anchoredPosition.y);
    }

    private bool IsCooking()
    {
        return _cookTimer < GameManager.Instance.GetItemObject(_itemCooking.Value).cookingTime && !_isCooked;
    }

    private bool IsBurning()
    {
        return _cookTimer < GameManager.Instance.GetItemObject(_itemCooking.Value).burnTime && _isCooked;
    }
    

    [ServerRpc(RequireOwnership = false)]
    private void SetItemServerRpc(int item)
    {
        _itemCooking.Value = item;
    }

    private Ingredient GetItemObject(Player player)
    {
        return GameManager.Instance.GetItemObject(player.GetBaseItem());
    }
}
