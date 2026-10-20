using System;

namespace BookBoy.Rules
{
    /// <summary>The sprint FOV kick (docs/unity-practices/30): how many degrees wider the view is at a given horizontal speed. Engine-free.</summary>
    public static class SprintFovRules
    {
        /// <summary>0 at or below <paramref name="walkSpeed"/>, the full <paramref name="maxKickDegrees"/> at or above <paramref name="sprintSpeed"/>, eased in between (smoothstep),
        /// all scaled by <paramref name="strength"/> (0 to 1, the player's setting).</summary>
        public static float Kick(float speed, float walkSpeed, float sprintSpeed, float maxKickDegrees, float strength)
        {
            if (sprintSpeed <= walkSpeed + 1e-4f || strength <= 0f || maxKickDegrees <= 0f)
            {
                return 0f;
            }

            float t = Math.Max(0f, Math.Min(1f, (speed - walkSpeed) / (sprintSpeed - walkSpeed)));
            t = t * t * (3f - 2f * t);
            return maxKickDegrees * t * Math.Min(1f, strength);
        }
    }
}
