using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.Interop;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;

namespace BossMod;

unsafe class DebugQuests
{
    private class NpcData
    {
        public readonly List<Quest> AllQuests = [];
        public List<Quest> AvailableQuests = [];
        public bool OutrankAll;
    }

    private record struct Rng(uint S0, uint S1 = 0, uint S2 = 0, uint S3 = 0)
    {
        public int Next(int range)
        {
            (S0, S1, S2, S3) = (S3, Transform(S0, S1), S1, S2);
            return (int)(S1 % range);
        }

        // returns new value for s1
        private static uint Transform(uint s0, uint s1)
        {
            var temp = s0 ^ (s0 << 11);
            return s1 ^ temp ^ ((temp ^ (s1 >> 11)) >> 8);
        }
    }

    private readonly UITree _tree = new();
    private readonly Dictionary<uint, NpcData> _dailyQuests = [];

    public DebugQuests()
    {
        foreach (var q in Service.LuminaSheet<Quest>()!)
        {
            if (q.RepeatIntervalType == 1 && q.QuestRepeatFlag.RowId == 0)
            {
                _dailyQuests.GetOrAdd(q.IssuerStart.RowId).AllQuests.Add(q);
            }
        }
        foreach (var quests in _dailyQuests.Values)
        {
            // TODO map Unknown11
            // quests.AllQuests.SortBy(q => (q.Unknown11 == 3, q.RowId));
            var rankMin = (int)quests.AllQuests.Min(q => q.BeastReputationRank.RowId);
            var rankMax = (int)quests.AllQuests.Max(q => q.BeastReputationRank.RowId);
            var rankCur = RankForQuest(quests.AllQuests[0], out var rankedUp);
            quests.OutrankAll = quests.AllQuests.All(q => RankForQuest(q, out _) > q.BeastReputationRank.RowId);
            quests.AvailableQuests = CalculateAvailable(quests.AllQuests, QuestManager.Instance()->DailyQuestSeed, quests.OutrankAll, rankCur, rankedUp);
        }
    }

    public void Draw()
    {
        var qm = QuestManager.Instance();
        foreach (var n in _tree.Node($"Player (seed={qm->DailyQuestSeed})###player"))
        {
            uint i = 1;
            foreach (ref var t in qm->BeastReputation)
            {
                _tree.LeafNode($"Tribe {i} '{Service.LuminaRow<BeastTribe>(i)?.Name}': rank={t.Rank}, value={t.Value}");
                ++i;
            }
        }

        var fwk = EventFramework.Instance();
        foreach (var n in _tree.Node($"Dailies map ({fwk->DailyQuests.Entries.Count})###dailies_map"))
        {
            foreach (var (baseId, entry) in fwk->DailyQuests.Entries)
            {
                foreach (var nnpc in _tree.Node($"NPC {baseId:X}: tribe={entry.TribeId} '{Service.LuminaRow<BeastTribe>(entry.TribeId)?.Name}', ranks={entry.RankRequirementMin}-{entry.RankRequirementMax}, dirty={entry.Dirty}###{baseId}"))
                {
                    int i = 0;
                    foreach (var e in entry.HandlersNormal)
                    {
                        _tree.LeafNode($"[Gx {i++}] {e.Value->QuestId} '{Service.LuminaRow<Quest>(0x10000u | e.Value->QuestId)?.Name}'");
                    }

                    i = 0;
                    foreach (var e in entry.HandlersExclusive)
                    {
                        _tree.LeafNode($"[G3 {i++}] {e.Value->QuestId} '{Service.LuminaRow<Quest>(0x10000u | e.Value->QuestId)?.Name}'");
                    }
                }
            }
        }

        foreach (var n in _tree.Node("Full daily map"))
        {
            foreach (var (baseId, quests) in _dailyQuests)
            {
                foreach (var nnpc in _tree.Node($"NPC {baseId:X}, outrank={quests.OutrankAll}"))
                {
                    int i = 0;
                    foreach (var q in quests.AllQuests)
                    {
                        // TODO map Unknown11
                        _tree.LeafNode($"[{i++}] G{{q.Unknown11}} {(IsEligible(q) ? "+" : "-")}{(quests.AvailableQuests.Contains(q) ? "+" : "-")} {q.RowId} '{q.Name}'");
                    }
                }
            }
        }

        var target = TargetSystem.Instance()->GetTargetObject();
        using (ImRaii.Disabled(target == null))
            if (ImGui.Button("Compare..."))
                CompareLogic(target->BaseId);
    }

