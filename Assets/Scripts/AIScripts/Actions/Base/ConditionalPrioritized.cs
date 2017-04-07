using BehaviorDesigner.Runtime.Tasks;

public class ConditionalPrioritized : Conditional
{
    public int Priority = 1;

    public override float GetPriority()
    {
        return Priority;
    }
}
