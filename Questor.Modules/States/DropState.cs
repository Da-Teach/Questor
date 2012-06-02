namespace Questor.Modules.States
{
    public enum DropState
    {
        Idle,
        Begin,
        OpenItemHangar,
        OpenCargo,
        MoveItems,
        AllItems,
        WaitForMove,
        StackItemsHangar,
        WaitForStacking,
        Done,
    }
}