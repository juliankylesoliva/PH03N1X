using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmDirector : MonoBehaviour
{
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] Vector3 centerPoint;

    [SerializeField] int seed = 0;

    [SerializeField] float startingRadius = 5f;
    [SerializeField] float radiusAmplitude = 1f;
    [SerializeField] float radiusChangeSineDegrees = 30f;

    [SerializeField] float rotationSpeedDegreesAmplitude = 30f;
    [SerializeField] float rotationSpeedSineDegrees = 15f;

    [SerializeField, Range(4, 16)] int enemyGroupsToSpawn = 1;

    [SerializeField] float minAttackInterval = 3f;
    [SerializeField] float maxAttackInterval = 6f;
    [SerializeField] float attackIntervalChange = 0.25f;

    private bool isSpawningEnemies = false;

    private float currentRadius = 0f;
    private float currentRadiusTheta = 0f;

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
        if (!isSpawningEnemies && PlayerSpawner.GetPlayerRef() != null && PlayerSpawner.GetPlayerRef().GetIsPlayerReady()) { TryEnemyAttack(); }

        currentRadius = ((radiusAmplitude * Mathf.Sin(currentRadiusTheta)) + startingRadius);
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
            // Command a random enemy to attack
            if (enemyRefs.Values.Count == 0) { return; }
            Flierwerk[] tempRefs = new Flierwerk[enemyRefs.Values.Count];
            enemyRefs.Values.CopyTo(tempRefs, 0);

            Flierwerk chosenEnemy = tempRefs[Random.Range(0, tempRefs.Length)];
            if (chosenEnemy == null) { return; }

            chosenEnemy.Attack();

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

    private void UpdatePositionVectors()
    {
        float degreeInterval = (360f / positionVectors.Length);
        for (int i = 0; i < positionVectors.Length; ++i) // 0 = 3 o'clock, goes counter-clockwise
        {
            if (positionVectors[i] != null)
            {
                float currentTheta = ((i * degreeInterval * Mathf.Deg2Rad) + currentRotationOffset);
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

            for (int j = 0; j < 3; ++j) { yield return null; }
        }

        isSpawningEnemies = false;
        yield break;
    }
}
