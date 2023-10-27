using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Normal,

}
public class SkillData
{
    public string skillName = "Unit_DefaultSkillName";

    public SkillType skillType = SkillType.Normal;

    private List<Action> skillWorkList = new();
    private Queue<Action> workQueue = new();

    private Action skillCallBack = () => { };

    private float workQueueDelay = 0f;
    private float workQueueDelayTimer = 0f;

    /// <summary>
    /// 해당 SkillData를 사용하는 UnitData 의 Update Message에 계속 호출시켜준다.
    /// </summary>
    public void OnUpdate()
    {
        if(workQueue.Count > 0)
        {
            if(workQueueDelayTimer <= 0f)
            {
                workQueue.Dequeue()?.Invoke();

                workQueueDelayTimer = workQueueDelay;
            }
            else
            {
                workQueueDelayTimer -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Action List에 들어가 있는 모든 Action을 순서대로 스킬 실행큐에 넣어준다.
    /// </summary>
    public void DoSkillWork(float workDelay, Action callBack)
    {
        for(int i = 0; i < skillWorkList.Count; ++i)
        {
            workQueue.Enqueue(skillWorkList[i]);
        }

        workQueueDelay = workDelay;
        workQueueDelayTimer = 0f;
    }

    /// <summary>
    /// skillWorkList에 Action을 새로 추가해준다. 무명 함수는 추가하면 제거할 수 없다.
    /// </summary>
    /// <param name="act"></param>
    public void AddWorkList(Action act)
    {
        if (!skillWorkList.Contains(act))
        {
            skillWorkList.Add(act);
        }
    }

    /// <summary>
    /// skillWorkList에 들어가있는 Action을 제거해준다. 무명 함수는 추가하면 제거할 수 없다.
    /// </summary>
    /// <param name="act"></param>
    public void RemoveWorkList(Action act)
    {
        if (skillWorkList.Contains(act))
        {
            skillWorkList.Remove(act);
        }
    }
}
