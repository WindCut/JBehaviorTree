using System;
using UnityEngine;
public class ColorAction : JBTAction
{
    public Material mat;
    private float time;
    public float totalTime;
    public Color color;
    public override void Init()
    {
        base.Init();
    }

    public override void OnEnter()
    {
        time = 0;
        Debug.LogError("Enter");
        Debug.Log(color);
        mat.color =  color;
    }
    public override JBTNodeState Update()
    {
        time = time + Time.deltaTime;
       
        if (totalTime < time)
        {
              
            return JBTNodeState.fail;
        }
        return JBTNodeState.running;
    }

    public override void OnExit()
    {
        mat.color = Color.white;
    }
}


public class ShakeAction : JBTAction
{
    public Transform transform;
    private float time;
    public float totalTime;
    Vector3 nativePos;
    float num;
    public override void Init()
    {
        base.Init();
        nativePos = transform.position;
    }

    public override void OnEnter()
    {
        time = 0;
        Debug.LogError("Enter ShakeAciton");
    }
    public override JBTNodeState Update()
    {
        time = time + Time.deltaTime;
        if (totalTime < time)
        {
            return JBTNodeState.fail;
        }
        num = UnityEngine.Random.Range(0,100)/30.0f;
        transform.position = nativePos + new Vector3(num,num,num);
      
        return JBTNodeState.running;
    }

    public override void OnExit()
    {
        Debug.LogError("Exit ShakeAciton");
        transform.position = nativePos;
    }
}
