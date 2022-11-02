using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// changes scene depending on what button is pressed
public class ButtonManager : MonoBehaviour {
    // changes scene after "GoodChoice" button is pressed
    public void GoodChoice() {
        Debug.Log("goodchoicemade");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    // changes scene after "BadChoice" button is pressed
    public void BadChoice() {
        Debug.Log("badchoicemade");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }
}