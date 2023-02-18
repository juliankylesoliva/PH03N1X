using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Drag and Drop")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject readyText;
    [SerializeField] GameObject gameOverText;
    [SerializeField] GameObject grandTotalScoreText;
    [SerializeField] GameObject thisLifeScoreText;
    [SerializeField] GameObject totalAshesText;
    [SerializeField] TMP_Text totalAshesNumber;
    [SerializeField] TMP_Text livesRemainingText;

    [SerializeField] GameObject upgradeScreen;
    [SerializeField] TMP_Text ashesUpgradeDisplayText;
    [SerializeField] TMP_Text maxBulletUpgradeText;
    [SerializeField] TMP_Text fireRateUpgradeText;
    [SerializeField] TMP_Text moveSpeedUpgradeText;
    [SerializeField] TMP_Text turnSpeedUpgradeText;
    [SerializeField] TMP_Text ashRateUpgradeText;
    [SerializeField] TMP_Text maxComboUpgradeText;
    [SerializeField] TMP_Text extraLifeText;

    [SerializeField] GameObject lifeSymbolPrefab;
    [SerializeField] Transform livesDisplay;

    [Header("Game Parameters")]
    [SerializeField, Range(1, 5)] int startingLives = 3;
    [SerializeField, Range(5, 9)] int maxLives = 9;
    [SerializeField] int extraLifeBaseCost = 10000;
    [SerializeField] int extraLifeCostIncrement = 10000;
    [SerializeField] int maxExtraLifeCost = 70000;

    [SerializeField] int maxUpgradeLevel = 3;
    [SerializeField] int[] baseUpgradeCosts;
    [SerializeField] float upgradeCostGrowthRate = 1.05f;
    [SerializeField] int[] upgradeLevels = new int[6];

    [Header("Miscellaneous")]
    [SerializeField] float readyTextDuration = 3f;

    [Header("Upgrade Tables")]
    [SerializeField] int[] maxBulletsUpgradeTable;
    [SerializeField] float[] fireRateUpgradeTable;
    [SerializeField] float[] moveSpeedUpgradeTable;
    [SerializeField] float[] turnSpeedUpgradeTable;
    [SerializeField] float[] ScoreToAshRateUpgradeTable;
    [SerializeField] int[] MaxComboMultiplierUpgradeTable;

    private GameObject playerRef = null;
    private static ShipControl _playerRef = null;

    private bool canStartTheGame = true;

    private bool isGameOver = true;
    private static bool _isGameOver = true;
    private bool isUpgrading = false;
    private int livesLeft = 0;
    private int currentAshes = 0;
    private int livesBought = 0;

    private TMP_Text[] upgradeTextRefs;

    void Start()
    {
        upgradeTextRefs = new TMP_Text[7] { maxBulletUpgradeText, fireRateUpgradeText, moveSpeedUpgradeText, turnSpeedUpgradeText, ashRateUpgradeText, maxComboUpgradeText, extraLifeText };
    }

    void Update()
    {
        if (canStartTheGame && Input.GetButtonDown("Submit"))
        {
            StartTheGame();
        }

        if (!SwarmDirector.AreEnemiesActive() && !isUpgrading && !isGameOver && playerRef == null)
        {
            StartCoroutine(UpgradePhase());
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

    public static bool GetIsGameOver()
    {
        return _isGameOver;
    }

    private void StartTheGame()
    {
        canStartTheGame = false;
        isGameOver = false;
        _isGameOver = false;
        livesLeft = startingLives;
        UpdateLivesDisplay();
        CheckInitialUpgradeLevels();
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (livesLeft > 0 && playerRef == null)
        {
            livesLeft--;
            UpdateLivesDisplay();
            playerRef = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            ShipControl tempShip = playerRef.GetComponent<ShipControl>();
            _playerRef = tempShip;
            tempShip.SetShipParameters(maxBulletsUpgradeTable[upgradeLevels[0]], fireRateUpgradeTable[upgradeLevels[1]], moveSpeedUpgradeTable[upgradeLevels[2]], turnSpeedUpgradeTable[upgradeLevels[4]]);
            Scorekeeper.SetMaxMultiplier(MaxComboMultiplierUpgradeTable[upgradeLevels[5]]);
            StartCoroutine(DisplayReadyText());
        }
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

    private IEnumerator DisplayReadyText()
    {
        readyText.SetActive(true);
        yield return new WaitForSeconds(readyTextDuration);
        readyText.SetActive(false);
    }

    private IEnumerator DisplayGameOverText()
    {
        gameOverText.SetActive(true);
        yield return new WaitForSeconds(5f);
        gameOverText.SetActive(false);
    }

    private IEnumerator UpgradePhase()
    {
        isUpgrading = true;

        while (SwarmDirector.AreEnemiesActive())
        {
            yield return null;
        }

        yield return ScreenFade.SetFadeLerp(0.8f, 1f);

        // Show this life's score and the grand total score
        grandTotalScoreText.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        thisLifeScoreText.SetActive(true);
        yield return new WaitForSeconds(0.6f);
        totalAshesNumber.text = currentAshes.ToString("D7");
        totalAshesText.SetActive(true);
        yield return new WaitForSeconds(1f);

        int ashesToTransfer = GetAshesFromLifeScore();

        int interval = (Scorekeeper.GetThisLifeScore() < 10 ? 1 : (int)Mathf.Pow(10f, Mathf.Log(Scorekeeper.GetThisLifeScore() / 10f, 10f)));
        while (Scorekeeper.GetThisLifeScore() > 0)
        {
            Scorekeeper.DecrementThisLifeScore(interval);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        int targetAshes = (currentAshes + ashesToTransfer);
        while (currentAshes < targetAshes)
        {
            currentAshes += interval;
            if (currentAshes > targetAshes) { currentAshes = targetAshes; }
            totalAshesNumber.text = currentAshes.ToString("D7");
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        livesRemainingText.text = $"YOU HAVE {livesLeft} LI{(livesLeft > 1 || livesLeft == 0 ? "VES" : "FE")} REMAINING";
        livesRemainingText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        grandTotalScoreText.SetActive(false);
        thisLifeScoreText.SetActive(false);
        totalAshesText.SetActive(false);
        livesRemainingText.gameObject.SetActive(false);

        // Show list of upgrades, wait for player to finish
        upgradeScreen.SetActive(true);
        int currentSelection = 0;
        bool isPlayerFinished = false;
        float previousYAxis = 0f;
        List<int> previousUpgrades = new List<int>();
        while (!isPlayerFinished)
        {
            ashesUpgradeDisplayText.text = $"ASHES LEFT: {currentAshes.ToString("D7")}";

            maxBulletUpgradeText.text   = $"MAX BULLET UP [LV. {upgradeLevels[0]}]:  {(upgradeLevels[0] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[0]) : "MAX")}";
            fireRateUpgradeText.text    = $"FIRE RATE UP  [LV. {upgradeLevels[1]}]:  {(upgradeLevels[1] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[1]) : "MAX")}";
            moveSpeedUpgradeText.text   = $"MOVE SPEED UP [LV. {upgradeLevels[2]}]:  {(upgradeLevels[2] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[2]) : "MAX")}";
            turnSpeedUpgradeText.text   = $"TURN SPEED UP [LV. {upgradeLevels[3]}]:  {(upgradeLevels[3] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[3]) : "MAX")}";
            ashRateUpgradeText.text     = $"ASH RATE UP   [LV. {upgradeLevels[4]}]:  {(upgradeLevels[4] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[4]) : "MAX")}";
            maxComboUpgradeText.text    = $"MAX COMBO UP  [LV. {upgradeLevels[5]}]:  {(upgradeLevels[5] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[5]) : "MAX")}";
            extraLifeText.text          = $"EXTRA LIFE    [{livesLeft} LEFT]: {GetExtraLifeCost()}";

            for (int i = 0; i < upgradeTextRefs.Length; ++i)
            {
                upgradeTextRefs[i].color = (i == currentSelection ? Color.red : Color.white);
            }

            if (Input.GetButtonDown("Submit"))
            {
                isPlayerFinished = true;
                continue;
            }
            else if (previousYAxis == 0f && Input.GetAxisRaw("Vertical") != 0f)
            {
                if (Input.GetAxisRaw("Vertical") > 0f)
                {
                    currentSelection--;
                    if (currentSelection < 0)
                    {
                        currentSelection = (upgradeTextRefs.Length - 1);
                    }
                }
                else
                {
                    currentSelection++;
                    if (currentSelection >= upgradeTextRefs.Length)
                    {
                        currentSelection = 0;
                    }
                }
            }
            else if (Input.GetButtonDown("Confirm"))
            {
                bool isExtraLife = currentSelection >= (upgradeTextRefs.Length - 1);
                int cost = (!isExtraLife ? GetUpgradeCost(upgradeLevels[currentSelection]) : GetExtraLifeCost());
                if ((isExtraLife ? livesLeft < maxLives : upgradeLevels[currentSelection] < maxUpgradeLevel) && currentAshes >= cost)
                {
                    currentAshes -= cost;
                    if (isExtraLife)
                    {
                        livesLeft++;
                        livesBought++;
                        UpdateLivesDisplay();
                    }
                    else
                    {
                        upgradeLevels[currentSelection]++;
                    }
                    previousUpgrades.Insert(0, currentSelection);
                }
                else
                {
                    // Buzzer sound
                }
            }
            else if (Input.GetButtonDown("Cancel"))
            {
                if (previousUpgrades.Count > 0)
                {
                    int selectionToUndo = previousUpgrades[0];
                    previousUpgrades.RemoveAt(0);

                    if (selectionToUndo >= (upgradeTextRefs.Length - 1))
                    {
                        livesLeft--;
                        livesBought--;
                        currentAshes += GetExtraLifeCost();
                        UpdateLivesDisplay();
                    }
                    else
                    {
                        upgradeLevels[selectionToUndo]--;
                        currentAshes += GetUpgradeCost(upgradeLevels[selectionToUndo]);
                    }
                }
                else
                {
                    // Buzzer sound
                }
            }
            else { /* Nothing */ }

            previousYAxis = Input.GetAxisRaw("Vertical");
            yield return null;
        }
        upgradeScreen.SetActive(false);


        // Undarken the screen
        yield return ScreenFade.SetFadeLerp(0f, 1f);

        // If there are lives remaining
        if (livesLeft > 0)
        {
            // Respawn player
            SpawnPlayer();
        }
        else
        {
            // If not, game over
            _isGameOver = true;
            isGameOver = true;
            Scorekeeper.UpdateRecordedHighScore();
            yield return StartCoroutine(DisplayGameOverText());
            canStartTheGame = true;
        }

        isUpgrading = false;
        yield return null;
    }

    private int GetUpgradeCost(int level)
    {
        if (level >= maxUpgradeLevel) { return -1; }

        int sum = 0;
        foreach (int i in upgradeLevels)
        {
            sum += i;
        }

        int result = (int)(baseUpgradeCosts[level] * Mathf.Pow(upgradeCostGrowthRate, (float)sum));
        return (result - (result % 5));
    }

    private int GetExtraLifeCost()
    {
        int result = extraLifeBaseCost + (livesBought * extraLifeCostIncrement);
        return Mathf.Min(maxExtraLifeCost, result);
    }

    private int GetAshesFromLifeScore()
    {
        int result = (int)(Scorekeeper.GetThisLifeScore() * ScoreToAshRateUpgradeTable[upgradeLevels[4]]);
        result -= (result % 5);
        return result;
    }

    private void UpdateLivesDisplay()
    {
        int numLivesInDisplay = livesDisplay.childCount;
        if (numLivesInDisplay != livesLeft)
        {
            if (livesLeft > numLivesInDisplay)
            {
                int times = (livesLeft - numLivesInDisplay);
                for (int i = 0; i < times; ++i)
                {
                    Instantiate(lifeSymbolPrefab, livesDisplay);
                }
            }
            else
            {
                int times = (numLivesInDisplay - livesLeft);
                for (int i = 0; i < times; ++i)
                {
                    GameObject.Destroy(livesDisplay.GetChild(i).gameObject);
                }
            }
        }
    }
}
