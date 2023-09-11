using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace BossMod
{
    [StructLayout(LayoutKind.Explicit, Size = 0x30)]
    public unsafe struct Mat4x3
    {
        [FieldOffset(0x00)] public Vector3 Row0;
        [FieldOffset(0x0C)] public Vector3 Row1;
        [FieldOffset(0x18)] public Vector3 Row2;
        [FieldOffset(0x24)] public Vector3 Row3;

        public SharpDX.Matrix M => new(Row0.X, Row0.Y, Row0.Z, 0, Row1.X, Row1.Y, Row1.Z, 0, Row2.X, Row2.Y, Row2.Z, 0, Row3.X, Row3.Y, Row3.Z, 1);
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public unsafe struct CollisionNode
    {
        [FieldOffset(0x00)] public void** Vtbl;
        [FieldOffset(0x08)] public void** Vtbl8;
        [FieldOffset(0x10)] public CollisionNode* Prev;
        [FieldOffset(0x18)] public CollisionNode* Next;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0xC0)]
    public unsafe struct CollisionModule
    {
        [FieldOffset(0x10)] public CollisionSceneManager* Manager;
        [FieldOffset(0xA8)] public int LoadInProgressCounter;
        [FieldOffset(0xAC)] public Vector4 ForcedStreamingBounds;

        public static CollisionModule* Instance => (CollisionModule*)Framework.Instance()->BGCollisionModule;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x38)]
    public unsafe struct CollisionSceneManager
    {
        [FieldOffset(0x00)] public void** Vtbl;
        [FieldOffset(0x18)] public CollisionSceneWrapper* FirstScene;
        [FieldOffset(0x20)] public int NumScenes;
        [FieldOffset(0x28)] public Vector4 StreamingBounds;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x30)]
    public unsafe struct CollisionSceneWrapper
    {
        [FieldOffset(0x00)] public CollisionNode Base;
        [FieldOffset(0x20)] public CollisionSceneManager* Owner;
        [FieldOffset(0x28)] public CollisionScene* Scene;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x40)]
    public unsafe struct CollisionScene
    {
        [FieldOffset(0x00)] public void** Vtbl;
        [FieldOffset(0x08)] public CollisionSceneManager* Owner;
        [FieldOffset(0x10)] public CollisionObjectBase* FirstObj;
        [FieldOffset(0x18)] public int NumObjs;
        [FieldOffset(0x20)] public Vector4 StreamingBounds; // center = player pos, w = radius
        [FieldOffset(0x30)] public int NumLoading;
        [FieldOffset(0x38)] public CollisionQuadtree* Quadtree;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x40)]
    public unsafe struct CollisionQuadtree
    {
        [FieldOffset(0x00)] public void** Vtbl;
        [FieldOffset(0x08)] public float MinX;
        [FieldOffset(0x0C)] public float MaxX;
        [FieldOffset(0x10)] public float LeafSizeX;
        [FieldOffset(0x14)] public float MinZ;
        [FieldOffset(0x18)] public float MaxZ;
        [FieldOffset(0x1C)] public float LeafSizeZ;
        [FieldOffset(0x20)] public int NumLevels;
        [FieldOffset(0x28)] public CollisionNode* Nodes;
        [FieldOffset(0x30)] public int NumNodes;
        [FieldOffset(0x38)] public void* Owner;
    }

    public enum CollisionObjectType : int
    {
        Multi = 1,
        Shape = 2,
        Box = 3,
        Cylinder = 4,
        Sphere = 5,
        Plane = 6,
        PlaneEx = 7,
    }

    [StructLayout(LayoutKind.Explicit, Size = 0xA0)]
    public unsafe struct CollisionObjectBase
    {
        [FieldOffset(0x00)] public CollisionObjectBaseVTable* Vtbl;
        [FieldOffset(0x00)] public CollisionNode Base;
        [FieldOffset(0x30)] public CollisionObjectBase* PrevNodeObj;
        [FieldOffset(0x38)] public CollisionObjectBase* NextNodeObj;
        [FieldOffset(0x44)] public uint NumRefs;
        [FieldOffset(0x48)] public CollisionScene* Scene;
        [FieldOffset(0x68)] public uint LayerMask;
    }

    [StructLayout(LayoutKind.Explicit, Size = 24*8)]
    public unsafe struct CollisionObjectBaseVTable
    {
        [FieldOffset(17 * 8)] public delegate* unmanaged[Stdcall]<CollisionObjectBase*, CollisionObjectType> GetObjectType;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public unsafe struct CollisionObjectMultiElement
    {
        [FieldOffset(0x00)] public int SubFile;
        [FieldOffset(0x08)] public CollisionObjectShape* Shape;
        [FieldOffset(0x10)] public float MinX;
        [FieldOffset(0x14)] public float MinZ;
        [FieldOffset(0x18)] public float MaxX;
        [FieldOffset(0x1C)] public float MaxZ;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x1E0)]
    public unsafe struct CollisionObjectMulti // type 1
    {
        [FieldOffset(0x000)] public CollisionObjectBase Base;
        [FieldOffset(0x0A8)] public fixed byte PathBase[256];
        [FieldOffset(0x1B8)] public float StreamedMinX;
        [FieldOffset(0x1BC)] public float StreamedMinZ;
        [FieldOffset(0x1C0)] public float StreamedMaxX;
        [FieldOffset(0x1C4)] public float StreamedMaxZ;
        [FieldOffset(0x1C8)] public int* PtrNumElements;
        [FieldOffset(0x1D8)] public CollisionObjectMultiElement* Elements;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x198)]
    public unsafe struct CollisionObjectShape // type 2
    {
        [FieldOffset(0x000)] public CollisionObjectBase Base;
        [FieldOffset(0x0C8)] public CollisionShapePCB* Shape; // pointer to interface really
        [FieldOffset(0x0D4)] public Vector3 Translation;
        [FieldOffset(0x0E0)] public Vector3 Rotation;
        [FieldOffset(0x0EC)] public Vector3 Scale;
        [FieldOffset(0x0F8)] public Vector3 TranslationPrev;
        [FieldOffset(0x104)] public Vector3 RotationPrev;
        [FieldOffset(0x110)] public Mat4x3 World;
        [FieldOffset(0x140)] public Mat4x3 InvWorld;
        [FieldOffset(0x170)] public Vector4 BoundingSphere;
        [FieldOffset(0x180)] public Vector3 BoundingBoxMin;
        [FieldOffset(0x18C)] public Vector3 BoundingBoxMax;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x140)]
    public unsafe struct CollisionObjectBox // type 3
    {
        [FieldOffset(0x000)] public CollisionObjectBase Base;
        [FieldOffset(0x0A0)] public Vector3 Translation;
        [FieldOffset(0x0AC)] public Vector3 TranslationPrev;
        [FieldOffset(0x0B8)] public Vector3 Rotation;
        [FieldOffset(0x0C4)] public Vector3 RotationPrev;
        [FieldOffset(0x0D0)] public Vector3 Scale;
        [FieldOffset(0x0DC)] public Mat4x3 World;
        [FieldOffset(0x10C)] public Mat4x3 InvWorld;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x148)]
    public unsafe struct CollisionObjectCylinder // type 4
    {
        [FieldOffset(0x000)] public CollisionObjectBase Base;
        [FieldOffset(0x0A0)] public Vector3 Translation;
        [FieldOffset(0x0AC)] public Vector3 TranslationPrev;
        [FieldOffset(0x0B8)] public Vector3 Rotation;
        [FieldOffset(0x0C4)] public Vector3 RotationPrev;
        [FieldOffset(0x0D0)] public Vector3 Scale;
        [FieldOffset(0x0DC)] public float Radius;
        [FieldOffset(0x0E0)] public Mat4x3 World;
        [FieldOffset(0x110)] public Mat4x3 InvWorld;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x150)]
    public unsafe struct CollisionObjectSphere // type 5
    {
        [FieldOffset(0x000)] public CollisionObjectBase Base;
        [FieldOffset(0x0A4)] public Vector3 Translation;
        [FieldOffset(0x0B0)] public Vector3 TranslationPrev;
        [FieldOffset(0x0BC)] public Vector3 Rotation;
        [FieldOffset(0x0C8)] public Vector3 RotationPrev;
        [FieldOffset(0x0D4)] public Vector3 Scale;
        [FieldOffset(0x0E0)] public Vector3 ScalePrev;
        [FieldOffset(0x0EC)] public Mat4x3 World;
        [FieldOffset(0x11C)] public Mat4x3 InvWorld;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x140)]
    public unsafe struct CollisionObjectPlane // type 6/7
    {
        [FieldOffset(0x000)] public CollisionObjectBase Base;
        [FieldOffset(0x0A0)] public Vector3 Translation;
        [FieldOffset(0x0AC)] public Vector3 TranslationPrev;
        [FieldOffset(0x0B8)] public Vector3 Rotation;
        [FieldOffset(0x0C4)] public Vector3 RotationPrev;
        [FieldOffset(0x0D0)] public Vector3 Scale;
        [FieldOffset(0x0DC)] public Mat4x3 World;
        [FieldOffset(0x10C)] public Mat4x3 InvWorld;
        [FieldOffset(0x13D)] public byte Extended;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x40)]
    public unsafe struct CollisionShapePCB
    {
        [FieldOffset(0x00)] public void** Vtbl;
        [FieldOffset(0x08)] public void** Vtbl8;
        [FieldOffset(0x10)] public CollisionObjectShape* OwnerObj;
        [FieldOffset(0x18)] public CollisionShapePCBData* Data;
    };

    [StructLayout(LayoutKind.Explicit, Size = 0x30)] // variable length structure: followed by raw verts then compressed verts then prims
    public unsafe struct CollisionShapePCBData
    {
        [FieldOffset(0x00)] public ulong Header;
        [FieldOffset(0x08)] public int Child1Offset;
        [FieldOffset(0x0C)] public int Child2Offset;
        [FieldOffset(0x10)] public Vector3 AABBMin;
        [FieldOffset(0x1C)] public Vector3 AABBMax;
        [FieldOffset(0x28)] public ushort NumVertsCompressed; // ushort[3] per vert
        [FieldOffset(0x2A)] public ushort NumPrims;
        [FieldOffset(0x2C)] public ushort NumVertsRaw; // vector3 per vert
    };

    [StructLayout(LayoutKind.Explicit, Size = 0xC)]
    public unsafe struct CollisionShapePrimitive
    {
        [FieldOffset(0x0)] public byte V1;
        [FieldOffset(0x1)] public byte V2;
        [FieldOffset(0x2)] public byte V3;
        [FieldOffset(0x4)] public uint Flags;
        [FieldOffset(0x8)] public uint Unk8;
    }

    public unsafe class DebugCollision : IDisposable
    {
        private UITree _tree = new();
        private (Action, nint) _drawExtra;
        private HashSet<nint> _slaveShapes = new();

        private nint _typeinfoCollisionShapePCB;

        public DebugCollision()
        {
            _typeinfoCollisionShapePCB = Service.SigScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 48 89 78 10 48 89 08");
            Service.Log($"vtbl CollisionShapePCB: {_typeinfoCollisionShapePCB:X}");

            _drawExtra = (() => { }, 0);
        }

        public void Dispose()
        {

        }

        public void Draw()
        {
            UpdateSlaveShapes();

            if (ImGui.Button("Clear selection"))
                _drawExtra = (() => { }, 0);
            ImGui.SameLine();
            if (ImGui.Button("Export to obj"))
                ExportToObj(true, true);
            ImGui.SameLine();
            if (ImGui.Button($"Generate report"))
                Report();

            //var screenPos = ImGui.GetMousePos();
            //var ray = CameraManager.Instance()->CurrentCamera->ScreenPointToRay(screenPos);
            //BGCollisionModule.Raycast(ray.Origin, ray.Direction, out var hitInfo);
            //ImGui.TextUnformatted($"S2W: {screenPos} -> {hitInfo.Point}");
            //for (int i = 0; i < 0x58; i += 8)
            //    ImGui.TextUnformatted($"{i:X} = {Utils.ReadField<nint>(&hitInfo, i):X16}");

            var module = CollisionModule.Instance;
            ImGui.TextUnformatted($"Module: {(nint)module:X}->{(nint)module->Manager:X} ({module->Manager->NumScenes} scenes, {module->LoadInProgressCounter} loads)");
            ImGui.TextUnformatted($"Streaming: {SphereStr(module->ForcedStreamingBounds)} / {SphereStr(module->Manager->StreamingBounds)}");

            var scene = module->Manager->FirstScene;
            while (scene != null)
            {
                DrawScene(scene);
                scene = (CollisionSceneWrapper*)scene->Base.Next;
            }

            _drawExtra.Item1();
        }

        private void UpdateSlaveShapes()
        {
            bool foundCtx = _drawExtra.Item2 == 0;

            _slaveShapes.Clear();
            var scene = CollisionModule.Instance->Manager->FirstScene;
            while (scene != null)
            {
                var obj = scene->Scene->FirstObj;
                while (obj != null)
                {
                    foundCtx |= (nint)obj == _drawExtra.Item2;
                    if (obj->Vtbl->GetObjectType(obj) == CollisionObjectType.Multi)
                    {
                        var castObj = (CollisionObjectMulti*)obj;
                        if (castObj->Elements != null)
                        {
                            for (int i = 0; i < *castObj->PtrNumElements; ++i)
                                _slaveShapes.Add((nint)castObj->Elements[i].Shape);
                        }
                    }
                    obj = (CollisionObjectBase*)obj->Base.Next;
                }
                scene = (CollisionSceneWrapper*)scene->Base.Next;
            }

            if (!foundCtx)
            {
                Service.Log($"resetting selection for {_drawExtra.Item2:X}");
                _drawExtra = (() => { }, 0);
            }
        }

        private void DrawScene(CollisionSceneWrapper* wrapper)
        {
            foreach (var n in _tree.Node($"{(nint)wrapper:X}->{(nint)wrapper->Scene:X} ({wrapper->Scene->NumObjs} objects, {wrapper->Scene->NumLoading} loads)###{(nint)wrapper:X}", select: MakeSelect(() => VisualizeScene(wrapper->Scene), null)))
            {
                _tree.LeafNode($"Streaming bounds: [{SphereStr(wrapper->Scene->StreamingBounds)}");

                foreach (var n2 in _tree.Node($"Objects"))
                {
                    var obj = wrapper->Scene->FirstObj;
                    while (obj != null)
                    {
                        DrawObject(obj);
                        obj = (CollisionObjectBase*)obj->Base.Next;
                    }
                }

                var tree = wrapper->Scene->Quadtree;
                foreach (var n2 in _tree.Node($"Quadtree {(nint)tree:X}: {tree->NumLevels} levels ([{tree->MinX}, {tree->MaxX}]x[{tree->MinZ}, {tree->MaxZ}], leaf {tree->LeafSizeX}x{tree->LeafSizeZ}), {tree->NumNodes} nodes", tree->NumNodes == 0))
                {
                    for (int i = 0; i < tree->NumNodes; ++i)
                    {
                        var node = tree->Nodes + i;
                        foreach (var n3 in _tree.Node($"{i}", node->Next == null, select: MakeSelect(() => VisualizeQuadtreeNode(node), null)))
                        {
                            var child = (CollisionObjectBase*)node->Next;
                            while (child != null && child != node)
                            {
                                DrawObject(child);
                                child = child->NextNodeObj;
                            }
                        }
                    }
                }
            }
        }

        private void DrawObject(CollisionObjectBase* obj)
        {
            var type = obj->Vtbl->GetObjectType(obj);
            foreach (var n3 in _tree.Node($"{type} {(nint)obj:X}, layers={obj->LayerMask:X8}, refs={obj->NumRefs}", color: _slaveShapes.Contains((nint)obj) ? ArenaColor.Safe : 0xffffffff, select: MakeSelect(() => VisualizeObject(obj), obj)))
            {
                switch (type)
                {
                    case CollisionObjectType.Multi:
                        {
                            var castObj = (CollisionObjectMulti*)obj;
                            _tree.LeafNode($"Path: {MemoryHelper.ReadStringNullTerminated((nint)castObj->PathBase)}");
                            _tree.LeafNode($"Streamed: [{castObj->StreamedMinX:f3}x{castObj->StreamedMinZ:f3}] - [{castObj->StreamedMaxX:f3}x{castObj->StreamedMaxZ:f3}]");
                            if (castObj->Elements != null)
                            {
                                foreach (var n4 in _tree.Node($"Elements: {*castObj->PtrNumElements}"))
                                {
                                    for (int i = 0; i < *castObj->PtrNumElements; ++i)
                                    {
                                        var elem = castObj->Elements + i;
                                        var elemBase = &elem->Shape->Base;
                                        foreach (var n5 in _tree.Node($"#{i}: {elem->SubFile:d4} [{elem->MinX:f3}x{elem->MinZ:f3}] - [{elem->MaxX:f3}x{elem->MaxZ:f3}] == {(nint)elem->Shape:X}", elem->Shape == null, select: MakeSelect(() => VisualizeObject(elemBase), elemBase)))
                                            if (elem->Shape != null)
                                                DrawObjectShape(elem->Shape);
                                    }
                                }
                            }
                        }
                        break;
                    case CollisionObjectType.Shape:
                        DrawObjectShape((CollisionObjectShape*)obj);
                        break;
                    case CollisionObjectType.Box:
                        {
                            var castObj = (CollisionObjectBox*)obj;
                            _tree.LeafNode($"Translation: {Utils.Vec3String(castObj->Translation)}");
                            _tree.LeafNode($"Rotation: {Utils.Vec3String(castObj->Rotation)}");
                            _tree.LeafNode($"Scale: {Utils.Vec3String(castObj->Scale)}");
                            DrawMat4x3("World", ref castObj->World);
                            DrawMat4x3("InvWorld", ref castObj->InvWorld);
                        }
                        break;
                    case CollisionObjectType.Cylinder:
                        {
                            var castObj = (CollisionObjectCylinder*)obj;
                            _tree.LeafNode($"Translation: {Utils.Vec3String(castObj->Translation)}");
                            _tree.LeafNode($"Rotation: {Utils.Vec3String(castObj->Rotation)}");
                            _tree.LeafNode($"Scale: {Utils.Vec3String(castObj->Scale)}");
                            _tree.LeafNode($"Radius: {castObj->Radius:f3}");
                            DrawMat4x3("World", ref castObj->World);
                            DrawMat4x3("InvWorld", ref castObj->InvWorld);
                        }
                        break;
                    case CollisionObjectType.Sphere:
                        {
                            var castObj = (CollisionObjectSphere*)obj;
                            _tree.LeafNode($"Translation: {Utils.Vec3String(castObj->Translation)}");
                            _tree.LeafNode($"Rotation: {Utils.Vec3String(castObj->Rotation)}");
                            _tree.LeafNode($"Scale: {Utils.Vec3String(castObj->Scale)}");
                            DrawMat4x3("World", ref castObj->World);
                            DrawMat4x3("InvWorld", ref castObj->InvWorld);
                        }
                        break;
                    case CollisionObjectType.Plane:
                    case CollisionObjectType.PlaneEx:
                        {
                            var castObj = (CollisionObjectPlane*)obj;
                            _tree.LeafNode($"Translation: {Utils.Vec3String(castObj->Translation)}");
                            _tree.LeafNode($"Rotation: {Utils.Vec3String(castObj->Rotation)}");
                            _tree.LeafNode($"Scale: {Utils.Vec3String(castObj->Scale)}");
                            DrawMat4x3("World", ref castObj->World);
                            DrawMat4x3("InvWorld", ref castObj->InvWorld);
                        }
                        break;
                }
            }
        }

        private void DrawObjectShape(CollisionObjectShape* obj)
        {
            _tree.LeafNode($"Translation: {Utils.Vec3String(obj->Translation)}");
            _tree.LeafNode($"Rotation: {Utils.Vec3String(obj->Rotation)}");
            _tree.LeafNode($"Scale: {Utils.Vec3String(obj->Scale)}");
            DrawMat4x3("World", ref obj->World);
            DrawMat4x3("InvWorld", ref obj->InvWorld);
            _tree.LeafNode($"Bounding sphere: [{SphereStr(obj->BoundingSphere)}", select: MakeSelect(() => VisualizeSphere(obj->BoundingSphere), &obj->Base));
            _tree.LeafNode($"Bounding box: {Utils.Vec3String(obj->BoundingBoxMin)} - {Utils.Vec3String(obj->BoundingBoxMax)}", select: MakeSelect(() => VisualizeAABB(obj->BoundingBoxMin, obj->BoundingBoxMax), &obj->Base));

            bool shapeHasData = obj->Shape != null && (nint)obj->Shape->Vtbl == _typeinfoCollisionShapePCB && obj->Shape->Data != null;
            var shapeType = obj->Shape == null ? "null" : (nint)obj->Shape->Vtbl == _typeinfoCollisionShapePCB ? "PCB" : $"unknown +{(nint)obj->Shape->Vtbl - Service.SigScanner.Module.BaseAddress:X}";
            foreach (var n in _tree.Node($"Shape: {(nint)obj->Shape:X} {shapeType}", !shapeHasData, select: MakeSelect(shapeHasData ? () => VisualizeShape(obj->Shape->Data, obj) : () => { }, &obj->Base)))
            {
                if (shapeHasData)
                    DrawPCBShape(obj->Shape->Data, obj);
            }
        }

        private void DrawPCBShape(CollisionShapePCBData* data, CollisionObjectShape* obj)
        {
            if (data == null)
                return;
            _tree.LeafNode($"Header: {data->Header:X16}");
            _tree.LeafNode($"AABB: {Utils.Vec3String(data->AABBMin)} - {Utils.Vec3String(data->AABBMax)}", select: MakeSelect(() => VisualizeOBB(data->AABBMin, data->AABBMax, obj), &obj->Base));
            foreach (var n in _tree.Node($"Vertices: {data->NumVertsRaw}+{data->NumVertsCompressed}", data->NumVertsRaw + data->NumVertsCompressed == 0))
            {
                var pRaw = (float*)(data + 1);
                for (int i = 0; i < data->NumVertsRaw; ++i)
                {
                    var v = new Vector3(pRaw[0], pRaw[1], pRaw[2]);
                    _tree.LeafNode($"[{i}] (r): {Utils.Vec3String(v)}", select: MakeSelect(() => VisualizeVertex(v, obj), &obj->Base));
                    pRaw += 3;
                }
                var pCompressed = (ushort*)pRaw;
                var quantScale = (data->AABBMax - data->AABBMin) / 65535.0f;
                for (int i = 0; i < data->NumVertsCompressed; ++i)
                {
                    var v = data->AABBMin + quantScale * new Vector3(pCompressed[0], pCompressed[1], pCompressed[2]);
                    _tree.LeafNode($"[{i + data->NumVertsRaw}] (c): {Utils.Vec3String(v)}", select: MakeSelect(() => VisualizeVertex(v, obj), &obj->Base));
                    pCompressed += 3;
                }
            }
            foreach (var n in _tree.Node($"Primitives: {data->NumPrims}", data->NumPrims == 0))
            {
                var pRaw = (float*)(data + 1);
                var pCompr = (ushort*)(pRaw + 3 * data->NumVertsRaw);
                var pPrims = (CollisionShapePrimitive*)(pCompr + 3 * data->NumVertsCompressed);
                for (int i = 0; i < data->NumPrims; ++i)
                {
                    var idx = i;
                    _tree.LeafNode($"[{i}]: {pPrims->V1}x{pPrims->V2}x{pPrims->V3}, {pPrims->Flags:X8}, {pPrims->Unk8:X8}", select: MakeSelect(() => VisualizePrimitive(data, idx, obj), &obj->Base));
                    ++pPrims;
                }
            }
            foreach (var n in _tree.Node($"Child 1 (+{data->Child1Offset})", data->Child1Offset == 0, select: MakeSelect(data->Child1Offset != 0 ? () => VisualizeShape((CollisionShapePCBData*)((byte*)data + data->Child1Offset), obj) : () => { }, &obj->Base)))
            {
                if (data->Child1Offset != 0)
                    DrawPCBShape((CollisionShapePCBData*)((byte*)data + data->Child1Offset), obj);
            }
            foreach (var n in _tree.Node($"Child 2 (+{data->Child2Offset})", data->Child2Offset == 0, select: MakeSelect(data->Child2Offset != 0 ? () => VisualizeShape((CollisionShapePCBData*)((byte*)data + data->Child2Offset), obj) : () => { }, &obj->Base)))
            {
                if (data->Child2Offset != 0)
                    DrawPCBShape((CollisionShapePCBData*)((byte*)data + data->Child2Offset), obj);
            }
        }

        private void DrawMat4x3(string tag, ref Mat4x3 mat)
        {
            _tree.LeafNode($"{tag} R0: {Utils.Vec3String(mat.Row0)}");
            _tree.LeafNode($"{tag} R1: {Utils.Vec3String(mat.Row1)}");
            _tree.LeafNode($"{tag} R2: {Utils.Vec3String(mat.Row2)}");
            _tree.LeafNode($"{tag} R3: {Utils.Vec3String(mat.Row3)}");
        }

        private Action MakeSelect(Action sel, CollisionObjectBase* ctx) => () =>
        {
            Service.Log($"select: {(nint)ctx:X}");
            _drawExtra = (sel, (nint)ctx);
        };

        private void VisualizeSphere(Vector4 sphere) => Camera.Instance?.DrawWorldSphere(new(sphere.X, sphere.Y, sphere.Z), sphere.W, ArenaColor.Safe);
        private void VisualizeAABB(Vector3 min, Vector3 max) => Camera.Instance?.DrawWorldOBB(min, max, SharpDX.Matrix.Identity, ArenaColor.Safe);
        private void VisualizeOBB(Vector3 min, Vector3 max, CollisionObjectShape* obj) => Camera.Instance?.DrawWorldOBB(min, max, obj->World.M, ArenaColor.Safe);
        private void VisualizeVertex(Vector3 v, CollisionObjectShape* obj) => Camera.Instance?.DrawWorldSphere(SharpDX.Vector3.TransformCoordinate(new(v.X, v.Y, v.Z), obj->World.M).ToSystem(), 0.1f, ArenaColor.Danger);

        private void VisualizePrimitive(CollisionShapePCBData* data, int iPrim, CollisionObjectShape* obj, uint color = ArenaColor.Danger)
        {
            var pRaw = (float*)(data + 1);
            var pCompr = (ushort*)(pRaw + 3 * data->NumVertsRaw);
            var pPrim = (CollisionShapePrimitive*)(pCompr + 3 * data->NumVertsCompressed);
            pPrim += iPrim;
            var v1 = LocalVertex(data, pPrim->V1);
            var v2 = LocalVertex(data, pPrim->V2);
            var v3 = LocalVertex(data, pPrim->V3);
            var w = obj->World.M;
            var w1 = SharpDX.Vector3.TransformCoordinate(new(v1.X, v1.Y, v1.Z), w).ToSystem();
            var w2 = SharpDX.Vector3.TransformCoordinate(new(v2.X, v2.Y, v2.Z), w).ToSystem();
            var w3 = SharpDX.Vector3.TransformCoordinate(new(v3.X, v3.Y, v3.Z), w).ToSystem();
            Camera.Instance?.DrawWorldLine(w1, w2, color);
            Camera.Instance?.DrawWorldLine(w2, w3, color);
            Camera.Instance?.DrawWorldLine(w3, w1, color);
        }

        private void VisualizeShape(CollisionShapePCBData* data, CollisionObjectShape* obj, uint color = ArenaColor.Danger)
        {
            for (int i = 0; i < data->NumPrims; ++i)
                VisualizePrimitive(data, i, obj, color);
            if (data->Child1Offset != 0)
                VisualizeShape((CollisionShapePCBData*)((byte*)data + data->Child1Offset), obj, color);
            if (data->Child2Offset != 0)
                VisualizeShape((CollisionShapePCBData*)((byte*)data + data->Child2Offset), obj, color);
        }

        private void VisualizeObject(CollisionObjectBase* obj)
        {
            switch (obj->Vtbl->GetObjectType(obj))
            {
                case CollisionObjectType.Multi:
                    {
                        var castObj = (CollisionObjectMulti*)obj;
                        if (castObj->Elements != null)
                        {
                            for (int i = 0; i < *castObj->PtrNumElements; ++i)
                            {
                                var elem = castObj->Elements + i;
                                if (elem->Shape != null && elem->Shape->Shape != null && (nint)elem->Shape->Shape->Vtbl == _typeinfoCollisionShapePCB && elem->Shape->Shape->Data != null)
                                    VisualizeShape(elem->Shape->Shape->Data, elem->Shape, ArenaColor.Safe);
                            }
                        }
                    }
                    break;
                case CollisionObjectType.Shape:
                    {
                        var castObj = (CollisionObjectShape*)obj;
                        if (castObj->Shape != null && (nint)castObj->Shape->Vtbl == _typeinfoCollisionShapePCB && castObj->Shape->Data != null)
                            VisualizeShape(castObj->Shape->Data, castObj, _slaveShapes.Contains((nint)obj) ? ArenaColor.Safe : ArenaColor.Danger);
                    }
                    break;
                case CollisionObjectType.Box:
                    {
                        var castObj = (CollisionObjectBox*)obj;
                        Camera.Instance?.DrawWorldOBB(new(-1), new(+1), castObj->World.M, ArenaColor.Enemy);
                    }
                    break;
                case CollisionObjectType.Cylinder:
                    {
                        var castObj = (CollisionObjectCylinder*)obj;
                        Camera.Instance?.DrawWorldUnitCylinder(castObj->World.M, ArenaColor.Enemy);
                    }
                    break;
                case CollisionObjectType.Sphere:
                    {
                        var castObj = (CollisionObjectSphere*)obj;
                        Camera.Instance?.DrawWorldSphere(castObj->Translation, castObj->Scale.X, ArenaColor.Enemy);
                    }
                    break;
                case CollisionObjectType.Plane:
                case CollisionObjectType.PlaneEx:
                    {
                        var castObj = (CollisionObjectPlane*)obj;
                        var m = castObj->World.M;
                        var a = SharpDX.Vector3.TransformCoordinate(new(-1, +1, 0), m).ToSystem();
                        var b = SharpDX.Vector3.TransformCoordinate(new(-1, -1, 0), m).ToSystem();
                        var c = SharpDX.Vector3.TransformCoordinate(new(+1, -1, 0), m).ToSystem();
                        var d = SharpDX.Vector3.TransformCoordinate(new(+1, +1, 0), m).ToSystem();
                        Camera.Instance?.DrawWorldLine(a, b, ArenaColor.Enemy);
                        Camera.Instance?.DrawWorldLine(b, c, ArenaColor.Enemy);
                        Camera.Instance?.DrawWorldLine(c, d, ArenaColor.Enemy);
                        Camera.Instance?.DrawWorldLine(d, a, ArenaColor.Enemy);
                    }
                    break;
            }
        }

        private void VisualizeQuadtreeNode(CollisionNode* node)
        {
            var child = (CollisionObjectBase*)node->Next;
            while (child != null && child != node)
            {
                VisualizeObject(child);
                child = child->NextNodeObj;
            }
        }

        private void VisualizeScene(CollisionScene* scene)
        {
            var obj = scene->FirstObj;
            while (obj != null)
            {
                VisualizeObject(obj);
                obj = (CollisionObjectBase*)obj->Base.Next;
            }
        }

        private Vector3 LocalVertex(CollisionShapePCBData* data, int index)
        {
            var pRaw = (float*)(data + 1);
            if (index < data->NumVertsRaw)
            {
                pRaw += 3 * index;
                return new(pRaw[0], pRaw[1], pRaw[2]);
            }
            var pCompr = (ushort*)(pRaw + 3 * data->NumVertsRaw);
            pCompr += 3 * (index - data->NumVertsRaw);
            var quantScale = (data->AABBMax - data->AABBMin) / 65535.0f;
            return data->AABBMin + quantScale * new Vector3(pCompr[0], pCompr[1], pCompr[2]);
        }

        private void ExportToObj(bool streamed, bool nonStreamedShapes)
        {
            var res = new StringBuilder();
            var firstVertex = 1;

            var scene = CollisionModule.Instance->Manager->FirstScene;
            var identity = SharpDX.Matrix.Identity;
            while (scene != null)
            {
                var obj = scene->Scene->FirstObj;
                while (obj != null)
                {
                    switch (obj->Vtbl->GetObjectType(obj))
                    {
                        case CollisionObjectType.Multi:
                            if (streamed)
                            {
                                var castObj = (CollisionObjectMulti*)obj;
                                if (castObj->Elements != null)
                                {
                                    var basePath = MemoryHelper.ReadStringNullTerminated((nint)castObj->PathBase);
                                    for (int i = 0; i < *castObj->PtrNumElements; ++i)
                                    {
                                        var f = Service.DataManager.GetFile($"{basePath}/tr{castObj->Elements[i].SubFile:d4}.pcb");
                                        if (f != null)
                                        {
                                            // format: dword 0, dword version (1/4), dword totalChildNodes, dword totalPrims, pcbdata
                                            fixed (byte* data = &f.Data[0])
                                            {
                                                var version = *(int*)(data + 4);
                                                if (version is 1 or 4)
                                                {
                                                    ExportShape(res, ref identity, (CollisionShapePCBData*)(data + 16), ref firstVertex);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case CollisionObjectType.Shape:
                            if (nonStreamedShapes && !_slaveShapes.Contains((nint)obj))
                            {
                                var castObj = (CollisionObjectShape*)obj;
                                if (castObj->Shape != null && (nint)castObj->Shape->Vtbl == _typeinfoCollisionShapePCB && castObj->Shape->Data != null)
                                {
                                    var m = castObj->World.M;
                                    ExportShape(res, ref m, castObj->Shape->Data, ref firstVertex);
                                }
                            }
                            break;
                    }
                    obj = (CollisionObjectBase*)obj->Base.Next;
                }
                scene = (CollisionSceneWrapper*)scene->Base.Next;
            }
            ImGui.SetClipboardText(res.ToString());
        }

        private void ExportShape(StringBuilder res, ref SharpDX.Matrix world, CollisionShapePCBData* data, ref int firstVertex)
        {
            var pRaw = (float*)(data + 1);
            for (int i = 0; i < data->NumVertsRaw; ++i)
            {
                var v = new Vector3(pRaw[0], pRaw[1], pRaw[2]);
                var w = SharpDX.Vector3.TransformCoordinate(new(v.X, v.Y, v.Z), world);
                res.AppendLine($"v {w.X} {w.Y} {w.Z}");
                pRaw += 3;
            }
            var pCompressed = (ushort*)pRaw;
            var quantScale = (data->AABBMax - data->AABBMin) / 65535.0f;
            for (int i = 0; i < data->NumVertsCompressed; ++i)
            {
                var v = data->AABBMin + quantScale * new Vector3(pCompressed[0], pCompressed[1], pCompressed[2]);
                var w = SharpDX.Vector3.TransformCoordinate(new(v.X, v.Y, v.Z), world);
                res.AppendLine($"v {w.X} {w.Y} {w.Z}");
                pCompressed += 3;
            }
            var pPrims = (CollisionShapePrimitive*)pCompressed;
            for (int i = 0; i < data->NumPrims; ++i)
            {
                res.AppendLine($"f {pPrims->V1 + firstVertex} {pPrims->V2 + firstVertex} {pPrims->V3 + firstVertex}");
                ++pPrims;
            }
            firstVertex += data->NumVertsRaw + data->NumVertsCompressed;

            if (data->Child1Offset != 0)
                ExportShape(res, ref world, (CollisionShapePCBData*)((byte*)data + data->Child1Offset), ref firstVertex);
            if (data->Child2Offset != 0)
                ExportShape(res, ref world, (CollisionShapePCBData*)((byte*)data + data->Child2Offset), ref firstVertex);
        }

        private void Report()
        {
            Dictionary<nint, int> shapeVtbls = new();
            Dictionary<int, int> multiVersions = new();

            var scene = CollisionModule.Instance->Manager->FirstScene;
            while (scene != null)
            {
                var obj = scene->Scene->FirstObj;
                while (obj != null)
                {
                    switch (obj->Vtbl->GetObjectType(obj))
                    {
                        case CollisionObjectType.Multi:
                            {
                                var castObj = (CollisionObjectMulti*)obj;
                                if (castObj->Elements != null)
                                {
                                    var basePath = MemoryHelper.ReadStringNullTerminated((nint)castObj->PathBase);
                                    for (int i = 0; i < *castObj->PtrNumElements; ++i)
                                    {
                                        var f = Service.DataManager.GetFile($"{basePath}/tr{castObj->Elements[i].SubFile:d4}.pcb");
                                        if (f != null)
                                        {
                                            // format: dword 0, dword version (1/4), dword totalChildNodes, dword totalPrims, pcbdata
                                            fixed (byte* data = &f.Data[0])
                                            {
                                                var version = *(int*)(data + 4);
                                                if (!multiVersions.ContainsKey(version))
                                                    multiVersions[version] = 0;
                                                ++multiVersions[version];
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case CollisionObjectType.Shape:
                            {
                                var castObj = (CollisionObjectShape*)obj;
                                if (castObj->Shape != null)
                                {
                                    var vt = (nint)castObj->Shape->Vtbl;
                                    if (!shapeVtbls.ContainsKey(vt))
                                        shapeVtbls[vt] = 0;
                                    ++shapeVtbls[vt];
                                }
                            }
                            break;
                    }
                    obj = (CollisionObjectBase*)obj->Base.Next;
                }
                scene = (CollisionSceneWrapper*)scene->Base.Next;
            }

            var res = new StringBuilder();
            res.AppendLine("multi versions:");
            foreach (var v in multiVersions)
                res.AppendLine($"v{v.Key} == {v.Value}");
            res.AppendLine("shape vtbls:");
            foreach (var vt in shapeVtbls)
                res.AppendLine($"{vt.Key - Service.SigScanner.Module.BaseAddress:X} == {vt.Value}");
            ImGui.SetClipboardText(res.ToString());
        }

        private string SphereStr(Vector4 s) => $"[{s.X:f3}, {s.Y:f3}, {s.Z:f3}] R{s.W:f3}";
    }
}
