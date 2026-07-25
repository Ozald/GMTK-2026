using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage;
    public int comboGain;

    private HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();

    private void OnEnable()
    {
        hitEnemies.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyController enemy = collision.GetComponent<EnemyController>();

        if (enemy == null)
            return;

        if (hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);

        if (enemy.TakeDamage(damage))
        {
            ComboManager.ComboAdd(comboGain);
        }
    }
}