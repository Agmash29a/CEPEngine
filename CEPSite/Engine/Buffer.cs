using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace CEPSite
{
    public static class Buffer
    {
        public static Queue<SimpleEvent> EventBuffer = new Queue<SimpleEvent>();
        //public static List<PropertyValue> PropertyBuffer = new List<PropertyValue>();

        public static int BufferSize { get; set; } // Buffer size can change on the fly

        public static DateTime MinDate
        {
            get { return EventBuffer.Min(e => e.TimeStamp); }
        }

        public static DateTime MaxDate
        {
            get { return EventBuffer.Max(e => e.TimeStamp); }
        }

        public static int Property
        {
            get => default(int);
            set
            {
            }
        }

        public static void AddToEventBuffer(SimpleEvent newEvent)
        {
            while (EventBuffer.Count >= BufferSize)
            {
                EventBuffer.Dequeue();
            }

            CheckMatches(newEvent);
            EventBuffer.Enqueue(newEvent);

            // Add Properties....
        }

        public static void CheckMatches(SimpleEvent newEvent)
        {
            foreach (var p in ActivePatterns<DAG<SimpleEvent>>.Patterns)
            {
                var complexEvent = new ComplexEvent {PatternInstance = p};

                if (p.Match(ref complexEvent, newEvent))
                {
                    AddToEventBuffer(complexEvent);
                }
            }
        }

        public static SimpleEvent GetEvent(string Id)
        {
            return EventBuffer.FirstOrDefault(e => e.Id == Id);
        }
    }

    public static class Buffer_Overlay
    {
        public static Queue<SimpleEvent> EventBuffer = new Queue<SimpleEvent>();
        //public static List<PropertyValue> PropertyBuffer = new List<PropertyValue>();

        public static int BufferSize { get; set; } // Buffer size can change on the fly

        public static DateTime MinDate
        {
            get { return EventBuffer.Min(e => e.TimeStamp); }
        }

        public static DateTime x
        {
            get { return EventBuffer.Max(e => e.TimeStamp); }
        }

        public static void AddToEventBuffer(SimpleEvent newEvent)
        {
            EventBuffer.Enqueue(newEvent);
        }

        public static SimpleEvent GetEvent(string Id)
        {
            return EventBuffer.FirstOrDefault(e => e.Id == Id);
        }
    }

}