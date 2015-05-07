using System.Collections.Generic;
using System.Linq;
using GFSM;
using MoreLinq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using VirusFactory.OpenTK.FSM.Elements.Base;
using VirusFactory.OpenTK.FSM.Interface;
using VirusFactory.OpenTK.GameHelpers;

namespace VirusFactory.OpenTK.FSM.States.Base {
	public abstract class GameStateBase : State<GameStateBase>, IRenderable, IUpdateable, IResizable {
		#region Fields

		protected GameWindow Owner;
		protected readonly TickScheduler Scheduler = new TickScheduler();

		#endregion Fields

		#region Properties

		public List<GameElementBase> GameElements { get; } = new List<GameElementBase>();

		#endregion Properties

		#region Constructors

		protected GameStateBase(GameWindow owner, GameFiniteStateMachine parent) : base(parent) {
			Owner = owner;
		}

		#endregion Constructors

		#region Methods

		public virtual void Load() {
			GameElements.ForEach(o => o.Load());
		}

		public virtual void RenderFrame(FrameEventArgs e) {
			GameElements.OfType<IRenderable>().ForEach(o => o.RenderFrame(e));
		}

		public virtual void UnLoad() {
			GameElements.ForEach(o => o.UnLoad());
		}

		#endregion Methods

		public virtual void UpdateFrame(FrameEventArgs e) {
			Scheduler.Tick(Owner, e);
			GameElements.OfType<IUpdateable>().ForEach(o => o.UpdateFrame(e));
		}

		public virtual void Resize() {
			GL.Viewport(0, 0, Owner.Width, Owner.Height);
			GameElements.OfType<IResizable>().ForEach(o => o.Resize());
		}
	}
}