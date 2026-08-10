namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V20ShishuChochin;

public enum OID : uint
{
    ShishuChochin = 0x3EFC, // R2.0
    Lantern = 0x3F18, // R1.0
}

public enum AID : uint
{
    AutoAttack = 6499, // ShishuChochin->player, no cast, single-target

    Illume = 33618 // ShishuChochin->self, 3.0s cast, range 6 90-degree cone
}

sealed class Lanterns(BossModule module) : Components.GenericAOEs(module)
{
    private readonly V20ShishuChochinConfig _config = Service.Config.Get<V20ShishuChochinConfig>();

    private readonly Circle lantern1 = new(new(723.5f, 57.5f), 1f), lantern2 = new(new(690.5f, 57.5f), 1f), lantern3 = new(new(681.2f, 51.6f), 1f);
    private readonly List<Circle> lanterns = [with(3)];
    private AOEInstance[] _aoe = [];
    private bool aoeInit;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!aoeInit)
        {
            InitAOE();
        }
        return _aoe;
    }

    private void InitAOE()
    {
        if (lanterns.Count == 0)
        {
            lanterns.AddRange([lantern1, lantern2, lantern3]);
        }
        var center = Arena.Center;
        var shape = new AOEShapeCustom(center, [.. lanterns], invertForbiddenZone: true);
        _aoe = [new(shape, center, default, WorldState.FutureTime(99d), Colors.SafeFromAOE, shapeDistance: shape.InvertedDistance(center, default))];
        aoeInit = true;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            var circ = index switch
            {
                0x48 => lantern1,
                0x47 => lantern2,
                0x46 => lantern3,
                _ => null
            };
            if (circ != null)
            {
                lanterns.Remove(circ);
            }
            InitAOE();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Illume)
        {
            ++NumCasts;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.P12LanternAI || !aoeInit)
        {
            return;
        }
        var count = (3 - NumCasts) == lanterns.Count;
        if (count)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
        var lanternPriorityCount = 0;

        var countT = hints.PotentialTargets.Count;
        for (var i = 0; i < countT; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID == (uint)OID.ShishuChochin)
            {
                ref var aoe = ref _aoe[0];
                var inAOE = aoe.Check(actor.Position);
                if (lanternPriorityCount == 0 && inAOE && count)
                {
                    Actor? closestBoss = null;
                    var closestDistance = float.MaxValue;
                    var boss = Module.Enemies((uint)OID.ShishuChochin);
                    var countBoss = boss.Count;
                    for (var j = 0; j < countBoss; ++j)
                    {
                        var b = boss[i];
                        var distance = (b.Position - actor.Position).LengthSq();
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestBoss = b;
                        }
                    }

                    if (closestBoss == e.Actor)
                    {
                        e.Priority = 1;
                        ++lanternPriorityCount;
                    }
                }
                else
                {
                    e.Priority = AIHints.Enemy.PriorityUndesirable;
                }
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("To unlock path 12, pull the ghosts to the marked spots\nand kill them one at a time while they face a lantern.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = lanterns.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(lanterns[i].Center, 5f, Colors.Safe, 5f);
        }
    }
}

sealed class Illume(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Illume, new AOEShapeCone(6f, 45f.Degrees()));

sealed class V20ShishuChochinStates : StateMachineBuilder
{
    public V20ShishuChochinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Lanterns>()
            .ActivateOnEnter<Illume>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.ShishuChochin);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.ShishuChochin, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12396, SortOrder = 1, Category = BossModuleInfo.Category.VariantCriterion, Expansion = BossModuleInfo.Expansion.Endwalker)]
