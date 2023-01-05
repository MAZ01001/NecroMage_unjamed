using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Create Upgrade", fileName = "New Upgrade")]
public class Upgrade : ScriptableObject{
    //~ inspector (public)
    [Header("UI")]
    [SerializeField][Tooltip("An image for displaying.")]                                                             public Image Icon;
    [SerializeField][Min(0)][Tooltip("The level of the player required for this upgrade.")]                           public uint LevelRequirement = 1;
    [SerializeField][Tooltip("The name of the upgrade.")]                                                             public string UpgradeName;
    [SerializeField][Multiline][Tooltip("A short description for the upgrade.")]                                      public string UpgradeDescription;
    [Header("Upgrade information")]
    [SerializeField][Range(0, 10)][Tooltip("Add to the players health points.")]                                      public int PlayerHP = 0;
    [SerializeField][Min(0)][Tooltip("Add to the players experience points.")]                                        public int PlayerXP = 0;
    [SerializeField][Tooltip("If this upgrade is a minion unlock of the following minions.")]                         public bool IsMinionUnlock = false;
    [SerializeField][Tooltip("The minions (prefabs) for which the upgrade is for.")]                                  public List<GameObject> MinionPrefabs;
    [Header("Attack")]
    [SerializeField][Min(0)][Tooltip("Add to damage per attack.")]                                                    public int Damage = 0;
    [SerializeField][Min(0f)][Tooltip("Add to attack delay in seconds.")]                                             public float TimeBetweenAttacks = 0f;
    [Header("AOE (if available)")]
    [SerializeField][Tooltip("Set the AOE active state.")]                                                            public bool AreaOfEffectActive = false;
    [SerializeField][Min(0f)][Tooltip("Add to AOE attack range.")]                                                    public float AreaOfEffectRange = 0f;
    [SerializeField][Min(0)][Tooltip("Add to AOE attack damage.")]                                                    public int AreaOfEffectDamage = 0;
    [Header("Projectile (if available)")]
    [SerializeField][Tooltip("Set the projectile to spawn for range attacks, if set does no meele attacks anymore.")] public GameObject Projectile = null;
    [SerializeField][Tooltip("An offset for the projectile spawn.")]                                                  public Vector3 ProjectileOffset = Vector3.zero;
    [SerializeField][Min(0f)][Tooltip("Add to the lifespan of the projectile in seconds.")]                           public float ProjectileMaxTimeAlive = 0f;
    [SerializeField][Min(0f)][Tooltip("Add to the projectile speed in units per seconds.")]                           public float ProjectileSpeed = 0f;
    [Header("Ranges")]
    [SerializeField][Min(0f)][Tooltip("Add to the walking speed.")]                                                   public float WalkingSpeed = 0f;
    [SerializeField][Min(0f)][Tooltip("Add to the attack range.")]                                                    public float AttackRange = 0f;
    [SerializeField][Min(0f)][Tooltip("Add to the following range.")]                                                 public float FollowRange = 0f;
}
