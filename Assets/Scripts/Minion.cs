using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Minion : MonoBehaviour{
    //~ inspector (private)
    [SerializeField][Tooltip("the game manager object")]                                                                          private GameManager gameManager;
    [Header("Visuals")]
    [SerializeField][Tooltip("the child game object for fliping")]                                                                private GameObject visualChild;
    [SerializeField][Tooltip("if this object is looking to the right or not")]                                                    private bool facingRight = true;
    [Header("Movement")]
    [SerializeField][Tooltip("the enemy layer mask")]                                                                             private LayerMask enemyLayer;
    [SerializeField][Tooltip("at this range, or further outwards, follows the player (also will not attack outside this range)")] private float playerFollowRange = 4f;
    [Header("Personality")]
    [SerializeField][Tooltip("the attack behaviour instance for this minion")]                                                    private AttackBehaviour attackBehaviour;
    //~ private
    private NavMeshAgent agent;
    private bool doneAttackDelay = true;
    private bool playerInFollowRange = false;
    private GameObject closestEnemy = null;
    private float closestEnemyDistanceSquare;
    private Animator anim;
    private bool lockOnEnemy = false;
    //~ public (getter)
    public ref AttackBehaviour AttackBehaviour => ref this.attackBehaviour;

    /// <summary> Apply the given upgrade </summary>
    /// <param name="upgrade"> The upgrade to apply </param>
    public void ApplyUpgrade(Upgrade upgrade){
        this.attackBehaviour.ApplyUpgrade(upgrade);
        //~ re-set the agent values (may have changed)
        this.agent.speed = this.attackBehaviour.walkingSpeed;
        this.agent.stoppingDistance = this.attackBehaviour.attackRange - 0.3f;
    }
    /// <summary> Copy attack behaviour (upgrades) from the given minion </summary>
    /// <param name="minion"> The minion to copy values from </param>
    public void CopyUpgradesFrom(Minion minion){
        this.attackBehaviour.CopyValuesFrom(minion.attackBehaviour);
        //~ re-set the agent values (may have changed)
        this.agent.speed = this.attackBehaviour.walkingSpeed;
        this.agent.stoppingDistance = this.attackBehaviour.attackRange - 0.3f;
    }

    private void Start(){
        this.agent = GetComponent<NavMeshAgent>();
        this.agent.speed = this.attackBehaviour.walkingSpeed;
        this.agent.stoppingDistance = this.attackBehaviour.attackRange - 0.3f;
        this.anim = GetComponent<Animator>();
    }

    private void Update(){
        float distanceToPlayerSquare = (this.gameManager.Player.transform.position - this.transform.position).sqrMagnitude;
        this.playerInFollowRange = distanceToPlayerSquare < this.playerFollowRange * this.playerFollowRange;
        if(this.playerInFollowRange){
            //~ set closest enemy in follow range
            if(
                !this.lockOnEnemy
                || this.closestEnemy == null
                || (this.transform.position - this.closestEnemy.transform.position).sqrMagnitude
                    > this.attackBehaviour.followRange * this.attackBehaviour.followRange
            ) this.SetClosestEnemy();
            //~ if enemy set follow enemy and attack if in range
            if(this.closestEnemy == null){
                this.lockOnEnemy = false;
                this.agent.ResetPath();
            }else{
                this.agent.SetDestination(this.closestEnemy.transform.position);
                if(!this.lockOnEnemy) Debug.Log($"[Minion : {this.gameObject.name}] Enemy Spotted!");
                this.lockOnEnemy = true;
                //~ if enemy is to the left, flip sprite horizontally else flip back → looks in enemies direction
                this.FlipHorizontally(this.transform.position.x > this.closestEnemy.transform.position.x);
                if(
                    this.doneAttackDelay
                    && this.closestEnemyDistanceSquare <= this.attackBehaviour.attackRange * this.attackBehaviour.attackRange
                ){
                    this.doneAttackDelay = false;
                    // FIXME ? meele attack does not work ???! (not projectile ~)
                    anim.SetBool("Attacking", true);
                    this.AttackEnemy();
                    Debug.Log($"[Minion : {this.gameObject.name}] Pow!");
                    StartCoroutine(this.DelayNextAttack(this.attackBehaviour.timeBetweenAttacks));
                }
            }
        }else{
            this.lockOnEnemy = false;
            this.FollowPlayer();
            this.anim.SetBool("Attacking", false);
            this.FlipHorizontally(this.transform.position.x > this.gameManager.Player.transform.position.x);
        };
    }

    private void LateUpdate(){ this.anim.SetBool("isMoving", agent.velocity.sqrMagnitude > 0.001f); }

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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, this.playerFollowRange);
    }

    /// <summary> minions follow the player </summary>
    private void FollowPlayer(){ this.agent.SetDestination(this.gameManager.Player.transform.position);}

    /// <summary>
    ///     If enemies are in range finds the closest one.
    ///     <br/>sets <paramref name="closestEnemy"/> and <paramref name="closestEnemyDistanceSquare"/>
    /// </summary>
    private void SetClosestEnemy(){
        this.closestEnemy = null;
        this.closestEnemyDistanceSquare = float.PositiveInfinity;
        float currentEnemyDistanceSquare;
        //~ get the closest enemy from all enemies in followRange, or stay at null when no enemy is in followRange
        // TODO Physics.OverlapSphereNonAlloc()
        foreach(Collider enemyCollider in Physics.OverlapSphere(this.transform.position, this.attackBehaviour.followRange, this.enemyLayer)){
            currentEnemyDistanceSquare = (this.transform.position - enemyCollider.transform.position).sqrMagnitude;
            if(currentEnemyDistanceSquare < this.closestEnemyDistanceSquare){
                this.closestEnemyDistanceSquare = currentEnemyDistanceSquare;
                this.closestEnemy = enemyCollider.gameObject;
            }
        }
    }

    /// <summary> minion attacks enemy </summary>
    [ContextMenu(itemName:"Attack")]
    private void AttackEnemy(){
        //~ meele OR projectile
        if(this.attackBehaviour.projectile != null){
            // BUG no meele attack/damage ?!
            Vector3 direction = this.closestEnemy.transform.position - this.transform.position;
            Projectile projectile = Instantiate<GameObject>(
                this.attackBehaviour.projectile,
                this.transform.position + this.attackBehaviour.projectileOffset,
                Quaternion.LookRotation(direction, Vector3.up)
            ).GetComponent<Projectile>();
            projectile.attackBehaviourLink = this.attackBehaviour;
            projectile.travelDirectionNormal = direction.normalized;
            projectile.fromMinion = true;
            //~ projectile handles damage
        }else{
            if(this.attackBehaviour.areaOfEffectActive){
                // TODO Physics.OverlapSphereNonAlloc()
                foreach(Collider collider in Physics.OverlapSphere(this.transform.position, this.attackBehaviour.areaOfEffectRange, this.enemyLayer)){
                    Enemy enemy = collider.GetComponent<Enemy>();
                    if(enemy != null) enemy.Damage(this.attackBehaviour.areaOfEffectDamage);
                }
            }else if(this.closestEnemy != null){
                Enemy enemy = this.closestEnemy.GetComponent<Enemy>();
                if(enemy != null) enemy.Damage(this.attackBehaviour.damage);
            }
        }
    }

    /// <summary> (Coroutine) Sets <paramref name="doneAttackDelay"/> to true after given <paramref name="delay"/> </summary>
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
}
