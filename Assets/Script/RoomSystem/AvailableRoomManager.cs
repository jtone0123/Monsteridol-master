using RoomPlacementSystem;
using System.Collections.Generic;
using UnityEngine;

public class AvailableRoomManager : MonoBehaviour
{
    public static AvailableRoomManager Instance { get; private set; }

    private List<GameObject> availableRooms = new List<GameObject>();

    public float LimitSchedule = 6f;
    public GameObject roomUIprefab;

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
    }

    
    public void AddSchedule(RoomData roomData = null, List<RoomData> roomDatas = null)
    {
        if (roomDatas != null)
        {
            foreach(RoomData data in roomDatas)
            {
                if (availableRooms.Count <= LimitSchedule)
                {
                    GameObject roomUI = Instantiate(roomUIprefab);
                    roomUI.GetComponent<DraggableRoomItem>()?.SetUp(data);
                    availableRooms.Add(roomUI);
                }
                //추후 수정 필요
            }
        }
        if (roomData != null)
        {
            GameObject roomUI = Instantiate(roomUIprefab);
            roomUI.GetComponent<DraggableRoomItem>()?.SetUp(roomData);
            availableRooms.Add(roomUI);
        }
        //UI
    }




    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
