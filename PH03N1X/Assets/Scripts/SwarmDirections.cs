using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SwarmDirections", menuName = "ScriptableObjects/SwarmDirections", order = 1)]
public class SwarmDirections : ScriptableObject
{
    public int seed = 0;

    [HideInInspector] public float startingRadius = 5f;
    [HideInInspector] public float radiusAmplitude = 1f;
    public float radiusChangeSineDegrees = 30f;
    [HideInInspector] public float retreatRadius = 3f;
    [HideInInspector] public float retreatTime = 5f;

    public float rotationSpeedDegreesAmplitude = 30f;
    public float rotationSpeedSineDegrees = 15f;

    [Range(4, 16)] public int enemyGroupsToSpawn = 4;
    [HideInInspector] public float setupTime = 2f;

    [Range(1, 4)] public int maxGroupSizePerAttack = 1;
    public float minAttackInterval = 3f;
    public float maxAttackInterval = 6f;
    public float attackIntervalChange = 0.25f;
}
