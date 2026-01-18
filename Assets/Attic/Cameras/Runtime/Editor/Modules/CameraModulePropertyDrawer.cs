using System;
using System.Reflection;
using Attic.Utils;
using UnityEditor;
using UnityEngine;

namespace Attic.Cameras.Modules.Editor
{
	[CustomPropertyDrawer(typeof(CameraModule), true)]
	public class CameraModulePropertyDrawer : PropertyDrawer
	{
		private GUIStyle headerStyle;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			EditorGUILayout.BeginVertical("box");

			if(headerStyle == null)
			{
				headerStyle = new GUIStyle();
				headerStyle.normal.textColor = Color.white;
				headerStyle.fontSize = 13;
				headerStyle.alignment = TextAnchor.MiddleLeft;
			}

			EditorGUILayout.BeginHorizontal("box");

			Type objectType = property.objectReferenceValue.GetType();

			SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
			serializedObject.Update();

			EditorGUILayout.LabelField(objectType.Name, headerStyle);

			if(GUILayout.Button("-", GUILayout.Width(30)))
			{
				if(EditorUtility.DisplayDialog($"Remove {objectType.Name} from Virtual Camera", "Are you sure you wish to remove this module?", "Yes", "No!"))
				{
					serializedObject.FindProperty("queuedForRemoval").boolValue = true;
					serializedObject.ApplyModifiedPropertiesWithoutUndo();
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			// Get all fields from base class
			FieldInfo[] baseFieldInfos = ReflectionTools.GetFieldsWithAttribute<SerializeField>(typeof(CameraModule));

			// Get all fields from serialized object (this includes fields from base class)
			FieldInfo[] fieldInfos = ReflectionTools.GetFieldsWithAttribute<SerializeField>(objectType);

			for(int i = 0; i < fieldInfos.Length; i++)
			{
				bool isBaseField = false;

				for(int k = 0; k < baseFieldInfos.Length; k++)
				{
					isBaseField = baseFieldInfos[k].Name == fieldInfos[i].Name;

					if(isBaseField)
					{
						break;
					}
				}

				if(isBaseField
					|| fieldInfos[i].Attributes != FieldAttributes.Private && fieldInfos[i].ReflectedType != property.objectReferenceValue.GetType())
				{
					continue;
				}

				EditorGUILayout.PropertyField(serializedObject.FindProperty(fieldInfos[i].Name));
			}

			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedPropertiesWithoutUndo();

			EditorGUI.EndProperty();
		}
	}
}
