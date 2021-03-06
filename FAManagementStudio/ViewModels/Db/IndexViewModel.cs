﻿using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System.Collections.Generic;

namespace FAManagementStudio.ViewModels
{
    public class IndexViewModel : ViewModelBase, IAddtionalDbInfo
    {
        private IndexInfo _index;
        public IndexViewModel(IndexInfo inf)
        {
            _index = inf;
        }

        public string IndexName { get { return _index.Name; } }

        public ConstraintsKind Kind { get { return _index.Kind; } }

        public string ForignKeyName { get { return _index.ForigenKeyName; } }
        public string UpdateRule { get { return _index.UpdateRule; } }
        public string DeleteRule { get { return _index.DeleteRule; } }

        public string TableName { get { return _index.TableName; } }

        public List<string> FieldNames { get { return _index.FieldNames; } }
    }
}
