using Dalamud.Bindings.ImGui;

namespace BossMod.Dev;

class GeometryHelper() : TestWindow("Gemoetry helper", new(500, 200), ImGuiWindowFlags.AlwaysAutoResize)
{
    float X;
    float Z;

    float R;
    float Theta;

    public override void Draw()
    {
        if (ImGui.InputFloat("X", ref X) || ImGui.InputFloat("Z", ref Z))
        {
            var x = new WDir(X, Z);
            R = x.Length();
            Theta = x.ToAngle().Rad;
        }

        if (ImGui.InputFloat("R", ref R) || ImGui.InputFloat("ϴ", ref Theta))
        {
            var d = Theta.Radians();
            (X, Z) = d.ToDirection() * R;
        }
    }
}
