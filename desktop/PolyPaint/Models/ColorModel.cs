namespace PolyPaint.Models
{
    class ColorModel
    {
        public short R { get; }
        public short G { get; }
        public short B { get; }

        public ColorModel(short r, short g, short b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}