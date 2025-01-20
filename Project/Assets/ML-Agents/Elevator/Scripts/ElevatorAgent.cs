using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class ElevatorAgent : Agent
{
    public Elevator elevator;

    [SerializeField]
    protected BufferSensorComponent m_BufferSensor;

    //[SerializeField] private int maxDecisions = 50;
    protected int m_decisions = 0;

    protected Dictionary<string, float> m_Data = new();

    [SerializeField] List<string> m_Reasons = new List<string>();

    //FileLogger in the scene
    private DataCollector m_FileLogger;

    public override void Initialize()
    {
        m_BufferSensor = GetComponent<BufferSensorComponent>();
    }

    private void Begin()
    {
        //find an object named "FileLogger" in the scene
        m_FileLogger = GameObject.Find("FileLogger").GetComponent<DataCollector>();

        m_Data.Add("Decisions", 0);
        m_Data.Add("Reward Total", 0);
        m_Data.Add("Time", 0);
        m_Data.Add("ShortestPickupTime", 0);
        m_Data.Add("LongestPickupTime", 0);
        m_Data.Add("AveragePickupTime", 0);
        m_Data.Add("ShortestDropoffTime", 0);
        m_Data.Add("LongestDropoffTime", 0);
        m_Data.Add("AverageDropoffTime", 0);
        m_Data.Add("TotalPickups", 0);
        m_Data.Add("TotalDropoffs", 0);
        foreach (string reason in m_Reasons)
        {
            Debug.Log("Adding reason " + reason);
            m_Data.Add(reason, 0);
        }

        Debug.Log("Data initialized");
    }

    private bool m_FirstEpisode = true;
    public override void OnEpisodeBegin()
    {
        if (!m_FirstEpisode)
        {
            if(m_FileLogger != null)
            {
                UpdateTimes();
                m_FileLogger.SubmitData(m_Data);
            }
            //reset the data
            ResetData();
        }
        else
        {
            Begin();
            m_FirstEpisode = false;
        }

        elevator.Reset();

    }

    private void UpdateTimes()
    {
        //update the average pickup time
        m_Data["AveragePickupTime"] = elevator.totalPickupTime / elevator.totalPickups;
        m_Data["AverageDropoffTime"] = elevator.totalDropoffTime / elevator.totalDropoffs;

        //update the shortest and longest pickup time
        if (elevator.totalPickupTime < m_Data["ShortestPickupTime"] || m_Data["ShortestPickupTime"] == 0)
        {
            m_Data["ShortestPickupTime"] = elevator.totalPickupTime;
        }
        if (elevator.totalPickupTime > m_Data["LongestPickupTime"])
        {
            m_Data["LongestPickupTime"] = elevator.totalPickupTime;
        }

        //update the shortest and longest dropoff time
        if (elevator.totalDropoffTime < m_Data["ShortestDropoffTime"] || m_Data["ShortestDropoffTime"] == 0)
        {
            m_Data["ShortestDropoffTime"] = elevator.totalDropoffTime;
        }
        if (elevator.totalDropoffTime > m_Data["LongestDropoffTime"])
        {
            m_Data["LongestDropoffTime"] = elevator.totalDropoffTime;
        }

        //update the total pickups and dropoffs
        m_Data["TotalPickups"] = elevator.totalPickups;

        m_Data["TotalDropoffs"] = elevator.totalDropoffs;


    }

    public void AddRewardCustom(string reason, float reward)
    {
        base.AddReward(reward);

        //check if reason is in m_Data if not debug log
        if (!m_Data.ContainsKey(reason))
        {
            Debug.Log("Reason " + reason + " is not in the data dictionary");
            return;
        }

        m_Data[reason] += reward;
        m_Data["Reward Total"] += reward;

       // Debug.Log("Reward added for " + reason + " with value " + reward);
    }

    public void SetRewardCustom(string reason, float reward)
    {
        var totalReward = GetCumulativeReward();
        base.SetReward(reward);

        //check if reason is in m_Data if not debug log
        if (!m_Data.ContainsKey(reason))
        {
            Debug.Log("Reason " + reason + " is not in the data dictionary");
            return;
        }

        m_Data[reason] += reward - totalReward;
        m_Data["Reward Total"] = reward;
    }

    // set all data to 0 again
    public void ResetData()
    {
        // Create a list to store the keys
        List<string> keys = new List<string>(m_Data.Keys);

        // Loop over the list of keys
        foreach (string key in keys)
        {
            m_Data[key] = 0;
        }
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
        m_Data["Decisions"]++;
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
