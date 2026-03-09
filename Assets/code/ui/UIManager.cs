using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject selectLevelGroup;

    private bool levelSelectShown = false;

    public void onClick_LevelButton(int scenelevel)
    {
        SceneManager.LoadScene(scenelevel);
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
