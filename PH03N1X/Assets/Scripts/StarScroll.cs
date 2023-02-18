using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarScroll : MonoBehaviour
{
    [SerializeField] float yBoundary = -20f;
    private float currentScrollSpeed = 0f;
    private float targetScrollSpeed = 0f;
    private float currentChangeRate = 0f;

    void Update()
    {
        if (currentScrollSpeed != targetScrollSpeed)
        {
            if (currentScrollSpeed > targetScrollSpeed)
            {
                currentScrollSpeed -= (currentChangeRate * Time.deltaTime);
                if (currentScrollSpeed < targetScrollSpeed)
                {
                    currentScrollSpeed = targetScrollSpeed;
                }
            }
            else
            {
                currentScrollSpeed += (currentChangeRate * Time.deltaTime);
                if (currentScrollSpeed > targetScrollSpeed)
                {
                    currentScrollSpeed = targetScrollSpeed;
                }
            }
        }

        this.transform.position -= (Vector3.up * currentScrollSpeed * Time.deltaTime);
        if (this.transform.position.y <= yBoundary)
        {
            this.transform.position = (Vector3.up * -yBoundary);
        }
    }

    public void SetScrollSpeed(float speed, float rate)
    {
        targetScrollSpeed = speed;
        currentChangeRate = rate;
    }
}
