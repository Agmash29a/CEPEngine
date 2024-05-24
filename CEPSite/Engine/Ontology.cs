using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;


namespace Ontology
{
    /// <summary>Types</summary>
    public class SemanticType
    {
        /// <summary>Type information</summary>
        private Type type;
        /// <summary>Virtual type?</summary>
        private bool isVirtual;
        /// <summary>The name</summary>
        private string name;
        /// <summary>The relationships</summary> 
        private Hashtable isA;

        /// <summary>The name</summary>
        public string Name { get { return name; } }
        /// <summary>Is this a virtual type?</summary>
        public bool IsVirtual { get { return isVirtual; } }
        /// <summary>Type information, if not virtual</summary>
        public Type Type { get { return type; } }

        /// <summary>Constructor</summary>
        public SemanticType(string _name, Type _type)
        {
            name = _name;
            type = _type;
            isVirtual = (type == null);
            isA = new Hashtable();
        }

        /// <summary>Returns a classification based on a given schema</summary>
        public SemanticType IsA(string schema)
        {
            return isA[schema] as SemanticType;
        }

        public void Add(string schema, SemanticType st)
        {
            isA[schema] = st;
        }

        /// <summary>Returns a string representation of this class</summary>
        public override string ToString()
        {
            return string.Format("[{0}/{1}]", name, (isVirtual ? "virtual" : type.FullName));
        }
    }

    /// <summary>Ontology based extension</summary>
    public class SemanticObject
    {
        /// <summary>Loaded schemas</summary>
        private static Hashtable schemas = new Hashtable();
        /// <summary>Entity information</summary>
        private static Hashtable ontology = new Hashtable();
        /// <summary>Get/Set semantic type information</summary>
        public SemanticType SemanticType { get { return ontology[this.GetType().Name] as SemanticType; } }

        /// <summary>Returns a classification based on a given schema</summary>
        public SemanticType IsA(string schema)
        {
            SemanticType st = ontology[this.GetType().Name] as SemanticType;
            return st.IsA(schema);
        }

        /// <summary>Is a given schema available?</summary>
        public static bool Available(string schema)
        {
            return schemas.ContainsKey(schema);
        }

        /// <summary>Read a classification scheme</summary>
        public static void Parse(string url)
        {
            string schema = Path.GetFileNameWithoutExtension(url);
            string _namespace = null;

            // Was the schema already loaded?
            if (schemas.ContainsKey(schema))
                return;

            XmlReader reader = new XmlTextReader(url);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "schema": // <schema namespace="x">
                                {
                                    _namespace = reader.GetAttribute("namespace");
                                }
                                break;
                            case "object": // <object name="x" [isA="y"] /> 
                                {
                                    SemanticType parent = null;
                                    string name = reader.GetAttribute("name");
                                    string isA = reader.GetAttribute("isA");

                                    SemanticType st = ontology[name] as SemanticType;
                                    if (st != null)
                                    {
                                        if (isA != null) parent = ontology[isA] as SemanticType;
                                        if (parent != null) st.Add(schema, parent);
                                    }
                                    else
                                    {
                                        if (isA != null) parent = ontology[isA] as SemanticType;

                                        Assembly x = Assembly.GetCallingAssembly();
                                        Type type = x.GetType(_namespace + "." + name);
                                        st = new SemanticType(name, type);
                                        if (parent != null) st.Add(schema, parent);
                                        ontology[name] = st;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        break;
                    case XmlNodeType.Text:
                        break;
                    default: break;
                }
            }
            schemas[schema] = true;
        }
    }

    // ---------------

    class Element : SemanticObject { }
    class Node : Element { }
    class SE : Node { }
    class SF : Node { }
}