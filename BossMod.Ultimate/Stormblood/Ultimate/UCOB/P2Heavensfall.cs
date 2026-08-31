using static BossMod.PartyRolesConfig;

namespace BossMod.Stormblood.Ultimate.UCOB;

class P2Heavensfall(BossModule module) : Components.Knockback(module, AID.Heavensfall, true)
{
    public DateTime Activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => [new Source(Module.Center, 11, Activation)];

    public override void AddAIHints(int slot, Actor actor, Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(ShapeDistance.PrecisePosition(new WPos(0, 9), new(0, 1), 0.5f, actor.Position, 0.1f), Activation);
    }
}

class P2HeavensfallPillar(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID != OID.EventHelper)
            return;
        switch (state)
        {
            case 0x00040008: // appear
                _aoe = new(_shape, actor.Position, actor.Rotation);
                break;
            // 0x00100020: ? 0.5s after appear
            // 0x00400080: ? 4.0s after appear
            // 0x01000200: ? 5.8s after appear
            // 0x04000800: ? 7.5s after appear
            // 0x10002000: ? 9.4s after appear
            case 0x40008000: // disappear (11.1s after appear)
                _aoe = null;
                break;
        }
    }
}

class P2ThermionicBurst(BossModule module) : Components.StandardAOEs(module, AID.ThermionicBurst, new AOEShapeCone(24.5f, 11.25f.Degrees()));

class P2MeteorStream : Components.UniformStackSpread
{
    public int NumCasts;

    public P2MeteorStream(BossModule module) : base(module, 0, 4, alwaysShowSpreads: true)
    {
        AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(5.6f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MeteorStream)
        {
            ++NumCasts;
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);

            // update activation time for second set
            if (NumCasts == 4)
                for (var i = 0; i < 4; i++)
                    Spreads.Ref(i).Activation = WorldState.FutureTime(3.1f);
        }
    }

    public override void AddAIHints(int slot, Actor actor, Assignment assignment, AIHints hints)
    {
        if (Spreads.Count == 8)
        {
            var (dist, angle) = assignment switch
            {
                Assignment.MT => (9, -11.25f.Degrees()),
                Assignment.OT => (9, 11.25f.Degrees()),
                Assignment.H1 => (18, -11.25f.Degrees()),
                Assignment.H2 => (18, 11.25f.Degrees()),
                Assignment.M1 => (9, -56.25f.Degrees()),
                Assignment.M2 => (9, 56.25f.Degrees()),
                Assignment.R1 => (18, -56.25f.Degrees()),
                Assignment.R2 => (18, 56.25f.Degrees()),
                _ => (0, default)
            };

            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center + angle.ToDirection() * dist, 2), Spreads[0].Activation);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class P2HeavensfallDalamudDive(BossModule module) : Components.GenericBaitAway(module, AID.DalamudDive, true, true)
{
    private readonly Actor? _target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);

    private static readonly AOEShapeCircle _shape = new(5);

    public void Show()
    {
        if (_target != null)
            CurrentBaits.Add(new(_target, _target, _shape));
    }

    public override void AddAIHints(int slot, Actor actor, Assignment assignment, AIHints hints)
    {
        if (!CurrentBaits.Any(b => b.Target == actor))
            base.AddAIHints(slot, actor, assignment, hints);

        // preposition close to nael
        if (actor.Role is Role.Melee or Role.Tank)
            foreach (var b in ActiveBaitsNotOn(actor))
                hints.GoalZones.Add(AIHints.GoalSingleTarget(b.Target.Position, 6));
    }
}
