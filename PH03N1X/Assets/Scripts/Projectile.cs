using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 5f;
    [SerializeField] float despawnBoundary = 10f;

    void Start()
    {
        
    }

    void Update()
    {
        if (this.transform.position.x < despawnBoundary && this.transform.position.x > -despawnBoundary && this.transform.position.y < despawnBoundary && this.transform.position.y > -despawnBoundary)
        {
            Vector3 nextPosition = (this.transform.position + (this.transform.up * projectileSpeed * Time.deltaTime));
            this.transform.position = nextPosition;
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
