using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Assets.Crimson.Core.Common
{
	[Serializable]
	public struct UserInputBinding
	{
		public List<InputActionReference> CustomInputs;
		public InputActionReference LookInputRef;
		public InputActionReference MoveInputRef;
		public InputActionReference PointerInputRef;
	}
}