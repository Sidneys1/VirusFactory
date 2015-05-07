using OpenTK;

namespace VirusFactory.OpenTK.FSM.Interface {

	public interface IUpdateable {
		#region Public Methods

		void UpdateFrame(FrameEventArgs e);

		#endregion Public Methods
	}
}