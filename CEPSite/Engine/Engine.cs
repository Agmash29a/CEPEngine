using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Model;

public static class Engine
{
    const int BUFFER_SIZE = 100000;
    public static int PRICE_INTERVAL_SECONDS = 60;
    const int MODIFIER = 0;
    public static string PRODUCT_NAME = "GER";
    private const int NUMBER_OF_DATES = 5;

    public static int Lowest_Y = int.MaxValue;
    public static int Highest_Y = 0;

    // JSON Arrays

    public static string JSON_DateLabels { get; set; }
    public static string JSON_NumberOfPNACsPerPriceUpdate { get; set; }
    public static string JSON_NumberOfPNACsPerPriceUpdate_Overlay { get; set; }

    // JSON Events

    public static string JSON_PriceUpdates { get; set; }

    public static string JSON_NumberOfNewsUpdatesPerPriceUpdate { get; set; }
    public static string JSON_NumberOfRestrictedPriceUpdates { get; set; }
    public static string JSON_NumberOfPositiveSentimentPerPriceUpdate { get; set; }
    public static string JSON_NumberOfNegativeSentimentPerPriceUpdate { get; set; }
    public static string JSON_NumberOfPriceJumpsPerPriceUpdate { get; set; }
    public static string JSON_NumberOfPriceFallsPerPriceUpdate { get; set; }
    public static string JSON_NumberOfNewsStoryMinusPerPriceUpdate { get; set; }
    public static string JSON_NumberOfNewsStoryPlusPerPriceUpdate { get; set; }
    public static string JSON_NumberOfNewsUpdateMinusPerPriceUpdate { get; set; }
    public static string JSON_NumberOfNewsUpdatePlusPerPriceUpdate { get; set; }
    public static string JSON_NumberOfNewsStoriesPerPriceUpdate { get; set; }

    public static string JSON_PriceUpdates_Overlay { get; set; }

    // Active Overlay 1

    public static string JSON_NumberOfNewsUpdatesPerPriceUpdate_Overlay { get; set; }
    public static string JSON_NumberOfRestrictedPriceUpdates_Overlay { get; set; }
    public static string JSON_NumberOfPositiveSentimentPerPriceUpdate_Overlay { get; set; }
    public static string JSON_NumberOfNegativeSentimentPerPriceUpdate_Overlay { get; set; }
    public static string JSON_NumberOfPriceJumpsPerPriceUpdate_Overlay { get; set; }
    public static string JSON_NumberOfPriceFallsPerPriceUpdate_Overlay { get; set; }
    public static string JSON_NumberOfNewsStoryMinusPerPriceUpdate_Overlay { get; set; }
    public static string JSON_NumberOfNewsStoryPlusPerPriceUpdate_Overlay { get; set; }
    public static string JSON_NumberOfNewsUpdateMinusPerPriceUpdate_Overlay { get; set; }
    public static string JSON_NumberOfNewsUpdatePlusPerPriceUpdate_Overlay { get; set; }
    public static string JSON_NumberOfNewsStoriesPerPriceUpdate_Overlay { get; set; }

    // Other Active Overlays go here

    public static int NumPriceUpdates { get; set; }
    public static int NumNewsUpdates { get; set; }
    public static int NumNegativeSentiment { get; set; }
    public static int NumPositiveSentiment { get; set; }
    public static int NumPriceJumps { get; set; }
    public static int NumNewsUpdatePlus { get; set; }
    public static int NumPriceFalls { get; set; }
    public static int NumNewsUpdateMinus { get; set; }
    public static int NumNewsStories { get; set; }
    public static int NumNewsStoryPlus { get; set; }
    public static int NumNewsStoryMinus { get; set; }

    public static int NumPriceUpdates_Overlay { get; set; }
    public static int NumNewsUpdates_Overlay { get; set; }
    public static int NumNegativeSentiment_Overlay { get; set; }
    public static int NumPositiveSentiment_Overlay { get; set; }
    public static int NumPriceJumps_Overlay { get; set; }
    public static int NumNewsUpdatePlus_Overlay { get; set; }
    public static int NumPriceFalls_Overlay { get; set; }
    public static int NumNewsUpdateMinus_Overlay { get; set; }
    public static int NumNewsStories_Overlay { get; set; }
    public static int NumNewsStoryPlus_Overlay { get; set; }
    public static int NumNewsStoryMinus_Overlay { get; set; }

