using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Inventory : MonoBehaviour
{
    [Header("Referência ao jogador")]
    public GameObject player;

    [Header("UI & Hotbar")]
    public GameObject hotbarObject;
    public GameObject inventorySlotParent;
    public GameObject container;

    [SerializeField] private TextMeshProUGUI healthText;

    public Image dragIcon;
    public TMP_Text pickupText;

    public float pickupRange = 3f;

    private Renderer loockedAtRenderer;

    private int equippedHotbarIndex = 0;
    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;

    [Header("Hand Item")]
    public Transform hand;
    private GameObject currentHandItem;

    private List<slot> inventorySlots = new List<slot>();
    private List<slot> hotbarSlots = new List<slot>();
    private List<slot> allSlots = new List<slot>();

    private slot draggedSlot;
    private bool isDragging;

    void Awake()
    {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<slot>());
        hotbarSlots.AddRange(hotbarObject.GetComponentsInChildren<slot>());

        allSlots.Clear();
        allSlots.AddRange(hotbarSlots);
        allSlots.AddRange(inventorySlots);

        if (pickupText != null)
            pickupText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            container.SetActive(!container.activeInHierarchy);

            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;

            Cursor.visible = !Cursor.visible;

            CameraController.Instance.updatingRotation = !CameraController.Instance.updatingRotation;
        }

        DetectLoockedAtItem();
        Pickup();

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();

        HandleHotbarSelection();
        HandleDropEquippedItem();
        UpdateHotbarOpacity();
        HandleUseEquippedItem();
    }

    IEnumerator LimparTextoDepoisDeTempo(float tempo)
    {
        yield return new WaitForSeconds(tempo);
        healthText.text = "";
    }

    private void Pickup()
    {
        if (loockedAtRenderer == null || !Input.GetKeyDown(KeyCode.E))
            return;

        item worldItem = loockedAtRenderer.GetComponent<item>();
        if (worldItem == null)
            return;

        bool wentToHotbar = AddItem(worldItem.newItem, worldItem.amount, out int hotbarIndexUsed);

        Destroy(worldItem.gameObject);

        if (wentToHotbar)
        {
            equippedHotbarIndex = hotbarIndexUsed;
            UpdateHotbarOpacity();
        }

        EquipHandItem();
    }

    private void DetectLoockedAtItem()
    {
        loockedAtRenderer = null;

        if (pickupText != null)
            pickupText.gameObject.SetActive(false);

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            item it = hit.collider.GetComponent<item>();
            if (it != null)
            {
                loockedAtRenderer = it.GetComponent<Renderer>();

                if (pickupText != null && it.newItem != null)
                {
                    pickupText.text = it.newItem.pickupMessage;
                    pickupText.gameObject.SetActive(true);
                }
            }
        }
    }

    private void HandleHotbarSelection()
    {
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                equippedHotbarIndex = i;
                UpdateHotbarOpacity();
                EquipHandItem();
            }
        }
    }

    private void UpdateHotbarOpacity()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            Image icon = hotbarSlots[i].GetComponent<Image>();
            if (icon != null)
            {
                icon.color = (i == equippedHotbarIndex)
                    ? new Color(1, 1, 1, equippedOpacity)
                    : new Color(1, 1, 1, normalOpacity);
            }
        }
    }

    private void HandleDropEquippedItem()
    {
        if (!Input.GetKeyDown(KeyCode.Q))
            return;

        slot equippedSlot = hotbarSlots[equippedHotbarIndex];
        if (!equippedSlot.HasItem())
            return;

        ItemSO itemSO = equippedSlot.GetItem();
        if (itemSO.itemPrefab == null)
            return;

        GameObject dropped = Instantiate(
            itemSO.itemPrefab,
            Camera.main.transform.position + Camera.main.transform.forward,
            Quaternion.identity
        );

        item item = dropped.GetComponent<item>();
        if (item != null)
        {
            item.newItem = itemSO;
            item.amount = 1;
        }

        int newAmount = equippedSlot.GetAmount() - 1;

        if (newAmount > 0)
            equippedSlot.SetItem(itemSO, newAmount);
        else
            equippedSlot.ClearSlot();

        EquipHandItem();
    }

    private void EquipHandItem()
    {
        if (currentHandItem != null)
            Destroy(currentHandItem);

        slot equippedSlot = hotbarSlots[equippedHotbarIndex];
        if (!equippedSlot.HasItem())
            return;

        ItemSO itemSO = equippedSlot.GetItem();
        if (itemSO.handItemPrefab == null)
            return;

        currentHandItem = Instantiate(itemSO.handItemPrefab, hand);
        currentHandItem.transform.localPosition = Vector3.zero;
        currentHandItem.transform.localRotation = Quaternion.Euler(90f, -90f, -90f);
    }

    public bool AddItem(ItemSO itemToAdd, int amount, out int hotbarIndexUsed)
    {
        hotbarIndexUsed = -1;
        int remaining = amount;

        slot firstTouchedSlot = null;

        foreach (slot s in allSlots)
        {
            if (s.HasItem() && s.GetItem() == itemToAdd)
            {
                int space = itemToAdd.maxStackSize - s.GetAmount();
                if (space > 0)
                {
                    int add = Mathf.Min(space, remaining);
                    s.SetItem(itemToAdd, s.GetAmount() + add);
                    remaining -= add;

                    if (firstTouchedSlot == null)
                        firstTouchedSlot = s;

                    if (remaining <= 0)
                        break;
                }
            }
        }

        if (remaining > 0)
        {
            foreach (slot s in allSlots)
            {
                if (!s.HasItem())
                {
                    int add = Mathf.Min(itemToAdd.maxStackSize, remaining);
                    s.SetItem(itemToAdd, add);
                    remaining -= add;

                    if (firstTouchedSlot == null)
                        firstTouchedSlot = s;

                    if (remaining <= 0)
                        break;
                }
            }
        }

        if (firstTouchedSlot != null)
        {
            int idx = hotbarSlots.IndexOf(firstTouchedSlot);
            if (idx >= 0)
            {
                hotbarIndexUsed = idx;
                return true;
            }
        }

        return false;
    }

    private void StartDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            slot s = GetHoveredSlot();
            if (s != null && s.HasItem())
            {
                draggedSlot = s;
                isDragging = true;
                dragIcon.sprite = s.GetItem().itemIcon;
                dragIcon.enabled = true;
            }
        }
    }

    private void EndDrag()
    {
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            slot s = GetHoveredSlot();
            if (s != null)
                HandleDrop(draggedSlot, s);

            dragIcon.enabled = false;
            isDragging = false;
            draggedSlot = null;
        }
    }

    private void UpdateDragItemPosition()
    {
        if (isDragging)
            dragIcon.transform.position = Input.mousePosition;
    }

    private slot GetHoveredSlot()
    {
        foreach (slot s in allSlots)
            if (s.hovering)
                return s;
        return null;
    }

    private void HandleDrop(slot from, slot to)
    {
        if (from == to) return;

        if (to.HasItem() && to.GetItem() == from.GetItem())
        {
            int space = to.GetItem().maxStackSize - to.GetAmount();
            if (space > 0)
            {
                int move = Mathf.Min(space, from.GetAmount());
                to.SetItem(to.GetItem(), to.GetAmount() + move);
                from.SetItem(from.GetItem(), from.GetAmount() - move);
                if (from.GetAmount() <= 0) from.ClearSlot();
                return;
            }
        }

        if (to.HasItem())
        {
            ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetAmount();
            to.SetItem(from.GetItem(), from.GetAmount());
            from.SetItem(tempItem, tempAmount);
        }
        else
        {
            to.SetItem(from.GetItem(), from.GetAmount());
            from.ClearSlot();
        }
    }

    private void HandleUseEquippedItem()
    {
        slot equippedSlot = hotbarSlots[equippedHotbarIndex];
        if (!equippedSlot.HasItem() || currentHandItem == null)
            return;

        ItemSO itemSO = equippedSlot.GetItem();
        IUsable usable = currentHandItem.GetComponent<IUsable>();
        if (usable == null)
            return;

        // Verifica a tecla para usar o item
        KeyCode keyToUse = KeyCode.F;
        if (itemSO.itemType == ItemType.Flashlight) keyToUse = KeyCode.F;
        else if (itemSO.itemType == ItemType.Health) keyToUse = KeyCode.H;

        if (!Input.GetKeyDown(keyToUse))
            return;

        //não usar se  a vida estiver cheia
        if (itemSO.itemType == ItemType.Health && player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null && ph.currentHealth >= ph.maxHealth)
            {
                healthText.text = "Max health !";
                Debug.Log("Max health !");

                StopAllCoroutines();
                StartCoroutine(LimparTextoDepoisDeTempo(2f));

                return;
            }
        }

        usable.Use(player);

        if (itemSO.consumable)
        {
            int newAmount = equippedSlot.GetAmount() - 1;
            if (newAmount > 0)
                equippedSlot.SetItem(itemSO, newAmount);
            else
                equippedSlot.ClearSlot();

            EquipHandItem();
        }
    }


}
