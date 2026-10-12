namespace VyshKat.ConsoleApp;

public interface IApplicationUi
{
    void RegisterTransport();
    void RegisterEquipment();
    void ShowSummary();
    void ShowBeginnerFriendly();
    void ShowInventory();
    void RunInteractive();
    void RunDemo();
}
