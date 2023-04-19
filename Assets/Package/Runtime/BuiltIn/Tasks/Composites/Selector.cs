﻿namespace BehaviorDesigner.Tasks
{
    [TaskDescription("The selector task is similar to an \"or\" operation. It will return success as soon as one of its child tasks return success. " +
                     "If a child task returns failure then it will sequentially run the next task. If no child task returns success then it will return failure.")]
    public class Selector : Composite
    {
        public override TaskStatus OnUpdate()
        {
            if (UpdateAbort())
            {
                if (CanExecute)
                {
                    children[currentChildIndex].OnAbort();
                }

                RestartAbort();
            }

            while (CanExecute)
            {
                if (children[currentChildIndex].IsDisabled)
                {
                    currentChildIndex++;
                }
                else
                {
                    Task task = children[currentChildIndex];
                    if (CanChildStart)
                    {
                        task.OnStart();
                    }

                    TaskStatus status = children[currentChildIndex].Update();
                    if (status == TaskStatus.Success || status == TaskStatus.Failure)
                    {
                        currentChildIndex++;
                        task.OnEnd();
                    }
#if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isPaused && status == TaskStatus.Failure)
                    {
                        return TaskStatus.Running;
                    }
#endif
                    if (status != TaskStatus.Failure)
                    {
                        return status;
                    }
                }
            }

            return TaskStatus.Failure;
        }
    }
}