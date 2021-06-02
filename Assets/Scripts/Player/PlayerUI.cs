using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerUI : NetworkBehaviour
{
    // -- References --
    [Header("References: gameobjects")]
    public GameObject inventory;
    public GameObject heldItem;
    public GameObject wornItem;
    public SyncList<Item> previousItems;

    [Header("References: crosshair")]
    public Image crosshair;
    public Sprite[] crosshairSprites;

    [Header("References: inventory")]
    public Image heldItemImage;
    public Image wornItemImage;
    public Image[] inventorySlots;

    [Header("References: status script")]
    public Image healthBar;
    public Image staminaBar;
    public Image blinkBar;
    public Image blinkOverlay;

    [Header("References: status text")]
    public Image statusTextBackground;
    public Text statusText;

    [Header("References: text")]
    public Text inventoryTooltipText;

    [Header("References: overlays")]
    public Image maskOverlay;

    // -- Variables --
    public float statusTextTimer = 5.0f;

    // -- Flags --
    [Header("Flags")]
    public bool showHeldItem;
    public bool showWornItem;
    public bool showInventory;
    public bool showMenu;
    public bool showStatusText;
    public bool hideCursor;

    // -- Functions --
    public void Update()
    {
        if (!isLocalPlayer) { return; }

        heldItem.SetActive(showHeldItem);
        wornItem.SetActive(showWornItem);
        inventory.SetActive(showInventory);

        if (showInventory || showMenu)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (showStatusText)
        {
            statusText.color = new Color(statusText.color.r, statusText.color.b, statusText.color.b, Mathf.Lerp(statusText.color.a, 1, 0.05f));
            statusTextBackground.color = new Color(statusTextBackground.color.r, statusTextBackground.color.b, statusTextBackground.color.b, Mathf.Lerp(statusTextBackground.color.a, 0.6f, 0.05f));

            statusTextTimer -= 1.0f * Time.deltaTime;

            if (statusTextTimer < 0)
            {
                showStatusText = false;
            }
        }
        else
        {
            statusText.color = new Color(statusText.color.r, statusText.color.b, statusText.color.b, Mathf.Lerp(statusText.color.a, 0, 0.05f));
            statusTextBackground.color = new Color(statusTextBackground.color.r, statusTextBackground.color.b, statusTextBackground.color.b, Mathf.Lerp(statusTextBackground.color.a, 0, 0.05f));
        }
    }

    public void SetHeldItem(string sprite)
    {
        if (sprite != null)
        {
            showHeldItem = true;
            heldItemImage.sprite = Resources.Load<Sprite>("Sprites/Items/" + sprite);
        }
        else 
        {
            showHeldItem = false;
            heldItemImage.sprite = null;
        }
    }

    public void SetWornItem(string sprite)
    {
        if (sprite != null)
        {
            showWornItem = true;
            wornItemImage.sprite = Resources.Load<Sprite>("Sprites/Items/" + sprite);
        }
        else
        {
            showWornItem = false;
            wornItemImage.sprite = null;
        }
    }

    public void MaskOverlay(float alpha)
    {
        maskOverlay.color = new Color(maskOverlay.color.r, maskOverlay.color.g, maskOverlay.color.b, alpha);
    }

    public void BlinkOverlay(float alpha, float speed)
    {
        blinkOverlay.color = new Color(blinkOverlay.color.r, blinkOverlay.color.g, blinkOverlay.color.b, Mathf.Lerp(blinkOverlay.color.a, alpha, speed));
    }

    public void InventoryToolTip(string text)
    {
        inventoryTooltipText.text = text;
    }

    public void StatusText(string text)
    {
        statusTextTimer = 5.0f;
        showStatusText = true;

        statusText.text = text;
    }

    /// <summary>
    /// Update inventory sprites, but only when contents are changed
    /// </summary>
    /// <param name="items">Current inventory list</param>
    public void UpdateInventory(SyncList<Item> items)
    {
        if (previousItems == null)
        {
            previousItems = items;
        }

        if (CheckInventoryLists(items, previousItems))
        {
            for (int i = 0; i < 10; i++)
            {
                inventorySlots[i].sprite = null;

                if (i < items.Count)
                {
                    inventorySlots[i].sprite = Resources.Load<Sprite>("Sprites/Items/" + items[i].spritePath);
                }

                if (inventorySlots[i].sprite == null)
                {
                    inventorySlots[i].color = new Color(inventorySlots[i].color.r, inventorySlots[i].color.g, inventorySlots[i].color.b, 0);
                }
                else
                {
                    inventorySlots[i].color = new Color(inventorySlots[i].color.r, inventorySlots[i].color.g, inventorySlots[i].color.b, 1);
                }
            }

            previousItems = items;
        }
    }

    /// <summary>
    /// Check if both lists contain the same contents
    /// </summary>
    /// <param name="items">Current inventory list</param>
    /// <param name="previousItems">Previous inventory list</param>
    /// <returns>boolean</returns>
    private bool CheckInventoryLists(SyncList<Item> items, SyncList<Item> previousItems)
    {
        if (items.Count != previousItems.Count)
        {
            return false;
        }

        for (int a = 0; a < items.Count; a++)
        {
            bool same = false;

            for (int b = 0; b < previousItems.Count; b++)
            {
                if (items[a] == previousItems[b])
                {
                    same = true;
                    break;
                }
            }

            if (!same)
            {
                return false;
            }
        }

        return true;
    }

    public void UpdateStatus(float[] data)
    {
        healthBar.fillAmount = Mathf.Ceil((data[0] / data[1]) * 10.0f) * 0.1f;
        staminaBar.fillAmount = Mathf.Ceil((data[2] / data[3]) * 10.0f) * 0.1f;
        blinkBar.fillAmount = Mathf.Ceil((data[4] / data[5]) * 10.0f) * 0.1f;
    }

    /// <summary>
    /// Update crosshair state.
    /// </summary>
    /// <param name="state">0: default, 1: finger, 2: grab, 3: cross</param>
    public void UpdateCrosshair(int state)
    {
        switch (state)
        {
            case 0: // default
                crosshair.sprite = crosshairSprites[0];
                crosshair.color = new Color(crosshair.color.r, crosshair.color.g, crosshair.color.b, 1);
                break;

            case 1: // finger
                crosshair.sprite = crosshairSprites[1];
                crosshair.color = new Color(crosshair.color.r, crosshair.color.g, crosshair.color.b, 1);
                break;

            case 2: // grab
                crosshair.sprite = crosshairSprites[2];
                crosshair.color = new Color(crosshair.color.r, crosshair.color.g, crosshair.color.b, 1);
                break;

            case 3: // cross
                crosshair.sprite = crosshairSprites[3];
                crosshair.color = new Color(crosshair.color.r, crosshair.color.g, crosshair.color.b, 1);
                break;

            case 4: // card
                crosshair.sprite = crosshairSprites[4];
                crosshair.color = new Color(crosshair.color.r, crosshair.color.g, crosshair.color.b, 1);
                break;

            default:
                crosshair.sprite = crosshairSprites[0];
                crosshair.color = new Color(crosshair.color.r, crosshair.color.g, crosshair.color.b, 1);
                break;
        }
    }
}
