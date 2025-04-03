namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

class ABSide(BossModule module) : BossComponent(module)
{
    public enum Mechanic { None, Roles, Parties }
    public Mechanic NextMechanic { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_FlipToASide:
                NextMechanic = Mechanic.Roles;
                break;
            case AID._Weaponskill_FlipToBSide:
                NextMechanic = Mechanic.Parties;
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        switch (NextMechanic)
        {
            case Mechanic.Roles:
                hints.Add("Next: roles");
                break;
            case Mechanic.Parties:
                hints.Add("Next: light parties");
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_PlayASide1:
            case AID._Weaponskill_PlayBSide1:
                NextMechanic = Mechanic.None;
                break;
        }
    }
}

class PlayASide(BossModule module) : BossComponent(module)
{
    public bool Active { get; private set; }
    private ABSide? _ab;

    private DateTime Activation;

    private IEnumerable<Actor> DifferentRole(Actor pc) => Raid.WithoutSlot().Where(r => r.Class.GetRole2() != pc.Class.GetRole2());

    public override void Update()
    {
        _ab ??= Module.FindComponent<ABSide>();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_2SnapTwistDropTheNeedle2 or AID._Weaponskill_3SnapTwistDropTheNeedle3 or AID._Weaponskill_4SnapTwistDropTheNeedle4)
        {
            Active = _ab?.NextMechanic == ABSide.Mechanic.Roles;
            if (Active)
                Activation = WorldState.FutureTime(5.3f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_PlayASide1)
            Active = false;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        Arena.AddCone(Module.PrimaryActor.Position, 60, Module.PrimaryActor.AngleTo(pc), 22.5f.Degrees(), ArenaColor.Danger);

        var cat = pc.Class.GetRole2();
        if (cat != Role2.DPS)
            DrawBait(Role2.DPS);
        if (cat != Role2.Healer)
            DrawBait(Role2.Healer);
        if (cat != Role2.Tank)
            DrawBait(Role2.Tank);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Active)
            return;

        hints.PredictedDamage.Add((Raid.WithSlot().Mask(), Activation));

        var cones = DifferentRole(actor).Select(p => ShapeDistance.Cone(Module.PrimaryActor.Position, 60, Module.PrimaryActor.AngleTo(p), 22.5f.Degrees())).ToList();
        if (cones.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Union(cones), Activation);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (!Active)
            return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);

        return pc.Class.GetRole3() == player.Class.GetRole3() ? PlayerPriority.Normal : PlayerPriority.Danger;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;

        hints.Add("Bait away from raid!", DifferentRole(actor).Any(p => p.Position.InCone(Module.PrimaryActor.Position, Module.PrimaryActor.DirectionTo(actor), 22.5f.Degrees())));
    }

    private void DrawBait(Role2 c)
    {
        if (Raid.WithoutSlot().FirstOrDefault(r => r.Class.GetRole2() == c) is { } actor)
            Arena.ZoneCone(Module.PrimaryActor.Position, 0, 60, Module.PrimaryActor.AngleTo(actor), 22.5f.Degrees(), ArenaColor.AOE);
    }
}

class PlayBSide(BossModule module) : Components.GenericWildCharge(module, 4, ActionID.MakeSpell(AID._Weaponskill_PlayBSide1), 60)
{
    private ABSide? _ab;

    public override void Update()
    {
        _ab ??= Module.FindComponent<ABSide>();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_ab?.NextMechanic == ABSide.Mechanic.Parties && (AID)spell.Action.ID is AID._Weaponskill_2SnapTwistDropTheNeedle2 or AID._Weaponskill_3SnapTwistDropTheNeedle3 or AID._Weaponskill_4SnapTwistDropTheNeedle4)
        {
            Source = Module.PrimaryActor;
            Array.Fill(PlayerRoles, PlayerRole.Share);
            foreach (var (i, player) in Raid.WithSlot())
                if (player.Role == Role.Healer)
                    PlayerRoles[i] = PlayerRole.Target;
            Activation = WorldState.FutureTime(5.3f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Source = null;
    }
}

class PlaySideCounter(BossModule module) : Components.CastCounterMulti(module, [ActionID.MakeSpell(AID._Weaponskill_PlayASide1), ActionID.MakeSpell(AID._Weaponskill_PlayBSide1)]);
