using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flierwerk : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] protected GameObject shrapnelPrefab;
    [SerializeField] Sprite[] animSprites;
    [SerializeField] int spriteChangeFrameInterval = 30;
    [SerializeField, Range(0f, 1f)] float distancePortion = 1f;
    [SerializeField] float attackWindup = 1f;
    [SerializeField] float attackSpeed = 10f;
    [SerializeField] Vector2 playerPositionOffset;
    [SerializeField] int numberOfShrapnel = 1;
    [SerializeField] int arcOfShotSpread = 90;
    [SerializeField] float aimOffset = 0f;

    private GameObject playerRef = null;
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

    void Update() // Do idle animation while looking at the center point of the swarm
    {
        if (Input.GetKeyDown(KeyCode.R)) { Attack(); } // Testing

        if (!isAttacking)
        {
            this.transform.up = (Vector3.zero - this.transform.position).normalized;
            CheckSpriteChange();
            --frameTimer;
        }
    }

    public void Attack()
    {
        if (isAttacking) { return; }
        isAttacking = true;
        StartCoroutine(AttackCR());
    }

    protected IEnumerator AttackCR()
    {
        // Play flashing red animation while pointing at the player's location or target location
        spriteRenderer.sprite = animSprites[0];
        spriteRenderer.color = Color.red;
        float currentWindupTimer = attackWindup;
        frameTimer = spriteChangeFrameInterval;

        while (currentWindupTimer > 0f)
        {
            UpdatePlayerRef();
            if (playerRef != null) { playerPosition = (playerRef.transform.position + (playerRef.transform.up * playerPositionOffset.y) + (playerRef.transform.right * playerPositionOffset.x)); }
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

    private void FireShrapnel()
    {
        int increment = (arcOfShotSpread / numberOfShrapnel);
        for (int i = 0; i < arcOfShotSpread; i += increment)
        {
            Instantiate(shrapnelPrefab, this.transform.position, this.transform.rotation * Quaternion.Euler(0f, 0f, (float)i + aimOffset));
        }
    }

    private void UpdatePlayerRef()
    {
        if (isAttacking && playerRef == null)
        {
            playerRef = GameObject.FindWithTag("Player");
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
}