    public static bool ShowPriceUpdates { get; set; }
    public static bool ShowNewsUpdates { get; set; }
    public static bool ShowPositiveSentiment { get; set; }
    public static bool ShowNegativeSentiment { get; set; }
    public static bool ShowPriceJumps { get; set; }
    public static bool ShowNewsUpdatePlus { get; set; }
    public static bool ShowPriceFalls { get; set; }
    public static bool ShowNewsUpdateMinus { get; set; }
    public static bool ShowNewsStories { get; set; }
    public static bool ShowNewsStoryPlus { get; set; }
    public static bool ShowNewsStoryMinus { get; set; }

    public static double PriceChangedPositiveWithNewsUpdate_THRESHHOLD { get; set; }
    public static double PriceChangedPositive_THRESHHOLD { get; set; }
    public static double PriceChangedNegativeWithNewsUpdate_THRESHHOLD { get; set; }
    public static double PriceChangedNegative_THRESHHOLD { get; set; }

    public static int NumberOfDates { get; set; }

    public static void ReadInData()
    {
        Parser.ParseNewsFile();
        Parser.ParsePricesFile();
        Parser.InsertNewsIntoDB();
    }

    public static void Initialise(Page page, DateTime startDate, DateTime endDate, string RIC, int priceInterval)
	{
        // Set up Parser
        Parser.NewsUpdates = new List<NewsUpdate>();
        Parser.PriceUpdates = new List<PriceUpdate>();
        Parser.PositiveSentimentValues = new List<Sentiment>();
        Parser.NegativeSentimentValues = new List<Sentiment>();

        CEPSite.Buffer.EventBuffer.Clear();

        Parser.PriceIntervalSeconds = priceInterval;
        Parser.Modifier = MODIFIER;
        Parser.ProductName = PRODUCT_NAME;

        var dal = DataAccess.Instance;

        Parser.PriceUpdates = dal.GetPricesByDateRangeAndRIC(startDate, endDate, RIC);
	    Parser.NewsUpdates = dal.GetNewsByDateRangeAndRIC(startDate, endDate, RIC);

        // Set Relativity
	    var activePatterns = ActivePatterns<DAG<SimpleEvent>>.Patterns;

        //Since Price updates may not start until after the startDate we need to pad the first price update
        Parser.PriceUpdates = Parser.PriceUpdates.OrderBy(t => t.TimeStamp).ToList();
        Parser.PriceUpdates[0].TimeStamp = Parser.PriceUpdates[0].TimeStamp > startDate ? startDate : Parser.PriceUpdates[0].TimeStamp;

        //TODO - special case where thee are no price updates

        Parser.PositiveSentimentValues = dal.GetSentimentByDateRangeAndRIC(startDate, endDate, RIC);
        Parser.NegativeSentimentValues = dal.GetNegativeSentimentByDateRangeAndRIC(startDate, endDate, RIC);

        // VI related
        var priceUpdates = Parser.GeneratePaddedSortedPriceUpdates();

        NumberOfDates = NUMBER_OF_DATES;

        JSON_DateLabels = Parser.GetDateLabelsJSON(NumberOfDates, priceUpdates.ToList());
        JSON_PriceUpdates = Parser.GetSortedPricesJSON(priceUpdates.ToList());
        JSON_NumberOfPNACsPerPriceUpdate = Parser.GetNumberOfPNACsPerPriceUpdateJSON(priceUpdates.ToList());

        // Add Patterns

        var p1 = new PriceChangedPositiveWithNewsUpdate(PriceChangedPositiveWithNewsUpdate_THRESHHOLD);
        var p2 = new PriceChangedPositive(PriceChangedPositive_THRESHHOLD);
        var p3 = new PriceChangedNegativeWithNewsUpdate(PriceChangedNegativeWithNewsUpdate_THRESHHOLD);
        var p4 = new PriceChangedNegative(PriceChangedNegative_THRESHHOLD);
        var p5 = new BasicStory();
        var p6 = new PriceChangedPositiveBasicStory();
        var p7 = new PriceChangedNegativeBasicStory();

	    activePatterns.Clear();

        CEPSite.Buffer.BufferSize = BUFFER_SIZE;

        // Add Complex event pattern definitions

        if (ShowNewsUpdatePlus)
            activePatterns.Add(p1);

        if (ShowPriceJumps)
            activePatterns.Add(p2);

        if (ShowNewsUpdateMinus)
            activePatterns.Add(p3);

        if (ShowPriceFalls)
            activePatterns.Add(p4);

        if (ShowNewsStories)
            activePatterns.Add(p5);

        if (ShowNewsStoryPlus)
            activePatterns.Add(p6);

        if (ShowNewsStoryMinus)
            activePatterns.Add(p7);

        SimulateStream();
	}

