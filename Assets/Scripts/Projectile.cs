using UnityEngine;

public class Projectile : MonoBehaviour{
    //~ inspector (private)
    [SerializeField][Tooltip("the child game object for fliping")]             private GameObject visualChild;
    [SerializeField][Tooltip("if this object is looking to the right or not")] private bool facingRight = true;
    //~ public
    [HideInInspector] public AttackBehaviour attackBehaviourLink;
    [HideInInspector] public Vector3 travelDirectionNormal;
    [HideInInspector] public bool fromMinion = true;
    //~ private
    private float timeAlive = 0f;

    private void Start(){
        //~ if traveling to the left, flip sprite horizontally else flip back → looks in the direction of travel
        this.FlipHorizontally(this.travelDirectionNormal.x < 0);
    }

    private void FixedUpdate(){
        if((this.timeAlive += Time.fixedDeltaTime) >= this.attackBehaviourLink.projectileMaxTimeAlive) GameObject.Destroy(this.gameObject);
        else this.transform.position += this.travelDirectionNormal * this.attackBehaviourLink.projectileSpeed * Time.fixedDeltaTime;
    }

    private void OnCollisionEnter(Collision collision){
        // TODO detect if minion or enemy (transform parent ?)
        // FIXME damage only on collider transform parent get component...
        if(this.fromMinion){
            if(this.attackBehaviourLink.areaOfEffectActive){
                // TODO Physics.OverlapSphereNonAlloc()
                foreach(Collider collider in Physics.OverlapSphere(this.transform.position, this.attackBehaviourLink.areaOfEffectRange, LayerMask.GetMask("EnemyCollider"))){
                    Enemy enemy = collider.transform.parent.GetComponent<Enemy>();
                    if(enemy != null) enemy.Damage(this.attackBehaviourLink.areaOfEffectDamage);
                }
            }else{
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if(enemy != null) enemy.Damage(this.attackBehaviourLink.damage);
            }
        }else if(collision.gameObject.CompareTag("Player")){
            collision.transform.parent.GetComponent<PlayerManager>().Damage(this.attackBehaviourLink.damage);
            //~ no AOE here since the enemies can only damage player and not minions or themselves
        }else if(this.attackBehaviourLink.areaOfEffectActive){
            //~ if player is not directly hit, might hit indirectly
            // TODO Physics.OverlapSphereNonAlloc()
            foreach(Collider collider in Physics.OverlapSphere(this.transform.position, this.attackBehaviourLink.areaOfEffectRange, LayerMask.GetMask("PlayerCollider"))){
                if(collider.CompareTag("Player")){ collider.transform.parent.GetComponent<PlayerManager>().Damage(this.attackBehaviourLink.areaOfEffectDamage); }
            }
        }
        // TODO spawn explosion here ~ one second - then destroy both
        // BUG this does not work ?
        GameObject.Destroy(this.gameObject);
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
        this.visualChild.transform.localEulerAngles = new Vector3(
            this.visualChild.transform.localEulerAngles.x,
            this.visualChild.transform.localEulerAngles.y,
            this.visualChild.transform.localEulerAngles.z * -1f
        );
    }
}
