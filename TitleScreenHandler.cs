using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TitleScreenHandler : MonoBehaviour
{
    public void Start() {
        GameScene.loadData();
    }
    public void StartGameButton() {
        if (GameScene.scenes.ContainsKey("main") && GameScene.scenes["main"] != null) {
            GameScene.scenes["main"].LoadScene();
        } else {
            Debug.Log("No main found");
        }

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
