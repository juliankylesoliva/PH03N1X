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

    [SerializeField] TMP_Text grandTotalScoreNumber;
    [SerializeField] TMP_Text thisLifeScoreNumber;

    [SerializeField] int noMissBonus = 5000;
    private static int _noMissBonus = 0;

    [SerializeField] int grazePointValue = 5;
    private static int _grazePointValue = 0;

    [SerializeField] int streakToIncreaseMultiplier = 5;
    private static int _streakToIncreaseMultiplier = 0;

    private static int currentCombo = 0;
    private static int currentMuliplier = 1;
    private static int maxMultiplier = 2;

    private static int livesLostThisWave = 0;
    private static int shotsMissedThisWave = 0;
    private static int escapeesThisWave = 0;

    private static int thisLifeScore = 0;
    private static int grandTotalScore = 0;

    private static int recordedHighScore = 20000;

    private static int highScoreDisplay = 0;

    private bool showGrandTotal = false;

    void Awake()
    {
        _noMissBonus = noMissBonus;
        _grazePointValue = grazePointValue;
        _streakToIncreaseMultiplier = streakToIncreaseMultiplier;
    }

    void Start()
    {
        ResetHighScoreDisplay();
    }

    void Update()
    {
        if (!PlayerSpawner.GetIsGameOver() && Input.GetButtonDown("Toggle Score Display")){ showGrandTotal = !showGrandTotal; }

        p1ScoreNumber.gameObject.SetActive(!PlayerSpawner.GetIsGameOver());
        p1ScoreNumber.text = (PlayerSpawner.GetIsGameOver() || showGrandTotal ? grandTotalScore : thisLifeScore).ToString("D7");
        highScoreDisplayNumber.text = highScoreDisplay.ToString("D7");

        comboMeter.gameObject.SetActive(!PlayerSpawner.GetIsGameOver());
        comboMeter.text = $"{new string('|', currentCombo)}x{currentMuliplier}";

        grandTotalScoreNumber.text = grandTotalScore.ToString("D7");
        thisLifeScoreNumber.text = thisLifeScore.ToString("D7");
    }

    public static int AddGraze()
    {
        return AddToScore(_grazePointValue);
    }

    public static int AddNoMiss()
    {
        return AddToScore(_noMissBonus);
    }

    public static int AddToScore(int points, bool useMultiplier = false)
    {
        int totalPoints = (points * (useMultiplier ? currentMuliplier : 1));
        thisLifeScore += totalPoints;
        grandTotalScore += totalPoints;
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
                if (currentMuliplier < maxMultiplier) { currentCombo = 0; }
            }
        }
    }

    public static void BreakCombo()
    {
        currentCombo = 0;
        currentMuliplier = 1;
    }

    public static bool GetIsNoMiss()
    {
        return (livesLostThisWave <= 0 && shotsMissedThisWave <= 0 && escapeesThisWave <= 0);
    }

    public static void IncrementLivesLost()
    {
        livesLostThisWave++;
    }

    public static void IncrementShotsMissed()
    {
        shotsMissedThisWave++;
    }

    public static void IncrementEscapees()
    {
        escapeesThisWave++;
    }

    public static int GetLivesLost()
    {
        return livesLostThisWave;
    }

    public static int GetShotsMissed()
    {
        return shotsMissedThisWave;
    }

    public static int GetEscapees()
    {
        return escapeesThisWave;
    }

    public static void ResetNoMissConditions()
    {
        livesLostThisWave = 0;
        shotsMissedThisWave = 0;
        escapeesThisWave = 0;
    }

    public static void ResetHighScoreDisplay()
    {
        highScoreDisplay = recordedHighScore;
    }

    public static bool UpdateRecordedHighScore()
    {
        if (grandTotalScore > recordedHighScore)
        {
            recordedHighScore = grandTotalScore;
            ResetHighScoreDisplay();
            return true;
        }
        return false;
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

    public static void DecrementThisLifeScore(int n)
    {
        thisLifeScore -= n;
        if (thisLifeScore < 0) { thisLifeScore = 0; }
    }

    public static void ResetThisLifeScore()
    {
        thisLifeScore = 0;
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

    public static void ResetScorekeeper()
    {
        BreakCombo();
        thisLifeScore = 0;
        grandTotalScore = 0;
    }
}
