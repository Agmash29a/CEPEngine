using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class PriceChangedPositive : Pattern
{
    public PriceChangedPositive()
    {
        LastPriceUpdate = null;
    }

    public static double Threshold = 0.0005;
    public static PriceUpdate LastPriceUpdate;

    public override bool Match(ref ComplexEvent complexEvent, SimpleEvent simpleEvent)
    {
        // ---> STEP 1 - Check if the incoming event is of the right type
        if (simpleEvent.GetType() != typeof(PriceUpdate))
        {
            return false;
        }
        // ---> STEP 2 - Scan for matches, run through most recent queue entries 
        // and find the most recent price update before the current one, then compare.

        if (LastPriceUpdate == null)
        {
            LastPriceUpdate = simpleEvent as PriceUpdate;
            return false;
        }

        var priceUpdate2 = simpleEvent as PriceUpdate;
        PriceUpdate earlierPriceUpdate = null;

        if (priceUpdate2 != null)
        {
            if (priceUpdate2.TimeStamp > LastPriceUpdate.TimeStamp)
            {
                earlierPriceUpdate = LastPriceUpdate;
                LastPriceUpdate = priceUpdate2;
            }
            else
            {
                return false;
            }
        }

        if (earlierPriceUpdate != null)
        {
            if (IsPriceIncrease(earlierPriceUpdate, priceUpdate2))
            {
                // Create a price jump object
                complexEvent = new PriceJump(earlierPriceUpdate, priceUpdate2, this);

                return true;
            }
        }

        return false;
    }

    private static bool IsPriceIncrease(PriceUpdate p1, PriceUpdate p2)
    {
        return (p2.LIST_PRICE - p1.LIST_PRICE) > (Threshold * p1.LIST_PRICE);
    }
}

public class PriceChangedNegative : Pattern
{
    public PriceChangedNegative()
    {
        LastPriceUpdate = null;
    }

    public static double Threshold = 0.0005;
    public static PriceUpdate LastPriceUpdate;

    public override bool Match(ref ComplexEvent complexEvent, SimpleEvent simpleEvent)
    {
        // ---> STEP 1 - Check if the incoming event is of the right type
        if (simpleEvent.GetType() != typeof(PriceUpdate))
        {
            return false;
        }
        // ---> STEP 2 - Scan for matches, run through most recent queue entries 
        // and find the most recent price update before the current one, then compare.

        if (LastPriceUpdate == null)
        {
            LastPriceUpdate = simpleEvent as PriceUpdate;
            return false;
        }

        var priceUpdate2 = simpleEvent as PriceUpdate;
        PriceUpdate earlierPriceUpdate = null;

        if (priceUpdate2 != null)
        {
            if (priceUpdate2.TimeStamp > LastPriceUpdate.TimeStamp)
            {
                earlierPriceUpdate = LastPriceUpdate;
                LastPriceUpdate = priceUpdate2;
            }
            else
            {
                return false;
            }
        }

        if (earlierPriceUpdate != null)
        {
            if (IsPriceDecrease(earlierPriceUpdate, priceUpdate2))
            {
                // Create a price jump object
                complexEvent = new PriceFall(earlierPriceUpdate, priceUpdate2, this);

                return true;
            }
        }

        return false;
    }


    private static bool IsPriceDecrease(PriceUpdate p1, PriceUpdate p2)
    {
        return (p1.LIST_PRICE - p2.LIST_PRICE) > (Threshold * p1.LIST_PRICE);
    }
}

public class PriceChangedNegativeWithNewsUpdate : Pattern
{
    public double NewsUpdateThreshold = 30; // 30 minutes

