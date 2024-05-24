using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading;

public class ObjectDumper
{
    private int _level;
    private readonly int _indentSize;
    private readonly StringBuilder _stringBuilder;
    private readonly List<int> _hashListOfFoundElements;

    private ObjectDumper(int indentSize)
    {
        _indentSize = indentSize;
        _stringBuilder = new StringBuilder();
        _hashListOfFoundElements = new List<int>();
    }

    public ObjectDumper()
    {
    }

    private List<string> _eventIDs;
    public static Dictionary<string, Tuple<Type, object, object>> Types; //<Type, Max (or array of values), Min>

    public static string Dump(object element)
    {
        return Dump(element, 2);
    }

    public Dictionary<string, Tuple<Type, object, object>> TypesCloneFactory()
    {
        var TypesClone = new Dictionary<string, Tuple<Type, object, object>>();

        if (Types == null)
        {
            Types = new Dictionary<string, Tuple<Type, object, object>>();
        }

        try
        {
            var types = Types.ToArray();

            foreach (var o in types)
            {
                if (o.Key != null && !TypesClone.ContainsKey(o.Key))
                {
                    try
                    {
                        TypesClone.Add(o.Key, o.Value);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            return TypesClone;
        }
        catch (Exception ex)
        {
            Thread.Sleep(1000);

            Debug.WriteLine(ex.Message);

            var types = Types.ToArray();

            foreach (var o in types)
            {
                if (o.Key != null && !TypesClone.ContainsKey(o.Key))
                {
                    try
                    {
                        TypesClone.Add(o.Key, o.Value);
                    }
                    catch (Exception ex2)
                    {
                        Debug.WriteLine(ex2.Message);
                    }
                }
            }

            return TypesClone;
        }

    }

    public static string Dump(object element, int indentSize)
    {
        Types = new Dictionary<string, Tuple<Type, object, object>>();

        var instance = new ObjectDumper(indentSize);
        return instance.DumpElement(element);
    }

    private string DumpElement(object element)
    {
        if (element == null || element is ValueType || element is string)
        {
            Write(FormatValue(element));
        }
        else
        {
            var objectType = element.GetType();

            // Only show certain types
            if (element is SimpleEvent)
            {
                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    Write("<b style='color: red'>{{{0}}}</b>", objectType.FullName);
                    _hashListOfFoundElements.Add(element.GetHashCode());
                    _level++;
                }

                var enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {
                    foreach (object item in enumerableElement)
                    {
                        if (item is IEnumerable && !(item is string))
                        {
                            _level++;
                            DumpElement(item);
                            _level--;
                        }
                        else
                        {
                            if (!AlreadyTouched(item))
                                DumpElement(item);
                            else
                                Write("<b>{{{0}}}</b> <-- bidirectional reference found", item.GetType().FullName);
                        }
                    }
                }
                else
                {
                    MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var memberInfo in members)
                    {
                        var fieldInfo = memberInfo as FieldInfo;
                        var propertyInfo = memberInfo as PropertyInfo;

                        if (fieldInfo == null && propertyInfo == null)
                            continue;

                        var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
                        object value = fieldInfo != null
                            ? fieldInfo.GetValue(element)
                            : propertyInfo.GetValue(element, null);

                        if (type.IsValueType || type == typeof(string))
                        {
                            Write("<b>{0}</b>: {1}", memberInfo.Name, FormatValue(value));

                            AddTypeToTypesList(memberInfo.Name, value);
                        }
                        else
                        {
                            var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                            Write("<b>{0}</b>: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }");

                            AddTypeToTypesList(memberInfo.Name, value);

                            var alreadyTouched = !isEnumerable && AlreadyTouched(value);
                            _level++;
                            if (!alreadyTouched)
                                DumpElement(value);
                            else
                            {
                                try
                                {
                                    Write("<b>{{{0}}}</b> <-- <i>bidirectional reference found</i>",
                                        value.GetType().FullName);
                                }
                                catch (Exception ex)
                                {
                                }
                                _level--;
                            }
                        }
                    }
                }

                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    _level--;
                }
            }
        }

        return _stringBuilder.ToString();
    }

    public static List<string> GetNestedIDs(object element)
    {
        var instance = new ObjectDumper(2);

        instance._eventIDs = new List<string>();
        instance.GetNestedIDsForElement(element);

        return instance._eventIDs;
    }

