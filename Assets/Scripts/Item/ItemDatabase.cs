using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemDatabase : NetworkBehaviour
{
    // -- References --
    public List<Item> items = new List<Item>();

    public override void OnStartServer()
    {
        base.OnStartServer();

        BuildDatabase();
    }

    public void BuildDatabase()
    {
        Debug.Log("Retrieving json data...");



        Debug.Log("Building item database...");

        items = new List<Item>()
        {
            // Keycards
            new Item(0, "Keycard Lv. 1", "<i>Clearance level 1.</i>\nUsed to gain access to other rooms.", "Keycard_1", 1),
            new Item(1, "Keycard Lv. 2", "<i>Clearance level 2.</i>\nUsed to gain access to other rooms.", "Keycard_2", 2),
            new Item(2, "Keycard Lv. 3", "<i>Clearance level 3.</i>\nUsed to gain access to other rooms.", "Keycard_3", 3),
            new Item(3, "Keycard Lv. 4", "<i>Clearance level 4.</i>\nUsed to gain access to other rooms.", "Keycard_4", 4),
            new Item(4, "Keycard Lv. 5", "<i>Clearance level 5.</i>\nUsed to gain access to other rooms.", "Keycard_5", 5),
            new Item(5, "Omni-keycard", "<i>05 clearance level.</i>\nUsed to gain access to other rooms.", "Keycard_O", 6),
            // Gas masks
            new Item(6, "Gas mask", "Protects the wearer from inhaling toxic gases.", "Gasmask", 1.00f, true, false),
            new Item(7, "Refined gas mask", "<i>Refinement allows for improved air circulation.</i>\nProtects the wearer from inhaling toxic gases.", "SuperGasmask", 2.00f, true, false),
            new Item(8, "Heavy gas mask", "<i>Imbued with the effects of SCP-148.</i>\nProtects the wearer from inhaling toxic gases.", "HeavyGasmask", 2.00f, true, true),
            // First aid kits
            new Item(9, "First aid kit", "A collection of supplies and equipment used to give medical treatment.\nThere is enough for one use.", "FirstAidKit", 20.0f, false),
            new Item(10, "Small first aid kit", "A collection of <i>advanced</i> supplies and equipment used to give medical treatment.\nThere is enough for one use.", "MiniFirstAidKit", 60.0f, false),
            new Item(11, "Blue first aid kit", "<i>Emits a odd, blue glow.</i>\nA collection of supplies and equipment used to give medical treatment.\nThere is enough for one use.", "BlueFirstAidKit", 20.0f, true),
            // Syringes
            new Item(12, "Syringe", "A syringe filled with a serum of epinephrine, activating the adrenal glands within the body.", "Syringe", 1.5f, 10.0f, false, false),
            new Item(13, "Refined syringe", "A syringe filled with an <i>enhanced</i> serum of epinephrine, activating the adrenal glands within the body.\n<i>Has an extremely sharp needle.</i>", "FineSyringe", 2.0f, 25.0f, false, true),
            new Item(14, "Blue syringe", "<i>Emits a odd, blue glow.</i>\nA syringe filled with a serum of epinephrine, activating the adrenal glands within the body.", "VeryFineSyringe", 1.5f, 10.0f, true, false),
            // Ballistic vests
            new Item(15, "Ballistic vest", "A lightweight, kevlar-woven vest, designed to absorb kinetic forces to protect the wearer.\n<i>Will not protect the arms, legs and head.</i>", "BallisticVest", 0.75f, 0.9f, false, false, false),
            new Item(16, "Heavy ballistic vest", "A heavy, ceramic-plated vest, designed to absorb extreme kinetic forces to protect the wearer.\n<i>Will not protect the head.</i>", "HeavyBallisticVest", 0.33f, 0.75f, false, true, true),
            new Item(17, "Bulky ballistic vest", "A complete EOD outfit, designed to absorb the harshest of kinetic forces to protect the wearer.\n<i>Impossibly heavy.</i>", "BulkyBallisticVest", 0.1f, 0.2f, true, true, true)
        };

        Debug.Log("Built item database with " + items.Count + " items present.");
    }

    public Item GetItem(int id)
    {
        return items.Find(item => item.id == id);
    }

    public Item GetItem(string name)
    {
        return items.Find(item => item.name == name);
    }
}
