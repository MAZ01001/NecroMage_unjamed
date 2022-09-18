using UnityEngine;

public class PlayerManager : MonoBehaviour{
    [SerializeField][Tooltip("the child game object for fliping")]private GameObject visualChild;
    [SerializeField][Tooltip("if this object is looking to the right or not")]private bool facingRight = true;
    [SerializeField][Tooltip("starting health of the player")]private int health;
    [SerializeField][Tooltip("the movement speed of the player")]private float moveSpeed = 12f;
    [SerializeField][Tooltip("the smoothing factor of the player movement")]private float moveSmooth = 0.1f;
    [SerializeField][Tooltip("amount of invincibility frames")]private int invinciblityFrames = 100;
    [SerializeField][Tooltip("Diegetic Health Light Left")]private Light lightL;
    [SerializeField][Tooltip("Diegetic Health Light Right")]private Light lightR;

    private InputProvider inputProvider;
    private Rigidbody rb;
    private Vector3 smoothVelocity;
    private int invincFrames;

    public int GetHP => this.health;

    private void Start(){
        this.inputProvider = this.GetComponent<InputProvider>();
        this.rb = this.GetComponent<Rigidbody>();
    }
    private void Update(){
        if(this.invincFrames > 0) this.invincFrames--;
    }
    private void FixedUpdate(){
        //~ move
        this.rb.velocity = Vector3.SmoothDamp(
            this.rb.velocity,
            new Vector3(
                this.inputProvider.move.x,
                0f,
                this.inputProvider.move.y
            ).normalized * this.moveSpeed,
            ref this.smoothVelocity,
            this.moveSmooth
        );
        //~ if player is moving to the left, flip sprite horizontally else flip back → looks in direction of travel, or the right
        this.FlipHorizontally(this.inputProvider.move.x < 0f);
    }

    /// <summary> flips <paramref name="visualChild"/> transform horizontally </summary>
    /// <param name="flipToLeft"> whether to flip to the left site or not </param>
    private void FlipHorizontally(bool flipToLeft){
        if((flipToLeft == this.facingRight) == (this.visualChild.transform.localScale.x < 0f)) return;
        this.visualChild.transform.localScale = new Vector3(
            this.visualChild.transform.localScale.x * -1f,
            this.visualChild.transform.localScale.y,
            this.visualChild.transform.localScale.z
        );
    }

    /// <summary> damages the player and initiate game over if dead </summary>
    /// <param name="_"> [DEPRICATED] amount of damage </param>
    public void Damage(float _){
        if(this.invincFrames > 0)return;
        this.invincFrames = this.invinciblityFrames;
        Debug.Log("Ouch");
        Color color;
        switch(this.health){
            case >=4: color = Color.green; break;
            case 3: color = Color.yellow; break;
            case 2: color = new Color(1f,1f,0f,1f); break;
            case 1: color = Color.red; break;
            default: color = Color.black; break;
        }
        this.lightL.color = color;
        this.lightR.color = color;
        if((--this.health) <= 0f) this.inputProvider.gameManager.OnGameOver();
    }
}
