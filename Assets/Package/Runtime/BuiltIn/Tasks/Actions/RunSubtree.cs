using System;
using UnityEngine;

namespace BehaviorDesigner.Tasks
{
    [TaskDescription("Run a new behavior tree.")]
    public class RunSubtree : Action
    {
        [SerializeField]
        private ExternalBehavior behavior;
        [SerializeField]
        private bool syncVariables;
        [SerializeField]
        private bool resetVariables;

        [NonSerialized]
        private BehaviorSource source;
        private Root root;
        private bool isRestart;

#if UNITY_EDITOR
        private ExternalBehavior behaviorInstance;
#endif

        public override void OnStart()
        {
            base.OnStart();
            if (source == null)
            {
#if UNITY_EDITOR
                behaviorInstance = behavior.Clone();
                source = behaviorInstance.GetSource();
#else
                source = behavior.GetSource().Clone();
#endif
            }

            if (!isRestart)
            {
                source.Load();
                root = source.Root;
                root.Bind(source);
                root.Init(owner);
            }
            else if (resetVariables)
            {
                source.ReloadVariables();
                root.Bind(source);
            }

            if (syncVariables)
            {
                SyncVariables(owner.GetSource(), source);
            }

            root.OnStart();
            isRestart = true;
        }

        public override TaskStatus OnUpdate()
        {
            return root.OnUpdate();
        }

        public override void OnEnd()
        {
            if (syncVariables)
            {
                SyncVariables(source, owner.GetSource());
            }
        }

        public override void OnReset()
        {
            behavior = null;
            syncVariables = false;
            resetVariables = false;
        }

        private void SyncVariables(BehaviorSource s1, BehaviorSource s2)
        {
            foreach (SharedVariable variable in s1.GetVariables())
            {
                SharedVariable targetVariable = s2.GetVariable(variable.Name);
                if (targetVariable != null)
                {
                    if (targetVariable.GetType() == variable.GetType())
                    {
                        targetVariable.SetValue(variable.GetValue());
                    }
                }
            }
        }

#if UNITY_EDITOR
        [Button]
        private void OpenSubtree()
        {
            if (behaviorInstance)
            {
                UnityEditor.AssetDatabase.OpenAsset(behaviorInstance);
                return;
            }

            if (behavior)
            {
                UnityEditor.AssetDatabase.OpenAsset(behavior);
            }
        }
#endif
    }
}