public sealed class V20ShishuChochin : BossModule
{
    public V20ShishuChochin(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private V20ShishuChochin(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(701.34f, -29.85f), new(701.24f, -19.5f), new(701.24f, 6.28f), new(701.24f, 6.79f), new(700.88f, 7.17f),
        new(686.24f, 17.56f), new(685.38f, 17.77f), new(685.07f, 18.25f), new(685.05f, 18.76f), new(685.12f, 19.34f),
        new(684.91f, 19.86f), new(684.9f, 31.92f), new(684.92f, 32.74f), new(685.45f, 33), new(717.76f, 33),
        new(717.89f, 38.42f), new(717.69f, 38.92f), new(717.07f, 39.1f), new(716.79f, 39.54f), new(716.79f, 40.25f),
        new(717.39f, 40.49f), new(717.92f, 40.66f), new(719.73f, 40.7f), new(719.77f, 41.29f), new(719.77f, 41.89f),
        new(720.04f, 42.97f), new(722.02f, 43.22f), new(722.42f, 42.81f), new(722.65f, 42.32f), new(723.67f, 42.25f),
        new(724.17f, 41.99f), new(724.24f, 41.4f), new(724.26f, 40.9f), new(724.58f, 40.49f), new(725.11f, 40.48f),
        new(725.64f, 40.41f), new(726.42f, 40.1f), new(727.45f, 40.01f), new(731.44f, 39.98f), new(731.99f, 40.06f),
        new(731.99f, 41.06f), new(732.37f, 41.45f), new(733.64f, 41.45f), new(733.73f, 41.99f), new(733.86f, 42.5f),
        new(734.24f, 42.9f), new(738.37f, 49.46f), new(738.46f, 49.96f), new(738.46f, 54.6f), new(737.91f, 54.69f),
        new(737.35f, 54.69f), new(736.89f, 54.91f), new(736.62f, 55.36f), new(736.65f, 55.95f), new(736.7f, 56.49f),
        new(736.49f, 56.97f), new(736.48f, 58.6f), new(735.17f, 58.6f), new(734.84f, 59.02f), new(728.56f, 59.02f),
        new(728.19f, 58.61f), new(727.04f, 58.58f), new(726.53f, 58.61f), new(724.22f, 58.61f), new(724.22f, 58.02f),
        new(723.84f, 57.67f), new(722.67f, 57.68f), new(722.44f, 58.13f), new(721.66f, 58.57f), new(720.57f, 58.61f),
        new(720.57f, 56.06f), new(720.23f, 55.6f), new(719.5f, 55.58f), new(719.42f, 55.06f), new(719.11f, 54.64f),
        new(717.44f, 54.56f), new(716.89f, 54.37f), new(715.18f, 54.46f), new(714.15f, 54.69f), new(713.64f, 54.9f),
        new(713.52f, 55.53f), new(712.87f, 55.65f), new(712.46f, 55.97f), new(712.48f, 58.6f), new(711.35f, 58.6f),
        new(711.26f, 56.92f), new(711.26f, 55.81f), new(710.81f, 55.41f), new(710.2f, 55.29f), new(710.15f, 54.75f),
        new(709.91f, 54.27f), new(709.4f, 54.16f), new(708.87f, 54.08f), new(708.27f, 54.19f), new(707.74f, 54.25f),
        new(707.45f, 54.78f), new(707.24f, 55.31f), new(706.67f, 55.37f), new(706.15f, 55.37f), new(705.55f, 55.41f),
        new(705.3f, 55.86f), new(705.2f, 56.44f), new(703.71f, 56.44f), new(703.35f, 56.82f), new(703.25f, 57.36f),
        new(703.38f, 58.48f), new(702.82f, 58.61f), new(696.11f, 58.97f), new(695.6f, 58.78f), new(695.11f, 58.61f),
        new(691.34f, 58.61f), new(691.34f, 58), new(690.92f, 57.59f), new(689.88f, 57.66f), new(689.6f, 58.09f),
        new(688.21f, 58.57f), new(687.52f, 58.61f), new(684.39f, 58.59f), new(684.28f, 57.09f), new(684.29f, 55.39f),
        new(683.98f, 54.94f), new(683.41f, 54.93f), new(683.33f, 54.41f), new(682.9f, 54.02f), new(681.06f, 54.1f),
        new(680.46f, 54.1f), new(680.21f, 53.64f), new(680.22f, 53.08f), new(680.31f, 52.49f), new(680.81f, 52.49f),
        new(681.13f, 52.06f), new(681.13f, 51.03f), new(680.67f, 50.75f), new(680.21f, 50.55f), new(683.2f, 50.47f),
        new(683.73f, 50.39f), new(683.91f, 49.84f), new(683.92f, 48.65f), new(683.89f, 48.15f), new(683.34f, 47.88f),
        new(682.77f, 47.84f), new(682.28f, 47.61f), new(682.29f, 46.4f), new(681.83f, 45.99f), new(681.34f, 45.84f),
        new(680.77f, 46.02f), new(680.2f, 45.83f), new(680.51f, 45.42f), new(682.8f, 45.32f), new(683.36f, 45.06f),
        new(683.54f, 44.58f), new(683.49f, 44.08f), new(683.55f, 43.52f), new(683.64f, 42.9f), new(683.74f, 41.22f),
        new(683.94f, 40.73f), new(684.99f, 40.56f), new(685.08f, 42.41f), new(685.45f, 42.86f), new(686.51f, 43.18f),
        new(687.03f, 43.32f), new(687.55f, 43.14f), new(689.22f, 43.06f), new(689.67f, 42.7f), new(689.67f, 42.14f),
        new(689.57f, 41.58f), new(689.54f, 41.03f), new(694.38f, 40.54f), new(694.47f, 41.4f), new(694.88f, 41.77f),
        new(695.85f, 41.77f), new(696.26f, 41.43f), new(696.22f, 40.55f), new(697.72f, 40.55f), new(698.46f, 40.71f),
        new(698.6f, 41.21f), new(700.16f, 42.65f), new(701.8f, 43.48f), new(702.15f, 43.07f), new(702.46f, 42.62f),
        new(705.33f, 42.63f), new(705.91f, 42.61f), new(706.18f, 43.6f), new(706.63f, 43.89f), new(707.39f, 43.89f),
        new(707.92f, 43.86f), new(708.13f, 44.32f), new(708.65f, 44.63f), new(709.16f, 44.57f), new(710.25f, 44.51f),
        new(710.8f, 44.3f), new(711.32f, 44.03f), new(711.58f, 43.54f), new(711.8f, 42.39f), new(711.91f, 40.84f),
        new(714.53f, 40.45f), new(714.88f, 40.07f), new(714.87f, 39.54f), new(714.41f, 39.24f), new(712.09f, 39.24f),
        new(711.44f, 39.11f), new(711.37f, 37.16f), new(710.86f, 36.88f), new(710.22f, 36.92f), new(709.14f, 36.92f),
        new(680.96f, 36.97f), new(680.96f, 22.45f), new(681.02f, 17.95f), new(680.54f, 17.66f), new(680.45f, 15.48f),
        new(680.97f, 15.39f), new(681.5f, 15.39f), new(681.85f, 14.94f), new(681.94f, 14.41f), new(681.94f, 13.85f),
        new(681.87f, 12.77f), new(681.44f, 12.46f), new(680.85f, 12.46f), new(680.45f, 7.79f), new(684.94f, 7.54f),
        new(685.23f, 7.08f), new(685.23f, 5.67f), new(685.02f, 1.02f), new(685.09f, -29.98f), new(701.27f, -30.04f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.ShishuChochin));
    }
}
