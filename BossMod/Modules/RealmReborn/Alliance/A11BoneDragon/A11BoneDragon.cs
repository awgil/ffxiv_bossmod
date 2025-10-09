namespace BossMod.RealmReborn.Alliance.A11BoneDragon;

public enum OID : uint
{
    Boss = 0x92B, // R5.000, x1
    Helper = 0x92C, // R0.500, x1
    Platinal = 0x92D, // R1.000, x0 (spawn during fight)
    RottingEye = 0x982, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    DarkWave = 736, // Boss->self, no cast, range 6+R circle
    AutoAttack = 1461, // Boss/Platinal->player, no cast, single-target
    DarkThorn = 745, // Boss->location, no cast, range 6 circle
    MiasmaBreath = 735, // Boss->self, no cast, range 8+R ?-degree cone
    HellSlash = 341, // Platinal->player, no cast, single-target
    Apocalypse = 749, // BoneDragon->location, 3.0s cast, range 6 circle
    Stone = 970, // RottingEye->player, 1.0s cast, single-target
    EvilEye = 750, // Boss->self, 3.0s cast, range 100+R 120-degree cone
}

class MiasmaBreath(BossModule module) : Components.Cleave(module, AID.MiasmaBreath, new AOEShapeCone(13, 45.Degrees()), activeWhileCasting: false);
class Apocalypse(BossModule module) : Components.StandardAOEs(module, AID.Apocalypse, 6);
class EvilEye(BossModule module) : Components.StandardAOEs(module, AID.EvilEye, new AOEShapeCone(105, 60.Degrees()));

class Platinal(BossModule module) : Components.Adds(module, (uint)OID.Platinal, 1);

class Poison(BossModule module) : BossComponent(module)
{
    private int _poisonStage;

    private static bool Safe(WPos center, int poisonStage, WPos p)
    {
        if (poisonStage == 0 || p.InCircle(center, 8))
            return true;

        for (var i = 0; i < 8; i++)
        {
            var deg = (45 * i).Degrees();

            // long platforms
            if (poisonStage == 1)
            {
                if (p.InRect(center + new WDir(0, 31.87f).Rotate(deg), deg, 17.675f, 17.675f, 3.9f))
                    return true;
            }

            if (poisonStage == 2)
            {
                if (p.InRect(center + new WDir(0, 18.02f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f))
                    return true;
                if (p.InRect(center + new WDir(0, 32.13f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f))
                    return true;
                if (p.InRect(center + new WDir(0, 45.745f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f))
                    return true;
            }
        }

        return false;
    }
    private bool Safe(WPos p) => Safe(Arena.Center, _poisonStage, p);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_poisonStage > 0)
        {
            Arena.ZoneCircle(Arena.Center, 8, ArenaColor.SafeFromAOE);

            for (var i = 0; i < 8; i++)
            {
                var deg = (45 * i).Degrees();

                if (_poisonStage == 1)
                {
                    Arena.ZoneRect(Arena.Center + new WDir(0, 31.87f).Rotate(deg), deg, 17.675f, 17.675f, 3.9f, ArenaColor.SafeFromAOE);
                }

                if (_poisonStage == 2)
                {
                    Arena.ZoneRect(Arena.Center + new WDir(0, 18.02f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f, ArenaColor.SafeFromAOE);
                    Arena.ZoneRect(Arena.Center + new WDir(0, 32.13f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f, ArenaColor.SafeFromAOE);
                    Arena.ZoneRect(Arena.Center + new WDir(0, 45.745f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f, ArenaColor.SafeFromAOE);
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_poisonStage > 0)
        {
            var center = Arena.Center;
            var stage = _poisonStage;
            hints.AddForbiddenZone(p => !Safe(center, stage, p), DateTime.MaxValue);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Safe(actor.Position))
            hints.Add("GTFO from poison!");
    }

    public override void OnLegacyMapEffect(byte seq, byte param, byte[] data)
    {
        if (data[0] == 3 && data[2] == 0x10)
            _poisonStage = 1;

        if (data[0] == 3 && data[2] == 0x20)
            _poisonStage = 2;

        if (data[0] == 3 && data[2] == 0x80)
            _poisonStage = 0;
    }
}
class BossDeathTracker(BossModule module) : BossComponent(module)
{
    public bool Dead { get; private set; }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000001 && param1 == 0xB4)
            Dead = true;
    }
}

class A11BoneDragonStates : StateMachineBuilder
{
    public A11BoneDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Apocalypse>()
            .ActivateOnEnter<BossDeathTracker>()
            .ActivateOnEnter<MiasmaBreath>()
            .ActivateOnEnter<EvilEye>()
            .ActivateOnEnter<Platinal>()
            .ActivateOnEnter<Poison>()
            .Raw.Update = () => Module.FindComponent<BossDeathTracker>()!.Dead || Module.PrimaryActor.IsDestroyed;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 706)]
public class A11BoneDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-451.2f, 23.93f), new ArenaBoundsCircle(49.4f, MapResolution: 1));
