using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class ElevatorReimagined : Elevator
{
    [SerializeField] private int steps = 0;
    [SerializeField] private int maxSteps = 1000;
    protected override void FixedUpdate()
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
        UpdatePunishForPassengers();
    }

    protected override void Idle()
    {
        ChooseNextDestination();
        m_State = ElevatorState.Moving;
        steps++;
        if (steps > maxSteps)
        {
            steps = 0;
            m_Brain.Failed();
        }
    }

    [SerializeField] private float PunishAmount = -0.5f;
    protected void UpdatePunishForPassengers()
    {
        foreach (var floor in building.floors)
        {
            foreach (var passenger in floor.passengers)
            {
                m_Brain.Reward("Waiting Reward", PunishAmount * 1.3f * (1 +passenger.m_WaitTime/50.0f));
            }
        }

        //look at all inside passengers
        foreach (var passenger in passengers)
        {
            m_Brain.Reward("Waiting Reward", PunishAmount * (1 +passenger.m_WaitTime/50.0f) );
        }
    }

    protected override void FlowOut()
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
            var reward = 50.0f;
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

    protected override void ChooseNextDestination()
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
        if (destinationFloor == currentFloor)
        {
            // m_Brain.Failed();
            m_Brain.Reward("Reward Same Location",-2000.0f);
            steps--;

        }

    }

    //go over all passengers on the floor and in the elevator, punish the brain for each passenger

    public override void PunishForPassengers()
    {
    }

    public override void FailIteration()
    {
    }
}