    public static void SimulateStream()
    {
        var eventSource = new List<SimpleEvent>();

        // find highest price/lowest Y
        Highest_Y = 0;
        Lowest_Y = int.MaxValue;

        Parser.PriceUpdates.ForEach(p =>
        {
            if (p.LIST_PRICE > Highest_Y)
                Highest_Y = (int)Math.Ceiling(p.LIST_PRICE);

            if (p.LIST_PRICE < Lowest_Y)
                Lowest_Y = (int)Math.Floor(p.LIST_PRICE);
        } );

        // Add Simple Eevent Feeds

        if (ShowPriceUpdates)
            eventSource.AddRange(Parser.PriceUpdates);

        if (ShowNewsUpdates)
            eventSource.AddRange(Parser.NewsUpdates);

        if (ShowPositiveSentiment)
            eventSource.AddRange(Parser.NegativeSentimentValues);

        if (ShowNegativeSentiment)
            eventSource.AddRange(Parser.PositiveSentimentValues);

        eventSource.Sort((x, y) => DateTime.Compare(x.TimeStamp, y.TimeStamp));

        foreach (var sourceEvent in eventSource)
        {
            EventConfig.SetOffset(sourceEvent.TimeStamp);
            CEPSite.Buffer.AddToEventBuffer(sourceEvent);
        }
    }

