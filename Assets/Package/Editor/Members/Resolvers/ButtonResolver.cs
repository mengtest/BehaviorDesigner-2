using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace BehaviorDesigner.Editor
{
    public class ButtonResolver : IFieldResolver
    {
        private Button editorField;

        public ButtonResolver(object obj, MethodInfo methodInfo, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = methodInfo.Name;
            }

            Button button = new Button();
            button.text = ObjectNames.NicifyVariableName(name);
            button.clickable.clicked += () =>
            {
                methodInfo.Invoke(obj, null);
            };

            editorField = button;
        }

        public VisualElement EditorField
        {
            get { return editorField; }
        }

        public void Register()
        {
        }

        public void Register(SharedVariable variable)
        {
        }

        public void Restore(SharedVariable variable)
        {
        }

        public void Restore(Task task)
        {
        }

        public void Save(Task task)
        {
        }
    }
}