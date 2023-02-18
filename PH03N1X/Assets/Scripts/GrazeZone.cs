using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrazeZone : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    private int numInZone = 0;

    void Awake()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        spriteRenderer.color = (numInZone > 0 ? Color.white : Color.clear );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "EnemyProjectile")
        {
            numInZone++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "EnemyProjectile")
        {
            numInZone--;
            Projectile projTemp = other.gameObject.GetComponent<Projectile>();
            if (projTemp != null && !projTemp.GetIsGrazed())
            {
                projTemp.SetIsGrazed();
                Scorekeeper.AddGraze();
            }
        }
    }
}
