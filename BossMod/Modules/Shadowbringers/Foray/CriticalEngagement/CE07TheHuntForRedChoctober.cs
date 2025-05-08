namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE07TheHuntForRedChoctober;

public enum OID : uint
{
    Boss = 0x2E68,
    Helper = 0x233C,
}

public enum AID : uint
{
    ChocoSlash = 20588, // Boss->player, 5.0s cast, single-target
    ChocoMeteorImpact = 20570, // Boss->players, no cast, range 5 circle
    ChocoMeteorain = 21424, // Helper->self, 11.0s cast, range 40 circle
    ChocoBeak = 20582, // Helper->self, 5.0s cast, range 40 width 16 rect
    ChocoMeteorStreamSlow = 20586, // Helper->location, 7.0s cast, range 8 circle
    ChocoMeteorStreamFast = 20587, // Helper->location, 1.0s cast, range 8 circle
    ChocoMeteoruption = 20568, // Helper->location, 4.0s cast, range 6 circle
    ChocoMeteorStrikeFirst = 20575, // Helper->self, 5.0s cast, range 8 circle
    ChocoMeteorStrikeRest = 20576, // Helper->self, no cast, range 8 circle
    ChocoBeakWide = 20574, // Helper->self, 5.0s cast, range 40 width 20 rect
    ChocoComet = 20580, // Helper->location, 5.0s cast, range 5 circle
}

public enum IconID : uint
{
    Stack = 161, // player->self
}

class ChocoMeteoruption(BossModule module) : Components.StandardAOEs(module, AID.ChocoMeteoruption, 6);
class ChocoMeteorain(BossModule module) : Components.StandardAOEs(module, AID.ChocoMeteorain, new AOEShapeCircle(21), maxCasts: 2);
class ChocoMeteorImpact(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.ChocoMeteorImpact, 5, 5.1f);
class ChocoSlash(BossModule module) : Components.SingleTargetCast(module, AID.ChocoSlash);
class ChocoBeak(BossModule module) : Components.StandardAOEs(module, AID.ChocoBeak, new AOEShapeRect(40, 8));
class ChocoBeakWide(BossModule module) : Components.StandardAOEs(module, AID.ChocoBeakWide, new AOEShapeRect(40, 10));
class ChocoComet(BossModule module) : Components.CastTowers(module, AID.ChocoComet, 5);
class ChocoMeteorStrike(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(8))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChocoMeteorStrikeFirst)
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 4,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1,
                ExplosionsLeft = 8,
                MaxShownExplosions = 3
            });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ChocoMeteorStrikeFirst or AID.ChocoMeteorStrikeRest && Lines.Find(l => l.Next.AlmostEqual(caster.Position, 1)) is { } l)
        {
            AdvanceLine(l, caster.Position);
        }
    }
}

class ChocoMeteorStreamFirst(BossModule module) : Components.StandardAOEs(module, AID.ChocoMeteorStreamSlow, 8);

class ChocoMeteorStream(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<WPos> Locations = [];
    private readonly List<AOEInstance> PredictedCasts = [];

    private Angle? Rotation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChocoMeteorStreamSlow or AID.ChocoMeteorStreamFast)
        {
            if (Locations.Count == 2)
            {
                var rot0 = Angle.FromDirection(Locations[0] - Arena.Center);
                var rot1 = Angle.FromDirection(spell.LocXZ - Arena.Center);
                AddRotations(rot0.Normalized(), rot1.Normalized(), Module.CastFinishAt(spell));
            }
            Locations.Add(spell.LocXZ);
        }
    }

    private void AddRotations(Angle prev, Angle cur, DateTime activate)
    {
        Rotation = cur - prev;
        var nextRot = cur.Normalized();
        for (var i = 0; i < 13; i++)
        {
            PredictedCasts.Add(new AOEInstance(new AOEShapeCircle(8), Arena.Center + new WDir(0, 17).Rotate(nextRot), default, activate));
            PredictedCasts.Add(new AOEInstance(new AOEShapeCircle(8), Arena.Center + new WDir(0, -17).Rotate(nextRot), default, activate));
            nextRot = (nextRot + Rotation.Value).Normalized();
            activate += TimeSpan.FromSeconds(2);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChocoMeteorStreamSlow or AID.ChocoMeteorStreamFast)
        {
            NumCasts++;
            if (PredictedCasts.Count > 0)
                PredictedCasts.RemoveAt(0);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var a in PredictedCasts.Take(6))
            yield return a with { Color = i++ < 2 ? ArenaColor.Danger : ArenaColor.AOE };
    }
}

class RedCometStates : StateMachineBuilder
{
    public RedCometStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChocoMeteorain>()
            .ActivateOnEnter<ChocoMeteorImpact>()
            .ActivateOnEnter<ChocoSlash>()
            .ActivateOnEnter<ChocoBeak>()
            .ActivateOnEnter<ChocoBeakWide>()
            .ActivateOnEnter<ChocoMeteorStreamFirst>()
            .ActivateOnEnter<ChocoMeteorStream>()
            .ActivateOnEnter<ChocoComet>()
            .ActivateOnEnter<ChocoMeteorStrike>()
            .ActivateOnEnter<ChocoMeteoruption>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 7)] // bnpcname=9407
public class RedComet(WorldState ws, Actor primary) : BossModule(ws, primary, new(393, 268), new ArenaBoundsCircle(19.5f));

