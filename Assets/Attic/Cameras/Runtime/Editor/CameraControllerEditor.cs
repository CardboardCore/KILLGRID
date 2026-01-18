using Attic.Cameras.VirtualCameras;
using UnityEditor;
using UnityEngine;

namespace Attic.Cameras.Editor
{
	[CustomEditor(typeof(CameraController))]
	public class CameraControllerEditor : UnityEditor.Editor
	{
		private SerializedProperty virtualCameraManagerProperty;
		private SerializedProperty initialCameraIdProperty;

		private GUIStyle headerStyle;
		private GUIStyle errorStyle;
		private GUIStyle settingsStyle;

		private void OnEnable()
		{
			virtualCameraManagerProperty = serializedObject.FindProperty("virtualCameraManager");
			initialCameraIdProperty = serializedObject.FindProperty("initialCameraId");

			headerStyle = new GUIStyle();
			headerStyle.normal.textColor = Color.white;
			headerStyle.fontSize = 16;
			headerStyle.alignment = TextAnchor.MiddleCenter;

			errorStyle = new GUIStyle();
			errorStyle.fontSize = 11;
			errorStyle.normal.textColor = Color.red;
			errorStyle.alignment = TextAnchor.MiddleCenter;

			settingsStyle = new GUIStyle();
			settingsStyle.normal.textColor = Color.white;
			settingsStyle.fontSize = 14;
			settingsStyle.alignment = TextAnchor.MiddleCenter;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Camera Controller", headerStyle);

			if(virtualCameraManagerProperty.objectReferenceValue == null)
			{
				VirtualCameraManager foundVirtualCameraManager = FindObjectOfType<VirtualCameraManager>();

				if(foundVirtualCameraManager == null)
				{
					EditorGUILayout.LabelField("No VirtualCameraManager found in scene(s), please add it!", errorStyle);
				}
				else
				{
					virtualCameraManagerProperty.objectReferenceValue = foundVirtualCameraManager;
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			VirtualCameraManager virtualCameraManager = (VirtualCameraManager)virtualCameraManagerProperty.objectReferenceValue;

			GenericMenu genericMenu = new GenericMenu();

			for(int i = 0; i < virtualCameraManager.VirtualCameras.Count; i++)
			{
				genericMenu.AddItem(new GUIContent(virtualCameraManager.VirtualCameras[i].Id), false, OnSelect, virtualCameraManager.VirtualCameras[i].Id);
			}

			string initialCameraString = string.IsNullOrEmpty(initialCameraIdProperty.stringValue) ? "No Initial Camera is Set!" : initialCameraIdProperty.stringValue;

			EditorGUILayout.BeginVertical("box");

			EditorGUILayout.LabelField("Settings", settingsStyle);

			EditorGUILayout.BeginHorizontal("box");

			EditorGUILayout.LabelField("Initial Camera:");

			if(GUILayout.Button(initialCameraString))
			{
				genericMenu.ShowAsContext();
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();
		}
		private void OnSelect(object idObject)
		{
			initialCameraIdProperty.stringValue = (string)idObject;
			serializedObject.ApplyModifiedProperties();
		}
	}
}
