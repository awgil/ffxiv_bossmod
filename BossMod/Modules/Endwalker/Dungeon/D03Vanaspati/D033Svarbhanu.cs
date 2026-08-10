namespace BossMod.Endwalker.Dungeon.D03Vanaspati.D033Svarbhanu;

public enum OID : uint
{
    Boss = 0x33EB, // R=7.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    AetherialDisruption = 25160, // Boss->self, 7.0s cast, single-target
    ChaoticPulse = 27489, // Boss->self, no cast, single-target
    ChaoticUndercurrentRedVisual = 25164, // Helper->self, no cast, single-target
    ChaoticUndercurrentBlueVisual = 25165, // Helper->self, no cast, single-target
    ChaoticUndercurrentRedRect = 25162, // Helper->self, no cast, range 40 width 10 rect
    ChaoticUndercurrentBlueRect = 25163, // Helper->self, no cast, range 40 width 10 rect
    CosmicKissVisual = 25161, // Boss->self, no cast, single-target
    CosmicKissCircle = 25167, // Helper->location, 3.0s cast, range 6 circle
    CosmicKissRect = 25374, // Helper->self, no cast, range 50 width 10 rect, 15 knockback, away from source
    CosmicKissSpread = 25168, // Helper->player, 8.0s cast, range 6 circle
    CosmicKiss = 25169, // Helper->location, 6.0s cast, range 100 circle, knockback 13, away from source
    CrumblingSky = 25166, // Boss->self, 3.0s cast, single-target
    FlamesOfDecay = 25170, // Boss->self, 5.0s cast, range 40 circle
    GnashingOfTeeth = 25171 // Boss->player, 5.0s cast, single-target
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlamesOfDecay && Arena.Bounds.Radius > 21f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 24.5f)], [new Square(center, 20f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 8.9d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x07 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
    }
}

sealed class ChaoticUndercurrent(BossModule module) : Components.GenericAOEs(module)
{
    public enum Pattern { None, BBRR, RRBB, BRRB, RBBR }
    public Pattern currentPattern;
    public readonly List<AOEInstance> AOEs = [with(2)];
    private static readonly AOEShapeRect rect = new(40f, 5f);
    private CosmicKissKnockback? _kb;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnMapEffect(byte index, uint state)
    {
        // index 0x08
        // red blue blue red => 0x00400020, red (-142, -172), blue (-152, -162)
        // blue blue red red => 0x00020001, red (-142, -152), blue (-162, -172) 
        // blue red red blue => 0x00100008, red (-152, -162), blue (-172, -142)
        // red red blue blue => 0x01000080, red (-162, -172), blue (-152, -142)
        if (index == 0x08)
        {
            currentPattern = state switch
            {
                0x00400020u => Pattern.RBBR,
                0x00020001u => Pattern.BBRR,
                0x00100008u => Pattern.BRRB,
                0x01000080u => Pattern.RRBB,
                _ => Pattern.None
            };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChaoticUndercurrentBlueVisual:
                AddAOEsForPattern(true);
                break;
            case (uint)AID.ChaoticUndercurrentRedVisual:
                AddAOEsForPattern(false);
                break;
            case (uint)AID.ChaoticUndercurrentRedRect:
            case (uint)AID.ChaoticUndercurrentBlueRect:
                AOEs.Clear();
                currentPattern = Pattern.None;
                break;
        }
    }

