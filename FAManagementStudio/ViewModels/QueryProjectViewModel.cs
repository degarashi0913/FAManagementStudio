using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace FAManagementStudio.ViewModels;

public class QueryProjectViewModel
{
    public static IEnumerable<IProjectNodeViewModel> GetData(params string[] paths)
    {
        foreach (var path in paths)
        {
            var info = new DirectoryInfo(path);
            yield return new QueryProjectFolderViewModel(info.Name, info.FullName);
        }
    }
}

public class QueryProjectFolderViewModel : IProjectNodeViewModel
{
    private readonly FileSystemWatcher watcher;
    public QueryProjectFolderViewModel(string name, string fullPath)
    {
        Name = name;
        FullPath = fullPath;
        watcher = new FileSystemWatcher(fullPath, "*.fmq");

        watcher.Created += (sender, e) =>
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (_children == null || e.Name == null) return;

                if (File.Exists(e.FullPath))
                {
                    _children.Add(new QueryProjectFileViewModel(e.Name, e.FullPath));
                }
                else
                {
                    _children.Add(new QueryProjectFolderViewModel(e.Name, e.FullPath));
                }
            }));
        };

        watcher.Deleted += (sender, e) =>
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (_children == null) return;

                var item = _children.Where(x => x.FullPath == e.FullPath).FirstOrDefault();
                if (item == null) return;
                _children.Remove(item);
            }));
        };
        watcher.Renamed += (sender, e) =>
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (_children == null || e.Name == null) return;

                var item = _children.Where(x => x.FullPath == e.FullPath).FirstOrDefault();
                if (item == null) return;
                _children.Remove(item);
                if (File.Exists(e.FullPath))
                {
                    _children.Add(new QueryProjectFileViewModel(e.Name, e.FullPath));
                }
                else
                {
                    _children.Add(new QueryProjectFolderViewModel(e.Name, e.FullPath));
                }
            }));
        };
        watcher.EnableRaisingEvents = true;
    }
    public string Name { get; }
    public string FullPath { get; }
    private ObservableCollection<IProjectNodeViewModel>? _children;
    public ObservableCollection<IProjectNodeViewModel> Children
    {
        get => _children ??= GetData();
    }

    private ObservableCollection<IProjectNodeViewModel> GetData()
    {
        var list = new ObservableCollection<IProjectNodeViewModel>();
        var dir = new DirectoryInfo(FullPath);
        foreach (var folder in dir.GetDirectories())
        {
            list.Add(new QueryProjectFolderViewModel(folder.Name, folder.FullName));
        }
        foreach (var file in dir.GetFiles("*.fmq"))
        {
            list.Add(new QueryProjectFileViewModel(file.Name, file.FullName));
        }
        return list;
    }
}

public class QueryProjectFileViewModel(string name, string fullPath) : IProjectNodeViewModel
{
    public string Name { get; } = name;
    public string FullPath { get; } = fullPath;
}

public interface IProjectNodeViewModel
{
    string Name { get; }
    string FullPath { get; }
}
