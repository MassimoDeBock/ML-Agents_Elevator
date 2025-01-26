using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public enum ElevatorState
{
    Idle,
    Moving,
    FlowIn,
    FlowOut,
    DoorOpening,
    DoorClosing
}

public class Elevator : MonoBehaviour
{

    [SerializeField]
    public int currentFloor = 0;

    // [SerializeField]
    // public List<FloorPiece> floors = new List<FloorPiece>();
    //
    // private int m_FloorsAmount = 5;

    public Building building;

    [SerializeField]
    public int destinationFloor = 3;

    [SerializeField]
    public float speed = 1;

    [SerializeField]
    public float doorOpenTime = 1;

    protected ElevatorState m_State = ElevatorState.Idle;

    [SerializeField]
    public float exchangeTime = 1.0f;

    [SerializeField]
    protected BaseElevatorBrain m_Brain;

    // [SerializeField]
    // public float doorOpenTime = 1;
    //
    // [SerializeField]
    // public float doorCloseTime = 1;

    // passenger area
    [SerializeField]
    public GameObject passengerArea;

    //list of passengers in the elevator
    [SerializeField]
    public List<PassengerLogic> passengers = new List<PassengerLogic>();


    public float longestPickupTime = -10.0f;

    public float shortestPickupTime = -10.0f;

    public float totalPickupTime = 0f;

    public int totalPickups = 0;

    public float longestDropoffTime = -10.0f;

    public float shortestDropoffTime = -10.0f;

    public float totalDropoffTime = 0f;

    public int totalDropoffs = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected virtual void FixedUpdate()
    {
        switch (m_State)
        {
            case ElevatorState.Idle:
                Idle();
                break;
            case ElevatorState.Moving:
                MoveElevator();
                break;
            case ElevatorState.DoorOpening:
                OpenDoor();
                break;
            case ElevatorState.DoorClosing:
                CloseDoor();
                break;
            case ElevatorState.FlowOut:
                FlowOut();
                break;
            case ElevatorState.FlowIn:
                FlowIn();
                break;
        }
    }

    public void SetBuilding(Building building)
    {
        this.building = building;



        //move the transform y to the first floor y
        var pos = transform.position;
        transform.position = new Vector3(pos.x, building.floors[currentFloor].transform.position.y, pos.z);

        //go over all the floors and set the floor number
        for (int i = 0; i < building.floors.Count; i++)
        {
            building.floors[i].SetFloorNumber(i);
            building.floors[i].ChangeColorPlate();
        }

        //assign the elevator to the brain
        m_Brain.AssignElevator(this);

        //set the elevator to idle
        m_State = ElevatorState.Idle;
    }

    [CanBeNull] protected PassengerLogic m_MovingPassenger;


    protected virtual void FlowOut()
    {
        var pplInElevator = passengers.Count;
        if (pplInElevator < 1)
        {
            m_State = ElevatorState.FlowIn;
            return;
        }

        if (m_MovingPassenger == null)
        {
            //find a passenger to move
            foreach (var passenger in passengers)
            {
                if (passenger.IsDesiredFloor(currentFloor) && !passenger.isLocked)
                {
                    m_MovingPassenger = passenger;
                    m_MovingPassenger.StartExitingElevator(building.floors[currentFloor].ExitSocket.transform.position, exchangeTime);
                    break;
                }
            }
        }

        if(m_MovingPassenger == null)
        {
            m_State = ElevatorState.FlowIn;
            return;
        }

        if(m_MovingPassenger.ExitElevator(Time.deltaTime))
        {
            RemoveFromElevator(m_MovingPassenger);
            float reward = 15000.0f-Mathf.Pow(m_MovingPassenger.m_WaitTime,2);
            reward = Mathf.Min(50.0f,reward);
            m_Brain.Reward("Reward Drop off",reward);

            EvaluateDropOff(m_MovingPassenger.m_WaitTime);


            //destroy the passenger
            Destroy(m_MovingPassenger.gameObject);
            //Debug.Log("Passenger exited the elevator");

            // building.floors[currentFloor].AddPassenger(m_MovingPassenger);
            // m_MovingPassenger.timer = 20.0f;
            m_MovingPassenger = null;
        }
    }

    protected void EvaluateDropOff(float waitTime)
    {
        //update the longest and shortest dropoff time
        if (longestDropoffTime < waitTime || longestDropoffTime < 0)
        {
            longestDropoffTime = waitTime;
        }

        if (shortestDropoffTime > waitTime || shortestDropoffTime < 0)
        {
            shortestDropoffTime = waitTime;
        }

        totalDropoffTime += waitTime;
        totalDropoffs += 1;
    }

    protected virtual void FlowIn()
    {
        var pplOnFloor = building.floors[currentFloor].passengers.Count;
        if (pplOnFloor < 1)
        {
            StartClosingDoor();
            return;
        }

        if (m_MovingPassenger == null)
        {
            //find a passenger to move
            foreach (var passenger in building.floors[currentFloor].passengers)
            {
                if (!passenger.IsDesiredFloor(currentFloor) && !passenger.isLocked)
                {
                    m_MovingPassenger = passenger;
                    m_Brain.Reward("Reward Pick up",20.0f);
                    EvaluatePickUp(m_MovingPassenger.m_WaitTime);
                    m_MovingPassenger.StartMovingToElevator(GetRandomSpotInArea(), exchangeTime);
                    break;
                }
            }
        }

        if(m_MovingPassenger == null)
        {
            Debug.Log("No valid passengers to move");
            StartClosingDoor();
            return;
        }

        if(m_MovingPassenger.MoveInElevator(Time.deltaTime))
        {
            AddToElevator(m_MovingPassenger);
            building.floors[currentFloor].RemovePassenger(m_MovingPassenger);
            m_MovingPassenger = null;
        }
    }

