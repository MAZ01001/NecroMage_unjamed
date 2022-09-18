using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName ="Create Upgrade", fileName = "New Upgrade")]
public class Upgrade : ScriptableObject{
    public string upgradeName;
    public string upgradeDescription;
    public Image icon;

    [Header("Attack")]
    [Tooltip("add to damage per attack")]public float damage = 0f;
    [Tooltip("add to attack delay in seconds")]public float timeBetweenAttacks = 0f;

    [Header("AOE (if available)")]
    [Tooltip("add to AOE attack range")]public float areaOfEffectRange = 0f;
    [Tooltip("add to AOE attack damage")]public float areaOfEffectDamage = 0f;

    [Header("Projectile (if available)")]
    [Tooltip("add to the lifespan of the projectile in seconds")]public float projectileMaxTimeAlive = 0f;
    [Tooltip("add to the projectile speed in units per seconds")]public float projectileSpeed = 0f;

    [Header("Range")]
    [Tooltip("add to the walking speed")]public float walkingSpeed = 0f;
    [Tooltip("add to the attack range")]public float attackRange = 0f;
    [Tooltip("add to the following range")]public float followRange = 0f;
}
