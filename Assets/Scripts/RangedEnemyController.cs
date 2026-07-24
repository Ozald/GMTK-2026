using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyController : EnemyController
{

    public float minShootingDistance = 4f;
    public float maxShootingDistance = 7f;

    private float preferredShootingDistance;
    public GameObject projectilePrefab;
    public Transform firePoint;

    void Awake()
    {
        preferredShootingDistance = Random.Range(minShootingDistance, maxShootingDistance);
    }

    protected override void WalkState()
    {
        float xDistance = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float yDistance = Mathf.Abs(transform.position.y - playerTransform.position.y);

        Vector3 targetPosition = transform.position;

        if (yDistance > laneTolerance)
        {
            targetPosition.y = playerTransform.position.y;
        }

        float side = Mathf.Sign(transform.position.x - playerTransform.position.x);

        targetPosition.x = playerTransform.position.x + side * preferredShootingDistance;


        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        if (xDistance <= preferredShootingDistance && yDistance <= laneTolerance)
        {
            enemyState = EnemyStates.ReadyToAttack;
        }
    }

    protected override IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(attackWindUp);

        Shoot();

        yield return new WaitForSeconds(attackAnimationLength);

        enemyState = EnemyStates.Waiting;
        attackCoroutine = null;
    }

    void Shoot()
    {
        GameObject projectile = Instantiate( projectilePrefab,firePoint.position,transform.rotation);

        Projectile projectileScript = projectile.GetComponent<Projectile>();

        projectileScript.Initialize(transform.right);
    }
}
