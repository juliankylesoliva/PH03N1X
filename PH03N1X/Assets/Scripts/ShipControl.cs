using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControl : MonoBehaviour
{
    /* COMPONENTS */
    Camera playerCam;

    /* EDITOR PARAMETERS */
    [SerializeField] float moveSpeed = 5f;


    /* PRIVATE VARIABLES */
    private float moveTimeLeft = 0f;
    private Vector2 moveDirection = Vector2.zero;

    void Awake()
    {
        playerCam = Camera.main;
    }

    void Start()
    {
        
    }

    void Update()
    {
        SetMovementDirection();
        GoToNextPosition();
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
        else if (Input.GetMouseButtonDown(1))
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
                moveTimeLeft = 0f;
                moveDirection.x = 0f;
                moveDirection.y = 0f;
            }
        }
        else
        {
            moveTimeLeft = 0f;
            moveDirection.x = 0f;
            moveDirection.y = 0f;
        }
    }

    private void GoToNextPosition()
    {
        if (moveDirection.x != 0f || moveDirection.y != 0f)
        {
            Vector3 nextPosition = this.transform.position;
            nextPosition += ((new Vector3(moveDirection.x, moveDirection.y, 0f)) * moveSpeed * Time.deltaTime);

            this.transform.position = nextPosition;
        }
    }
}
