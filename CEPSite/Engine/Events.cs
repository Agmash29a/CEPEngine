using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public static class EventConfig
{
    public static TimeSpan Offset = TimeSpan.MinValue;

    public static void SetOffset(DateTime streamDate)
    {
        Offset = DateTime.Now - streamDate;
    }
}

public class SimpleEvent
{
    private Guid _id;
    public DateTime TimeStamp;

    // This is the name of the event type
    public string Significance { get; set; }

    public string Id
    {
        get
        {
            return _id.ToString();
        }
    }

    public SimpleEvent()
    {
        _id = Guid.NewGuid();

        // Overwrite with event timestamp if it exists
        TimeStamp = EventConfig.Offset != TimeSpan.MinValue ? DateTime.Now.Subtract(EventConfig.Offset) : TimeStamp = DateTime.Now;
    }
}

public class ComplexEvent : SimpleEvent
{
    // Relativity - This is the causal vector
    public Pattern<DAG<SimpleEvent>> PatternInstance;
    public string MatchDescription { get; set; }

    // Straight line DAGs
    public void AssignThreeNodeDAG(SimpleEvent event1, SimpleEvent event2, Pattern<DAG<SimpleEvent>> pattern)
    {
        var dag = new DAG<SimpleEvent>();
        dag.AddEdge(event1, event2);
        dag.AddEdge(event2, this);

        pattern.Relativity = dag;
        PatternInstance = pattern;
    }

    public void AssignFourNodeDAG(SimpleEvent event1, SimpleEvent event2, SimpleEvent event3, Pattern<DAG<SimpleEvent>> pattern)
    {
        var dag = new DAG<SimpleEvent>();
        dag.AddEdge(event1, event2);
        dag.AddEdge(event2, event3);
        dag.AddEdge(event3, this);

        pattern.Relativity = dag;
        PatternInstance = pattern;
    }
}
