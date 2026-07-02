using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Time.timeScale = 1f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerGameOver(TurnManager.TurnPhase losingPlayer)
    {
        Debug.Log("Game Over Dimulai Dari Gameover Manager");

        if (TurnManager.instance != null)
        {
            TurnManager.instance.SetGameOver();
        }

        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if(winnerText != null)
        {
            if(losingPlayer == TurnManager.TurnPhase.Player1)
            {
                winnerText.text = "Player 2 Menang";
            }
            else
            {
                winnerText.text = "Player 1 Menang";
            }
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
