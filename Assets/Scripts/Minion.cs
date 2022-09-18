using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Minion : MonoBehaviour{
    [SerializeField][Tooltip("the game manager object")]private GameManager gameManager;

    [Header("Visuals")]
    [SerializeField][Tooltip("if this object is looking to the right or not")]private bool facingRight = true;
    [SerializeField][Tooltip("the child game object for fliping")]private GameObject visualChild;
    
    [Header("Movement")]
    [SerializeField][Tooltip("the enemy layer mask")]private LayerMask enemyLayer;
    [SerializeField][Tooltip("at this range, or closer, enemies are spotted and followed")]private float playerFollowRange = 4f;
    [SerializeField][Tooltip("closest distance to the target object in the navmesh")]private float closestDistance = 0.8f;

    [Header("Personality")]
    [SerializeField][Tooltip("the attack behaviour instance for this enemy")]public AttackBehaviour attackBehaviour;
    [SerializeField][Tooltip("the name of this minion")]public string displayName = "";
    [SerializeField][Tooltip("the description of this minion")]public string displayDescription = "";

    private NavMeshAgent agent;
    private bool doneAttackDelay = true;
    private bool playerInFollowRange = false;
    private GameObject closestEnemy = null;
    private float closestEnemyDistanceSquare;
    private Animator anim;
    private bool lockOnEnemy = false;

    private void Start(){
        this.agent = GetComponent<NavMeshAgent>();
        this.agent.speed = this.attackBehaviour.walkingSpeed;
        this.agent.stoppingDistance = this.closestDistance;
        this.anim = GetComponent<Animator>();
    }

    /// <summary> physics stuff </summary>
    private void FixedUpdate(){
        float distanceToPlayerSquare = (this.gameManager.player.transform.position - this.transform.position).sqrMagnitude;
        this.playerInFollowRange = distanceToPlayerSquare < this.playerFollowRange * this.playerFollowRange;
        if (this.playerInFollowRange) {
            if (!this.lockOnEnemy)
            {
                this.FollowClosestEnemy();
            }
            if (this.closestEnemy == null) this.agent.ResetPath();
            else { 
                    this.agent.SetDestination(this.closestEnemy.transform.position);
                    Debug.Log("Enemy Spotted!");
                
                this.lockOnEnemy = true;
                //~ if enemy is to the left, flip sprite horizontally else flip back → looks in enemies direction
                this.FlipHorizontally(this.transform.position.x > this.closestEnemy.transform.position.x);
                if (
                    this.doneAttackDelay
                    && this.closestEnemyDistanceSquare <= this.attackBehaviour.attackRange * this.attackBehaviour.attackRange
                ) {
                    this.doneAttackDelay = false;
                    // FIXME meele attack does not work (not projectile ~)
                    anim.SetBool("Attacking", true);
                    this.AttackEnemy();
                    Debug.Log("Pow!");
                    StartCoroutine(this.DelayNextAttack(this.attackBehaviour.timeBetweenAttacks));
                }
            }
        } 
        else
        {
            this.lockOnEnemy = false;
            FollowPlayer();
            anim.SetBool("Attacking", false);
            this.FlipHorizontally(this.transform.position.x > this.gameManager.player.transform.position.x);
        };
    }

    private void LateUpdate()
    {
        if (agent.velocity.sqrMagnitude > 0.001f)
        {
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, this.playerFollowRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, this.closestDistance);
    }

    /// <summary> minions follow the player </summary>
    private void FollowPlayer(){ this.agent.SetDestination(this.gameManager.player.transform.position);}
    /// <summary> minion chases closest enemy it finds in its range </summary>
    private void FollowClosestEnemy(){
        this.closestEnemy = null;
        this.closestEnemyDistanceSquare = float.PositiveInfinity;
        float currentEnemyDistanceSquare;
        //~ get the closest enemy from all enemies in followRange, or stay at null when no enemy is in followRange
        foreach(Collider enemy in Physics.OverlapSphere(this.transform.position, this.attackBehaviour.followRange, this.enemyLayer)){
            currentEnemyDistanceSquare = (this.transform.position - enemy.transform.position).sqrMagnitude;
            if(currentEnemyDistanceSquare < this.closestEnemyDistanceSquare * this.closestEnemyDistanceSquare){
                this.closestEnemyDistanceSquare = currentEnemyDistanceSquare;
                this.closestEnemy = enemy.gameObject;
            }

        }
    }

    /// <summary> minion attacks enemy </summary>
    [ContextMenu(itemName:"Attack")]
    private void AttackEnemy(){
        //~ meele OR projectile
        if (this.attackBehaviour.projectile != null)
        {
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
