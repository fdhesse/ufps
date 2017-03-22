//
// ReceiverBaseEditor.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using UnityEditor;

namespace FastShadowReceiver.Editor {
	[CustomEditor(typeof(ReceiverBase))]
	public class ReceiverBaseEditor : EditorBase {
		private GUIStyle m_errorStyle = null;
		protected GUIStyle errorStyle {
			get {
				if (m_errorStyle == null) {
					m_errorStyle = new GUIStyle();
					m_errorStyle.richText = true;
					m_errorStyle.wordWrap = true;
				}
				return m_errorStyle;
			}
		}
		public override void OnInspectorGUI ()
		{
			ReceiverBase receiver = target as ReceiverBase;
			DrawDefaultInspector();
			Object projector = receiver.customProjector;
			if (projector == null) {
				projector = receiver.unityProjector;
			}
			Component newProjector = EditorGUILayout.ObjectField("Projector", projector, typeof(Component), true) as Component;
			if (newProjector != projector) {
				if (newProjector == null) {
					Undo.RecordObject(receiver, "Inspector");
					receiver.unityProjector = null;
					receiver.customProjector = null;
				}
				else {
					Projector unityProjector = newProjector.GetComponent<Projector>();
					Renderer renderer = receiver.GetComponent<Renderer>();
					Material material = renderer.sharedMaterial;
					if (unityProjector != null) {
						Undo.RecordObject(receiver, "Inspector");
						receiver.unityProjector = unityProjector;
						if (material == null || material == editorSettings.m_defaultShadowmapReceiverMaterial) {
							Undo.RecordObject(renderer, "Inspector");
							renderer.sharedMaterial = editorSettings.m_defaultReceiverMaterial;
#if (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6) // support Unity 4.3 or later
							renderer.castShadows = false;
#else
							renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
							renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
#endif
							renderer.useLightProbes = false;
							renderer.receiveShadows = false;
						}
					}
					else {
						ProjectorBase projectorBase = newProjector.GetComponent<ProjectorBase>();
						if (projectorBase != null) {
							Undo.RegisterCompleteObjectUndo(receiver, "Inspector");
							receiver.customProjector = projectorBase;
							if (material == null || material == editorSettings.m_defaultReceiverMaterial) {
								Undo.RecordObject(renderer, "Inspector");
								renderer.sharedMaterial = editorSettings.m_defaultShadowmapReceiverMaterial;
#if (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6) // support Unity 4.3 or later
								renderer.castShadows = false;
#else
								renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
								renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
#endif
								renderer.useLightProbes = false;
								renderer.receiveShadows = true;
							}
						}
					}
				}
			}
			// check errors
			string errorMessage = null;
			if (projector == null) {
				// check if the object is prefab or not.
				PrefabType type = PrefabUtility.GetPrefabType(receiver.gameObject);
				if (type != PrefabType.Prefab && type != PrefabType.ModelPrefab) {
					// check if there is ProjectorManager and the receiver is add to it.
					ProjectorManager proman = Object.FindObjectOfType<ProjectorManager>();
					if (proman != null) {
						if (!proman.receivers.Contains(receiver)) {
							errorMessage = "<color=red>Projector has not been set! There is a ProjectorManager in the scene. Do you forget to add this shadow receiver into the ProjectorManager?</color>";
						}
					}
					else {
						errorMessage = "<color=red>Projector has not been set! Shadow receiver will not work until a Projector is assigned.</color>";
					}
				}
			}
			else if (projector is Projector) {
				if ((((Projector)projector).ignoreLayers & (1 << receiver.gameObject.layer)) != 0) {
					errorMessage = "<color=red>This shadow receiver is being ignored by the Projector. Cannot receive the shadow from the Projector. Please check the layer of this object and Ignore Layers of the Projector.</color>";
				}
			}
			if (!string.IsNullOrEmpty(errorMessage)) {
				GUILayout.TextArea(errorMessage, errorStyle);
			}
		}
	}
}
