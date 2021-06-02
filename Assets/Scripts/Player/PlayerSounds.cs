using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSounds : NetworkBehaviour
{
    public enum FootstepType { Normal, Metal, Corrosion, Forest }

    // -- References --
    [Header("References: footsteps")]
    public AudioSource footstepSource;
    public AudioClip[] footstep_normal;
    public AudioClip[] footstep_run;
    public AudioClip[] footstep_metal;
    public AudioClip[] footstep_metal_run;
    public AudioClip[] footstep_corrosion;
    public AudioClip[] footstep_forest;

    [Header("References: damage")]
    public AudioSource damageSource;
    public AudioClip[] damage_blunt;
    public AudioClip[] damage_sharp;
    public AudioClip damage_106;
    public AudioClip[] death_173;
    public AudioClip death_914;
    public AudioClip death_gib;
    public AudioClip death_generic;

    [Header("References: bodily sounds")]
    public AudioSource bleedSource;
    public AudioSource pantSource;
    public AudioSource coughSource;
    public AudioClip[] body_bleed;
    public AudioClip[] body_cough;
    public AudioClip body_pant_recover;
    public AudioClip body_pant_masked_recover;
    public AudioClip[] body_pant;
    public AudioClip[] body_pant_masked;

    [Header("References: items")]
    public AudioSource itemSource;
    public AudioClip[] item_use;
    public AudioClip item_firstaid;
    public AudioClip item_adrenaline;

    // -- Variables --
    [Header("Variables: footsteps")]
    public float speedMultiplier;
    public float maxFootstepVolume;
    public float minFootstepVolume;
    public float footstepInterval;
    private float footstepTimer;

    [Header("Variables: bodily sounds")]
    public float pantInterval;
    private float pantTimer;
    public float pantVolume;

    // -- Flags --
    [Header("Flags")]
    public bool moving;
    public bool sprinting;
    private bool sprintCheck;
    public bool sneaking;
    public bool gasMask;
    public FootstepType footstepType;

    // -- Functions --
    public void Update()
    {
        // Footstep timer
        if (moving)
        {
            if (footstepTimer > 0)
            {
                footstepTimer -= 1.0f * Time.deltaTime;
            }
            else
            {
                if (sprinting)
                {
                    footstepTimer = footstepInterval / speedMultiplier;
                    footstepSource.volume = maxFootstepVolume;
                }
                else if (sneaking)
                {
                    footstepTimer = footstepInterval / speedMultiplier;
                    footstepSource.volume = minFootstepVolume;
                }
                else
                {
                    footstepTimer = footstepInterval;
                    footstepSource.volume = maxFootstepVolume / 1.66f;
                }

                Footstep();
            }
        }

        // Footstep ground type
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position, Vector3.down, out groundHit, 1.1f))
        {
            if (groundHit.transform.CompareTag("FloorMetal"))
            {
                footstepType = FootstepType.Metal;
            }
            else if (groundHit.transform.CompareTag("FloorCorrosion"))
            {
                footstepType = FootstepType.Corrosion;
            }
            else if (groundHit.transform.CompareTag("FloorForest"))
            {
                footstepType = FootstepType.Forest;
            }
            else
            {
                footstepType = FootstepType.Normal;
            }
        }

        // Panting
        if (sprinting && moving)
        {
            sprintCheck = true;

            if (pantTimer > 0)
            {
                pantTimer -= 1.0f * Time.deltaTime;
            }
            else
            {
                pantTimer = pantInterval;
                Cmd_Pant(gasMask, Random.Range(0, body_pant.Length));
            }
        }

        if (!sprinting && sprintCheck)
        {
            sprintCheck = false;
            Cmd_PantRecover(gasMask);
        }

        // Cough interrupt
        if (coughSource.isPlaying)
        {
            pantSource.volume = 0;
        }
        else
        {
            pantSource.volume = 0.1f;
        }
    }

    public void Footstep()
    {
        int type = 0;
        int sound = 0;

        if (sprinting) // uneven
        {
            if (footstepType == FootstepType.Normal)
            {
                type = 1;
                sound = Random.Range(0, footstep_run.Length);
            }
            else if (footstepType == FootstepType.Metal)
            {
                type = 3;
                sound = Random.Range(0, footstep_metal_run.Length);
            }
            else if (footstepType == FootstepType.Corrosion)
            {
                type = 4;
                sound = Random.Range(0, footstep_corrosion.Length);
            }
            else if (footstepType == FootstepType.Forest)
            {
                type = 5;
                sound = Random.Range(0, footstep_forest.Length);
            }
        }
        else // even
        {
            if (footstepType == FootstepType.Normal)
            {
                type = 0;
                sound = Random.Range(0, footstep_normal.Length);
            }
            else if (footstepType == FootstepType.Metal)
            {
                type = 2;
                sound = Random.Range(0, footstep_metal.Length);
            }
            else if (footstepType == FootstepType.Corrosion)
            {
                type = 4;
                sound = Random.Range(0, footstep_corrosion.Length);
            }
            else if (footstepType == FootstepType.Forest)
            {
                type = 5;
                sound = Random.Range(0, footstep_forest.Length);
            }
        }

        Cmd_Footstep(type, sound);
    }

    #region CMD
    [Command]
    public void Cmd_Footstep(int type, int sound)
    {
        Rpc_Footstep(type, sound);
    }

    [Command]
    public void Cmd_PantRecover(bool masked)
    {
        Rpc_PantRecover(masked);
    }

    [Command]
    public void Cmd_Pant(bool masked, int sound)
    {
        Rpc_Pant(masked, sound);
    }

    [Command]
    public void Cmd_Drip(int sound)
    {
        Rpc_Drip(sound);
    }

    [Command(ignoreAuthority = true)]
    public void Cmd_Cough(int sound)
    {
        if (!coughSource.isPlaying)
        {
            Rpc_Cough(sound);
        }     
    }

    [Command]
    public void Cmd_ItemUse(int sound)
    {
        Rpc_ItemUse(sound);
    }

    [Command]
    public void Cmd_Adrenaline()
    {
        Rpc_Adrenaline();
    }

    [Command(ignoreAuthority = true)]
    public void Cmd_TakeDamage(int type)
    {
        int sound = -1;

        switch (type)
        {
            case 0:
                sound = Random.Range(0, damage_blunt.Length);
                break;

            case 1:
                sound = Random.Range(0, damage_sharp.Length);
                break;

            default:
                break;
        }


        if (type != -1)
        {
            Rpc_TakeDamage(type, sound);
        }       
    }

    [Command]
    public void Cmd_Heal()
    {
        Rpc_Heal();
    }
    #endregion

    #region RPC
    [ClientRpc]
    public void Rpc_Footstep(int type, int sound)
    {
        switch(type)
        {
            case 0: // normal
                footstepSource.clip = footstep_normal[sound];
                break;

            case 1: // normal run
                footstepSource.clip = footstep_run[sound];
                break;

            case 2: // metal
                footstepSource.clip = footstep_metal[sound];
                break;

            case 3: // metal run
                footstepSource.clip = footstep_metal_run[sound];
                break;

            case 4: // corrosion
                footstepSource.clip = footstep_corrosion[sound];
                break;

            case 5: // forest
                footstepSource.clip = footstep_forest[sound];
                break;
        }

        footstepSource.Play();
    }

    [ClientRpc]
    public void Rpc_PantRecover(bool masked)
    {
        if (masked)
        {
            pantSource.clip = body_pant_masked_recover;
        }
        else
        {
            pantSource.clip = body_pant_recover;
        }

        pantSource.Play();
    }

    [ClientRpc]
    public void Rpc_Pant(bool masked, int sound)
    {
        if (masked)
        {
            pantSource.clip = body_pant_masked[sound];
        }
        else
        {
            pantSource.clip = body_pant[sound];
        }

        pantSource.Play();
    }

    [ClientRpc]
    public void Rpc_Drip(int sound)
    {
        bleedSource.clip = body_bleed[sound];
        bleedSource.Play();
    }

    [ClientRpc]
    public void Rpc_Cough(int sound)
    {
        coughSource.clip = body_cough[sound];
        coughSource.Play();
    }

    [ClientRpc]
    public void Rpc_ItemUse(int sound)
    {
        itemSource.clip = item_use[sound];
        itemSource.Play();
    }

    [ClientRpc]
    public void Rpc_Adrenaline()
    {
        itemSource.clip = item_adrenaline;
        itemSource.Play();
    }

    [ClientRpc]
    public void Rpc_TakeDamage(int type, int sound)
    {
        switch (type)
        {
            case 0:
                damageSource.clip = damage_blunt[sound];
                break;

            case 1:
                damageSource.clip = damage_sharp[sound];
                break;

            case 2:
                damageSource.clip = damage_106;
                break;

            default:
                break;
        }

        damageSource.Play();
    }

    [ClientRpc]
    public void Rpc_Heal()
    {
        itemSource.clip = item_firstaid;
        itemSource.Play();
    }
    #endregion
}
