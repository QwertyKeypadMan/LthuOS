namespace CosmosMiniOS.Gui
{
    public struct GuiRect
    {
        public int X, Y, W, H;
        public GuiRect(int x, int y, int w, int h) { X = x; Y = y; W = w; H = h; }
        public int Right => X + W;
        public int Bottom => Y + H;
        public bool Contains(int px, int py) => px >= X && px <= Right && py >= Y && py <= Bottom;
        public bool Intersects(GuiRect o) => X < o.Right && Right > o.X && Y < o.Bottom && Bottom > o.Y;
    }
}
