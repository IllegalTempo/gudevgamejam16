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
        Instantiate(GameCorePrefab);
        SpawnPlayer(false, playerSpawnpoint.transform);
        SpawnPlayer(true, cloneSpawnpoint.transform);
        resetables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IResetable>().ToArray();
        GameCore.Instance.endscreengroup.SetActive(false);
    }
    public void SpawnPlayer(bool clone, Transform spawn)
    {

        PlayerMovement player = Instantiate(GameCore.Instance.playerPrefab, spawn.transform.position, spawn.transform.rotation).GetComponent<PlayerMovement>();
        player.Init(clone);
        
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
        //get current scene index
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        float currentrecord = PlayerPrefs.GetFloat(currentSceneIndex + "_record",999999);
        if(levelTimer < currentrecord)
        {
            PlayerPrefs.SetFloat(currentSceneIndex + "_record", levelTimer);
        }


            

    }
    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void ResetScene()
    {
        levelTimer = 0;
        foreach (IResetable resetable in resetables)
        {
            resetable.onReset();
        }

    }

}
