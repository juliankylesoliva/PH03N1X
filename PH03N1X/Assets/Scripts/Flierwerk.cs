using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flierwerk : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] protected GameObject shrapnelPrefab;
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
    private Vector3 targetDirection = Vector3.zero;
    private bool isAttacking = false;
    private int frameTimer = 0;

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
        spriteRenderer.color = Color.red;
        float currentWindupTimer = attackWindup;
        frameTimer = spriteChangeFrameInterval;

        while (currentWindupTimer > 0f)
        {
            ShipControl ctrl = PlayerSpawner.GetPlayerRef();
            if (ctrl != null) { playerPosition = (ctrl.transform.position + (ctrl.transform.up * playerPositionOffset.y) + (ctrl.transform.right * playerPositionOffset.x)); }
            targetDirection = (playerPosition - this.transform.position);
            this.transform.up = targetDirection.normalized;

            CheckColorChange();

            --frameTimer;
            currentWindupTimer -= Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = Color.white;

        float currentAttackTimer = ((targetDirection.magnitude / attackSpeed) * distancePortion);
        while (currentAttackTimer > 0f)
        {
            this.transform.position += (this.transform.up * attackSpeed * Time.deltaTime);

            CheckSpriteChange(true);

            --frameTimer;
            currentAttackTimer -= Time.deltaTime;
            yield return null;
        }

        FireShrapnel();
        GameObject.Destroy(this.gameObject);

        yield break;
    }

    public void KillEnemy()
    {
        if (isAttacking) { StopCoroutine("AttackCR"); }
        GameObject.Destroy(this.gameObject);
        Scorekeeper.AddToScore(pointValue * (isAttacking ? 2 : 1), true);
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
            spriteRenderer.color = (spriteRenderer.color == Color.red ? Color.white : Color.red);
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

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            ShipControl tempShip = other.gameObject.GetComponent<ShipControl>();
            if (tempShip != null) { tempShip.KillShip(); }
            if (isAttacking) { StopCoroutine("AttackCR"); }
            GameObject.Destroy(this.gameObject);
        }
    }
}
