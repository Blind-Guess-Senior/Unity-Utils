using Extensions.Containers;
using UnityEditor;
using UnityEngine;

namespace Editor.ClassDrawer
{
    /// <summary>
    /// Inspector drawer for DictionarySerializable&lt;,&gt; class.
    /// </summary>
    [CustomPropertyDrawer(typeof(DictionarySerializable<,>), true)]
    public class DictionarySerializableDrawer : PropertyDrawer
    {
        #region Override Methods

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIContent listLabel = new GUIContent("Dictionary");
            GUIContent pairLabel = new GUIContent("New Key-Value Pair");

            // Get property object.
            SerializedProperty list = property.FindPropertyRelative("pairList");
            SerializedProperty pair = property.FindPropertyRelative("newPair");

            // Show properties.
            EditorGUILayout.LabelField(listLabel);
            // Show every single entry rather than show whole list.
            for (var i = 0; i < list.arraySize; i++)
            {
                // Reject direct modify in inspector.
                EditorGUI.BeginDisabledGroup(true);
                GUIContent elementLabel = new GUIContent("Element " + i);
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), elementLabel);
                EditorGUI.EndDisabledGroup();

                // Button for delete pair.
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "Delete Key-Value Pair indexed" + i);
                    list.DeleteArrayElementAtIndex(i);
                    property.serializedObject.ApplyModifiedProperties();
                }

                GUILayout.Space(10); // The fixed space on the right side.
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(20); // The fixed space between show space and modify space.
            EditorGUILayout.PropertyField(pair, pairLabel);

            // Button for adding pair.
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Add New Key-Value Pair");

                property.serializedObject.ApplyModifiedProperties();

                // Get relative object, method.
                var target = property.serializedObject.targetObject;
                var dictionary = fieldInfo.GetValue(target);
                var tryAddMethod = dictionary.GetType().GetMethod("TryAddPair");

                // Add key-value pair to dictionary.
                if (tryAddMethod == null)
                {
                    Debug.LogError($"This should never happen: {dictionary.GetType()} does not have TryAddPair().");
                }
                else if (!(bool)tryAddMethod.Invoke(dictionary, null))
                {
                    Debug.LogWarning("Duplicate key: {p.key} and {p.value}");
                }

                property.serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Space(10); // The fixed space on the right side.
            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }
}