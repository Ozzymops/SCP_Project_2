using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MusicTrigger : NetworkBehaviour
{
    public AudioClip musicClip;
    public bool ignoreOverride;

    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerManager player = other.gameObject.GetComponent<PlayerManager>();
            
            if (!player.musicScript.canOverrideMusic || ignoreOverride)
            {
                if (player.musicScript.musicSource.clip != musicClip)
                {
                    player.musicScript.Crossfade(musicClip);
                }
            }
        }
    }
}
