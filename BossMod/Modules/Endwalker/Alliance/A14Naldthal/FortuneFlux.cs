namespace BossMod.Endwalker.Alliance.A14Naldthal;

class FortuneFluxOrder(BossModule module) : BossComponent(module)
{
    public enum Mechanic { None, AOE, Knockback }

    public List<(WPos source, Mechanic mechanic, DateTime activation)> Mechanics = [];
    public int NumComplete;
    private WPos _currentTethered;
    private Mechanic _currentMechanic;

    public override void AddGlobalHints(GlobalHints hints)
    {
        var order = string.Join(" > ", Mechanics.Skip(NumComplete).Select(m => m.mechanic));
        if (order.Length > 0)
            hints.Add($"Order: {order}");
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.FiredUp)
        {
            _currentTethered = source.Position;
            TryAdd();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FiredUp1AOE:
            case AID.FiredUp2AOE:
            case AID.FiredUp3AOE:
                _currentMechanic = Mechanic.AOE;
                TryAdd();
                break;
            case AID.FiredUp1Knockback:
            case AID.FiredUp2Knockback:
            case AID.FiredUp3Knockback:
                _currentMechanic = Mechanic.Knockback;
                TryAdd();
                break;
            case AID.FortuneFluxAOE1:
                UpdateActivation(0, Mechanic.AOE, spell);
                break;
            case AID.FortuneFluxAOE2:
                UpdateActivation(1, Mechanic.AOE, spell);
                break;
            case AID.FortuneFluxAOE3:
                UpdateActivation(2, Mechanic.AOE, spell);
                break;
            case AID.FortuneFluxKnockback1:
                UpdateActivation(0, Mechanic.Knockback, spell);
                break;
            case AID.FortuneFluxKnockback2:
                UpdateActivation(1, Mechanic.Knockback, spell);
                break;
            case AID.FortuneFluxKnockback3:
                UpdateActivation(2, Mechanic.Knockback, spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FortuneFluxAOE1 or AID.FortuneFluxAOE2 or AID.FortuneFluxAOE3 or AID.FortuneFluxKnockback1 or AID.FortuneFluxKnockback2 or AID.FortuneFluxKnockback3)
            ++NumComplete;
    }

    private void TryAdd()
    {
        if (_currentTethered != default && _currentMechanic != Mechanic.None)
        {
            Mechanics.Add((_currentTethered, _currentMechanic, DateTime.MaxValue));
            _currentTethered = default;
            _currentMechanic = Mechanic.None;
        }
    }

    private void UpdateActivation(int order, Mechanic mechanic, ActorCastInfo spell)
    {
        if (order >= Mechanics.Count)
        {
            ReportError($"Unexpected mechanic #{order}, only {Mechanics.Count} in list");
            return;
        }

        ref var m = ref Mechanics.AsSpan()[order];
        if (m.mechanic != mechanic)
        {
            ReportError($"Unexpected mechanic #{order}: started {mechanic}, expected {m.mechanic}");
            m.mechanic = mechanic;
        }

        if (!m.source.AlmostEqual(spell.LocXZ, 0.5f))
        {
            ReportError($"Unexpected mechanic #{order} position: started {spell.LocXZ}, expected {m.source}");
            m.source = spell.LocXZ;
        }

        if (m.activation != DateTime.MaxValue)
        {
            ReportError($"Several cast-start for #{order}");
        }
        m.activation = spell.NPCFinishAt;
    }
}

class FortuneFluxAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly FortuneFluxOrder? _order = module.FindComponent<FortuneFluxOrder>();

    private static readonly AOEShapeCircle _shape = new(20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_order != null)
            foreach (var m in _order.Mechanics.Skip(_order.NumComplete).Where(m => m.mechanic == FortuneFluxOrder.Mechanic.AOE))
                yield return new(_shape, m.source, default, m.activation);
    }
}

class FortuneFluxKnockback(BossModule module) : Components.Knockback(module)
{
    private readonly FortuneFluxOrder? _order = module.FindComponent<FortuneFluxOrder>();

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_order != null)
            foreach (var m in _order.Mechanics.Skip(_order.NumComplete).Where(m => m.mechanic == FortuneFluxOrder.Mechanic.Knockback))
                yield return new(m.source, 30, m.activation);
    }
}
