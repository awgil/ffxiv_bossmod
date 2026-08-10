namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

sealed class ShimmeringShot(BossModule module) : Components.GenericAOEs(module)
{
    public enum Pattern { Unknown, EWNormal, EWInverted, WENormal, WEInverted }
    private readonly PlayerTemperatures _temps = module.FindComponent<PlayerTemperatures>()!;
    private Pattern _pattern;
    private readonly AOEInstance[][] _safezone = new AOEInstance[PartyState.MaxAllianceSize][];
    private readonly AOEShapeRect square = new(5f, 5f, 5f, invertForbiddenZone: true);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => slot is < 0 or > 23 ? [] : _safezone[slot];

    public override void OnActorCreated(Actor actor)
    {
        var temp = actor.OID switch
        {
            (uint)OID.GlacierArrow => 2u,
            (uint)OID.SparkArrow => 3u,
            (uint)OID.FrostArrow => 1u,
            (uint)OID.FlameArrow => 4u,
            _ => default
        };
        if (temp != default)
        {
            int[] remap = _pattern switch
            {
                Pattern.EWNormal => [2, 0, 4, 2, 3],
                Pattern.WENormal => [2, 4, 0, 3, 1],
                Pattern.EWInverted => [1, 3, 0, 4, 2],
                Pattern.WEInverted => [3, 1, 4, 0, 2],
                _ => []
            };
            if (remap == default)
                return;

            var isEW = _pattern is Pattern.EWNormal or Pattern.EWInverted;
            var srcRow = RowIndex();
            if (!isEW)
                srcRow = 4 - srcRow;
            var xOffset = isEW ? -20f : 20f;
            var destRow = remap[srcRow];
            var zOffset = 10f * (destRow - 2);
            var temps = _temps.Temperatures;
            var act = WorldState.FutureTime(10.8d);
            var pos = new WPos(-272f, -82f) + new WDir(xOffset, zOffset);
            for (var i = 0; i < 24; ++i)
            {
                var playertemp = temps[i];
                if (playertemp != default && playertemp == temp)
                {
                    _safezone[i] = [new(square, pos, default, act, Colors.SafeFromAOE)];
                }
            }
        }
        int RowIndex() => (actor.Position.Z - -82f) switch
        {
            < -15 => 0,
            < -5 => 1,
            < 5 => 2,
            < 15 => 3,
            _ => 4
        };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ChillArrow or (uint)AID.FreezingArrow or (uint)AID.HeatedArrow or (uint)AID.SearingArrow)
        {
            Array.Clear(_safezone);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        _pattern = (index, state) switch
        {
            (0x14, 0x00200010u) => Pattern.EWNormal,
            (0x14, 0x00020001u) => Pattern.EWInverted,
            (0x15, 0x00200010u) => Pattern.WENormal,
            (0x15, 0x00020001u) => Pattern.WEInverted,
            _ => Pattern.Unknown
        };
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
            return;
        ref readonly var aoe = ref aoes[0];
        hints.Add("Stand in correct tile!", !aoe.Check(actor.Position));
    }
}
