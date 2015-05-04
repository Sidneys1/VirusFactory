using OpenTK;
using OpenTK.Input;

namespace VirusFactory.OpenTK.FSM.Interface {
    public interface IInputtable {
        void KeyDown(KeyboardKeyEventArgs e);
        void KeyPress(KeyPressEventArgs e);
        void KeyUp(KeyPressEventArgs e);

        void MouseDown(MouseButtonEventArgs e);
        void MouseUp(MouseButtonEventArgs e);
        void MouseEnter();
        void MouseLeave();
        void MouseMove(MouseMoveEventArgs e);
        void MouseWheel(MouseWheelEventArgs e);

    }
}
