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
    /// �ش� SkillData�� ����ϴ� UnitData �� Update Message�� ��� ȣ������ش�.
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
    /// Action List�� �� �ִ� ��� Action�� ������� ��ų ����ť�� �־��ش�.
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
    /// skillWorkList�� Action�� ���� �߰����ش�. ���� �Լ��� �߰��ϸ� ������ �� ����.
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
    /// skillWorkList�� ���ִ� Action�� �������ش�. ���� �Լ��� �߰��ϸ� ������ �� ����.
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
