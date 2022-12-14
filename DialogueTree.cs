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
    public abstract void PushTree(DialogueTree t); // Adds a tree to the node
}


// TODO This node will make a character say a message.
public class DialogueMessage : DialogueTree {
    public string msg;
    public string charName;
    public DialogueTree next;
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

    public override void PushTree(DialogueTree t) {
        if (this.next == null) {
            this.next = t;
        } else {
            this.next.PushTree(t);
        }
    }

    private void OnClick() {
        Next();
    }
}


    // TODO This node will give the user a choice and follow the relevant branch
public class DialogueChoice : DialogueTree {
    Dictionary<string, (Func<bool>, DialogueTree)> next; // Match a string to its relevant tree branch.
    string currentSelection; // The current selection we're looking at. 

    public DialogueChoice(Dictionary<string, (Func<bool>, DialogueTree)> choices) { // Accept dictionary of choices
        this.next = choices;
    }
    public override void Next() {
        this.next[currentSelection].Item2.Begin();
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("DialogueChoice")) {
            GameObject.Destroy(o);
        }
    }
    public override void Begin() {
        const int buttonHeight = 75;
        const int buttonWidth = 400;
        int x = 0;
        foreach (string option in next.Keys) {
            if (next[option].Item1 == null || next[option].Item1()) {
                var buttonPos = new Vector2(x + buttonWidth/2, buttonHeight/2);
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
    }

    // TODO Determine if you're hovering over a choice, update currentSelection and then call Next().
    private void OnClick(string selection) {
        this.currentSelection = selection;
        Next();
    }
    
    bool hasNext = false;
    public override void PushTree(DialogueTree t) {
        if (hasNext) {
            foreach (var n in next.Values) { // Jank method to select an arbitrary node since this doesn't follow typical iterator conventions.
                n.Item2.PushTree(t);
                break;
            }
        } else {
            foreach (var n in next.Values) {
                n.Item2.PushTree(t);
            }
            hasNext = true;
        }
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
    
    public override void PushTree(DialogueTree t) {
        throw new CompileException("Unreachable statements behind goto statement.");
    }
}


// This class is just to mark the end for testing. It will likely result in errors if used.
public class DebugEnd : DialogueTree {
    public override void Next() { } // nothin
    public override void Begin() { } // still nothin
    
    public override void PushTree(DialogueTree t) {
        throw new CompileException("Can't have anything after DebugEnd.");
    }
}
public class ArbitraryCodeNode : DialogueTree {
    private Action code;
    public DialogueTree next;
    public ArbitraryCodeNode(Action  r, DialogueTree tea) {
        this.code = r;
        this.next = tea;
    }
    public override void Begin() {
        code();
        this.Next();
    }

    public override void Next() {
        this.next.Begin();
    }
    public override void PushTree(DialogueTree t) {
        if (this.next == null) {
            this.next = t;
        } else {
            this.next.PushTree(t);
        }
    }
    
}
class GameVariables {
    private static Dictionary<String, int> data = new Dictionary<String, int>();

    public static void updateStatDisplay() {
        var sDisp = GameObject.FindGameObjectWithTag("SupplyDisplay");
        sDisp.GetComponent<TextMeshProUGUI>().text = "Supplies: " + data["supplies"].ToString();
    }

    public static int Get(string name) {
        return data.GetValueOrDefault(name, 0);
    }
    public static void Set(string name, int val) {
        data[name] = val;
        updateStatDisplay();
    }
}

