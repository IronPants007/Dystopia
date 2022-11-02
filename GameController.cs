using UnityEngine;

public class GameController : MonoBehaviour {

    // Start is called before the first frame update
    void Start() {
        
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            MouseClicked?.Invoke();
        }
    }

    public delegate void Notify();

    public static event Notify MouseClicked;
}
