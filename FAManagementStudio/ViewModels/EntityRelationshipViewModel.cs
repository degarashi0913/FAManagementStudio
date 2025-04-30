using FAManagementStudio.Common;
using GraphShape.Algorithms.Layout;
using QuikGraph;
using System.Collections.Generic;
using System.Linq;

namespace FAManagementStudio.ViewModels;

public class EntityRelationshipViewModel : ViewModelBase
{
    /// <summary>
    /// Default constructor for design time data.
    /// </summary>
#pragma warning disable CS8618
    public EntityRelationshipViewModel() { }
#pragma warning restore CS8618 
    private readonly List<EntityTableModel> _data = [];
    public EntityRelationshipViewModel(DbViewModel db)
    {
        CreateGraphToVisualize(db);
        _type = "Tree";
    }

    private void CreateGraphToVisualize(DbViewModel db)
    {
        foreach (var item in db.Tables)
        {
            var table = new EntityTableModel(item.TableName, item.Colums);
            Graph.AddVertex(table);
            _data.Add(table);
        }

        foreach (var table in _data)
        {
            foreach (var col in table.Columns.Where(x => (x.ConstraintsInf?.Kind & ConstraintsKind.Foreign) == ConstraintsKind.Foreign))
            {
                if (_data.FirstOrDefault(x => x.TableName == col.ConstraintsInf.ForeignKeyTableName) is { } column)
                {
                    Graph.AddEdge(new Edge<object>(column, table));
                }
            }
        }
    }
    public BidirectionalGraph<object, IEdge<object>> Graph { get; set; } = new BidirectionalGraph<object, IEdge<object>>();
    private string _type;
    public string LayoutAlgorithmType
    {
        get => _type;
        set
        {
            _type = value;
            Parameter = GetParameter(value);
            RaisePropertyChanged(nameof(LayoutAlgorithmType));
        }
    }

    private static ILayoutParameters? GetParameter(string type)
        => type switch
        {
            "Tree" => new SimpleTreeLayoutParameters
            {
                Direction = LayoutDirection.BottomToTop,
                LayerGap = 20,
                SpanningTreeGeneration = SpanningTreeGeneration.BFS,
                VertexGap = 20.0
            },
            "BoundedFR" => new BoundedFRLayoutParameters(),
            "ISOM" => new ISOMLayoutParameters(),
            "LinLog" => new LinLogLayoutParameters(),
            "Sugiyama" => new SugiyamaLayoutParameters
            {
                EdgeRouting = SugiyamaEdgeRouting.Orthogonal,
                WidthPerHeight = 5
            },
            _ => null,
        };

    private ILayoutParameters? _param;
    public ILayoutParameters? Parameter
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

    public record EntityTableModel(string TableName, List<ColumViewMoodel> Columns);
}
