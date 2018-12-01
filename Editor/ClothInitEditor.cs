using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ClothInit))]
public class ClothInitEditor : Editor 
{

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();


        ClothInit script = (ClothInit)target;

        //script.animator = (Animator)EditorGUILayout.ObjectField(
            //"Animator",
            //script.animator,
            //typeof(Animator),
            //true);

        script.maxDistance = EditorGUILayout.FloatField("Max Move Distance", script.maxDistance);

        script.clothGenerationType = (ClothInit.ClothInitType)EditorGUILayout.EnumPopup("Cloth Init Type", script.clothGenerationType);

        if (
            //script.clothGenerationType == ClothInit.ClothInitType.UV_Hard ||
          script.clothGenerationType == ClothInit.ClothInitType.UV_Gradient ||
           script.clothGenerationType == ClothInit.ClothInitType.UV_Gradient_Vert_Red// ||
           //script.clothGenerationType == ClothInit.ClothInitType.UV_Hard_Vert_Red
        )
        {
            script.uvThreshold = EditorGUILayout.Slider("UV Threshold", script.uvThreshold, 0, 1);
        }

        //if (script.clothGenerationType == ClothInit.ClothInitType.Vert_Red ||
        //   script.clothGenerationType == ClothInit.ClothInitType.UV_Hard_Vert_Red ||
        //   script.clothGenerationType == ClothInit.ClothInitType.UV_Gradient_Vert_Red)
        //{ 
        //script.minDistForMatch = EditorGUILayout.FloatField("Min Point Match Distance", script.minDistForMatch);
        //}




        if (GUILayout.Button("Go!"))
        {
            //Debug.Log("Went!");
            script.InitCloth();
        }
    }
}
