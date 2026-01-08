namespace BossMod.Dawntrail.Savage.RM09SVampFatale;

class KillerVoice(BossModule module) : Components.RaidwideCast(module, AID.KillerVoice);

class HardcoreSmall(BossModule module) : Components.BaitAwayCast(module, AID.HardcoreSmall, new AOEShapeCircle(6), true, true);
class HardcoreBig(BossModule module) : Components.BaitAwayCast(module, AID.HardcoreBig, new AOEShapeCircle(15), true, true);
class HardcoreCounter(BossModule module) : Components.CastCounterMulti(module, [AID.HardcoreSmall, AID.HardcoreBig]);

class BrutalRain(BossModule module) : Components.StackWithIcon(module, (uint)IconID.ShareMulti, AID.BrutalRainHit, 6, 5.1f, 4)
{
    int _numExpected;
    int _numStacks;
    public int NumCasts { get; private set; }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        base.OnEventIcon(actor, iconID, targetID);
        if (iconID == StackIcon)
        {
            _numStacks = Module.PrimaryActor.FindStatus(SID.Satisfied) is { } stat ? stat.Extra & 0xFF : 0;
            // guessing
            _numExpected = 3 + _numStacks / 4;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            NumCasts++;
            if (NumCasts >= _numExpected)
            {
                if (Stacks.Count == 0)
                    ReportError($"Extra hit from Brutal Rain; expected {_numExpected} hits but this is hit {NumCasts}, boss had {_numStacks} stacks");
                Stacks.Clear();
            }
        }
    }
}

class SadisticScreech(BossModule module) : Components.RaidwideCastDelay(module, AID.SadisticScreechCast, AID.SadisticScreech, 0.8f);

class HalfMoon(BossModule module) : Components.GroupedAOEs(module, [AID.HalfMoonShort1, AID.HalfMoonLong1, AID.HalfMoonShort4, AID.HalfMoonLong4, AID.HalfMoonShort2, AID.HalfMoonLong2, AID.HalfMoonShort3, AID.HalfMoonLong3], new AOEShapeCone(64, 90.Degrees()), maxCasts: 1);

class CrowdKill(BossModule module) : Components.RaidwideCastDelay(module, AID.CrowdKillCast, AID.CrowdKill, 5.1f);
class FinaleFatale(BossModule module) : Components.RaidwideCastDelay(module, AID.FinaleFataleCast, AID.FinaleFatale, 1.3f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FinaleFataleCast or AID.FinaleFataleCast2)
            Activation = Module.CastFinishAt(spell, Delay);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FinaleFatale or AID.FinaleFatale2)
        {
            NumCasts++;
            Activation = default;
        }
    }
}
class InsatiableThirst(BossModule module) : Components.RaidwideCastDelay(module, AID.InsatiableThirstCast, AID.InsatiableThirst, 3.3f);

class RingBounds(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x10)
        {
            if (state == 0x00020001)
                Arena.Bounds = new ArenaBoundsCircle(20);
            if (state == 0x00080004)
                Arena.Bounds = new ArenaBoundsSquare(20);
        }
    }
}

class PulpingPulse(BossModule module) : Components.StandardAOEs(module, AID.PulpingPulse, 5);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1069, NameID = 14300, PlanLevel = 100)]
public class RM09SVampFatale(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
