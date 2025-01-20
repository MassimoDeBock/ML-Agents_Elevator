using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIElevatorController : BaseElevatorBrain
{
    [SerializeField]
    private ElevatorAgent m_Agent;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void AssignElevator(Elevator newElevator)
    {
        Elevator = newElevator;
        m_Agent.elevator = newElevator;
    }

    public override void Ponder()
    {
        m_Agent.Ponder();
    }

    public override void Reward(string reason, float reward)
    {
        m_Agent.AddRewardCustom(reason, reward);
    }

    public override int GetNextDestination()
    {
        return m_Agent.GetNextDestination();
    }

    public override void Failed()
    {
        m_Agent.Failed();
    }
}
