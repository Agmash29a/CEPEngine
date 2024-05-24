using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Model;

public static class Parser
{
    public const string SIMPLE_EVENT_MARKER = "-- SIMPLE EVENT --";
    public const string COMPLEX_EVENT_MARKER = "-- COMPLEX EVENT --";

    public static List<NewsUpdate> NewsUpdates { get; set; }
    public static List<PriceUpdate> PriceUpdates { get; set; }
    public static List<PriceUpdate> PriceUpdates_Overlay { get; set; }
    public static List<Sentiment> NegativeSentimentValues { get; set; }
    public static List<Sentiment> PositiveSentimentValues { get; set; }
    public static List<DateTime> Dates { get; set; }

    public static string NewsFilePath { get; set; }
    public static string PricesFilePath { get; set; }

    public static int PriceIntervalSeconds { get; set; } = 60;

    public static int Modifier { get; set; }
    public static string ProductName { get; set; }

    public static void ParseNewsFile()
    {
        var random = new Random();

        //create object for CSVReader and pass the stream
        var reader = new CSVReader(File.OpenRead(NewsFilePath));

        //get the header
        string[] headers = reader.GetCSVLine();
        string[] data;

        NewsUpdates = new List<NewsUpdate>();

        while ((data = reader.GetCSVLine()) != null)
        {
            var story = new NewsUpdate();

            if (data[0] == "") continue;

            story.MSG_DATE = DateTime.Parse(data[0]);

            string[] time = data[1].Split(':');
            string[] seconds = time[2].Split('.');

            story.MSG_TIME_MILLISECONDS = int.Parse(seconds[1]);
            story.MSG_TIME_SECONDS = int.Parse(seconds[0]);
            story.MSG_TIME_MINUTES = int.Parse(time[1]);

            //string[] daysHours = data[5].Split("  ".ToCharArray());
            //string[] hours = daysHours[1].Split(':');

            //story.MSG_TIME_HOURS = int.Parse(hours[0]);

            story.MSG_TIME_HOURS = int.Parse(time[0]);

            story.UNIQUE_STORY_INDEX = data[2];
            story.EVENT_TYPE = (EVENT_TYPES) Enum.Parse(typeof(EVENT_TYPES), data[3]);
            story.PNAC = data[4];

            // 3/01/2010  8:55:18 PM

            story.STORY_DATE_TIME =
                story.MSG_DATE
                    .AddHours(story.MSG_TIME_HOURS)
                    .AddMinutes(story.MSG_TIME_MINUTES)
                    .AddSeconds(story.MSG_TIME_SECONDS);

            DateTime tryParseValueDateTime;
            story.TAKE_DATE_TIME = (DateTime.TryParse(data[6], out tryParseValueDateTime))
                ? tryParseValueDateTime
                : (DateTime?) null;

            story.HEADLINE_ALERT_TEXT = data[7];
            story.ACCUMULATED_STORY_TEXT = data[8];
            story.TAKE_TEXT = data[9];
            story.PRODUCTS = data[10].Split(' ');
            story.TOPICS = data[11].Split(' ');
            story.RELATED_RICS = data[12].Split(' ');
            story.NAMED_ITEMS = data[13].Split(' ');

            int tryParseValue;
            story.HEADLINE_SUBTYPE = (int.TryParse(data[14], out tryParseValue)) ? tryParseValue : (int?) null;

            story.STORY_TYPE = data[15];
            story.TABULAR_FLAG = bool.Parse(data[16]);
            story.ATTRIBUTION = data[17];
            story.LANGUAGE = data[18];

            // Add event model timestamp
            story.TimeStamp = DateTime.Parse(data[5])
                .AddHours(story.MSG_TIME_HOURS)
                .AddMinutes(story.MSG_TIME_MINUTES)
                .AddSeconds(story.MSG_TIME_SECONDS);

            NewsUpdates.Add(story);
        }
    }

