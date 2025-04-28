using FAManagementStudio.Common;
using GraphShape.Algorithms.Layout;
using QuikGraph;
using System.Collections.Generic;
using System.Linq;

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
                    return new SimpleTreeLayoutParameters
                    {
                        Direction = LayoutDirection.BottomToTop,
                        LayerGap = 20,
                        SpanningTreeGeneration = SpanningTreeGeneration.BFS,
                        VertexGap = 20.0
                    };
                case "BoundedFR":
                    return new BoundedFRLayoutParameters();
                case "ISOM":
                    return new ISOMLayoutParameters();
                case "LinLog":
                    return new LinLogLayoutParameters();
                case "Sugiyama":
                    return new SugiyamaLayoutParameters
                    {
                        EdgeRouting = SugiyamaEdgeRouting.Orthogonal,
                        WidthPerHeight = 5
                    };
                default:
                    return null;
            }
        }

        private ILayoutParameters _param;
        public ILayoutParameters Parameter
        {
            get => _param;
            set
            {
                _param = value;
                RaisePropertyChanged(nameof(Parameter));
            }
        }

        private double _scale = 1.0;
        public double Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                RaisePropertyChanged(nameof(Scale));
            }
        }
    }

    public class EntityTableModel
    {
        public string TableName { get; set; }
        public List<ColumViewMoodel> Colums { get; set; }
    }
}
