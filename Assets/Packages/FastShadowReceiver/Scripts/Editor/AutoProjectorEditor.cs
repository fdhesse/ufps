//
// AutoProjectorEditor.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using UnityEditor;

namespace FastShadowReceiver.Editor {
	[CustomEditor(typeof(AutoProjector))]
	public class AutoProjectorEditor : EditorBase {
		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector();
			AutoProjector projector = target as AutoProjector;
			Component predictor = (Component)EditorGUILayout.ObjectField("Predictor", projector.predictor as Component, typeof(Component), true);
			if (predictor != projector.predictor) {
				if (predictor == null) {
					Undo.RecordObject(projector, "Inspector");
					projector.predictor = null;
					EditorUtility.SetDirty(projector);
				}
				else {
					Component[] components = predictor.GetComponents<Component>();
					for (int i = 0; i < components.Length; ++i) {
						if (components[i] is ITransformPredictor) {
							Undo.RecordObject(projector, "Inspector");
							projector.predictor = components[i] as ITransformPredictor;
							EditorUtility.SetDirty(projector);
							break;
						}
					}
				}
			}
		}
	}
}