using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Door : NetworkBehaviour
{
    public enum TransformAxis { X, Z }

    public AudioSource doorSource;
    public AudioClip[] door_opening;
    public AudioClip[] door_closing;
    public AudioSource forceSource;

    public Transform doorTransform;
    public TransformAxis transformAxis;

    [SyncVar]
    public float speed;

    public bool open;
    public bool forcing;

    public void ToggleDoor(bool forcing)
    {
        if (open)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    public void OpenDoor()
    {
        StopAllCoroutines();
        StartCoroutine("OpeningDoor");

        open = true;

        if (!forcing)
        {
            Rpc_DoorSound(0, Random.Range(0, 3));
        }
        else
        {
            Rpc_DoorSound(0, 3);
        }
    }

    public void CloseDoor()
    {
        StopAllCoroutines();
        StartCoroutine("ClosingDoor");

        open = false;

        if (!forcing)
        {
            Rpc_DoorSound(0, Random.Range(0, 3));
        }
        else
        {
            Rpc_DoorSound(0, 3);
        }
    }

    public IEnumerator OpeningDoor()
    {
        if (transformAxis == TransformAxis.X)
        {
            while (doorTransform.localPosition.x > -1.98f)
            {
                doorTransform.localPosition += (new Vector3(1, 0, 0) * (-speed * Time.deltaTime));
                yield return null;
            }
        }
        else // Z
        {
            while (doorTransform.localPosition.z > -1.98f)
            {
                doorTransform.localPosition += (new Vector3(0, 0, 1) * (-speed * Time.deltaTime));
                yield return null;
            }
        }
    }

    public IEnumerator ClosingDoor()
    {
        if (transformAxis == TransformAxis.X)
        {
            while (doorTransform.localPosition.x < 0.00f)
            {
                doorTransform.localPosition += (new Vector3(1, 0, 0) * (speed * Time.deltaTime));
                yield return null;
            }
        }
        else // Z
        {
            while (doorTransform.localPosition.z < 0.00f)
            {
                doorTransform.localPosition += (new Vector3(0, 0, 1) * (speed * Time.deltaTime));
                yield return null;
            }
        }
    }

    [ClientRpc(excludeOwner = true)]
    public void Rpc_DoorSound(int type, int sound)
    {
        if (type == 0)
        {
            doorSource.clip = door_opening[sound];
        }
        else
        {
            doorSource.clip = door_closing[sound];
        }

        doorSource.Play();
    }
}
