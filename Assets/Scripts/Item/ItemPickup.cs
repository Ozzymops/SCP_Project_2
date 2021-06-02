using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemPickup : NetworkBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioClip;

    public Item item;
    public int itemId;
    public int soundType;

    [SyncVar]
    public bool shouldExist = true;

    public void Update()
    {
        if (shouldExist)
        {
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
            GetComponent<Rigidbody>().isKinematic = false;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    [Command(ignoreAuthority = true)]
    public void Cmd_PickedUp()
    {
        Rpc_PickedUp();
    }

    [ClientRpc]
    public void Rpc_PickedUp()
    {
        shouldExist = false;

        audioSource.clip = audioClip[soundType];
        audioSource.Play();
    }

    [Command(ignoreAuthority = true)]
    public void Cmd_Dropped(Vector3 position)
    {
        Rpc_Dropped(position);
    }

    [ClientRpc]
    public void Rpc_Dropped(Vector3 position)
    {
        transform.position = position;
        shouldExist = true;
    }

    [Command(ignoreAuthority = true)]
    public void Cmd_Destroy()
    {
        Rpc_Destroy();
    }

    [ClientRpc]
    public void Rpc_Destroy()
    {
        Destroy(this.gameObject);
    }
}
