using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrazeZone : MonoBehaviour
{

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "EnemyProjectile")
        {
            Projectile projTemp = other.gameObject.GetComponent<Projectile>();
            if (projTemp != null && !projTemp.GetIsGrazed())
            {
                projTemp.SetIsGrazed();
                Debug.Log("Graze!");
                // Add to score
            }
        }
    }
}