    public static void InsertNewsIntoDB()
    {
        var dal = DataAccess.Instance;

        foreach (var newsUpdate in NewsUpdates)
        {
            int id = dal.AddNewsUpdate(newsUpdate);

            foreach (var RELATED_RIC in newsUpdate.RELATED_RICS)
            {
                dal.AddNewsUpdate_RELATED_RIC(id, RELATED_RIC);
            }

            foreach (var TOPIC in newsUpdate.TOPICS)
            {
                dal.AddNewsUpdate_TOPIC(id, TOPIC);
            }

            foreach (var NAMED_ITEM in newsUpdate.NAMED_ITEMS)
            {
                dal.AddNewsUpdate_NAMED_ITEM(id, NAMED_ITEM);
            }

            foreach (var PRODUCT in newsUpdate.PRODUCTS)
            {
                dal.AddNewsUpdate_RELATED_RIC(id, PRODUCT);
            }
        }
    }

    public static void ParsePricesFile()
    {
        //create object for CSVReader and pass the stream
        var reader = new CSVReader(File.OpenRead(PricesFilePath));

        //get the header
        var headers = reader.GetCSVLine();

        string[] data;

        PriceUpdates = new List<PriceUpdate>();

        while ((data = reader.GetCSVLine()) != null)
        {
            var priceUpdate = new PriceUpdate();

            priceUpdate.SECURITY = data[0];
            priceUpdate.DATE_OF_TRADE = DateTime.Parse(data[1]);

            string[] time = data[2].Split(':');

            double MINUTES = int.Parse(time[1]);
            double HOURS = int.Parse(time[0]);

            priceUpdate.START_PERIOD = DateTime.Parse(data[1]).AddHours(HOURS).AddMinutes(MINUTES);

            time = data[3].Split(':');

            MINUTES = int.Parse(time[1]);
            HOURS = int.Parse(time[0]);

            priceUpdate.END_PERIOD = DateTime.Parse(data[1]).AddHours(HOURS).AddMinutes(MINUTES);

            priceUpdate.NUM_TRADE = int.Parse(data[4]);
            priceUpdate.LIST_PRICE = float.Parse(data[5]);
            priceUpdate.AVERAGE_PRICE = float.Parse(data[6]);
            priceUpdate.TOTAL_VOLUME = double.Parse(data[7]);

            // Add event model timestamp.
            priceUpdate.TimeStamp = DateTime.Parse(data[1]).AddHours(HOURS).AddMinutes(MINUTES);

            PriceUpdates.Add(priceUpdate);
        }
    }

    public static string GetDateLabelsJSON(int numberOfDates, List<PriceUpdate> prices)
    {
        Dates = new List<DateTime>();

        // Insert day dd/MM/yyyy into new list
        prices.ForEach(pu =>
            {
                if (!Dates.Contains(pu.TimeStamp))
                {
                    Dates.Add(pu.TimeStamp);
                }
            }
        );

        var JSON = "";

        Dates.Sort((a, b) => a.CompareTo(b));

        var lastDate = Dates.Last();
        var firstDate = Dates.First();

        numberOfDates--;

        var delta = TimeSpan.FromTicks(lastDate.Subtract(firstDate).Ticks/numberOfDates);

        var tempDate = firstDate;

        // Cycle through Dates
        while (numberOfDates > -1)
        {
            numberOfDates--;
            JSON += "'" + $"{tempDate:g}" + "', ";
            tempDate = tempDate.Add(delta);
        }

        JSON = "[" + JSON.Substring(0, JSON.Length - 3) + "     '" + "]";

        return JSON;
    }

