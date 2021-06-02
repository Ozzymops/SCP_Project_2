using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    // -- References --
    [Header("References: transforms")]
    public Transform cameraTransform;
    public Transform modelTransform;
    public Transform headTransform;
    public GameObject maskTransform;
    public GameObject[] eyeTransforms;
    public Material[] eyeMaterials;
    public PostProcessVolume postProcessVolume;

    [Header("References: components")]
    public CharacterController controller;
    public Canvas canvas;
    public AudioLowPassFilter lowPassFilter;
    public AudioHighPassFilter highPassFilter;

    [Header("References: scripts")]
    public PlayerCamera cameraScript;
    public PlayerMovement movementScript;
    public PlayerSounds soundScript;
    public PlayerInventory inventoryScript;
    public PlayerUI uiScript;
    public PlayerStatus statusScript;
    public PlayerMusic musicScript;

    // -- Variables --
    [Header("Variables: items")]
    public int keycardClearance = 0;
    private int currentHeldItem;
    private int currentWornItem;

    private float focalLength;
    private float adrenalineTimer;

    // -- Flags --
    [Header("Flags")]
    public bool canLook;
    public bool canMove;
    private bool itemHeldCheck;
    private bool itemWornCheck;
    private bool inventoryUICheck;
    private bool adrenalineCheck = true;
    public bool dead;

    // -- Functions --
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Get itemdatabase for inventory
        inventoryScript.itemDatabase = GameObject.Find("ItemManager").GetComponent<ItemDatabase>();

        // Disable canvas
        canvas.enabled = false;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Get main camera and parent to player
        cameraTransform = Camera.main.transform;
        cameraScript.cameraTransform = cameraTransform;
        cameraTransform.SetParent(transform);
        cameraTransform.localPosition = cameraScript.offset;

        // Get audio filters
        lowPassFilter = cameraTransform.GetComponent<AudioLowPassFilter>();
        highPassFilter = cameraTransform.GetComponent<AudioHighPassFilter>();

        // Hide own model
        modelTransform.gameObject.SetActive(false);

        // Display canvas
        canvas.enabled = true;

        // Set up post process volume + profiles
        postProcessVolume = cameraTransform.GetChild(0).GetComponent<PostProcessVolume>();

        // -- Vignette
        if (postProcessVolume.profile.TryGetSettings<Vignette>(out var vignette))
        {
            vignette.intensity.overrideState = true;
        }

        // -- DoF
        if (postProcessVolume.profile.TryGetSettings<DepthOfField>(out var dof))
        {
            dof.focalLength.overrideState = true;
        }

        // -- Lens distortion
        if (postProcessVolume.profile.TryGetSettings<LensDistortion>(out var lensDistortion))
        {
            lensDistortion.intensity.overrideState = true;
        }
    }

    public void Update()
    {
        // Set globally visible things
        // -- 'Dead' boolean
        dead = statusScript.dead;
        cameraScript.dead = dead;
        movementScript.dead = dead;
        musicScript.dead = dead;

        // -- Miscellaneous booleans
        cameraScript.canLook = canLook;
        movementScript.canMove = canMove;
        statusScript.moving = movementScript.moving;
        soundScript.moving = movementScript.moving;
        soundScript.sprinting = movementScript.sprinting;
        soundScript.sneaking = movementScript.sneaking;

        // -- Status values
        movementScript.speedMulti = inventoryScript.speedMultiplier;   
        statusScript.staminaMultiplier = inventoryScript.staminaMultiplier * inventoryScript.adrenalineMultiplier;
        statusScript.bulletResistance = inventoryScript.bulletResistance;
        statusScript.gasImmune = inventoryScript.gasImmune;

        if (movementScript.sprinting)
        {
            soundScript.speedMultiplier = movementScript.sprintMulti * movementScript.speedMulti;
        }
        else if (movementScript.sneaking)
        {
            soundScript.speedMultiplier = movementScript.sneakMulti * movementScript.speedMulti;
        }
        else
        {
            soundScript.speedMultiplier = movementScript.speedMulti;
        }

        // -- Miscellaneous values
        keycardClearance = inventoryScript.keycardClearance;

        // Set locally visible things
        if (!isLocalPlayer) { return; }

        // -- UI
        uiScript.UpdateStatus(new float[] { statusScript.health, statusScript.maxHealth, statusScript.stamina, statusScript.maxStamina, statusScript.blink, statusScript.maxBlink });

        if (uiScript.showInventory)
        {
            uiScript.UpdateInventory(inventoryScript.items);
        }

        // -- Camera
        headTransform.eulerAngles = new Vector3(Mathf.Clamp(cameraScript.rotation.x, -50, 60), cameraScript.rotation.y, cameraScript.rotation.z);
        transform.eulerAngles = new Vector3(0, cameraScript.rotation.y, 0);

        if (dead || uiScript.showInventory || uiScript.showMenu)
        {
            canLook = false;
        }
        else
        {
            canLook = true;
        }

        // -- Sound
        soundScript.gasMask = inventoryScript.gasImmune;

        // -- Inventory
        // ---- Worn item
        if (inventoryScript.wornItemIndex != -1)
        {          
            if (currentWornItem != inventoryScript.wornItemIndex)
            {
                currentWornItem = inventoryScript.wornItemIndex;
                itemWornCheck = false;
            }

            if (!itemWornCheck)
            {
                itemWornCheck = true;
                uiScript.SetWornItem(inventoryScript.wornSprite);

                switch (inventoryScript.items[inventoryScript.wornItemIndex].itemType)
                {
                    case Item.ItemType.GasMask: // gas mask
                        Cmd_MaskModel(true);
                        uiScript.MaskOverlay(1);
                        lowPassFilter.enabled = true;
                        StatusText("You put on the gas mask.");
                        break;

                    case Item.ItemType.BallisticVest: // ballistic vest
                        StatusText("You put on the ballistic vest.");
                        break;

                    default:
                        Cmd_MaskModel(false);
                        uiScript.MaskOverlay(0);
                        lowPassFilter.enabled = false;
                        break;
                }

                if (inventoryScript.currentWornSoundType != -1)
                {
                    soundScript.Cmd_ItemUse(inventoryScript.currentWornSoundType);
                }
            }
        }
        else
        {
            if (itemWornCheck)
            {
                itemWornCheck = false;
                uiScript.SetWornItem(null);

                if (lowPassFilter.enabled)
                {
                    StatusText("You take off the gas mask.");
                }

                Cmd_MaskModel(false);         
                uiScript.MaskOverlay(0);
                lowPassFilter.enabled = false;

                if (inventoryScript.currentWornSoundType != -1)
                {
                    soundScript.Cmd_ItemUse(inventoryScript.currentWornSoundType);
                }
            }
        }

        // ---- Held item
        if (inventoryScript.heldItemIndex != -1)
        {
            if (currentHeldItem != inventoryScript.heldItemIndex)
            {
                currentHeldItem = inventoryScript.heldItemIndex;
                itemHeldCheck = false;
            }

            if (!itemHeldCheck)
            {
                itemHeldCheck = true;               
                uiScript.SetHeldItem(inventoryScript.heldSprite);

                if (inventoryScript.currentHeldSoundType != -1)
                {
                    if (inventoryScript.items[inventoryScript.heldItemIndex].itemType == Item.ItemType.FirstAidKit ||
                        inventoryScript.items[inventoryScript.heldItemIndex].itemType == Item.ItemType.Syringe)
                    {
                        soundScript.Cmd_ItemUse(inventoryScript.currentHeldSoundType);
                    }
                }
            }      
            
            if (inventoryScript.items[inventoryScript.heldItemIndex].usableItem)
            {
                // -- First aid kit
                if (inventoryScript.items[inventoryScript.heldItemIndex].itemType == Item.ItemType.FirstAidKit)
                {
                    if (inventoryScript.items[inventoryScript.heldItemIndex].randomEffect != -1)
                    {
                        switch (inventoryScript.items[inventoryScript.heldItemIndex].randomEffect)
                        {
                            case 0: // increased speed -> death
                                Cmd_Adrenaline(3, 15.0f);
                                statusScript.dyingOfSpeed = true;
                                StatusText("'My body feels on fire, but I am filled with energy!'");
                                break;

                            case 1: // invert mouse
                                StatusText("'I feel dizzy...'");
                                break;

                            case 2: // blurry vision
                                statusScript.blinding = true;
                                StatusText("'Argh! My eyes... they burn...!'");
                                break;

                            case 3: // full heal
                                Cmd_Heal(1000);
                                StatusText("The first aid kit somehow cures all of your wounds, even the ones you haven't treated yet.");
                                break;

                            case 4: // critical wound
                                Cmd_TakeDamage(statusScript.health - 1, 1);
                                StatusText("'Augh! This shit burns my veins!'");
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        uiScript.StatusText("'Aah... Much better.'");
                        Cmd_Heal(inventoryScript.items[inventoryScript.heldItemIndex].healingAmount);
                    }

                    inventoryScript.Cmd_DestroyItem(inventoryScript.heldItemIndex);
                }
                // -- Syringe
                else if (inventoryScript.items[inventoryScript.heldItemIndex].itemType == Item.ItemType.Syringe)
                {
                    if (inventoryScript.items[inventoryScript.heldItemIndex].randomEffect != -1)
                    {
                        switch (inventoryScript.items[inventoryScript.heldItemIndex].randomEffect)
                        {
                            case 0: // increased speed -> death
                                Cmd_Adrenaline(3, 15.0f);
                                statusScript.dyingOfSpeed = true;
                                StatusText("'My legs are moving on their own!'");
                                break;

                            case 1: // quick stamina regen
                                StatusText("'I feel reinvigorated!'");
                                break;

                            case 2: // vomit
                                StatusText("'Ugh... I don't feel so good...'");
                                break;

                            case 3: // full heal
                                Cmd_Heal(1000);
                                StatusText("The syringe reinvigorates your body, making it like it was new.");
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        StatusText("'Fight or flight, fight or flight...'");
                        Cmd_Adrenaline(inventoryScript.items[inventoryScript.heldItemIndex].adrenalineMultiplier, inventoryScript.items[inventoryScript.heldItemIndex].adrenalineTimer);
                    }

                    inventoryScript.Cmd_DestroyItem(inventoryScript.heldItemIndex);
                }
            }
        }
        else
        {
            if (itemHeldCheck)
            {
                itemHeldCheck = false;
                uiScript.SetHeldItem(null);

                if (inventoryScript.currentHeldSoundType != -1)
                {
                    soundScript.Cmd_ItemUse(inventoryScript.currentHeldSoundType);
                }   
            }
        }

        if (adrenalineTimer > 0)
        {
            adrenalineCheck = false;
            highPassFilter.enabled = true;

            adrenalineTimer -= 1.0f * Time.deltaTime;
        }
        else
        {
            if (!adrenalineCheck)
            {
                adrenalineCheck = true;
                highPassFilter.enabled = false;

                inventoryScript.Adrenaline(1);
                movementScript.Adrenaline(1);
            }
        }

        // Crosshair
        int crosshairLayerMask = LayerMask.GetMask("Item");

        RaycastHit crosshairCast;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out crosshairCast, 3.0f, crosshairLayerMask))
        {
            if (crosshairCast.transform.CompareTag("Keypad"))
            {
                if (crosshairCast.transform.GetComponent<Keypad>().requiredClearance > keycardClearance)
                {
                    uiScript.UpdateCrosshair(3);
                }
                else
                {
                    if (inventoryScript.heldItemIndex != -1 && inventoryScript.items[inventoryScript.heldItemIndex].keycardClearance > 0)
                    {
                        uiScript.UpdateCrosshair(4);
                    }
                    else
                    {
                        uiScript.UpdateCrosshair(1);
                    }
                }
            }
            else if (crosshairCast.transform.CompareTag("Item"))
            {
                if (inventoryScript.inventoryFull)
                {
                    uiScript.UpdateCrosshair(3);
                }
                else
                {
                    uiScript.UpdateCrosshair(2);
                }
            }
        }
        else
        {
            uiScript.UpdateCrosshair(0);
        }

        // Post processing
        // -- Blur from blind/gas
        if (statusScript.blinding || statusScript.gassed)
        {
            if (statusScript.blinding)
            {
                float intensity = (1.0f - (statusScript.blindTimer / 120.0f));
                intensity = Mathf.Clamp(intensity, 0, 1.0f);

                focalLength = 64 * intensity;
            }
            else if (statusScript.gassed)
            {
                focalLength = Mathf.Lerp(focalLength, 64, 0.05f);
            }
        }
        else
        {
            focalLength = Mathf.Lerp(focalLength, 0, 0.002f);
        }

        if (postProcessVolume.profile.TryGetSettings<DepthOfField>(out var dof))
        {
            dof.focalLength.value = focalLength;
        }

        if (postProcessVolume.profile.TryGetSettings<Vignette>(out var vignette))
        {
            if (statusScript.blind)
            {
                vignette.intensity.value = (1.33f - (statusScript.blindTimer / 60.0f));
            }
            else
            {
                vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.33f, 0.004f);
            }
        }

        // Blinking
        if (statusScript.blinking)
        {
            uiScript.BlinkOverlay(1, 0.33f);
        }
        else
        {
            uiScript.BlinkOverlay(0, 0.33f);
        }

        // Input
        if (Input.GetButton("Sprint") && !statusScript.exhausted)
        {
            movementScript.sprinting = true;
            statusScript.sprinting = true;
        }
        else
        {
            movementScript.sprinting = false;
            statusScript.sprinting = false;
        }

        if (Input.GetButton("Sneak"))
        {
            movementScript.sneaking = true;
        }
        else
        {
            movementScript.sneaking = false;
        }

        if (Input.GetButtonDown("Blink"))
        {
            statusScript.blink = 0;
        }

        if (Input.GetButtonDown("Use"))
        {
            int layerMask = LayerMask.GetMask("Item");

            RaycastHit useCast;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out useCast, 3.0f, layerMask)) 
            {
                if (useCast.transform.CompareTag("Keypad"))
                {
                    useCast.transform.GetComponent<Keypad>().Cmd_Keypad(keycardClearance);

                    if (inventoryScript.heldItemIndex != -1)
                    {
                        if (keycardClearance >= useCast.transform.GetComponent<Keypad>().requiredClearance)
                        {
                            StatusText("You swipe your card across the keypad, and the door opens.");
                        }
                        else
                        {
                            StatusText(string.Format("You swipe your card across the keypad, but nothing happens.\nYou need a level {0} keycard.", useCast.transform.GetComponent<Keypad>().requiredClearance));
                        }

                        inventoryScript.Cmd_HoldItem(inventoryScript.heldItemIndex);
                    }
                    else if (keycardClearance >= useCast.transform.GetComponent<Keypad>().requiredClearance)
                    {
                        StatusText("You use the keypad, and the door opens.");
                    }
                    else
                    {
                        StatusText(string.Format("You use the keypad, but nothing happens.\nYou need a level {0} keycard.", useCast.transform.GetComponent<Keypad>().requiredClearance));
                    }
                }
                else if (useCast.transform.CompareTag("Item"))
                {
                    inventoryScript.Cmd_TakeItem(useCast.transform.gameObject, useCast.transform.GetComponent<ItemPickup>().itemId);
                    useCast.transform.GetComponent<ItemPickup>().Cmd_PickedUp();
                }
            }
        }

        if (Input.GetButtonDown("Inventory"))
        {
            uiScript.showInventory = !uiScript.showInventory;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiScript.showMenu = !uiScript.showMenu;
        }

        // Debug
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            soundScript.Cmd_Cough(Random.Range(0, 2));
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Cmd_TakeDamage(10.0f, 0);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Cmd_Heal(25.0f);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            cameraScript.ScreenShake(0.07f, 0.25f, 1.0f);
        }
    }

    public void InventoryToolTip(int index)
    {
        if (index != -1)
        {
            string editedText = string.Format("<b>{0}</b>:\n{1}", inventoryScript.items[index].name, inventoryScript.items[index].description);
            uiScript.InventoryToolTip(editedText);
        }
        else
        {
            uiScript.InventoryToolTip("Select an item.");
        }
    }

    public void StatusText(string text)
    {
        uiScript.StatusText(text);
    }

    #region CMD
    [Command]
    public void Cmd_MaskModel(bool show)
    {
        Rpc_MaskModel(show);
    }

    public void Cmd_TakeDamage(float damage, int type)
    {
        statusScript.TakeDamage(damage);
        soundScript.Cmd_TakeDamage(type);
    }

    [Command]
    public void Cmd_Heal(float heal)
    {
        statusScript.Heal(heal);
        soundScript.Cmd_Heal();
    }

    [Command]
    public void Cmd_Adrenaline(float multiplier, float timer)
    {
        adrenalineTimer = timer;

        inventoryScript.Adrenaline(multiplier);
        movementScript.Adrenaline(multiplier);
        soundScript.Cmd_Adrenaline();
    }
    #endregion

    #region RPC
    [ClientRpc]
    public void Rpc_MaskModel(bool show)
    {
        maskTransform.SetActive(show);
    }
    #endregion
}
