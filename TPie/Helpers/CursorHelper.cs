using ImGuiNET;
using System.Numerics;
using System.Runtime.InteropServices;

namespace TPie.Helpers
{
    public static class CursorHelper
    {
        public static void SetCursorPosition(Vector2 pos)
        {
            GetCursorPos(out PointInter systemMousePos);
            Vector2 imguiMousePos = ImGui.GetMousePos();

            float x = systemMousePos.X - (imguiMousePos.X - pos.X);
            float y = systemMousePos.Y - (imguiMousePos.Y - pos.Y);

            SetCursorPos((int)x, (int)y);
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out PointInter lpPoint);

#pragma warning disable CS0649
        private struct PointInter
        {
            public int X;
            public int Y;
        }
#pragma warning disable CS0649
    }
}
