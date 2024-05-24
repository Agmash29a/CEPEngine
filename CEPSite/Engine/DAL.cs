using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Model
{

    internal abstract class BaseDataAccess
    {
        /// <summary>
        /// Sets the connection string as part of the constructor.
        /// </summary>
        /// <param name="connectionstring"></param>
        protected BaseDataAccess(string connectionstring)
        {
            _connectionstring = connectionstring;
        }

        /// <summary>
        /// Contains the connection string.
        /// </summary>
        protected string _connectionstring;

        #region private helpers
        private SqlConnection GetConnnection(string connStr)
        {
            //return the connection string
            var con = new SqlConnection(connStr);
            con.Open();

            return con;
        }

        /// <summary>
        /// Gets the connection object
        /// </summary>
        /// <returns></returns>
        protected SqlConnection GetConnnection()
        {
            return GetConnnection(_connectionstring);
        }

        /// <summary>
        /// Sets the maximum length of a string.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        protected string length(string s, int l)
        {
            if (string.IsNullOrEmpty(s) || s.Length < l)
                return s;
            else
                return (s.Substring(0, l));
        }
        #endregion
    }

    internal class DataAccess : BaseDataAccess
    {
        #region Private Variables
        /// <summary>
        /// 
        /// </summary>
        private static DataAccess _instance = new DataAccess();
        #endregion

        #region Public Properties
        /// <summary>
        /// Contains the static singleton instance
        /// </summary>
        public static DataAccess Instance
        {
            get { return _instance; }
        }
        #endregion

        #region Class Constructors
        /// <summary>
        /// Server=localhost\SQLEXPRESS;Database=master;Trusted_Connection=True;
        /// </summary>
        private DataAccess()
            : base(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=finance;Integrated Security=true;Persist Security Info=True;Connection Reset=FALSE;Connection Timeout=100")
            //: base(@"Data Source=DESKTOP-OMTPRS1\SQLEXPRESS;Initial Catalog=finance;Integrated Security=true;Persist Security Info=True;Connection Reset=FALSE;Connection Timeout=100")
        { }
        #endregion

        #region Public Implementation

        public List<PriceUpdate> GetPricesByDateRangeAndRIC(DateTime startDate, DateTime endDate, string RIC)
        {
            var items = new List<PriceUpdate>();

            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("GetPriceUpdatesByDateRangeAndRIC", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@startDate", startDate));
                cmd.Parameters.Add(new SqlParameter("@endDate", endDate));
                cmd.Parameters.Add(new SqlParameter("@RIC", RIC));

                cmd.CommandTimeout = 90;

                var reader = cmd.ExecuteReader();
                int i2 = 0;

                while (reader.Read())
                {
                    i2++;
                    DateTime d;
                    float f;
                    int i;

                    var dateOfTrade = DateTime.TryParse(reader["DATE_OF_TRADE"].ToString(), out d)
                        ? d
                        : DateTime.MinValue;

                    string[] time_START_PERIOD = reader["START_PERIOD"].ToString().Split(':');

                    double MINUTES_START_PERIOD = int.Parse(time_START_PERIOD[1]);
                    double HOURS_START_PERIOD = int.Parse(time_START_PERIOD[0]);

                    var START_PERIOD = dateOfTrade.AddHours(HOURS_START_PERIOD).AddMinutes(MINUTES_START_PERIOD);

                    string[] time_END_PERIOD = reader["END_PERIOD"].ToString().Split(':');

                    double MINUTES_END_PERIOD = int.Parse(time_END_PERIOD[1]);
                    double HOURS_END_PERIOD = int.Parse(time_END_PERIOD[0]);

                    var END_PERIOD = dateOfTrade.AddHours(HOURS_END_PERIOD).AddMinutes(MINUTES_END_PERIOD);

                    var priceUpdate = new PriceUpdate()
                    {
                        AVERAGE_PRICE = float.TryParse(reader["AVERAGE_PRICE"].ToString(), out f) ? f : 0,
                        DATE_OF_TRADE = dateOfTrade,
                        END_PERIOD = END_PERIOD,
                        LIST_PRICE = float.TryParse(reader["LIST_PRICE"].ToString(), out f) ? f : 0,
                        TimeStamp = START_PERIOD,
                        NUM_TRADE = int.TryParse(reader["NUM_TRADE"].ToString(), out i) ? i : 0,
                        SECURITY = reader["SECURITY"].ToString(),
                        START_PERIOD = START_PERIOD,
                    };

                    items.Add(priceUpdate);
                }
            }

            return items;
        }

        public List<NewsUpdate> GetNewsByDateRangeAndRIC(DateTime startDate, DateTime endDate, string RIC)
        {
            var items = new List<NewsUpdate>();

            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("GetNewsUpdatesByDateRangeAndRIC", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@startDate", startDate));
                cmd.Parameters.Add(new SqlParameter("@endDate", endDate));
                cmd.Parameters.Add(new SqlParameter("@RIC", RIC));

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DateTime d;
                    float f;
                    int i;
                    double dbl;
                    bool b;
                    EVENT_TYPES e;

                    DateTime Timestamp = DateTime.TryParse(reader["MSG_DATE"].ToString(), out d) ? d : DateTime.MinValue;

                    Timestamp = Timestamp.AddHours(double.TryParse(reader["MSG_TIME_HOURS"].ToString(), out dbl) ? dbl : 0)
                    .AddMinutes(double.TryParse(reader["MSG_TIME_MINUTES"].ToString(), out dbl) ? dbl : 0)
                    .AddSeconds(double.TryParse(reader["MSG_TIME_SECONDS"].ToString(), out dbl) ? dbl : 0)
                    .AddMilliseconds(double.TryParse(reader["MSG_TIME_MILLISECONDS"].ToString(), out dbl) ? dbl : 0);

                    var newsUpdate = new NewsUpdate
                    {
                        MSG_DATE = DateTime.TryParse(reader["MSG_DATE"].ToString(), out d) ? d : DateTime.MinValue,
                        MSG_TIME_MILLISECONDS = int.TryParse(reader["MSG_TIME_MILLISECONDS"].ToString(), out i) ? i : 0,
                        MSG_TIME_SECONDS = int.TryParse(reader["MSG_TIME_SECONDS"].ToString(), out i) ? i : 0,
                        MSG_TIME_MINUTES = int.TryParse(reader["MSG_TIME_MINUTES"].ToString(), out i) ? i : 0,
                        MSG_TIME_HOURS = int.TryParse(reader["MSG_TIME_HOURS"].ToString(), out i) ? i : 0,
                        UNIQUE_STORY_INDEX = reader["UNIQUE_STORY_INDEX"].ToString(),
                        EVENT_TYPE =
                            Enum.TryParse(reader["EVENT_TYPE"].ToString(), out e) ? e : EVENT_TYPES.STORY_TAKE_APPEND,
                        PNAC = reader["PNAC"].ToString(),
                        STORY_DATE_TIME =
                            DateTime.TryParse(reader["STORY_DATE_TIME"].ToString(), out d) ? d : DateTime.MinValue,
                        TAKE_DATE_TIME =
                            DateTime.TryParse(reader["TAKE_DATE_TIME"].ToString(), out d) ? d : DateTime.MinValue,
                        HEADLINE_ALERT_TEXT = reader["HEADLINE_ALERT_TEXT"].ToString(),
                        ACCUMULATED_STORY_TEXT = reader["ACCUMULATED_STORY_TEXT"].ToString(),
                        TAKE_TEXT = reader["TAKE_TEXT"].ToString(),
                        HEADLINE_SUBTYPE = int.TryParse(reader["HEADLINE_SUBTYPE"].ToString(), out i) ? i : 0,
                        STORY_TYPE = reader["STORY_TYPE"].ToString(),
                        TABULAR_FLAG = bool.TryParse(reader["TABULAR_FLAG"].ToString(), out b) && b,
                        ATTRIBUTION = reader["ATTRIBUTION"].ToString(),
                        LANGUAGE = reader["LANGUAGE"].ToString(),
                        TimeStamp = Timestamp,
                        NAMED_ITEMS = GetNewsUpdate_NAMED_ITEMS(int.Parse(reader["ID"].ToString())).ToArray(),
                        RELATED_RICS = GetNewsUpdate_RELATED_RICS(int.Parse(reader["ID"].ToString())).ToArray(),
                        PRODUCTS = GetNewsUpdate_PRODUCTS(int.Parse(reader["ID"].ToString())).ToArray(),
                        TOPICS = GetNewsUpdate_TOPICS(int.Parse(reader["ID"].ToString())).ToArray()
                    };

                    items.Add(newsUpdate);
                }
            }

            return items;
        }

        public List<Sentiment> GetSentimentByDateRangeAndRIC(DateTime startDate, DateTime endDate, string RIC)
        {
            var items = new List<Sentiment>();

            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("GetPositiveSentimentByDateRangeAndRIC", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@startDate", startDate));
                cmd.Parameters.Add(new SqlParameter("@endDate", endDate));
                cmd.Parameters.Add(new SqlParameter("@RIC", RIC));

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DateTime d;

                    var dateOfTrade = DateTime.TryParse(reader["STORY_DATE"].ToString(), out d)
                        ? d
                        : DateTime.MinValue;

                    var time_STORY_TIME = reader["STORY_TIME"].ToString().Split(':');

                    double SECONDS_STORY_TIME = int.Parse(time_STORY_TIME[2]);
                    double MINUTES_STORY_TIME = int.Parse(time_STORY_TIME[1]);
                    double HOURS_STORY_TIME = int.Parse(time_STORY_TIME[0]);

                    var sentiment = new Sentiment()
                    {

                        STOCK_RIC = reader["STOCK_RIC"].ToString(),
                        PNAC = reader["PNAC"].ToString(),
                        SENTIMENT = int.Parse(reader["SENTIMENT"].ToString()),
                        TimeStamp = dateOfTrade.AddHours(HOURS_STORY_TIME).AddMinutes(MINUTES_STORY_TIME).AddSeconds(SECONDS_STORY_TIME),
                    };

                    items.Add(sentiment);
                }
            }

            return items;
        }

        public List<Sentiment> GetNegativeSentimentByDateRangeAndRIC(DateTime startDate, DateTime endDate, string RIC)
        {
            var items = new List<Sentiment>();

            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("GetNegativeSentimentByDateRangeAndRIC", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@startDate", startDate));
                cmd.Parameters.Add(new SqlParameter("@endDate", endDate));
                cmd.Parameters.Add(new SqlParameter("@RIC", RIC));

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DateTime d;

                    var dateOfTrade = DateTime.TryParse(reader["STORY_DATE"].ToString(), out d)
                        ? d
                        : DateTime.MinValue;

                    var time_STORY_TIME = reader["STORY_TIME"].ToString().Split(':');

                    double SECONDS_STORY_TIME = int.Parse(time_STORY_TIME[2]);
                    double MINUTES_STORY_TIME = int.Parse(time_STORY_TIME[1]);
                    double HOURS_STORY_TIME = int.Parse(time_STORY_TIME[0]);

                    var sentiment = new Sentiment()
                    {

                        STOCK_RIC = reader["STOCK_RIC"].ToString(),
                        PNAC = reader["PNAC"].ToString(),
                        SENTIMENT = int.Parse(reader["SENTIMENT"].ToString()),
                        TimeStamp = dateOfTrade.AddHours(HOURS_STORY_TIME).AddMinutes(MINUTES_STORY_TIME).AddSeconds(SECONDS_STORY_TIME),
                    };

                    items.Add(sentiment);
                }
            }

            return items;
        }

        public int AddNewsUpdate(NewsUpdate story)
        {
            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("AddNewsUpdate", connection) {CommandType = CommandType.StoredProcedure};

                cmd.Parameters.Add(new SqlParameter("@MSG_DATE", story.MSG_DATE));
                cmd.Parameters.Add(new SqlParameter("@MSG_TIME_MILLISECONDS", story.MSG_TIME_MILLISECONDS));
                cmd.Parameters.Add(new SqlParameter("@MSG_TIME_SECONDS", story.MSG_TIME_SECONDS));
                cmd.Parameters.Add(new SqlParameter("@MSG_TIME_MINUTES", story.MSG_TIME_MINUTES));
                cmd.Parameters.Add(new SqlParameter("@MSG_TIME_HOURS", story.MSG_TIME_HOURS));
                cmd.Parameters.Add(new SqlParameter("@UNIQUE_STORY_INDEX", story.UNIQUE_STORY_INDEX));
                cmd.Parameters.Add(new SqlParameter("@EVENT_TYPE", story.EVENT_TYPE));
                cmd.Parameters.Add(new SqlParameter("@PNAC", story.PNAC));
                cmd.Parameters.Add(new SqlParameter("@STORY_DATE_TIME", story.STORY_DATE_TIME));
                cmd.Parameters.Add(new SqlParameter("@TAKE_DATE_TIME", story.TAKE_DATE_TIME));
                cmd.Parameters.Add(new SqlParameter("@HEADLINE_ALERT_TEXT", story.HEADLINE_ALERT_TEXT));
                cmd.Parameters.Add(new SqlParameter("@ACCUMULATED_STORY_TEXT", story.ACCUMULATED_STORY_TEXT));
                cmd.Parameters.Add(new SqlParameter("@TAKE_TEXT", story.TAKE_TEXT));
                cmd.Parameters.Add(new SqlParameter("@HEADLINE_SUBTYPE", story.HEADLINE_SUBTYPE));
                cmd.Parameters.Add(new SqlParameter("@STORY_TYPE", story.STORY_TYPE));
                cmd.Parameters.Add(new SqlParameter("@TABULAR_FLAG", story.TABULAR_FLAG));
                cmd.Parameters.Add(new SqlParameter("@ATTRIBUTION", story.ATTRIBUTION));
                cmd.Parameters.Add(new SqlParameter("@LANGUAGE", story.LANGUAGE));
                cmd.Parameters.Add(new SqlParameter("@TimeStamp", story.TimeStamp));

                var outputId = new SqlParameter("@ID", SqlDbType.Int) {Direction = ParameterDirection.Output};
                
                cmd.Parameters.Add(outputId);
                cmd.ExecuteNonQuery();

                var id = int.Parse(cmd.Parameters["@ID"].Value.ToString());

                cmd.Connection.Close();

                return id;
            }
        }

        public void AddNewsUpdate_PRODUCT(int ID, string Value)
        {
            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("AddNewsUpdate_PRODUCT", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                cmd.Parameters.Add(new SqlParameter("@Value", Value));
                cmd.ExecuteNonQuery();
            }
        }

        public void AddNewsUpdate_TOPIC(int ID, string Value)
        {
            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("AddNewsUpdate_TOPIC", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                cmd.Parameters.Add(new SqlParameter("@Value", Value));
                cmd.ExecuteNonQuery();
            }
        }

        public void AddNewsUpdate_RELATED_RIC(int ID, string Value)
        {
            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("AddNewsUpdate_RELATED_RIC", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                cmd.Parameters.Add(new SqlParameter("@Value", Value));
                cmd.ExecuteNonQuery();
            }
        }

        public void AddNewsUpdate_NAMED_ITEM(int ID, string Value)
        {
            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("AddNewsUpdate_NAMED_ITEM", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                cmd.Parameters.Add(new SqlParameter("@Value", Value));
                cmd.ExecuteNonQuery();
            }
        }

        public List<string> GetNewsUpdate_PRODUCTS(int ID)
        {
            var items = new List<string>();

            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("GetNewsUpdate_PRODUCTS", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    items.Add(reader["Value"].ToString());
                }
            }

            return items;
        }

        public List<string> GetNewsUpdate_TOPICS(int ID)
        {
            var items = new List<string>();

            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("GetNewsUpdate_TOPICS", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    items.Add(reader["Value"].ToString());
                }
            }

            return items;
        }

        public List<string> GetNewsUpdate_RELATED_RICS(int ID)
        {
            var items = new List<string>();

            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("GetNewsUpdate_RELATED_RICS", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    items.Add(reader["Value"].ToString());
                }
            }

            return items;
        }

        public List<string> GetNewsUpdate_NAMED_ITEMS(int ID)
        {
            var items = new List<string>();

            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("GetNewsUpdate_NAMED_ITEMS", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@ID", ID));
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    items.Add(reader["Value"].ToString());
                }
            }

            return items;
        }

        public void AddPriceUpdate(PriceUpdate priceUpdate)
        {
            using (var connection = GetConnnection())
            {
                var cmd = new SqlCommand("AddPriceUpdate", connection) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.Add(new SqlParameter("@SECURITY", priceUpdate.SECURITY));
                cmd.Parameters.Add(new SqlParameter("@DATE_OF_TRADE", priceUpdate.DATE_OF_TRADE));
                cmd.Parameters.Add(new SqlParameter("@START_PERIOD", priceUpdate.START_PERIOD));
                cmd.Parameters.Add(new SqlParameter("@END_PERIOD", priceUpdate.END_PERIOD));
                cmd.Parameters.Add(new SqlParameter("@NUM_TRADE", priceUpdate.NUM_TRADE));
                cmd.Parameters.Add(new SqlParameter("@LIST_PRICE", priceUpdate.LIST_PRICE));
                cmd.Parameters.Add(new SqlParameter("@AVERAGE_PRICE", priceUpdate.AVERAGE_PRICE));
                cmd.Parameters.Add(new SqlParameter("@TOTAL_VOLUME", priceUpdate.TOTAL_VOLUME));
                cmd.Parameters.Add(new SqlParameter("@TimeStamp", priceUpdate.TimeStamp));

                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