    public override bool Match(ref ComplexEvent complexEvent, SimpleEvent simpleEvent)
    {
        // ---> STEP 1 - Check if the incoming event is of the right type
        if (simpleEvent.GetType() != typeof(PriceFall))
        {
            return false;
        }

        // ---> STEP 2 - Scan for matches, run through most recent queue entries 
        // and find the most recent news update before the current price jump.

        // Copy Queue into a stack
        var stack = new Stack<SimpleEvent>(Buffer.EventBuffer);

        NewsUpdate earlierNewsUpdate = null;
        var priceFall = simpleEvent as PriceFall;

        if (priceFall != null)
        {
            while (stack.Count >= 1)
            {
                var poppedEvent = stack.Pop();

                if (!(poppedEvent is NewsUpdate)) continue;

                if (priceFall.TimeStamp > poppedEvent.TimeStamp)
                {
                    if (priceFall.TimeStamp > poppedEvent.TimeStamp.AddMinutes(NewsUpdateThreshold))
                    {
                        return false;
                    }

                    earlierNewsUpdate = (NewsUpdate) poppedEvent;
                    break;
                }
            }
        }

        if (earlierNewsUpdate != null)
        {
            // Create a NewsUpdatePlus object
            complexEvent = new NewsUpdateMinus(earlierNewsUpdate, priceFall, this);

            return true;
        }

        return false;
    }
}

public class PriceChangedPositiveWithNewsUpdate : Pattern
{
    public double NewsUpdateThreshold = 30; // 30 minutes

    public override bool Match(ref ComplexEvent complexEvent, SimpleEvent simpleEvent)
    {
        // ---> STEP 1 - Check if the incoming event is of the right type
        if (simpleEvent.GetType() != typeof(PriceJump))
        {
            return false;
        }

        // ---> STEP 2 - Scan for matches, run through most recent queue entries 
        // and find the most recent news update before the current price jump.

        // Copy Queue into a stack
        var stack = new Stack<SimpleEvent>(Buffer.EventBuffer);

        NewsUpdate earlierNewsUpdate = null;
        var priceJump = simpleEvent as PriceJump;

        if (priceJump != null)
        {
            while (stack.Count >= 1)
            {
                var poppedEvent = stack.Pop();

                if (!(poppedEvent is NewsUpdate)) continue;

                if (priceJump.TimeStamp > poppedEvent.TimeStamp)
                {
                    if (priceJump.TimeStamp > poppedEvent.TimeStamp.AddMinutes(NewsUpdateThreshold))
                    {
                        return false;
                    }

                    earlierNewsUpdate = (NewsUpdate)poppedEvent;
                    break;
                }
            }
        }

        if (earlierNewsUpdate != null)
        {
            // Create a NewsUpdatePlus object
            complexEvent = new NewsUpdatePlus(earlierNewsUpdate, priceJump, this);

            return true;
        }

        return false;
    }
}

public class BasicStory : Pattern
{
    public List<string> PNACs = new List<string>();

    public override bool Match(ref ComplexEvent complexEvent, SimpleEvent simpleEvent)
    {
        // ---> STEP 1 - Check if the incoming event is of the right type
        if (simpleEvent.GetType() != typeof(NewsUpdate))
        {
            return false;
        }

        // ---> STEP 2 - Scan for first matching PNAC

        // Copy Queue into a List
        var list = new List<SimpleEvent>(Buffer.EventBuffer);

        var newsUpdate2 = simpleEvent as NewsUpdate;
        NewsUpdate earlierNewsUpdate = null;

        // An analysis of news items from the German stock market shows that most stories end with a single OVERWRITE event.
        if (newsUpdate2 != null && newsUpdate2.EVENT_TYPE == EVENT_TYPES.STORY_TAKE_OVERWRITE)
        {
            if (list.Count >= 1)
            {
                // Find first event with the same PNAC as the story with the OVERWRITE
                earlierNewsUpdate = (NewsUpdate)list.DefaultIfEmpty(newsUpdate2).FirstOrDefault(e => ((e.GetType() == typeof(NewsUpdate)) && ((NewsUpdate)e).PNAC == newsUpdate2.PNAC));
            }
        }

        // If a news story has not already been generated for this PNAC then generate new event
        if (earlierNewsUpdate != null && !PNACs.Contains(newsUpdate2.PNAC))
        {
            PNACs.Add(newsUpdate2.PNAC);

            // Create a news story object
            complexEvent = new NewsStory(earlierNewsUpdate, newsUpdate2, this);

            return true;
        }

        return false;
    }
}

