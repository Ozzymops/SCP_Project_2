using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerInventory : NetworkBehaviour
{
    // -- References --
    [Header("References")]
    public ItemDatabase itemDatabase;
    [SerializeField]
    public SyncList<Item> items = new SyncList<Item>();
    public int itemCount;
    public SyncList<GameObject> objects = new SyncList<GameObject>();

    [SyncVar]
    public string heldSprite;

    [SyncVar]
    public string wornSprite;

    // -- Variables --
    [Header("Variables: inventory")]
    [SyncVar]
    public int heldItemIndex = -1;

    [SyncVar]
    public int wornItemIndex = -1;

    [SyncVar]
    public int currentHeldSoundType;

    [SyncVar]
    public int currentWornSoundType;

    [Header("Variables: items")]
    [SyncVar]
    public int keycardClearance = 0;

    [SyncVar]
    public float staminaMultiplier = 1.0f;

    [SyncVar]
    public float adrenalineMultiplier = 1.0f;

    [SyncVar]
    public float bulletResistance = 1.0f;

    [SyncVar]
    public float speedMultiplier = 1.0f;

    // -- Flags --
    [Header("Flags: inventory")]
    public bool inventoryFull;

    [SyncVar]
    public bool holdingItem;

    [SyncVar]
    public bool wearingItem;

    [Header("Flags: items")]
    [SyncVar]
    public bool gasImmune;

    [SyncVar]
    public bool virusImmune;

    [SyncVar]
    public bool mindImmune;

    // -- Functions --
    public void Update()
    {
        itemCount = items.Count;

        if (!isLocalPlayer) { return; }

        if (items.Count > 9)
        {
            inventoryFull = true;
        }
        else
        {
            inventoryFull = false;
        }

        if (heldItemIndex != -1)
        {
            holdingItem = true;
        }
        else
        {
            holdingItem = false;
        }

        if (wornItemIndex != -1)
        {
            wearingItem = true;
        }
        else
        {
            wearingItem = false;
        }
    }

    public void EquipHeld(Item item)
    {
        heldSprite = items[heldItemIndex].spritePath;
        currentHeldSoundType = items[heldItemIndex].soundType;
        keycardClearance = items[heldItemIndex].keycardClearance;
    }

    public void UnequipHeld()
    {
        heldSprite = null;
        keycardClearance = 0;
    }

    public void EquipWorn(Item item)
    {
        wornSprite = items[wornItemIndex].spritePath;
        currentWornSoundType = items[wornItemIndex].soundType;       
        gasImmune = items[wornItemIndex].gasImmune;
        mindImmune = items[wornItemIndex].mindImmune;
        virusImmune = items[wornItemIndex].virusImmune;
        staminaMultiplier = items[wornItemIndex].staminaMultiplier;
        speedMultiplier = items[wornItemIndex].weightMultiplier;
        bulletResistance = items[wornItemIndex].bulletResistance;
    }

    public void UnequipWorn()
    {
        wornSprite = null;      
        gasImmune = false;
        mindImmune = false;
        virusImmune = false;
        staminaMultiplier = 1.0f;
        speedMultiplier = 1.0f;
        bulletResistance = 1.0f;
    }

    public void Adrenaline(float multiplier)
    {
        adrenalineMultiplier = multiplier;
    }

    [Command]
    public void Cmd_HoldItem(int index)
    {
        if (index < items.Count)
        {
            if (items[index].wornItem)
            {
                if (wornItemIndex != -1)
                {
                    if (wornItemIndex != index)
                    {
                        wornItemIndex = index;
                        EquipWorn(items[wornItemIndex]);
                    }
                    else
                    {
                        wornItemIndex = -1;
                        UnequipWorn();
                    }
                }
                else
                {
                    wornItemIndex = index;
                    EquipWorn(items[wornItemIndex]);
                }
            }
            else
            {
                if (heldItemIndex != -1)
                {
                    if (heldItemIndex != index)
                    {
                        heldItemIndex = index;
                        EquipHeld(items[heldItemIndex]);
                    }
                    else
                    {
                        heldItemIndex = -1;
                        UnequipHeld();
                    }
                }
                else
                {
                    heldItemIndex = index;
                    EquipHeld(items[heldItemIndex]);
                }
            }  
        }      
    }

    [Command]
    public void Cmd_TakeItem(GameObject gameObject, int id)
    {
        items.Add(itemDatabase.GetItem(id));
        objects.Add(gameObject);
    }

    [Command]
    public void Cmd_DropItem(int index)
    {
        if (index < items.Count)
        {
            if (index == wornItemIndex)
            {               
                wornItemIndex = -1;

                UnequipWorn();
            }

            if (index == heldItemIndex)
            {             
                heldItemIndex = -1;

                UnequipHeld();
            }

            if (heldItemIndex != -1)
            {
                heldItemIndex -= 1;
            }

            if (wornItemIndex != -1)
            {
                wornItemIndex -= 1;     
            }

            objects[index].GetComponent<ItemPickup>().Cmd_Dropped(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z));

            items.RemoveAt(index);
            objects.RemoveAt(index);
        }
    }

    [Command]
    public void Cmd_DestroyItem(int index)
    {
        if (index < items.Count)
        {
            if (index == wornItemIndex)
            {
                wornItemIndex = -1;

                UnequipWorn();
            }

            if (index == heldItemIndex)
            {
                heldItemIndex = -1;

                UnequipHeld();
            }

            if (heldItemIndex != -1)
            {
                heldItemIndex -= 1;
            }

            if (wornItemIndex != -1)
            {
                wornItemIndex -= 1;
            }

            objects[index].GetComponent<ItemPickup>().Cmd_Destroy();

            items.RemoveAt(index);
            objects.RemoveAt(index);
        }
    }

    [Command]
    public void Cmd_DropAll()
    {
        int limit = items.Count;

        for (int i = 0; i < limit; i++)
        {
            Cmd_DropItem(0);
        }
    }
}
