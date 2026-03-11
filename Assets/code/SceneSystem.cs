using System.Linq;
using UnityEngine;

public class SceneSystem : MonoBehaviour
{
    public GameObject playerSpawnpoint;
    public GameObject cloneSpawnpoint;
    public GameObject GameCorePrefab;
    private IResetable[] resetables;
    public float levelTimer = 0;

    public void Start()
    {
        Debug.Log($"[SceneSystem] Start running. playerSpawn={playerSpawnpoint.transform.position}, cloneSpawn={cloneSpawnpoint.transform.position}");
        Instantiate(GameCorePrefab);
        SpawnPlayer(false, playerSpawnpoint.transform);
        SpawnPlayer(true, cloneSpawnpoint.transform);
        resetables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IResetable>().ToArray();
        Debug.Log($"[SceneSystem] Found {resetables.Length} IResetable objects");
        if (GameCore.Instance != null && GameCore.Instance.endscreengroup != null)
            GameCore.Instance.endscreengroup.SetActive(false);
    }
    public void SpawnPlayer(bool clone, Transform spawn)
    {

        Debug.Log($"[SceneSystem] Spawning player clone={clone} at {spawn.position}");
        PlayerMovement player = Instantiate(GameCore.Instance.playerPrefab, spawn.transform.position, spawn.transform.rotation).GetComponent<PlayerMovement>();
        player.Init(clone);
        Debug.Log($"[SceneSystem] Spawned player instance {player.name} at {player.transform.position}");
        
    }
    private void Update()
    {
        levelTimer += Time.deltaTime;
        GameCore.Instance.timerText.text = $"{levelTimer:00.##}";
        if(Input.GetKeyDown(KeyCode.Return))
        {
            ResetScene();
        }
    }
    public void FinishScene()
    {
        GameCore.Instance.endscreengroup.SetActive(true);
        GameCore.Instance.endScreenRecord.text = $"Clock Elapsed: {levelTimer:00.##} seconds";
        //go back to main screen after 5 seconds
        Invoke("GoToMainMenu", 5f);

    }
    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void ResetScene()
    {
        levelTimer = 0;
        Debug.Log("[SceneSystem] ResetScene called\n" + new System.Diagnostics.StackTrace(true).ToString());
        foreach (IResetable resetable in resetables)
        {
            resetable.onReset();
        }
    }

}
