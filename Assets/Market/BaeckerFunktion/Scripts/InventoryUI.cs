using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public TextMeshProUGUI inventoryText;
    private bool inventoryOpen = false;

    void Start()
    {
        inventoryText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryOpen = !inventoryOpen;
            inventoryText.gameObject.SetActive(inventoryOpen);
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        inventoryText.text = "=== INVENTAR ===\n";
	inventoryText.text += "As: " + inventory.GetComponent<Geldbeutel>().as_Muenzen + "\n\n";
        
        if (inventory.items.Count == 0)
        {
            inventoryText.text += "Leer!";
        }
        else
        {
            for (int i = 0; i < inventory.items.Count; i++)
            {
                inventoryText.text += (i + 1) + ". " + inventory.items[i] + "\n";
            }
        }
    }
}