    private void AddTypeToTypesList(string name, object value)
    {
        object maxOrArray = null;
        object min = null;

        if (!Types.ContainsKey(name))
        {
            if (value is int)
            {
                maxOrArray = (int) value;
                min = (int) value;
            }

            if (value is float)
            {
                maxOrArray = (float) value;
                min = (float) value;
            }

            if (value is double)
            {
                maxOrArray = (double) value;
                min = (double) value;
            }

            if (value is long)
            {
                maxOrArray = (long) value;
                min = (long) value;
            }

            if (value is string[])
            {
                maxOrArray = value;
            }

            if (!Types.ContainsKey(name))
            {
                try
                {
                    Types.Add(name, new Tuple<Type, object, object>(value == null ? typeof(string) : value.GetType(), maxOrArray, min));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            } 
        }
        else
        {
            var entryInTypesList = Types[name];
            maxOrArray = entryInTypesList.Item2;
            min = entryInTypesList.Item3;

            // check for Max/Min

            if (value is int)
            {
                if ((int)value > (int)maxOrArray)
                    maxOrArray = value;

                if ((int)value < (int)min)
                    min = value;
            }

            if (value is float)
            {
                if ((float)value > (float)maxOrArray)
                    maxOrArray = value;

                if ((float)value < (float)min)
                    min = value;
            }

            if (value is double)
            {
                if ((double)value > (double)maxOrArray)
                    maxOrArray = value;

                if ((double)value < (double)min)
                    min = value;
            }

            if (value is long)
            {
                if ((long)value > (long)maxOrArray)
                    maxOrArray = value;

                if ((long)value < (long)min)
                    min = value;
            }

            if (value is string[])
            {
                foreach(var entry in (string[])value)
                {                
                    if(!((string[])maxOrArray).ToList().Contains(entry))
                    {
                        ((string[])maxOrArray).ToList().Add(entry);
                    }
                }
            }

            // Tuples are immutable so we nee a new one
            if (value != null)
            {
                var newTuple = new Tuple<Type, object, object>(value.GetType(), maxOrArray, min);

                Types[name] = newTuple;
            }
        }
    }

    private void GetNestedIDsForElement(object element)
    {
        if (!(element == null || element is ValueType || element is string))
        {
            var objectType = element.GetType();

            // Only show certain types
            if (element is SimpleEvent)
            {
                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    _hashListOfFoundElements.Add(element.GetHashCode());
                }

                var enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {
                    foreach (object item in enumerableElement)
                    {
                        if (item is IEnumerable && !(item is string))
                        {
                            GetNestedIDsForElement(item);
                        }
                        else
                        {
                            if (!AlreadyTouched(item))
                                GetNestedIDsForElement(item);
                        }
                    }
                }
                else
                {
                    MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    
                    foreach (var memberInfo in members)
                    {
                        var fieldInfo = memberInfo as FieldInfo;
                        var propertyInfo = memberInfo as PropertyInfo;

                        if (fieldInfo == null && propertyInfo == null)
                            continue;

                        var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;

                        object value = fieldInfo != null
                                           ? fieldInfo.GetValue(element)
                                           : propertyInfo.GetValue(element, null);

                        if (type.IsValueType || type == typeof(string))
                        {
                            // Test if the value is an ID
                            if (memberInfo.Name == "Id")
                            {
                                _eventIDs.Add(value.ToString());
                            }
                        }
                        else
                        {
                            var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);

                            var alreadyTouched = !isEnumerable && AlreadyTouched(value);

                            if (!alreadyTouched)
                                GetNestedIDsForElement(value);
                        }
                    }
                }
            }
        }
    }

    private bool AlreadyTouched(object value)
    {
        try
        {
            var hash = value.GetHashCode();
            for (var i = 0; i < _hashListOfFoundElements.Count; i++)
            {
                if (_hashListOfFoundElements[i] == hash)
                    return true;
            }
            return false;
        }
        catch(Exception ex)
        {
            return true;
        }
    }

    private void Write(string value, params object[] args)
    {
        if (args != null)
            value = string.Format(value, args);

        _stringBuilder.AppendLine(value + "<br>");
    }

    private string FormatValue(object o)
    {
        if (o == null)
            return ("null");

        if (o is DateTime)
            return (((DateTime)o).ToShortDateString() + " - " + ((DateTime)o).ToShortTimeString());

        if (o is string)
            return string.Format("\"{0}\"", o);

        if (o is ValueType)
            return (o.ToString());

        if (o is IEnumerable)
            return ("...");

        return ("{ }");
    }
}