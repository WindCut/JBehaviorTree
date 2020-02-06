using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    JBehaviorTree jbt;
    public Material material;
    public int inputNumber;
    // Start is called before the first frame update
    void Start()
    {
        //InitJBT();
        InitJBT_Seq();
        //InitJBT_Selector_Condition();
        //InitJBT_Parallel();
        //InitJBT_Decorator();    
    }

    // Update is called once per frame
    void Update()
    {
        //material.color = Color.red;
        jbt.Update();
    }




    void InitJBT_Parallel()
    {
        ColorAction ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 3;
        ca.color = Color.red;

        ShakeAction sa = new ShakeAction();
        sa.transform = transform;
        sa.totalTime = 1.3f;
        sa.Init();

        JBNActionNode ja1 = new JBNActionNode();
        ja1.action = ca;
        JBNActionNode ja2 = new JBNActionNode();
        ja2.action = sa;


        ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 4;
        ca.color = Color.green;
        JBNActionNode ja3 = new JBNActionNode();
        ja3.action = ca;

        sa = new ShakeAction();
        sa.totalTime = 1.3f;
        sa.transform = transform;
        sa.Init();
        JBNActionNode ja4 = new JBNActionNode();
        ja4.action = sa;

        JBNParallel r1 = new JBNParallel();
        r1.children.Add(ja1);
        r1.children.Add(ja2);

        JBNSequence r2 = new JBNSequence();
        r2.children.Add(ja3);
        r2.children.Add(ja4);


        jbt = new JBehaviorTree();
        jbt.root = new JBNSequence();
        jbt.root.children.Add(r1);
        jbt.root.children.Add(r2);
    }

    void InitJBT_Decorator()
    {
        ColorAction ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 3;
        ca.color = Color.red;

        JBNActionNode ja1 = new JBNActionNode();
        ja1.action = ca;


        NumberCondition nc1 = new NumberCondition();
        nc1.SetKonwledgePool(this);
        nc1.Number = 1;
        JBNConditionNode conditionNode1 = new JBNConditionNode();
        conditionNode1.condition = nc1;


        NumberCondition nc2 = new NumberCondition();
        nc2.SetKonwledgePool(this);
        nc2.Number = 3;
        JBNConditionNode conditionNode2 = new JBNConditionNode();
        conditionNode2.condition = nc2;

        JBNDecorator jd = new JBNDecorator();
        jd.DefSuccState(JBTNodeState.succ);
        jd.children.Add(conditionNode2);

        ShakeAction sa = new ShakeAction();
        sa.transform = transform;
        sa.totalTime = 3.0f;
        sa.Init();
        JBNActionNode ja2 = new JBNActionNode();
        ja2.action = sa;


        JBNSequence r1 = new JBNSequence();
        r1.children.Add(conditionNode1);
        r1.children.Add(ja1);

        JBNSequence r2 = new JBNSequence();
        
        r2.children.Add(jd);
        r2.children.Add(ja2);


        jbt = new JBehaviorTree();
        jbt.root = new JBNSequence();
        jbt.root.children.Add(r1);
        jbt.root.children.Add(r2);
    }


    void InitJBT_Seq()
    {
        ColorAction ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 3;
        ca.color = Color.red;

        JBNActionNode ja1 = new JBNActionNode();
        ja1.action = ca;

        ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 2;
        ca.color = Color.blue;
        JBNActionNode ja2 = new JBNActionNode();
        ja2.action = ca;


        ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 4   ;
        ca.color = Color.green;
        JBNActionNode ja3 = new JBNActionNode();
        ja3.action = ca;

        ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 2.2f;
        ca.color = Color.black;
        JBNActionNode ja4 = new JBNActionNode();
        ja4.action = ca;

        JBNSequence r1 = new JBNSequence();
        r1.children.Add(ja1);
        r1.children.Add(ja2);

        JBNSequence r2 = new JBNSequence();
        //r2.children.Add(ja2);
        r2.children.Add(ja3);
        r2.children.Add(ja4);


        jbt = new JBehaviorTree();
        jbt.root = new JBNSequence();
        jbt.root.children.Add(r1);
        jbt.root.children.Add(r2);
    }

    
    void InitJBT_Selector_Condition()
    {
        NumberCondition nc1 = new NumberCondition();
        nc1.SetKonwledgePool(this);
        nc1.Number = 2;
        JBNConditionNode conditionNode1 = new JBNConditionNode();
        conditionNode1.condition = nc1;

        ColorAction ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 3;
        ca.color = Color.red;

        JBNActionNode ja1 = new JBNActionNode();
        ja1.action = ca;

        ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 2;
        ca.color = Color.blue;
        JBNActionNode ja2 = new JBNActionNode();
        ja2.action = ca;


        NumberCondition nc2 = new NumberCondition();
        nc2.SetKonwledgePool(this);
        nc2.Number = 1;
        JBNConditionNode conditionNode2 = new JBNConditionNode();
        conditionNode2.condition = nc2;

        ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 4;
        ca.color = Color.green;
        JBNActionNode ja3 = new JBNActionNode();
        ja3.action = ca;

        ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 2.2f;
        ca.color = Color.black;
        JBNActionNode ja4 = new JBNActionNode();
        ja4.action = ca;

        JBNSequence r1 = new JBNSequence();
        r1.children.Add(conditionNode1);
        r1.children.Add(ja1);
        r1.children.Add(ja2);

        JBNSelector r2 = new JBNSelector();
        r2.children.Add(conditionNode2);
        r2.children.Add(ja3);
        r2.children.Add(ja4);


        jbt = new JBehaviorTree();
        jbt.root = new JBNSequence();
        jbt.root.children.Add(r1);
        jbt.root.children.Add(r2);

    }

    void InitJBT()
    {
        ColorAction ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 3;
        ca.color = Color.red;

        JBNActionNode ja1 = new JBNActionNode();
        ja1.action = ca;

        ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 2;
        ca.color = Color.blue;
        JBNActionNode ja2 = new JBNActionNode();
        ja2.action = ca;


        ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 1;
        ca.color = Color.green;
        JBNActionNode ja3 = new JBNActionNode();
        ja3.action = ca;

        ca = new ColorAction();
        ca.mat = material;
        ca.totalTime = 2.2f;
        ca.color = Color.black;
        JBNActionNode ja4 = new JBNActionNode();
        ja4.action = ca;

        JBNRandom r1 = new JBNRandom();
        r1.children.Add(ja1);
        r1.children.Add(ja2);

        JBNRandom r2 = new JBNRandom();
        r2.children.Add(ja2);
        r2.children.Add(ja3);
        r2.children.Add(ja4);


        jbt = new JBehaviorTree();
        jbt.root = new JBNRandom();
        jbt.root.children.Add(r1);
        jbt.root.children.Add(r2);
    }

    
}
