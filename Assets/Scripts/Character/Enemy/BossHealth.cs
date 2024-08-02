using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    private BossManager _bossManager;
    public int health;
    public int maxHealth;
    void Start()
    {
        health = maxHealth;
        _bossManager = GetComponent<BossManager>();
    }

    // Update is called once per frame
   public void TakeDamageToBoss(int amount)
    {
         health -= amount;
        _bossManager.OnDamageTaken();
    }
}
