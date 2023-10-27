using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStat
{
    public float hp = 10f;
    public float ap = 1f;
    public float dp = 1f;

    public float attackDistance = 0.0f;

    public bool movable = true;
    private bool skillWork = false;

    private Dictionary<string, SkillData> skillDataDict = new();
    private Queue<(string, float)> skillDataQueue = new();

    public void DoSKill(string key, float workDelay)
    {
        if (skillWork)
        {
            skillDataQueue.Enqueue((key, workDelay));
        }
        else
        {
            skillDataDict[key]?.DoSkillWork(workDelay, ()=>{
                skillWork = false;

                if(skillDataQueue.Count > 0)
                {
                    (string, float) data = skillDataQueue.Dequeue();

                    DoSKill(data.Item1, data.Item2);
                }
            });

            skillWork = true;
        }
    }

    public void OnUpdate()
    {
        foreach(var data in skillDataDict)
        {
            data.Value.OnUpdate();
        }
    }
}