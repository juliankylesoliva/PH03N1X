using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwarmDirector : MonoBehaviour
{
    [SerializeField] TMP_Text waveTextCenter;
    [SerializeField] TMP_Text waveTextCorner;

    [SerializeField] TMP_Text waveResultsHeaderText;
    [SerializeField] TMP_Text resultsLivesLostText;
    [SerializeField] TMP_Text resultsShotsMissedText;
    [SerializeField] TMP_Text resultsEscapeesText;
    [SerializeField] TMP_Text resultsFinalEvaluationText;

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
        if (!PlayerSpawner.GetIsGameOver())
        {
            if (!isBetweenWaves && !isSpawningEnemies && !AreEnemiesAlive() && !AreEnemiesActive() && PlayerSpawner.GetPlayerRef() != null && PlayerSpawner.GetPlayerRef().GetIsPlayerReady())
            {
                StartCoroutine(BetweenWaves());
            }

            if (!isBetweenWaves && !isSpawningEnemies && PlayerSpawner.GetPlayerRef() != null && PlayerSpawner.GetPlayerRef().GetIsPlayerReady()) { TryEnemyAttack(); }

            if (dirs != null && positionVectors != null)
            {
                currentRadius = ((dirs.radiusAmplitude * Mathf.Sin(currentRadiusTheta)) + dirs.startingRadius) * Mathf.Lerp(1, dirs.retreatRadius, currentRadiusScaleLerp);
                currentRotationSpeed = (dirs.rotationSpeedDegreesAmplitude * Mathf.Sin(currentRotationSpeedTheta) * Mathf.Deg2Rad);
                currentRotationOffset += (currentRotationSpeed * Time.deltaTime);

                UpdatePositionVectors();

                currentRadiusTheta += (dirs.radiusChangeSineDegrees * Mathf.Deg2Rad * Time.deltaTime);
                currentRotationSpeedTheta += (dirs.rotationSpeedSineDegrees * Mathf.Deg2Rad * Time.deltaTime);
            }
        }
        else
        {
            if (enemyRefs != null && enemyRefs.Values.Count > 0 && positionVectors != null && currentRadiusScaleLerp < 1f)
            {
                currentRadiusScaleLerp += (Time.deltaTime / dirs.retreatTime);
                currentRadius = ((dirs.radiusAmplitude * Mathf.Sin(currentRadiusTheta)) + dirs.startingRadius) * Mathf.Lerp(1, dirs.retreatRadius, currentRadiusScaleLerp);
                UpdatePositionVectors();
                if (currentRadiusScaleLerp >= 1f)
                {
                    enemyRefs.Clear();
                    positionVectors = null;
                    GameObject[] tempEnemyRefs = GameObject.FindGameObjectsWithTag("Enemy");
                    for (int i = 0; i < tempEnemyRefs.Length; ++i)
                    {
                        GameObject.Destroy(tempEnemyRefs[i]);
                    }
                    currentRadiusScaleLerp = 0f;
                    waveTextCorner.gameObject.SetActive(false);
                    currentRound = -1;
                }
            }
        }
    }

    public Vector3 GetPositionByID(int id)
    {
        if (positionVectors == null || positionVectors[id] == null) { return Vector3.zero; }
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
        GameObject testProj2 = GameObject.FindWithTag("PlayerProjectile");
        return (testProj != null || testProj2 != null);
    }

    public static bool AreEnemiesAlive()
    {
        GameObject[] tempRefs = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject g in tempRefs)
        {
            if (g != null)
            {
                return true;
            }
        }
        return false;
    }

    public static bool AreEnemiesAttacking()
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
        return false;
    }

    private IEnumerator BetweenWaves()
    {
        if (isBetweenWaves || isSpawningEnemies) { yield break; }
        isBetweenWaves = true;

        yield return new WaitForSeconds(1f);

        if (currentRound >= 0)
        {
            yield return ScreenFade.SetFadeLerp(0.8f, 1f);

            waveResultsHeaderText.gameObject.SetActive(true);
            waveResultsHeaderText.text = $"WAVE {currentRound + 1} RESULTS";
            yield return new WaitForSeconds(1f);
            resultsLivesLostText.gameObject.SetActive(true);
            resultsLivesLostText.text = $"  LIVES LOST:\t{Scorekeeper.GetLivesLost()}";
            yield return new WaitForSeconds(0.5f);
            resultsShotsMissedText.gameObject.SetActive(true);
            resultsShotsMissedText.text = $"SHOTS MISSED:\t{Scorekeeper.GetShotsMissed()}";
            yield return new WaitForSeconds(0.5f);
            resultsEscapeesText.gameObject.SetActive(true);
            resultsEscapeesText.text = $"    ESCAPEES:\t{Scorekeeper.GetEscapees()}";
            yield return new WaitForSeconds(1.5f);
            resultsFinalEvaluationText.gameObject.SetActive(true);
            resultsFinalEvaluationText.text = (Scorekeeper.GetIsNoMiss() ? $"PERFECT! {Scorekeeper.AddNoMiss()} POINTS!" : "NO BONUS");
            yield return new WaitForSeconds(3f);

            waveResultsHeaderText.gameObject.SetActive(false);
            resultsLivesLostText.gameObject.SetActive(false);
            resultsShotsMissedText.gameObject.SetActive(false);
            resultsEscapeesText.gameObject.SetActive(false);
            resultsFinalEvaluationText.gameObject.SetActive(false);

            yield return ScreenFade.SetFadeLerp(0f, 1f);
        }

        Scorekeeper.ResetNoMissConditions();

        currentRound++;
        
        waveTextCenter.text = $"WAVE {currentRound + 1}";
        waveTextCorner.text = $"W{currentRound + 1}";

        waveTextCorner.gameObject.SetActive(true);

        waveTextCenter.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        waveTextCenter.gameObject.SetActive(false);

        InitializeSwarm(roundList[(currentRound < roundList.Length ? currentRound : (roundList.Length - 1))]);
        isBetweenWaves = false;
        yield return null;
    }

    private void InitializeSwarm(SwarmDirections d)
    {
        dirs = d;
        currentRadiusScaleLerp = 0f;

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
            if (AreEnemiesAttacking()) { return; }

            int numEnemiesToChoose = (dirs.maxGroupSizePerAttack > 1 ? Random.Range(1, dirs.maxGroupSizePerAttack + 1) : 1);
            for (int i = 0; i < numEnemiesToChoose; ++i)
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
