using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GLSample.Sources.Core
{
    public class PostProcessing
    {
        public bool TritoneEnabled = false;
        public Vector4 ShadowsColor = Vector4.Zero;
        public Vector4 MidtonesColor = Vector4.One / 2;
        public Vector4 HighlightsColor = Vector4.One;

        public bool ChromaticAberrationEnabled = false;
        public float ChromaticAberrationIntensity = 0.5f;

        public float Brightness = 1.0f;

        public void RenderImGui()
        {
            if (ImGui.CollapsingHeader("Post Processing", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat("Brightness", ref Brightness, 0.005f);

                ImGui.Separator();
                ImGui.Checkbox("Tritone Effect", ref TritoneEnabled);
                ImGui.ColorEdit4("Shadows", ref ShadowsColor);
                ImGui.ColorEdit4("Midtones", ref MidtonesColor);
                ImGui.ColorEdit4("Highlights", ref HighlightsColor);

                ImGui.Separator();
                ImGui.Checkbox("Chromatic Aberration", ref ChromaticAberrationEnabled);
                ImGui.SliderFloat("Intensity", ref ChromaticAberrationIntensity, 0.0f, 1.0f);
            }
        }
    }
}
