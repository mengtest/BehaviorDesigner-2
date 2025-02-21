﻿using UnityEngine;

namespace BehaviorDesigner.Tasks.UnityVector3
{
    [TaskCategory("Vector3")]
    [TaskName("Distance (V3)")]
    [TaskDescription("Returns the distance between two Vector3s.")]
    public class Distance : Action
    {
        [SerializeField]
        public SharedVector3 firstVector3;
        [SerializeField]
        public SharedVector3 secondVector3;
        [SerializeField] [RequiredField]
        public SharedFloat storeResult;

        public override TaskStatus OnUpdate()
        {
            storeResult.Value = Vector3.Distance(firstVector3.Value, secondVector3.Value);
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            firstVector3 = Vector3.zero;
            secondVector3 = Vector3.zero;
            storeResult = 0;
        }
    }
}