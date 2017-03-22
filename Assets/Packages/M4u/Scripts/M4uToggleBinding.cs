//----------------------------------------------
// MVVM 4 uGUI
// © 2015 yedo-factory
//----------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace M4u
{
    /// <summary>
    /// M4uToggleBinding. Bind Toggle
    /// </summary>
	[AddComponentMenu("M4u/ToggleBinding")]
	public class M4uToggleBinding : M4uBindingBool
	{
		private Toggle ui = null;

		public override void Start ()
		{
			base.Start ();

			ui = GetComponent<Toggle> ();

            ui.onValueChanged.AddListener(OnValueChanged);
			OnChange ();
		}

        private void OnValueChanged(bool value)
        {
            SetMember(0, value);
        }

		public override void OnChange ()
		{
			base.OnChange ();

			ui.isOn = IsCheck();
		}

        public override string ToString()
        {
            return "Toggle.isOn=" + base.ToString();
        }
	}
}