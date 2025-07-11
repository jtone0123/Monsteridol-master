using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using RoomPlacementSystem;


public class RoomSchedule : MonoBehaviour, IClickable
{
    public RoomData roomData;
    public List<RoomData.GeneratedScheduleInfo> schedulesTemp;
   
    

    private float CurrentTurnLate;

    private void Awake()
    {
        if (roomData != null)
        {
            schedulesTemp.AddRange(roomData.Schedules);
            
        }
        else
        {
            Debug.Log("데이터 없음");
        }

    }

    private void OnEnable()
    {
        
    }

    void Start()
    {
        TurnManager.instance.ChangeTurn += CalculateScheduleTurn;
       
    }

    public void CalculateScheduleTurn(float turnAmount)
    {
        for (int i = 0; i < schedulesTemp.Count; i++)
        {
            RoomData.GeneratedScheduleInfo sch = schedulesTemp[i];
            sch.scheduleTurnLate -= turnAmount;
        }
        
    }

    public void GetNormalSchedule(float turnAmount)
    {
        if(roomData.normalSchedulesInfo != null)
        {
            for (int i = 0; i < roomData.normalSchedulesInfo.Count; i++)
            {
                RoomData.normalScheduleInfo nomalSchTemp = roomData.normalSchedulesInfo[i];
                float tempSchduleCount = 0f;
                foreach (GameObject schUI in AvailableScheduleManager.Instance.availableSchedules)
                {
                    if (nomalSchTemp.normalSchedule == schUI.GetComponent<DraggableScheduleItem>().scheduleData)
                    {
                        tempSchduleCount++;
                    }
                }
                if (tempSchduleCount <= nomalSchTemp.normalScheduleLimit)
                {
                    AvailableScheduleManager.Instance.AddSchedule(nomalSchTemp.normalSchedule);
                }
            }
        }
     }


    public void OnClick()
    {
       
        GetSchedule();
    }
    public void GetSchedule()
    {
        if (roomData.Schedules != null)
        {
            for (int i = 0; i < schedulesTemp.Count; i++)
            {
                RoomData.GeneratedScheduleInfo sch = schedulesTemp[i];
                if (sch.scheduleTurnLate <= 0)
                {
                    RoomData.GeneratedScheduleInfo originalSch = roomData.Schedules[i];
                    AvailableScheduleManager.Instance.AddSchedule(sch.GeneratedSchdule);
                    sch.scheduleTurnLate = originalSch.scheduleTurnLate; //리셋
                }
            }
        }
    }

    

  
    
    void Update()
    {
        
    }

    
}
