using System;
using state;
using UnityEngine;
using TMPro;

namespace UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText; // Assign in Inspector

        public void Awake()
        {
            UpdateScore(0);
            GameStateManager.Instance.OnScoreChanged += UpdateScore;
        }

        public void UpdateScore(int score)
        {
            scoreText.text = "Score: " + score;
        }
    }
}