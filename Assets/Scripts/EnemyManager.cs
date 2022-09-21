using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour{
    [SerializeField][Tooltip("THE game manager in the scene")]private GameManager gameManager;
    [SerializeField][Tooltip("The game object to spawn the enemies inside of")]private Transform enemyContainer;

    [Header("Enemy prefabs")]
    [SerializeField][Tooltip("Enemy prefabs to spawn randomly from")]private GameObject[] enemy;

    [Header("Spawn area size")]
    [SerializeField][Tooltip("the area (from - to + on each axis) to randomly spawn inside of")]private Vector3 spawnArea;
    [SerializeField][Tooltip("the area (from - to + on each axis) to exclude from random spawning")]private Vector3 spawnAreaHole;

    [Header("Timing")]
    [SerializeField][Tooltip("Time in seconds to wait for next spawn")]private float spawnTimer;

    private bool spawnLoopRunning = true;

    private void OnEnable(){ this.spawnLoopRunning = true;}
    private void OnDisable(){ this.spawnLoopRunning = false;}

    private void Start(){ StartCoroutine(this.SpawnTimer()); }

    private IEnumerator SpawnTimer(){
        while(this.spawnLoopRunning){
            yield return new WaitForSeconds(this.spawnTimer);
            this.SpawnRandomEnemy();
        }
    }

    [ContextMenu(itemName:"SpawnRandomEnemy")]
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
            this.gameManager.player.transform.position + new Vector3(spawnX, 0f, spawnZ),
            enemyToSpawn.transform.rotation,
            this.enemyContainer
        ).GetComponent<Enemy>();
        //~ set gamemanager to the instance in scene
        enemyScriptOfSpawnedEnemy.gameManager = this.gameManager;
    }

    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(gameManager.player.transform.position, spawnArea * 2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(gameManager.player.transform.position, spawnAreaHole * 2f);
    }
}
