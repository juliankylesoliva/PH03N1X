using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmDirector : MonoBehaviour
{
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] float positionDegreeOffset = 90f;
    [SerializeField] bool reversePositionDirection = true;
    [SerializeField] Vector3 centerPoint;

    // TODO: Copy and paste these in a scriptable object, then keep a list of them to act as "rounds"
    [SerializeField] int seed = 0;

    [SerializeField] float startingRadius = 5f;
    [SerializeField] float radiusAmplitude = 1f;
    [SerializeField] float radiusChangeSineDegrees = 30f;
    [SerializeField] float retreatRadius = 3f;
    [SerializeField] float retreatTime = 5f;

    [SerializeField] float rotationSpeedDegreesAmplitude = 30f;
    [SerializeField] float rotationSpeedSineDegrees = 15f;

    [SerializeField, Range(4, 16)] int enemyGroupsToSpawn = 1;
    [SerializeField] float setupTime = 2f;

    [SerializeField, Range(1, 4)] int maxGroupSizePerAttack = 1;
    [SerializeField] float minAttackInterval = 3f;
    [SerializeField] float maxAttackInterval = 6f;
    [SerializeField] float attackIntervalChange = 0.25f;
    // ...End the scriptable object here

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
        // TODO: Put this all in an initialization function
        Random.InitState(seed);

        currentRadius = startingRadius;
        currentRadiusTheta = 0f;

        currentRotationSpeed = 0f;
        currentRotationSpeedTheta = 0f;
        currentRotationOffset = 0f;

        currentAttackInterval = minAttackInterval;
        currentIntervalChangeDirection = 1f;
        currentAttackTimer = currentAttackInterval;
        if (enemyRefs == null) { enemyRefs = new Dictionary<int, Flierwerk>(); }
        else { enemyRefs.Clear(); }

        positionVectors = new Vector2[(4 * enemyGroupsToSpawn)];
        UpdatePositionVectors();
        StartCoroutine(SpawnEnemyGroups());
    }

    void Update()
    {
        // TODO: If there are no enemies left, check if there was no miss, then reset and load the next wave

        if (!isSpawningEnemies && PlayerSpawner.GetPlayerRef() != null && PlayerSpawner.GetPlayerRef().GetIsPlayerReady()) { TryEnemyAttack(); }

        currentRadius = ((radiusAmplitude * Mathf.Sin(currentRadiusTheta)) + startingRadius) * Mathf.Lerp(1, retreatRadius, currentRadiusScaleLerp);
        currentRotationSpeed = (rotationSpeedDegreesAmplitude * Mathf.Sin(currentRotationSpeedTheta) * Mathf.Deg2Rad);
        currentRotationOffset += (currentRotationSpeed * Time.deltaTime);

        UpdatePositionVectors();

        currentRadiusTheta += (radiusChangeSineDegrees * Mathf.Deg2Rad * Time.deltaTime);
        currentRotationSpeedTheta += (rotationSpeedSineDegrees * Mathf.Deg2Rad * Time.deltaTime);
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

    private void TryEnemyAttack()
    {
        if (currentAttackTimer > 0f)
        {
            currentAttackTimer -= Time.deltaTime;
        }
        else
        {
            for (int i = 0; i < (maxGroupSizePerAttack > 1 ? Random.Range(1, maxGroupSizePerAttack + 1) : 1); ++i)
            {
                if (enemyRefs.Keys.Count == 0) { return; }
                int[] tempIDs = new int[enemyRefs.Keys.Count];
                enemyRefs.Keys.CopyTo(tempIDs, 0);

                Flierwerk chosenEnemy = null;
                enemyRefs.Remove(tempIDs[Random.Range(0, tempIDs.Length)], out chosenEnemy);
                if (chosenEnemy == null) { return; }

                chosenEnemy.Attack();
            }

            currentAttackInterval += (Mathf.Abs(attackIntervalChange) * currentIntervalChangeDirection);
            if (currentAttackInterval > maxAttackInterval)
            {
                currentAttackInterval = maxAttackInterval;
                currentIntervalChangeDirection *= -1f;
            }
            else if (currentAttackInterval < minAttackInterval)
            {
                currentAttackInterval = minAttackInterval;
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
                currentRadiusScaleLerp += (Time.deltaTime / retreatTime);
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
                currentRadiusScaleLerp -= (Time.deltaTime / setupTime);
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

            if (i < (positionVectors.Length - 1)) { yield return new WaitForSeconds(setupTime / positionVectors.Length); }
        }

        isSpawningEnemies = false;
        yield break;
    }
}
