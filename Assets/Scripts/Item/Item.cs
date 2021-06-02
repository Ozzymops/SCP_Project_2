using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public enum ItemType { Keycard, GasMask, FirstAidKit, Syringe, BallisticVest }

    // -- Variables --
    [Header("General")]
    public ItemType itemType;
    public int id;
    public string name;
    public string description;
    public string spritePath;
    public bool wornItem;
    public bool usableItem;
    public int soundType;

    [Header("Shared")]
    public int randomEffect;
    public bool causeBleeding;
    public bool gagItem;

    [Header("Keycards")]
    public int keycardClearance;

    [Header("Gas masks")]
    public float staminaMultiplier;
    public bool gasImmune;
    public bool mindImmune;
    public bool virusImmune;

    [Header("First aid kits")]
    public float healingAmount;

    [Header("Syringes")]
    public float adrenalineMultiplier;
    public float adrenalineTimer;

    [Header("Ballistic vests")]
    public float bulletResistance;
    public float weightMultiplier;
    public bool protectHead;
    public bool protectArms;
    public bool protectLegs;

    /// <summary>
    /// Keycard
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="name">Item name</param>
    /// <param name="description">Item description</param>
    /// <param name="spritePath">Item sprite</param>
    /// <param name="keycardClearance">Keycard clearance level</param>
    public Item(int id, string name, string description, string spritePath, int keycardClearance)
    {
        this.itemType = ItemType.Keycard;

        this.id = id;
        this.name = name;
        this.description = description;
        this.spritePath = spritePath;

        this.wornItem = false;
        this.usableItem = false;
        this.soundType = 2;

        this.keycardClearance = keycardClearance;
    }

    /// <summary>
    /// Gas mask
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="name">Item name</param>
    /// <param name="description">Item description</param>
    /// <param name="spritePath">Item sprite</param>
    /// <param name="staminaMultiplier">Stamina multiplier adjustment when worn</param>
    /// <param name="gasImmune">Make player gas immune?</param>
    /// <param name="mindImmune">Make player mind-control immune?</param>
    public Item(int id, string name, string description, string spritePath, float staminaMultiplier, bool gasImmune, bool mindImmune)
    {
        this.itemType = ItemType.GasMask;

        this.id = id;
        this.name = name;
        this.description = description;
        this.spritePath = spritePath;

        this.wornItem = true;
        this.usableItem = false;
        this.soundType = 1;

        this.staminaMultiplier = staminaMultiplier;
        this.gasImmune = gasImmune;
        this.mindImmune = mindImmune;
    }

    /// <summary>
    /// First aid kit
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="name">Item name</param>
    /// <param name="description">Item description</param>
    /// <param name="spritePath">Item sprite</param>
    /// <param name="healingAmount">Amount of healing applied</param>
    /// <param name="randomEffect">Roll for random effect?</param>
    public Item(int id, string name, string description, string spritePath, float healingAmount, bool randomEffect)
    {
        this.itemType = ItemType.FirstAidKit;

        this.id = id;
        this.name = name;
        this.description = description;
        this.spritePath = spritePath;

        this.wornItem = false;
        this.usableItem = true;
        this.soundType = 1;

        this.healingAmount = healingAmount;

        if (randomEffect)
        {
            this.randomEffect = Random.Range(0, 5);
        }
        else
        {
            this.randomEffect = -1;
        }      
    }

    /// <summary>
    /// Syringe
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="name">Item name</param>
    /// <param name="description">Item description</param>
    /// <param name="spritePath">Item sprite</param>
    /// <param name="adrenalineMultiplier">Stamina/speed multiplier</param>
    /// <param name="adrenalineTimer">Adrenaline effect timer</param>
    /// <param name="randomEffect">Roll for random effect?</param>
    /// <param name="causeBleeding">Cause bleeding?</param>
    public Item(int id, string name, string description, string spritePath, float adrenalineMultiplier, float adrenalineTimer, bool randomEffect, bool causeBleeding)
    {
        this.itemType = ItemType.Syringe;

        this.id = id;
        this.name = name;
        this.description = description;
        this.spritePath = spritePath;

        this.wornItem = false;
        this.usableItem = true;
        this.soundType = 1;

        this.adrenalineMultiplier = adrenalineMultiplier;
        this.adrenalineTimer = adrenalineTimer;

        this.causeBleeding = causeBleeding;

        if (randomEffect)
        {
            this.randomEffect = Random.Range(0, 3);
        }
        else
        {
            this.randomEffect = -1;
        }
    }

    /// <summary>
    /// Ballistic vest
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="name">Item name</param>
    /// <param name="description">Item description</param>
    /// <param name="spritePath">Item sprite</param>
    /// <param name="bulletResistance">Bullet damage reduction (%)</param>
    /// <param name="weightMultiplier">Speed multiplier</param>
    /// <param name="protectArms">Vest protects arms?</param>
    /// <param name="protectLegs">Vest protects legs?</param>
    public Item(int id, string name, string description, string spritePath, float bulletResistance, float weightMultiplier, bool protectHead, bool protectArms, bool protectLegs)
    {
        this.itemType = ItemType.BallisticVest;

        this.id = id;
        this.name = name;
        this.description = description;
        this.spritePath = spritePath;

        this.wornItem = true;
        this.usableItem = false;
        this.soundType = 1;

        this.bulletResistance = bulletResistance;
        this.weightMultiplier = weightMultiplier;
        this.protectHead = protectHead;
        this.protectArms = protectArms;
        this.protectLegs = protectLegs;
    }

    /// <summary>
    /// Duplicate item
    /// </summary>
    /// <param name="item">Item object</param>
    public Item(Item item)
    {
        this.itemType = item.itemType;

        this.id = item.id;
        this.name = item.name;
        this.description = item.description;
        this.spritePath = item.spritePath;

        this.wornItem = item.wornItem;
        this.usableItem = item.usableItem;
        this.soundType = item.soundType;

        this.keycardClearance = item.keycardClearance;

        this.staminaMultiplier = item.staminaMultiplier;
        this.gasImmune = item.gasImmune;
        this.mindImmune = item.mindImmune;

        this.healingAmount = item.healingAmount;
        this.randomEffect = item.randomEffect;

        this.bulletResistance = item.bulletResistance;
        this.weightMultiplier = item.weightMultiplier;
        this.protectHead = item.protectHead;
        this.protectArms = item.protectArms;
        this.protectLegs = item.protectLegs;
    }

    public Item()
    {
        // do nothing
    }
}
