using OpenTK;

namespace VirusFactory.OpenTK.FSM.Interface {

    public interface IRenderable : ILoadable {

        void RenderFrame(FrameEventArgs e);
    }
}