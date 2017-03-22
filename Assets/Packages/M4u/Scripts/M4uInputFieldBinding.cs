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
    /// M4uInputFieldBinding. Bind InputField
    /// </summary>
	[AddComponentMenu("M4u/InputFieldBinding")]
	public class M4uInputFieldBinding : M4uBindingSingle
	{
		public string Format = "";

		private InputField ui = null;

        protected override void OnDestroy()
        {
            //ui.onEndEdit.RemoveListener(OnEndEdit);

            base.OnDestroy();
        }

		public override void Start ()
		{
			base.Start ();

			ui = GetComponent<InputField> ();

            ui.onEndEdit.AddListener(OnEndEdit);

			OnChange ();
		}

        private void OnEndEdit(string value)
        {
            SetMember(0, value);
        }

		public override void OnChange ()
		{
			base.OnChange ();

			ui.text = string.Format(Format, Values[0]);
		}

        public override string ToString()
        {
            return "InputField.text=" + string.Format(Format, GetBindStr(Path));
        }
	}
}