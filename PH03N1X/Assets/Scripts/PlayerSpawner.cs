using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSpawner : MonoBehaviour
{
    AudioSource src;

    [Header("Drag and Drop")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject readyText;
    [SerializeField] GameObject gameOverText;
    [SerializeField] GameObject grandTotalScoreText;
    [SerializeField] GameObject thisLifeScoreText;
    [SerializeField] GameObject totalAshesText;
    [SerializeField] TMP_Text totalAshesNumber;
    [SerializeField] TMP_Text livesRemainingText;

    [SerializeField] GameObject titleInstructions;
    [SerializeField] TMP_Text quitText;
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

    private float quitTimer = 3f;

    void Awake()
    {
        src = this.gameObject.GetComponent<AudioSource>();
    }

    void Start()
    {
        upgradeTextRefs = new TMP_Text[7] { maxBulletUpgradeText, fireRateUpgradeText, moveSpeedUpgradeText, turnSpeedUpgradeText, ashRateUpgradeText, maxComboUpgradeText, extraLifeText };
    }

    void Update()
    {
        if (canStartTheGame && Input.GetButtonDown("Submit"))
        {
            titleInstructions.SetActive(false);
            src.clip = SoundLibrary.GetClip("input_start");
            src.Play();
            StartTheGame();
        }
        else if (Input.GetButtonDown("Quit Game"))
        {
            quitTimer = 3f;
            quitText.text = "QUITTING...";
        }
        else if (Input.GetButton("Quit Game"))
        {
            quitTimer -= Time.deltaTime;
            if (quitTimer < 0f)
            {
                quitTimer = 0f;
                Application.Quit();
            }
        }
        else if (Input.GetButtonUp("Quit Game"))
        {
            quitText.text = "HOLD [ESC] TO QUIT";
        }
        else { /* Nothing */ }

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
        currentAshes = 0;
        livesBought = 0;
        UpdateLivesDisplay();
        ResetUpgradeLevels();
        Scorekeeper.ResetScorekeeper();
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
            tempShip.SetShipParameters(maxBulletsUpgradeTable[upgradeLevels[0]], fireRateUpgradeTable[upgradeLevels[1]], moveSpeedUpgradeTable[upgradeLevels[2]], turnSpeedUpgradeTable[upgradeLevels[3]]);
            Scorekeeper.SetMaxMultiplier(MaxComboMultiplierUpgradeTable[upgradeLevels[5]]);
            StartCoroutine(DisplayReadyText());
        }
    }

    private void ResetUpgradeLevels()
    {
        for (int i = 0; i < upgradeLevels.Length; ++i)
        {
            upgradeLevels[i] = 0;
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

        src.clip = SoundLibrary.GetClip("score_blip");
        int interval = (Scorekeeper.GetThisLifeScore() < 10 ? 1 : (int)Mathf.Pow(10f, Mathf.Log(Scorekeeper.GetThisLifeScore() / 10f, 10f)));
        while (Scorekeeper.GetThisLifeScore() > 0)
        {
            Scorekeeper.DecrementThisLifeScore(interval);
            src.Play();
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        int targetAshes = (currentAshes + ashesToTransfer);
        while (currentAshes < targetAshes)
        {
            currentAshes += interval;
            src.Play();
            if (currentAshes > targetAshes) { currentAshes = targetAshes; }
            totalAshesNumber.text = currentAshes.ToString("D7");
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        bool savingGrace = (livesLeft == 0 && currentAshes >= GetExtraLifeCost());
        livesRemainingText.text = (savingGrace ? "YOU CAN GET AN EXTRA LIFE!" : $"YOU HAVE {livesLeft} LI{(livesLeft > 1 || livesLeft == 0 ? "VES" : "FE")} REMAINING");
        livesRemainingText.gameObject.SetActive(true);
        if (savingGrace)
        {
            src.clip = SoundLibrary.GetClip("input_start");
            src.Play();
        }

        yield return new WaitForSeconds(2f);

        grandTotalScoreText.SetActive(false);
        thisLifeScoreText.SetActive(false);
        totalAshesText.SetActive(false);
        livesRemainingText.gameObject.SetActive(false);

        if (livesLeft > 0 || savingGrace)
        {
            // Show list of upgrades, wait for player to finish
            upgradeScreen.SetActive(true);
            int currentSelection = (savingGrace ? (upgradeTextRefs.Length - 1) : 0);
            bool isPlayerFinished = false;
            float previousYAxis = 0f;
            List<int> previousUpgrades = new List<int>();
            while (!isPlayerFinished)
            {
                ashesUpgradeDisplayText.text = $"ASHES LEFT: {currentAshes.ToString("D7")}";

                maxBulletUpgradeText.text = $"MAX BULLET UP  [LV. {upgradeLevels[0]}]: {(upgradeLevels[0] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[0]) : "MAX")}";
                fireRateUpgradeText.text = $"FIRE RATE UP   [LV. {upgradeLevels[1]}]: {(upgradeLevels[1] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[1]) : "MAX")}";
                moveSpeedUpgradeText.text = $"MOVE SPEED UP  [LV. {upgradeLevels[2]}]: {(upgradeLevels[2] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[2]) : "MAX")}";
                turnSpeedUpgradeText.text = $"TURN SPEED UP  [LV. {upgradeLevels[3]}]: {(upgradeLevels[3] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[3]) : "MAX")}";
                ashRateUpgradeText.text = $"ASH RATE UP    [LV. {upgradeLevels[4]}]: {(upgradeLevels[4] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[4]) : "MAX")}";
                maxComboUpgradeText.text = $"MAX COMBO UP   [LV. {upgradeLevels[5]}]: {(upgradeLevels[5] < maxUpgradeLevel ? GetUpgradeCost(upgradeLevels[5]) : "MAX")}";
                extraLifeText.text = $"EXTRA LIFE     [{livesLeft} / {maxLives}]: {(livesLeft < maxLives ? GetExtraLifeCost() : "MAX")}";

                for (int i = 0; i < upgradeTextRefs.Length; ++i)
                {
                    upgradeTextRefs[i].color = (i == currentSelection ? Color.red : Color.white);
                }

                if (Input.GetButtonDown("Submit") && livesLeft > 0)
                {
                    isPlayerFinished = true;
                    src.clip = SoundLibrary.GetClip("input_start");
                    src.Play();
                    continue;
                }
                else if ((!savingGrace || livesLeft > 0) && previousYAxis == 0f && Input.GetAxisRaw("Vertical") != 0f)
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

                    src.clip = SoundLibrary.GetClip("input_select");
                    src.Play();
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

                        src.clip = SoundLibrary.GetClip("input_confirm");
                        src.Play();
                    }
                    else
                    {
                        src.clip = SoundLibrary.GetClip("input_invalid");
                        src.Play();
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
                            if (savingGrace) { currentSelection = (upgradeTextRefs.Length - 1); }
                        }
                        else
                        {
                            upgradeLevels[selectionToUndo]--;
                            currentAshes += GetUpgradeCost(upgradeLevels[selectionToUndo]);
                        }

                        src.clip = SoundLibrary.GetClip("input_undo");
                        src.Play();
                    }
                    else
                    {
                        // Buzzer sound
                        src.clip = SoundLibrary.GetClip("input_invalid");
                        src.Play();
                    }
                }
                else { /* Nothing */ }

                previousYAxis = Input.GetAxisRaw("Vertical");
                yield return null;
            }
            upgradeScreen.SetActive(false);
        }

        // Undarken the screen
        yield return ScreenFade.SetFadeLerp(0f, 1f);

        // If there are lives remaining
        if (livesLeft > 0)
        {
            // Respawn player
            SpawnPlayer();
            StartCoroutine(RestartMusic());
        }
        else
        {
            // If not, game over
            _isGameOver = true;
            isGameOver = true;
            Scorekeeper.UpdateRecordedHighScore();
            yield return StartCoroutine(DisplayGameOverText());
            titleInstructions.SetActive(true);
            canStartTheGame = true;
        }

        isUpgrading = false;
        yield return null;
    }

    private IEnumerator RestartMusic()
    {
        while (!_playerRef.GetIsPlayerReady())
        {
            yield return null;
        }
        MusicPlayer.PlayMusic();
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
