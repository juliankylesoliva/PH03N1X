using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField, Range(1, 5)] int startingLives = 3;
    [SerializeField] int maxUpgradeLevel = 3;
    [SerializeField] int baseUpgradeCost = 100;
    [SerializeField] float upgradeCostGrowthRate = 1.5f;
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

        // Wait for enemies and projectiles to stop moving
        // Darken the screen a bit
        // Show this life's score and the grand total score

        yield return new WaitForSeconds(2f);

        // If there are lives remaining
        if (livesLeft > 0)
        {
            // Decrease lives left by one
            --livesLeft;

            // convert this life's score to ashes while showing the grand score
            currentAshes += GetAshesFromLifeScore();

            // Show list of upgrades, wait for player to finish
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
        Scorekeeper.ResetThisLifeScore();
        return result;
    }
}
