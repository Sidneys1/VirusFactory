using OpenTK;
using VirusFactory.OpenTK.FSM.Interface;

namespace VirusFactory.OpenTK.FSM {
    public abstract class GameElementBase : ILoadable {
        protected GameWindow Owner;

        protected GameElementBase(GameWindow owner) {
            Owner = owner;
        }

        public virtual void Load() { }
        public virtual void UnLoad() { }
    }
}