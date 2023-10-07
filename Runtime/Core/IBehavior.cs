using UnityEngine;

namespace BehaviorDesigner
{
    public interface IBehavior
    {
        int InstanceID { get; }

        Object GetObject(bool local = false);
        
        BehaviorSource GetSource(bool local = false);
    }
}