    public static string GetSortedPricesJSON(List<PriceUpdate> prices)
    {
        string JSON = "";

        var pricesSorted = prices.OrderByDescending(t => t.TimeStamp);

        var lastDate_Modified = pricesSorted.FirstOrDefault().TimeStamp;
        var firstDate_Modified = pricesSorted.Last().TimeStamp;

        float lastPrice = 0;

        while (lastDate_Modified > firstDate_Modified)
        {
            var pricesForThisPeriod =
                prices.Where(
                    p =>
                        p.TimeStamp >= firstDate_Modified &&
                        p.TimeStamp < firstDate_Modified.AddSeconds(PriceIntervalSeconds)).ToList();

            if (!pricesForThisPeriod.Any())
            {
                JSON += lastPrice + Modifier + ", ";
            }
            else
            {
                float sum = 0;

                pricesForThisPeriod.ForEach(p => sum += p.LIST_PRICE);

                JSON += (sum/pricesForThisPeriod.Count) + Modifier + ", ";

                lastPrice = pricesForThisPeriod.Last().LIST_PRICE;
            }

            firstDate_Modified = firstDate_Modified.AddSeconds(PriceIntervalSeconds);
        }

        return "[" + JSON.Substring(0, JSON.Length - 2) + "]";
    }

    public static string GetNumberOfPNACsPerPriceUpdateJSON(List<PriceUpdate> prices)
    {
        string JSON = "";
        var index = 0;

        foreach (var price in prices)
        {
            var endPeriod = price.TimeStamp.AddSeconds(PriceIntervalSeconds);

            if (prices.Count() > (index + 1))
            {
                var nextPrice = prices[index + 1];
                if (nextPrice.TimeStamp > endPeriod)
                    endPeriod = nextPrice.TimeStamp;
            }

            JSON += GetMatchesByProductName(price, endPeriod).Count() + ", ";
            index++;
        }

        return "[" + JSON.Substring(0, JSON.Length - 2) + "]";
    }

    public static List<List<NewsUpdate>> GetNewsUpdatesPerPriceUpdate(List<PriceUpdate> prices)
    {
        var NewsUpdates = new List<List<NewsUpdate>>();
        var index = 0;

        foreach (var price in prices)
        {
            var endPeriod = price.TimeStamp.AddSeconds(PriceIntervalSeconds);

            if (prices.Count() > (index + 1))
            {
                var nextPrice = prices[index + 1];
                if (nextPrice.TimeStamp > endPeriod)
                    endPeriod = nextPrice.TimeStamp;
            }

            NewsUpdates.Add(GetMatchesByProductName(price, endPeriod).ToList());
            index++;
        }

        return NewsUpdates;
    }

    public static List<List<SimpleEvent>> GetEventsPerPriceUpdate(List<PriceUpdate> prices, List<SimpleEvent> events)
    {
        var eventsPerPriceUpdate = new List<List<SimpleEvent>>();
        var index = 0;

        foreach (var price in prices)
        {
            var endPeriod = price.TimeStamp.AddSeconds(PriceIntervalSeconds);

            if (prices.Count() > (index + 1))
            {
                var nextPrice = prices[index + 1];
                if (nextPrice.TimeStamp > endPeriod)
                    endPeriod = nextPrice.TimeStamp;
            }

            var eventsForThisPrice =
                events.Where(e => ((e.TimeStamp >= price.TimeStamp) && (e.TimeStamp < endPeriod))).ToList();

            events.RemoveAll(e => ((e.TimeStamp >= price.TimeStamp) && (e.TimeStamp < endPeriod)));

            eventsPerPriceUpdate.Add(eventsForThisPrice);

            index++;
        }

        return eventsPerPriceUpdate;
    }

