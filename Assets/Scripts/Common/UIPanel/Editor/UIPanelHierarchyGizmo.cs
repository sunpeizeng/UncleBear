using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class UIPanelHierarchyGizmo
{
    static UIPanelHierarchyGizmo()
    {
        EditorApplication.hierarchyWindowItemOnGUI += DrawIndicator;
    }

    private static void DrawIndicator(int instanceID, Rect rect)
    {
        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject == null)
        {
            return;
        }

        UIPanel panel = gameObject.GetComponent<UIPanel>();
        if (panel != null)
        {
            Color c = GUI.contentColor;
            GUIStyle style = new GUIStyle();
            Rect drawRect = new Rect(rect);
            drawRect.x += 200f;

            if (panel.IsActive)
            {
                style.normal.textColor = Color.green;
                EditorGUI.LabelField(drawRect, "Active", style);
            }
            else
            {
                style.normal.textColor = Color.red;
                EditorGUI.LabelField(drawRect, "Inactive", style);
            }
            GUI.contentColor = c;
        }
    }
}
