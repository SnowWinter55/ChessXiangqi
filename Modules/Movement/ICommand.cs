/// <summary>Interface cho mọi hành động có thể undo/redo</summary>
public interface ICommand
{
    void Execute();
    void Undo();
    void Redo();
}