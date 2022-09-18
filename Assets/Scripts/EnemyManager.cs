using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] GameObject[] enemy;
    [SerializeField] Vector3 spawnArea;
    [SerializeField] Vector3 spawnAreaHole;
    [SerializeField] float spawnTimer;
    [SerializeField] GameManager gameManager;
    [SerializeField] Transform enemyContainer;
    float timer;

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0f)
        {
            SpawnEnemy();
            timer = spawnTimer;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(gameManager.player.transform.position, spawnArea * 2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(gameManager.player.transform.position, spawnAreaHole * 2f);
    }
    private void SpawnEnemy()
    {
        Vector3 spawnPoint = new Vector3(
            UnityEngine.Random.Range(Random.Range(-spawnArea.x, -spawnAreaHole.x), Random.Range(spawnArea.x, spawnAreaHole.x)),
            (0f),
            UnityEngine.Random.Range(Random.Range(-spawnArea.z, -spawnAreaHole.z), Random.Range(spawnArea.z, spawnAreaHole.z)));
        GameObject newEnemy = Instantiate<GameObject>(enemy[Random.Range(0,enemy.Length)], enemyContainer);
        newEnemy.GetComponent<Enemy>().gameManager = gameManager;
        newEnemy.transform.position = spawnPoint + gameManager.player.transform.position;
                                                 


    }
}
