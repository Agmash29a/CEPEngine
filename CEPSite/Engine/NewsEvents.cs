using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public enum EVENT_TYPES { STORY_TAKE_OVERWRITE, ALERT, HEADLINE, STORY_TAKE_APPEND, DELETE }
public enum SENTIMENT_TYPES { GOOD, NEUTRAL, BAD }
public enum STORY_TYPES { S }

public class PriceUpdate : SimpleEvent
{
    public PriceUpdate()
    {
        Significance = "Price Update";
    }

    // Form
    public string SECURITY { get; set; }
    public DateTime DATE_OF_TRADE { get; set; }
    public DateTime START_PERIOD { get; set; }
    public DateTime END_PERIOD { get; set; }
    public int NUM_TRADE { get; set; }
    public float LIST_PRICE { get; set; }
    public float LIST_RETURN { get; set; }
    public float AVERAGE_PRICE { get; set; }
    public double TOTAL_VOLUME { get; set; }
    public double TOTAL_VALUE { get; set; }
    public double VWAP { get; set; }
    public string HEADLINE_TEXT { get; set; }
}

public class Sentiment : SimpleEvent
{
    public Sentiment() { }

    // Form
    public string STOCK_RIC { get; set; }
    public int SENTIMENT { get; set; }
    public string PNAC { get; set; }
}

public class NewsUpdate : SimpleEvent
{
    public NewsUpdate()
    {
        Significance = "News Update";
    }

    // Form
    public DateTime MSG_DATE { get; set; }

    public int MSG_TIME_MILLISECONDS { get; set; }
    public int MSG_TIME_SECONDS { get; set; }
    public int MSG_TIME_MINUTES { get; set; }
    public int MSG_TIME_HOURS { get; set; }

    public string UNIQUE_STORY_INDEX { get; set; }
    public EVENT_TYPES EVENT_TYPE { get; set; }
    public string PNAC { get; set; }
    public DateTime STORY_DATE_TIME { get; set; }
    public DateTime? TAKE_DATE_TIME { get; set; }
    public string HEADLINE_ALERT_TEXT { get; set; }
    public string ACCUMULATED_STORY_TEXT { get; set; }
    public string TAKE_TEXT { get; set; }
    public string[] PRODUCTS { get; set; }
    public string[] TOPICS { get; set; }
    public string[] RELATED_RICS { get; set; }
    public string[] NAMED_ITEMS { get; set; }
    public int? HEADLINE_SUBTYPE { get; set; }
    public string STORY_TYPE { get; set; }
    public bool TABULAR_FLAG { get; set; }
    public string ATTRIBUTION { get; set; }
    public string LANGUAGE { get; set; }
    public SENTIMENT_TYPES SENTIMENT { get; set; }
}

public class PriceChange : ComplexEvent
{
    public PriceUpdate PriceUpdate1;
    public PriceUpdate PriceUpdate2;

    public PriceChange(PriceUpdate price1, PriceUpdate price2, Pattern<DAG<SimpleEvent>> pattern)
    {
        AssignThreeNodeDAG(price1, price2, pattern);

        this.PriceUpdate1 = price1;
        this.PriceUpdate2 = price2;
    }
}

public class PriceJump : PriceChange
{
    public PriceJump(PriceUpdate price1, PriceUpdate price2, Pattern<DAG<SimpleEvent>> pattern)
        : base(price1, price2, pattern)
    {
        TimeStamp = price2.TimeStamp;
        Significance = "Price Jump";
    }
}

public class PriceFall : PriceChange
{
    public PriceFall(PriceUpdate price1, PriceUpdate price2, Pattern<DAG<SimpleEvent>> pattern)
        : base(price1, price2, pattern)
    {
        TimeStamp = price2.TimeStamp;
        Significance = "Price Fall";
    }
}

public class NewsUpdateChange : ComplexEvent
{
    public PriceJump PriceJump;
    public PriceFall PriceFall;

    public NewsUpdate NewsUpdate;

