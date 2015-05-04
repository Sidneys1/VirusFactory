using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using VirusFactory.OpenTK.FSM.Interface;
using VirusFactory.OpenTK.GameHelpers.VBOHelper;

namespace VirusFactory.OpenTK.FSM.Elements {
    public class VboElement<T> : GameElementBase, IUpdateable, IRenderable where T : struct {
        public VertexBuffer<T> Buffer { get; }
        public Action UpdateBuffer { get; set; }
        public PrimitiveType PrimitiveType { get; }

        public VboElement(GameWindow owner, VertexBuffer<T> buffer, PrimitiveType ptype) : base(owner) {
            Buffer = buffer;
            PrimitiveType = ptype;
        }

        public void UpdateFrame(FrameEventArgs e) {
            // Update VBO
        }

        public void RenderFrame(FrameEventArgs e) {
            Buffer.Render(PrimitiveType);
        }
    }
}
