using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerStatus : NetworkBehaviour
{
    // -- Variables --
    [Header("Variables: health")]
    [SyncVar]
    public float health = 100.0f;
    public float maxHealth = 100.0f;

    [Header("Variables: stamina")]
    [SyncVar]
    public float stamina = 100.0f;
    public float maxStamina = 100.0f;
    public float staminaMultiplier = 1.0f;
    private float previousStaminaMultiplier;

    [Header("Variables: blink")]
    [SyncVar]
    public float blink = 10.0f;
    public float maxBlink = 10.0f;
    public float blinkMultiplier = 1.0f;
    public float blinkHold = 0.33f;

    [Header("Variables: miscellaneous")]
    public float bulletResistance = 0.0f;

    [Header("Variables: status effects")]
    public float gasHealthDamage = 3.0f;
    public float gasBlinkDamage = 2.0f;

    public float speedDeathTimer = 15.0f;
    public float blindTimer = 120.0f;

    // -- Flags --
    [Header("Flags")]
    [SyncVar]
    public bool dead;

    [SyncVar]
    public bool blinking;

    [SyncVar]
    public bool blind;
    public bool blinding;

    public bool moving;
    public bool sprinting;
    public bool exhausted;
    public bool bleeding;
    public bool influencedBlink;
    public bool dyingOfSpeed;
    public bool gassed;

    [Header("Flags: immunities")]
    [SyncVar]
    public bool gasImmune;

    public bool mindImmune;
    public bool virusImmune;

    // -- Functions --
    public void Update()
    {
        if (!dead)
        {
            // Health
            if (health <= 0)
            {
                dead = true;
            }
            
            if (health >= maxHealth)
            {
                health = maxHealth;
            }

            // Stamina
            if (sprinting && moving)
            {
                stamina -= (10.0f / staminaMultiplier) * Time.deltaTime;

                if (stamina <= 0)
                {
                    exhausted = true;
                    stamina = 0;
                }
            }
            else
            {
                if (stamina >= maxStamina)
                {
                    stamina = maxStamina;
                }
                else
                {
                    if (exhausted)
                    {
                        stamina += (2.0f * staminaMultiplier) * Time.deltaTime;

                        if (stamina > (maxStamina/5)) // = 20%
                        {
                            exhausted = false;
                        }
                    }
                    else if (moving)
                    {
                        stamina += (3.5f * staminaMultiplier) * Time.deltaTime;
                    }
                    else
                    {
                        stamina += (5.0f * staminaMultiplier) * Time.deltaTime;
                    }
                }
            }

            // Blink
            if (!blinking)
            {
                blink -= (1.0f / blinkMultiplier) * Time.deltaTime;
                blinkHold = 0.33f;

                if (blink <= 0)
                {
                    blink = 0;
                    blinking = true;
                }
            }
            else
            {
                blinkHold -= 1.0f * Time.deltaTime;

                if (blinkHold <= 0)
                {
                    blinkHold = 0;
                    blinking = false;
                    blink = maxBlink;
                }
            }

            // Status effects
            if (blinding)
            {
                blindTimer -= 1.0f * Time.deltaTime;

                if (blindTimer < 60.0f)
                {
                    blind = true;
                }

                if (blindTimer < 0)
                {
                    blind = false;
                    blinding = false;
                    blindTimer = 120.0f;
                }
            }

            if (dyingOfSpeed)
            {
                speedDeathTimer -= 1.0f * Time.deltaTime;

                if (speedDeathTimer < 0)
                {
                    speedDeathTimer = 0;
                    TakeDamage(1000);
                }
            }
        }
        else
        {
            if (health > 0)
            {
                dead = false;
            }

            stamina = 0;
            blink = 0;

            sprinting = false;
            exhausted = false;
            bleeding = false;
        }

        if (!isLocalPlayer) { return; }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    public void TakeBulletDamage(float damage)
    {
        health -= (damage * bulletResistance);
    }

    public void Heal(float heal)
    {
        health += heal;
    }
}
