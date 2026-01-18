using System;
using Attic.Cameras.Modules;
using Attic.Utils;
using UnityEditor;
using UnityEngine;

namespace Attic.Cameras.VirtualCameras.Editor
{
	[CustomEditor(typeof(VirtualCamera))]
	public class VirtualCameraEditor : UnityEditor.Editor
	{
		private VirtualCamera virtualCamera;

		private SerializedProperty isNameLockedProperty;
		private SerializedProperty modulesProperty;
		private SerializedProperty idProperty;

		private SerializedProperty dataProperty;

		private GenericMenu genericMenu;

		private GUIStyle headerStyle;
		private GUIStyle settingsStyle;

		private void OnEnable()
		{
			isNameLockedProperty = serializedObject.FindProperty("isNameLocked");
			dataProperty = serializedObject.FindProperty("data");

			modulesProperty = dataProperty.FindPropertyRelative("modules");
			idProperty = dataProperty.FindPropertyRelative("id");

			headerStyle = new GUIStyle();
			headerStyle.normal.textColor = Color.white;
			headerStyle.fontSize = 16;
			headerStyle.alignment = TextAnchor.MiddleCenter;

			settingsStyle = new GUIStyle();
			settingsStyle.normal.textColor = Color.white;
			settingsStyle.fontSize = 14;
			settingsStyle.alignment = TextAnchor.MiddleCenter;

			virtualCamera = (VirtualCamera) serializedObject.targetObject;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			GUILayout.Label("Virtual Camera", headerStyle);

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal("box");

			if(isNameLockedProperty.boolValue)
			{
				EditorGUILayout.LabelField(idProperty.stringValue);
			}
			else
			{
				EditorGUILayout.PropertyField(idProperty);
			}

			if(GUILayout.Button(isNameLockedProperty.boolValue ? "UnLock" : "Lock", GUILayout.Width(70)))
			{
				isNameLockedProperty.boolValue = !isNameLockedProperty.boolValue;

				if(isNameLockedProperty.boolValue)
				{
					serializedObject.targetObject.name = $"VirtualCamera-{idProperty.stringValue}";
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical("box");

			SerializedProperty isOrthographicProperty = dataProperty.FindPropertyRelative("isOrthographic");
			EditorGUILayout.PropertyField(isOrthographicProperty);
			if(!isOrthographicProperty.boolValue)
			{
				EditorGUILayout.PropertyField(dataProperty.FindPropertyRelative("fov"));
			}

			EditorGUILayout.PropertyField(dataProperty.FindPropertyRelative("nearClipPlane"));
			EditorGUILayout.PropertyField(dataProperty.FindPropertyRelative("farClipPlane"));

			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical("box");

			EditorGUILayout.LabelField("Modules", settingsStyle);

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			if(GUILayout.Button("Add Module", GUILayout.Height(30)))
			{
				genericMenu = new GenericMenu();

				// Get all derived types from "CameraModule"
				Type[] types = ReflectionTools.GetTypesFilterAbstract<CameraModule>();

				for(int i = 0; i < types.Length; i++)
				{
					genericMenu.AddItem(new GUIContent(types[i].Name), false, OnSelect, types[i]);
				}

				genericMenu.ShowAsContext();
			}

			for(int i = 0; i < modulesProperty.arraySize; i++)
			{
				SerializedProperty moduleProperty = modulesProperty.GetArrayElementAtIndex(i);

				if(moduleProperty == null || moduleProperty.objectReferenceValue == null)
				{
					virtualCamera.RemoveModuleAt(i);
					break;
				}

				SerializedObject s = new SerializedObject(moduleProperty.objectReferenceValue);
				SerializedProperty queuedForRemoval = s.FindProperty("queuedForRemoval");

				if(queuedForRemoval.boolValue)
				{
					virtualCamera.RemoveModuleAt(i);
					break;
				}

				EditorGUILayout.PropertyField(moduleProperty);
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Save"))
			{
				virtualCamera.Save();
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();
		}

		private void OnSelect(object @object)
		{
			Type type = (Type) @object;

			virtualCamera.AddModule((CameraModule) CreateInstance(type));

			serializedObject.ApplyModifiedProperties();
		}
	}
}
