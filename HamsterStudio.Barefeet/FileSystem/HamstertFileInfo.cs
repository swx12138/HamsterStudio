using CommunityToolkit.Mvvm.Input;
using HamsterStudio.Barefeet.SysCall;
using System.Windows.Input;

namespace HamsterStudio.Barefeet.FileSystem;

public class HamstertFileInfo(string filename)
{
    public string Name { get; } = Path.GetFileName(filename);
    public string FullName { get; } = Path.GetFullPath(filename);
    public string Directory { get; } = Path.GetDirectoryName(filename) ?? Environment.CurrentDirectory;
    public required ICommand RemoveCommand { get; init; }
    public ICommand RevealInExplorerCommand { get; } = new RelayCommand(() => { ShellApi.SelectFile(Path.GetFullPath(filename)); });
}
