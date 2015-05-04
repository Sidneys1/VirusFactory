using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using OpenTK;
using VirusFactory.OpenTK.FSM.Interface;
using VirusFactory.OpenTK.GameHelpers.FSM;

namespace VirusFactory.OpenTK.FSM {
    public abstract class GameStateBase : State, IRenderable {
        #region Fields

        protected GameWindow Owner;

        #endregion Fields

        #region Properties

        public List<GameElementBase> GameElements { get; } = new List<GameElementBase>();
        public GameFiniteStateMachine StateMachine { get; }

        #endregion Properties

        #region Constructors

        protected GameStateBase(GameWindow owner, GameFiniteStateMachine parent) : base(StateMode.Active) {
            Owner = owner;
            StateMachine = parent;
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
    }
}
