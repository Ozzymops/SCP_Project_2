using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GasLeak : NetworkBehaviour
{
    public float damageTicker = 0.5f;
    public bool canDamage;

    public void Update()
    {
        if (!isServer) { return; }

        if (!canDamage)
        {
            damageTicker -= 1.0f * Time.deltaTime;

            if (damageTicker <= 0)
            {
                canDamage = true;
                damageTicker = 0.5f;
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerManager player = other.gameObject.GetComponent<PlayerManager>();

            if (!player.statusScript.gasImmune)
            {
                if (canDamage)
                {
                    canDamage = false;
                    other.gameObject.GetComponent<PlayerManager>().Cmd_TakeDamage(other.gameObject.GetComponent<PlayerManager>().statusScript.gasHealthDamage, -1);
                    other.gameObject.GetComponent<PlayerManager>().soundScript.Cmd_Cough(Random.Range(0, 2));
                    other.gameObject.GetComponent<PlayerManager>().StatusText("*ack* *cough* 'I'm choking!'");
                }
                
                other.gameObject.GetComponent<PlayerManager>().statusScript.gassed = true;

                if (!other.gameObject.GetComponent<PlayerManager>().statusScript.influencedBlink)
                {
                    other.gameObject.GetComponent<PlayerManager>().statusScript.blinkMultiplier = 0.5f;
                }
            }
            else
            {
                other.gameObject.GetComponent<PlayerManager>().statusScript.gassed = false;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerManager>().statusScript.gassed = false;

            if (!other.gameObject.GetComponent<PlayerManager>().statusScript.influencedBlink)
            {
                other.gameObject.GetComponent<PlayerManager>().statusScript.blinkMultiplier = 1.0f;
            }           
        }
    }
}
