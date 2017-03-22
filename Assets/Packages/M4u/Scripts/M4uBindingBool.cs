//----------------------------------------------
// MVVM 4 uGUI
// © 2015 yedo-factory
//----------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace M4u
{
    /// <summary>
    /// M4uBindingBool. Bind bool type
    /// </summary>
	public class M4uBindingBool : M4uBindingSingle
	{
		public BoolCheckType CheckType;
		public double CheckValue;
        public string CheckString;
		public bool Invert;

		public bool IsCheck()
		{
			bool isCheck = false;
			object value = Values [0];
			switch (CheckType)
			{
				case BoolCheckType.Bool:
					isCheck = (bool)value;
					break;
                case BoolCheckType.Equal:
					isCheck = (double.Parse(value.ToString()) == CheckValue);
					break;
				case BoolCheckType.Greater:
					isCheck = (double.Parse(value.ToString()) > CheckValue);
					break;
				case BoolCheckType.Less:
					isCheck = (double.Parse(value.ToString()) < CheckValue);
					break;
				case BoolCheckType.Empty:
					isCheck = (value.ToString () != "");
					break;
                case BoolCheckType.String:
					isCheck = (value.ToString() == CheckString);
					break;
                case BoolCheckType.Enum:
					isCheck = (Enum.Parse(value.GetType(), CheckString).ToString() == value.ToString());
                    break;
			}
			return (!Invert) ? isCheck : !isCheck;
		}

        public override string ToString()
        {
            string invert = (Invert) ? "!" : "";
            switch (CheckType)
            {
                case BoolCheckType.Bool:
                    return invert + GetBindStr(Path);
                case BoolCheckType.Equal:
                    return invert + "(" + GetBindStr(Path) + "==" + CheckValue + ")";
                case BoolCheckType.Greater:
                    return invert + "(" + GetBindStr(Path) + ">" + CheckValue + ")";
                case BoolCheckType.Less:
                    return invert + "(" + GetBindStr(Path) + "<" + CheckValue + ")";
                case BoolCheckType.Empty:
                    return invert + "(" + GetBindStr(Path) + "==\"\")";
                case BoolCheckType.String:
                case BoolCheckType.Enum:
                    return invert + "(" + GetBindStr(Path) + "==" + CheckString + ")";
                default:
                    return "";
            }
        }
	}
}