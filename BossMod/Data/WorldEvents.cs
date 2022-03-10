using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public enum ActionEffectType : byte
    {
        Nothing = 0,
        Miss = 1,
        FullResist = 2,
        Damage = 3,
        Heal = 4,
        BlockedDamage = 5,
        ParriedDamage = 6,
        Invulnerable = 7,
        NoEffectText = 8,
        Unknown_9 = 9,
        MpLoss = 10, // 0x0A
        MpGain = 11, // 0x0B
        TpLoss = 12, // 0x0C
        TpGain = 13, // 0x0D
        ApplyStatusEffectTarget = 14, // 0x0E - dissector calls this "GpGain"
        ApplyStatusEffectSource = 15, // 0x0F
        RecoveredFromStatusEffect = 16, // 0x10
        LoseStatusEffectTarget = 17, // 0x11
        LoseStatusEffectSource = 18, // 0x12
        Unknown_13 = 19, // 0x13
        StatusNoEffect = 20, // 0x14
        ThreatPosition = 24, // 0x18
        EnmityAmountUp = 25, // 0x19
        EnmityAmountDown = 26, // 0x1A
        StartActionCombo = 27, // 0x1B
        Retaliation = 29, // 0x1D - 'vengeance' has value = 7, 'arms length' has value = 0
        Knockback1 = 32, // 0x20
        Knockback = 33, // 0x21
        Unknown_22 = 34, // 0x22
        Unknown_27 = 39, // 0x27
        Mount = 40, // 0x28
        unknown_30 = 48, // 0x30
        unknown_31 = 49, // 0x31
        Unknown_32 = 50, // 0x32
        Unknown_33 = 51, // 0x33
        FullResistStatus = 52, // 0x34
        Unknown_37 = 55, // 0x37 - 'arms length' has value = 9 on source, is this 'attack speed slow'?
        Unknown_38 = 56, // 0x38
        Unknown_39 = 57, // 0x39
        VFX = 59, // 0x3B
        Gauge = 60, // 0x3C
        Resource = 61, // 0x3D - value 0x34 = gain war gauge (amount == hitSeverity)
        Unknown_40 = 64, // 0x40
        Unknown_42 = 66, // 0x42
        Unknown_46 = 70, // 0x46
        Unknown_47 = 71, // 0x47
        Unknown_48 = 72, // 0x48
        Unknown_49 = 73, // 0x49
        Partial_Invulnerable = 74, // 0x4A
        Interrupt = 75, // 0x4B
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ActionEffect
    {
        public ActionEffectType effectType;
        public byte param0;
        public byte param1;
        public byte param2;
        public byte param3;
        public byte param4;
        public ushort value;
    }

    public enum DamageType
    {
        Unknown,
        Slashing,
        Piercing,
        Blunt,
        Shot,
        Magic,
        Breath,
        Physical,
        LimitBreak,
    }

    public enum DamageElementType
    {
        Unknown,
        Fire,
        Ice,
        Air,
        Earth,
        Lightning,
        Water,
        Unaspected,
    }

    public unsafe struct ActionEffects : IEnumerable<ActionEffect>
    {
        private fixed ulong _effects[8];

        public ulong this[int index]
        {
            get => _effects[index];
            set => _effects[index] = value;
        }

        public IEnumerator<ActionEffect> GetEnumerator()
        {
            for (int i = 0; i < 8; ++i)
            {
                var eff = Build(i);
                if (eff.effectType != ActionEffectType.Nothing)
                    yield return eff;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private unsafe ActionEffect Build(int index)
        {
            fixed (ulong* p = _effects)
                return *(ActionEffect*)(p + index);
        }
    }

    public class CastEvent
    {
        public struct Target
        {
            public uint ID;
            public ActionEffects Effects;
        }

        public uint CasterID;
        public uint MainTargetID; // note that actual affected targets could be completely different
        public ActionID Action;
        public float AnimationLockTime;
        public uint MaxTargets;
        public List<Target> Targets = new();
        public uint SourceSequence;

        public bool IsSpell() => Action.Type == ActionType.Spell;
        public bool IsSpell<AID>(AID aid) where AID : Enum => Action == ActionID.MakeSpell(aid);
    }

    // dispatcher for instant world events; part of the world state structure
    public class WorldEvents
    {
        public event EventHandler<(uint actorID, uint iconID)>? Icon; // TODO: this should really be an actor field, but I have no idea what triggers icon clear...
        public void DispatchIcon((uint actorID, uint iconID) args)
        {
            Icon?.Invoke(this, args);
        }

        public event EventHandler<CastEvent>? Cast;
        public void DispatchCast(CastEvent info)
        {
            Cast?.Invoke(this, info);
        }

        public event EventHandler<(uint featureID, byte index, uint state)>? EnvControl;
        public void DispatchEnvControl((uint featureID, byte index, uint state) args)
        {
            EnvControl?.Invoke(this, args);
        }
    }
}
