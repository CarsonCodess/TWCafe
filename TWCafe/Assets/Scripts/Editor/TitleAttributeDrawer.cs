using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TitleAttribute))]
public class TitleAttributeDrawer : DecoratorDrawer
{
    private static readonly float LineHeight = EditorGUIUtility.singleLineHeight * 2;

    public override void OnGUI(Rect position)
    {
        var titleAttribute = (TitleAttribute) attribute;

        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18
        };
        
        position.height = LineHeight;
        EditorGUI.LabelField(position, titleAttribute.Title, style);
    }

    public override float GetHeight()
    {
        return LineHeight;
    }
}