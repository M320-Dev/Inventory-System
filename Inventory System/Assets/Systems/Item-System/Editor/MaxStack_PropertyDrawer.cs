using ItemSystem.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.Editor
{
    [CustomPropertyDrawer(typeof(MaxStack))]
    public sealed class MaxStack_PropertyDrawer : PropertyDrawer
    {
        private readonly float LineHeightDelta = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            property.serializedObject.Update();

            SerializedProperty isStackableProperty = property.FindPropertyRelative("m_isStackable");
            SerializedProperty valueProperty = property.FindPropertyRelative("m_value");

            float positionY = position.y;
            Rect labelRect = new(position.x, positionY, position.width, EditorGUIUtility.singleLineHeight);
            positionY += LineHeightDelta;
            Rect isStackablelRect = new(position.x, positionY, position.width, EditorGUIUtility.singleLineHeight);
            positionY += LineHeightDelta;
            Rect valueRect = new(position.x, positionY, position.width, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label);
            if (property.isExpanded) 
            {
                EditorGUI.PropertyField(isStackablelRect, isStackableProperty);

                if (isStackableProperty.boolValue)
                {
                    valueProperty.intValue = Math.Max(2, valueProperty.intValue);
                    EditorGUI.PropertyField(valueRect, valueProperty);
                }
                else
                {
                    valueProperty.intValue = 1;
                    GUI.enabled = false;
                    EditorGUI.PropertyField(valueRect, valueProperty);
                    GUI.enabled = true;
                }
            }

            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded) height += 2f * LineHeightDelta;
            return height;
        }
    }
}
