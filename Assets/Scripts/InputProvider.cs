using UnityEngine;
using UnityEngine.InputSystem;

public class InputProvider : MonoBehaviour{
    public GameManager gameManager;
    [Space]
    public Vector2 move;
    public Vector2 look;
    public bool aimPressed;
    public bool isMoving;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void OnDisable(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnMove(InputValue value) { move = value.Get<Vector2>().normalized; if (move.magnitude > 0.001f) {anim.SetBool("isMoving", true);} else {anim.SetBool("isMoving", false);} }
    void OnLook(InputValue value){ look = value.Get<Vector2>().normalized; }
    void OnAim(InputValue value){ aimPressed = value.isPressed; }
    void OnPause(InputValue value){
        if(value.isPressed){
            if (Time.timeScale > 0.001f) this.gameManager.OnPause();
            else this.gameManager.OnResume();
        }
    }
}
