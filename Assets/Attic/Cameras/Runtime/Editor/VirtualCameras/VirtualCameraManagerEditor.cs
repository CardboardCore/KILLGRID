using UnityEditor;
using UnityEngine;

namespace Attic.Cameras.VirtualCameras.Editor
{
	[CustomEditor(typeof(VirtualCameraManager))]
	public class VirtualCameraManagerEditor : UnityEditor.Editor
	{
		private VirtualCameraManager virtualCameraManager;

		private SerializedProperty virtualCamerasProperty;

		private GUIStyle headerStyle;
		private GUIStyle settingsStyle;

		private void OnEnable()
		{
			virtualCameraManager = (VirtualCameraManager)target;

			virtualCamerasProperty = serializedObject.FindProperty("virtualCameras");

			headerStyle = new GUIStyle();
			headerStyle.normal.textColor = Color.white;
			headerStyle.fontSize = 16;
			headerStyle.alignment = TextAnchor.MiddleCenter;

			settingsStyle = new GUIStyle();
			settingsStyle.normal.textColor = Color.white;
			settingsStyle.fontSize = 14;
			settingsStyle.alignment = TextAnchor.MiddleCenter;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			GUILayout.Label("Virtual Camera Manager", headerStyle);

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Save All"))
			{
				virtualCameraManager.SaveAllCameras();
			}

			EditorGUILayout.EndHorizontal();

			if(GUILayout.Button("Refresh (Find all camera's in open scenes)"))
			{
				virtualCameraManager.Refresh();
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			GUILayout.Label("Registered Virtual Cameras", settingsStyle);

			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical("box");

			for(int i = 0; i < virtualCamerasProperty.arraySize; i++)
			{
				SerializedProperty virtualCameraProperty = virtualCamerasProperty.GetArrayElementAtIndex(i);

				if(virtualCameraProperty.objectReferenceValue == null)
				{
					virtualCamerasProperty.DeleteArrayElementAtIndex(i);

					serializedObject.ApplyModifiedProperties();
					serializedObject.Update();

					break;
				}

				SerializedObject virtualCameraObject = new SerializedObject(virtualCameraProperty.objectReferenceValue);

				EditorGUILayout.LabelField(virtualCameraObject.FindProperty("data").FindPropertyRelative("id").stringValue);
			}

			EditorGUILayout.EndVertical();
		}
	}
}