public class PriceChangedPositiveBasicStory : Pattern
{
    public double Threshold = 0.025;

    public override bool Match(ref ComplexEvent complexEvent, SimpleEvent simpleEvent)
    {
        // ---> STEP 1 - Check if the incoming event is of the right type
        if (simpleEvent.GetType() != typeof(NewsStory))
        {
            return false;
        }

        // ---> STEP 2 - select prices, last before the story started and last before the story ended

        // Copy Queue into a List
        var list = new List<SimpleEvent>(Buffer.EventBuffer);

        var newsStory = simpleEvent as NewsStory;

        PriceUpdate earlierPriceUpdate = null;
        PriceUpdate laterPriceUpdate = null;

        laterPriceUpdate = (PriceUpdate)list.OrderBy(e => e.TimeStamp).LastOrDefault(e => ((e.GetType() == typeof(PriceUpdate)) && ((PriceUpdate)e).TimeStamp >= newsStory.StoryStartTimeStamp));
        earlierPriceUpdate = (PriceUpdate)list.DefaultIfEmpty(laterPriceUpdate).OrderBy(e => e.TimeStamp).LastOrDefault(e => ((e.GetType() == typeof(PriceUpdate)) && (((PriceUpdate)e).TimeStamp < newsStory.StoryStartTimeStamp)));

        // If both prices are not null and there is a price jump
        if ((earlierPriceUpdate != null && laterPriceUpdate != null) && (laterPriceUpdate.LIST_PRICE >= ((earlierPriceUpdate.LIST_PRICE * Threshold) + earlierPriceUpdate.LIST_PRICE)))
        {
            // Create a PriceChangedPositiveBasicStory object
            complexEvent = new NewsStoryPlus(earlierPriceUpdate, laterPriceUpdate, newsStory, this);

            return true;
        }

        return false;
    }
}

public class PriceChangedNegativeBasicStory : Pattern
{
    public double Threshold = 0.025;

    public override bool Match(ref ComplexEvent complexEvent, SimpleEvent simpleEvent)
    {
        // ---> STEP 1 - Check if the incoming event is of the right type
        if (simpleEvent.GetType() != typeof(NewsStory))
        {
            return false;
        }

        // ---> STEP 2 - select prices, last before the story started and last before the story ended

        // Copy Queue into a List
        var list = new List<SimpleEvent>(Buffer.EventBuffer);

        var newsStory = simpleEvent as NewsStory;

        PriceUpdate earlierPriceUpdate = null;
        PriceUpdate laterPriceUpdate = null;

        laterPriceUpdate = (PriceUpdate)list.OrderBy(e => e.TimeStamp).LastOrDefault(e => ((e.GetType() == typeof(PriceUpdate)) && ((PriceUpdate)e).TimeStamp >= newsStory.StoryStartTimeStamp));
        earlierPriceUpdate = (PriceUpdate)list.DefaultIfEmpty(laterPriceUpdate).OrderBy(e => e.TimeStamp).LastOrDefault(e => ((e.GetType() == typeof(PriceUpdate)) && (((PriceUpdate)e).TimeStamp < newsStory.StoryStartTimeStamp)));

        // If both prices are not null and there is a price jump
        if ((earlierPriceUpdate != null && laterPriceUpdate != null) && (laterPriceUpdate.LIST_PRICE <= (earlierPriceUpdate.LIST_PRICE - (laterPriceUpdate.LIST_PRICE * Threshold))))
        {
            // Create a PriceChangedPositiveBasicStory object
            complexEvent = new NewsStoryMinus(earlierPriceUpdate, laterPriceUpdate, newsStory, this);

            return true;
        }

        return false;
    }
}