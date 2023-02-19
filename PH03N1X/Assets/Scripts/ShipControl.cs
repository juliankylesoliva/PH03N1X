using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControl : MonoBehaviour
{
    /* COMPONENTS */
    Camera playerCam;

    /* EDITOR PARAMETERS */
    [SerializeField] GameObject playerBulletPrefab;
    [SerializeField] Transform bulletSpawnLocation;
    [SerializeField] GameObject reticlePrefab;

    [SerializeField] float readyPlayerDuration = 3f;
    [SerializeField] Vector2 startCenter;
    [SerializeField] float boundaryRadius = 4f;

    [Header("Combat Parameters")]
    [SerializeField] float fireRate = 0.8f;

    [Header("Movement Parameters")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float turnSpeed = 1f;

    /* PRIVATE VARIABLES */
    private float currentReadyPlayerTimer = 0f;
    private float moveTimeLeft = 0f;
    private Vector2 moveDirection = Vector2.zero;
    private GameObject[] bulletRefs = null;
    private float fireRateTimer = 0f;
    private SpriteRenderer reticleRef;

    void Awake()
    {
        playerCam = Camera.main;
        GameObject tempObj = Instantiate(reticlePrefab, Vector3.zero, Quaternion.identity);
        if (tempObj != null) { reticleRef = tempObj.GetComponent<SpriteRenderer>(); }
    }

    void Start()
    {
        currentReadyPlayerTimer = readyPlayerDuration;
        this.transform.position = new Vector3(startCenter.x, startCenter.y, 0f);
    }

    void Update()
    {
        TickReadyPlayerTimer();
        SetAimDirection();
        SetMovementDirection();
        TickFireRateTimer();
        DisplayReticle();
        FireProjectile();
        GoToNextPosition();
    }

    /* PUBLIC FUNCTIONS */
    public void SetShipParameters(int bullets, float rate, float move, float turn)
    {
        bulletRefs = new GameObject[bullets];
        fireRate = rate;
        moveSpeed = move;
        turnSpeed = turn;
    }

    public bool GetIsPlayerReady()
    {
        return currentReadyPlayerTimer <= 0f;
    }

    public void KillShip()
    {
        StartCoroutine(KillShipCR());
    }

    /* HELPER FUNCTIONS */
    private void TickReadyPlayerTimer()
    {
        if (currentReadyPlayerTimer > 0f)
        {
            currentReadyPlayerTimer -= Time.deltaTime;
            if (currentReadyPlayerTimer < 0f)
            {
                currentReadyPlayerTimer = 0f;
            }
        }
    }

    private void FireProjectile()
    {
        if (CanFireAShot() && Input.GetButtonDown("Fire"))
        {
            bulletRefs[FindEmptyBulletSlot()] = Instantiate(playerBulletPrefab, bulletSpawnLocation.position, this.transform.rotation);
            SetFireRateTimer();
        }
    }

    private void DisplayReticle()
    {
        if (reticleRef != null && CanFireAShot())
        {
            reticleRef.color = Color.white;
            Vector3 mouseLocation = playerCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = (mouseLocation - this.transform.position);
            direction.z = 0f;
            float radius = direction.magnitude;
            reticleRef.transform.position = (this.transform.position + (this.transform.up * radius));
        }
        else if (reticleRef != null)
        {
            reticleRef.color = Color.clear;
        }
        else { /* Nothing */ }
    }

    private void SetMovementDirection()
    {
        if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f)
        {
            moveTimeLeft = 0f;
            moveDirection.x = Input.GetAxisRaw("Horizontal");
            moveDirection.y = Input.GetAxisRaw("Vertical");
            moveDirection = moveDirection.normalized;
        }
        else if (Input.GetButtonDown("Move"))
        {
            Vector3 mouseLocation = playerCam.ScreenToWorldPoint(Input.mousePosition);
            mouseLocation.z = 0f;
            Vector3 direction = (mouseLocation - this.transform.position);
            float distance = direction.magnitude;
            direction = direction.normalized;
            moveDirection.x = direction.x;
            moveDirection.y = direction.y;
            moveTimeLeft = (distance / moveSpeed);
        }
        else if (moveTimeLeft > 0f)
        {
            moveTimeLeft -= Time.deltaTime;
            if (moveTimeLeft < 0f)
            {
                ResetMoveTimerAndDirection();
            }
        }
        else
        {
            ResetMoveTimerAndDirection();
        }
    }

    private void GoToNextPosition()
    {
        if (moveDirection.x != 0f || moveDirection.y != 0f)
        {
            Vector3 nextPosition = this.transform.position;
            nextPosition += ((new Vector3(moveDirection.x, moveDirection.y, 0f)) * moveSpeed * Time.deltaTime);

            if ((Vector2.Distance(nextPosition, startCenter)) <= boundaryRadius)
            {
                this.transform.position = nextPosition;
            }
            else
            {
                ResetMoveTimerAndDirection();
            }
        }
    }

    private void SetAimDirection()
    {
        Vector3 mouseLocation = playerCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mouseLocation - this.transform.position;
        direction.z = 0f;

        Vector3 newRotation = Vector3.RotateTowards(this.transform.up, direction.normalized, turnSpeed * Time.deltaTime, 0.0f);
        this.transform.up = newRotation;
    }

    private void ResetMoveTimerAndDirection()
    {
        moveTimeLeft = 0f;
        moveDirection.x = 0f;
        moveDirection.y = 0f;
    }

    private bool CanFireAShot()
    {
        return SwarmDirector.AreEnemiesAlive() && GetIsPlayerReady() && fireRateTimer <= 0f && FindEmptyBulletSlot() > -1;
    }

    private int FindEmptyBulletSlot()
    {
        for (int i = 0; i < bulletRefs.Length; ++i)
        {
            if (bulletRefs[i] == null) { return i; }
        }

        return -1;
    }

    private void SetFireRateTimer()
    {
        fireRateTimer = fireRate;
    }

    private void TickFireRateTimer()
    {
        if (fireRateTimer > 0f)
        {
            fireRateTimer -= Time.deltaTime;
            if (fireRateTimer < 0f) { fireRateTimer = 0f; }
        }
    }

    private IEnumerator KillShipCR()
    {
        Scorekeeper.BreakCombo();
        Scorekeeper.IncrementLivesLost();

        MusicPlayer.StopMusic();
        SoundLibrary.Play("explode_hitstop", 0.9f);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
        SoundLibrary.Play("explode_player", 0.9f);

        GameObject.Destroy(reticleRef.gameObject);
        GameObject.Destroy(this.gameObject);
    }
}