    protected void EvaluatePickUp(float movingPassengerWaitTime)
    {
        //update the longest and shortest pickup time
        if (longestPickupTime < movingPassengerWaitTime || longestPickupTime < 0)
        {
            longestPickupTime = movingPassengerWaitTime;
        }

        if (shortestPickupTime > movingPassengerWaitTime || shortestPickupTime < 0)
        {
            shortestPickupTime = movingPassengerWaitTime;
        }

        totalPickupTime += movingPassengerWaitTime;
        totalPickups += 1;
    }

    protected void StartClosingDoor()
    {
        m_State = ElevatorState.DoorClosing;
        m_Brain.Ponder();
    }


    protected virtual void Idle()
    {
        ChooseNextDestination();
        m_State = ElevatorState.Moving;
    }

    [SerializeField] protected Door m_Door;

    protected void OpenDoor()
    {
        if (m_Door.Open())
        {
            if (!HasValidTargets())
            {
                m_Brain.Reward("Bad Destination",-5000.0f);
            }
            //check if theres any valid moving passenger
            m_State = ElevatorState.FlowOut;
        }
    }

    protected bool HasValidTargets()
    {
        //check if theres anyone on this floor
        if (building.floors[currentFloor].passengers.Count > 0)
        {
            return true;
        }
        //loop over all people in the elevator and check if they want to get out on this floor
        foreach (var passenger in passengers)
        {
            if (passenger.IsDesiredFloor(currentFloor))
            {
                return true;
            }
        }

        return false;
    }

    protected void CloseDoor()
    {
        if (m_Door.Close())
        {
            m_State = ElevatorState.Idle;
        }
    }

    protected void MoveElevator()
    {
        //move the elevator to the destination floor
        var pos = transform.position;
        var dest = building.floors[(int)destinationFloor].transform.position.y;
        var step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(pos, new Vector3(pos.x, dest, pos.z), step);

        //check if the elevator has reached the destination floor
        if (math.abs(pos.y - dest) < 0.01)
        {
           // Debug.Log("Elevator arrived on floor " + destinationFloor);
            m_State = ElevatorState.DoorOpening;
            currentFloor = destinationFloor;
        }

    }

    public void AddToElevator(PassengerLogic passenger)
    {
        passengers.Add(passenger);
        MovePassengerToElevator(passenger);
    }

    public void RemoveFromElevator(PassengerLogic passenger)
    {
        passengers.Remove(passenger);
    }

    protected void MovePassengerToElevator(PassengerLogic passenger)
    {
        if (passengerArea == null && passenger == null)
        {
            return;
        }
        // move the passenger to the elevator
        //get random location in the passenger area

        //make the passenger a child of the passenger area
        passenger.transform.parent = passengerArea.transform;
    }

    protected Vector3 GetRandomSpotInArea()
    {
        const float passengerSize = 1.2f;
        const float offset = passengerSize * 2.0f;
        var randomX = Random.Range(-offset, offset);
        var randomZ = Random.Range(-offset, offset);
        var loc = passengerArea.transform.position;
        loc = new Vector3(loc.x + randomX, loc.y, loc.z + randomZ);
        return loc;
    }

    protected virtual void ChooseNextDestination()
    {
        // check if brain is null
        if (m_Brain == null)
        {
            //take a random floor and move to that floor
            destinationFloor = Random.Range(0, building.floors.Count);
            return;
        }

        // get the next destination from the brain
        destinationFloor = m_Brain.GetNextDestination();
        if (destinationFloor >= building.floors.Count)
        {
            m_Brain.Failed();
            destinationFloor = currentFloor;
        }
        else if (destinationFloor < 0)
        {
            m_Brain.Failed();
            destinationFloor = currentFloor;
        }else if (destinationFloor == currentFloor)
        {
            // m_Brain.Failed();
            m_Brain.Reward("Reward Same Location",-2000.0f);
        }

    }

    public void Reset()
    {
        //delete all passengers
        foreach (var passenger in passengers)
        {
            Destroy(passenger.gameObject);
        }

        passengers.Clear();

        //remove all passengers from the floors
        foreach (var floor in building.floors)
        {
            floor.CleanFloor();
        }

        //reset the shortest longest and total wait time
        longestPickupTime = -10.0f;
        shortestPickupTime = -10.0f;
        totalPickupTime = 0f;
        totalPickups = 0;
        longestDropoffTime = -10.0f;
        shortestDropoffTime = -10.0f;
        totalDropoffTime = 0f;
        totalDropoffs = 0;
    }

    //go over all passengers on the floor and in the elevator, punish the brain for each passenger

    public virtual void PunishForPassengers()
    {
        foreach (var floor in building.floors)
        {
            foreach (var passenger in floor.passengers)
            {
                m_Brain.Reward("Reward Leftover",-Mathf.Pow(passenger.m_WaitTime,1.2f));
            }
        }

        foreach (var passenger in passengers)
        {
            m_Brain.Reward("Reward Leftover",-passenger.m_WaitTime);
        }
    }

    public virtual void FailIteration()
    {
        m_Brain.Failed();
    }
}
