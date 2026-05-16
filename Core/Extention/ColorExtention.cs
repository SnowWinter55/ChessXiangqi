// /Core/Extensions/ColorExtensions.cs
using ChessXiangqiSolution.Core.Enums;

namespace ChessXiangqiSolution.Core.Extensions
{
    public static class ColorExtensions
    {
        public static Color Opposite(this Color color)
        {
            return color == Color.White ? Color.Black : Color.White;
        }
    }
}