    private int RankForQuest(Quest q, out bool rankedUp)
    {
        var tribe = q.BeastTribe.RowId;
        var curRank = QuestManager.Instance()->BeastReputation[(int)tribe - 1].Rank;
        rankedUp = (curRank & 0x80) != 0;
        return curRank & 0x7F;
    }

    private bool IsEligible(Quest q)
    {
        var playerRank = RankForQuest(q, out var rankedUp);
        return IsEligible(q, playerRank, rankedUp);
    }

    private bool IsEligible(Quest q, int playerRank, bool rankedUp)
    {
        var questRank = q.BeastReputationRank.RowId;
        return rankedUp ? questRank == playerRank : questRank <= playerRank;
    }

    private List<Quest> CalculateAvailable(List<Quest> potential, byte seed, bool outrankAll, int playerRank, bool rankedUp)
    {
        List<Quest> eligible = [.. potential.Where(q => IsEligible(q, playerRank, rankedUp))];
        List<Quest> available = [];
        if (eligible.Count == 0)
            return available;

        var rng = new Rng(seed);
        if (outrankAll)
        {
            for (int i = 0, cnt = Math.Min(eligible.Count, 3); i < cnt; ++i)
            {
                var index = rng.Next(eligible.Count);
                while (available.Contains(eligible[index]))
                    index = (index + 1) % eligible.Count;
                available.Add(eligible[index]);
            }
        }
        else
        {
            // TODO map Unknown11
            var firstExclusive = -1; //  eligible.FindIndex(q => q.Unknown11 == 3);
            if (firstExclusive >= 0)
                available.Add(eligible[firstExclusive + rng.Next(eligible.Count - firstExclusive)]);
            else
                firstExclusive = eligible.Count;

            for (int i = available.Count, cnt = Math.Min(firstExclusive, 3); i < cnt; ++i)
            {
                var index = rng.Next(firstExclusive);
                while (available.Contains(eligible[index]))
                    index = (index + 1) % firstExclusive;
                available.Add(eligible[index]);
            }
        }
        return available;
    }

    private void CompareLogic(uint npcId)
    {
        var fwk = EventFramework.Instance();
        var loc = fwk->DailyQuests.Entries.WithOps.Tree.FindLowerBound(npcId);
        if (loc.KeyEquals(npcId))
        {
            Span<bool> bools = [false, true];
            Span<nint> availStorage = [0, 0, 0];
            var avail = (QuestEventHandler**)availStorage.GetPointer(0);
            Service.Log($"Comparing {npcId:X} r={loc.Bound->_Myval.Item2.RankRequirementMin}-{loc.Bound->_Myval.Item2.RankRequirementMax}");
            for (int rank = loc.Bound->_Myval.Item2.RankRequirementMin; rank <= loc.Bound->_Myval.Item2.RankRequirementMax; ++rank)
            {
                foreach (var rankInRange in bools)
                {
                    foreach (var exactMatch in bools)
                    {
                        for (int seed = 0; seed < 256; ++seed)
                        {
                            var numAvail = (int)fwk->DailyQuests.CalculateAvailableQuests(null, loc.Bound, (byte)seed, rankInRange, (byte)rank, exactMatch, avail);
                            var ourAvail = CalculateAvailable(_dailyQuests[npcId].AllQuests, (byte)seed, !rankInRange, rank, exactMatch);
                            if (ourAvail.Count != numAvail || Enumerable.Range(0, numAvail).Any(i => ((uint)avail[i]->QuestId | 0x10000) != ourAvail[i].RowId))
                            {
                                Service.Log($"Mismatch: s={seed},rank={rank}/{rankInRange}/{exactMatch}: game={string.Join('/', Enumerable.Range(0, numAvail).Select(i => (uint)avail[i]->QuestId | 0x10000))}, our={string.Join('/', ourAvail.Select(q => q.RowId))}");
                            }
                        }
                    }
                }
            }
        }
    }
}
