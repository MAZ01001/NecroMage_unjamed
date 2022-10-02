using UnityEngine;
using UnityEngine.InputSystem;

public class InputProvider : MonoBehaviour{
    //~ inspector (private)
    [SerializeField][Tooltip("THE game manager in the scene")] private GameManager gameManager;
    //~ public
    [HideInInspector] public Vector2 move;
    [HideInInspector] public Vector2 look;
    [HideInInspector] public bool aimPressed;
    [HideInInspector] public bool isMoving;
    //~ private
    private Animator anim;

    private void Start(){ this.anim = GetComponent<Animator>(); }

    private void OnEnable(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void OnDisable(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnMove(InputValue value){
        this.move = value.Get<Vector2>().normalized;
        this.anim.SetBool("isMoving", this.move.sqrMagnitude > 0.001f);
    }
    private void OnLook(InputValue value){ this.look = value.Get<Vector2>().normalized; }
    private void OnAim(InputValue value){ this.aimPressed = value.isPressed; }
    private void OnPause(InputValue value){
        if(value.isPressed){
            if (Time.timeScale > 0.001f) this.gameManager.OnPause();
            else this.gameManager.OnResumefromPause();
        }
    }
}
