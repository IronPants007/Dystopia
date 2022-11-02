using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TitleScreenHandler : MonoBehaviour
{

    public void StartGameButton() {
        Debug.Log("test");
        GameScene.scenes["main"].LoadScene();

    }
    public void QuitGameButton() {
        Debug.Log("test2!");
        Application.Quit();

    }
    public void SettingsButton() {
        Debug.Log("test3!");
    }
    public void SettingsPlaceholderButton() {
        Debug.Log("test4!");
    }
}
