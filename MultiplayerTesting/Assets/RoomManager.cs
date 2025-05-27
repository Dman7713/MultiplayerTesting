using System.Collections.Generic;
using UnityEngine;

public class RoomsBatchSpawner : MonoBehaviour
{
    public GameObject[] roomPrefabs;
    public Transform startingSpawnPoint;
    public int roomsToSpawn = 3;
    public bool reuseRooms = false;       // Allows reusing rooms if roomsToSpawn > prefab count
    public bool randomizeRooms = false;   // Randomize rooms order (no immediate repeats)

    private Transform lastEndPoint;
    private List<GameObject> spawnedRooms = new();

    [ContextMenu("Spawn Rooms")]
    void SpawnRooms()
    {
        ClearOldRooms();

        if (roomPrefabs.Length == 0 || startingSpawnPoint == null)
        {
            Debug.LogError("Assign roomPrefabs and startingSpawnPoint in inspector.");
            return;
        }

        int spawnCount = roomsToSpawn;

        if (!reuseRooms && spawnCount > roomPrefabs.Length)
        {
            Debug.LogWarning("roomsToSpawn is greater than prefab count and reuseRooms is OFF. Clamping spawn count.");
            spawnCount = roomPrefabs.Length;
        }

        lastEndPoint = startingSpawnPoint;

        int lastPrefabIndex = -1;

        for (int i = 0; i < spawnCount; i++)
        {
            int prefabIndex;

            if (randomizeRooms)
            {
                // Pick a random prefab index different from lastPrefabIndex
                do
                {
                    prefabIndex = Random.Range(0, roomPrefabs.Length);
                }
                while (prefabIndex == lastPrefabIndex && roomPrefabs.Length > 1);
            }
            else
            {
                prefabIndex = reuseRooms ? i % roomPrefabs.Length : i;
            }

            lastPrefabIndex = prefabIndex;

            GameObject newRoom = Instantiate(roomPrefabs[prefabIndex]);
            spawnedRooms.Add(newRoom);

            Transform startPoint = newRoom.transform.Find("StartPoint");
            Transform endPoint = newRoom.transform.Find("EndPoint");

            if (startPoint == null || endPoint == null)
            {
                Debug.LogWarning("Room prefab missing StartPoint or EndPoint. Cannot align properly.");
                Destroy(newRoom);
                break;
            }

            Vector3 offset = lastEndPoint.position - startPoint.position;
            newRoom.transform.position += offset;

            lastEndPoint = endPoint;
        }
    }

    void ClearOldRooms()
    {
        foreach (GameObject room in spawnedRooms)
        {
            if (room != null)
                DestroyImmediate(room);
        }
        spawnedRooms.Clear();
    }
}
