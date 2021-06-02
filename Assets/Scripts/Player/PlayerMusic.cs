using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMusic : NetworkBehaviour
{
    // -- References --
    [Header("References: audio sources")]
    public AudioSource musicSource;
    public AudioSource fadeSource;
    public AudioSource horrorSource;

    [Header("References: audio clips")]
    public AudioClip previousClip;

    public AudioClip[] horrorClips;

    public AudioClip scp049_nearby;
    public AudioClip scp049_danger;
    public AudioClip scp049_chase;
    public AudioClip scp049_chase_target;

    public AudioClip scp096_chase;
    public AudioClip scp096_chase_target;

    public AudioClip scp106_chase;
    public AudioClip scp106_chase_target;

    // -- Variables --
    private AudioClip musicClip;
    private float musicVolume;
    private float musicTime;

    [Header("Variables: timers")]
    private float musicCooldown = 0.5f;
    public float horrorCooldown;

    // -- Flags --
    [Header("Flags")]
    private bool musicCoolingDown;
    public bool dead;
    public bool canOverrideMusic;
    public bool canPlayHorror;

    // -- Functions --
    public void Update()
    {
        if (!isLocalPlayer) { return; }

        if (!dead)
        {
            musicSource.volume = Mathf.Lerp(musicSource.volume, 0.1f, 0.005f);
            fadeSource.volume = Mathf.Lerp(fadeSource.volume, 0.0f, 0.005f);
        }
        else
        {
            canOverrideMusic = false;

            musicSource.volume = Mathf.Lerp(musicSource.volume, 0.0f, 0.005f);
            fadeSource.volume = musicVolume;
        }

        if (musicCooldown > 0)
        {
            musicCoolingDown = true;
            musicCooldown -= 1.0f * Time.deltaTime;
        }
        else
        {
            musicCoolingDown = false;
        }

        // Private
        musicClip = musicSource.clip;
        musicVolume = musicSource.volume;
        musicTime = musicSource.time;
    }

    public void Crossfade(AudioClip newClip)
    {
        if (!isLocalPlayer) { return; }

        if (!dead && !musicCoolingDown)
        {
            if (canOverrideMusic && !previousClip)
            {
                previousClip = musicSource.clip;
            }

            if (musicSource.clip != newClip)
            {
                musicCoolingDown = true;
                musicCooldown = 0.5f;

                // Music source -> fade source
                fadeSource.clip = musicClip;
                fadeSource.volume = musicVolume;
                fadeSource.time = musicTime;

                // Reset music source
                musicSource.clip = newClip;
                musicSource.volume = 0.00f;
                musicSource.time = 0.00f;

                musicSource.Play();
                fadeSource.Play();
            }
        }
    }

    [Command(ignoreAuthority = true)]
    public void Cmd_ResumePreviousClip()
    {
        canOverrideMusic = false;

        if (previousClip)
        {
            Crossfade(previousClip);
        }
    }

    // Specific music
    [Command(ignoreAuthority = true)]
    public void Cmd_Play_NearSCP049()
    {
        canOverrideMusic = true;

        Crossfade(scp049_nearby);
    }

    [Command(ignoreAuthority = true)]
    public void Cmd_Play_DangerSCP049()
    {
        canOverrideMusic = true;

        Crossfade(scp049_danger);
    }

    [Command(ignoreAuthority = true)]
    public void Cmd_Play_ChaseSCP049()
    {
        canOverrideMusic = true;

        Crossfade(scp049_chase);
    }

    [Command(ignoreAuthority = true)]
    public void Cmd_Play_ChaseTargetSCP049()
    {
        canOverrideMusic = true;

        Crossfade(scp049_chase_target);
    }
}
