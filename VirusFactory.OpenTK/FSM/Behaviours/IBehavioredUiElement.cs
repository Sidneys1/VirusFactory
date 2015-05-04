using VirusFactory.OpenTK.FSM.Elements;
using VirusFactory.OpenTK.GameHelpers.Behaviourals;

namespace VirusFactory.OpenTK.FSM.Behaviours {
    public interface IBehavioredUiElement : IBehaviored<GameTriggers, IBehavioredUiElement>, IUiElement {
    }
}
