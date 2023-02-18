using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flierwerk : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] GameObject reticlePrefab;
    [SerializeField] GameObject directionArrowPrefab;
    [SerializeField] GameObject shrapnelPrefab;
    [SerializeField] Sprite[] animSprites;
    [SerializeField] int pointValue = 10;
    [SerializeField] int spriteChangeFrameInterval = 30;
    [SerializeField, Range(0f, 1f)] float distancePortion = 1f;
    [SerializeField] float attackWindup = 1f;
    [SerializeField] float attackSpeed = 10f;
    [SerializeField] Vector2 playerPositionOffset;
    [SerializeField] int numberOfShrapnel = 1;
    [SerializeField] int arcOfShotSpread = 90;
    [SerializeField] float aimOffset = 0f;

    private SwarmDirector director = null;
    private int positionID = -1;
    private Vector3 playerPosition = Vector3.zero;
    private Vector3 truePlayerPosition = Vector3.zero;
    private Vector3 targetDirection = Vector3.zero;
    private bool isAttacking = false;
    private int frameTimer = 0;

    private GameObject reticleRef = null;
    private GameObject directionArrowRef = null;

    void Awake()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        frameTimer = spriteChangeFrameInterval;
    }

    void Update()
    {
        if (!isAttacking)
        {
            this.transform.up = (Vector3.zero - this.transform.position).normalized;
            this.transform.position = director.GetPositionByID(positionID);
            CheckSpriteChange();
            --frameTimer;
        }
    }

    public void SetDirector(SwarmDirector d)
    {
        if (d != null) { director = d; }
    }

    public void SetPositionID(int id)
    {
        positionID = id;
    }

    public int GetPositionID()
    {
        return positionID;
    }

    public bool GetIsAttacking()
    {
        return isAttacking;
    }

    public void Attack()
    {
        if (isAttacking) { return; }
        isAttacking = true;
        StartCoroutine("AttackCR");
    }

    protected IEnumerator AttackCR()
    {
        spriteRenderer.sprite = animSprites[0];
        float currentWindupTimer = attackWindup;
        frameTimer = spriteChangeFrameInterval;

        while (currentWindupTimer > 0f)
        {
            ShipControl ctrl = PlayerSpawner.GetPlayerRef();
            if (ctrl != null)
            {
                playerPosition = (ctrl.transform.position + (ctrl.transform.up * playerPositionOffset.y) + (ctrl.transform.right * playerPositionOffset.x));
                truePlayerPosition = ctrl.transform.position;
            } 
            targetDirection = (playerPosition - this.transform.position);
            this.transform.up = targetDirection.normalized;
            this.transform.position = director.GetPositionByID(positionID);

            UpdateReticle(currentWindupTimer);

            UpdateDirectionArrow();

            CheckColorChange();

            --frameTimer;
            currentWindupTimer -= Time.deltaTime;
            yield return null;
        }
        UpdateReticle(0f);

        float currentAttackTimer = ((targetDirection.magnitude / attackSpeed) * distancePortion);
        while (currentAttackTimer > 0f)
        {
            ShipControl ctrl = PlayerSpawner.GetPlayerRef();
            if (ctrl != null) { truePlayerPosition = ctrl.transform.position; }
            this.transform.position += (this.transform.up * attackSpeed * Time.deltaTime);

            UpdateDirectionArrow();

            if (currentAttackTimer > 0.5f) { CheckSpriteChange(true); }
            else { spriteRenderer.sprite = animSprites[2]; }

            --frameTimer;
            currentAttackTimer -= Time.deltaTime;
            yield return null;
        }

        if (reticleRef != null) { GameObject.Destroy(reticleRef); }
        if (directionArrowRef != null) { GameObject.Destroy(directionArrowRef); }
        FireShrapnel();
        Scorekeeper.IncrementEscapees();
        GameObject.Destroy(this.gameObject);
        yield break;
    }

    public void KillEnemy()
    {
        if (isAttacking)
        {
            StopCoroutine("AttackCR");
            FireShrapnel();
        }
        Scorekeeper.AddToScore(pointValue * (isAttacking ? 2 : 1), true);
        if (reticleRef != null) { GameObject.Destroy(reticleRef); }
        if (directionArrowRef != null) { GameObject.Destroy(directionArrowRef); }
        GameObject.Destroy(this.gameObject);
    }

    private void FireShrapnel()
    {
        int increment = (arcOfShotSpread / numberOfShrapnel);
        for (int i = 0; i < arcOfShotSpread; i += increment)
        {
            Instantiate(shrapnelPrefab, this.transform.position, this.transform.rotation * Quaternion.Euler(0f, 0f, (float)i + aimOffset));
        }
    }

    private void CheckColorChange()
    {
        if (frameTimer == 0)
        {
            spriteRenderer.sprite = (spriteRenderer.sprite == animSprites[0] ? animSprites[2] : animSprites[0]);
            frameTimer = (spriteChangeFrameInterval / 2);
        }
    }

    private void CheckSpriteChange(bool halved = false)
    {
        if (frameTimer == 0)
        {
            spriteRenderer.sprite = (spriteRenderer.sprite == animSprites[0] ? animSprites[1] : animSprites[0]);
            frameTimer = (spriteChangeFrameInterval / (halved ? 2 : 1));
        }
    }

    private void UpdateReticle(float currentWindupTimer)
    {
        Vector3 resultPosition = (this.transform.position + (targetDirection * distancePortion * ((attackWindup - currentWindupTimer) / attackWindup)));
        if (reticleRef == null) { reticleRef = Instantiate(reticlePrefab, resultPosition, Quaternion.identity); }
        else { reticleRef.transform.position = resultPosition; }
    }

    private void UpdateDirectionArrow()
    {
        Vector3 resultPosition = (truePlayerPosition - (targetDirection.normalized * 1.25f));
        if (directionArrowRef == null) { directionArrowRef = Instantiate(directionArrowPrefab, resultPosition, Quaternion.identity); }
        else { directionArrowRef.transform.position = resultPosition; }
        directionArrowRef.transform.up = (-targetDirection.normalized);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            ShipControl tempShip = other.gameObject.GetComponent<ShipControl>();
            if (tempShip != null) { tempShip.KillShip(); }
            if (isAttacking) { StopCoroutine("AttackCR"); }
            FireShrapnel();
            if (reticleRef != null) { GameObject.Destroy(reticleRef); }
            if (directionArrowRef != null) { GameObject.Destroy(directionArrowRef); }
            GameObject.Destroy(this.gameObject);
        }
    }
}
