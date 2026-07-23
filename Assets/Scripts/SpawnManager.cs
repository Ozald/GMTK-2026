using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public GameObject enemyPrefab;

    public int maxEnemies = 20;
    public int minimumEnemies = 5;

    public float startingSpawnInterval = 3f;
    public float minimumSpawnInterval = 0.5f;
    public float spawnAcceleration = 0.05f;

    public float leftSpawnX = -10f;
    public float rightSpawnX = 10f;

    public float[] lanes;

    private float currentSpawnInterval;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentSpawnInterval = startingSpawnInterval;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            while (EnemyController.allEnemies.Count < minimumEnemies)
            {
                SpawnEnemy();

                yield return new WaitForSeconds(0.5f);
            }

            if (EnemyController.allEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }


            yield return new WaitForSeconds(currentSpawnInterval);


            currentSpawnInterval -= spawnAcceleration;

            currentSpawnInterval = Mathf.Max(
                currentSpawnInterval,
                minimumSpawnInterval
            );
        }
    }


    void SpawnEnemy()
    {
        bool spawnLeft = Random.value > 0.5f;

        float x = spawnLeft ? leftSpawnX : rightSpawnX;

        float y = lanes[Random.Range(0, lanes.Length)];

        Vector3 spawnPosition = new Vector3(x,y,0);

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}
