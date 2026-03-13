using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject selectLevelGroup;
    public AudioSource audioSource;
    

    private bool levelSelectShown = false;
    public TMP_Text[] levelrecords;
    public AudioClip clicksound;

    public void onClick_LevelButton(int scenelevel)
    {
        SceneManager.LoadScene(scenelevel);
        audioSource.PlayOneShot(clicksound);
    }
    private void Start()
    {
        selectLevelGroup.SetActive(false);
        Cursor.lockState = CursorLockMode.None
            ;
        Cursor.visible = true;
        for(int i = 0; i < levelrecords.Length;i++)
        {
            float record = PlayerPrefs.GetFloat(i + "_record", -1);
            if(record >= 0)
            {
                levelrecords[i].text = $"{record:00.##}s";
            } else
            {
                levelrecords[i].text = $"-";
            }
        }

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
