namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P3Inception1(BossModule module) : Components.CastCounter(module, (uint)AID.JudgmentCrystalAOE)
{
    private readonly List<Actor> _plasmaspheres = [];
    private readonly Actor?[] _tetherSources = new Actor?[PartyState.MaxPartySize];
    private readonly WPos[] _assignedPositions = new WPos[PartyState.MaxPartySize];

    public bool AllSpheresSpawned => _plasmaspheres.Count == 4;
    public bool CrystalsDone => NumCasts > 0;

    private const float _crystalRadius = 5f;
    private const float _sphereRadius = 6f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!AllSpheresSpawned)
            return;

        var sphere = _tetherSources[slot];
        if (sphere != null)
        {
            if (!sphere.IsDead && Raid.WithSlot(true, true, true).WhereSlot(s => _tetherSources[s] != null).InRadiusExcluding(actor, _sphereRadius * 2f).Any())
                hints.Add("GTFO from other tethers!");
        }
        else if (!CrystalsDone)
        {
            if (Raid.WithSlot(true, true, true).WhereSlot(s => _tetherSources[s] == null).InRadiusExcluding(actor, _crystalRadius * 2f).Any())
                hints.Add("GTFO from other crystals!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (hints.FindEnemy(((TEA)Module).TrueHeart()) is AIHints.Enemy heart)
        {
            heart.ForbidDOTs = true;
            // hitting heart advances combo
            heart.Priority = AIHints.Enemy.PriorityPointless;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!AllSpheresSpawned)
            return;

        foreach (var (slot, player) in Raid.WithSlot(true, true, true))
        {
            var sphere = _tetherSources[slot];
            if (sphere != null)
            {
                if (!sphere.IsDead)
                {
                    Arena.Actor(sphere, Colors.Object, true);
                    Arena.AddLine(sphere.Position, player.Position, slot == pcSlot ? Colors.Safe : default);
                    Arena.ZoneCircleOutline(player.Position, _sphereRadius);
                }
            }
            else if (!CrystalsDone)
            {
                Arena.ZoneCircleOutline(player.Position, _crystalRadius);
            }
        }

        var pcSphere = _tetherSources[pcSlot];
        if (pcSphere != null)
        {
            if (!pcSphere.IsDead)
            {
                Arena.ZoneCircleOutline(_assignedPositions[pcSlot], 1, Colors.Safe);
            }
        }
        else if (!CrystalsDone)
        {
            var color = Colors.Safe;
            var pos = _assignedPositions[pcSlot];
            Arena.ZoneCircleOutline(pos + new WDir(-5f, -5f), 1f, color);
            Arena.ZoneCircleOutline(pos + new WDir(-5f, +5f), 1f, color);
            Arena.ZoneCircleOutline(pos + new WDir(+5f, -5f), 1f, color);
            Arena.ZoneCircleOutline(pos + new WDir(+5f, +5f), 1f, color);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Plasmasphere && source.OID == (uint)OID.Plasmasphere)
        {
            _plasmaspheres.Add(source);
            var slot = Raid.FindSlot(tether.Target);
            if (slot >= 0)
                _tetherSources[slot] = source;

            if (AllSpheresSpawned)
                InitAssignments();
        }
    }

    private void InitAssignments()
    {
        // alex is either at N or S cardinal; 2 spheres are E and 2 spheres are W
        // for tethered player, assign 45-degree spot on alex's side, as far away from source as possible
        var alexNorth = ((TEA)Module).AlexPrime()?.PosRot.Z < Arena.Center.Z;
        var boxPos = Arena.Center + new WDir(default, alexNorth ? 13f : -13f);
        for (var slot = 0; slot < _tetherSources.Length; ++slot)
        {
            var sphere = _tetherSources[slot];
            if (sphere != null)
            {
                var pos = sphere.Position;
                var sphereWest = pos.X < Arena.Center.X;
                var sameSideSphere = _plasmaspheres.Find(o => o != sphere && (o.PosRot.X < Arena.Center.X) == sphereWest);
                var sphereNorth = pos.Z < sameSideSphere?.PosRot.Z;

                var spotDir = alexNorth ? (sphereNorth ? 90f.Degrees() : 135f.Degrees()) : (sphereNorth ? 45f.Degrees() : 90f.Degrees());
                if (!sphereWest)
                    spotDir = -spotDir;
                _assignedPositions[slot] = Arena.Center + 18f * spotDir.ToDirection();
            }
            else
            {
                // TODO: consider assigning concrete spots...
                _assignedPositions[slot] = boxPos;
            }
        }
    }
}
