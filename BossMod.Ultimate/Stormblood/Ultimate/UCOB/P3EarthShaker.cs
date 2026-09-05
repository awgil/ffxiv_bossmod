namespace BossMod.Stormblood.Ultimate.UCOB;

class P3EarthShaker(BossModule module) : Components.GenericBaitAway(module, AID.EarthShakerAOE)
{
    private List<Bait> _futureBaits = [];

    private static readonly AOEShapeCone _shape = new(60, 45.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Earthshaker && Module.Enemies(OID.BahamutPrime).FirstOrDefault() is var source && source != null)
        {
            var list = CurrentBaits.Count < 4 ? CurrentBaits : _futureBaits;
            list.Add(new(source, actor, _shape, WorldState.FutureTime(5.1f)));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<P3QuickmarchTrio>() is { } qmt)
        {
            var dirNorth = (qmt.RelativeNorth - Arena.Center).ToAngle();
            if (CurrentBaits.FirstOrNull(b => b.Target == actor) is { } bait)
            {
                var (safeDir, far) = assignment switch
                {
                    PartyRolesConfig.Assignment.H1 => (dirNorth + 45.Degrees(), true),
                    PartyRolesConfig.Assignment.H2 => (dirNorth - 45.Degrees(), true),
                    _ => (dirNorth - 135.Degrees(), false)
                };

                hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, safeDir, 60, 0, 1), bait.Activation);
                if (far)
                    hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 12), bait.Activation);
            }
            else if (actor.Role != Role.Tank)
            {
                foreach (var b in CurrentBaits)
                    hints.AddForbiddenZone(b.Shape, b.Source.Position, b.Rotation, b.Activation);
            }

            var damage = new BitMask();
            foreach (var b in CurrentBaits)
                damage.Set(Raid.FindSlot(b.Target.InstanceID));

            if (damage.Any())
                hints.AddPredictedDamage(damage, CurrentBaits[0].Activation);

            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.EarthShaker)
        {
            CurrentBaits.Clear();
            Utils.Swap(ref CurrentBaits, ref _futureBaits);
        }
    }
}

class P3EarthShakerVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 4, AID.EarthShakerAOE, OID.VoidzoneEarthShaker, 1.4f)
{
    readonly List<Actor> Targets = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Earthshaker)
            Targets.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && Targets.Count > 0)
        {
            _predictedByEvent.Add((Targets[0].Position, WorldState.FutureTime(CastEventToSpawn + ActivationDelay)));
            Targets.RemoveAt(0);
        }
    }
}
