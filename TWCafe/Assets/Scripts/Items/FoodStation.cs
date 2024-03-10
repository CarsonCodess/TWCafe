using System;
using UnityEngine;

public class FoodStation : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [Space]
    [SerializeField] private FoodItemType type;
    [SerializeField] private float distance;
    [SerializeField] private float cookingTime;
    private FoodItem _itemCooking;

    private void Update()
    {
        if (Vector2.Distance(transform.position, player.transform.position) <= distance)
        {
            if (_itemCooking == null && player.GetItem() != null && player.GetItem().foodType == type)
            {
                _itemCooking = player.GetItem();
                player.Drop();
            }
        }
    }
}
