using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public List<string> items = new List<string>();

    public void AddItem(string itemName)
    {
        items.Add(itemName);
        Debug.Log(itemName + " ins Inventar!");
    }

    public void ShowInventory()
    {
        Debug.Log("=== INVENTAR ===");
        for (int i = 0; i < items.Count; i++)
        {
            Debug.Log((i + 1) + ". " + items[i]);
        }
        if (items.Count == 0)
            Debug.Log("Leer!");
    }
}
