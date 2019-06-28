//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2016 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Tools for the editor
/// </summary>

static public class NGUIEditorTools
{
    static public bool minimalisticLook
    {
        get { return GetBool("NGUI Minimalistic", false); }
        set { SetBool("NGUI Minimalistic", value); }
    }

    /// <summary>
    /// Get the previously saved boolean value.
    /// </summary>

    static public bool GetBool(string name, bool defaultValue) { return EditorPrefs.GetBool(name, defaultValue); }

    /// <summary>
    /// Save the specified boolean value in settings.
    /// </summary>

    static public void SetBool(string name, bool val) { EditorPrefs.SetBool(name, val); }
    /// <summary>
    /// Draw a distinctly different looking header label
    /// </summary>

    static public bool DrawHeader (string text) { return DrawHeader(text, text, false, minimalisticLook); }

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text, string key) { return DrawHeader(text, key, false, minimalisticLook); }

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text, bool detailed) { return DrawHeader(text, text, detailed, !detailed); }

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader (string text, string key, bool forceOn, bool minimalistic)
	{
		bool state = EditorPrefs.GetBool(key, true);

		if (!minimalistic) GUILayout.Space(3f);
		if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
		GUILayout.BeginHorizontal();
		GUI.changed = false;

		if (minimalistic)
		{
			if (state) text = "\u25BC" + (char)0x200a + text;
			else text = "\u25BA" + (char)0x200a + text;

			GUILayout.BeginHorizontal();
			GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
			if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
			GUI.contentColor = Color.white;
			GUILayout.EndHorizontal();
		}
		else
		{
			text = "<b><size=11>" + text + "</size></b>";
			if (state) text = "\u25BC " + text;
			else text = "\u25BA " + text;
			if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
		}

		if (GUI.changed) EditorPrefs.SetBool(key, state);

		if (!minimalistic) GUILayout.Space(2f);
		GUILayout.EndHorizontal();
		GUI.backgroundColor = Color.white;
		if (!forceOn && !state) GUILayout.Space(3f);
		return state;
	}

	/// <summary>
	/// Begin drawing the content area.
	/// </summary>

	static public void BeginContents () { BeginContents(minimalisticLook); }

	static bool mEndHorizontal = false;

	/// <summary>
	/// Begin drawing the content area.
	/// </summary>

	static public void BeginContents (bool minimalistic)
	{
		if (!minimalistic)
		{
			mEndHorizontal = true;
			GUILayout.BeginHorizontal();
			EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
		}
		else
		{
			mEndHorizontal = false;
			EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
			GUILayout.Space(10f);
		}
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}

	/// <summary>
	/// End drawing the content area.
	/// </summary>

	static public void EndContents ()
	{
		GUILayout.Space(3f);

		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		if (mEndHorizontal)
		{
			GUILayout.Space(3f);
			GUILayout.EndHorizontal();
		}

		GUILayout.Space(3f);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty (this SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(null, serializedObject, property, false, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty (this SerializedObject serializedObject, string property, string label, params GUILayoutOption[] options)
	{
		return DrawProperty(label, serializedObject, property, false, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty (string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(label, serializedObject, property, false, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawPaddedProperty (this SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(null, serializedObject, property, true, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawPaddedProperty (string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(label, serializedObject, property, true, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty (string label, SerializedObject serializedObject, string property, bool padding, params GUILayoutOption[] options)
	{
		SerializedProperty sp = serializedObject.FindProperty(property);

		if (sp != null)
		{
			if (minimalisticLook) padding = false;

			if (padding) EditorGUILayout.BeginHorizontal();

			if (sp.isArray && sp.type != "string") DrawArray(serializedObject, property, label ?? property);
			else if (label != null) EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
			else EditorGUILayout.PropertyField(sp, options);

			if (padding)
			{
				NGUIEditorTools.DrawPadding();
				EditorGUILayout.EndHorizontal();
			}
		}
		else Debug.LogWarning("Unable to find property " + property);
		return sp;
	}

	/// <summary>
	/// Helper function that draws an array property.
	/// </summary>

	static public void DrawArray (this SerializedObject obj, string property, string title)
	{
		SerializedProperty sp = obj.FindProperty(property + ".Array.size");

		if (sp != null && NGUIEditorTools.DrawHeader(title))
		{
			NGUIEditorTools.BeginContents();
			int size = sp.intValue;
			int newSize = EditorGUILayout.IntField("Size", size);
			if (newSize != size) obj.FindProperty(property + ".Array.size").intValue = newSize;

			EditorGUI.indentLevel = 1;

			for (int i = 0; i < newSize; i++)
			{
				SerializedProperty p = obj.FindProperty(string.Format("{0}.Array.data[{1}]", property, i));
				if (p != null) EditorGUILayout.PropertyField(p);
			}
			EditorGUI.indentLevel = 0;
			NGUIEditorTools.EndContents();
		}
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public void DrawProperty (string label, SerializedProperty sp, params GUILayoutOption[] options)
	{
		DrawProperty(label, sp, true, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public void DrawProperty (string label, SerializedProperty sp, bool padding, params GUILayoutOption[] options)
	{
		if (sp != null)
		{
			if (padding) EditorGUILayout.BeginHorizontal();

			if (label != null) EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
			else EditorGUILayout.PropertyField(sp, options);

			if (padding)
			{
				NGUIEditorTools.DrawPadding();
				EditorGUILayout.EndHorizontal();
			}
		}
	}

	/// <summary>
	/// Helper function that draws a compact Vector4.
	/// </summary>

	static public void DrawBorderProperty (string name, SerializedObject serializedObject, string field)
	{
		if (serializedObject.FindProperty(field) != null)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(name, GUILayout.Width(75f));

				NGUIEditorTools.SetLabelWidth(50f);
				GUILayout.BeginVertical();
				NGUIEditorTools.DrawProperty("Left", serializedObject, field + ".x", GUILayout.MinWidth(80f));
				NGUIEditorTools.DrawProperty("Bottom", serializedObject, field + ".y", GUILayout.MinWidth(80f));
				GUILayout.EndVertical();

				GUILayout.BeginVertical();
				NGUIEditorTools.DrawProperty("Right", serializedObject, field + ".z", GUILayout.MinWidth(80f));
				NGUIEditorTools.DrawProperty("Top", serializedObject, field + ".w", GUILayout.MinWidth(80f));
				GUILayout.EndVertical();

				NGUIEditorTools.SetLabelWidth(80f);
			}
			GUILayout.EndHorizontal();
		}
	}

	/// <summary>
	/// Helper function that draws a compact Rect.
	/// </summary>

	static public void DrawRectProperty (string name, SerializedObject serializedObject, string field)
	{
		DrawRectProperty(name, serializedObject, field, 56f, 18f);
	}

	/// <summary>
	/// Helper function that draws a compact Rect.
	/// </summary>

	static public void DrawRectProperty (string name, SerializedObject serializedObject, string field, float labelWidth, float spacing)
	{
		if (serializedObject.FindProperty(field) != null)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(name, GUILayout.Width(labelWidth));

				NGUIEditorTools.SetLabelWidth(20f);
				GUILayout.BeginVertical();
				NGUIEditorTools.DrawProperty("X", serializedObject, field + ".x", GUILayout.MinWidth(50f));
				NGUIEditorTools.DrawProperty("Y", serializedObject, field + ".y", GUILayout.MinWidth(50f));
				GUILayout.EndVertical();

				NGUIEditorTools.SetLabelWidth(50f);
				GUILayout.BeginVertical();
				NGUIEditorTools.DrawProperty("Width", serializedObject, field + ".width", GUILayout.MinWidth(80f));
				NGUIEditorTools.DrawProperty("Height", serializedObject, field + ".height", GUILayout.MinWidth(80f));
				GUILayout.EndVertical();

				NGUIEditorTools.SetLabelWidth(80f);
				if (spacing != 0f) GUILayout.Space(spacing);
			}
			GUILayout.EndHorizontal();
		}
	}

    static public bool DrawPrefixButton(string text)
    {
        return GUILayout.Button(text, "DropDown", GUILayout.Width(76f));
    }

    static public bool DrawPrefixButton(string text, params GUILayoutOption[] options)
    {
        return GUILayout.Button(text, "DropDown", options);
    }

    static public int DrawPrefixList(int index, string[] list, params GUILayoutOption[] options)
    {
        return EditorGUILayout.Popup(index, list, "DropDown", options);
    }

    static public int DrawPrefixList(string text, int index, string[] list, params GUILayoutOption[] options)
    {
        return EditorGUILayout.Popup(text, index, list, "DropDown", options);
    }

    /// <summary>
    /// Unity 4.3 changed the way LookLikeControls works.
    /// </summary>

    static public void SetLabelWidth (float width)
	{
		EditorGUIUtility.labelWidth = width;
	}

	static public void DrawPadding ()
	{
		if (!minimalisticLook)
			GUILayout.Space(18f);
	}
}
