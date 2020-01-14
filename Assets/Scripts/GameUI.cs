using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour {

    public Image fadePlane;
    public GameObject gameOverUI;

    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;

    Spawner spawner;

    void Awake() {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    void Start() {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
    }

    void OnGameOver() {
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
        Cursor.visible = true;
    }

    IEnumerator Fade(Color from, Color to, float time) {
        float speed = 1 / time;
        float percent = 0;

        while(percent < 1) {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    //UI输入
    public void StartNewGame() {
        SceneManager.LoadScene("Game");
    }

    //横幅控制
    public void OnNewWave(int waveNumber) {
        string enemyCountString = spawner.waves[waveNumber - 1].inifinite ? "INFINITE" : spawner.waves[waveNumber - 1].enemyCount + "";
        newWaveEnemyCount.text = "ENEMIES: " + enemyCountString;
        StartCoroutine(AnimateNewWaveBanner());
    }

    IEnumerator AnimateNewWaveBanner() {
        float delayTime = 1.5f;
        float speed = 2f;
        float animatePercent = 0;
        int dir = 1;

        float endDelayTime = Time.time + 1/speed + delayTime;

        while (animatePercent >= 0) {
            animatePercent += Time.deltaTime * speed * dir;
            if (animatePercent >= 1) {
                animatePercent = 1;
                if(Time.time > endDelayTime) {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-643, -389, animatePercent);
            yield return null;
        }

    }
}
