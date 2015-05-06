using System;
using GFSM;
using OpenTK;
using OpenTK.Input;
using VirusFactory.OpenTK.FSM.Interface;

namespace VirusFactory.OpenTK.FSM {

    public class GameFiniteStateMachine : FiniteStateMachine<GameStateBase>, IUpdateable, IRenderable, IKeyboardInput, IResizable {

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
            CurrentState?.RenderFrame(e);
        }

        public void UpdateFrame(FrameEventArgs e) {
            (CurrentState as IUpdateable)?.UpdateFrame(e);
        }

        public void KeyDown(KeyboardKeyEventArgs e) {
            (CurrentState as IKeyboardInput)?.KeyDown(e);
        }

        public void KeyPress(KeyPressEventArgs e) {
            (CurrentState as IKeyboardInput)?.KeyPress(e);
        }

        public void KeyUp(KeyboardKeyEventArgs e) {
            (CurrentState as IKeyboardInput)?.KeyUp(e);
        }

        public void MouseDown(MouseButtonEventArgs e) {
            (CurrentState as IMouseInput)?.MouseDown(e);
        }

        public void MouseUp(MouseButtonEventArgs e) {
            (CurrentState as IMouseInput)?.MouseUp(e);
        }

        public void MouseEnter() {
            (CurrentState as IMouseInput)?.MouseEnter();
        }

        public void MouseLeave() {
            (CurrentState as IMouseInput)?.MouseLeave();
        }

        public void MouseMove(MouseMoveEventArgs e) {
            (CurrentState as IMouseInput)?.MouseMove(e);
        }

        public void MouseWheel(MouseWheelEventArgs e) {
            (CurrentState as IMouseInput)?.MouseWheel(e);
        }
        
        public void Resize() {
            (CurrentState as IResizable)?.Resize();
        }
        
        #endregion Methods
    }
}