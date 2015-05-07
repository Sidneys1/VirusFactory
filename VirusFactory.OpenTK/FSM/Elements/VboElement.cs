using OpenTK;
using OpenTK.Graphics.OpenGL;
using VirusFactory.OpenTK.FSM.Elements.Base;
using VirusFactory.OpenTK.FSM.Interface;
using VirusFactory.OpenTK.GameHelpers.VBOHelper;

namespace VirusFactory.OpenTK.FSM.Elements {
	public class VboElement<T> : GameElementBase, IRenderable where T : struct {
		#region Public Properties

		public VertexBuffer<T> Buffer { get; set; }
		
		public PrimitiveType PrimitiveType { get; }

		#endregion Public Properties

		#region Public Ctor / Dtor

		public VboElement(GameWindow owner, VertexBuffer<T> buffer, PrimitiveType ptype) : base(owner) {
			Buffer = buffer;
			PrimitiveType = ptype;
		}

		#endregion Public Ctor / Dtor

		#region Public Methods
		
		public void RenderFrame(FrameEventArgs e) {
			Buffer?.Render(PrimitiveType);
		}

		#endregion Public Methods
	}
}