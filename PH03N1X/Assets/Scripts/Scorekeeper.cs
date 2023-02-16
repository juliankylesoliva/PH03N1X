using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Scorekeeper : MonoBehaviour
{
    [SerializeField] TMP_Text p1ScoreNumber;
    [SerializeField] TMP_Text highScoreDisplayNumber;
    [SerializeField] TMP_Text comboMeter;

    [SerializeField] int grazePointValue = 5;
    private static int _grazePointValue = 0;

    [SerializeField] int streakToIncreaseMultiplier = 5;
    private static int _streakToIncreaseMultiplier = 0;

    private static int currentCombo = 0;
    private static int currentMuliplier = 1;
    private static int maxMultiplier = 2;

    private static int thisLifeScore = 0;
    private static int grandTotalScore = 0;

    private static int recordedHighScore = 20000;

    private static int highScoreDisplay = 0;

    void Awake()
    {
        _grazePointValue = grazePointValue;
        _streakToIncreaseMultiplier = streakToIncreaseMultiplier;
    }

    void Start()
    {
        highScoreDisplay = recordedHighScore;
    }

    void Update()
    {
        p1ScoreNumber.text = thisLifeScore.ToString("D7");
        highScoreDisplayNumber.text = highScoreDisplay.ToString("D7");
        comboMeter.text = $"{new string('|', currentCombo)} x{currentMuliplier}";
    }

    public static int AddGraze()
    {
        thisLifeScore += _grazePointValue;
        CheckHighScoreDisplay();
        return _grazePointValue;
    }

    public static int AddToScore(int points, bool useMultiplier = false)
    {
        int totalPoints = (points * (useMultiplier ? currentMuliplier : 1));
        thisLifeScore += totalPoints;
        CheckHighScoreDisplay();
        return totalPoints;
    }

    public static void AddToCombo()
    {
        if (PlayerSpawner.GetPlayerRef() == null ||  currentMuliplier == maxMultiplier) { return; }

        currentCombo++;
        if (currentCombo == _streakToIncreaseMultiplier)
        {
            if (currentMuliplier < maxMultiplier)
            {
                currentMuliplier++;
                currentCombo = 0;
            }
        }
    }

    public static void BreakCombo()
    {
        currentCombo = 0;
        currentMuliplier = 1;
    }

    public static void ResetHighScoreDisplay()
    {
        highScoreDisplay = recordedHighScore;
    }

    public static void CheckHighScoreDisplay()
    {
        if (grandTotalScore > highScoreDisplay)
        {
            highScoreDisplay = grandTotalScore;
        }
        else
        {
            highScoreDisplay = recordedHighScore;
        }
    }

    public static void SetMaxMultiplier(int m)
    {
        maxMultiplier = m;
    }

    public static int GetThisLifeScore()
    {
        return thisLifeScore;
    }

    public static int GetGrandTotalScore()
    {
        return grandTotalScore;
    }

    public static int GetHighScoreDisplay()
    {
        return highScoreDisplay;
    }

    public static int GetRecordedHighScore()
    {
        return recordedHighScore;
    }
}
