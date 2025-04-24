namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class RiverPhaseArena(BossModule module) : BossComponent(module)
{
    private bool RiverSafe;
    private DateTime Activation;

    private static RelSimplifiedComplexPolygon RiverPoly => RM06SSugarRiot.RiverPoly;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DoubleStyleFire)
        {
            RiverSafe = true;
            Activation = WorldState.FutureTime(10.2f);
        }

        if ((AID)spell.Action.ID == AID.DoubleStyleLightning)
        {
            RiverSafe = false;
            Activation = WorldState.FutureTime(10.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TasteOfThunderSpread or AID.TasteOfFire)
            Activation = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Activation == default)
            return;

        if (RiverSafe)
            hints.Add("Stand in water!", !RiverPoly.Contains(actor.Position - Arena.Center));
        else
            hints.Add("Avoid water!", RiverPoly.Contains(actor.Position - Arena.Center));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation == default)
            return;

        var chk = new AOEShapeCustom(RiverPoly).CheckFn(Arena.Center, default);
        hints.AddForbiddenZone(RiverSafe ? p => !chk(p) : chk, Activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Activation == default)
            return;

        Arena.ZoneComplex(Arena.Center, default, RiverPoly, RiverSafe ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }
}

class DoubleStyleFireLightning(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DoubleStyleFire)
        {
            foreach (var p in Raid.WithoutSlot().Where(r => r.Role == Role.Healer))
                Stacks.Add(new(p, 6, 4, 4, WorldState.FutureTime(10.2f)));
        }

        if ((AID)spell.Action.ID == AID.DoubleStyleLightning)
        {
            foreach (var p in Raid.WithoutSlot())
                Spreads.Add(new(p, 6, WorldState.FutureTime(10.2f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TasteOfFire && Stacks.Count > 0)
        {
            NumCasts++;
            Stacks.RemoveAt(0);
        }
        if ((AID)spell.Action.ID == AID.TasteOfThunderSpread && Spreads.Count > 0)
        {
            NumCasts++;
            Spreads.RemoveAt(0);
        }
    }
}

class StormPhaseArena(BossModule module) : Components.GenericAOEs(module, warningText: "GTFO from voidzone!")
{
    private DateTime Activation;

    public enum ArenaType { Normal, Storm, Lava }

    public ArenaType CurArena;
    public ArenaType NextArena;

    public override void OnEventEnvControl(byte index, uint state)
    {
        switch ((index, state))
        {
            case (4, 0x00800040):
                Activation = WorldState.FutureTime(7.1f);
                NextArena = ArenaType.Storm;
                break;
            case (4, 0x00200010):
                CurArena = ArenaType.Storm;
                Arena.Bounds = new ArenaBoundsCustom(20, Arena.Bounds.Clipper.Difference(new(CurveApprox.Rect(new WDir(1, 0), 20, 20)), new(RM06SSugarRiot.RiverPoly)));
                NextArena = default;
                break;

            case (4, 0x02000100):
                Activation = WorldState.FutureTime(7.1f);
                NextArena = ArenaType.Lava;
                break;
            case (4, 0x08000004):
                Arena.Bounds = new ArenaBoundsCustom(20, Arena.Bounds.Clipper.Difference(new(CurveApprox.Rect(new WDir(1, 0), 20, 20)), new(RM06SSugarRiot.LavaPoly)));
                CurArena = ArenaType.Lava;
                NextArena = default;
                break;

            case (03, 0x00020001):
                Arena.Bounds = new ArenaBoundsSquare(20);
                CurArena = default;
                NextArena = default;
                break;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        switch (NextArena)
        {
            case ArenaType.Storm:
                yield return new AOEInstance(new AOEShapeCustom(RM06SSugarRiot.RiverPoly), Arena.Center, default, Activation);
                break;
            case ArenaType.Lava:
                yield return new AOEInstance(new AOEShapeCustom(RM06SSugarRiot.LavaPoly), Arena.Center, default, Activation);
                break;
        }
    }
}
