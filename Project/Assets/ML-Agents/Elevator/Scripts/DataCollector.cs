using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    [SerializeField]
    private bool CollectData = true;
    //FilePath
    private string fileName = "";

    [SerializeField]
    private string baseName = "ElevatorData";

    private Dictionary<int,Dictionary<string, float>> m_Buffer = new();

    private int m_Counter = 1;

    [SerializeField]
    private int m_WriteToFileAt = 10;

    [SerializeField] private bool LogFilePath = true;

    //Todo: make sure this path exists
    [SerializeField] private string basePath = "../Data/";

    //startTime
    private float startTime;

    // Start is called before the first frame update
    void Start()
    {
        //get the current date and time
        string dateTime = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
        //create a file name
        fileName = baseName + '-' + dateTime + ".csv";

        //start time
        startTime = Time.time + 0;
    }

    public void SubmitData(Dictionary<string, float> data)
    {
        Dictionary<string, float> dataCopy = new Dictionary<string, float>(data);

        if (!CollectData) return;

        //set time equal to the time from the start
        data["Time"] = GetTime();
        // Collect data
        m_Buffer.Add(m_Counter, dataCopy);
        m_Counter++;

        if (m_Counter % m_WriteToFileAt == 0)
        {
            WriteBufferToFile();
        }
    }

    //get time from start
    private float GetTime()
    {
        return Time.time - startTime;
    }

    private void WriteBufferToFile()
    {
        // Write data to file
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(basePath + fileName, true))
        {
            if (m_Counter == m_WriteToFileAt)
            {
                // Write header
                file.WriteLine("Iteration;" + string.Join(";", m_Buffer[1].Keys));
            }

            // Write data
            foreach (var data in m_Buffer)
            {
                file.WriteLine(data.Key + ";" + string.Join(";", data.Value.Values));
            }

            if (LogFilePath)
            {
                //debug Log the file path
                Debug.Log("Data written to: " + fileName);
            }
        }
        // Clear buffer
        m_Buffer.Clear();
    }
}
