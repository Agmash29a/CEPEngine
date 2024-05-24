using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Pattern<T> where T : class
{
    public T Relativity { get; set; }

    public virtual bool Match(ref ComplexEvent complexEvent, SimpleEvent simpleEvent)
    {
        return false;
    }
}

public static class ActivePatterns<T> where T : class
{
    public static List<Pattern<T>> Patterns = new List<Pattern<T>>();
}