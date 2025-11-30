using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(EntityData))]
public class EntityDataEditor : Editor
{
    private SerializedProperty behaviorsProp;
    private ReorderableList behaviorList;

    private static Type[] behaviorTypes;
    private static Type[] conditionTypes;

    private void OnEnable()
    {
        behaviorsProp = serializedObject.FindProperty("defaultBehaviors");

        // Cache all available behavior and condition types
        behaviorTypes = GetAllSubclassesOf(typeof(EntityBehaviorBase));
        conditionTypes = GetAllSubclassesOf(typeof(IBehaviorCondition));

        behaviorList = new ReorderableList(serializedObject, behaviorsProp, true, true, true, true);
        behaviorList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Default Behaviors");

        behaviorList.drawElementCallback = DrawBehaviorElement;
        behaviorList.onAddCallback = AddBehaviorElement;
        behaviorList.elementHeightCallback = GetBehaviorElementHeight;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all other fields
        DrawPropertiesExcluding(serializedObject, "defaultBehaviors");

        EditorGUILayout.Space();
        behaviorList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
    private void DrawBehaviorElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var element = behaviorsProp.GetArrayElementAtIndex(index);

        // Start a vertical box for this behavior
        GUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"Behavior #{index + 1}", EditorStyles.boldLabel);

        // Priority and blockOthers
        EditorGUILayout.PropertyField(element.FindPropertyRelative("priority"), new GUIContent("Priority"));
        EditorGUILayout.PropertyField(element.FindPropertyRelative("blockOthers"), new GUIContent("Block Others"));

        // Behavior type selector
        var behaviorProp = element.FindPropertyRelative("behavior");
        Type currentType = GetManagedReferenceType(behaviorProp);
        int selected = Array.IndexOf(behaviorTypes, currentType);
        int newSelected = EditorGUILayout.Popup("Behavior Type", selected, behaviorTypes.Select(t => t.Name).ToArray());
        if (newSelected != selected && newSelected >= 0)
        {
            behaviorProp.managedReferenceValue = Activator.CreateInstance(behaviorTypes[newSelected]);
        }

        // Behavior fields
        if (behaviorProp.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            SerializedProperty iterator = behaviorProp.Copy();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
            EditorGUI.indentLevel--;
        }

        // Conditions
        var conditionsProp = element.FindPropertyRelative("conditions");
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

        for (int c = 0; c < conditionsProp.arraySize; c++)
        {
            SerializedProperty condElement = conditionsProp.GetArrayElementAtIndex(c);
            Type condType = GetManagedReferenceType(condElement);
            int condSelected = Array.IndexOf(conditionTypes, condType);
            int condNewSelected = EditorGUILayout.Popup(condType != null ? condType.Name : "Condition Type", condSelected, conditionTypes.Select(t => t.Name).ToArray());
            if (condNewSelected != condSelected && condNewSelected >= 0)
            {
                condElement.managedReferenceValue = Activator.CreateInstance(conditionTypes[condNewSelected]);
            }

            EditorGUI.indentLevel++;
            SerializedProperty condIter = condElement.Copy();
            condIter.NextVisible(true);
            while (condIter.NextVisible(false))
            {
                EditorGUILayout.PropertyField(condIter, true);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        // Add/remove condition buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Condition", GUILayout.Width(100)))
        {
            conditionsProp.arraySize++;
            conditionsProp.GetArrayElementAtIndex(conditionsProp.arraySize - 1).managedReferenceValue =
                Activator.CreateInstance(conditionTypes[0]);
        }
        if (GUILayout.Button("- Condition", GUILayout.Width(100)) && conditionsProp.arraySize > 0)
        {
            conditionsProp.arraySize--;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void AddBehaviorElement(ReorderableList list)
    {
        behaviorsProp.arraySize++;
        var newElement = behaviorsProp.GetArrayElementAtIndex(behaviorsProp.arraySize - 1);

        // Create a new ConditionalBehavior and assign a default behavior
        var conditional = new ConditionalBehavior
        {
            behavior = Activator.CreateInstance(behaviorTypes[0]) as EntityBehaviorBase,
            conditions = new List<IBehaviorCondition>(),
            priority = 50,
            blockOthers = false
        };
        newElement.managedReferenceValue = conditional;
    }

    private float GetBehaviorElementHeight(int index)
    {
        var element = behaviorsProp.GetArrayElementAtIndex(index);
        float height = EditorGUIUtility.singleLineHeight + 4; // Type selector

        if (element.managedReferenceValue != null)
        {
            SerializedProperty iterator = element.Copy();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                if (iterator.name == "conditions") break;
                height += EditorGUIUtility.singleLineHeight + 2;
            }

            SerializedProperty conditionsProp = element.FindPropertyRelative("conditions");
            if (conditionsProp != null)
            {
                height += EditorGUIUtility.singleLineHeight + 2; // "Conditions" label
                for (int c = 0; c < conditionsProp.arraySize; c++)
                {
                    height += EditorGUIUtility.singleLineHeight + 2; // Type selector
                    SerializedProperty condIter = conditionsProp.GetArrayElementAtIndex(c).Copy();
                    condIter.NextVisible(true);
                    while (condIter.NextVisible(false))
                    {
                        height += EditorGUIUtility.singleLineHeight + 2;
                    }
                }
                height += EditorGUIUtility.singleLineHeight + 2; // Add/remove buttons
            }
        }
        return height;
    }

    private static Type[] GetAllSubclassesOf(Type baseType)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => baseType.IsAssignableFrom(t) && t != baseType && !t.IsAbstract)
            .ToArray();
    }

    private static Type GetManagedReferenceType(SerializedProperty prop)
    {
        if (prop == null) return null;
        string typeName = prop.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(typeName)) return null;
        string[] split = typeName.Split(' ');
        if (split.Length != 2) return null;
        return Type.GetType($"{split[1]}, {split[0]}");
    }
}