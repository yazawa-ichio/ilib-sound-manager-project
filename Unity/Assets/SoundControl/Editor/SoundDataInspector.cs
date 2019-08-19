using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ILib.Audio
{
	[CustomEditor(typeof(SoundData), true)]
	public class SoundDataInspector : Editor
	{
		SoundData m_Taret;
		private void OnEnable()
		{
			m_Taret = target as SoundData;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();
			using (new EditorGUI.DisabledScope(true))
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
			}
			EditorGUILayout.PropertyField(serializedObject.FindProperty("Clip"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("Volume"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("Pitch"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ControlId"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ControlType"));

			EditorGUILayout.LabelField($"【{m_Taret.ControlType}】", GetTypeName(m_Taret.ControlType));

			switch (m_Taret.ControlType)
			{
				case StartControl.Type.LimitCount:
					EditorGUILayout.PropertyField(serializedObject.FindProperty("ControlParam1"), new GUIContent("最大個数(整数になります)"));
					break;
				case StartControl.Type.LimitInterval:
					EditorGUILayout.PropertyField(serializedObject.FindProperty("ControlParam1"), new GUIContent("再生間隔"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("ControlParam2"), new GUIContent("再生待ち状態で待機する時間"));
					break;
				case StartControl.Type.LateArrivalPriority:
					EditorGUILayout.PropertyField(serializedObject.FindProperty("ControlParam1"), new GUIContent("停止する音源のフェード時間"));
					break;
			}

			serializedObject.ApplyModifiedProperties();
		}

		string GetTypeName(StartControl.Type type)
		{
			switch (type)
			{
				case StartControl.Type.None:
					return "なし";
				case StartControl.Type.LimitCount:
					return "再生個数を制限";
				case StartControl.Type.LimitInterval:
					return "再生間隔を制限";
				case StartControl.Type.FirstComeFirstServed:
					return "先勝ち";
				case StartControl.Type.LateArrivalPriority:
					return "後勝ち";
			}
			return "不明";
		}
		
	}

}
