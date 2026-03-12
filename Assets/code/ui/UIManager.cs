using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject selectLevelGroup;
    public AudioSource audioSource;
    

    private bool levelSelectShown = false;

    public void onClick_LevelButton(int scenelevel)
    {
        SceneManager.LoadScene(scenelevel);
        audioSource.PlayOneShot(GameCore.Instance.clicksound);
    }
    private void Start()
    {
        selectLevelGroup.SetActive(false);
    }
    public void SelectLevel()
    {
        selectLevelGroup.SetActive(true); 
    }
    void Update()
    {
        if (!levelSelectShown && Input.anyKeyDown)
        {
            SelectLevel();
            levelSelectShown = true;
        }
    }
}
