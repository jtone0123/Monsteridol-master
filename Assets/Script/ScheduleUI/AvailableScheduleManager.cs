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


    public void AddAvailableSchedule(List<GameObject> schedules)
    {
       foreach (GameObject schedule in schedules)
        {
            availableSchedules.Add(Instantiate(schedule, availableSchelueZone.transform));
            availableSchelueZone.RefreshLayout(true, false);
        }
    }



    void Start()
    {
        if (SchedulePrefabs.Count > 0 && SchedulePrefabs != null)
        {
            AddAvailableSchedule(SchedulePrefabs);
            
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
