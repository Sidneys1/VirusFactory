using GFSM;
using OpenTK;
using OpenTK.Input;
using VirusFactory.OpenTK.FSM.Interface;

namespace VirusFactory.OpenTK.FSM {

    public class GameFiniteStateMachine : FiniteStateMachine<GameStateBase>, IUpdateable, IRenderable, IInputtable, IResizable {

        #region Methods

        public void Load() {
            // Load all states
            States.ForEach(o => o.Load());
        }

        public void UnLoad() {
            // Load all states
            States.ForEach(o => o.UnLoad());
        }

        public void RenderFrame(FrameEventArgs e) {
            CurrState?.RenderFrame(e);
        }

        public void UpdateFrame(FrameEventArgs e) {
            (CurrState as IUpdateable)?.UpdateFrame(e);
        }

        public void KeyDown(KeyboardKeyEventArgs e) {
            (CurrState as IInputtable)?.KeyDown(e);
        }

        public void KeyPress(KeyPressEventArgs e) {
            (CurrState as IInputtable)?.KeyPress(e);
        }

        public void KeyUp(KeyPressEventArgs e) {
            (CurrState as IInputtable)?.KeyUp(e);
        }

        public void MouseDown(MouseButtonEventArgs e) {
            (CurrState as IInputtable)?.MouseDown(e);
        }

        public void MouseUp(MouseButtonEventArgs e) {
            (CurrState as IInputtable)?.MouseUp(e);
        }

        public void MouseEnter() {
            (CurrState as IInputtable)?.MouseEnter();
        }

        public void MouseLeave() {
            (CurrState as IInputtable)?.MouseLeave();
        }

        public void MouseMove(MouseMoveEventArgs e) {
            (CurrState as IInputtable)?.MouseMove(e);
        }

        public void MouseWheel(MouseWheelEventArgs e) {
            (CurrState as IInputtable)?.MouseWheel(e);
        }
        
        public void Resize() {
            (CurrState as IResizable)?.Resize();
        }
        
        #endregion Methods
    }
}