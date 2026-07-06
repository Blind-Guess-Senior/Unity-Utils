using Artifact.Utils.Attributes.UnityAttributes;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace Artifact.Utils.Editor.PropertiesDrawer
{
    /// <summary>
    /// Property drawer for fields that has <c>[ReadOnly]</c> attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        #region Override Methods

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var wasEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = wasEnabled;
        }

        #endregion
    }
}