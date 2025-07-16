using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PayoffEventData", menuName = "Scriptable Objects/PayoffEventData")]
public class PayoffEventData : ScriptableObject
{
    public enum processGrade
    {
        Good,
        Normal,
        Fail,
        unknown
    }
    public struct Grade
    {
        public processGrade gradeStatus;
        public float MinGrade;
        public float MaxGrade;
        public float GradeConst;
    }
   public List<Grade> Grades;
    
    public string PayoffEventName;
    public StatType usedStat;
    public int maxMoneyReward;
    public int minMoneyReward;
    public int tickCount = 3;
    public float countConst;

}
