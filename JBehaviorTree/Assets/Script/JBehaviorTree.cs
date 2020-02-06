using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO 大多数状态都在使用叶节点的状态，但是实际上也该是采取使用自节点状态的方式
///      虽然目前使用叶节点不会有错五
/// </summary>

public enum JBTNodeState
{
    running,
    succ,
    fail
}
/// <summary>
/// 一个思路奇异的行为树。
/// 搜寻到行为节点，然后Update 行为节点
/// pre 与 curr期望是行为节点（JBNAction）
/// 行为节点：实现的重点在于Update，支持动态设置JBTAction。有JBehaviorTree控制Enter与Exit
/// 控制节点：重点在与DFS中  需要自己负责enter与Exit
/// 条件节点的重点也在DFS中
///
/// logError : 将DFS与Update分离会导致DFS判断是的状态与Update的状态冲突  尤其表现在actionNode，DFS进入actionNode时，会默认state为running，会覆盖上一次的状态
///            如果DFS时不更改为running，则这个action在整个树的循环中只会运行一次，，funk
///            这个问题似乎解决了，在OnExit中退出时处理，例如ActionA在Exit之后，当前帧不会再进入ActionA的Update中，再次进入时，其他控制节点枚举会忽略ActionA
/// </summary>
public class JBehaviorTree 
{
    public JBehaviorNode root;
    public JBehaviorNode curr;
    public JBehaviorNode pre;

    public void Update()
    {
        //查询当前可执行的节点
        curr = root.DFS();
        //当当前可执行的节点不是上次执行的节点是
        Trans();

        curr.Update();
        pre = curr;
    }

    public JBehaviorTree()
    {

    }

    void Trans()
    {
        if (null == pre || pre != curr)
        {
            if (pre != null)
                pre.Exit();
            if (curr!= null)
                curr.Enter();
        }
    }

    public void Init()
    {
        curr = root;
    }

   
}




#region JBehaviourNode 行为树节点
public abstract class JBehaviorNode
{
    public JBTNodeState state;
    public JBehaviorNode parent;
    public List<JBehaviorNode> children;
    // DFS时临时使用的节点
    protected JBehaviorNode _dfsNode;
    /// <summary>
    /// 当前节点正在执行的子节点
    /// </summary>
    protected JBehaviorNode runningNode;  
    public JBehaviorNode()
    {
        children = new List<JBehaviorNode>();
    }

    public virtual void Update()
    {

    }
    public virtual void Enter()
    {
        OnEnter();
    }

    public virtual void Exit()
    {
        OnExit();
    }

    /// <summary>
    /// 返回要执行的叶节点  条件或行为节点
    /// 在这里更新runningNode，注意RunningNode的范围为当前节点的子节点
    /// </summary>
    /// <returns></returns>
    public abstract JBehaviorNode DFS();
    public virtual void OnEnter()
    {
    }

    public virtual void OnExit()
    {
        
    }

}

#endregion 

#region 随机节点
/// <summary>
/// 随机节点
/// 这个节点实现时真的简单
/// 日后扩展一下实现能够对权重控制
/// 这个随机节点与随机选择节点不太相同，，随机选择节点是指改变枚举顺序的选择节点
/// </summary>
public class JBNRandom:JBehaviorNode
{
    int luckyNumber;
    public override JBehaviorNode DFS()
    {
        if (runningNode != null)
        {
            _dfsNode = runningNode.DFS();
            state = runningNode.state;
            if (runningNode.state != JBTNodeState.succ)
            {
                return _dfsNode;
            }
        }
        luckyNumber = Random.Range(0, children.Count);
        Debug.Log(luckyNumber);
        runningNode = children[luckyNumber];
        _dfsNode = runningNode.DFS();
        state = runningNode.state;
        return _dfsNode;

    }

    public override void OnExit()
    {
        base.OnExit();
        //runningNode = null; 这个似乎，，，
    }

}
#endregion

#region 选择节点

