using UnityEngine;
using System.Collections.Generic;


public class RoomSchedule : MonoBehaviour
{
    [Header("�� ���� ������")]
    public List<GameObject> holdSchedule = new List<GameObject>();
    

    public void GetSchedule()
    {
        if (holdSchedule.Count > 0)
        AvailableScheduleManager.Instance.AddSchedule(holdSchedule);
    }


    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
