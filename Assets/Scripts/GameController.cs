using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject hazard;
    public Vector3 spawnValue;
    public int hazardCount;
    public float individualSpawnWait;
    public float groupSpawnWait;
    public float startWait;
    public float waveWait;
    public int health;
    public int winScore;


    public GUIText scoreText;
    public GUIText restartText;
    public GUIText gameOverText;
    public GUIText healthText;

    private bool gameOver;
    private bool restart;
    private int score;
    private bool isWin;

    public GameObject playerExplosion;

    private GameObject mainCameraObj;
    private Vector3 initialCameraPosition;

    private void Start()
    {
        gameOver = false;
        restart = false;
        isWin = false;
        restartText.text = "";
        gameOverText.text = "";
        score = 0;
        UpdateScore();
        UpdateHealth();
        mainCameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        initialCameraPosition = mainCameraObj.transform.position;
        StartCoroutine(SpawnWaves());
    }

    private void Update()
    {
        if (restart)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        float shakeForce = (cameraShakeStartTime + 0.3f - Time.time) / 0.3f;
        if (shakeForce < 0)
        {
            mainCameraObj.transform.position = initialCameraPosition;
        }
        else
        {
            var shakeRatio = Random.insideUnitSphere * shakeForce * 1.0f;
            mainCameraObj.transform.position = initialCameraPosition + shakeRatio;
        }
    }

    IEnumerator SpawnWaves()
    {
        int difficulty = 2;
        int stoneCount = 1;

        yield return new WaitForSeconds(startWait);
        while (true)
        {
            for (int i = 0; i < stoneCount; i++)
            {
                Vector3 spawnPosition = new Vector3(Random.Range(-spawnValue.x, spawnValue.x), spawnValue.y, spawnValue.z);
                Quaternion spawnRotation = Quaternion.identity;
                Instantiate(hazard, spawnPosition, spawnRotation);
                if (difficulty % 5 == 0)
                {
                    yield return new WaitForSeconds(groupSpawnWait);
                }
                else
                {
                    yield return new WaitForSeconds(individualSpawnWait);
                }
            }
            yield return new WaitForSeconds(waveWait);

            if (gameOver)
            {
                restartText.text = "Press 'R' for Restart";
                restart = true;
                break;
            }
            else
            {
                if (difficulty % 5 == 0)
                {
                    stoneCount += 4;
                }
                else if (difficulty % 2 == 0)
                {
                    stoneCount++;
                }
                difficulty++;
            }
        }
    }

    public void AddScore(int newScoreValue)
    {
        score += newScoreValue;
        UpdateScore();
        if (!isWin && score >= winScore)
        {
            Win();
        }
    }

    void UpdateScore()
    {
        scoreText.text = "Score: " + score;
    }

    void UpdateHealth()
    {
        healthText.text = "Health: " + health;
    }

    public void GameOver()
    {
        if (!isWin)
        {
            gameOverText.text = "Game Over!";
        }
        gameOver = true;
    }

    public void Win()
    {
        gameOverText.text = "You Win!!!";
        isWin = true;
    }

    public void DecreaseHealth(int damage)
    {
        health -= damage;
        if (health < 0)
        {
            health = 0;
        }

        UpdateHealth();
        if (IsPlayerDeath())
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Instantiate(playerExplosion, player.transform.position, player.transform.rotation);
                Destroy(player);
            }
            GameOver();
        }

    }

    public bool IsPlayerDeath()
    {
        return health <= 0;
    }

    private float cameraShakeStartTime = -100f;

    public void ShakeCamera()
    {
        cameraShakeStartTime = Time.time;
    }

}
