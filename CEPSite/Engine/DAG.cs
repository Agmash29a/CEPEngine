using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class DAG<T> where T : class
{
    public LinkedList<Vertex<T>> Vertices = new LinkedList<Vertex<T>>();

    public void AddEdge(T startVertexData, T endVertexData)
    {
        // If both vertices exist then don't add vertices or edge
        bool startVertexAlreadyExists = false;
        bool endVertexAlreadyExists = false;

        Vertex<T> startVertex = null;
        Vertex<T> endVertex = null;

        var v1v2Edge = new Edge<T>
        {
            NodeFinish = endVertex,
            NodeStart = startVertex
        };

        if (Vertices.Any(v => v.Data == startVertexData))
        {
            //  This vertex already exists
            startVertex = Vertices.First(v => v.Data == startVertexData);
            startVertexAlreadyExists = true;
        }
        else
        {
            startVertex = new Vertex<T> { Data = startVertexData };
            Vertices.AddFirst(startVertex);
        }

        if (Vertices.Any(v => v.Data == endVertexData))
        {
            //  This vertex already exists
            endVertex = Vertices.First(v => v.Data == endVertexData);
            endVertexAlreadyExists = true;
        }
        else
        {
            endVertex = new Vertex<T> { Data = endVertexData };
            Vertices.AddLast(endVertex);
        }

        if (startVertexAlreadyExists && endVertexAlreadyExists)
        {
            return;
        }

        // only add if at least on of the vertices is new
        endVertex.Neighbors.AddFirst(v1v2Edge);
        startVertex.Neighbors.AddFirst(v1v2Edge);
    }
}

public class Vertex<T>
{
    public T Data;
    public LinkedList<Edge<T>> Neighbors = new LinkedList<Edge<T>>();
}

public class Edge<T>
{
    public Vertex<T> NodeStart;
    public Vertex<T> NodeFinish;
    public virtual bool Filter(SimpleEvent simpleEvent)
    {
        return false;
    }
}
