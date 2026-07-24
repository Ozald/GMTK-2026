using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    EnemyController rightSideEnemy;
    EnemyController leftSideEnemy;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        FindClosestEnemies();
    }

    void FindClosestEnemies()
    {
        float closestRightDistance = Mathf.Infinity;
        float closestLeftDistance = Mathf.Infinity;

        rightSideEnemy = null;
        leftSideEnemy = null;

        foreach(EnemyController enemy in EnemyController.allEnemies)
        {
            float distance = Mathf.Abs(enemy.transform.position.x - player.position.x);

            if (enemy.transform.position.x > player.position.x)
            {
                if (distance < closestRightDistance)
                {
                    closestRightDistance = distance;
                    rightSideEnemy = enemy;
                }
            }

            // Enemy is on the left side
            else if (enemy.transform.position.x < player.position.x)
            {
                if (distance < closestLeftDistance)
                {
                    closestLeftDistance = distance;
                    leftSideEnemy = enemy;
                }
            }
        }
    }

    public bool CanEnemyAttack(EnemyController enemy)
    {
        return enemy == rightSideEnemy || enemy == leftSideEnemy;
    }

    public bool IsOtherEnemyAttacking(EnemyController currentEnemy)
    {
        if (leftSideEnemy != null &&
            leftSideEnemy != currentEnemy &&
            leftSideEnemy.enemyState == EnemyController.EnemyStates.Attack)
        {
            return true;
        }

        if (rightSideEnemy != null &&
            rightSideEnemy != currentEnemy &&
            rightSideEnemy.enemyState == EnemyController.EnemyStates.Attack)
        {
            return true;
        }

        return false;
    }
}