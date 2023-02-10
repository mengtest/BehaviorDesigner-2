﻿using System;
using UnityEngine;

namespace BehaviorDesigner
{
    [Serializable]
    public class Composite : ParentTask
    {
        [SerializeField]
        protected AbortType abortType;

        public AbortType AbortType
        {
            get { return abortType; }
        }

        public override bool UpdateAbort()
        {
            int count = CanRunParallelChildren ? children.Count : currentChildIndex;
            for (int i = 0; i < count; i++)
            {
                Task child = children[i];
                if (UpdateAbort(child))
                {
                    return true;
                }
            }

            return false;
        }

        protected bool UpdateAbort(Task task)
        {
            if (task.IsDisabled)
            {
                return false;
            }

            if (task is Composite composite)
            {
                return composite.UpdateAbort();
            }
            else
            {
                bool canUpdateAbort = abortType == AbortType.Both ||
                                      abortType == AbortType.Self && CanExecute ||
                                      abortType == AbortType.LowerPriority && !CanExecute;
                if (canUpdateAbort)
                {
                    if (task is Decorator decorator)
                    {
                        return decorator.UpdateAbort();
                    }
                    else if (task is Conditional conditional)
                    {
                        TaskStatus status = conditional.CurrentStatus;
                        if (status != conditional.Update())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}