    public static List<List<SimpleEvent>> GetEventsPerPriceUpdate(List<PriceUpdate> prices, List<SimpleEvent> events,
        ComplexEvent singleEvent)
    {
        var eventsPerPriceUpdate = new List<List<SimpleEvent>>();
        var index = 0;
        var lastEqualStartAndEndTime = DateTime.MinValue;

        var singleEventDAGEvents = new List<SimpleEvent>();

        singleEvent.PatternInstance.Relativity.Vertices.ToList().ForEach(v => singleEventDAGEvents.Add(v.Data));

        foreach (var price in prices)
        {
            var endPeriod = price.TimeStamp.AddSeconds(PriceIntervalSeconds);

            if (prices.Count() > (index + 1))
            {
                var nextPrice = prices[index + 1];
                if (nextPrice.TimeStamp > endPeriod)
                    endPeriod = nextPrice.TimeStamp;
            }

            var eventsForThisPrice =
                events.Where(
                    e =>
                        ((e.TimeStamp >= price.TimeStamp) && (e.TimeStamp < endPeriod) &&
                         (singleEventDAGEvents.Contains(e)))).ToList();

            events.RemoveAll(
                e =>
                    ((e.TimeStamp >= price.TimeStamp) && (e.TimeStamp < endPeriod) &&
                     (singleEventDAGEvents.Contains(e))));
            eventsPerPriceUpdate.Add(eventsForThisPrice);

            index++;
        }

        return eventsPerPriceUpdate;
    }

    public static List<List<SimpleEvent>> GetEventsPerPriceUpdate(List<PriceUpdate> prices, List<SimpleEvent> events,
        List<SimpleEvent> eventSublist)
    {
        var eventsPerPriceUpdate = new List<List<SimpleEvent>>();
        var index = 0;

        foreach (var price in prices)
        {
            var endPeriod = price.TimeStamp.AddSeconds(PriceIntervalSeconds);

            if (prices.Count() > (index + 1))
            {
                var nextPrice = prices[index + 1];
                if (nextPrice.TimeStamp > endPeriod)
                    endPeriod = nextPrice.TimeStamp;
            }

            var eventsForThisPrice =
                events.Where(
                        e =>
                            ((e.TimeStamp >= price.START_PERIOD) && (e.TimeStamp < endPeriod) &&
                             eventSublist.Contains(e)))
                    .ToList();

            events.RemoveAll(
                e => ((e.TimeStamp >= price.TimeStamp) && (e.TimeStamp < endPeriod) && eventSublist.Contains(e)));

            eventsPerPriceUpdate.Add(eventsForThisPrice);

            index++;
        }

        return eventsPerPriceUpdate;
    }

    public static bool IsComplexEvent(string Id)
    {
        var bufferedEvent = CEPSite.Buffer.GetEvent(Id);

        if (bufferedEvent == null)
        {
            return CEPSite.Buffer_Overlay.GetEvent(Id) is ComplexEvent;
        }

        return CEPSite.Buffer.GetEvent(Id) is ComplexEvent;
    }

