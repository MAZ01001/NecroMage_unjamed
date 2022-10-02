using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Create Upgrade", fileName = "New Upgrade")]
public class Upgrade : ScriptableObject{
    //~ inspector (public)
    [Header("Upgrade information")]
    [SerializeField][Tooltip("The minion (prefab) for wich the upgrade is for.")]                                     public GameObject minionPrefab;
    [SerializeField][Tooltip("The name of the upgrade.")]                                                             public string upgradeName;
    [SerializeField][Multiline][Tooltip("A short description for the upgrade.")]                                      public string upgradeDescription;
    [SerializeField][Tooltip("An image for displaying.")]                                                             public Image icon;
    [Header("Attack")]
    [SerializeField][Min(0)][Tooltip("Add to damage per attack.")]                                                    public int damage = 0;
    [SerializeField][Min(0f)][Tooltip("Add to attack delay in seconds.")]                                             public float timeBetweenAttacks = 0f;
    [Header("AOE (if available)")]
    [SerializeField][Tooltip("Set the AOE active state.")]                                                            public bool areaOfEffectActive = false;
    [SerializeField][Min(0f)][Tooltip("Add to AOE attack range.")]                                                    public float areaOfEffectRange = 0f;
    [SerializeField][Min(0)][Tooltip("Add to AOE attack damage.")]                                                    public int areaOfEffectDamage = 0;
    [Header("Projectile (if available)")]
    [SerializeField][Tooltip("Set the projectile to spawn for range attacks, if set does no meele attacks anymore.")] public GameObject projectile = null;
    [SerializeField][Tooltip("An offset for the projectile spawn.")]                                                  public Vector3 projectileOffset = Vector3.zero;
    [SerializeField][Min(0f)][Tooltip("Add to the lifespan of the projectile in seconds.")]                           public float projectileMaxTimeAlive = 0f;
    [SerializeField][Min(0f)][Tooltip("Add to the projectile speed in units per seconds.")]                           public float projectileSpeed = 0f;
    [Header("Ranges")]
    [SerializeField][Min(0f)][Tooltip("Add to the walking speed.")]                                                   public float walkingSpeed = 0f;
    [SerializeField][Min(0f)][Tooltip("Add to the attack range.")]                                                    public float attackRange = 0f;
    [SerializeField][Min(0f)][Tooltip("Add to the following range.")]                                                 public float followRange = 0f;
}
