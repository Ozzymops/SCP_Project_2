using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemSpawner : NetworkBehaviour
{
    public ItemDatabase itemDatabase;
    public List<GameObject> itemPrefabs;

    public override void OnStartClient()
    {
        base.OnStartClient();

        SpawnItems();
    }

    public void SpawnItems()
    {
        // spawn items on spawn points
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("ItemSpawn");

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            //if (!spawnPoints[i].GetComponent<ItemSpawnPoint>().spawned)
            //{
                spawnPoints[i].GetComponent<ItemSpawnPoint>().spawned = true;
                Rpc_SpawnItem(spawnPoints[i].GetComponent<ItemSpawnPoint>().itemId, spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
                Debug.Log("Spawned object with id " + spawnPoints[i].GetComponent<ItemSpawnPoint>().itemId + " at " + spawnPoints[i].transform.position + ".");
            //}         
        }
    }

    [ClientRpc]
    public void Rpc_SpawnItem(int id, Vector3 position, Quaternion rotation)
    {
        GameObject spawnedItem = Instantiate(itemPrefabs[id], position, rotation);
        spawnedItem.GetComponent<ItemPickup>().item = itemDatabase.GetItem(id);
        spawnedItem.GetComponent<ItemPickup>().itemId = spawnedItem.GetComponent<ItemPickup>().item.id;
        spawnedItem.GetComponent<ItemPickup>().shouldExist = true;

        NetworkServer.Spawn(spawnedItem);
    }
}
