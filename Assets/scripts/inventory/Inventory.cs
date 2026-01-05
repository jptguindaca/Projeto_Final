using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public GameObject hotbarObject;
    public GameObject inventorySlotParent;
    public GameObject container;

    public Image dragIcon;

    public float pickupRange = 3f;
    public Material highlightMaterial;

    private Material originalMaterial;
    private Renderer loockedAtRenderer;

    private int equippedHotbarIndex = 0;
    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;

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

        // HOTBAR primeiro, INVENTORY depois
        allSlots.Clear();
        allSlots.AddRange(hotbarSlots);
        allSlots.AddRange(inventorySlots);
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
    }

    // ---------------- PICKUP ----------------
    private void Pickup()
    {
        if (loockedAtRenderer == null || !Input.GetKeyDown(KeyCode.E))
            return;

        item worldItem = loockedAtRenderer.GetComponent<item>();
        if (worldItem == null)
            return;

        // Adiciona e tenta auto-equipar se tiver ido para a hotbar
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
        if (loockedAtRenderer != null)
        {
            loockedAtRenderer.material = originalMaterial;
            loockedAtRenderer = null;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            item item = hit.collider.GetComponent<item>();
            if (item != null)
            {
                Renderer rend = item.GetComponent<Renderer>();
                if (rend != null)
                {
                    originalMaterial = rend.material;
                    rend.material = highlightMaterial;
                    loockedAtRenderer = rend;
                }
            }
        }
    }

    // ---------------- HOTBAR ----------------
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

    // ---------------- DROP ----------------
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
            item.amount = equippedSlot.GetAmount();
        }

        equippedSlot.ClearSlot();
        EquipHandItem();
    }

    // ---------------- HAND ITEM ----------------
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
        currentHandItem.transform.localRotation = Quaternion.identity;
    }

    // ---------------- INVENTORY ----------------
    // Retorna true se o primeiro slot usado foi na hotbar e devolve o índice desse slot.
    public bool AddItem(ItemSO itemToAdd, int amount, out int hotbarIndexUsed)
    {
        hotbarIndexUsed = -1;
        int remaining = amount;

        slot firstTouchedSlot = null;

        // 1) Tentar empilhar (hotbar primeiro porque allSlots tem hotbar primeiro)
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

        // 2) Tentar colocar em slots vazios (hotbar primeiro)
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

        // Se o primeiro slot tocado for da hotbar, auto-equip
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

    // ---------------- DRAG ----------------
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
}
