using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerCamera : NetworkBehaviour
{
    // -- References --
    [Header("References")]
    public Transform cameraTransform;

    // -- Variables --
    [Header("Variables")]
    public float sensitivity;
    public Vector3 rotation;
    public Vector3 offset;

    public float screenShakeIntensity;
    public float screenShakeTime;
    public float screenShakeDrain;

    // -- Flags --
    [Header("Flags")]
    public bool dead;
    public bool canLook;

    public bool screenShaking;

    // -- Functions --
    public void Update()
    {
        if (!isLocalPlayer) { return; }

        if (canLook)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

            rotation.y += mouseX;
            rotation.x -= mouseY;
            rotation.x = Mathf.Clamp(rotation.x, -90, 90);
        }

        if (dead)
        {
            canLook = false;

            rotation.x = Mathf.Lerp(rotation.x, -90, 0.02f);
            rotation.z = Mathf.Lerp(rotation.z, -10, 0.02f);

            offset.y = Mathf.Lerp(offset.y, 0.1f, 0.02f);           
        }
        else
        {
            rotation.z = 0;
            offset.y = 1.1f;

            if (screenShaking)
            {
                screenShakeTime -= screenShakeDrain * Time.deltaTime;

                offset += Random.insideUnitSphere * screenShakeIntensity;

                if (screenShakeTime < 0)
                {
                    screenShaking = false;
                }
            }
            else
            {
                offset.x = Mathf.Lerp(offset.x, 0, 0.05f);
                offset.y = Mathf.Lerp(offset.y, 1.1f, 0.05f);
                offset.z = Mathf.Lerp(offset.z, 0, 0.05f);
            }
        }

        cameraTransform.eulerAngles = rotation;
        cameraTransform.localPosition = new Vector3(offset.x, offset.y, offset.z);
    }

    public void ScreenShake(float intensity, float time, float drain)
    {
        screenShaking = true;
        screenShakeIntensity = intensity;
        screenShakeTime = time;
        screenShakeDrain = drain;
    }
}
