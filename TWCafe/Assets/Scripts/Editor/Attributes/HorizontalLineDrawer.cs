using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class HorizontalLineDrawer : OdinAttributeDrawer<HorizontalLineAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var attribute = Attribute;
        
        var position = EditorGUILayout.GetControlRect(false, attribute.Thickness + attribute.Padding);
        position.height = attribute.Thickness;
        position.y += attribute.Padding * 0.5f;
        
        EditorGUI.DrawRect(position, EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f));
        
        CallNextDrawer(label);
    }
}
