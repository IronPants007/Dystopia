using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;



/**
 * GameScene is NOT like a Unity Scene.
 * A GameScene stores a background, relevant characters, and a dialogue tree.
 */
public class GameScene {
    static string prog = null;
    static bool hasLoaded = false;
    public static Dictionary<string, GameScene> scenes = new Dictionary<string, GameScene>(); // Stores all of our scenes!

    private readonly Dictionary<string, GameObject> characters;
    private readonly Sprite background;
    public readonly DialogueTree dialogueTree;


    // Default stuff
    public static void loadData() {
        if (!hasLoaded) {
            prog = System.IO.File.ReadAllText(@"Assets\Scripts\GameData.txt");
            hasLoaded = true;
            Debug.Log("Loading file");
            scenes = new GameSceneParser(prog).scenes;
        }
    }

    public GameScene(Sprite background, string[] chars, DialogueTree dialogueTree) {
        characters = new Dictionary<string, GameObject>();
        foreach (string s in chars) {
            characters[s] = null;
        }
        this.background = background;
        this.dialogueTree = dialogueTree;
    }

    // This will load up a scene
    public void LoadScene() {
        // If we're not in a game already, put us in one.
        if (SceneManager.GetActiveScene().name != "Game") {
            SceneManager.LoadScene("Game"); // It would be unsafe to continue running the code, as not everything has been loaded in yet.
            SceneManager.sceneLoaded += LoadSceneDelegate; // Run "LoadSceneDelegate" once the scene is done loading.
            return; // Stop running.
        }

        // Load backdrop
        var backdrop = GameObject.FindGameObjectWithTag("Backdrop");
        backdrop.GetComponent<SpriteRenderer>().sprite = this.background;

        // Hide all characters
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Character")) {
            var name = o.GetComponent<Character>().charName;
            if (characters.ContainsKey(name)) { // Load all characters that need to be here
                characters[name] = o;
                o.SetActive(true);
            } else { // And unload any that don't
                o.SetActive(false);
            }
        }

        Debug.Log("Loaded Scene");
        dialogueTree.Begin();

    }

    public void LoadSceneDelegate(Scene scene, LoadSceneMode mode) {
        if (scene.name != "Game") {
            throw new System.Exception("Failed to load Game"); // This is to an infinite loop with LoadSceneDelegate and LoadScene if the scene fails to load or smth.
        }
        LoadScene(); // Literally just retry
    }

}
