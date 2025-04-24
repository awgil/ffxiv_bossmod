namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class Adds(BossModule module) : BossComponent(module)
{
    public int YanCounter;
    public int SquirrelCounter;
    public int CatCounter;
    public int JabberwockCounter;
    public int RayCounter;

    private int HuffyCat;

    private readonly RM06SSugarRiotConfig _config = Service.Config.Get<RM06SSugarRiotConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var squirrels = Module.Enemies(OID.Mu);
        Arena.Actors(Module.Enemies(OID.Yan), ArenaColor.Enemy);
        Arena.Actors(squirrels, ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID.GimmeCat), ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID.Jabberwock), ArenaColor.Enemy);
        Arena.Actors(Module.Enemies(OID.FeatherRay), ArenaColor.Enemy);

        if (squirrels.Any(x => !x.IsDeadOrDestroyed))
            foreach (var ram in Module.Enemies(OID.Yan).Where(x => x.IsTargetable && !x.IsDead && x.TargetID == pc.InstanceID))
                Arena.AddCircle(ram.Position, 13, ArenaColor.Danger); // estimate of Rallying Cheer radius
    }

    public override void OnTargetable(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID.Yan:
                YanCounter++;
                break;
            case OID.Mu:
                SquirrelCounter++;
                break;
            case OID.GimmeCat:
                CatCounter++;
                break;
            case OID.Jabberwock:
                JabberwockCounter++;
                break;
            case OID.FeatherRay:
                RayCounter++;
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.EnableAddsHints)
            return;

        var playerIsRanged = actor.Role is Role.Ranged or Role.Healer;

        foreach (var h in hints.PotentialTargets)
        {
            switch ((OID)h.Actor.OID)
            {
                case OID.GimmeCat:
                    h.AllowDOTs = _config.CatDotPolicy switch
                    {
                        RM06SSugarRiotConfig.CatDotStrategy.RangedOnly => playerIsRanged,
                        RM06SSugarRiotConfig.CatDotStrategy.None => false,
                        _ => true
                    };
                    h.Priority = 1;
                    break;

                case OID.Mu:
                case OID.Yan:
                    h.Priority = 1;
                    h.ForbidDOTs = _config.SmartDots;
                    break;

                case OID.FeatherRay:
                    h.ForbidDOTs = _config.SmartDots;

                    if (TetheredRays.Contains(h.Actor.InstanceID))
                    {
                        h.Priority = 1;

                        // prioritize rays over using aoe on squirrel
                        if (_config.MantaPrio && (playerIsRanged || actor.DistanceToHitbox(h.Actor) <= 3))
                            h.Priority = 3;
                    }
                    else if (!playerIsRanged && _config.ForbiddenManta)
                        // prevent melees/tanks from getting aggro on ray
                        // if ray times out and tethers a random person, it will be added to the set and this conditional will be skipped
                        h.Priority = AIHints.Enemy.PriorityForbidden;
                    break;

                case OID.Jabberwock:
                    h.ForbidDOTs = _config.SmartDots;
                    h.Priority = _config.JabberwockPrio ? 5 : 1;
                    h.ShouldBeStunned = true;
                    break;
            }
        }
    }

    private readonly HashSet<ulong> TetheredRays = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.FeatherRay)
            TetheredRays.Add(source.InstanceID);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.FeatherRay)
            TetheredRays.Remove(source.InstanceID);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HuffyCat)
            HuffyCat++;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HuffyCat)
            HuffyCat = 0;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (HuffyCat > 0)
            hints.Add($"Cat stacks: {HuffyCat}");
    }
}

class ICraveViolence(BossModule module) : Components.StandardAOEs(module, AID.ICraveViolence, new AOEShapeCircle(6));
class WaterIII(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, AID.WaterIIISpread, m => m.Enemies(0x1EBD91).Where(o => o.EventState != 7), 1.5f, 5);
class WaterIIITether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(8), (uint)TetherID.FeatherRay, centerAtTarget: true)
{
    public override void Update()
    {
        CurrentBaits.RemoveAll(b => b.Source.IsDeadOrDestroyed);
    }
}
class ReadyOreNot(BossModule module) : Components.RaidwideCast(module, AID.ReadyOreNot);
class OreRigato(BossModule module) : Components.RaidwideCast(module, AID.OreRigato, "Squirrel enrage!");
class HangryHiss(BossModule module) : Components.RaidwideCast(module, AID.HangryHiss, "Cat enrage!");
