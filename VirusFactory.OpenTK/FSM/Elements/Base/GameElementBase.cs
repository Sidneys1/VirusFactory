using OpenTK;
using VirusFactory.OpenTK.FSM.Interface;

namespace VirusFactory.OpenTK.FSM.Elements.Base {
	public abstract class GameElementBase : ILoadable {
		#region Protected Fields

		protected GameWindow Owner;

		#endregion Protected Fields

		#region Protected Ctor / Dtor

		protected GameElementBase(GameWindow owner) {
			Owner = owner;
		}

		#endregion Protected Ctor / Dtor

		#region Public Methods

		public virtual void Load() {
		}

		public virtual void UnLoad() {
		}

		#endregion Public Methods
	}
}