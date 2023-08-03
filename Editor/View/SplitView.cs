using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace BehaviorDesigner
{
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> {}

        private string key;

        public void Init(BehaviorWindow window, int index)
        {
            key = $"BehaviorDesign.SplitView.{window.BehaviorFileId}.{index}";
            fixedPaneInitialDimension = EditorPrefs.GetFloat(key, fixedPaneInitialDimension);
        }

        public void Dispose()
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            FieldInfo fixedPaneDimension = GetType().BaseType.GetField("m_FixedPaneDimension", BindingFlags.Instance | BindingFlags.NonPublic);
            float value = (float) fixedPaneDimension.GetValue(this);
            value = value <= 0 ? fixedPaneInitialDimension : value;
            EditorPrefs.SetFloat(key, value);
        }
    }
}