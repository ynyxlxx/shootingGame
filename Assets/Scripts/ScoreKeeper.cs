using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour {

    public static int score { get; private set; }
    float lastEnemyKilledTime;
    int streakCount;
    float streakExpiryTime = 1;

    void Start() {
        Enemy.OnDeathStatic += OnEnemyKilled;
        score = 0;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }

    void OnEnemyKilled() {
        if (Time.time < lastEnemyKilledTime + streakExpiryTime) {
            streakCount++;
        } else {
            streakCount = 0;
        }

        lastEnemyKilledTime = Time.time;
        score += 5 + streakCount;
    }

    void OnPlayerDeath() {
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
}