    public NewsUpdateChange(NewsUpdate newsUpdate, PriceJump priceJump, Pattern<DAG<SimpleEvent>> pattern)
    {
        this.PriceJump = priceJump;
        this.NewsUpdate = newsUpdate;

        AssignThreeNodeDAG(newsUpdate, priceJump, pattern);
    }
    public NewsUpdateChange(NewsUpdate newsUpdate, PriceFall priceFall, Pattern<DAG<SimpleEvent>> pattern)
    {
        this.PriceFall = priceFall;
        this.NewsUpdate = newsUpdate;

        AssignThreeNodeDAG(newsUpdate, priceFall, pattern);
    }
}

public class NewsUpdatePlus : NewsUpdateChange
{
    public NewsUpdatePlus(NewsUpdate newsUpdate, PriceJump priceJump, Pattern<DAG<SimpleEvent>> pattern) 
        : base(newsUpdate, priceJump, pattern)
    {
        TimeStamp = priceJump.TimeStamp;
        Significance = "News Update Plus";
    }
}

public class NewsUpdateMinus : NewsUpdateChange
{
    public NewsUpdateMinus(NewsUpdate newsUpdate, PriceFall priceFall, Pattern<DAG<SimpleEvent>> pattern)
        : base(newsUpdate, priceFall, pattern)
    {
        TimeStamp = priceFall.TimeStamp;
        Significance = "News Update Minus";
    }
}

public class NewsStoryData : ComplexEvent
{
    public NewsUpdate NewsUpdate1;
    public NewsUpdate NewsUpdate2;

    // Form
    public DateTime StoryStartTimeStamp;
    public DateTime StoryEndTimeStamp;

    public NewsStoryData(NewsUpdate newsUpdate1, NewsUpdate newsUpdate2, Pattern<DAG<SimpleEvent>> pattern)
    {
        this.NewsUpdate1 = newsUpdate1;
        this.NewsUpdate2 = newsUpdate2;
    }
}

public class NewsStoryChange : NewsStoryData
{
    public PriceUpdate PriceUpdate1;
    public PriceUpdate PriceUpdate2;

    public NewsStory NewsStory;

    public NewsStoryChange(PriceUpdate priceUpdate1, PriceUpdate priceUpdate2, NewsStory newsStory, Pattern<DAG<SimpleEvent>> pattern)
        : base(newsStory.NewsUpdate1, newsStory.NewsUpdate2, pattern)
    {
        this.PriceUpdate1 = priceUpdate1;
        this.PriceUpdate2 = priceUpdate2;

        this.NewsStory = newsStory;

        AssignFourNodeDAG(priceUpdate1, priceUpdate2, newsStory, pattern);
    }
}

public class NewsStory : NewsStoryData
{
    public NewsStory(NewsUpdate newsUpdate1, NewsUpdate newsUpdate2, Pattern<DAG<SimpleEvent>> pattern) 
        : base(newsUpdate1, newsUpdate2, pattern)
    {
        AssignThreeNodeDAG(newsUpdate1, newsUpdate2, pattern);

        StoryStartTimeStamp = newsUpdate1.TimeStamp;
        StoryEndTimeStamp = newsUpdate2.TimeStamp;
        TimeStamp = newsUpdate2.TimeStamp;

        Significance = "News Story";
    }
}

public class NewsStoryPlus : NewsStoryChange
{
    public NewsStoryPlus(PriceUpdate earlierPriceUpdate, PriceUpdate laterPriceUpdate, NewsStory newsStory, Pattern<DAG<SimpleEvent>> pattern) 
        : base (earlierPriceUpdate, laterPriceUpdate, newsStory, pattern)
    {
        TimeStamp = laterPriceUpdate.TimeStamp;

        Significance = "News Story Plus";
    }
}

public class NewsStoryMinus : NewsStoryChange
{
    public NewsStoryMinus(PriceUpdate earlierPriceUpdate, PriceUpdate laterPriceUpdate, NewsStory newsStory, Pattern<DAG<SimpleEvent>> pattern) 
        : base (earlierPriceUpdate, laterPriceUpdate, newsStory, pattern)
    {
        TimeStamp = laterPriceUpdate.TimeStamp;

        Significance = "News Story MInus";
    }
}