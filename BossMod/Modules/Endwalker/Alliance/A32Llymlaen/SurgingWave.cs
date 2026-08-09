namespace BossMod.Endwalker.Alliance.A32Llymlaen;

sealed class SurgingWaveCorridor(BossModule module) : BossComponent(module)
{
    public WDir CorridorDir;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x49 && state is 0x02000001u or 0x00200001u or 0x00800040u or 0x08000400u)
        {
            CorridorDir = state switch
            {
                0x00800040u => new(-1f, default),
                0x08000400u => new(1f, default),
                _ => default
            };
        }
    }
}

sealed class SurgingWaveAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurgingWaveAOE, 6f);
sealed class SurgingWaveShockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SurgingWaveShockwave, 68f, true)
{
    private RelSimplifiedComplexPolygon poly;
    private bool polyInit;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0 && Arena.Bounds is ArenaBoundsCustom arena)
        {
            if (!polyInit)
            {
                poly = arena.Polygon.Offset(-1f); // shrink polygon by 1 yalm for less suspect kb
                polyInit = true;
            }

            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDKnockbackInComplexPolygonAwayFromOrigin(Arena.Center, c.Origin, 68f, poly), c.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            polyInit = false;
        }
    }
}

sealed class SurgingWaveSeaFoam(BossModule module) : Components.Voidzone(module, 1.5f, GetVoidzones, 3f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SeaFoam);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}

sealed class SurgingWaveFrothingSea : Components.Exaflare
{
    public SurgingWaveFrothingSea(BossModule module) : base(module, new AOEShapeRect(6f, 20f, 80f))
    {
        ImminentColor = Colors.AOE;
        FutureColor = Colors.Danger;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        void AddLine(WPos first, Angle rot) => Lines.Add(new(first, 2.3f * rot.ToDirection(), WorldState.FutureTime(30d), 0.9d, 13, 2, rot));
        if (index == 0x49)
        {
            if (state == 0x00800040u)
                AddLine(new(-80f, -900f), 90f.Degrees());
            else if (state == 0x08000400u)
                AddLine(new(80f, -900f), -90f.Degrees());
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SurgingWaveFrothingSea)
        {
            ++NumCasts;
            if (Lines.Count != 0)
            {
                var line = Lines[0];
                AdvanceLine(line, line.Next + 2.3f * line.Rotation.ToDirection());
                if (line.ExplosionsLeft == 0)
                    Lines.RemoveAt(0);
            }
        }
    }
}

sealed class Strait(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftStrait, (uint)AID.RightStrait], new AOEShapeCone(100f, 90f.Degrees()));
