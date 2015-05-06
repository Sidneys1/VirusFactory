using OpenTK;
using VirusFactory.OpenTK.GameHelpers;

namespace VirusFactory.OpenTK.FSM.Behaviours {
    public static class BehaviorHelpers {
        public static Vector2 EaseMouse(Vector2 t) {
            return new Vector2(EaseMouse(t.X), EaseMouse(t.Y));
        }

        public static float EaseMouse(float t) {
            if (t < 0)
                return -Easing.EaseOut(-t, EasingType.Quadratic);
            return Easing.EaseOut(t, EasingType.Quadratic);
        }
    }
}
