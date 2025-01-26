using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField]
    public List<FloorPiece> floors = new List<FloorPiece>();

    [SerializeField]
    private List<Elevator> elevators = new List<Elevator>();

    [SerializeField]
    public float addPassengerTime = 5.0f;

    [SerializeField]
    public float addPassengerOffset = 0.5f;

    private float m_AddPassengerTimer = 0.0f;

    [SerializeField] private float m_ExtraPassengerChance = 0.15f;

    [SerializeField] private float m_VIPPassengerChance = 0.00f;

    //prefab for the passenger
    [SerializeField]
    public GameObject passengerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Elevator elevator in elevators)
        {
            elevator.SetBuilding(this);
            Debug.Log("Elevator set building");
        }
    }

    // Update is called once per frame
    void Update()
    {
        AddPassengers();
    }

    void AddPassengers()
    {
        m_AddPassengerTimer -= Time.deltaTime;
        if (m_AddPassengerTimer <= 0)
        {

            m_AddPassengerTimer = addPassengerTime + Random.Range(-addPassengerOffset, addPassengerOffset);
            int floor = Random.Range(0, floors.Count);
            int destination = Random.Range(0, floors.Count);
            while (destination == floor)
            {
                destination = Random.Range(0, floors.Count);
            }

            do
            {
                AddPassengerToFloor(floor, destination);
            }
            while(Random.value < m_ExtraPassengerChance);
        }
    }

    void AddPassengerToFloor(int floor, int destination)
    {
        //create a new passenger
        GameObject passengerObject = Instantiate(passengerPrefab);
        PassengerLogic passenger = passengerObject.GetComponent<PassengerLogic>();
        floors[floor].AddPassenger(passenger);
        passenger.SetDestination(destination);
        passenger.elevator = elevators[0];
        if(m_VIPPassengerChance > Random.value)
        {
            passenger.SetPriority(true);
        }
    }
}
