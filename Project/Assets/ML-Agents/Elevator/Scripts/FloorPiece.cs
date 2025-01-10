using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FloorPiece : MonoBehaviour
{
    [SerializeField]
    public int floorNumber = 1;

    //Todo: change this to not be a constant
    [SerializeField]
    private int maxFloorNumber = 5;

    //list of passengers on this floor
    [SerializeField]
    public List<PassengerLogic> passengers = new List<PassengerLogic>();

    [SerializeField] private int maxPassengersRow = 5;

    // waiting area plane for passengers
    [SerializeField]
    public GameObject waitingSocket;

    [SerializeField]
    public GameObject ExitSocket;

    [SerializeField]
    //color plate for the floor
    public GameObject colorPlate;

    private float MaxWaitingTime = 40.0f;

    private float WaitTime = 0.0f;

    public void AddPassenger(PassengerLogic passenger)
    {
        passengers.Add(passenger);
        //set parent of the passenger to the waiting area
        passenger.transform.parent = waitingSocket.transform;
        RelocatePassengers();
        if (passengers.Count == 1)
        {
            WaitTime = 0.0f;
        }
    }

    public void RemovePassenger(PassengerLogic passenger)
    {
        passengers.Remove(passenger);
        //RelocatePassengers();
        if(passengers.Count == 0)
        {
            WaitTime = 0.0f;
        }
    }

    public void SetFloorNumber(int floor)
    {
        floorNumber = floor;
    }
    // Start is called before the first frame update
    void Start()
    {
        RelocatePassengers();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (passengers.Count > 0)
        {
            WaitTime += Time.deltaTime;
        }
    }

    public float GetTimeNormalized()
    {
        return Mathf.Max(WaitTime / MaxWaitingTime,1.0f);
    }

    public void ChangeColorPlate()
    {
        Renderer hatRenderer = colorPlate.GetComponent<Renderer>();
        Color[] floorColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan , Color.black, Color.gray};

        hatRenderer.material.color = floorColors[floorNumber % floorColors.Length];
    }

    private const float PassengerSize = 1.2f;

    private void RelocatePassengers()
    {
        var waitingAreaPosition = waitingSocket.transform.position;
        // re-spread out the passengers over the waiting area
        int numPassengers = passengers.Count;

        //start from 1 side of the waiting area and have a distance between each passenger of passengerSize
        float start = waitingAreaPosition.x;

        //if there are more passengers than the maxPassengersRow, then we need to start a new row
        for(int i = 0; i < numPassengers; i++)
        {
            //calculate the position of the passenger
            float x = start + (i % maxPassengersRow) * PassengerSize;
            float z = waitingAreaPosition.z - (int)(i / maxPassengersRow) * PassengerSize;
            passengers[i].transform.position = new Vector3(x, waitingAreaPosition.y, z);
        }

    }

    public List<float> GetPassengerDestinationsCount()
    {
        List<float> destinations = new List<float>();
        for (int i = 0; i < maxFloorNumber; i++)
        {
            destinations.Add(0);
        }
        for (int i = 0; i < passengers.Count; i++)
        {
            int destination = passengers[i].destinationFloor;
            destinations[destination]++;
        }
        return destinations;
    }

    public void CleanFloor()
    {
        foreach (PassengerLogic passenger in passengers)
        {
            Destroy(passenger.gameObject);
        }
        passengers.Clear();

        WaitTime = 0.0f;
    }
}
