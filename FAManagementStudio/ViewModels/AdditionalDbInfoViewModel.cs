using FAManagementStudio.Common;
using System.Collections.Generic;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels;

public class AdditionalDbInfoViewModel(string displayName) : ViewModelBase
{
    public string DisplayName => displayName;
}

public class AdditionalDbInfoControl(DbViewModel db) : ViewModelBase
{
    private static IReadOnlyList<AdditionalDbInfoViewModel> ContentList { get; } = [
        new("Triggers"),
        new("Indexs"),
        new("Domains"),
        new("Procedures"),
        new("Sequences")];

    public object CurrentContent { get; private set; } = ContentList;

    private ICommand? _contentChange;
    public ICommand ContentChange => _contentChange ??= new RelayCommand<string>(ChangeContent);

    public void InitContent() => ChangeContent(string.Empty);

    public void ChangeContent(string target)
    {
        CurrentContent = target switch
        {
            "" => ContentList,
            "Triggers" => db.Triggers,
            "Indexs" => db.Indexes,
            "Domains" => db.Domains,
            "Procedures" => db.Procedures,
            "Sequences" => db.Sequences,
            _ => CurrentContent
        };
        RaisePropertyChanged(nameof(CurrentContent));
    }
}
