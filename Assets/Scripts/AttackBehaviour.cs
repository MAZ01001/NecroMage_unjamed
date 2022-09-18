using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create AttackBehaviour", fileName = "NewAttackBehaviour")]
public class AttackBehaviour : ScriptableObject{
    [Header("Attack")]
    [Tooltip("ammount of damage to deal for each attack")]public float damage = 0f;
    [Tooltip("minimum delay in seconds between previous and next attack")]public float timeBetweenAttacks = 1f;

    [Header("AOE")]
    [Tooltip("if AOE is active")]public bool areaOfEffectActive = false;
    [Tooltip("AOE attack range")]public float areaOfEffectRange = 0f;
    [Tooltip("AOE attack damage")]public float areaOfEffectDamage = 0f;

    [Header("Projectile")]
    [Tooltip("the projectile to spawn for range attacks, if set does no meele attacks")]public GameObject projectile = null;
    [Tooltip("an offset for the projectile spawn")]public Vector3 projectileOffset = Vector3.zero;
    [Tooltip("the maximum time in seconds for the projectile to live")]public float projectileMaxTimeAlive = 0f;
    [Tooltip("projectile speed in units per seconds")]public float projectileSpeed = 1f;

    [Header("Range")]
    [Tooltip("the walking speed")]public float walkingSpeed = 3f;
    [Tooltip("attack at that distance or closer")]public float attackRange = 1.5f;
    [Tooltip("start following at that distance or closer")]public float followRange = 10f;

    [Tooltip("temp storage for the upgrades for this \"type\" of minion / enemy")]public List<Upgrade> upgrades;

    public void ApplyUpgrade(Upgrade upgrade){
        // TODO test if this even works !!
        this.damage += upgrade.damage;
        this.timeBetweenAttacks += upgrade.timeBetweenAttacks;
        this.areaOfEffectRange += upgrade.areaOfEffectRange;
        this.areaOfEffectDamage += upgrade.areaOfEffectDamage;
        this.projectileMaxTimeAlive += upgrade.projectileMaxTimeAlive;
        this.projectileSpeed += upgrade.projectileSpeed;
        this.walkingSpeed += upgrade.walkingSpeed;
        this.attackRange += upgrade.attackRange;
        this.followRange += upgrade.followRange;
    }
}
