using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CEPSite
{
    public partial class Default : System.Web.UI.Page
    {
        public string NumberOfDates = "0";
        public string ProductName = "";

        public string JSON_DateLabels = "[]";
        public string JSON_PriceUpdates = "[]";

        public string JSON_NumberOfNewsUpdatesPerPriceUpdate = "[]";
        public string JSON_NumberOfRestrictedPriceUpdates = "[]";
        public string JSON_NumberOfPNACsPerPriceUpdate = "[]";
        public string JSON_NumberOfPriceJumpsPerPriceUpdate = "[]";
        public string JSON_NumberOfPriceFallsPerPriceUpdate = "[]";
        public string JSON_NumberOfNewsStoryMinusPerPriceUpdate = "[]";
        public string JSON_NumberOfNewsStoryPlusPerPriceUpdate = "[]";
        public string JSON_NumberOfNewsStoriesPerPriceUpdate = "[]";
        public string JSON_NumberOfNewsUpdateMinusPerPriceUpdate = "[]";
        public string JSON_NumberOfNewsUpdatePlusPerPriceUpdate = "[]";
        public string JSON_NumberOfNegativeSentimentPerPriceUpdate = "[]";
        public string JSON_NumberOfPositiveSentimentPerPriceUpdate = "[]";

        public string NumPriceUpdates = "N/A";
        public string NumNewsUpdates = "N/A";
        public string NumNegativeSentiment = "N/A";
        public string NumPositiveSentiment = "N/A";
        public string NumPriceJumps = "N/A";
        public string NumNewsUpdatePlus = "N/A";
        public string NumPriceFalls = "N/A";
        public string NumNewsUpdateMinus = "N/A";
        public string NumNewsStories = "N/A";
        public string NumNewsStoryPlus = "N/A";
        public string NumNewsStoryMinus = "N/A";

        public string JSON_PriceUpdates_Overlay = "[]";

        public string JSON_NumberOfNewsUpdatesPerPriceUpdate_Overlay = "[]";
        public string JSON_NumberOfRestrictedPriceUpdates_Overlay = "[]";
        public string JSON_NumberOfPNACsPerPriceUpdate_Overlay = "[]";
        public string JSON_NumberOfPriceJumpsPerPriceUpdate_Overlay = "[]";
        public string JSON_NumberOfPriceFallsPerPriceUpdate_Overlay = "[]";
        public string JSON_NumberOfNewsStoryMinusPerPriceUpdate_Overlay = "[]";
        public string JSON_NumberOfNewsStoryPlusPerPriceUpdate_Overlay = "[]";
        public string JSON_NumberOfNewsUpdateMinusPerPriceUpdate_Overlay = "[]";
        public string JSON_NumberOfNewsUpdatePlusPerPriceUpdate_Overlay = "[]";
        public string JSON_NumberOfNewsStoriesPerPriceUpdate_Overlay = "[]";
        public string JSON_NumberOfPositiveSentimentPerPriceUpdate_Overlay = "[]";
        public string JSON_NumberOfNegativeSentimentPerPriceUpdate_Overlay = "[]";

        public string NumPriceUpdates_OVERLAY = "N/A";
        public string NumNewsUpdates_OVERLAY = "N/A";
        public string NumPositiveSentiment_OVERLAY = "N/A";
        public string NumNegativeSentiment_OVERLAY = "N/A";
        public string NumPriceJumps_OVERLAY = "N/A";
        public string NumNewsUpdatePlus_OVERLAY = "N/A";
        public string NumPriceFalls_OVERLAY = "N/A";
        public string NumNewsUpdateMinus_OVERLAY = "N/A";
        public string NumNewsStories_OVERLAY = "N/A";
        public string NumNewsStoryPlus_OVERLAY = "N/A";
        public string NumNewsStoryMinus_OVERLAY = "N/A";

        public string SIMPLE_EVENT_MARKER = "";

        public string SearchHTML_PriceUpdates = "";
        public string SearchHTML_NewsUpdates = "";
        public string SearchHTML_NegativeSentiment = "";
        public string SearchHTML_PositiveSentiment = "";
        public string SearchHTML_PriceJumps = "";
        public string SearchHTML_NewsUpdatePlus = "";
        public string SearchHTML_PriceFalls = "";
        public string SearchHTML_NewsUpdateMinus = "";
        public string SearchHTML_NewsStories = "";
        public string SearchHTML_NewsStoryPlus = "";
        public string SearchHTML_NewsStoryMinus = "";

        public string SearchHTML_PriceUpdates_OVERLAY = "";
        public string SearchHTML_NewsUpdates_OVERLAY = "";
        public string SearchHTML_NegativeSentiment_OVERLAY = "";
        public string SearchHTML_PositiveSentiment_OVERLAY = "";
        public string SearchHTML_PriceJumps_OVERLAY = "";
        public string SearchHTML_NewsUpdatePlus_OVERLAY = "";
        public string SearchHTML_PriceFalls_OVERLAY = "";
        public string SearchHTML_NewsUpdateMinus_OVERLAY = "";
        public string SearchHTML_NewsStories_OVERLAY = "";
        public string SearchHTML_NewsStoryPlus_OVERLAY = "";
        public string SearchHTML_NewsStoryMinus_OVERLAY = "";

        public string AIO_1_StartDateTime = "";
        public string AIO_1_EndDateTime = "";
        public string AIO_2_StartDateTime = "";
        public string AIO_2_EndDateTime = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MainPanel"] == null)
                Session["MainPanel"] = "";

            if (Session["OverlayOn"] == null)
                Session["OverlayOn"] = "";

            SIMPLE_EVENT_MARKER = Parser.SIMPLE_EVENT_MARKER;

            if (Session["AIO_1_StartDateTime"] == null)
                Session["AIO_1_StartDateTime"] = "";

            if (Session["AIO_1_EndDateTime"] == null)
                Session["AIO_1_EndDateTime"] = "";

            AIO_2_StartDateTime = Session["AIO_1_StartDateTime"].ToString();
            AIO_2_EndDateTime = Session["AIO_1_EndDateTime"].ToString();

            SearchHTML_PriceUpdates = Parser.GetSearchHTML(new PriceUpdate());
            SearchHTML_NewsUpdates = Parser.GetSearchHTML(new NewsUpdate());
            SearchHTML_NegativeSentiment = Parser.GetSearchHTML(new Sentiment());
            SearchHTML_PositiveSentiment = Parser.GetSearchHTML(new Sentiment());
            SearchHTML_PriceJumps = Parser.GetSearchHTML(new PriceJump(new PriceUpdate(), new PriceUpdate(), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_NewsUpdatePlus = Parser.GetSearchHTML(new NewsUpdatePlus(new NewsUpdate(), new PriceJump(new PriceUpdate(), new PriceUpdate(), new Pattern<DAG<SimpleEvent>>()), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_PriceFalls = Parser.GetSearchHTML(new PriceFall(new PriceUpdate(), new PriceUpdate(), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_NewsUpdateMinus = Parser.GetSearchHTML(new NewsUpdateMinus(new NewsUpdate(), new PriceFall(new PriceUpdate(), new PriceUpdate(), new Pattern<DAG<SimpleEvent>>()), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_NewsStories = Parser.GetSearchHTML(new NewsStory(new NewsUpdate(), new NewsUpdate(), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_NewsStoryPlus = Parser.GetSearchHTML(new NewsStoryPlus(new PriceUpdate(), new PriceUpdate(), new NewsStory(new NewsUpdate(), new NewsUpdate(), new Pattern<DAG<SimpleEvent>>()), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_NewsStoryMinus = Parser.GetSearchHTML(new NewsStoryMinus(new PriceUpdate(), new PriceUpdate(), new NewsStory(new NewsUpdate(), new NewsUpdate(), new Pattern<DAG<SimpleEvent>>()), new Pattern<DAG<SimpleEvent>>()));

            SearchHTML_PriceUpdates_OVERLAY = Parser.GetSearchHTML_OVERLAY(new PriceUpdate());
            SearchHTML_NewsUpdates_OVERLAY = Parser.GetSearchHTML_OVERLAY(new NewsUpdate());
            SearchHTML_NegativeSentiment_OVERLAY = Parser.GetSearchHTML_OVERLAY(new Sentiment());
            SearchHTML_PositiveSentiment_OVERLAY = Parser.GetSearchHTML_OVERLAY(new Sentiment());
            SearchHTML_PriceJumps_OVERLAY = Parser.GetSearchHTML_OVERLAY(new PriceJump(new PriceUpdate(), new PriceUpdate(), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_NewsUpdatePlus_OVERLAY = Parser.GetSearchHTML_OVERLAY(new NewsUpdatePlus(new NewsUpdate(), new PriceJump(new PriceUpdate(), new PriceUpdate(), new Pattern<DAG<SimpleEvent>>()), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_PriceFalls_OVERLAY = Parser.GetSearchHTML_OVERLAY(new PriceFall(new PriceUpdate(), new PriceUpdate(), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_NewsUpdateMinus_OVERLAY = Parser.GetSearchHTML_OVERLAY(new NewsUpdateMinus(new NewsUpdate(), new PriceFall(new PriceUpdate(), new PriceUpdate(), new Pattern<DAG<SimpleEvent>>()), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_NewsStories_OVERLAY = Parser.GetSearchHTML_OVERLAY(new NewsStory(new NewsUpdate(), new NewsUpdate(), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_NewsStoryPlus_OVERLAY = Parser.GetSearchHTML_OVERLAY(new NewsStoryPlus(new PriceUpdate(), new PriceUpdate(), new NewsStory(new NewsUpdate(), new NewsUpdate(), new Pattern<DAG<SimpleEvent>>()), new Pattern<DAG<SimpleEvent>>()));
            SearchHTML_NewsStoryMinus_OVERLAY = Parser.GetSearchHTML_OVERLAY(new NewsStoryMinus(new PriceUpdate(), new PriceUpdate(), new NewsStory(new NewsUpdate(), new NewsUpdate(), new Pattern<DAG<SimpleEvent>>()), new Pattern<DAG<SimpleEvent>>()));

        }

        public int GetHours(string timsString)
        {
            var timeArray = timsString.Split(':');

            var hours = int.Parse(timeArray[0]);

            if (timeArray[1].Contains("pm"))
            {
                if (hours != 12)
                {
                    hours += 12;
                }
            }
            else
            {
                hours = hours == 12 ? 0 : hours;
            }

            return hours;
        }

        public int GetMins(string timsString)
        {
            var timeArray = timsString.Split(':');

            return int.Parse(timeArray[1].Substring(0, 2));
        }

        protected void Button_GetData_Click(object sender, EventArgs e)
        {
            GetHours(TextBox_StartTime.Text);
            GetMins(TextBox_StartTime.Text);

            ProductName = Engine.PRODUCT_NAME = TextBox_ProductName.Text;
            Engine.PRICE_INTERVAL_SECONDS = int.Parse(TextBox_Seconds.Text);

            Engine.ShowPriceUpdates = StageOne_CheckBox_ShowPriceUpdates.Checked;
            Engine.ShowNewsUpdates = StageOne_CheckBox_ShowNewsUpdates.Checked;
            Engine.ShowPositiveSentiment = StageOne_CheckBox_ShowSentimentMinus.Checked;
            Engine.ShowNegativeSentiment = StageOne_CheckBox_ShowSentimentPlus.Checked;
            Engine.ShowPriceJumps = StageOne_CheckBox_ShowPriceJumps.Checked;
            Engine.ShowPriceFalls = StageOne_CheckBox_ShowPriceFalls.Checked;
            Engine.ShowNewsUpdatePlus = StageOne_CheckBox_ShowNewsUpdatePlus.Checked;
            Engine.ShowNewsUpdateMinus = StageOne_CheckBox_ShowNewsUpdateMinus.Checked;
            Engine.ShowNewsStories = StageOne_CheckBox_ShowNewsStories.Checked;
            Engine.ShowNewsStoryPlus = StageOne_CheckBox_ShowNewsStoryPlus.Checked;
            Engine.ShowNewsStoryMinus = StageOne_CheckBox_ShowNewsStoryMinus.Checked;


            Engine.PriceChangedPositiveWithNewsUpdate_THRESHHOLD = double.Parse(TextBox_PriceChangedPositiveWithNewsUpdate_THRESHHOLD.Text);
            Engine.PriceChangedPositive_THRESHHOLD = double.Parse(TextBox_PriceChangedPositive_THRESHHOLD.Text);
            Engine.PriceChangedNegativeWithNewsUpdate_THRESHHOLD = double.Parse(TextBox_PriceChangedNegativeWithNewsUpdate_THRESHHOLD.Text);
            Engine.PriceChangedNegative_THRESHHOLD = double.Parse(TextBox_PriceChangedNegative_THRESHHOLD.Text);

            var startDate = DateTime.Parse(TextBox_StartDate.Text).AddHours(GetHours(TextBox_StartTime.Text)).AddMinutes(GetMins(TextBox_StartTime.Text));
            var endDate = DateTime.Parse(TextBox_EndDate.Text).AddHours(GetHours(TextBox_EndTime.Text)).AddMinutes(GetMins(TextBox_EndTime.Text));

            SetAIO1Dates();

            Engine.Initialise(Page, startDate, endDate, TextBox_ProductName.Text, int.Parse(TextBox_Seconds.Text));
            Engine.ShowAllEvents();

            SetUpEvents();

            Session["MainPanel"] = "MainPanelOn";
        }

        public void SetAIO1Dates()
        {
            var startDate = DateTime.Parse(TextBox_StartDate.Text).AddHours(GetHours(TextBox_StartTime.Text)).AddMinutes(GetMins(TextBox_StartTime.Text));
            var endDate = DateTime.Parse(TextBox_EndDate.Text).AddHours(GetHours(TextBox_EndTime.Text)).AddMinutes(GetMins(TextBox_EndTime.Text));

            AIO_1_StartDateTime = $"{startDate:g}";
            AIO_1_EndDateTime = $"{endDate:g}";
        }

        protected void Button_SaveOverlay_Click(object sender, EventArgs e)
        {
            SetAIO1Dates();

            Session["AIO_1_StartDateTime"] = AIO_1_StartDateTime;
            Session["AIO_1_EndDateTime"] = AIO_1_EndDateTime;

            AIO_2_StartDateTime = Session["AIO_1_StartDateTime"].ToString();
            AIO_2_EndDateTime = Session["AIO_1_EndDateTime"].ToString();

            foreach (SimpleEvent ev in CEPSite.Buffer.EventBuffer)
            {
                CEPSite.Buffer_Overlay.AddToEventBuffer(ev);
            }

            Engine.ShowAllEvents_Overlay();

            SetUpEvents();
            SetUpEvents_Overlay();

            Session["OverlayOn"] = "OverlayOn";
        }

        protected void Button_ShowOverlay_Click(object sender, EventArgs e)
        {
            SetAIO1Dates();

            AIO_2_StartDateTime = Session["AIO_1_StartDateTime"].ToString();
            AIO_2_EndDateTime = Session["AIO_1_EndDateTime"].ToString();

            Engine.ShowAllEvents_Overlay();

            SetUpEvents();
            SetUpEvents_Overlay();
        }

        protected void Button_ShowIndividualEvent_Click(object sender, EventArgs e)
        {
            Engine.ShowSingleComplexEvent(Request.Form["EventToDisplay"]);

            SetUpEvents();
        }

        protected void Button_ShowAllEvents_Click(object sender, EventArgs e)
        {
            Engine.ShowAllEvents();

            SetUpEvents();
        }

        protected void Button_ShowIndividualEventWithSubEvents_Click(object sender, EventArgs e)
        {
            Engine.ShowSingleComplexEventWithNestedTypes(Request.Form["EventToDisplay"]);

            SetUpEvents();
        }

        protected void SetUpEvents_Overlay()
        {
            JSON_PriceUpdates_Overlay = Engine.JSON_PriceUpdates_Overlay;

            JSON_NumberOfNewsUpdatesPerPriceUpdate_Overlay = Engine.JSON_NumberOfNewsUpdatesPerPriceUpdate_Overlay;
            JSON_NumberOfRestrictedPriceUpdates_Overlay = Engine.JSON_NumberOfRestrictedPriceUpdates_Overlay;
            JSON_NumberOfPNACsPerPriceUpdate_Overlay = Engine.JSON_NumberOfPNACsPerPriceUpdate_Overlay;
            JSON_NumberOfPriceJumpsPerPriceUpdate_Overlay = Engine.JSON_NumberOfPriceJumpsPerPriceUpdate_Overlay;
            JSON_NumberOfPriceFallsPerPriceUpdate_Overlay = Engine.JSON_NumberOfPriceFallsPerPriceUpdate_Overlay;
            JSON_NumberOfNewsStoryMinusPerPriceUpdate_Overlay = Engine.JSON_NumberOfNewsStoryMinusPerPriceUpdate_Overlay;
            JSON_NumberOfNewsStoryPlusPerPriceUpdate_Overlay = Engine.JSON_NumberOfNewsStoryPlusPerPriceUpdate_Overlay;
            JSON_NumberOfNewsUpdateMinusPerPriceUpdate_Overlay = Engine.JSON_NumberOfNewsUpdateMinusPerPriceUpdate_Overlay;
            JSON_NumberOfNewsUpdatePlusPerPriceUpdate_Overlay = Engine.JSON_NumberOfNewsUpdatePlusPerPriceUpdate_Overlay;
            JSON_NumberOfNewsStoriesPerPriceUpdate_Overlay = Engine.JSON_NumberOfNewsStoriesPerPriceUpdate_Overlay;
            JSON_NumberOfPositiveSentimentPerPriceUpdate_Overlay = Engine.JSON_NumberOfPositiveSentimentPerPriceUpdate_Overlay;
            JSON_NumberOfNegativeSentimentPerPriceUpdate_Overlay = Engine.JSON_NumberOfNegativeSentimentPerPriceUpdate_Overlay;

            NumPriceUpdates_OVERLAY = Engine.NumPriceUpdates_Overlay.ToString();
            NumNewsUpdates_OVERLAY = Engine.NumNewsUpdates_Overlay.ToString();
            NumPositiveSentiment_OVERLAY = Engine.NumPositiveSentiment_Overlay.ToString();
            NumNegativeSentiment_OVERLAY = Engine.NumNegativeSentiment_Overlay.ToString();
            NumPriceJumps_OVERLAY = Engine.NumPriceJumps_Overlay.ToString();
            NumNewsUpdatePlus_OVERLAY = Engine.NumNewsUpdatePlus_Overlay.ToString();
            NumPriceFalls_OVERLAY = Engine.NumPriceFalls_Overlay.ToString();
            NumNewsUpdateMinus_OVERLAY = Engine.NumNewsUpdateMinus_Overlay.ToString();
            NumNewsStories_OVERLAY = Engine.NumNewsStories_Overlay.ToString();
            NumNewsStoryPlus_OVERLAY = Engine.NumNewsStoryPlus_Overlay.ToString();
            NumNewsStoryMinus_OVERLAY = Engine.NumNewsStoryMinus_Overlay.ToString();
        }

        protected void SetUpEvents()
        {
            JSON_PriceUpdates = Engine.JSON_PriceUpdates;
            JSON_DateLabels = Engine.JSON_DateLabels;
            NumberOfDates = Engine.NumberOfDates.ToString();

            JSON_NumberOfNewsUpdatesPerPriceUpdate = Engine.JSON_NumberOfNewsUpdatesPerPriceUpdate;
            JSON_NumberOfRestrictedPriceUpdates = Engine.JSON_NumberOfRestrictedPriceUpdates;
            JSON_NumberOfPNACsPerPriceUpdate = Engine.JSON_NumberOfPNACsPerPriceUpdate;
            JSON_NumberOfPriceJumpsPerPriceUpdate = Engine.JSON_NumberOfPriceJumpsPerPriceUpdate;
            JSON_NumberOfPriceFallsPerPriceUpdate = Engine.JSON_NumberOfPriceFallsPerPriceUpdate;
            JSON_NumberOfNewsStoryMinusPerPriceUpdate = Engine.JSON_NumberOfNewsStoryMinusPerPriceUpdate;
            JSON_NumberOfNewsStoryPlusPerPriceUpdate = Engine.JSON_NumberOfNewsStoryPlusPerPriceUpdate;
            JSON_NumberOfNewsUpdateMinusPerPriceUpdate = Engine.JSON_NumberOfNewsUpdateMinusPerPriceUpdate;
            JSON_NumberOfNewsUpdatePlusPerPriceUpdate = Engine.JSON_NumberOfNewsUpdatePlusPerPriceUpdate;
            JSON_NumberOfNewsStoriesPerPriceUpdate = Engine.JSON_NumberOfNewsStoriesPerPriceUpdate;
            JSON_NumberOfPositiveSentimentPerPriceUpdate = Engine.JSON_NumberOfPositiveSentimentPerPriceUpdate;
            JSON_NumberOfNegativeSentimentPerPriceUpdate = Engine.JSON_NumberOfNegativeSentimentPerPriceUpdate;

            NumPriceUpdates = Engine.NumPriceUpdates.ToString();
            NumNewsUpdates = Engine.NumNewsUpdates.ToString();
            NumPositiveSentiment = Engine.NumPositiveSentiment.ToString();
            NumNegativeSentiment = Engine.NumNegativeSentiment.ToString();
            NumPriceJumps = Engine.NumPriceJumps.ToString();
            NumNewsUpdatePlus = Engine.NumNewsUpdatePlus.ToString();
            NumPriceFalls = Engine.NumPriceFalls.ToString();
            NumNewsUpdateMinus = Engine.NumNewsUpdateMinus.ToString();
            NumNewsStories = Engine.NumNewsStories.ToString();
            NumNewsStoryPlus = Engine.NumNewsStoryPlus.ToString();
            NumNewsStoryMinus = Engine.NumNewsStoryMinus.ToString();
        }
    }
}