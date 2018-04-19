using System.Windows.Media;

namespace PolyPaint.Utils
{
    public static class ThemeColors
    {
        public static readonly Brush MainBackground = (Brush)new BrushConverter().ConvertFrom("#4080FF");
        public static readonly Brush MainBackgroundDark = (Brush)new BrushConverter().ConvertFrom("#3F7DF7");
        public static readonly Brush MainBackgroundLight = (Brush)new BrushConverter().ConvertFrom("#80C0FF");
        public static readonly Brush MainForeground = (Brush)new BrushConverter().ConvertFrom("White");

        public static readonly Brush SecondaryBackgroundDark = (Brush)new BrushConverter().ConvertFrom("#E4E4E4");
        public static readonly Brush SecondaryBackgroundLight = (Brush)new BrushConverter().ConvertFrom("#F4F4F4");
        public static readonly Brush SecondaryForeground = (Brush)new BrushConverter().ConvertFrom("Black");

        public static readonly Brush FeedbackIconForeground = (Brush)new BrushConverter().ConvertFrom("#CCCCCC");

        public static readonly Brush AlmostBlack = (Brush)new BrushConverter().ConvertFrom("#2F2F2F");
        public static readonly Brush AlmostBlackLight = (Brush) new BrushConverter().ConvertFrom("#7C7C7C");
    }
}
