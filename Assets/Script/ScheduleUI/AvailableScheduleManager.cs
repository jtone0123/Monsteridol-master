using UnityEngine;
using System.Collections.Generic;


public class AvailableScheduleManager : MonoBehaviour
{

    public static AvailableScheduleManager Instance { get; private set; }
    public List<GameObject> SchedulePrefabs = new List<GameObject>();
    private List<GameObject> availableSchedules = new List<GameObject>();
    public ScheduleDropZone availableSchelueZone;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    private void UpdateSchedule()
    {
       foreach (GameObject schedulePrefab in SchedulePrefabs)
        {
            availableSchedules.Add(Instantiate(schedulePrefab, availableSchelueZone.transform));
            
        }
        availableSchelueZone.RefreshLayout(true, false);
    }

    public void AddSchedule(List<GameObject> schedules)
    {
        foreach(GameObject schedule in schedules)
        {
            SchedulePrefabs.Add(schedule);
        }
        UpdateSchedule();
    }



    void Start()
    {
        if (SchedulePrefabs.Count > 0 && SchedulePrefabs != null)
        {
            UpdateSchedule();
            
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
