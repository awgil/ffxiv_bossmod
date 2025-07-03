namespace BossMod.Dawntrail.Ultimate.FRU;

class P1TurnOfHeavensBurntStrikeFire(BossModule module) : Components.StandardAOEs(module, AID.TurnOfHeavensBurntStrikeFire, new AOEShapeRect(80, 5));
class P1TurnOfHeavensBurntStrikeLightning(BossModule module) : Components.StandardAOEs(module, AID.TurnOfHeavensBurntStrikeLightning, new AOEShapeRect(80, 5));
class P1TurnOfHeavensBurnout(BossModule module) : Components.StandardAOEs(module, AID.TurnOfHeavensBurnout, new AOEShapeRect(80, 10));
class P1BrightfireSmall(BossModule module) : Components.StandardAOEs(module, AID.BrightfireSmall, new AOEShapeCircle(5));
class P1BrightfireLarge(BossModule module) : Components.StandardAOEs(module, AID.BrightfireLarge, new AOEShapeCircle(10));

// TODO: fixed tethers strat variant (tether target with clone on safe side goes S, other goes N, if any group has 5 players prio1 adjusts)
class P1BoundOfFaith(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, 4)
{
    public new bool EnableHints;
    public WDir SafeSide;
    public DateTime Activation;
    public readonly int[] AssignedGroups = new int[PartyState.MaxPartySize];
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private OID _safeHalo;

    public WDir AssignedLane(int slot) => new(0, AssignedGroups[slot] * 5.4f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // we have dedicated components for this

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (AssignedGroups[pcSlot] != 0 && SafeSide.X != 0)
        {
            Arena.AddCircle(Module.Center + SafeSide * 18.2f + AssignedLane(pcSlot), 1, ArenaColor.Safe);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _safeHalo = (AID)spell.Action.ID switch
        {
            AID.TurnOfHeavensFire => OID.HaloOfLevin,
            AID.TurnOfHeavensLightning => OID.HaloOfFlame,
            _ => _safeHalo
        };
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Fire && WorldState.Actors.Find(tether.Target) is var target && target != null)
        {
            Activation = WorldState.FutureTime(10.6f);
            AddStack(target, Activation);
            if (Stacks.Count == 2)
                InitAssignments();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BoundOfFaithSinsmoke)
            Stacks.Clear();
    }

    private void InitAssignments()
    {
        if (_safeHalo != default)
        {
            WDir averageOffset = default;
            foreach (var aoe in Module.Enemies(_safeHalo))
                averageOffset += aoe.Position - Module.Center;
            SafeSide.X = averageOffset.X > 0 ? 1 : -1;
        }

        // initial assignments
        Span<int> tetherSlots = [-1, -1];
        Span<int> prio = [0, 0, 0, 0, 0, 0, 0, 0];
        foreach (var (slot, group) in _config.P1BoundOfFaithAssignment.Resolve(Raid))
        {
            AssignedGroups[slot] = group < 4 ? -1 : 1;
            prio[slot] = group & 3;
            if (IsStackTarget(Raid[slot]))
                tetherSlots[tetherSlots[0] < 0 ? 0 : 1] = slot;
        }

        // swaps
        if (tetherSlots[0] >= 0 && AssignedGroups[tetherSlots[0]] == AssignedGroups[tetherSlots[1]])
        {
            // flex tether with lower prio
            var tetherFlexSlot = prio[tetherSlots[0]] < prio[tetherSlots[1]] ? tetherSlots[0] : tetherSlots[1];
            AssignedGroups[tetherFlexSlot] = -AssignedGroups[tetherFlexSlot];

            // now the group where we've moved flex slot has 5 people, find untethered with lowest prio
            for (var normalFlexSlot = 0; normalFlexSlot < PartyState.MaxPartySize; ++normalFlexSlot)
            {
                if (normalFlexSlot != tetherFlexSlot && prio[normalFlexSlot] == 0 && AssignedGroups[normalFlexSlot] == AssignedGroups[tetherFlexSlot])
                {
                    AssignedGroups[normalFlexSlot] = -AssignedGroups[normalFlexSlot];
                    break;
                }
            }
        }
    }
}

class P1BoundOfFaithAIKnockback(BossModule module) : BossComponent(module)
{
    private readonly P1BoundOfFaith? _comp = module.FindComponent<P1BoundOfFaith>();
    private bool _horizDone;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_comp == null || _comp.SafeSide == default)
            return;

        var sideOffset = _horizDone ? 0 : 7; // before horizonal aoes are done, we don't show knockback, so adjust the unsafe zone
        hints.AddForbiddenZone(ShapeContains.HalfPlane(Module.Center + sideOffset * _comp.SafeSide, _comp.SafeSide), _comp.Activation);

        var lane = _comp.AssignedLane(slot);
        if (_horizDone && lane.Z != 0)
        {
            hints.AddForbiddenZone(ShapeContains.InvertedRect(Module.Center + lane, new WDir(1, 0), 20, 20, 0.7f), _comp.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TurnOfHeavensBurnout)
            _horizDone = true;
    }
}

class P1BoundOfFaithAIStack(BossModule module) : BossComponent(module)
{
    private readonly P1BoundOfFaith? _comp = module.FindComponent<P1BoundOfFaith>();
    private bool _haveFetters;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_comp == null)
            return;

        if (_haveFetters)
        {
            // stack with closest (note: we could also stack with assigned, but that won't work well if people swap and assignments end up wrong)
            //var stackWith = _comp.Stacks.FirstOrDefault(s => _comp.AssignedGroups[Raid.FindSlot(s.Target.InstanceID)] == _comp.AssignedGroups[slot]);
            var stackWith = _comp.Stacks.MinBy(s => (s.Target.Position - actor.Position).LengthSq());
            foreach (var s in _comp.Stacks)
            {
                var zone = s.Target == stackWith.Target
                    ? ShapeContains.InvertedCircle(s.Target.Position, 4) // stay a bit closer to the target to avoid spooking people
                    : ShapeContains.Circle(s.Target.Position, 6);
                hints.AddForbiddenZone(zone, _comp.Activation);
            }

            // all else being equal, try staying closer to center
            hints.GoalZones.Add(hints.GoalSingleTarget(Module.Center, 7.5f, 0.5f));
        }
        else
        {
            // just go to center
            hints.AddForbiddenZone(ShapeContains.InvertedRect(Module.Center, new WDir(1, 0), 1, 1, 20), DateTime.MaxValue);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FloatingFetters)
            _haveFetters = true;
    }
}
