using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    public void StartButton()
    {
        GameManager.Instance.StartNewRun();
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void MainMenuButton()
    {
        GameManager.Instance.ChangeScene(GameScenes.MainMenuScene);
    }
}
