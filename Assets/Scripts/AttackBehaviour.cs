using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Create AttackBehaviour", fileName = "NewAttackBehaviour")]
public class AttackBehaviour : ScriptableObject{
    //~ inspector (public)
    [Header("Attack")]
    [SerializeField][Min(0f)][Tooltip("ammount of damage to deal for each attack")]                      public int damage = 1;
    [SerializeField][Min(0f)][Tooltip("minimum delay in seconds between previous and next attack")]      public float timeBetweenAttacks = 1f;
    [Header("AOE")]
    [SerializeField][Tooltip("if AOE is active")]                                                        public bool areaOfEffectActive = false;
    [SerializeField][Min(0f)][Tooltip("AOE attack range")]                                               public float areaOfEffectRange = 0f;
    [SerializeField][Min(0f)][Tooltip("AOE attack damage")]                                              public int areaOfEffectDamage = 1;
    [Header("Projectile")]
    [SerializeField][Tooltip("the projectile to spawn for range attacks, if set does no meele attacks")] public GameObject projectile = null;
    [SerializeField][Tooltip("an offset for the projectile spawn")]                                      public Vector3 projectileOffset = Vector3.zero;
    [SerializeField][Min(0f)][Tooltip("the maximum time in seconds for the projectile to live")]         public float projectileMaxTimeAlive = 0f;
    [SerializeField][Min(0f)][Tooltip("projectile speed in units per seconds")]                          public float projectileSpeed = 1f;
    [Header("Ranges")]
    [SerializeField][Min(0f)][Tooltip("the walking speed")]                                              public float walkingSpeed = 3f;
    [SerializeField][Min(0f)][Tooltip("attack at that distance or closer (also stops walking closer)")]  public float attackRange = 1.5f;
    [SerializeField][Min(0f)][Tooltip("start following at that distance or closer")]                     public float followRange = 10f;

    /// <summary>
    ///     Adds the values from the given <paramref name="upgrade"/> to the existing values of this attack behaviour
    ///     <br/>but <paramref name="projectile"/> and <paramref name="projectileOffset"/> will be overitten
    ///     <br/>also re-sets the navmeshagent values, that might have changed (taken from the <paramref name="minionPrefab"/> from the given <paramref name="upgrade"/>).
    /// </summary>
    /// <param name="upgrade"> the upgrade to get the values from </param>
    public void ApplyUpgrade(Upgrade upgrade){
        this.damage += upgrade.damage;
        this.timeBetweenAttacks += upgrade.timeBetweenAttacks;

        this.areaOfEffectActive = upgrade.areaOfEffectActive;
        this.areaOfEffectRange += upgrade.areaOfEffectRange;
        this.areaOfEffectDamage += upgrade.areaOfEffectDamage;

        this.projectile = upgrade.projectile;
        this.projectileOffset = upgrade.projectileOffset;
        this.projectileMaxTimeAlive += upgrade.projectileMaxTimeAlive;
        this.projectileSpeed += upgrade.projectileSpeed;

        this.walkingSpeed += upgrade.walkingSpeed;
        this.attackRange += upgrade.attackRange;
        this.followRange += upgrade.followRange;

        //~ re-set the agent values (may have changed)
        NavMeshAgent minionAgent = upgrade.minionPrefab.GetComponent<NavMeshAgent>();
        minionAgent.speed = this.walkingSpeed;
        minionAgent.stoppingDistance = this.attackRange - 0.3f;
    }
    /// <summary> copies the values from the <paramref name="otherAttackBehaviour"/> into this one (overrides all) </summary>
    /// <param name="otherAttackBehaviour"> the other attack behaviour to copy the values from </param>
    public void CopyValuesFrom(AttackBehaviour otherAttackBehaviour){
        this.damage = otherAttackBehaviour.damage;
        this.timeBetweenAttacks = otherAttackBehaviour.timeBetweenAttacks;

        this.areaOfEffectActive = otherAttackBehaviour.areaOfEffectActive;
        this.areaOfEffectRange = otherAttackBehaviour.areaOfEffectRange;
        this.areaOfEffectDamage = otherAttackBehaviour.areaOfEffectDamage;

        this.projectile = otherAttackBehaviour.projectile;
        this.projectileOffset = otherAttackBehaviour.projectileOffset;
        this.projectileMaxTimeAlive = otherAttackBehaviour.projectileMaxTimeAlive;
        this.projectileSpeed = otherAttackBehaviour.projectileSpeed;

        this.walkingSpeed = otherAttackBehaviour.walkingSpeed;
        this.attackRange = otherAttackBehaviour.attackRange;
        this.followRange = otherAttackBehaviour.followRange;
    }
}
