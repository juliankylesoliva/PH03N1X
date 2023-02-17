using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Drag and Drop")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Image screenFade;
    [SerializeField] GameObject grandTotalScoreText;
    [SerializeField] GameObject thisLifeScoreText;
    [SerializeField] GameObject totalAshesText;
    [SerializeField] TMP_Text totalAshesNumber;
    [SerializeField] TMP_Text livesRemainingText;

    [SerializeField] TMP_Text maxBulletUpgradeText;
    [SerializeField] TMP_Text fireRateUpgradeText;
    [SerializeField] TMP_Text moveSpeedUpgradeText;
    [SerializeField] TMP_Text turnSpeedUpgradeText;
    [SerializeField] TMP_Text ashRateUpgradeText;
    [SerializeField] TMP_Text maxComboUpgradeText;
    [SerializeField] TMP_Text extraLifeText;
    [SerializeField] TMP_Text repawnText;
    [SerializeField] TMP_Text undoText;

    [Header("Game Parameters")]
    [SerializeField, Range(1, 5)] int startingLives = 3;
    [SerializeField] int extraLifeBaseCost = 10000;
    [SerializeField] int extraLifeCostIncrement = 10000;
    [SerializeField] int maxExtraLifeCost = 70000;

    [SerializeField] int maxUpgradeLevel = 3;
    [SerializeField] int[] baseUpgradeCosts;
    [SerializeField] float upgradeCostGrowthRate = 1.05f;
    [SerializeField] int[] upgradeLevels = new int[6];

    [Header("Upgrade Tables")]
    [SerializeField] int[] maxBulletsUpgradeTable;
    [SerializeField] float[] fireRateUpgradeTable;
    [SerializeField] float[] moveSpeedUpgradeTable;
    [SerializeField] float[] turnSpeedUpgradeTable;
    [SerializeField] float[] ScoreToAshRateUpgradeTable;
    [SerializeField] int[] MaxComboMultiplierUpgradeTable;

    private GameObject playerRef = null;
    private static ShipControl _playerRef = null;

    private bool isGameOver = false;
    private bool isUpgrading = false;
    private int livesLeft = 0;
    private int currentAshes = 0;
    private int livesBought = 0;

    void Start()
    {
        livesLeft = startingLives;
        CheckInitialUpgradeLevels();
        SpawnPlayer();
    }

    void Update()
    {
        if (!isGameOver && playerRef == null && !isUpgrading)
        {
            StartCoroutine(UpgradePhase());
        }
    }

    public void SpawnPlayer()
    {
        if (livesLeft > 0 && playerRef == null)
        {
            playerRef = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            ShipControl tempShip = playerRef.GetComponent<ShipControl>();
            _playerRef = tempShip;
            tempShip.SetShipParameters(maxBulletsUpgradeTable[upgradeLevels[0]], fireRateUpgradeTable[upgradeLevels[1]], moveSpeedUpgradeTable[upgradeLevels[2]], turnSpeedUpgradeTable[upgradeLevels[4]]);
            Scorekeeper.SetMaxMultiplier(MaxComboMultiplierUpgradeTable[upgradeLevels[5]]);
        }
    }

    public void IncrementUpgradeLevelArray(int index)
    {
        if (index >= 0 && index < upgradeLevels.Length && upgradeLevels[index] < maxUpgradeLevel)
        {
            upgradeLevels[index]++;
        }
    }

    public static ShipControl GetPlayerRef()
    {
        return _playerRef;
    }

    private void CheckInitialUpgradeLevels()
    {
        for (int i = 0; i < upgradeLevels.Length; ++i)
        {
            if (upgradeLevels[i] < 0) { upgradeLevels[i] = 0; }
            else if (upgradeLevels[i] > maxUpgradeLevel) { upgradeLevels[i] = maxUpgradeLevel; }
            else { /* Nothing */ }
        }
    }

    private int GetNumberOfUpgradesBought()
    {
        int sum = 0;
        foreach (int i in upgradeLevels)
        {
            sum += i;
        }
        return sum;
    }

    private IEnumerator UpgradePhase() // TODO
    {
        isUpgrading = true;

        while (SwarmDirector.AreEnemiesActive())
        {
            yield return null;
        }

        float currentLerp = 0f;
        Color colorOne = new Color(0f, 0f, 0f, 0f);
        Color colorTwo = new Color(0f, 0f, 0f, 0.8f);
        while (currentLerp < 1f)
        {
            currentLerp += Time.deltaTime;
            if (currentLerp > 1f) { currentLerp = 1f; }
            screenFade.color = Color.Lerp(colorOne, colorTwo, currentLerp);
            yield return null;
        }

        // Show this life's score and the grand total score
        grandTotalScoreText.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        thisLifeScoreText.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        totalAshesText.SetActive(true);
        yield return new WaitForSeconds(1f);

        int ashesToTransfer = GetAshesFromLifeScore();

        while (Scorekeeper.GetThisLifeScore() > 0)
        {
            Scorekeeper.DecrementThisLifeScore(10);
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        while (ashesToTransfer > 0)
        {
            ashesToTransfer -= 5;
            currentAshes += 5;
            totalAshesNumber.text = currentAshes.ToString("D7");
            yield return null;
        }
        yield return new WaitForSeconds(1f);

        livesRemainingText.gameObject.SetActive(true);
        livesRemainingText.text = $"YOU HAVE {livesLeft} LI{(livesLeft > 1 || livesLeft == 0 ? "VES" : "FE")} REMAINING";
        yield return new WaitForSeconds(0.5f);
        if (livesLeft > 0) { --livesLeft; }
        livesRemainingText.text = $"YOU HAVE {livesLeft} LI{(livesLeft > 1 || livesLeft == 0 ? "VES" : "FE")} REMAINING";

        yield return new WaitForSeconds(2f);

        grandTotalScoreText.SetActive(false);
        thisLifeScoreText.SetActive(false);
        totalAshesText.SetActive(false);
        livesRemainingText.gameObject.SetActive(false);

        // Show list of upgrades, wait for player to finish
        bool isPlayerFinished = false;
        while (!isPlayerFinished)
        {



            yield return null;
        }


        // Undarken the screen
        while (currentLerp > 0f)
        {
            currentLerp -= Time.deltaTime;
            if (currentLerp < 0f) { currentLerp = 0f; }
            screenFade.color = Color.Lerp(colorOne, colorTwo, currentLerp);
            yield return null;
        }

        // If there are lives remaining
        if (livesLeft > 0)
        {
            // Respawn player
            SpawnPlayer();
        }
        else
        {
            // If not, game over
            isGameOver = true;
        }

        isUpgrading = false;
        yield return null;
    }

    private int GetAshesFromLifeScore()
    {
        int result = (int)(Scorekeeper.GetThisLifeScore() * ScoreToAshRateUpgradeTable[upgradeLevels[4]]);
        result -= (result % 5);
        return result;
    }
}
