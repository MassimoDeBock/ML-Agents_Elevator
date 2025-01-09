using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseElevatorBrain : MonoBehaviour
{
    protected Elevator Elevator;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void AssignElevator(Elevator newElevator)
    {
        Elevator = newElevator;
    }

    public virtual void Ponder()
    {

    }

    public virtual void Reward(float reward)
    {

    }


    public virtual int GetNextDestination()
    {
        int bestFloor = -1;
        float bestScore = 0;

        //create a list of scores for each floor
        List<float> scores = new List<float>();

        //give each floor a score based on the amount of people waiting / the distance from the current floor (in floors) + the amount of people in the elevator wanting to go to that floor
        for (int i = 0; i < Elevator.building.floors.Count; i++)
        {
            scores.Add(0);
            if (i == Elevator.currentFloor)
            {
                continue;
            }
            foreach (PassengerLogic passenger in Elevator.building.floors[i].passengers)
            {
                if (passenger.destinationFloor != i)
                {
                    scores[i] += 1;
                }
            }
            //score based on the amount of people in the elevator wanting to go to that floor
            foreach (PassengerLogic passenger in Elevator.passengers)
            {
                if (passenger.destinationFloor == i)
                {
                    scores[i]  += 1;
                }
            }
            //score based on the distance from the current floor
            var distance = Mathf.Abs(i - Elevator.currentFloor);
            scores[i]/=distance;
            if (scores[i]  > bestScore)
            {
                bestScore = scores[i] ;
                bestFloor = i;
            }
        }

        //loop over all floors we'll have to pass to get to the best floor
        if (Elevator.currentFloor < bestFloor)
        {
            for (int i = Elevator.currentFloor+1; i < bestFloor; i++)
            {
                if (scores[i] > 1)
                {
                    bestFloor = i;
                }
            }
        }
        else
        {
            for (int i = Elevator.currentFloor-1; i > bestFloor; i--)
            {
                if (scores[i] > 1)
                {
                    bestFloor = i;
                }
            }
        }


        if (bestFloor == -1)
        {
            //choose the middle floor
            bestFloor = Elevator.building.floors.Count / 2;
        }

        //debug log the list
        string log = "Scores: ";
        foreach (float score in scores)
        {
            log += score + ", ";
        }

        //Debug.Log(log);

        return bestFloor;
    }

    public virtual void Failed()
    {
    }
}
