using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour{
    //~ inspector (private)
    [SerializeField][Tooltip("THE game manager in the scene")]                                       private GameManager gameManager;
    [Header("Enemy prefabs")]
    [SerializeField][Tooltip("Enemy prefabs to spawn randomly from")]                                private GameObject[] enemy;
    [Header("Spawn area size")]
    [SerializeField][Tooltip("the area (from - to + on each axis) to randomly spawn inside of")]     private Vector3 spawnArea;
    [SerializeField][Tooltip("the area (from - to + on each axis) to exclude from random spawning")] private Vector3 spawnAreaHole;
    //~ inspector (public)
    [Header("Timing")]
    [SerializeField][Min(0f)][Tooltip("Time in seconds to wait for next spawn")]                     public float spawnTimer;
    [SerializeField][Min(1)][Tooltip("Maximum amount of enemies in scene")]                          public int maxEnemies = 100;
    //~ public
    [HideInInspector] public bool spawnLoopRunning = true;
    //~ public (getter)
    public ref GameObject[] Enemy => ref this.enemy;

    private void OnEnable(){ this.spawnLoopRunning = true; }
    private void OnDisable(){ this.spawnLoopRunning = false; }

    private void Start(){ StartCoroutine(this.SpawnLoop()); }

    /// <summary> Loop spawning until the flag <paramref name="spawnLoopRunning"/> is set to false. </summary>
    private IEnumerator SpawnLoop(){
        while(this.spawnLoopRunning){
            yield return new WaitForSeconds(this.spawnTimer);
            if(this.transform.childCount <= this.maxEnemies) this.SpawnRandomEnemy();
        }
    }

    /// <summary> Spawn a random enemy within set boundaries. </summary>
    [ContextMenu("Spawn a random enemy")]
    private void SpawnRandomEnemy(){
        //~ get random position within spawnArea, but outside spawnAreaHole
        float spawnX = Random.value > 0.5f
            ? Random.Range(-this.spawnArea.x, -this.spawnAreaHole.x)
            : Random.Range(this.spawnAreaHole.x, this.spawnArea.x);
        float spawnZ = Random.value > 0.5f
            ? Random.Range(-this.spawnArea.z, -this.spawnAreaHole.z)
            : Random.Range(this.spawnAreaHole.z, this.spawnArea.z);
        //~ get random enemy prefab
        GameObject enemyToSpawn = this.enemy[Random.Range(0,this.enemy.Length)];
        //~ spawn enemy
        Enemy enemyScriptOfSpawnedEnemy = Object.Instantiate<GameObject>(
            enemyToSpawn,
            this.gameManager.Player.transform.position + new Vector3(spawnX, 0f, spawnZ),
            enemyToSpawn.transform.rotation,
            this.transform
        ).GetComponent<Enemy>();
        //~ set gamemanager to the instance in scene
        enemyScriptOfSpawnedEnemy.gameManager = this.gameManager;
    }

    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(gameManager.Player.transform.position, spawnArea * 2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(gameManager.Player.transform.position, spawnAreaHole * 2f);
    }
}
