using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    // -- References --
    [Header("References")]
    public CharacterController controller;

    // -- Variables --
    [Header("Variables")]
    public float speed;
    public float baseSpeed;
    public float speedMulti;
    public float adrenalineMulti = 1.0f;
    public float sprintMulti;
    public float sneakMulti;

    private float velocity;
    private float gravity = 0.098f;
    private float horizontal;
    private float vertical;

    // -- Flags --
    [Header("Flags")]
    public bool dead;
    public bool canMove;

    public bool moving;
    public bool sprinting;
    public bool sneaking;

    public bool grounded;
    public bool jumping;

    // -- Functions --
    public void FixedUpdate()
    {
        // Gravity
        if (!grounded)
        {
            controller.Move(new Vector3(0, velocity, 0));
        }

        if (!isLocalPlayer) { return; }

        // Movement
        if (!dead && canMove)
        {
            controller.Move((controller.transform.right * horizontal + controller.transform.forward * vertical) * Time.fixedDeltaTime);
        }
        else if (dead && canMove)
        {
            // spectator fly
        }
    }

    public void Update()
    {
        // Gravity
        if (grounded)
        {
            velocity = 0;
        }
        else
        {
            velocity -= gravity * Time.deltaTime;
        }

        // Grounded
        RaycastHit groundedHit;
        if (Physics.Raycast(transform.position, Vector3.down, out groundedHit, 1.1f))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        // Speed
        if (sprinting && !sneaking)
        {
            speed = (baseSpeed * sprintMulti) * speedMulti * adrenalineMulti;
        }
        else if (!sprinting && sneaking)
        {
            speed = (baseSpeed * sneakMulti) * speedMulti * adrenalineMulti;
        }
        else
        {
            speed = (baseSpeed * speedMulti) * adrenalineMulti;
        }

        // Movement check
        if (horizontal != 0 || vertical != 0)
        {
            moving = true;
        }
        else
        {
            moving = false;
        }

        if (!isLocalPlayer) { return; }

        // Movement
        if (canMove)
        {
            horizontal = Input.GetAxis("Horizontal") * speed;
            vertical = Input.GetAxis("Vertical") * speed;
        }
        else
        {
            horizontal = 0;
            vertical = 0;
        }
    }

    public void Adrenaline(float multiplier)
    {
        adrenalineMulti = multiplier;     
    }
}
