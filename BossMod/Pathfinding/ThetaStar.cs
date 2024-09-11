namespace BossMod.Pathfinding;

public class ThetaStar
{
    public enum Score
    {
        JustBad, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), destination is unsafe and has same or lower max-g than starting cell
        UltimatelyBetter, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), destination is unsafe but has larger max-g than starting cell
        UltimatelySafe, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), however destination is safe
        UnsafeAsStart, // the path is unsafe (there are cells along the path with negative leeway, but no max-g lower than starting cell), destination is unsafe with same max-g as starting cell (starting cell will have this score if its max-g is <= 0)
        SemiSafeAsStart, // the path is semi-safe (no cell along the path has negative leeway or max-g lower than starting cell), destination is unsafe with same max-g as starting cell (starting cell will have this score if its max-g is > 0)
        UnsafeImprove, // the path is unsafe (there are cells along the path with negative leeway, but no max-g lower than starting cell), destination is at least better than start
        SemiSafeImprove, // the path is semi-safe (no cell along the path has negative leeway or max-g lower than starting cell), destination is unsafe but better than start
        Safe, // the path reaches safe cell and is fully safe (no cell along the path has negative leeway) (starting cell will have this score if it's safe)
        SafeMaxPrio, // the path reaches safe cell with max goal priority and is fully safe (no cell along the path has negative leeway)
    }

    public struct Node
    {
        public float GScore;
        public float HScore;
        public int ParentIndex;
        public int OpenHeapIndex; // -1 if in closed list, 0 if not in any lists, otherwise (index+1)
        public float PathLeeway; // min diff along path between node's g-value and cell's g-value
        public float PathMinG; // minimum 'max g' value along path
        public Score Score;
        public Vector2 EnterOffset; // from cell center; up to +-0.5

        public readonly float FScore => GScore + HScore;
    }

    private Map _map = new();
    //private Vector2 _goalFrac;
    //private float _goalRadius;
    private Node[] _nodes = [];
    private readonly List<int> _openList = [];
    private float _deltaGSide;
    private int _startNodeIndex;
    private float _startMaxG;
    private Score _startScore;

    private int _bestIndex; // node with best score
    private int _fallbackIndex; // best 'fallback' node: node that we don't necessarily want to go to, but might want to move closer to it (to the parent)

    // statistics
    public int NumSteps { get; private set; }
    public int NumReopens { get; private set; }

    private const float BorderCushion = 0.1f;
    private const float MaxNeighbourOffset = 0.5f - BorderCushion;
    private const float CenterToNeighbour = 0.5f + BorderCushion;

    public ref Node NodeByIndex(int index) => ref _nodes[index];
    public WPos CellCenter(int index) => _map.GridToWorld(index % _map.Width, index / _map.Width, 0.5f, 0.5f);

    // gMultiplier is typically inverse speed, which turns g-values into time
    public void Start(Map map, WPos startPos, WPos goalPos, float goalRadius, float gMultiplier)
    {
        _map = map;
        //_goalFrac = map.WorldToGridFrac(goalPos);
        //_goalRadius = goalRadius;
        var numPixels = map.Width * map.Height;
        if (_nodes.Length < numPixels)
            _nodes = new Node[numPixels];
        else
            Array.Fill(_nodes, default, 0, numPixels);
        _openList.Clear();
        _deltaGSide = map.Resolution * gMultiplier;

        PrefillH();

        var startFrac = map.WorldToGridFrac(startPos);
        var start = map.ClampToGrid(map.FracToGrid(startFrac));
        _startNodeIndex = _bestIndex = _fallbackIndex = _map.GridToIndex(start.x, start.y);
        _startMaxG = _map.PixelMaxG[_startNodeIndex];
        _startScore = CalculateScore(_startMaxG, _startMaxG, _startMaxG, _startNodeIndex);
        NumSteps = NumReopens = 0;

        startFrac.X -= start.x + 0.5f;
        startFrac.Y -= start.y + 0.5f;
        ref var startNode = ref _nodes[_startNodeIndex];
        startNode = new()
        {
            GScore = 0,
            HScore = startNode.HScore, //HeuristicDistance(start.x, start.y),
            ParentIndex = _startNodeIndex, // start's parent is self
            PathLeeway = _startMaxG,
            PathMinG = _startMaxG,
            Score = _startScore,
            EnterOffset = startFrac,
        };
        AddToOpen(_startNodeIndex);
    }

    // returns whether search is to be terminated; on success, first node of the open list would contain found goal
    public bool ExecuteStep()
    {
        if (_openList.Count == 0 /*|| _nodes[_openList[0]].HScore <= 0*/)
            return false;

        ++NumSteps;
        int nextNodeIndex = PopMinOpen();
        var (nextNodeX, nextNodeY) = _map.IndexToGrid(nextNodeIndex);
        ref var nextNode = ref _nodes[nextNodeIndex];

        // update our best indices
        if (CompareNodeScores(ref nextNode, ref _nodes[_bestIndex]) < 0)
            _bestIndex = nextNodeIndex;
        if (nextNode.Score == Score.UltimatelySafe && (_fallbackIndex == _startNodeIndex || CompareNodeScores(ref nextNode, ref _nodes[_fallbackIndex]) < 0))
            _fallbackIndex = nextNodeIndex;

        if (nextNodeY > _map.MinY)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY - 1, nextNodeIndex - _map.Width, CenterToNeighbour + nextNode.EnterOffset.Y);
        if (nextNodeX > _map.MinX)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX - 1, nextNodeY, nextNodeIndex - 1, CenterToNeighbour + nextNode.EnterOffset.X);
        if (nextNodeX < _map.MaxX)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX + 1, nextNodeY, nextNodeIndex + 1, CenterToNeighbour - nextNode.EnterOffset.X);
        if (nextNodeY < _map.MaxY)
            VisitNeighbour(nextNodeX, nextNodeY, nextNodeIndex, nextNodeX, nextNodeY + 1, nextNodeIndex + _map.Width, CenterToNeighbour - nextNode.EnterOffset.Y);
        return true;
    }

    public int Execute()
    {
        while (_nodes[_bestIndex].HScore > 0 && _fallbackIndex == _startNodeIndex && ExecuteStep())
            ;
        return BestIndex();
    }

    public int BestIndex()
    {
        if (_nodes[_bestIndex].Score > _startScore)
            return _bestIndex; // we've found something better than start

        if (_fallbackIndex != _startNodeIndex)
        {
            // find first parent of best-among-worst that is at least as good as start
            var destIndex = _fallbackIndex;
            var parentIndex = _nodes[destIndex].ParentIndex;
            while (_nodes[parentIndex].Score < _startScore)
            {
                destIndex = parentIndex;
                parentIndex = _nodes[destIndex].ParentIndex;
            }

            // TODO: this is very similar to LineOfSight, try to unify implementations...
            ref var startNode = ref _nodes[parentIndex];
            ref var destNode = ref _nodes[destIndex];
            var (x2, y2) = _map.IndexToGrid(destIndex);
            var (x1, y1) = _map.IndexToGrid(parentIndex);
            int dx = x2 - x1;
            int dy = y2 - y1;
            int sx = dx > 0 ? 1 : -1;
            int sy = dy > 0 ? 1 : -1;
            var hsx = 0.5f * sx;
            var hsy = 0.5f * sy;
            var indexDeltaX = sx;
            var indexDeltaY = sy * _map.Width;

            var ab = new Vector2(dx + destNode.EnterOffset.X - startNode.EnterOffset.X, dy + destNode.EnterOffset.Y - startNode.EnterOffset.Y);
            ab /= ab.Length();
            var invx = ab.X != 0 ? 1 / ab.X : float.MaxValue; // either can be infinite, but not both; we want to avoid actual infinities here, because 0*inf = NaN (and we'd rather have it be 0 in this case)
            var invy = ab.Y != 0 ? 1 / ab.Y : float.MaxValue;
            var off1 = startNode.EnterOffset;
            while (x1 != x2 || y1 != y2)
            {
                var tx = (hsx - off1.X) * invx; // if negative, we'll never intersect it
                var ty = (hsy - off1.Y) * invy;
                if (tx < 0 || x1 == x2)
                    tx = float.MaxValue;
                if (ty < 0 || y1 == y2)
                    ty = float.MaxValue;

                var nextIndex = parentIndex;
                if (tx < ty)
                {
                    x1 += sx;
                    off1.X = -hsx;
                    off1.Y = Math.Clamp(off1.Y + tx * ab.Y, -0.5f, +0.5f);
                    nextIndex += indexDeltaX;
                }
                else
                {
                    y1 += sy;
                    off1.Y = -hsy;
                    off1.X = Math.Clamp(off1.X + ty * ab.X, -0.5f, +0.5f);
                    nextIndex += indexDeltaY;
                }

                if (_nodes[nextIndex].Score < _startScore)
                {
                    return parentIndex;
                }
                parentIndex = nextIndex;
            }
        }

        return _bestIndex;
    }

    public Score CalculateScore(float pixMaxG, float pathMinG, float pathLeeway, int pixelIndex)
    {
        var destSafe = pixMaxG == float.MaxValue;
        var pathSafe = pathLeeway > 0;
        var destBetter = pixMaxG > _startMaxG;
        if (destSafe && pathSafe)
            return _map.PixelPriority[pixelIndex] == _map.MaxPriority ? Score.SafeMaxPrio : Score.Safe;

        if (pathMinG == _startMaxG) // TODO: some small threshold? should be solved by preprocessing...
            return pathSafe
                ? (destBetter ? Score.SemiSafeImprove : Score.SemiSafeAsStart) // note: if pix.MaxG is < _startMaxG, then PathMinG will be < too
                : (destBetter ? Score.UnsafeImprove : Score.UnsafeAsStart);

        return destSafe ? Score.UltimatelySafe : destBetter ? Score.UltimatelyBetter : Score.JustBad;
    }

    // return a 'score' difference: 0 if identical, -1 if left is somewhat better, -2 if left is significantly better, +1/+2 when right is better
    public int CompareNodeScores(ref Node nodeL, ref Node nodeR)
    {
        if (nodeL.Score != nodeR.Score)
            return nodeL.Score > nodeR.Score ? -2 : +2;

        // TODO: should we use leeway here or distance?..
        //return nodeL.PathLeeway > nodeR.PathLeeway;
        var gl = nodeL.GScore;
        var gr = nodeR.GScore;
        var fl = gl + nodeL.HScore;
        var fr = gr + nodeR.HScore;
        if (fl + 0.00001f < fr)
            return -1;
        else if (fr + 0.00001f < fl)
            return +1;
        else if (gl != gr)
            return gl > gr ? -1 : 1; // tie-break towards larger g-values
        else
            return 0;
    }

    public Vector2 CalculateEnterOffset(int fromX, int fromY, Vector2 fromOff, int toX, int toY)
    {
        var x = fromX == toX ? fromOff.X : fromX < toX ? -MaxNeighbourOffset : +MaxNeighbourOffset;
        var y = fromY == toY ? fromOff.Y : fromY < toY ? -MaxNeighbourOffset : +MaxNeighbourOffset;
        return new(x, y);
    }

    public float LineOfSight(int x1, int y1, Vector2 off1, int x2, int y2, out Vector2 off2, out float length, out float minG)
    {
        var curNodeIndex = _map.GridToIndex(x1, y1);
        ref var startNode = ref _nodes[curNodeIndex];
        float minLeeway = startNode.PathLeeway;
        minG = startNode.PathMinG;

        int dx = x2 - x1;
        int dy = y2 - y1;
        int sx = dx > 0 ? 1 : -1;
        int sy = dy > 0 ? 1 : -1;
        var hsx = 0.5f * sx;
        var hsy = 0.5f * sy;
        var indexDeltaX = sx;
        var indexDeltaY = sy * _map.Width;

        off2 = CalculateEnterOffset(x1, y1, off1, x2, y2);
        var abx = dx + off2.X - off1.X;
        var aby = dy + off2.Y - off1.Y;
        length = MathF.Sqrt(abx * abx + aby * aby);
        if (length < 0.01f)
            return minLeeway; // zero length would create NaN's

        abx /= length; // note that ab.X == 0 does not imply dx == 0 (could be crossing the border) or vice versa (could have a small movement along axis in any direction without crossing cell boundary)
        aby /= length;
        var invx = abx != 0 ? 1 / abx : float.MaxValue; // either can be infinite, but not both; we want to avoid actual infinities here, because 0*inf = NaN (and we'd rather have it be 0 in this case)
        var invy = aby != 0 ? 1 / aby : float.MaxValue;

        var curG = startNode.GScore;
        var prevPixMaxG = _map.PixelMaxG[curNodeIndex];
        var numIterationsLeft = dx * sx + dy * sy; // on every iteration we move 1 closer to (x2, y2)
        var curOffX = off1.X;
        var curOffY = off1.Y;
        while (numIterationsLeft-- > 0)
        {
            // TODO: consider somehow removing conditionals here...
            var tx = x1 != x2 ? (hsx - curOffX) * invx : float.MaxValue; // if negative, we'll never intersect it
            var ty = y1 != y2 ? (hsy - curOffY) * invy : float.MaxValue;
            if (tx < 0)
                tx = float.MaxValue;
            if (ty < 0)
                ty = float.MaxValue;

            // note: we need the clamp to handle corners properly
            if (tx < ty)
            {
                x1 += sx;
                curOffX = -hsx;
                curOffY = Math.Clamp(curOffY + tx * aby, -0.5f, +0.5f);
                curG += tx * _deltaGSide;
                curNodeIndex += indexDeltaX;
            }
            else
            {
                y1 += sy;
                curOffY = -hsy;
                curOffX = Math.Clamp(curOffX + ty * abx, -0.5f, +0.5f);
                curG += ty * _deltaGSide;
                curNodeIndex += indexDeltaY;
            }

            var pixG = _map.PixelMaxG[curNodeIndex];
            minLeeway = Math.Min(minLeeway, Math.Min(pixG, prevPixMaxG) - curG);
            minG = Math.Min(minG, pixG);
            prevPixMaxG = pixG;
        }
        return minLeeway;
    }

    private void VisitNeighbour(int parentX, int parentY, int parentIndex, int nodeX, int nodeY, int nodeIndex, float deltaGrid)
    {
        ref var destNode = ref _nodes[nodeIndex];
        if (destNode.OpenHeapIndex < 0 && destNode.Score >= Score.SemiSafeAsStart)
            return; // in closed list already, and the previous path was decent - TODO: is it possible to visit again with lower cost?..

        //if (destNode.OpenHeapIndex == 0)
        //    destNode.HScore = HeuristicDistance(nodeX, nodeY); // if this is the first time we visit this node, initialize h-score, we're going to add it to open list

        // note: we may visit the node even if it's blocked (eg we might be moving outside imminent aoe)
        var destPixG = _map.PixelMaxG[nodeIndex];
        var deltaG = _deltaGSide * deltaGrid;
        var destGScore = _nodes[parentIndex].GScore + deltaG;
        var destLeeway = MathF.Min(_nodes[parentIndex].PathLeeway, Math.Min(destPixG, _map.PixelMaxG[parentIndex]) - destGScore);
        var destMinG = MathF.Min(_nodes[parentIndex].PathMinG, destPixG);
        var altNode = new Node()
        {
            GScore = destGScore,
            HScore = destNode.HScore,
            ParentIndex = parentIndex,
            OpenHeapIndex = destNode.OpenHeapIndex,
            PathLeeway = destLeeway,
            PathMinG = destMinG,
            Score = CalculateScore(destPixG, destMinG, destLeeway, nodeIndex),
            EnterOffset = CalculateEnterOffset(parentX, parentY, _nodes[parentIndex].EnterOffset, nodeX, nodeY),
        };
        //if (_nodes[parentIndex].PathLeeway > 0 && nodeLeeway <= 0)
        //    return; // don't try to enter danger from safety

        // check LoS from grandparent
        int grandParentIndex = _nodes[parentIndex].ParentIndex;
        if (_nodes[grandParentIndex].PathMinG >= _nodes[parentIndex].PathMinG)
        {
            var (grandParentX, grandParentY) = _map.IndexToGrid(grandParentIndex);
            var losLeeway = LineOfSight(grandParentX, grandParentY, _nodes[grandParentIndex].EnterOffset, nodeX, nodeY, out var grandParentOffset, out var grandParentDist, out var losMinG);
            var losScore = CalculateScore(destPixG, losMinG, losLeeway, nodeIndex);
            // accept direct route either if score is better, score is same and leeway is better, or score is safe and leeway is good enough
            if (losScore > altNode.Score || losScore == altNode.Score && losLeeway >= (losScore >= Score.Safe ? 0 : altNode.PathLeeway))
            {
                parentIndex = grandParentIndex;
                altNode.GScore = _nodes[parentIndex].GScore + _deltaGSide * grandParentDist;
                altNode.ParentIndex = grandParentIndex;
                altNode.PathLeeway = losLeeway;
                altNode.PathMinG = losMinG;
                altNode.Score = losScore;
                altNode.EnterOffset = grandParentOffset;
            }
        }

        //if (destNode.OpenHeapIndex < 0 && destNode.PathLeeway >= nodeLeeway)
        //    return; // node was already visited before, old path was bad, but new path is even worse
        //if (destNode.OpenHeapIndex > 0 && destNode.PathLeeway > 0 && nodeLeeway <= 0)
        //    return; // node is already in scheduled for visit with a reasonable path, and new path is bad (even if it's shorter)

        // - always visit nodes that were never visited before
        // - revisit nodes only if new path is much better
        // - update nodes scheduled for visit if new path is even somewhat better
        var visit = destNode.OpenHeapIndex == 0 || CompareNodeScores(ref altNode, ref destNode) < (destNode.OpenHeapIndex < 0 ? -1 : 0);
        if (visit)
        {
            if (destNode.OpenHeapIndex < 0)
                ++NumReopens;
            destNode = altNode;
            AddToOpen(nodeIndex);
        }
    }

    //private float HeuristicDistance(int x, int y) => Math.Max(0, (new Vector2(x + 0.5f, y + 0.5f) - _goalFrac).Length() * _deltaGSide - _goalRadius);

    private void PrefillH()
    {
        int iCell = 0;
        for (int y = 0; y < _map.Height; ++y)
        {
            for (int x = 0; x < _map.Width; ++x, ++iCell)
            {
                if (_map.PixelPriority[iCell] < _map.MaxPriority)
                {
                    ref var node = ref _nodes[iCell];
                    node.HScore = float.MaxValue;
                    if (x > 0)
                        UpdateHNeighbour(x, y, ref node, x - 1, y, iCell - 1);
                    if (y > 0)
                        UpdateHNeighbour(x, y, ref node, x, y - 1, iCell - _map.Width);
                }
                // else: leave unfilled (H=0, parent=uninit)
            }
        }
        --iCell;
        for (int y0 = _map.Height - 1, y = y0; y >= 0; --y)
        {
            for (int x0 = _map.Width - 1, x = x0; x >= 0; --x, --iCell)
            {
                if (_map.PixelPriority[iCell] < _map.MaxPriority)
                {
                    ref var node = ref _nodes[iCell];
                    if (x < x0)
                        UpdateHNeighbour(x, y, ref node, x + 1, y, iCell + 1);
                    if (y < y0)
                        UpdateHNeighbour(x, y, ref node, x, y + 1, iCell + _map.Width);
                }
            }
        }
    }

    private void UpdateHNeighbour(int x1, int y1, ref Node node, int x2, int y2, int neighIndex)
    {
        ref var neighbour = ref _nodes[neighIndex];
        if (neighbour.HScore == 0)
        {
            node.HScore = _deltaGSide; // don't bother with min, it can't be lower
            node.ParentIndex = neighIndex;
        }
        else if (neighbour.HScore < float.MaxValue)
        {
            (x2, y2) = _map.IndexToGrid(neighbour.ParentIndex);
            var dx = x2 - x1;
            var dy = y2 - y1;
            var hScore = _deltaGSide * MathF.Sqrt(dx * dx + dy * dy);
            if (hScore < node.HScore)
            {
                node.HScore = hScore;
                node.ParentIndex = neighbour.ParentIndex;
            }
        }
    }

    private void AddToOpen(int nodeIndex)
    {
        if (_nodes[nodeIndex].OpenHeapIndex <= 0)
        {
            _openList.Add(nodeIndex);
            _nodes[nodeIndex].OpenHeapIndex = _openList.Count;
        }
        // update location
        PercolateUp(_nodes[nodeIndex].OpenHeapIndex - 1);
    }

    // remove first (minimal) node from open heap and mark as closed
    private int PopMinOpen()
    {
        int nodeIndex = _openList[0];
        _openList[0] = _openList[^1];
        _nodes[nodeIndex].OpenHeapIndex = -1;
        _openList.RemoveAt(_openList.Count - 1);
        if (_openList.Count > 0)
        {
            _nodes[_openList[0]].OpenHeapIndex = 1;
            PercolateDown(0);
        }
        return nodeIndex;
    }

    private void PercolateUp(int heapIndex)
    {
        var openSpan = _openList.AsSpan();
        int nodeIndex = openSpan[heapIndex];
        ref var node = ref _nodes[nodeIndex];
        while (heapIndex > 0)
        {
            int parentHeapIndex = (heapIndex - 1) >> 1;
            ref int parentNodeIndex = ref openSpan[parentHeapIndex];
            ref var parent = ref _nodes[parentNodeIndex];
            if (CompareNodeScores(ref node, ref parent) >= 0)
                break; // parent is 'less' (same/better), stop

            openSpan[heapIndex] = parentNodeIndex;
            parent.OpenHeapIndex = heapIndex + 1;
            heapIndex = parentHeapIndex;
        }
        openSpan[heapIndex] = nodeIndex;
        node.OpenHeapIndex = heapIndex + 1;
    }

    private void PercolateDown(int heapIndex)
    {
        var openSpan = _openList.AsSpan();
        int nodeIndex = openSpan[heapIndex];
        ref var node = ref _nodes[nodeIndex];

        int maxSize = openSpan.Length;
        while (true)
        {
            // find 'better' child
            int childHeapIndex = (heapIndex << 1) + 1;
            if (childHeapIndex >= maxSize)
                break; // node is already a leaf

            int childNodeIndex = openSpan[childHeapIndex];
            ref var child = ref _nodes[childNodeIndex];
            int altChildHeapIndex = childHeapIndex + 1;
            if (altChildHeapIndex < maxSize)
            {
                int altChildNodeIndex = openSpan[altChildHeapIndex];
                ref var altChild = ref _nodes[altChildNodeIndex];
                if (CompareNodeScores(ref altChild, ref child) < 0)
                {
                    childHeapIndex = altChildHeapIndex;
                    childNodeIndex = altChildNodeIndex;
                    child = ref altChild;
                }
            }

            if (CompareNodeScores(ref node, ref child) < 0)
                break; // node is better than best child, so should remain on top

            openSpan[heapIndex] = childNodeIndex;
            child.OpenHeapIndex = heapIndex + 1;
            heapIndex = childHeapIndex;
        }
        openSpan[heapIndex] = nodeIndex;
        node.OpenHeapIndex = heapIndex + 1;
    }
}
