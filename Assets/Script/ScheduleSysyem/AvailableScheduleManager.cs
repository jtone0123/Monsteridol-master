using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;


public class AvailableScheduleManager : MonoBehaviour
{

    //싱글톤
    public static AvailableScheduleManager Instance { get; private set; }

    public List<GameObject> availableSchedules = new List<GameObject>();
  

    public GameObject scheduleUI;

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

    

   
    public void AddSchedule(ScheduleData schData)
    {
           
        if (schData != null)
        {
            GameObject schUI = Instantiate(scheduleUI, availableSchelueZone.transform);
            schUI.GetComponent<DraggableScheduleItem>()?.SetUP(schData);
            availableSchedules.Add(schUI);
        }
        availableSchelueZone.RefreshLayout(true, false);
    }



    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
