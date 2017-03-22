//
// EditorBase.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;

namespace FastShadowReceiver.Editor {
	public class EditorBase : UnityEditor.Editor {
		protected EditorSettings editorSettings
		{
			get {
				return EditorSettings.GetSettings(UnityEditor.MonoScript.FromScriptableObject(this));
			}
		}
	}
}
