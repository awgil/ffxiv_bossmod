using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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
        FailMissingStatus = 9,
        MpLoss = 10, // 0x0A
        MpGain = 11, // 0x0B
        TpLoss = 12, // 0x0C
        TpGain = 13, // 0x0D
        ApplyStatusEffectTarget = 14, // 0x0E - dissector calls this "GpGain"
        ApplyStatusEffectSource = 15, // 0x0F
        RecoveredFromStatusEffect = 16, // 0x10
        LoseStatusEffectTarget = 17, // 0x11
        LoseStatusEffectSource = 18, // 0x12
        Unknown_13 = 19, // 0x13 - sometimes part of pvp Purify & Empyrean Rain spells, related to afflictions removal?..
        StatusNoEffect = 20, // 0x14
        ThreatPosition = 24, // 0x18
        EnmityAmountUp = 25, // 0x19
        EnmityAmountDown = 26, // 0x1A
        StartActionCombo = 27, // 0x1B
        Retaliation = 29, // 0x1D - 'vengeance' has value = 7, 'arms length' has value = 0
        Knockback = 32, // 0x20
        Attract1 = 33, // 0x21
        Attract2 = 34, // 0x22
        AttractCustom1 = 35, // 0x23
        AttractCustom2 = 36, // 0x24
        AttractCustom3 = 37, // 0x25
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
        Unknown_48 = 72, // 0x48 - looks like 'keep in place'?
        SetHP = 73, // 0x49 - e.g. zodiark's kokytos
        Partial_Invulnerable = 74, // 0x4A
        Interrupt = 75, // 0x4B
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ActionEffect
    {
        public ActionEffectType Type;
        public byte Param0;
        public byte Param1;
        public byte Param2;
        public byte Param3;
        public byte Param4;
        public ushort Value;
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

    public enum KnockbackDirection
    {
        AwayFromSource = 0, // direction = target-source
        Arg = 1, // direction = arg.degrees()
        Random = 2, // direction = random(0, 2pi)
        SourceForward = 3, // direction = src.direction
        SourceRight = 4, // direction = src.direction - pi/2
        SourceLeft = 5, // direction = src.direction + pi/2
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
                if (eff.Type != ActionEffectType.Nothing)
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

    public static class ActionEffectParser
    {
        public static string DescribeFields(ActionEffect eff)
        {
            // note: for all effects, bit 7 of param4 means "applied to caster instead of target"
            var res = new StringBuilder();
            switch (eff.Type)
            {
                case ActionEffectType.Damage:
                case ActionEffectType.BlockedDamage:
                case ActionEffectType.ParriedDamage:
                    // param0: bit 0 = crit, bit 1 = direct hit, others unused
                    // param1: damage/element type
                    // param2: bonus percent in log (purely visual)
                    // param3: third (high) byte of value (if bit 6 in param4 is set), 0 otherwise
                    // param4: bit1 = ? (seen when part of damage is absorbed by BLM manaward), bit2 = partial absorb? (seen when part of damage is absorbed by SMN succor), bit4 = immune (e.g. because of transcendent after raise),
                    //         bit5 = retaliation (set together with bit 7, e.g. for damage from vengeance), bit 6 = large value, bit 7 = applied to source, others unused
                    res.Append($"amount={eff.Value + ((eff.Param4 & 0x40) != 0 ? eff.Param3 * 0x10000 : 0)} {(DamageType)(eff.Param1 & 0x0F)} {(DamageElementType)(eff.Param1 >> 4)} ({(sbyte)eff.Param2}% bonus)");
                    if ((eff.Param0 & 1) != 0)
                        res.Append(", crit");
                    if ((eff.Param0 & 2) != 0)
                        res.Append(", dhit");
                    if ((eff.Param4 & 2) != 0)
                        res.Append(", manaward absorb?");
                    if ((eff.Param4 & 4) != 0)
                        res.Append(", partially absorbed?");
                    if ((eff.Param4 & 0x10) != 0)
                        res.Append(", immune");
                    if ((eff.Param4 & 0x20) != 0)
                        res.Append(", retaliation");
                    break;
                case ActionEffectType.Heal:
                    // param0: bit 0 = lifedrain? (e.g. melee bloodbath, SCH energy drain, etc. - also called "absorb"), bit 1 = nascent flash?, others unused
                    // param1: bit 0 = crit, others unused
                    // param2: unused
                    // param3: third (high) byte of value (if bit 6 in param4 is set), 0 otherwise
                    // param4: bit 6 = large value, bit 7 = applied to source, others unused
                    res.Append($"amount={eff.Value + ((eff.Param4 & 0x40) != 0 ? eff.Param3 * 0x10000 : 0)}");
                    if ((eff.Param1 & 1) != 0)
                        res.Append(", crit");
                    if ((eff.Param0 & 1) != 0)
                        res.Append(", lifedrain?");
                    if ((eff.Param0 & 2) != 0)
                        res.Append(", nascent flash?");
                    break;
                case ActionEffectType.Invulnerable:
                    // value: either 0 or status id
                    if (eff.Value != 0)
                        res.Append($"status {Utils.StatusString(eff.Value)}");
                    break;
                case ActionEffectType.MpGain:
                case ActionEffectType.TpGain:
                    res.Append($"amount={eff.Value}");
                    break;
                case ActionEffectType.ApplyStatusEffectTarget:
                case ActionEffectType.ApplyStatusEffectSource:
                    // param0/1: ??? (seen full range of values)
                    // param2: low byte of 'extra' (don't know where high byte is...)
                    // param3: unused
                    // param4: bit5 = retaliation, bit 7 = applied to source, others unused
                    res.Append($"{Utils.StatusString(eff.Value)} (xx{eff.Param2:X2})");
                    if ((eff.Param4 & 0x20) != 0)
                        res.Append(", retaliation");
                    break;
                case ActionEffectType.RecoveredFromStatusEffect:
                    // param0: low byte of 'extra' (don't know where high byte is...)
                    // param1-4: unused (except source bit in param4)
                    res.Append($"{Utils.StatusString(eff.Value)} (xx{eff.Param0:X2})");
                    break;
                case ActionEffectType.LoseStatusEffectTarget:
                case ActionEffectType.LoseStatusEffectSource:
                    res.Append(Utils.StatusString(eff.Value));
                    break;
                case ActionEffectType.Knockback:
                    var kbData = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Knockback>(eff.Value);
                    res.Append($"row={eff.Value}, dist={kbData?.Distance}+{eff.Param0}, dir={(KnockbackDirection?)kbData?.Direction}{(kbData?.Direction == (byte)KnockbackDirection.Arg ? $" ({kbData.DirectionArg}deg)" : "")}, speed={kbData?.Speed}");
                    break;
                case ActionEffectType.Attract1:
                case ActionEffectType.Attract2:
                    res.Append($"row={eff.Value}, TODO lumina...");
                    break;
                case ActionEffectType.AttractCustom1:
                case ActionEffectType.AttractCustom2:
                case ActionEffectType.AttractCustom3:
                    res.Append($"dist={eff.Value} (min={eff.Param1}), speed={eff.Param0}");
                    break;
                case ActionEffectType.SetHP:
                    res.Append($"value={eff.Value}");
                    break;
            }
            return res.ToString();
        }

        public static string DescribeUnknown(ActionEffect eff)
        {
            switch (eff.Type)
            {
                case ActionEffectType.Miss:
                case ActionEffectType.FullResist:
                case ActionEffectType.NoEffectText: // e.g. taunt immune
                case ActionEffectType.FailMissingStatus: // e.g. deployment tactics or bane when target doesn't have required status
                    // so far never seen any non-zero params
                    return eff.Param0 != 0 || eff.Param1 != 0 || eff.Param2 != 0 || eff.Param3 != 0 || eff.Param4 != 0 || eff.Value != 0 ? "non-zero params" : "";
                case ActionEffectType.Damage:
                case ActionEffectType.BlockedDamage:
                case ActionEffectType.ParriedDamage:
                    if ((eff.Param0 & ~3) != 0)
                        return $"param0={eff.Param0 & ~3:X2}";
                    else if (eff.Param3 != 0 && (eff.Param4 & 0x40) == 0)
                        return "non-zero param3 while large-value bit is unset";
                    else if ((eff.Param4 & ~0xF0) != 0)
                        return $"param4={eff.Param4 & ~0xF0:X2}";
                    else if ((eff.Param4 & 0x10) != 0 && eff.Value != 0)
                        return $"immune bit set but value is non-zero";
                    else if ((eff.Param4 & 0xA0) == 0x20)
                        return $"retaliation bit set but source is unset";
                    else
                        return "";
                case ActionEffectType.Heal:
                    if ((eff.Param0 & ~3) != 0)
                        return $"param0={eff.Param0 & ~3:X2}";
                    else if ((eff.Param1 & ~1) != 0)
                        return $"param1={eff.Param1 & ~1:X2}";
                    else if (eff.Param2 != 0)
                        return $"param2={eff.Param2}";
                    else if (eff.Param3 != 0 && (eff.Param4 & 0x40) == 0)
                        return "non-zero param3 while large-value bit is unset";
                    else if ((eff.Param4 & ~0xC0) != 0)
                        return $"param4={eff.Param4 & ~0xC0:X2}";
                    else if (eff.Param0 != 0 && (eff.Param4 & 0x80) == 0)
                        return "lifedrain bits set while source bit is unset";
                    else
                        return "";
                case ActionEffectType.Invulnerable:
                case ActionEffectType.MpGain:
                case ActionEffectType.TpGain:
                case ActionEffectType.LoseStatusEffectTarget:
                case ActionEffectType.LoseStatusEffectSource:
                case ActionEffectType.SetHP:
                    // so far only seen 'source' flag and non-zero values
                    return eff.Param0 != 0 || eff.Param1 != 0 || eff.Param2 != 0 || eff.Param3 != 0 || (eff.Param4 & ~0x80) != 0 ? "non-zero params" : "";
                case ActionEffectType.ApplyStatusEffectTarget:
                case ActionEffectType.ApplyStatusEffectSource:
                    if (eff.Param3 != 0 || (eff.Param4 & ~0xA0) != 0)
                        return "non-zero param3/4";
                    else if ((eff.Param4 & 0xA0) == 0x20)
                        return $"retaliation bit set but source is unset";
                    else
                        return "TODO investigate param0/1";// $"{Utils.StatusString(eff.Value)} {eff.Param0:X2}{eff.Param1:X2}"; - these are often non-zero, but I have no idea what they mean...
                case ActionEffectType.RecoveredFromStatusEffect:
                    return eff.Param1 != 0 || eff.Param2 != 0 || eff.Param3 != 0 || (eff.Param4 & ~0x80) != 0 ? "non-zero params" : "";
                case ActionEffectType.Knockback:
                    return eff.Param1 != 0 || eff.Param2 != 0 || eff.Param3 != 0 || eff.Param4 != 0 ? "non-zero params" : "";
                case ActionEffectType.Attract1:
                case ActionEffectType.Attract2:
                    return eff.Param0 != 0 || eff.Param1 != 0 || eff.Param2 != 0 || eff.Param3 != 0 || eff.Param4 != 0 ? "non-zero params" : "";
                case ActionEffectType.AttractCustom1:
                case ActionEffectType.AttractCustom2:
                case ActionEffectType.AttractCustom3:
                    return eff.Param2 != 0 || eff.Param3 != 0 || eff.Param4 != 0 ? "non-zero params" : "";
                default:
                    return $"unknown type";
            }
        }
    }
}
