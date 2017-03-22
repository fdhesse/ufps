using UnityEngine;
using UnityEditor;

namespace FastShadowReceiver.Editor {
	[CustomEditor(typeof(ProjectionReceiverRenderer))]
	public class ProjectionReceiverRendererEditor : UnityEditor.Editor {
		static GUIContent s_projectorContent = new GUIContent("Projector", "If nothing is specified, a projector in shadow receiver component (GetComponent<ReceiverBase>().projector) will be used.");
		static GUIContent s_buttonContent = new GUIContent("Reset Shader Keywords", "ProjectionReceiverRenderer component will set \"FSR_RECEIVER\" keyword to render.material. So, usually, it won't affect shared material. If this keyword is set to the shared material, the material cannot work with normal Projector. In case that the shared material was affected, press this button.");
		public override void OnInspectorGUI ()
		{
			ProjectionReceiverRenderer receiver = target as ProjectionReceiverRenderer;
			DrawDefaultInspector();
			Object projector = receiver.customProjector;
			if (projector == null) {
				projector = receiver.unityProjector;
			}
			Component newProjector = EditorGUILayout.ObjectField(s_projectorContent, projector, typeof(Component), true) as Component;
			if (newProjector != projector) {
				if (newProjector == null) {
					Undo.RecordObject(receiver, "Inspector");
					receiver.unityProjector = null;
					receiver.customProjector = null;
				}
				else {
					Projector unityProjector = newProjector.GetComponent<Projector>();
					if (unityProjector != null) {
						Undo.RecordObject(receiver, "Inspector");
						receiver.unityProjector = unityProjector;
					}
					else {
						ProjectorBase projectorBase = newProjector.GetComponent<ProjectorBase>();
						if (projectorBase != null) {
							Undo.RegisterCompleteObjectUndo(receiver, "Inspector");
							receiver.customProjector = projectorBase;
						}
					}
				}
			}
			if (GUILayout.Button(s_buttonContent)) {
				Renderer renderer = receiver.GetComponent<Renderer>();
				if (renderer != null && renderer.sharedMaterial != null) {
					renderer.sharedMaterial.shaderKeywords = null;
				}
			}
		}
	}
}
