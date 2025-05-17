using UnityEditor;
using UnityEngine;
using System;

[Serializable]
public class BoolGrid5x5
{
	[HideInInspector]
	public bool[] cells = new bool[25]; // flat 5x5 grid

	public bool Get(int x, int y) => cells[y * 5 + x];
	public void Set(int x, int y, bool value) => cells[y * 5 + x] = value;
}

[CustomPropertyDrawer(typeof(BoolGrid5x5))]
public class BoolGrid5x5Drawer : PropertyDrawer
{
	private const int GridSize = 5;
	private const float ToggleSize = 18f;
	private const float Padding = 2f;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUIUtility.singleLineHeight + (ToggleSize + Padding) * GridSize + 10f; // extra space
	}


	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty cellsProp = property.FindPropertyRelative("cells");

		// Draw label in bold
		GUIStyle boldLabel = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
		Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		EditorGUI.LabelField(labelRect, label, boldLabel);

		position.y += EditorGUIUtility.singleLineHeight + Padding;

		// Draw 5x5 toggle grid (excluding center)
		int cellIndex = 0;
		for (int y = 0; y < GridSize; y++)
		{
			for (int x = 0; x < GridSize; x++)
			{
				if (x == 2 && y == 2)
				{
					// Optional visual placeholder
					Rect placeholder = new Rect(
						position.x + x * (ToggleSize + Padding),
						position.y + y * (ToggleSize + Padding),
						ToggleSize,
						ToggleSize
					);
					EditorGUI.LabelField(placeholder, "", EditorStyles.centeredGreyMiniLabel);
					cellIndex++;
					continue;
				}

				Rect toggleRect = new Rect(
					position.x + x * (ToggleSize + Padding),
					position.y + y * (ToggleSize + Padding),
					ToggleSize,
					ToggleSize
				);

				SerializedProperty cellProp = cellsProp.GetArrayElementAtIndex(cellIndex);
				cellProp.boolValue = EditorGUI.Toggle(toggleRect, cellProp.boolValue);
				cellIndex++;
			}
		}

		// Add extra vertical spacing or separator
		position.y += (ToggleSize + Padding) * GridSize;

		// Optional: draw a horizontal line separator
		var separatorRect = new Rect(position.x, position.y + Padding, position.width, 1);
		EditorGUI.DrawRect(separatorRect, new Color(0.4f, 0.4f, 0.4f, 1f)); // Dark gray line
	}
}
