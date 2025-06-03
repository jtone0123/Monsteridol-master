using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using RoomPlacementSystem;


public class RoomSchedule : MonoBehaviour, IPointerClickHandler
{
    public RoomData roomData;

    private List<GameObject> holdSchedule = new List<GameObject>();
    private float oringinalScheduleTurnLate;

    private float CurrentTurnLate;

    private void Awake()
    {
        if (roomData != null)
        {
           
            holdSchedule.AddRange(roomData.GeneratedSchdules);
            oringinalScheduleTurnLate = roomData.scheduleTurnLate;
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
        ResetSchedule();
    }

    public void CalculateScheduleTurn(float lessturn)
    {
        CurrentTurnLate -= lessturn;
        Debug.Log("턴 쿨 줄었음");
    }

    public void ResetSchedule()
    {
        CurrentTurnLate = oringinalScheduleTurnLate;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("클릭됨");
        GetSchedule();
    }

    public void GetSchedule()
    {
        if (CurrentTurnLate <= 0)
        {
            AvailableScheduleManager.Instance.AddSchedule(holdSchedule);
            ResetSchedule();
        }
        else if(CurrentTurnLate >0)
        {
            Debug.Log("쿨 남음");
            return;
        }
    }

    

  
    
    void Update()
    {
        
    }

   
}
