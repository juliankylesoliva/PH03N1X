using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] int maxUpgradeLevel = 3;
    [SerializeField] int[] upgradeLevels = new int[6];

    private GameObject playerRef = null;
    private static ShipControl _playerRef = null;

    [Header("Upgrade Tables")]
    [SerializeField] int[] maxBulletsUpgradeTable;
    [SerializeField] float[] fireRateUpgradeTable;
    [SerializeField] float[] moveSpeedUpgradeTable;
    [SerializeField] float[] turnSpeedUpgradeTable;
    [SerializeField] float[] ScoreToAshRateUpgradeTable;
    [SerializeField] float[] MaxComboMultiplierUpgradeTable;

    void Start()
    {
        CheckInitialUpgradeLevels();
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (playerRef == null)
        {
            playerRef = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            ShipControl tempShip = playerRef.GetComponent<ShipControl>();
            _playerRef = tempShip;
            tempShip.SetShipParameters(maxBulletsUpgradeTable[upgradeLevels[0]], fireRateUpgradeTable[upgradeLevels[1]], moveSpeedUpgradeTable[upgradeLevels[2]], turnSpeedUpgradeTable[upgradeLevels[4]]);
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
}