public class JBNSelector : JBehaviorNode
{
    int runningIndex = 0;
    JBehaviorNode node;
    /// <summary>
    ///  状态通过当前节点的子节点状态来判断，不能使用叶节点判断
    ///  获取到succ状态时，自身为succ并返回，不再执行接下来的子节点
    ///  取到failure状态时，继续枚举下一个子节点
    ///  取到running状态时，设置自身为running并种植枚举
    ///  子节点全部取完切没有找到succ，则返回fail
    /// </summary>
    /// <returns></returns>
    public override JBehaviorNode DFS()
    {
        state = JBTNodeState.fail;
       
        if (runningNode != null)
        {
            _dfsNode = runningNode.DFS();
            if (runningNode.state == JBTNodeState.succ)
            {
                Exit();
                state = JBTNodeState.succ;
                return _dfsNode;
            }else 
            if (runningNode.state == JBTNodeState.running)
            {
                state = JBTNodeState.running;
                return _dfsNode;
            } else
            if (runningNode.state == JBTNodeState.fail)
            {
                runningIndex++;
            }
        }
        for (int i = runningIndex; i < children.Count ; i++)
        {
            node = children[i];
            _dfsNode = node.DFS();
            ///选择节点，获取到成功时则退出
            if (node.state == JBTNodeState.succ)
            {
                state = JBTNodeState.succ;
                Exit();
                return _dfsNode;
            }
            //当前节点为运行中，则可以运行
            if (node.state == JBTNodeState.running)
            {
                state = JBTNodeState.running;
                runningNode = node;
                runningIndex = i;
                return _dfsNode;
            }
            //否则，如果时失败，则继续前行
        }

        //全部运行完毕，是指一个没有选择出来，就是fail

        state = JBTNodeState.fail;
        Exit();
        return _dfsNode;
    }
    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        runningNode = null;
        runningIndex = 0;
        base.OnExit();
    }
}


#endregion 

#region 顺序节点
/// <summary>
/// SequenceNode
/// 当有节点返回succ时，会执行下一个节点
/// </summary>
public class JBNSequence : JBehaviorNode
{
    int runningIndex = 0;
    JBehaviorNode node;


    /// <summary>
    /// 状态通过当前节点的子节点状态来判断，不能使用叶节点判断
    /// running时，只负责把自身状态改为running并返回runningNode
    /// fail时，不返回自身
    /// succ时，绝不返回自身
    /// </summary>
    /// <returns></returns>
    public override JBehaviorNode DFS()
    {
        state = JBTNodeState.fail;

        
        
        // 当当前运行节点不为空时，处理当前节点
        if (runningNode != null )
        {
            _dfsNode = runningNode.DFS();
            // 如果成功，则枚举下一节点
            if(runningNode.state == JBTNodeState.succ)
            {
                runningIndex++;
            }
            // 如果状态为失败，则退出该节点
            else if (runningNode.state == JBTNodeState.fail)
            {
                state = JBTNodeState.fail;
                Exit();
                return _dfsNode;

            }
            else  //如果是运行中   保持运行
            {
                state = JBTNodeState.running;
                return _dfsNode;
            }
        }
       
        if (runningIndex < children.Count)
        {
            for (int i = runningIndex; i < children.Count ; i++)
            {
                node = children[i];
                _dfsNode = node.DFS();
                if (node.state == JBTNodeState.fail)
                {
                    state = JBTNodeState.fail;
                    Exit();
                    return _dfsNode;
                }
                if (node.state == JBTNodeState.running)
                {
                    state = JBTNodeState.running;
                    runningNode = node;
                    runningIndex = i;
                    return _dfsNode;
                }

            }
        }
        
        //全部完成时，则定义为succ
        
        state = JBTNodeState.succ;
        runningNode = children[children.Count-1];
        Exit();
        return _dfsNode;
    }
    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {

        runningIndex = 0;
        runningNode = null;
        base.OnExit();
    }
    ///该节点不能是叶节点，所以没有具体Update

}

#endregion



#region 修饰节点
/// <summary>
/// SequenceNode
/// 保持running直到满足一定条件
/// 关于自定义条件  如果想要实现类似于until(customCondition) -->action  应该构造为
///                                 Decorator->seq -> conditon
///                                                -> action
/// </summary>
public class JBNDecorator : JBehaviorNode
{
    int runningIndex = 0;
    JBTNodeState succState = JBTNodeState.succ;
    /// <summary>
    /// 定义该节点的执行成功的条件
    /// 详细的说法时定义该节点的子节点为哪种状态时，当前节点为成功
    /// </summary>
    public void DefSuccState(JBTNodeState state)
    {
        succState = state;
    }
    /// <summary>
    /// 状态通过当前节点的子节点状态来判断，不能使用叶节点判断
    /// running时，只负责把自身状态改为running并返回runningNode
    /// fail时，不返回自身
    /// succ时，绝不返回自身
    /// </summary>
    /// <returns></returns>
    public override JBehaviorNode DFS()
    {
        if (children.Count != 1)
        {
            Debug.LogError("修饰节点的子节点数量应当为1却不为1");
        }
        state = JBTNodeState.running;
        runningNode = children[0];
        _dfsNode = runningNode.DFS();
        if (runningNode.state == succState)
        {
            state = JBTNodeState.succ;
            return _dfsNode;
        }

        state = JBTNodeState.running;
        return _dfsNode;

    }
    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        runningNode = null;
        base.OnExit();
    }
    ///该节点不能是叶节点，所以没有具体Update

}