    private void AddAOEsForPattern(bool isBlue)
    {
        switch (currentPattern)
        {
            case Pattern.RBBR:
                AddAOEs(isBlue ? (1, 2) : (0, 3));
                break;
            case Pattern.BBRR:
                AddAOEs(isBlue ? (2, 3) : (0, 1));
                break;
            case Pattern.BRRB:
                AddAOEs(isBlue ? (0, 3) : (1, 2));
                break;
            case Pattern.RRBB:
                AddAOEs(isBlue ? (0, 1) : (2, 3));
                break;
        }
        void AddAOEs((int, int) indices)
        {
            WPos[] coords = [new(280f, -142f), new(280f, -152f), new(280f, -162f), new(280f, -172f)];
            AddAOE(coords[indices.Item1]);
            AddAOE(coords[indices.Item2]);
            void AddAOE(WPos pos)
            {
                var activation = WorldState.FutureTime(7.7d);
                AOEs.Add(new(rect, pos.Quantized(), Angle.AnglesCardinals[3], activation));
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AOEs.Count == 0)
            return;
        _kb ??= Module.FindComponent<CosmicKissKnockback>();

        if (_kb!.Casters.Count != 0 && !_kb.IsImmune(slot, _kb.Casters.Ref(0).Activation))
        { } // remove forbidden zones while knockback is active to not confuse the AI
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class CosmicKissSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.CosmicKissSpread, 6f);
sealed class CosmicKissCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CosmicKissCircle, 6f);

sealed class CosmicKissRect(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(9)];
    private static readonly AOEShapeRect rect = new(50f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 3 ? 3 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u && _aoes.Count == 0)
        {
            var aoeSets = index switch
            {
                0x0A => // for set 09, 0A, 0C --> 1B, 1D, 1E --> 09, 0A, 0B
                [[0, 2, 3], [0, 1, 3], [1, 2, 3]],
                0x0B => // for set 09, 0B, 0C --> 1B, 1C, 1E --> 0A, 0B, 0C
                [[0, 1, 3], [0, 2, 3], [0, 1, 2]],
                _ => new List<int[]>()
            };

            if (aoeSets.Count != 0)
            {
                AddAOEs(aoeSets[0], 4.3d);
                AddAOEs(aoeSets[1], 9.5d);
                AddAOEs(aoeSets[2], 14.6d);
            }
            void AddAOEs(int[] indices, double delay)
            {
                WPos[] coords = [new(320f, -142f), new(320f, -152f), new(320f, -162f), new(320f, -172f)];
                for (var i = 0; i < 3; ++i)
                {
                    _aoes.Add(new(rect, coords[indices[i]].Quantized(), Angle.AnglesCardinals[0], WorldState.FutureTime(delay)));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.CosmicKissRect)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class CosmicKissRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.CosmicKiss);

sealed class CosmicKissKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.CosmicKiss, 13f)
{
    private readonly ChaoticUndercurrent _aoe = module.FindComponent<ChaoticUndercurrent>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _aoe.AOEs.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var component = _aoe.AOEs;
        var count = component.Count;
        if (count != 0 && Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {

                var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
                var len = aoes.Length;
                var rects = new (WPos origin, WDir rotation)[len];
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    rects[i] = (aoe.Origin, aoe.Rotation.ToDirection());
                }

                hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOriginPlusAOERects(Arena.Center, c.Origin, 13f, 19f, rects, 40f, 5f, len), act);
            }
        }
    }
}

sealed class FlamesOfDecay(BossModule module) : Components.RaidwideCast(module, (uint)AID.FlamesOfDecay);
sealed class GnashingOfTeeth(BossModule module) : Components.SingleTargetCast(module, (uint)AID.GnashingOfTeeth);

sealed class D033SvarbhanuStates : StateMachineBuilder
{
    public D033SvarbhanuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<ChaoticUndercurrent>()
            .ActivateOnEnter<CosmicKissSpread>()
            .ActivateOnEnter<CosmicKissCircle>()
            .ActivateOnEnter<CosmicKissRect>()
            .ActivateOnEnter<CosmicKissKnockback>()
            .ActivateOnEnter<CosmicKissRaidwide>()
            .ActivateOnEnter<FlamesOfDecay>()
            .ActivateOnEnter<GnashingOfTeeth>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 789u, NameID = 10719u)]
public sealed class D033Svarbhanu(WorldState ws, Actor primary) : BossModule(ws, primary, new(300f, -157f), new ArenaBoundsSquare(24.5f));