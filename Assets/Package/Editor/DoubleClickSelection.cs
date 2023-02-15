﻿using UnityEditor;
using UnityEngine.UIElements;

namespace BehaviorDesigner.Editor
{
    public class DoubleClickSelection : MouseManipulator
    {
        private double time;
        private double duration;

        public System.Action onDoubleClicked;

        public DoubleClickSelection(System.Action callback, double duration = 0.3d)
        {
            time = EditorApplication.timeSinceStartup;
            onDoubleClicked = callback;
            this.duration = duration;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup - time < duration)
            {
                onDoubleClicked?.Invoke();
            }

            time = EditorApplication.timeSinceStartup;
        }
    }
}