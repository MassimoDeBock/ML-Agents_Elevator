using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ElevatorAgent_Two : ElevatorAgent
{
    [SerializeField] private BufferSensorComponent m_DestinationBuffer;
    public override void Initialize()
    {

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


        List<List<float>> destinations = new List<List<float>>();
        for (int i = 0; i < elevator.building.floors.Count; i++)
        {
            destinations.Add(elevator.building.floors[i].GetPassengerDestinationsCount());
        }

        //Todo: Potentially improve this
        //Get the max value of the destinations
        float maxDestinations = 1;
        foreach (List<float> dest in destinations)
        {
            foreach (float d in dest)
            {
                if (d > maxDestinations)
                {
                    maxDestinations = d;
                }
            }
        }

        //Normalize the destinations
        for (int i = 0; i < elevator.building.floors.Count; i++)
        {
            for (int j = 0; j < elevator.building.floors.Count; j++)
            {
                destinations[i][j] /= maxDestinations;
            }

            m_DestinationBuffer.AppendObservation(destinations[i].ToArray());
        }

    }
}
