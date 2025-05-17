namespace BossMod.Shadowbringers.Quest.FadedMemories;

class Overcome(BossModule module) : Components.StandardAOEs(module, AID.Overcome, new AOEShapeCone(8, 60.Degrees()), 2);
class Skydrive(BossModule module) : Components.StandardAOEs(module, AID.Skydrive, new AOEShapeCircle(5));

class SkyHighDrive(BossModule module) : Components.GenericRotatingAOE(module)
{
    Angle angle;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SkyHighDriveCCW:
                angle = -20.Degrees();
                return;
            case AID.SkyHighDriveCW:
                angle = 20.Degrees();
                return;
            case AID.SkyHighDriveFirst:
                if (angle != default)
                {
                    Sequences.Add(new(new AOEShapeRect(40, 4), caster.Position, spell.Rotation, angle, Module.CastFinishAt(spell, 0.5f), 0.6f, 10, 4));
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SkyHighDriveFirst or AID.SkyHighDriveRest)
        {
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
            if (Sequences.Count == 0)
                angle = default;
        }
    }
}

class AvalancheAxe(BossModule module) : Components.StandardAOEs(module, AID.AvalanceAxe1, new AOEShapeCircle(10));
class AvalancheAxe2(BossModule module) : Components.StandardAOEs(module, AID.AvalanceAxe2, new AOEShapeCircle(10));
class AvalancheAxe3(BossModule module) : Components.StandardAOEs(module, AID.AvalanceAxe3, new AOEShapeCircle(10));
class OvercomeAllOdds(BossModule module) : Components.StandardAOEs(module, AID.OvercomeAllOdds, new AOEShapeCone(60, 15.Degrees()), 1)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (NumCasts > 0)
            MaxCasts = 2;
    }
}
class Soulflash(BossModule module) : Components.StandardAOEs(module, AID.Soulflash1, new AOEShapeCircle(4));
class EtesianAxe(BossModule module) : Components.KnockbackFromCastTarget(module, AID.EtesianAxe, 15, kind: Kind.DirForward);
class Soulflash2(BossModule module) : Components.StandardAOEs(module, AID.Soulflash2, new AOEShapeCircle(8));

class GroundbreakerExaflares(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GroundbreakerExaFirst)
        {
            Lines.Add(new Line
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 6,
                Rotation = default,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1,
                ExplosionsLeft = 8,
                MaxShownExplosions = 3
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.GroundbreakerExaFirst or (uint)AID.GroundbreakerExaRest)
        {
            var line = Lines.FirstOrDefault(x => x.Next.AlmostEqual(caster.Position, 1));
            if (line != null)
                AdvanceLine(line, caster.Position);
        }
    }
}

class GroundbreakerCone(BossModule module) : Components.StandardAOEs(module, AID.GroundbreakerCone, new AOEShapeCone(40, 45.Degrees()));
class GroundbreakerDonut(BossModule module) : Components.StandardAOEs(module, AID.GroundbreakerDonut, new AOEShapeDonut(5, 20));
class GroundbreakerCircle(BossModule module) : Components.StandardAOEs(module, AID.GroundbreakerCircle, new AOEShapeCircle(15));

class ArdbertStates : StateMachineBuilder
{
    public ArdbertStates(BossModule module) : base(module)
    {
        TrivialPhase(0)
            .ActivateOnEnter<SkyHighDrive>()
            .ActivateOnEnter<Skydrive>()
            .ActivateOnEnter<Overcome>()
            .ActivateOnEnter<AvalancheAxe>()
            .ActivateOnEnter<AvalancheAxe2>()
            .ActivateOnEnter<AvalancheAxe3>()
            .ActivateOnEnter<OvercomeAllOdds>()
            .ActivateOnEnter<Soulflash>()
            .ActivateOnEnter<EtesianAxe>()
            .ActivateOnEnter<Soulflash2>()
            .ActivateOnEnter<GroundbreakerExaflares>()
            .ActivateOnEnter<GroundbreakerCone>()
            .ActivateOnEnter<GroundbreakerDonut>()
            .ActivateOnEnter<GroundbreakerCircle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69311, NameID = 8258, PrimaryActorOID = (uint)OID.Ardbert)]
public class Ardbert(WorldState ws, Actor primary) : BossModule(ws, primary, new(-392, 780), new ArenaBoundsCircle(20));
