namespace CosmosMiniOS.System
{
    public class Process
    {
        public string Name;
        public WindowData WindowData = new WindowData();

        // Logic — input, state
        public virtual void Run() { }

        // Çizim — sadece canvas'a yazar
        public virtual void Draw() { }

        public virtual void Start() { }
    }
}