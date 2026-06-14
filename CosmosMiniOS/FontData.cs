namespace CosmosMiniOS.Resources
{
    public static class FontData
    {
        public struct Glyph
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public int Advance;

            public Glyph(int x, int y, int w, int h, int advance)
            {
                X = x;
                Y = y;
                Width = w;
                Height = h;
                Advance = advance;
            }
        }


        public static Glyph[] Glyphs = new Glyph[256];


        public static void Init()
        {
            // ÖRNEK
            // Buraya converter'ın ürettiği satırlar gelecek

            Glyphs[65] = new Glyph(10, 20, 12, 16, 13); // A
            Glyphs[66] = new Glyph(25, 20, 11, 16, 12); // B
        }
    }
}