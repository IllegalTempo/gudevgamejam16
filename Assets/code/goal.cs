using UnityEngine;

public class goal : MonoBehaviour,IResetable
{
    int goalplayer = 0;
    private PlayerMovement alreadyenteredplayer;
    public void onReset()
    {
        goalplayer = 0;
        alreadyenteredplayer = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerMovement>() != alreadyenteredplayer)
        {
            goalplayer++;
            alreadyenteredplayer = other.GetComponent<PlayerMovement>();
            if (goalplayer >= 2)
            {
                SceneSystem sceneSystem = FindFirstObjectByType<SceneSystem>();
                sceneSystem.FinishScene();
            } else
            {
                GameCore.Instance.displayStatusText("One timeline has reached the goal, the other timeline also needs to reach");
            }
        }
    }
}
