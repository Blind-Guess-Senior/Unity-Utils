using Artifact.Utils.Attributes;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace Artifact.Utils.Editor.PropertiesDrawer
{
    /// <summary>
    /// Property drawer for fields that has [ReadOnly] attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        #region Override Methods

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

        #endregion
    }
}