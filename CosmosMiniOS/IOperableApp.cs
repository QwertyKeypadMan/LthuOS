namespace CosmosMiniOS
{
    public interface IOperableApp
    {
        string FileName { get; }
        string Description { get; }
        void Run(); 
    }
}