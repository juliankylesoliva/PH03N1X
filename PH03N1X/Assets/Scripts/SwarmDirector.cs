using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmDirector : MonoBehaviour
{
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] float positionDegreeOffset = 90f;
    [SerializeField] bool reversePositionDirection = true;
    [SerializeField] Vector3 centerPoint;

    [SerializeField] SwarmDirections[] roundList;

    private int currentRound = -1;
    private SwarmDirections dirs = null;

    private bool isBetweenWaves = false;

    private bool isSpawningEnemies = false;

    private float currentRadius = 0f;
    private float currentRadiusTheta = 0f;
    private float currentRadiusScaleLerp = 0f;

    private float currentRotationSpeed = 0f;
    private float currentRotationSpeedTheta = 0f;
    private float currentRotationOffset = 0f;

    private float currentAttackInterval = 0f;
    private float currentIntervalChangeDirection = 1f;
    private float currentAttackTimer = 0f;
    private Dictionary<int, Flierwerk> enemyRefs = null;

    private Vector2[] positionVectors = null;

    void Start()
    {
        
    }

    void Update()
    {
        // TODO: If there are no enemies left, check if there was no miss, then reset and load the next wave
        if (!isBetweenWaves && !isSpawningEnemies && !AreEnemiesAlive() && !AreEnemiesActive() && PlayerSpawner.GetPlayerRef() != null)
        {
            StartCoroutine(BetweenWaves());
        }

        if (!isBetweenWaves && !isSpawningEnemies && PlayerSpawner.GetPlayerRef() != null && PlayerSpawner.GetPlayerRef().GetIsPlayerReady()) { TryEnemyAttack(); }

        if (dirs != null)
        {
            currentRadius = ((dirs.radiusAmplitude * Mathf.Sin(currentRadiusTheta)) + dirs.startingRadius) * Mathf.Lerp(1, dirs.retreatRadius, currentRadiusScaleLerp);
            currentRotationSpeed = (dirs.rotationSpeedDegreesAmplitude * Mathf.Sin(currentRotationSpeedTheta) * Mathf.Deg2Rad);
            currentRotationOffset += (currentRotationSpeed * Time.deltaTime);

            UpdatePositionVectors();

            currentRadiusTheta += (dirs.radiusChangeSineDegrees * Mathf.Deg2Rad * Time.deltaTime);
            currentRotationSpeedTheta += (dirs.rotationSpeedSineDegrees * Mathf.Deg2Rad * Time.deltaTime);
        }
    }

    public Vector3 GetPositionByID(int id)
    {
        Vector2 resultVec = positionVectors[id];
        return new Vector3(resultVec.x, resultVec.y, 0f);
    }

    public void RemoveEnemyFromRefs(int id)
    {
        if (enemyRefs.ContainsKey(id))
        {
            enemyRefs.Remove(id);
        }
    }

    public static bool AreEnemiesActive()
    {
        GameObject[] tempRefs = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject g in tempRefs)
        {
            if (g != null)
            {
                Flierwerk tempEnemy = g.GetComponent<Flierwerk>();
                if (tempEnemy != null && tempEnemy.GetIsAttacking())
                {
                    return true;
                }
            }
        }

        GameObject testProj = GameObject.FindWithTag("EnemyProjectile");
        return (testProj != null);
    }

    private IEnumerator BetweenWaves()
    {
        if (isBetweenWaves || isSpawningEnemies) { yield break; }
        isBetweenWaves = true;

        yield return new WaitForSeconds(1f);

        if (currentRound >= 0)
        {
            if (Scorekeeper.GetIsNoMiss())
            {
                Scorekeeper.AddNoMiss();
            }
        }

        Scorekeeper.ResetIsNoMiss();

        yield return new WaitForSeconds(2f);

        currentRound++;
        InitializeSwarm(roundList[(currentRound < roundList.Length ? currentRound : (roundList.Length - 1))]);

        isBetweenWaves = false;
        yield return null;
    }

    private bool AreEnemiesAlive()
    {
        return (enemyRefs != null && enemyRefs.Values.Count > 0);
    }

    private void InitializeSwarm(SwarmDirections d)
    {
        dirs = d;

        if (d.seed >= 0) { Random.InitState(d.seed); }

        currentRadius = d.startingRadius;
        currentRadiusTheta = 0f;

        currentRotationSpeed = 0f;
        currentRotationSpeedTheta = 0f;
        currentRotationOffset = 0f;

        currentAttackInterval = d.minAttackInterval;
        currentIntervalChangeDirection = 1f;
        currentAttackTimer = currentAttackInterval;
        if (enemyRefs == null) { enemyRefs = new Dictionary<int, Flierwerk>(); }
        else { enemyRefs.Clear(); }

        positionVectors = new Vector2[(4 * d.enemyGroupsToSpawn)];
        UpdatePositionVectors();
        StartCoroutine(SpawnEnemyGroups());
    }

    private void TryEnemyAttack()
    {
        if (currentAttackTimer > 0f)
        {
            currentAttackTimer -= Time.deltaTime;
        }
        else
        {
            for (int i = 0; i < (dirs.maxGroupSizePerAttack > 1 ? Random.Range(1, dirs.maxGroupSizePerAttack + 1) : 1); ++i)
            {
                if (enemyRefs == null || enemyRefs.Keys.Count == 0) { return; }
                int[] tempIDs = new int[enemyRefs.Keys.Count];
                enemyRefs.Keys.CopyTo(tempIDs, 0);

                Flierwerk chosenEnemy = null;
                enemyRefs.Remove(tempIDs[Random.Range(0, tempIDs.Length)], out chosenEnemy);
                if (chosenEnemy == null) { return; }

                chosenEnemy.Attack();
            }

            currentAttackInterval += (Mathf.Abs(dirs.attackIntervalChange) * currentIntervalChangeDirection);
            if (currentAttackInterval > dirs.maxAttackInterval)
            {
                currentAttackInterval = dirs.maxAttackInterval;
                currentIntervalChangeDirection *= -1f;
            }
            else if (currentAttackInterval < dirs.minAttackInterval)
            {
                currentAttackInterval = dirs.minAttackInterval;
                currentIntervalChangeDirection *= -1f;
            }
            else { /* Nothing */ }

            currentAttackTimer = currentAttackInterval;
        }
    }

    private void UpdateRadiusScaleLerp()
    {
        if (PlayerSpawner.GetPlayerRef() == null)
        {
            if (currentRadiusScaleLerp < 1f)
            {
                currentRadiusScaleLerp += (Time.deltaTime / dirs.retreatTime);
                if (currentRadiusScaleLerp > 1f)
                {
                    currentRadiusScaleLerp = 1f;
                }
            }
        }
        else
        {
            if (currentRadiusScaleLerp > 0f)
            {
                currentRadiusScaleLerp -= (Time.deltaTime / dirs.setupTime);
                if (currentRadiusScaleLerp < 0f)
                {
                    currentRadiusScaleLerp = 0f;
                }
            }
        }
    }

    private void UpdatePositionVectors()
    {
        float degreeInterval = (360f / positionVectors.Length);
        for (int i = 0; i < positionVectors.Length; ++i) // 0 = 3 o'clock, goes counter-clockwise
        {
            if (positionVectors[i] != null)
            {
                float currentTheta = ((reversePositionDirection ? -1f : 1f) * ((i * degreeInterval * Mathf.Deg2Rad) + currentRotationOffset + (positionDegreeOffset * Mathf.Deg2Rad)));
                positionVectors[i].x = (centerPoint.x + (currentRadius * Mathf.Cos(currentTheta)));
                positionVectors[i].y = (centerPoint.y + (currentRadius * Mathf.Sin(currentTheta)));
            }
        }
    }

    private IEnumerator SpawnEnemyGroups()
    {
        if (isSpawningEnemies) { yield break; }
        isSpawningEnemies = true;

        List<int> remainingNumbers = new List<int>();
        for (int i = 0; i < 4; ++i) { remainingNumbers.Add(i); }

        for (int i = 0; i < positionVectors.Length; ++i)
        {
            int rng = Random.Range(0, remainingNumbers.Count);
            GameObject objTemp = Instantiate(enemyPrefabs[remainingNumbers[rng]], GetPositionByID(i), Quaternion.identity);
            Flierwerk flwTemp = objTemp.GetComponent<Flierwerk>();
            flwTemp.SetDirector(this);
            flwTemp.SetPositionID(i);

            enemyRefs.Add(i, flwTemp);

            remainingNumbers.RemoveAt(rng);
            if (remainingNumbers.Count == 0)
            {
                for (int j = 0; j < 4; ++j) { remainingNumbers.Add(j); }
            }

            if (i < (positionVectors.Length - 1)) { yield return new WaitForSeconds(dirs.setupTime / positionVectors.Length); }
        }

        isSpawningEnemies = false;
        yield break;
    }
}
