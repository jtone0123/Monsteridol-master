using UnityEngine;

public class ScheduleNavType : MonoBehaviour
{

    public NavPoint targetNavPoint;
    public NavPoint.PointType type;

    public void FindNavPoint()
    {
        NavPoint[] foundNavPoints = FindObjectsByType<NavPoint>(FindObjectsSortMode.None);
        foreach (NavPoint navPoint in foundNavPoints)
        {
            if(navPoint.type == type)
            {
                targetNavPoint = navPoint;
            }
        }
    }
    //추후 수정 필요
    private void Awake()
    {
        
    }



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
