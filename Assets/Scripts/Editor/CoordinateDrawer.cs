using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Coordinate))]
public class CoordinateDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();

        var popupWindow = new UnityEngine.UIElements.PopupWindow() { text = property.name.ToUpper(),  };

        // Create property fields.
        var anchorField = new PropertyField(property.FindPropertyRelative("anchor"), "Anchor");
        var offsetField = new PropertyField(property.FindPropertyRelative("offset"), "Offset From Anchor");

        // Add fields to the container.
        popupWindow.Add(anchorField);
        popupWindow.Add(offsetField);

        container.Add(popupWindow);

        return container;
    }
}
