namespace Infrastructure.Cheats
{
    public interface ICheatsProvider
    {
        bool IsOpen { get; set; }
        string GetLabel();
        void RenderCheats();
    }
}
