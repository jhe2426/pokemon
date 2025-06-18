using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Merchant : MonoBehaviour
{
    [SerializeField] List<ItemBase> availableItems;
    
    public IEnumerator Trade()
    { 
        yield return ShopController.i.StartTrading(this);
    }

    public List<ItemBase> AvailableItems => availableItems;
}
