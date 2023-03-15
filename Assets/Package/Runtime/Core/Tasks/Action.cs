namespace BehaviorDesigner
{
    public class Action : Task
    {
        public sealed override void Bind(BehaviorSource source)
        {
            base.Bind(source);
        }
        
        public sealed override void Init(Behavior behavior)
        {
            base.Init(behavior);
        }
    }
}