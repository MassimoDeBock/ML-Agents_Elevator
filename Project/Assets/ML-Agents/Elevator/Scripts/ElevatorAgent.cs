using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class ElevatorAgent : Agent
{
    public Elevator elevator;

    BufferSensorComponent m_BufferSensor;

    //[SerializeField] private int maxDecisions = 50;
    private int m_TotalDecisions = 0;

    public override void Initialize()
    {
        m_BufferSensor = GetComponent<BufferSensorComponent>();
    }

    public override void OnEpisodeBegin()
    {
        m_TotalDecisions = 0;
        elevator.Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        sensor.AddObservation(elevator.currentFloor);
        sensor.AddObservation(elevator.building.floors.Count);

        List<float> maxWaitingTimerEachFloor = new List<float>();
        List<float> waiting = new List<float>();
        float maxWaiting = 1;
        foreach (FloorPiece floor in elevator.building.floors)
        {
            if (floor.passengers.Count > maxWaiting)
            {
                maxWaiting = floor.passengers.Count;
            }
            waiting.Add(floor.passengers.Count);
            maxWaitingTimerEachFloor.Add(0);
        }

        //go over each passenger and add the destination to the destinations array
        float[] passengers = new float[elevator.building.floors.Count];
        // set all passengers to 0 as default
        for (int i = 0; i < passengers.Length; i++)
        {
            passengers[i] = 0;
        }
        float maxDestination = 1;
        foreach (PassengerLogic passenger in elevator.passengers)
        {
            //add 1 to the destination floor count
            passengers[passenger.destinationFloor] += 1;

            //check if the timer of this passenger is higher than that in maxWaitingTimerEachFloor
            if (passenger.NormalizedWaitTime() > maxWaitingTimerEachFloor[passenger.destinationFloor])
            {
                maxWaitingTimerEachFloor[passenger.destinationFloor] = passenger.NormalizedWaitTime();
            }
        }

        //normalize the destinations array
        for (int i = 0; i < passengers.Length; i++)
        {
            if (passengers[i] > maxDestination)
            {
                maxDestination = passengers[i];
            }
        }


        //loop over each floor
        for (int i = 0; i < elevator.building.floors.Count; i++)
        {
            m_BufferSensor.AppendObservation(new float[] { waiting[i] / maxWaiting, passengers[i] / maxDestination, elevator.building.floors[i].GetTimeNormalized(), maxWaitingTimerEachFloor[i] });
        }

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + i)))
            {
                discreteActionsOut[0] = i;
                break;
            }
        }

        Debug.Log(discreteActionsOut[0] + " is the chosen floor");
    }

    public int GetNextDestination()
    {
        return GetStoredActionBuffers().DiscreteActions[0];
    }

    public void Ponder()
    {
        RequestDecision();

        m_TotalDecisions++;
        // if (m_TotalDecisions > maxDecisions)
        // {
        //     //elevator.PunishForPassengers();
        //     EndEpisode();
        //
        // }
        // think of the next floor to go to

    }

    public void Failed()
    {
        elevator.PunishForPassengers();
        //AddReward((1-(float)m_TotalDecisions/maxDecisions)*-10000);
        EndEpisode();
    }
}