    public static void ShowAllEvents()
    {
        NumPriceUpdates =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(PriceUpdate));
        NumNewsUpdates =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(NewsUpdate));
        NumNegativeSentiment =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == -1);
        NumPositiveSentiment =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == 1);
        NumPriceJumps =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(PriceJump));
        NumNewsUpdatePlus =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(NewsUpdatePlus));
        NumPriceFalls =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(PriceFall));
        NumNewsUpdateMinus =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(NewsUpdateMinus));
        NumNewsStories =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(NewsStory));
        NumNewsStoryPlus =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(NewsStoryPlus));
        NumNewsStoryMinus =CEPSite.Buffer.EventBuffer.Count(e => e.GetType() == typeof(NewsStoryMinus));

        var paddedPriceUpdates = Parser.GeneratePaddedSortedPriceUpdates();

        JSON_NumberOfRestrictedPriceUpdates = ShowPriceUpdates ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceUpdate)).ToList())) : "[]";
        JSON_NumberOfNewsUpdatesPerPriceUpdate = ShowNewsUpdates ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdate)).ToList())) : "[]";
        JSON_NumberOfPositiveSentimentPerPriceUpdate = ShowPositiveSentiment ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == 1).ToList())) : "[]";
        JSON_NumberOfNegativeSentimentPerPriceUpdate = ShowNegativeSentiment ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == -1).ToList())) : "[]";
        JSON_NumberOfPriceJumpsPerPriceUpdate = ShowPriceJumps ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceJump)).ToList())) : "[]";
        JSON_NumberOfPriceFallsPerPriceUpdate = ShowPriceFalls ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceFall)).ToList())) : "[]";
        JSON_NumberOfNewsStoryMinusPerPriceUpdate = ShowNewsStoryMinus ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryMinus)).ToList())) : "[]";
        JSON_NumberOfNewsStoryPlusPerPriceUpdate = ShowNewsStoryPlus ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryPlus)).ToList())) : "[]";
        JSON_NumberOfNewsUpdateMinusPerPriceUpdate = ShowNewsUpdateMinus ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdateMinus)).ToList())) : "[]";
        JSON_NumberOfNewsUpdatePlusPerPriceUpdate = ShowNewsUpdatePlus ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdatePlus)).ToList())) : "[]";
        JSON_NumberOfNewsStoriesPerPriceUpdate = ShowNewsStories ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStory)).ToList())) : "[]";
    }

    public static void ShowAllEvents_Overlay()
    {
        var priceUpdates = new List<PriceUpdate>();

        CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(PriceUpdate)).ToList().ForEach(p => priceUpdates.Add((PriceUpdate)p));

        priceUpdates.ForEach(p =>
        {
            if (p.LIST_PRICE > Highest_Y)
                Highest_Y = (int)Math.Ceiling(p.LIST_PRICE);

            if (p.LIST_PRICE < Lowest_Y)
                Lowest_Y = (int)Math.Floor(p.LIST_PRICE);
        });

        JSON_PriceUpdates_Overlay = Parser.GetSortedPricesJSON(priceUpdates.ToList());
        JSON_NumberOfPNACsPerPriceUpdate_Overlay = Parser.GetNumberOfPNACsPerPriceUpdateJSON(priceUpdates.ToList());

        NumPriceUpdates_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(PriceUpdate));
        NumNewsUpdates_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(NewsUpdate));
        NumNegativeSentiment_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == -1);
        NumPositiveSentiment_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == 1);
        NumPriceJumps_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(PriceJump));
        NumNewsUpdatePlus_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(NewsUpdatePlus));
        NumPriceFalls_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(PriceFall));
        NumNewsUpdateMinus_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(NewsUpdateMinus));
        NumNewsStories_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(NewsStory));
        NumNewsStoryPlus_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(NewsStoryPlus));
        NumNewsStoryMinus_Overlay = CEPSite.Buffer_Overlay.EventBuffer.Count(e => e.GetType() == typeof(NewsStoryMinus));

        var paddedPriceUpdates_Overlay = Parser.GeneratePaddedSortedPriceUpdates_Overlay(priceUpdates.ToList());

        JSON_NumberOfRestrictedPriceUpdates_Overlay = ShowPriceUpdates ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(PriceUpdate)).ToList())) : "[]";
        JSON_NumberOfNewsUpdatesPerPriceUpdate_Overlay = ShowNewsUpdates ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdate)).ToList())) : "[]";
        JSON_NumberOfPositiveSentimentPerPriceUpdate_Overlay = ShowPositiveSentiment ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == 1).ToList())) : "[]";
        JSON_NumberOfNegativeSentimentPerPriceUpdate_Overlay = ShowNegativeSentiment ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == -1).ToList())) : "[]";
        JSON_NumberOfPriceJumpsPerPriceUpdate_Overlay = ShowPriceJumps ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(PriceJump)).ToList())) : "[]";
        JSON_NumberOfPriceFallsPerPriceUpdate_Overlay = ShowPriceFalls ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(PriceFall)).ToList())) : "[]";
        JSON_NumberOfNewsStoryMinusPerPriceUpdate_Overlay = ShowNewsStoryMinus ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryMinus)).ToList())) : "[]";
        JSON_NumberOfNewsStoryPlusPerPriceUpdate_Overlay = ShowNewsStoryPlus ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryPlus)).ToList())) : "[]";
        JSON_NumberOfNewsUpdateMinusPerPriceUpdate_Overlay = ShowNewsUpdateMinus ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdateMinus)).ToList())) : "[]";
        JSON_NumberOfNewsUpdatePlusPerPriceUpdate_Overlay = ShowNewsUpdatePlus ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdatePlus)).ToList())) : "[]";
        JSON_NumberOfNewsStoriesPerPriceUpdate_Overlay = ShowNewsStories ? Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates_Overlay, CEPSite.Buffer_Overlay.EventBuffer.Where(e => e.GetType() == typeof(NewsStory)).ToList())) : "[]";
    }

    public static bool ShowSingleComplexEvent(string Id)
    {
        var eventInstance = CEPSite.Buffer.EventBuffer.First(e => e.Id == Id);

        if (eventInstance is ComplexEvent)
        {
            var paddedPriceUpdates = Parser.GeneratePaddedSortedPriceUpdates();

            NumPriceUpdates = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceUpdate)).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();
            NumNewsUpdates = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdate)).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();
            NumNegativeSentiment = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == -1).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();
            NumPositiveSentiment = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == 1).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();
            NumPriceJumps = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceJump)).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();
            NumNewsUpdatePlus = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdatePlus)).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();
            NumPriceFalls = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceFall)).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();
            NumNewsUpdateMinus = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdateMinus)).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();
            NumNewsStories = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStory)).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();
            NumNewsStoryPlus = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryPlus)).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();
            NumNewsStoryMinus = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryMinus)).ToList(), (ComplexEvent)eventInstance).SelectMany(i => i).Count();

            JSON_NumberOfRestrictedPriceUpdates = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceUpdate)).ToList(), (ComplexEvent)eventInstance));
            JSON_NumberOfNewsUpdatesPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdate)).ToList(), (ComplexEvent)eventInstance));
            JSON_NumberOfNegativeSentimentPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == -1).ToList(), (ComplexEvent)eventInstance));
            JSON_NumberOfPositiveSentimentPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == 1).ToList(), (ComplexEvent)eventInstance));
            JSON_NumberOfPriceJumpsPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceJump)).ToList(), (ComplexEvent)eventInstance));
            JSON_NumberOfPriceFallsPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceFall)).ToList(), (ComplexEvent)eventInstance));
            JSON_NumberOfNewsStoryMinusPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryMinus)).ToList(), (ComplexEvent)eventInstance));
            JSON_NumberOfNewsStoryPlusPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryPlus)).ToList(), (ComplexEvent)eventInstance));
            JSON_NumberOfNewsUpdateMinusPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdateMinus)).ToList(), (ComplexEvent)eventInstance));
            JSON_NumberOfNewsUpdatePlusPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdatePlus)).ToList(), (ComplexEvent)eventInstance));
            JSON_NumberOfNewsStoriesPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStory)).ToList(), (ComplexEvent)eventInstance));
        }
        else
        {
            return false;
        }

        return true;
    }

    public static void ResetNumbersOfEvents()
    {
        NumPriceUpdates = 0;
        NumNewsUpdates = 0;
        NumNegativeSentiment = 0;
        NumPositiveSentiment = 0;
        NumPriceJumps = 0;
        NumNewsUpdatePlus = 0;
        NumPriceFalls = 0;
        NumNewsUpdateMinus = 0;
        NumNewsStories = 0;
        NumNewsStoryPlus = 0;
        NumNewsStoryMinus = 0;
    }

    public static bool ShowSingleComplexEventWithNestedTypes(string Id)
    {
        var eventInstance =CEPSite.Buffer.EventBuffer.First(e => e.Id == Id);

        if (!(eventInstance is ComplexEvent))
            return false;

        List<string> eventIds = ObjectDumper.GetNestedIDs(CEPSite.Buffer.EventBuffer.First(e => e.Id == Id));
        List<SimpleEvent> eventInstances =CEPSite.Buffer.EventBuffer.Where(e => eventIds.Contains(e.Id)).ToList();

        var paddedPriceUpdates = Parser.GeneratePaddedSortedPriceUpdates();

        NumPriceUpdates = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceUpdate)).ToList(), eventInstances).SelectMany(i => i).Count();
        NumNewsUpdates = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdate)).ToList(), eventInstances).SelectMany(i => i).Count();
        NumNegativeSentiment = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == -1).ToList(), eventInstances).SelectMany(i => i).Count();
        NumPositiveSentiment = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == 1).ToList(), eventInstances).SelectMany(i => i).Count();
        NumPriceJumps = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceJump)).ToList(), eventInstances).SelectMany(i => i).Count();
        NumNewsUpdatePlus = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdatePlus)).ToList(), eventInstances).SelectMany(i => i).Count();
        NumPriceFalls = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceFall)).ToList(), eventInstances).SelectMany(i => i).Count();
        NumNewsUpdateMinus = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdateMinus)).ToList(), eventInstances).SelectMany(i => i).Count();
        NumNewsStories = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStory)).ToList(), eventInstances).SelectMany(i => i).Count();
        NumNewsStoryPlus = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryPlus)).ToList(), eventInstances).SelectMany(i => i).Count();
        NumNewsStoryMinus = Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryMinus)).ToList(), eventInstances).SelectMany(i => i).Count();

        JSON_NumberOfRestrictedPriceUpdates = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceUpdate)).ToList(), eventInstances));
        JSON_NumberOfNewsUpdatesPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdate)).ToList(), eventInstances));
        JSON_NumberOfPositiveSentimentPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == 1).ToList(), eventInstances));
        JSON_NumberOfNegativeSentimentPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(Sentiment) && ((Sentiment)e).SENTIMENT == -1).ToList(), eventInstances));
        JSON_NumberOfPriceJumpsPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceJump)).ToList(), eventInstances));
        JSON_NumberOfPriceFallsPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(PriceFall)).ToList(), eventInstances));
        JSON_NumberOfNewsStoryMinusPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryMinus)).ToList(), eventInstances));
        JSON_NumberOfNewsStoryPlusPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStoryPlus)).ToList(), eventInstances));
        JSON_NumberOfNewsUpdateMinusPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdateMinus)).ToList(), eventInstances));
        JSON_NumberOfNewsUpdatePlusPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsUpdatePlus)).ToList(), eventInstances));
        JSON_NumberOfNewsStoriesPerPriceUpdate = Parser.GetEventsPerPriceUpdateJSON(Parser.GetEventsPerPriceUpdate(paddedPriceUpdates,CEPSite.Buffer.EventBuffer.Where(e => e.GetType() == typeof(NewsStory)).ToList(), eventInstances));

        return true;
    }
}