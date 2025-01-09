using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private bool isOpen = false;

    [SerializeField]
    private bool m_DesiredOpen = false;

    [SerializeField]
    private GameObject m_LeftDoor;

    [SerializeField]
    private GameObject m_RightDoor;

    [SerializeField]
    private float m_ChangeTime = 2.0f;

    private float m_ChangeTimer = 0.0f;

    private float originalXLeft;
    private float originalXRight;
    [SerializeField] private float xChangeClose = 1.25f;

    private float originalXScaleLeft;
    private float originalXScaleRight;
    [SerializeField] private float xScaleClose = 3.5f;
    void Start()
    {
        originalXLeft = m_LeftDoor.transform.localPosition.x;
        originalXRight = m_RightDoor.transform.localPosition.x;

        originalXScaleLeft = m_LeftDoor.transform.localScale.x;
        originalXScaleRight = m_RightDoor.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (m_DesiredOpen != isOpen)
        {
            if (m_DesiredOpen == true)
            {
                if (OpenDoor())
                {
                    isOpen = true;
                }
            }
            else
            {
                if (CloseDoor())
                {
                    isOpen = false;
                }
            }
        }
    }

    public bool Open()
    {
        m_DesiredOpen = true;
        return isOpen;
    }

    public bool Close()
    {
        m_DesiredOpen = false;
        return !isOpen;
    }

    //open is 0 close is 1
    private bool OpenDoor()
    {
        m_ChangeTimer -= Time.deltaTime;

        ChangeDoors();

        if (m_ChangeTimer <= 0)
        {
            m_ChangeTimer = 0;
            return true;
        }

        return false;
    }

    private void ChangeDoors()
    {
        float xChange = Mathf.Lerp(0, xChangeClose, m_ChangeTimer / m_ChangeTime);
        float xScale = Mathf.Lerp(1, xScaleClose, m_ChangeTimer / m_ChangeTime);

        m_LeftDoor.transform.localPosition = new Vector3(originalXLeft + xChange, m_LeftDoor.transform.localPosition.y, m_LeftDoor.transform.localPosition.z);
        m_RightDoor.transform.localPosition = new Vector3(originalXRight - xChange, m_RightDoor.transform.localPosition.y, m_RightDoor.transform.localPosition.z);

        m_LeftDoor.transform.localScale = new Vector3(xScale, m_LeftDoor.transform.localScale.y, m_LeftDoor.transform.localScale.z);
        m_RightDoor.transform.localScale = new Vector3(xScale, m_RightDoor.transform.localScale.y, m_RightDoor.transform.localScale.z);
    }

    private bool CloseDoor()
    {
        m_ChangeTimer += Time.deltaTime;

        ChangeDoors();

        if (m_ChangeTimer >= m_ChangeTime)
        {
            m_ChangeTimer = m_ChangeTime;
            return true;
        }

        return false;
    }
}
