using UnityEngine;

public class ScheduleNavType : MonoBehaviour
{

    public NavPoint navPoint;
    public NavPoint.PointType type;

    public void FindNavPoint()
    {
        NavPoint[] foundNavPoints = FindObjectsByType<NavPoint>(FindObjectsSortMode.None);
        foreach (NavPoint navPoint in foundNavPoints)
        {
            if(navPoint.type == type)
            {
                
            }
        }
    }

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
