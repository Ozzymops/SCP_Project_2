using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemSpawnPoint : NetworkBehaviour
{
    [SyncVar]
    public int itemId;

    [SyncVar]
    public bool spawned;
}
