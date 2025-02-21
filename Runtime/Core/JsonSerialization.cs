﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviorDesigner
{
    public class JsonSerialization
    {
        private readonly StringBuilder builder = new StringBuilder();
        private readonly List<SerializableNode> serializableNodes = new List<SerializableNode>();
        private readonly List<SerializableField> serializableField = new List<SerializableField>();
        private readonly List<string> jsons = new List<string>();
        private readonly Dictionary<string, Object> unityObjectDict = new Dictionary<string, Object>();

        public void LoadUnityObjects(ref List<UnityObject> unityObjects)
        {
            unityObjectDict.Clear();
            for (int i = unityObjects.Count - 1; i >= 0; i--)
            {
                if (!unityObjects[i].obj)
                {
                    unityObjects.RemoveAt(i);
                }
            }

            foreach (UnityObject unityObject in unityObjects)
            {
                unityObjectDict.TryAdd(unityObject.id, unityObject.obj);
            }
        }

        public void SaveUnityObjects(ref List<UnityObject> unityObjects)
        {
            unityObjects.Clear();
            foreach (KeyValuePair<string, Object> kvPair in unityObjectDict)
            {
                if (!kvPair.Value)
                {
                    continue;
                }

                unityObjects.Add(new UnityObject()
                {
                    id = kvPair.Key,
                    obj = kvPair.Value
                });
            }
        }

        public string FromArray<T>(List<T> array)
        {
            if (array == null || array.Count == 0)
            {
                return null;
            }

            serializableField.Clear();
            foreach (T t in array)
            {
                SerializeObject(t);
                serializableField.Add(new SerializableField
                {
                    type = t.GetType().ToString(),
                    json = JsonUtility.ToJson(t)
                });
            }

            return InternalFromArray(serializableField);
        }

        public List<T> ToArray<T>(string json)
        {
            List<T> array = new List<T>();
            ToArray(json, array);
            return array;
        }

        public void ToArray<T>(string json, List<T> array)
        {
            array.Clear();
            if (string.IsNullOrEmpty(json) || json.Length < 3)
            {
                return;
            }

            try
            {
                InternalToArray(json, serializableField);
                foreach (SerializableField field in serializableField)
                {
                    T t = (T) JsonUtility.FromJson(field.json, Type.GetType(field.type));
                    DeserializeObject(t);
                    array.Add(t);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                array.Clear();
            }
        }

        public string FromTask(Task task, HashSet<Task> checkChildren = null)
        {
            serializableNodes.Clear();
            AddTaskToSerializedNodes(task, checkChildren);
            return InternalFromArray(serializableNodes);
        }

        public Task ToTask(string json)
        {
            try
            {
                InternalToArray(json, serializableNodes);
                ReadTaskFormSerializedNodes(0, out Task task);
                return task;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public string FromTaskArray(List<Task> tasks, HashSet<Task> checkChildren = null)
        {
            if (tasks == null || tasks.Count == 0)
            {
                return null;
            }

            jsons.Clear();
            foreach (Task task in tasks)
            {
                jsons.Add(FromTask(task, checkChildren));
            }

            return InternalFromArray(jsons);
        }

        public List<Task> ToTaskArray(string json)
        {
            List<Task> tasks = new List<Task>();
            ToTaskArray(json, tasks);
            return tasks;
        }

        public void ToTaskArray(string json, List<Task> tasks)
        {
            tasks.Clear();
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            try
            {
                InternalToArray(json, jsons);
                foreach (string subJson in jsons)
                {
                    tasks.Add(ToTask(subJson));
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                tasks.Clear();
            }
        }

        private void AddTaskToSerializedNodes(Task task, HashSet<Task> checkChildren)
        {
            ParentTask parentTask = task as ParentTask;
            int childCount = 0;
            if (parentTask != null)
            {
                childCount = parentTask.Children.Count;
                if (checkChildren != null)
                {
                    foreach (Task child in parentTask.Children)
                    {
                        if (!checkChildren.Contains(child))
                        {
                            childCount--;
                        }
                    }
                }
            }

            SerializeObject(task);
            serializableNodes.Add(new SerializableNode
            {
                type = task.GetType().ToString(),
                task = JsonUtility.ToJson(task),
                childCount = childCount,
            });

            if (parentTask != null)
            {
                foreach (Task child in parentTask.Children)
                {
                    if (checkChildren != null && !checkChildren.Contains(child))
                    {
                        continue;
                    }

                    AddTaskToSerializedNodes(child, checkChildren);
                }
            }
        }

        private int ReadTaskFormSerializedNodes(int index, out Task task)
        {
            SerializableNode node = serializableNodes[index];
            Type type = BehaviorUtils.GetType(node.type);
            Task newTask;
            if (type == null)
            {
                if (node.childCount > 0)
                {
                    UnknownParentTask unknownParentTask = (UnknownParentTask) JsonUtility.FromJson(node.task, typeof(UnknownParentTask));
                    unknownParentTask.unknownTaskType = node.type;
                    newTask = unknownParentTask;
                }
                else
                {
                    UnknownTask unknownTask = (UnknownTask) JsonUtility.FromJson(node.task, typeof(UnknownTask));
                    unknownTask.unknownTaskType = node.type;
                    newTask = unknownTask;
                }
            }
            else
            {
                newTask = (Task) JsonUtility.FromJson(node.task, type);
            }

            ParentTask newParentTask = newTask as ParentTask;
            DeserializeObject(newTask);
            for (int i = 0; i < node.childCount; i++)
            {
                index = ReadTaskFormSerializedNodes(++index, out Task childTask);
                newParentTask.Children.Add(childTask);
            }

            task = newTask;
            return index;
        }

        private string InternalFromArray<T>(List<T> array)
        {
            builder.Clear();
            builder.Append("[");
            bool isFirst = true;
            foreach (T t in array)
            {
                if (!isFirst)
                {
                    builder.Append(",");
                }

                builder.Append(typeof(T) == typeof(string) ? t : JsonUtility.ToJson(t));
                isFirst = false;
            }

            builder.Append("]");
            return builder.ToString();
        }

        private void InternalToArray<T>(string json, List<T> array)
        {
            array.Clear();
            string arrayJson = json.Remove(json.Length - 1, 1).Remove(0, 1);
            string splitChar = arrayJson.StartsWith("[") ? "[" : "{\"";
            string splitString = string.Concat(",", splitChar);
            string[] jsonArray = arrayJson.Split(splitString);
            bool isFirst = true;
            foreach (string subJson in jsonArray)
            {
                if (!isFirst)
                {
                    if (typeof(T) == typeof(string))
                    {
                        array.Add((T) (object) subJson.Insert(0, splitChar));
                    }
                    else
                    {
                        array.Add(JsonUtility.FromJson<T>(subJson.Insert(0, splitChar)));
                    }
                }
                else
                {
                    if (typeof(T) == typeof(string))
                    {
                        array.Add((T) (object) subJson);
                    }
                    else
                    {
                        array.Add(JsonUtility.FromJson<T>(subJson));
                    }
                }

                isFirst = false;
            }
        }

        private void SerializeObject(object source)
        {
            Type type = source.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null)
                {
                    continue;
                }

                if (field.FieldType.BaseType.IsGenericType && field.FieldType.BaseType.GetGenericTypeDefinition() == typeof(SharedList<>))
                {
                    Type[] args = field.FieldType.BaseType.GetGenericArguments();
                    if (args.Length == 1 && typeof(Object).IsAssignableFrom(args[0]))
                    {
                        if (field.GetValue(source) is SharedVariable variable)
                        {
                            if (source is Task task && variable.GetValue() is IList list)
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (list[i] is Object obj && obj)
                                    {
                                        unityObjectDict.TryAdd(string.Join("_", task.Id, field.Name, i), obj);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (field.FieldType.IsSubclassOf(typeof(SharedVariable)))
                {
                    if (field.GetValue(source) is SharedVariable variable)
                    {
                        if (source is Task task && variable.GetValue() is Object obj && obj)
                        {
                            unityObjectDict.TryAdd(string.Join("_", task.Id, field.Name), obj);
                        }
                    }
                }
                else if (typeof(Object).IsAssignableFrom(field.FieldType))
                {
                    if (field.GetValue(source) is Object obj && obj)
                    {
                        if (source is Task task)
                        {
                            unityObjectDict.TryAdd(string.Join("_", task.Id, field.Name), obj);
                        }
                        else if (source is SharedVariable variable)
                        {
                            unityObjectDict.TryAdd(variable.Name, obj);
                        }
                    }
                }
                else if (typeof(IList).IsAssignableFrom(field.FieldType))
                {
                    Type[] args = field.FieldType.GetGenericArguments();
                    if (args.Length == 1 && typeof(Object).IsAssignableFrom(args[0]))
                    {
                        if (field.GetValue(source) is IList list)
                        {
                            if (source is Task task)
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (list[i] is Object obj && obj)
                                    {
                                        unityObjectDict.TryAdd(string.Join("_", task.Id, field.Name, i), obj);
                                    }
                                }
                            }
                            else if (source is SharedVariable variable)
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (list[i] is Object obj && obj)
                                    {
                                        unityObjectDict.TryAdd(string.Join("_", variable.Name, i), obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DeserializeObject(object source)
        {
            Type type = source.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null)
                {
                    continue;
                }

                if (field.FieldType.BaseType.IsGenericType && field.FieldType.BaseType.GetGenericTypeDefinition() == typeof(SharedList<>))
                {
                    Type[] args = field.FieldType.BaseType.GetGenericArguments();
                    if (args.Length == 1 && typeof(Object).IsAssignableFrom(args[0]))
                    {
                        if (field.GetValue(source) is SharedVariable variable)
                        {
                            if (source is Task task && variable.GetValue() is IList list)
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (unityObjectDict.TryGetValue(string.Join("_", task.Id, field.Name, i), out Object value))
                                    {
                                        list[i] = value;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (field.FieldType.IsSubclassOf(typeof(SharedVariable)))
                {
                    if (field.GetValue(source) is SharedVariable variable)
                    {
                        if (source is Task task)
                        {
                            if (unityObjectDict.TryGetValue(string.Join("_", task.Id, field.Name), out Object value))
                            {
                                variable.SetValue(value);
                            }
                        }
                    }
                }
                else if (typeof(Object).IsAssignableFrom(field.FieldType))
                {
                    if (source is Task task)
                    {
                        if (unityObjectDict.TryGetValue(string.Join("_", task.Id, field.Name), out Object value))
                        {
                            field.SetValue(source, value);
                        }
                    }
                    else if (source is SharedVariable variable)
                    {
                        if (unityObjectDict.TryGetValue(variable.Name, out Object value))
                        {
                            field.SetValue(source, value);
                        }
                    }
                }
                else if (typeof(IList).IsAssignableFrom(field.FieldType))
                {
                    Type[] args = field.FieldType.GetGenericArguments();
                    if (args.Length == 1 && typeof(Object).IsAssignableFrom(args[0]))
                    {
                        if (field.GetValue(source) is IList list)
                        {
                            if (source is Task task)
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (unityObjectDict.TryGetValue(string.Join("_", task.Id, field.Name, i), out Object value))
                                    {
                                        list[i] = value;
                                    }
                                }
                            }
                            else if (source is SharedVariable variable)
                            {
                                for (int i = 0; i < list.Count; i++)
                                {
                                    if (unityObjectDict.TryGetValue(string.Join("_", variable.Name, i), out Object value))
                                    {
                                        list[i] = value;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Serializable]
        public struct SerializableNode
        {
            public string type;
            public string task;
            public int childCount;
        }

        [Serializable]
        public struct SerializableField
        {
            public string type;
            public string json;
        }
    }
}