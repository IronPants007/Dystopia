using UnityEngine;
using TMPro;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;
using System;
using Unity.VisualScripting;

// This represents a "node" in the tree.
public abstract class DialogueTree {
    public abstract void Next(); // Called to progress to the next state
    public abstract void Begin(); // Called to switch to this state
}


// TODO This node will make a character say a message.
public class DialogueMessage : DialogueTree {
    public string msg;
    public string charName;
    DialogueTree next;
    /* Characters, Expressions */

    public DialogueMessage(string msg, string cn, DialogueTree txt) {
        this.msg = msg;
        this.next = txt;
        this.charName = cn;
    }
    public override void Next() {
        GameController.MouseClicked -= OnClick;

        var dialogueBox = GameObject.FindGameObjectWithTag("DialogueBox");
        dialogueBox.GetComponent<TextMeshProUGUI>().enabled = false;


        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Character")) {
            var name = o.GetComponent<Character>().charName;
            if (name == charName) {
                o.transform.SetPositionAndRotation(
                    new Vector3(o.transform.position.x, o.transform.position.y - 5, o.transform.position.z),
                    Quaternion.identity
                );
                break;
            }
        }


        next.Begin();
    }


    public override void Begin() {
        var dialogueBox = GameObject.FindGameObjectWithTag("DialogueBox");
        dialogueBox.GetComponent<TextMeshProUGUI>().enabled = true;
        dialogueBox.GetComponent<TextMeshProUGUI>().text = msg;

        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Character")) {
            var name = o.GetComponent<Character>().charName;
            if (name == charName) {
                o.transform.SetPositionAndRotation(
                    new Vector3(o.transform.position.x, o.transform.position.y + 5, o.transform.position.z), 
                    Quaternion.identity
                );
                break;
            }
        }



        GameController.MouseClicked += OnClick; // Call OnClick once the user clicks (Which just advances)
    }

    private void OnClick() {
        Next();
    }
}


    // TODO This node will give the user a choice and follow the relevant branch
public class DialogueChoice : DialogueTree {
    Dictionary<string, DialogueTree> next; // Match a string to its relevant tree branch.
    string currentSelection; // The current selection we're looking at. 

    public DialogueChoice(Dictionary<string, DialogueTree> choices) { // Accept dictionary of choices
        this.next = choices;
    }
    public override void Next() {
        this.next[currentSelection].Begin();
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("DialogueChoice")) {
            GameObject.Destroy(o);
        }
    }
    public override void Begin() {
        Debug.Log("Created buttons!");
        const int buttonHeight = 75;
        const int buttonWidth = 400;
        int x = 0;
        foreach (string option in next.Keys) {
            var buttonPos = new Vector2(x + buttonWidth/2, buttonHeight/2);
            Debug.Log(buttonPos);
            var button = Button.Instantiate(GameObject.FindGameObjectWithTag("TemplateButton").GetComponent<Button>(), buttonPos, Quaternion.identity);
            //button.transform.SetPositionAndRotation(buttonPos, Quaternion.identity);
            var rectTransform = button.GetComponent<RectTransform>();
            rectTransform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(buttonWidth, buttonHeight);
            rectTransform.position = buttonPos;
            button.tag = "DialogueChoice";

            button.GetComponentInChildren<TextMeshProUGUI>().text = option;
            button.onClick.AddListener(() => OnClick(option));
            x+=buttonWidth;
        }
    }

    // TODO Determine if you're hovering over a choice, update currentSelection and then call Next().
    private void OnClick(string selection) {
        this.currentSelection = selection;
        Next();
    }

}


// TODO This node will indicate a scene transition, it is an "endpoint" to the tree.
public class SceneChange : DialogueTree {
    string sceneName;
    public SceneChange(string sceneName) {
        this.sceneName = sceneName;
    }
    public override void Next() {
        GameScene.scenes[sceneName].LoadScene(); // When we want to "move on" from this scene, load the specified scene.

    }
    public override void Begin() {
        Next(); // The second we "enter" this node, just immediately move on to the next.    
    }
}


// TODO This node indicates playing a visual / audio effect
public class Effect : DialogueTree {
    public override void Next() { }
    public override void Begin() { }

}


// This class is just to mark the end for testing. It will likely result in errors if used.
public class DebugEnd : DialogueTree {
    public override void Next() { } // nothin
    public override void Begin() { } // still nothin

}
public class ArbitraryCodeNode : DialogueTree {
    private Func<Unit> code;
    private DialogueTree tree;
    public ArbitraryCodeNode(Func<Unit> r,DialogueTree tea) {
        this.code = r;
        this.tree = tea;
    }
    public override void Begin() {
        code();
        this.Next();
    }

    public override void Next() {
        this.tree.Begin();
    }
}
class GameVariables {
    private static int _s = 0;
    public static int supplies {
        get { return _s; }
        set { _s = value; updateStatDisplay(); }
    }

    public static void updateStatDisplay() {
        var sDisp = GameObject.FindGameObjectWithTag("SupplyDisplay");
        sDisp.GetComponent<TextMeshProUGUI>().text = "Supplies: " + supplies.ToString();
    }

    public static void initStats() {
        supplies = 0;
    }
}

