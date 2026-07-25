using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatFeed : MonoBehaviour
{
    public static CombatFeed Instance;

    [SerializeField] private TMP_Text feedText;
    [SerializeField] private int maxEntries = 5;

    private int hitChain = 0;
    private float lastHitTime = -100f;
    [SerializeField] private float hitChainTime = 2f;

    private List<string> entries = new();

    private void Awake()
    {
        Instance = this;
    }

    public int GetHitChain()
    {
        return hitChain;
    }

    public void Add(string message)
    {
        entries.Insert(0, message);

        if (entries.Count > maxEntries)
            entries.RemoveAt(entries.Count - 1);

        feedText.text = string.Join("\n", entries);
    }

    public void AddHit()
    {
        // Reset if too much time has passed
        if (Time.time - lastHitTime > hitChainTime)
        {
            hitChain = 1;
            entries.Insert(0, "Hit");
        }
        else
        {
            hitChain++;

            // Replace the newest entry
            entries[0] = $"Hit (x{hitChain})";
        }

        lastHitTime = Time.time;

        if (entries.Count > maxEntries)
            entries.RemoveAt(entries.Count - 1);

        feedText.text = string.Join("\n", entries);
    }
}