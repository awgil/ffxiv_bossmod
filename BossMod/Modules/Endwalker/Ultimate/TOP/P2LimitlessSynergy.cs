namespace BossMod.Endwalker.Ultimate.TOP;

class P2OptimizedSagittariusArrow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OptimizedSagittariusArrow), new AOEShapeRect(100, 5));

class P2OptimizedBladedance : Components.BaitAwayTethers
{
    public P2OptimizedBladedance(BossModule module) : base(module, new AOEShapeCone(100, 45.Degrees()), (uint)TetherID.OptimizedBladedance, ActionID.MakeSpell(AID.OptimizedBladedanceAOE))
    {
        ForbiddenPlayers = Raid.WithSlot(true).WhereActor(p => p.Role != Role.Tank).Mask();
    }
}

class P2BeyondDefense(BossModule module) : Components.UniformStackSpread(module, 6, 5, 3, alwaysShowSpreads: true)
{
    public enum Mechanic { None, Spread, Stack }

    public Mechanic CurMechanic;
    private Actor? _source;
    private DateTime _activation;
    private BitMask _forbiddenStack;

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();
        if (_source != null)
        {
            switch (CurMechanic)
            {
                case Mechanic.Spread:
                    AddSpreads(Raid.WithoutSlot().SortedByRange(_source.Position).Take(2), _activation);
                    break;
                case Mechanic.Stack:
                    if (Raid.WithoutSlot().Closest(_source.Position) is var target && target != null)
                        AddStack(target, _activation, _forbiddenStack);
                    break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_source, ArenaColor.Object, true);
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SyntheticShield:
                _source = caster;
                break;
            case AID.BeyondDefense:
                _source = caster;
                CurMechanic = Mechanic.Spread;
                _activation = spell.NPCFinishAt.AddSeconds(0.2f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BeyondDefenseAOE:
                foreach (var t in spell.Targets)
                    _forbiddenStack.Set(Raid.FindSlot(t.ID));
                CurMechanic = Mechanic.Stack;
                _activation = WorldState.FutureTime(3.2f);
                break;
            case AID.PilePitch:
                CurMechanic = Mechanic.None;
                break;
        }
    }
}

class P2CosmoMemory(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CosmoMemoryAOE));

class P2OptimizedPassageOfArms(BossModule module) : BossComponent(module)
{
    public Actor? _invincible;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_invincible != null)
        {
            var e = hints.PotentialTargets.FirstOrDefault(e => e.Actor == _invincible);
            if (e != null)
            {
                e.Priority = AIHints.Enemy.PriorityForbidFully;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Invincibility && (OID)actor.OID == OID.OmegaM)
            _invincible = actor;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Invincibility && _invincible == actor)
            _invincible = null;
    }
}
