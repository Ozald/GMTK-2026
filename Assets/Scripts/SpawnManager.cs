using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private int rangedCount;
    [SerializeField] private int tankCount;

    public static SpawnManager Instance;

    public int maxEnemies = 20;
    public int minimumEnemies = 5;

    public float startingSpawnInterval = 3f;
    public float minimumSpawnInterval = 0.5f;
    public float spawnAcceleration = 0.05f;

    public GameObject gruntPrefab;
    public GameObject rangedPrefab;
    public GameObject tankPrefab;

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


    void Start()
    {
        currentSpawnInterval = startingSpawnInterval;
        StartCoroutine(SpawnRoutine());
    }


    private void Update()
    {
        UpdateEnemyCounts();
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
        }
    }


    void SpawnEnemy()
    {
        UpdateEnemyCounts();

        GameObject prefabToSpawn;

        float roll = Random.value;

        // 15% chance for ranged
        if (roll < 0.15f && rangedCount < 2)
        {
            prefabToSpawn = rangedPrefab;
        }
        // 10% chance for tank
        else if (roll < 0.25f && tankCount < 2)
        {
            prefabToSpawn = tankPrefab;
        }
        // Otherwise grunt
        else
        {
            prefabToSpawn = gruntPrefab;
        }

        bool spawnLeft = Random.value > 0.5f;

        float x = spawnLeft ? leftSpawnX : rightSpawnX;
        float y = lanes[Random.Range(0, lanes.Length)];

        Vector3 spawnPosition = new Vector3(x, y, 0);

        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    }

    public void IncreaseDifficulty()
    {
        currentSpawnInterval -= 0.3f;

        currentSpawnInterval = Mathf.Max(
            currentSpawnInterval,
            minimumSpawnInterval
        );
    }

    void UpdateEnemyCounts()
    {
        rangedCount = 0;
        tankCount = 0;

        foreach (EnemyController enemy in EnemyController.allEnemies)
        {
            switch (enemy.enemyType)
            {
                case EnemyController.EnemyType.Ranged:
                    rangedCount++;
                    break;

                case EnemyController.EnemyType.Tank:
                    tankCount++;
                    break;
            }
        }
    }
}