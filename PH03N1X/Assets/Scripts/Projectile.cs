using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 5f;
    [SerializeField] float despawnBoundary = 10f;

    private bool isGrazed = false;

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
            if (this.gameObject.tag == "PlayerProjectile")
            {
                Scorekeeper.BreakCombo();
                Scorekeeper.IncrementShotsMissed();
            }
            GameObject.Destroy(this.gameObject);
        }
    }

    public bool GetIsGrazed()
    {
        return isGrazed;
    }

    public void SetIsGrazed()
    {
        isGrazed = true;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (this.gameObject.tag == "PlayerProjectile" && other.gameObject.tag == "Enemy")
        {
            Flierwerk tempEnemy = other.gameObject.GetComponent<Flierwerk>();
            if (tempEnemy != null) { tempEnemy.KillEnemy(); }
            Scorekeeper.AddToCombo();
            GameObject.Destroy(this.gameObject);
        }
        else if (this.gameObject.tag == "EnemyProjectile" && other.gameObject.tag == "Player")
        {
            ShipControl tempShip = other.gameObject.GetComponent<ShipControl>();
            if (tempShip != null) { tempShip.KillShip(); }
            GameObject.Destroy(this.gameObject);
        }
        else { /* Nothing */ }
    }

    void OnCollisionExit2D(Collision2D other)
    {

    }
}
