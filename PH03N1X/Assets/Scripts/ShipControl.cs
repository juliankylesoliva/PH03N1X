using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControl : MonoBehaviour
{
    /* COMPONENTS */
    Camera playerCam;

    /* EDITOR PARAMETERS */
    [SerializeField] Vector2 startCenter;
    [SerializeField] float boundaryRadius = 4f;

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
        this.transform.position = new Vector3(startCenter.x, startCenter.y, 0f);
    }

    void Update()
    {
        SetAimDirection();
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
                ResetTimerAndDirection();
            }
        }
        else
        {
            ResetTimerAndDirection();
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
                ResetTimerAndDirection();
            }
        }
    }

    private void SetAimDirection()
    {
        Vector3 mouseLocation = playerCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mouseLocation - this.transform.position;
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
        this.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void ResetTimerAndDirection()
    {
        moveTimeLeft = 0f;
        moveDirection.x = 0f;
        moveDirection.y = 0f;
    }
}
