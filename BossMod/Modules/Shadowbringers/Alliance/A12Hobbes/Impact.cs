namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class Impact(BossModule module) : Components.StandardAOEs(module, AID.Impact, new AOEShapeCircle(15));
class Towerfall(BossModule module) : Components.GenericAOEs(module, AID.Towerfall)
{
    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var ix = _predicted.FindIndex(p => p.Origin.AlmostEqual(caster.Position, 1) && p.Rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (ix < 0)
                ReportError($"missing cast for {spell.Rotation}");
            else
                _predicted.RemoveAt(ix);
        }

        var deg = (AID)spell.Action.ID switch
        {
            AID.ImpactRotation1 => 0,
            AID.ImpactRotation2 => 67.5f,
            AID.ImpactRotation3 => 45,
            AID.ImpactRotation4 => 22.5f,
            _ => -1
        };

        if (deg >= 0)
        {
            var rotation = deg.Degrees();
            for (var i = 0; i < 4; i++)
            {
                _predicted.Add(new(new AOEShapeRect(20, 4), caster.Position, caster.Rotation + rotation, WorldState.FutureTime(3)));
                rotation += 90.Degrees();
            }
        }
    }
}

class ConvenientSelfDestruction(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(5), (uint)TetherID.ConvenientSelfDestruction, AID.ConvenientSelfDestruction, centerAtTarget: true)
{
    public DateTime Activation { get; private set; }

    // with this much time remaining before explosion, treat it as a regular spread mechanic, since trying to pass tethers might kill more people
    public const float PanicTime = 1.2f;

    private IEnumerable<Bait> ImportantBaits(Actor a)
    {
        var plat = A12Hobbes.PlatformCenters.MinBy(p => (p - a.Position).LengthSq());
        return CurrentBaits.Where(b => b.Source.Position.InCircle(plat, 20));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.Impact)
            Activation = WorldState.FutureTime(9.5f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (WorldState.FutureTime(PanicTime) > Activation)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        if (actor.Role == Role.Tank)
        {
            var playerBaits = ImportantBaits(actor).Where(b => b.Target.Role != Role.Tank).Select(b => ShapeContains.Rect(b.Source.Position, b.Target.Position, 1)).ToList();
            if (playerBaits.Count > 0)
            {
                var anyPlayerBait = ShapeContains.Union(playerBaits);
                hints.AddForbiddenZone(p => !anyPlayerBait(p), Activation);
                return;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (WorldState.FutureTime(PanicTime) > Activation)
        {
            base.AddHints(slot, actor, hints);
            return;
        }

        if (actor.Role == Role.Tank)
        {
            if (ImportantBaits(actor).Any(b => b.Target.Role != Role.Tank))
            {
                hints.Add("Take tethers from party!");
                return;
            }
        }
        else
        {
            var haveTank = Raid.WithoutSlot(excludeAlliance: true, includeDead: false).Any(p => p.Role == Role.Tank);
            if (ActiveBaitsOn(actor).Any() && haveTank)
            {
                hints.Add("Pass tether to tank!");
                return;
            }
        }

        base.AddHints(slot, actor, hints);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => ActiveBaitsOn(pc).Any() && pc.Role != Role.Tank && player.Role == Role.Tank && playerSlot < 8
            ? PlayerPriority.Critical
            : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
}
