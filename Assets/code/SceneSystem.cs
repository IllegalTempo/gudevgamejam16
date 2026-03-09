using UnityEngine;

public class SceneSystem : MonoBehaviour
{
    public GameObject playerSpawnpoint;
    public GameObject cloneSpawnpoint;
    public void Start()
    {
        SpawnPlayer(false, playerSpawnpoint.transform);
        SpawnPlayer(true, cloneSpawnpoint.transform);
    }
    public void SpawnPlayer(bool clone, Transform spawn)
    {

        PlayerMovement player = Instantiate(GameCore.Instance.playerPrefab, spawn.transform.position, spawn.transform.rotation).GetComponent<PlayerMovement>();
        player.Init(clone);
        
    }

}