#endregion


#region 并行节点
/// <summary>
/// SequenceNode
/// 保持running直到满足一定条件
/// </summary>
public class JBNParallel : JBehaviorNode
{
    int runningIndex = 0;


    int succCount;  //执行成功的子节点个数
    int failCount;  //执行失败的子节点个数
    /// <summary>
    /// 状态通过当前节点的子节点状态来判断，不能使用叶节点判断
    /// running时，只负责把自身状态改为running并返回runningNode
    /// fail时，不返回自身
    /// succ时，绝不返回自身
    /// </summary>
    /// <returns></returns>
    public override JBehaviorNode DFS()
    {
        succCount = 0;
        failCount = 0;
        foreach (JBehaviorNode child in children)
        {
            child.DFS();
            if (child.state == JBTNodeState.succ) succCount++;
            if (child.state == JBTNodeState.fail) failCount++;
        }
        
        if (succCount == children.Count)
        {
            state = JBTNodeState.succ;
            return this;
        }
        if (failCount != 0)
        {

            state = JBTNodeState.fail;
            return this;
        }
        state = JBTNodeState.running;
        return this;
    }
    public override void OnEnter()
    {
        base.OnEnter();
        foreach (JBehaviorNode child in children)
        {
            child.Enter();
        }
    }

    public override void OnExit()
    {
        Debug.LogError("OnExit");
        foreach (JBehaviorNode child in children)
        {
            child.Exit();
        }
        base.OnExit();
    }
    ///特殊情况 ，虽然该节点不是叶节点，但是也要Update
    public override void Update()
    {
        foreach (JBehaviorNode child in children)
        {
            child.Update();
        }
        base.Update();
    }
}

#endregion



#region 条件节点


/// <summary>
/// 返回running与succ
/// </summary>
public class JBNConditionNode : JBehaviorNode
{
    public JBTCondition condition;
    bool flag;
    public override void OnEnter()
    {
        condition.OnEnter();
    }
    //退出时将状态设置为running以便下次的寻找时能够在此执行
    public override void OnExit()
    {
        state = JBTNodeState.running;
        condition.OnExit();

    }


    //p
    public override void Update()
    {
        condition.Update();
        flag = condition.Check();
        if (flag)
        {
            state =  JBTNodeState.succ;
        }
        else
        {
            state = JBTNodeState.fail;
        }
    }

    public override JBehaviorNode DFS()
    {
        Update();   
        return this;
    }
}
#endregion

#region 行为节点

/// <summary>
/// 返回running与succ
/// </summary>
public class JBNActionNode:JBehaviorNode
{
    public JBTAction action;
    public override void OnEnter()
    {
        //state = JBTNodeState.running;
        action.OnEnter();
    }
    //退出时将状态设置为running以便下次的寻找时能够在此执行
    public override void OnExit()
    {
        state = JBTNodeState.running;
        action.OnExit();
        
    }


    //p
    public override void Update()
    {
        state = action.Update();
        // 当行为执行返回失败时，对于行为树的节点来说，是指节点执行结束，而节点结束标志也就是succ
        if (JBTNodeState.fail == state)
        {
            state = JBTNodeState.succ;
        }
    }

    public override JBehaviorNode DFS()
    {
        //state = JBTNodeState.running;
        return this;
    }
}
#endregion

#region 具体行为  ，由用户自己实现
public abstract class JBTAction
{

    /// <summary>
    /// 返回正在运行running或运行结束fail  
    /// </summary>
    /// <returns></returns>
    public abstract JBTNodeState Update();
    /// <summary>
    /// 设计中在树外调用，初始化树之前就已经进行了Init
    /// </summary>
    public virtual void Init()
    {

    }

    public virtual void OnEnter()
    {

    }

    public virtual void OnExit()
    {
        
    }
}
#endregion


#region JBTCondition 条件节点的条件
public abstract class JBTCondition
{
    /// <summary>
    /// 返回正在运行running或运行结束fail  
    /// </summary>
    /// <returns></returns>
    public abstract JBTNodeState Update();

    public abstract bool Check();

    /// <summary>
    /// 设计中在树外调用，初始化树之前就已经进行了Init
    /// </summary>
    public virtual void Init()
    {

    }

    public virtual void OnEnter()
    {

    }

    public virtual void OnExit()
    {

    }
}
#endregion