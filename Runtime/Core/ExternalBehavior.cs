using UnityEngine;

namespace BehaviorDesigner
{
    public class ExternalBehavior : ScriptableObject, IBehavior
    {
        [SerializeField]
        private BehaviorSource source;

        public int InstanceID
        {
            get { return GetInstanceID(); }
        }

        public Object GetObject(bool local = false)
        {
            return this;
        }

        public BehaviorSource GetSource(bool local = false)
        {
            if (source == null)
            {
                source = new BehaviorSource();
            }

            return source;
        }

        public void BindVariables(Task task)
        {
            GetSource().BindVariables(task);
        }

#if UNITY_EDITOR
        public ExternalBehavior Clone()
        {
            ExternalBehavior behavior = CreateInstance<ExternalBehavior>();
            behavior.name = string.Concat(name, " (Clone)");
            behavior.source = source.Clone();
            return behavior;
        }
#endif
    }
}