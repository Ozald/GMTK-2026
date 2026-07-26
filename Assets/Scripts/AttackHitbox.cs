using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage;
    public float knockback;
    public int comboGain;

    [Header("Multi Hit")]
    public float multiHitIncrease = 0.25f;
    public float maxMultiHitMultiplier = 3f;

    private HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();


    private void OnEnable()
    {
        hitEnemies.Clear();
    }


    private void OnDisable()
    {
        CheckMultiHit();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyController enemy = collision.GetComponent<EnemyController>();

        if (enemy == null)
            return;

        if (hitEnemies.Contains(enemy))
            return;

        if (transform.position.y > collision.transform.position.y + 1f || transform.position.y < collision.transform.position.y - 1f)
            return;

        hitEnemies.Add(enemy);

        CombatFeed.Instance.AddHit();

        int finalDamage = damage;

        if (enemy.TakeDamage(finalDamage, knockback))
        {
            float hitMultiplier = ComboManager.GetDamageMultiplier();
            float multiHitMultiplier = GetMultiHitMultiplier();

            int finalComboGain = Mathf.RoundToInt(
                comboGain * hitMultiplier * multiHitMultiplier
            );

            ComboManager.ComboAdd(finalComboGain);
        }
    }


    private float GetMultiHitMultiplier()
    {
        int targets = hitEnemies.Count;

        if (targets <= 1)
            return 1f;


        float multiplier = 1f + ((targets - 1) * multiHitIncrease);

        return Mathf.Min(
            multiplier,
            maxMultiHitMultiplier
        );
    }


    private void CheckMultiHit()
    {
        if (hitEnemies.Count > 1)
        {
            CombatFeed.Instance.Add(
                $"MultiHit (x{hitEnemies.Count})"
            );
        }
    }
}