using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class GradientSkyboxEditor : UnityEditor.MaterialEditor {

    public override void OnInspectorGUI() {
        serializedObject.Update();

		var theShader = serializedObject.FindProperty ("m_Shader"); 

		if (isVisible && !theShader.hasMultipleDifferentValues && theShader.objectReferenceValue != null) 
		{
			var keyWords = (target as Material).shaderKeywords;
			var snap = keyWords.Contains ("SNAP_ON");
			
            EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();
			
			snap = EditorGUILayout.Toggle ("Snap Stars", snap);

            if (EditorGUI.EndChangeCheck()) {
				var dirPitch = GetMaterialProperty(targets, "_DirectionPitch");
				var dirYaw = GetMaterialProperty(targets, "_DirectionYaw");

                var dirPitchRad = dirPitch.floatValue * Mathf.Deg2Rad;
                var dirYawRad = dirYaw.floatValue * Mathf.Deg2Rad;
                
                var direction = new Vector4(Mathf.Sin(dirPitchRad) * Mathf.Sin(dirYawRad), Mathf.Cos(dirPitchRad), 
				                            Mathf.Sin(dirPitchRad) * Mathf.Cos(dirYawRad), 0.0f);
                GetMaterialProperty(targets, "_Direction").vectorValue = direction;
                
                var keywords = new List<string> { snap ? "SNAP_ON" : "SNAP_OFF"};
                (target as Material).shaderKeywords = keywords.ToArray ();
                EditorUtility.SetDirty(target as Material);

                PropertiesChanged();
            }
        }
    }

}
