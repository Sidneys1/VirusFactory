using MoreLinq;
using OpenTK;
using System.Collections.Generic;
using System.Linq;
using GFSM;
using VirusFactory.OpenTK.FSM.Interface;

namespace VirusFactory.OpenTK.FSM {

    public abstract class GameStateBase : State<GameStateBase>, IRenderable, IUpdateable, IResizable {

        #region Fields

        protected GameWindow Owner;

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
            GameElements.OfType<IUpdateable>().ForEach(o=>o.UpdateFrame(e));
        }

        public virtual void Resize() {
            GameElements.OfType<IResizable>().ForEach(o=>o.Resize());
        }
    }
}