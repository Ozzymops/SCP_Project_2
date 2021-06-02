using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Keypad : NetworkBehaviour
{
    public Door linkedDoor;
    public Keypad[] linkedKeypads;

    public AudioSource audioSource;
    public AudioClip[] audioClips;

    [SyncVar]
    public int requiredClearance;

    private float useCooldown = 2f;
    public bool canUse = true;
    private bool forcing;

    public override void OnStartServer()
    {
        base.OnStartServer();

        //audioSource = GetComponent<AudioSource>();
    }

    public void Update()
    {
        if (!isServer) { return; }

        if (!canUse)
        {
            useCooldown -= 1.0f * Time.deltaTime;

            if (useCooldown <= 0.0f)
            {
                canUse = true;
                useCooldown = 2f;
            }
        }
    }

    [Command(ignoreAuthority = true)]
    public void Cmd_Keypad(int keycardClearance)
    {
        if (canUse)
        {
            if (keycardClearance >= requiredClearance)
            {
                if (keycardClearance == 79)
                {
                    forcing = true;
                }
                else
                {
                    forcing = false;
                }

                linkedDoor.ToggleDoor(forcing);

                canUse = false;

                if (linkedKeypads.Length != 0)
                {
                    for (int i = 0; i < linkedKeypads.Length; i++)
                    {
                        linkedKeypads[i].canUse = false;
                    }
                }

                Rpc_PlaySound(0);
            }
            else
            {
                Rpc_PlaySound(1);
            }
        }      
    }

    [ClientRpc(excludeOwner = true)]
    public void Rpc_PlaySound(int sound)
    {
        audioSource.clip = audioClips[sound];
        audioSource.Play();
    }
}
