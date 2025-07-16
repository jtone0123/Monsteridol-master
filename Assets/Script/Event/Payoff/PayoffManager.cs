using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class PayoffManager : MonoBehaviour
{
    [Header("참여 아이돌 정보")]
    public List<IdolCharacter> attendedIdols;

    public PayoffEventData payoffEventData;
    public float executionDelay;
    public float progress;
    public float maxProgress;

    [Header("종합 스탯")]
    private int totalFans;
    private int totalStat;


    



    IEnumerator PayoffEventExecution(PayoffEventData payoffEvent)
    {
        
        //틱 당 적용
        for (int i = 0; i < payoffEvent.tickCount; i++)
        {
            progress += totalStat * payoffEvent.countConst;
            progress = Mathf.Clamp(progress, 0, maxProgress); Mathf.Clamp(progress, 0, maxProgress);
            yield return new WaitForSeconds(executionDelay);
        }

        //최종 적용


        int finalReward = Mathf.RoundToInt(GradeCalculate(progress) * totalFans);
        MoneyManager.Instance.AddMoney(finalReward);
        
        yield return null;
    }
    public float GradeCalculate(float progress)
    {
        foreach(var grade in payoffEventData.Grades)
        {
            if(progress <= grade.MaxGrade &&  progress >= grade.MinGrade)
            {
                return grade.GradeConst;
            }
        }
        return 0;
    }

    public void AddAtendedIdol(IdolCharacter attendedIdol)
    {
        attendedIdols.Add(attendedIdol);
    }
    
    public void RemoveAtendedIdol(IdolCharacter attendedIdol)
    {
        attendedIdols.Remove(attendedIdol);
    }
    public void SumFan()
    {
        totalFans = 0;
        foreach (IdolCharacter character in attendedIdols)
        {
            totalFans += character.fanCount;
        }
    }
    public void SumStat()
    {
        totalStat = 0;
        foreach (IdolCharacter character in attendedIdols)
        {
            totalStat += character.stats[payoffEventData.usedStat];
        }
    }

}
