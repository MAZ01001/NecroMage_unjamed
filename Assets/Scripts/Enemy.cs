using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour{
    [SerializeField][Tooltip("the game manager object")]public GameManager gameManager;
    [SerializeField][Tooltip("the child game object for fliping")]private GameObject visualChild;
    [SerializeField][Tooltip("if this object is looking to the right or not")]private bool facingRight = true;
    [SerializeField][Tooltip("health of this enemy")]private float health;
    [SerializeField][Tooltip("the collectible prefab / pile of bones to drop on death")]private GameObject dropObject = null;
    [SerializeField][Tooltip("HP to drop on death")]private float dropHP = 0f;
    [SerializeField][Tooltip("XP to drop on death")]private float dropXP = 0f;
    [SerializeField][Tooltip("the attack behaviour instance for this enemy")]private AttackBehaviour attackBehaviour;
    [SerializeField][Tooltip("closest distance to the target object in the navmesh")]private float closestDistance = 0.8f;
    [SerializeField][Tooltip("amount of exp awarded for defeating enemy")]int experianceReward = 400;
    [SerializeField] [Tooltip("counts enemies that have been defeated")]public int enemiesDefeated;

    private bool playerInAttackRange;
    private bool playerInFollowRange;
    private NavMeshAgent agent;
    private bool doneAttackDelay = true;

    private void Start(){
        this.agent = this.GetComponent<NavMeshAgent>();
        this.agent.speed = this.attackBehaviour.walkingSpeed;
        this.agent.stoppingDistance = this.closestDistance;
    }
    private void FixedUpdate(){
        //~ calculate distances
        float distanceToPlayerSquare = (this.gameManager.player.transform.position - this.transform.position).sqrMagnitude;
        this.playerInAttackRange = distanceToPlayerSquare < this.attackBehaviour.attackRange * this.attackBehaviour.attackRange;
        this.playerInFollowRange = distanceToPlayerSquare < this.attackBehaviour.followRange * this.attackBehaviour.followRange;
    }
    private void LateUpdate(){
        if(this.playerInFollowRange)this.agent.SetDestination(this.gameManager.player.transform.position);
        else this.agent.ResetPath();
        //~ if player is to the left, flip sprite horizontally else flip back → looks in players direction
        this.FlipHorizontally(this.transform.position.x > this.gameManager.player.transform.position.x);
        if (
            this.playerInAttackRange
            && this.doneAttackDelay
        ){
            this.doneAttackDelay = false;
            // FIXME only one time attack ?!
            this.AttackPlayer();
            StartCoroutine(this.DelayNextAttack(this.attackBehaviour.timeBetweenAttacks));
        }
    }

    /// <summary> makes attacking available again after a small delay </summary>
    /// <param name="delay"> the delay in seconds </param>
    private IEnumerator DelayNextAttack(float delay){
        yield return new WaitForSeconds(delay);
        this.doneAttackDelay = true;
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

    /// <summary> [debug] if selected draws Attack and Sight range </summary>
    private void OnDrawGizmosSelected(){
        if(this.attackBehaviour != null){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, this.attackBehaviour.attackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, this.attackBehaviour.followRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, this.attackBehaviour.areaOfEffectRange);
        }
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, this.closestDistance);
    }

    /// <summary> enemy attacks player </summary>
    [ContextMenu(itemName:"Attack")]
    private void AttackPlayer(){
        //~ meele OR projectile
        if(this.attackBehaviour.projectile != null){
            Vector3 direction = this.gameManager.player.transform.position - this.transform.position;
            Projectile projectile = Instantiate<GameObject>(
                this.attackBehaviour.projectile,
                this.transform.position + this.attackBehaviour.projectileOffset,
                Quaternion.LookRotation(direction, Vector3.up)
            ).GetComponent<Projectile>();
            projectile.attackBehaviourLink = this.attackBehaviour;
            projectile.travelDirectionNormal = direction.normalized;
            projectile.fromMinion = false;
            //~ projectile handles damage
        }else{
            if(this.attackBehaviour.areaOfEffectActive){
                foreach(Collider collider in Physics.OverlapSphere(this.transform.position, this.attackBehaviour.areaOfEffectRange, LayerMask.GetMask("Default"))){
                    if(collider.gameObject.CompareTag("Player")) collider.gameObject.GetComponent<PlayerManager>().Damage(this.attackBehaviour.areaOfEffectDamage);
                }
            }else this.gameManager.player.GetComponent<PlayerManager>().Damage(this.attackBehaviour.damage);
        }
    }

    /// <summary> damage this enemy and removes it if it's dead </summary>
    /// <param name="damage"> amount of damage </param>
    public void Damage(float damage){
        if((this.health -= damage) <= 0f){
            this.gameManager.AddExperience(experianceReward);
            if(this.dropObject != null){
                Collectible drop = Instantiate<GameObject>(
                    this.dropObject,
                    this.transform.position,
                    Quaternion.identity
                ).GetComponent<Collectible>();
                drop.giveHP = this.dropHP;
                drop.giveXP = this.dropXP;
            }
            GameObject.Destroy(this.gameObject);
            enemiesDefeated++;
        }
    }
}
