using System;
public class NumberCondition:JBTCondition
{
    public int Number;
    int curr;
    Main _main;
    public NumberCondition()
    {

    }
    public void SetKonwledgePool(Main m)
    {
        _main = m; 
    }
    public override bool Check()
    {
        return curr == Number;
    }

    public override JBTNodeState Update()
    {
        curr = _main.inputNumber ;


        ///返回值实际上用不着
        return JBTNodeState.fail;
    }
}
