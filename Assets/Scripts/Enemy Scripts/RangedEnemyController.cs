using System.Collections;
using UnityEngine;

public class RangedEnemyController : EnemyController
{
    [Header("Ranged Settings")]
    public float minShootingDistance = 4f;
    public float maxShootingDistance = 7f;

    private float preferredShootingDistance;

    public GameObject projectilePrefab;
    public Transform firePoint;


    protected override void Start()
    {
        base.Start();

        preferredShootingDistance = Random.Range(
            minShootingDistance,
            maxShootingDistance
        );

        // Ranged enemies wait farther away
        waitingDistance = preferredShootingDistance;
        attackDistance = preferredShootingDistance;
    }

    protected override void WalkState()
    {
        if (isKnockedBack)
            return;

        float xDistance = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float yDistance = Mathf.Abs(transform.position.y - playerTransform.position.y);

        float side = Mathf.Sign(transform.position.x - playerTransform.position.x);

        Vector3 targetPosition = playerTransform.position +
            Vector3.right * side * preferredShootingDistance;

        // Match player's lane
        targetPosition.y = playerTransform.position.y;


        float distance = Vector3.Distance(
            transform.position,
            targetPosition
        );


        if (distance > stoppingDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);

            // Ready to shoot
            enemyState = EnemyStates.ReadyToAttack;
        }
    }

    protected override IEnumerator AttackCoroutine()
    {
        animator.Play("AttackState", -1, 0f);

        // Wait until the middle of the animation
        yield return new WaitForSeconds(attackWindUp * 0.5f);

        // Fire projectile here
        Shoot();

        // Finish the rest of the animation
        yield return new WaitForSeconds(attackAnimationLength - (attackWindUp * 0.5f));


        hitCount = 0;
        canBeInterrupted = true;

        animator.SetBool("IsWalking", false);

        enemyState = EnemyStates.Waiting;

        attackCoroutine = null;
    }


    private void Shoot()
    {
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            transform.rotation
        );


        Projectile projectileScript = projectile.GetComponent<Projectile>();

        if (projectileScript != null)
        {
            projectileScript.Initialize(transform.right);
        }
    }
}