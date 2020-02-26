using System.Collections.Generic;
using BunnyLand.DesktopGL.Enums;
using PubSub;

namespace BunnyLand.DesktopGL
{
    public class Variables
    {
        private readonly Dictionary<GlobalVariable, float> global = new Dictionary<GlobalVariable, float> {
            {GlobalVariable.JetpackAcceleration, 0.2f},
            {GlobalVariable.JetpackBoostAcceleration, 1f},
            {GlobalVariable.JetpackMaxSpeed, 8f},
            {GlobalVariable.JetpackBoostMaxSpeed, 12f},
            {GlobalVariable.GravityMultiplier, 1f},
            {GlobalVariable.BounceFactor, 0.5f},
            {GlobalVariable.BrakePower, 0.1f},
            {GlobalVariable.InertiaRatio, 1f},
            {GlobalVariable.GlobalMaxSpeed, 100f},
            {GlobalVariable.GameSpeed, 1f},
            {GlobalVariable.DebugVectorMultiplier, 10f},
            {GlobalVariable.BulletSpeed, 10f}
        };

        public IReadOnlyDictionary<GlobalVariable, float> Global => global;

        public Variables()
        {
            Hub.Default.Subscribe<(GlobalVariable key, float value)>(SetGlobalVariable);
        }

        private void SetGlobalVariable((GlobalVariable key, float value) tuple)
        {
            global[tuple.key] = tuple.value;
        }
    }
}
