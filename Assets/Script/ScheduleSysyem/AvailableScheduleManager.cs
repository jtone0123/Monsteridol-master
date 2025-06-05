using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;


public class AvailableScheduleManager : MonoBehaviour
{

    //싱글톤
    public static AvailableScheduleManager Instance { get; private set; }

    private List<GameObject> availableSchedules = new List<GameObject>();
    public List<GameObject> SchedulePrefabs = new List<GameObject>();

    public ScheduleDropZone availableSchelueZone;

    public float LimitSchedule = 6f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        //availableSchelueZone = FindAnyObjectByType<ScheduleDropZone>();
        if (availableSchelueZone == null)
        {
            Debug.Log("스케줄 이용 창 없음");
        }

        
    }

    

    public void UpdateSchedule()
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
            availableSchedules.Add(Instantiate(schedule, availableSchelueZone.transform));
        }
        availableSchelueZone.RefreshLayout(true, false);
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