    public static string GetSearchHTML(SimpleEvent e)
    {
        string controlsHTML = "";

        controlsHTML = "<table class='SearchControls'>";


        // Copy Queue into a stack
        var stack = new Stack<SimpleEvent>(CEPSite.Buffer.EventBuffer);

        while (stack.Count >= 1)
        {
            var poppedEvent = stack.Pop();

            if (poppedEvent.GetType() == e.GetType())
            {
                e = poppedEvent;
                break;
            }
        }

        ObjectDumper.Dump(e);

        var typesParent = new ObjectDumper();
        var types = typesParent.TypesCloneFactory();

        foreach (var o in types)
        {
            controlsHTML += "<tr>";

            if (o.Value.Item1 == typeof(bool))
            {
                controlsHTML += "<td><b style='Color: blue'>" + o.Key + "</b></td><td style='padding-left:10px'><input type='radio' name='group_" +
                                o.Key + "' value='True'>True&nbsp;<input type='radio' name='group_" + o.Key +
                                "' value='False'>False<br><input type='radio' name='group_" + o.Key +
                                "' value='Either' checked>Either</td></tr>";
                continue;
            }

            if (o.Value.Item1 == typeof(DateTime))
            {
                controlsHTML += "<td><b style='Color: blue'>" + o.Key +
                                "</b></td><td style='padding-left:10px'><input type='text' name='DateTime_" + o.Key + "'>&nbsp;<select id='Select_" +
                                o.Key +
                                "'><option>&gt</option><option>&lt</option><option>&gt =</option><option>&lt =</option><option>=</option><option>Any</option selected></select></td></tr>";
                continue;
            }

            if (o.Value.Item1 == typeof(string[]))
            {
                controlsHTML += "<td><b style='Color: blue'>" + o.Key +
                                "</b></td><td colspan='2' style='padding-left:10px'><select id='Select_Multiple_" + o.Key + "' multiple>";
                for (int i = 0; i < ((string[]) o.Value.Item2).Length; i++)
                {
                    controlsHTML += "<option>" + ((string[]) o.Value.Item2)[i] + "</option>";
                }

                controlsHTML += "</select>&nbsp;<select id='Select_" + o.Key +
                                "'><option>any</option><option>does not contain</option><option>exact match</option><option>contains</option></select></td></tr>";
                continue;
            }

            if (o.Value.Item1 == typeof(int) || o.Value.Item1 == typeof(float) || o.Value.Item1 == typeof(double) ||
                o.Value.Item1 == typeof(long))
            {
                int int_max = 0;
                int int_min = 0;

                float float_max = 0;
                float float_min = 0;

                double double_max = 0;
                double double_min = 0;

                long long_max = 0;
                long long_min = 0;

                string Max = "";
                string Min = "";

                var buffer = CEPSite.Buffer.EventBuffer.Where(ev => ev.GetType() == e.GetType()).ToList();


                if (o.Value.Item1 == typeof(int))
                {
                    foreach (var ev in buffer)
                    {
                        var type = e.GetType();

                        var dynamicTypedObject = Convert.ChangeType(ev, type);
                        var value = GetPropValue(dynamicTypedObject, o.Key);
                        int_max = (int) value > int_max ? (int) value : int_max;
                        int_min = (int) value < int_min ? (int) value : int_min;
                    }

                    Max = int_max.ToString();
                    Min = int_min.ToString();
                }

                if (o.Value.Item1 == typeof(float))
                {
                    foreach (var ev in buffer)
                    {
                        var type = e.GetType();

                        var dynamicTypedObject = Convert.ChangeType(ev, type);
                        var value = GetPropValue(dynamicTypedObject, o.Key);
                        try
                        {
                            float_max = (float)value > int_max ? (float)value : int_max;
                            float_min = (float)value < int_min ? (float)value : int_min;
                        }
                        catch (Exception ex)
                        {
                        }

                    }

                    Max = float_max.ToString();
                    Min = float_min.ToString();
                }

                if (o.Value.Item1 == typeof(double))
                {
                    foreach (var ev in buffer)
                    {
                        var type = e.GetType();

                        var dynamicTypedObject = Convert.ChangeType(ev, type);
                        var value = GetPropValue(dynamicTypedObject, o.Key);
                        try
                        {
                            double_max = (double) value > int_max ? (double) value : int_max;
                            double_min = (double) value < int_min ? (double) value : int_min;
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    }

                    Max = double_max.ToString();
                    Min = double_min.ToString();
                }

                if (o.Value.Item1 == typeof(long))
                {
                    foreach (var ev in buffer)
                    {
                        var type = e.GetType();

                        var dynamicTypedObject = Convert.ChangeType(ev, type);
                        var value = GetPropValue(dynamicTypedObject, o.Key);
                        long_max = (long)value > int_max ? (long)value : int_max;
                        long_min = (long)value < int_min ? (long)value : int_min;
                    }

                    Max = long_max.ToString();
                    Min = long_min.ToString();
                }

                controlsHTML += "<td style='padding-left:10px'><b style='Color: blue'>" + o.Key +
                                "</b></td><td colspan='2' style='padding-left:10px'><input type='text' name='Int_" + o.Key + "'> Max:" + Max +
                                " Min:" + Min + "</td></tr>";
                continue;
            }

            controlsHTML += "</td><td style='padding-left:10px'><b style='Color: blue'>" + o.Key +
                            "</b></td><td colpan='2' style='padding-left:10px'><input type='text' name='Various_" + o.Key +
                            "'>&nbsp;<select id='Select_" + o.Key +
                            "'><option>contains</option><option>does not contain</option><option>exact match</option><option>Any</option selected></select></td></tr>";
        }

        return "<div class='EventControld'>" + controlsHTML + "</table><br><hr><br></div>";
    }

    public static string GetSearchHTML_OVERLAY(SimpleEvent e)
    {
        string controlsHTML = "";

        controlsHTML = "<table>";

        // Copy Queue into a stack
        var stack = new Stack<SimpleEvent>(CEPSite.Buffer_Overlay.EventBuffer);

        while (stack.Count >= 1)
        {
            var poppedEvent = stack.Pop();

            if (poppedEvent.GetType() == e.GetType())
            {
                e = poppedEvent;
                break;
            }
        }

        ObjectDumper.Dump(e);

        var typesParent = new ObjectDumper();
        var types = typesParent.TypesCloneFactory();

        foreach (var o in types)
        {
            controlsHTML += "<tr>";

            if (o.Value.Item1 == typeof(bool))
            {
                controlsHTML += "<td style='padding-left:10px'><b style='Color: blue'>" + o.Key + "</b></td><td style='padding-left:10px'><input type='radio' name='group_" +
                                o.Key + "' value='True'>True&nbsp;<input type='radio' name='group_" + o.Key +
                                "' value='False'>False<br><input type='radio' name='group_" + o.Key +
                                "' value='Either' checked>Either</td></tr>";
                continue;
            }

            if (o.Value.Item1 == typeof(DateTime))
            {
                controlsHTML += "<td style='padding-left:10px'><b style='Color: blue'>" + o.Key +
                                "</b></td><td style='padding-left:10px'><input type='text' name='DateTime_" + o.Key + "'>&nbsp;<select id='Select_" +
                                o.Key +
                                "'><option>&gt</option><option>&lt</option><option>&gt =</option><option>&lt =</option><option>=</option><option>Any</option selected></select></td></tr>";
                continue;
            }

            if (o.Value.Item1 == typeof(string[]))
            {
                controlsHTML += "<td style='padding-left:10px'><b style='Color: blue'>" + o.Key +
                                "</b></td><td colspan='2' style='padding-left:10px'><select id='Select_Multiple_" + o.Key + "' multiple>";
                for (int i = 0; i < ((string[])o.Value.Item2).Length; i++)
                {
                    controlsHTML += "<option>" + ((string[])o.Value.Item2)[i] + "</option>";
                }

                controlsHTML += "</select>&nbsp;<select id='Select_" + o.Key +
                                "'><option>any</option><option>does not contain</option><option>exact match</option><option>contains</option></select></td></tr>";
                continue;
            }

            if (o.Value.Item1 == typeof(int) || o.Value.Item1 == typeof(float) || o.Value.Item1 == typeof(double) ||
                o.Value.Item1 == typeof(long))
            {
                int int_max = 0;
                int int_min = 0;

                float float_max = 0;
                float float_min = 0;

                double double_max = 0;
                double double_min = 0;

                long long_max = 0;
                long long_min = 0;

                string Max = "";
                string Min = "";

                var buffer = CEPSite.Buffer_Overlay.EventBuffer.Where(ev => ev.GetType() == e.GetType()).ToList();


                if (o.Value.Item1 == typeof(int))
                {
                    foreach (var ev in buffer)
                    {
                        var type = e.GetType();

                        var dynamicTypedObject = Convert.ChangeType(ev, type);
                        var value = GetPropValue(dynamicTypedObject, o.Key);
                        int_max = (int)value > int_max ? (int)value : int_max;
                        int_min = (int)value < int_min ? (int)value : int_min;
                    }

                    Max = int_max.ToString();
                    Min = int_min.ToString();
                }

                if (o.Value.Item1 == typeof(float))
                {
                    foreach (var ev in buffer)
                    {
                        var type = e.GetType();

                        var dynamicTypedObject = Convert.ChangeType(ev, type);
                        var value = GetPropValue(dynamicTypedObject, o.Key);
                        try
                        {
                            float_max = (float)value > int_max ? (float)value : int_max;
                            float_min = (float)value < int_min ? (float)value : int_min;
                        }
                        catch (Exception ex)
                        {
                        }

                    }

                    Max = float_max.ToString();
                    Min = float_min.ToString();
                }

                if (o.Value.Item1 == typeof(double))
                {
                    foreach (var ev in buffer)
                    {
                        var type = e.GetType();

                        var dynamicTypedObject = Convert.ChangeType(ev, type);
                        var value = GetPropValue(dynamicTypedObject, o.Key);
                        try
                        {
                            double_max = (double)value > int_max ? (double)value : int_max;
                            double_min = (double)value < int_min ? (double)value : int_min;
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    Max = double_max.ToString();
                    Min = double_min.ToString();
                }

                if (o.Value.Item1 == typeof(long))
                {
                    foreach (var ev in buffer)
                    {
                        var type = e.GetType();

                        var dynamicTypedObject = Convert.ChangeType(ev, type);
                        var value = GetPropValue(dynamicTypedObject, o.Key);
                        long_max = (long)value > int_max ? (long)value : int_max;
                        long_min = (long)value < int_min ? (long)value : int_min;
                    }

                    Max = long_max.ToString();
                    Min = long_min.ToString();
                }

                controlsHTML += "<td style='padding-left:10px'><b style='Color: blue'>" + o.Key +
                                "</b></td><td colspan='2' style='padding-left:10px'><input type='text' name='Int_" + o.Key + "'> Max:" + Max +
                                " Min:" + Min + "</td></tr>";
                continue;
            }

            controlsHTML += "</td><td style='padding-left:10px'><b style='Color: blue'>" + o.Key +
                            "</b></td><td colpan='2' style='padding-left:10px'><input type='text' name='Various_" + o.Key +
                            "'>&nbsp;<select id='Select_" + o.Key +
                            "'><option>contains</option><option>does not contain</option><option>exact match</option><option>Any</option selected></select></td></tr>";
        }

        return "<div class='EventControld'>" + controlsHTML + "</table><br><hr><br></div>";
    }

    public static object GetPropValue(object src, string propName)
    {
        try
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        catch (Exception ex)
        {
            return 0;
        }
    }

    public static string GetEventHTML(string Id)
    {
        var bufferedEvent = CEPSite.Buffer.GetEvent(Id);

        if (bufferedEvent == null)
        {
            bufferedEvent = CEPSite.Buffer_Overlay.GetEvent(Id);
        }

        string eventHTML = "";
        string controlsHTML = "";

        if (bufferedEvent != null)
        {
            eventHTML = (IsComplexEvent(Id) ? COMPLEX_EVENT_MARKER : SIMPLE_EVENT_MARKER) + "<br>";
            eventHTML += ObjectDumper.Dump(bufferedEvent);
        }
        else
        {
            eventHTML = "Event " + Id + " is not in buffer";
        }

        return "<div class='EventDescription'>" + eventHTML + "<br><hr><br></div>";
    }

    public static string GetEventsPerPriceUpdateJSON(List<List<SimpleEvent>> eventsPerPriceUpdate)
    {
        string JSON = "";

        foreach (var eventList in eventsPerPriceUpdate)
        {
            string eventIds = "[";

            foreach (var eventDetails in eventList)
            {
                eventIds += "'" + eventDetails.Id + "', ";
            }

            eventIds =  eventIds == "[" ? "[]" : eventIds.Substring(0, eventIds.Length - 2) + "]";

            JSON += eventList.Count() + ", " + eventIds + ", ";

        }

        return "[" + JSON.Substring(0, JSON.Length - 2) + "]";
    }

    public static List<PriceUpdate> GeneratePaddedSortedPriceUpdates()
    {
        var prices = new List<PriceUpdate>(PriceUpdates);
        var paddedPriceUpdates = new List<PriceUpdate>();

        var pricesSorted = prices.OrderByDescending(t => t.TimeStamp);

        var lastDate_Modified = pricesSorted.FirstOrDefault().TimeStamp;
        var firstDate_Modified = pricesSorted.Last().TimeStamp;

        float lastPrice = 0;

        while (lastDate_Modified > firstDate_Modified)
        {
            var pricesForThisPeriod =
                prices.Where(
                    p =>
                        p.TimeStamp >= firstDate_Modified &&
                        p.TimeStamp < firstDate_Modified.AddSeconds(PriceIntervalSeconds)).ToList();

            if (!pricesForThisPeriod.Any())
            {
                var newPrice = new PriceUpdate()
                {
                    LIST_PRICE = lastPrice,
                    TimeStamp = firstDate_Modified
                };

                paddedPriceUpdates.Add(newPrice);
            }
            else
            {
                float sum = 0;

                pricesForThisPeriod.ForEach(p => sum += p.LIST_PRICE);

                var newPrice = new PriceUpdate()
                {
                    LIST_PRICE = sum / pricesForThisPeriod.Count,
                    TimeStamp = firstDate_Modified
                };

                paddedPriceUpdates.Add(newPrice);

                lastPrice = pricesForThisPeriod.Last().LIST_PRICE;
            }

            firstDate_Modified = firstDate_Modified.AddSeconds(PriceIntervalSeconds);
        }

        return paddedPriceUpdates.OrderBy(p => p.TimeStamp).ToList();
    }

    public static List<PriceUpdate> GeneratePaddedSortedPriceUpdates_Overlay(List<PriceUpdate> priceUpdates_Overlay)
    {
        var prices = new List<PriceUpdate>(priceUpdates_Overlay);
        var paddedPriceUpdates = new List<PriceUpdate>();

        var pricesSorted = prices.OrderByDescending(t => t.TimeStamp);

        var lastDate_Modified = pricesSorted.FirstOrDefault().TimeStamp;
        var firstDate_Modified = pricesSorted.Last().TimeStamp;

        float lastPrice = 0;

        while (lastDate_Modified > firstDate_Modified)
        {
            var pricesForThisPeriod =
                prices.Where(
                    p =>
                        p.TimeStamp >= firstDate_Modified &&
                        p.TimeStamp < firstDate_Modified.AddSeconds(PriceIntervalSeconds)).ToList();

            if (!pricesForThisPeriod.Any())
            {
                var newPrice = new PriceUpdate()
                {
                    LIST_PRICE = lastPrice,
                    TimeStamp = firstDate_Modified
                };

                paddedPriceUpdates.Add(newPrice);
            }
            else
            {
                float sum = 0;

                pricesForThisPeriod.ForEach(p => sum += p.LIST_PRICE);

                var newPrice = new PriceUpdate()
                {
                    LIST_PRICE = sum / pricesForThisPeriod.Count,
                    TimeStamp = firstDate_Modified
                };

                paddedPriceUpdates.Add(newPrice);

                lastPrice = pricesForThisPeriod.Last().LIST_PRICE;
            }

            firstDate_Modified = firstDate_Modified.AddSeconds(PriceIntervalSeconds);
        }

        return paddedPriceUpdates.OrderBy(p => p.TimeStamp).ToList();
    }

    public static IEnumerable<NewsUpdate> GetMatchesByProductName(PriceUpdate price, DateTime endPeriod)
    {
        return NewsUpdates.Where(s => ((s.STORY_DATE_TIME >= price.TimeStamp) && (s.STORY_DATE_TIME < endPeriod) && s.PRODUCTS.Contains(ProductName)));
    }
}