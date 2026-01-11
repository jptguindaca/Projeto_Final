using UnityEngine;

public enum ItemType { None, Flashlight, Health }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Geral")]
    public string itemName;
    public Sprite itemIcon;
    public int maxStackSize;
    public bool consumable;
    public string pickupMessage;

    [Header("Prefabs")]
    public GameObject itemPrefab;
    public GameObject handItemPrefab;

    [Header("Uso")]
    public ItemType itemType = ItemType.None;
    public KeyCode useKey = KeyCode.F;
}
