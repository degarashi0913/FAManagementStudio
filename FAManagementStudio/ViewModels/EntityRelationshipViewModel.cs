using FAManagementStudio.Common;
using GraphSharp.Algorithms.Layout;
using GraphSharp.Algorithms.Layout.Simple.FDP;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using GraphSharp.Algorithms.Layout.Simple.Tree;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAManagementStudio.ViewModels
{
    public class EntityRelationshipViewModel : ViewModelBase
    {
        public EntityRelationshipViewModel() { }
        private List<EntityTableModel> _data = new List<EntityTableModel>();
        public EntityRelationshipViewModel(DbViewModel db)
        {
            CreateGraphToVisualize(db);
            this.LayoutAlgorithmType = "Tree";
        }

        private void CreateGraphToVisualize(DbViewModel db)
        {
            foreach (var item in db.Tables)
            {
                var table = new EntityTableModel { TableName = item.TableName, Colums = item.Colums };
                Graph.AddVertex(table);
                _data.Add(table);
            }

            foreach (var table in _data)
            {
                foreach (var col in table.Colums.Where(x => (x.ConstraintsInf?.Kind & ConstraintsKind.Foreign) == ConstraintsKind.Foreign))
                {
                    Graph.AddEdge(new Edge<object>(_data.Find(x => x.TableName == col.ConstraintsInf.ForeignKeyTableName), table));
                }
            }
        }
        public BidirectionalGraph<object, IEdge<object>> Graph { get; set; } = new BidirectionalGraph<object, IEdge<object>>();
        private string _type;
        public string LayoutAlgorithmType
        {
            get { return _type; }
            set
            {
                _type = value;
                Parameter = GetParameter(value);
                RaisePropertyChanged(nameof(LayoutAlgorithmType));
            }
        }

        private ILayoutParameters GetParameter(string type)
        {
            switch (type)
            {
                case "Tree":
                    var treeParam = new SimpleTreeLayoutParameters();
                    treeParam.Direction = LayoutDirection.BottomToTop;
                    treeParam.LayerGap = 20;
                    treeParam.SpanningTreeGeneration = SpanningTreeGeneration.BFS;
                    treeParam.VertexGap = 20.0;
                    treeParam.WidthPerHeight = 50;

                    return treeParam;
                case "BoundedFR":
                    var frParam = new BoundedFRLayoutParameters();

                    return frParam;
                case "ISOM":
                    var isomParam = new ISOMLayoutParameters();

                    return isomParam;
                case "LinLog":
                    var linlogParam = new LinLogLayoutParameters();

                    return linlogParam;
                case "EfficientSugiyama":
                    var sugiyamaParam = new EfficientSugiyamaLayoutParameters();
                    sugiyamaParam.EdgeRouting = SugiyamaEdgeRoutings.Orthogonal;
                    sugiyamaParam.WidthPerHeight = 5;

                    return sugiyamaParam;

                default:
                    return null;
            }
        }

        private ILayoutParameters _param;
        public ILayoutParameters Parameter
        {
            get { return _param; }
            set
            {
                _param = value;
                RaisePropertyChanged(nameof(Parameter));
            }
        }
    }

    public class EntityTableModel
    {
        public string TableName { get; set; }
        public List<ColumViewMoodel> Colums { get; set; }
    }
}
