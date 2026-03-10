using System.Linq;
using UnityEngine;

public class SceneSystem : MonoBehaviour
{
    public GameObject playerSpawnpoint;
    public GameObject cloneSpawnpoint;
    public GameObject GameCorePrefab;
    private IResetable[] resetables;
    public void Start()
    {
        Instantiate(GameCorePrefab);
        SpawnPlayer(false, playerSpawnpoint.transform);
        SpawnPlayer(true, cloneSpawnpoint.transform);
        resetables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IResetable>().ToArray();
    }
    public void SpawnPlayer(bool clone, Transform spawn)
    {

        PlayerMovement player = Instantiate(GameCore.Instance.playerPrefab, spawn.transform.position, spawn.transform.rotation).GetComponent<PlayerMovement>();
        player.Init(clone);
        
    }
    public void ResetScene()
    {
        foreach (IResetable resetable in resetables)
        {
            resetable.onReset();
        }
    }

}
