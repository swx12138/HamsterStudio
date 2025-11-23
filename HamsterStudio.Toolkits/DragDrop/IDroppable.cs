using System.Windows;

namespace HamsterStudio.Toolkits.DragDrop;

public interface IDragable
{
    string DataFormat { get; } 
}

public interface IDroppable<T>
{
    string[] AcceptDataFormat { get; }
    void Drop(T data);
    void Drop(object? data);
}

public abstract class FileDroppableBase : IDroppable<string[]>
{
    public string[] AcceptDataFormat { get; } = [DataFormats.FileDrop];
    public abstract void Drop(string[] data);

    public void Drop(object? data)
    {
        throw new NotImplementedException();
    }
}
