using OpenTK;
using OpenTK.Input;

namespace VirusFactory.OpenTK.FSM.Interface {

	public interface IKeyboardInput {
		#region Public Methods

		void KeyDown(KeyboardKeyEventArgs e);

		void KeyPress(KeyPressEventArgs e);

		void KeyUp(KeyboardKeyEventArgs e);

		#endregion Public Methods
	}
}