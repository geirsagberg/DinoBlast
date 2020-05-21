using System.Collections.Generic;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Services;

namespace BunnyLand.DesktopGL
{
    public interface IVariables
    {
        IReadOnlyDictionary<GlobalVariable, float> Global { get; }
    }

    public class Variables : IVariables
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
            {GlobalVariable.BulletSpeed, 20f},
            {GlobalVariable.FiringInterval, 0.2f},
            {GlobalVariable.BulletLifespan, 1f},
        };

        public IReadOnlyDictionary<GlobalVariable, float> Global => global;

        public Variables(MessageHub messageHub)
        {
            messageHub.Subscribe<SetGlobalVariableMessage>(SetGlobalVariable);
        }

        private void SetGlobalVariable(SetGlobalVariableMessage msg)
        {
            global[msg.Name] = msg.Value;
        }
    }
}
