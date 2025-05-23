﻿using FAManagementStudio.Foundation.Common;
using FAManagementStudio.ViewModels.Commons;
using FAManagementStudio.ViewModels.Db;
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
        new("Indexes"),
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
            "Indexes" => db.Indexes,
            "Domains" => db.Domains,
            "Procedures" => db.Procedures,
            "Sequences" => db.Sequences,
            _ => CurrentContent
        };
        RaisePropertyChanged(nameof(CurrentContent));
    }
}
