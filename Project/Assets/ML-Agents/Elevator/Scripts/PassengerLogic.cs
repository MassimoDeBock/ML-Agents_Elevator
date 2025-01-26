using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

enum PassengerState
{
    Waiting,
    Entering,
    InElevator,
    Exiting,
    Arrived
}

public class PassengerLogic : MonoBehaviour
{
    [SerializeField]
    public int destinationFloor = 1;

    public float m_WaitTime = 0.0f;

    [SerializeField]
    public float m_FullRedWaitTime = 10.0f;

    [SerializeField]
    public float m_MaxWaitTime = 50.0f;

    public float movement_timer = 0.0f;

    private bool m_HasPriority = false;

    [FormerlySerializedAs("hat")] [SerializeField]
    //hat color piece
    public GameObject destinationHat;

    public GameObject hatBrim;

    public GameObject mainHat;

    [SerializeField]
    private Material silverMaterial;

    public bool isLocked = false;

    [SerializeField]
    private GameObject MainBody;

    private Renderer hatRenderer;

    public Elevator elevator;

    public void SetDestination(int floor)
    {
        destinationFloor = floor;
    }

    // Start is called before the first frame update
    void Start()
    {
        MainBody.GetComponent<Renderer>();
        ChangeColorHat();
        ChangeColorPatience();
    }

    // Update is called once per frame
    void Update()
    {
        //increment the wait time
        m_WaitTime += Time.deltaTime;
        ChangeColorPatience();
    }

    private void FixedUpdate()
    {
        if (m_WaitTime > m_FullRedWaitTime)
        {
            elevator.FailIteration();
            //destroy the passenger

        }
    }


    private void ChangeColorPatience()
    {
        Renderer hatRenderer = MainBody.GetComponent<Renderer>();
        //make the color go from white to red based on how long the passenger has been waiting
        hatRenderer.material.color = Color.Lerp(Color.white, Color.red, m_WaitTime / m_FullRedWaitTime);



    }

    void ChangeColorHat()
    {
        Renderer hatRenderer = destinationHat.GetComponent<Renderer>();
        Color[] floorColors = { Color.red, Color.blue, Color.green, Color.yellow, new Color(1.0f, 0.4f, 0.2f), Color.cyan , Color.black, Color.gray};

        if (destinationFloor >= 0 && destinationFloor < floorColors.Length)
        {
            hatRenderer.material.color = floorColors[destinationFloor];
        }
        else
        {
            hatRenderer.material.color = Color.white;
        }
    }

    public bool IsDesiredFloor(int floor)
    {
        return destinationFloor == floor;
    }

    private Vector3 m_StartingPosition;
    private Vector3 m_TargetPosition;

    public void StartMovingToElevator(Vector3 target, float time)
    {
        m_StartingPosition = transform.position;
        m_TargetPosition = target;
        movement_timer = time;
        isLocked = true;
        transform.position = Vector3.Lerp(m_TargetPosition, m_StartingPosition, movement_timer);
        m_WaitTime /= 2f;
    }

    public bool MoveInElevator(float elapsedTime)
    {
        if (movement_timer <= 0)
        {
            isLocked = false;
            return true;
        }
        movement_timer -= elapsedTime;

        //change current position to target position
        transform.position = Vector3.Lerp(m_TargetPosition, m_StartingPosition, movement_timer);

        return false;
    }

    public void StartExitingElevator(Vector3 target, float time)
    {
        m_TargetPosition = target;
        m_StartingPosition = transform.position;
        movement_timer = time;
        isLocked = true;
        transform.position = Vector3.Lerp(m_TargetPosition,m_StartingPosition,  movement_timer);

    }

    public bool ExitElevator(float elapsedTime)
    {
        if (movement_timer <= 0)
        {
            isLocked = false;
            return true;
        }
        movement_timer -= elapsedTime;

        //change current position to target position
        transform.position = Vector3.Lerp(m_TargetPosition,m_StartingPosition,  movement_timer);

        return false;
    }

    public float NormalizedWaitTime()
    {
        return Mathf.Max(m_WaitTime / m_MaxWaitTime, 1.0f);
    }

    public bool HasPriority()
    {
        return m_HasPriority;
    }

    public void SetPriority(bool priority)
    {
        m_HasPriority = priority;

        //if its has priority make the hat and brim silver (slightly reflective)

        if (m_HasPriority)
        {
            hatRenderer = mainHat.GetComponent<Renderer>();
            hatRenderer.material = silverMaterial;
            hatRenderer = hatBrim.GetComponent<Renderer>();
            hatRenderer.material = silverMaterial;
        }

    }
}
