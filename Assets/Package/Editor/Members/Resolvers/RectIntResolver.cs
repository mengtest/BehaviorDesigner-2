﻿using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
    public class RectIntResolver : FieldResolver<RectIntField, RectInt>
    {
        public RectIntResolver(FieldInfo fieldInfo, BehaviorWindow window) : base(fieldInfo, window)
        {
        }

        protected override RectIntField CreateEditorField(FieldInfo fieldInfo)
        {
            return new RectIntField(fieldInfo.Name);
        }

        public static bool IsAcceptable(FieldInfo info)
        {
            return info.FieldType == typeof(RectInt);
        }
    }
}