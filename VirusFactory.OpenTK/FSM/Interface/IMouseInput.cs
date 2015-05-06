using OpenTK.Input;

namespace VirusFactory.OpenTK.FSM.Interface {
    public interface IMouseInput {
        void MouseDown(MouseButtonEventArgs e);
        void MouseUp(MouseButtonEventArgs e);
        void MouseEnter();
        void MouseLeave();
        void MouseMove(MouseMoveEventArgs e);
        void MouseWheel(MouseWheelEventArgs e);
    }
}