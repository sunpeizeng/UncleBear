using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(UIPanel), true)]
public class UIPanelEditor : Editor
{
    SerializedProperty _mSPLayer;
    SerializedProperty _mSPTestSortingOrder;
    UIPanel _mTarget;

    void OnEnable()
    {
        _mSPLayer = serializedObject.FindProperty("mLayer");
        _mSPTestSortingOrder = serializedObject.FindProperty("_mTestSortingOrder");
        _mTarget = (UIPanel)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DoozyUIHelper.VerticalSpace(8);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Layer");
        _mSPLayer.intValue = EditorGUILayout.IntField(_mSPLayer.intValue);
        EditorGUILayout.EndHorizontal();

        DoozyUIHelper.VerticalSpace(4);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Test Sorting Order");
        _mSPTestSortingOrder.intValue = EditorGUILayout.IntField(_mSPTestSortingOrder.intValue);
        EditorGUILayout.EndHorizontal();

        DoozyUIHelper.VerticalSpace(4);

        if (GUILayout.Button("Update Test Sorting Order", GUILayout.Height(EditorGUIUtility.singleLineHeight * 3)))
        {
            _mTarget.UpdateSortingOrder(_mSPTestSortingOrder.intValue);
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}