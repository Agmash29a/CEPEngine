using Deedle;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CSharp
{
    class FrameSamples
    {
        public static void Samples([CallerFilePath] string file = "")
        {
            var root = Path.GetDirectoryName(file);

            // ------------------------------------------------------------
            // Creating and loading data frames
            // ------------------------------------------------------------

            // [create-records]
            // Create a collection of anonymous types
            var rnd = new Random();
            var objects = Enumerable.Range(0, 10).Select(i =>
              new { Key = "ID_" + i.ToString(), Number = rnd.Next() });

            // Create data frame with properties as column names
            var dfObjects = Frame.FromRecords(objects);
            dfObjects.Print();
            // [/create-records]

            // [create-rows]
            // Generate collection of rows
            var rows = Enumerable.Range(0, 100).Select(i => {
                // Build each row using series builder & return 
                // KeyValue representing row key with row data
                var sb = new SeriesBuilder<string>();
                sb.Add("Index", i);
                sb.Add("Sin", Math.Sin(i / 100.0));
                sb.Add("Cos", Math.Cos(i / 100.0));
                return KeyValue.Create(i, sb.Series);
            });

            // Turn sequence of row information into data frame
            var df = Frame.FromRows(rows);
            // [/create-rows]

            // [create-csv]
            // Read MSFT & FB stock prices from a CSV file
            var msftRaw = Frame.ReadCsv(Path.Combine(root, "../data/stocks/msft.csv"));
            var fbRaw = Frame.ReadCsv(Path.Combine(root, "../data/stocks/fb.csv"));
            // [/create-csv]

            msftRaw.Print();

            // ------------------------------------------------------------
            // Working with row and column indices
            // ------------------------------------------------------------

            // [index-date]
            // Get MSFT & FB stock prices indexed by data
            var msft = msftRaw.IndexRows<DateTime>("Date").SortRowsByKey();
            var fb = fbRaw.IndexRows<DateTime>("Date").SortRowsByKey();

            // And rename columns to avoid overlap
            msft.RenameColumns(s => "Msft" + s);
            fb.RenameColumns(s => "Fb" + s);
            // [/index-date]

            // [index-cols]
            // Read US debt data from a CSV file
            var debt = Frame.ReadCsv(Path.Combine(root, "../data/us-debt.csv"));
            // Index by Year column and
            var debtByYear = debt
              .IndexRows<int>("Year")
              .IndexColumnsWith(new[] { "Year", "GDP", "Population", "Debt", "?" });
            // [/index-cols]

            debtByYear.Print();

            // ------------------------------------------------------------
            // Joining and aligning data frames
            // ------------------------------------------------------------

            // [join-inout]
            // Inner join (take intersection of dates)
            var joinIn = msft.Join(fb, JoinKind.Inner);
            // Outer join (take union & fill with missing)
            var joinOut = msft.Join(fb, JoinKind.Outer);
            // [/join-inout]

            joinIn.Print();
            joinOut.Print();
            Console.ReadLine();

            // [join-lookup]
            // Shift MSFT observations by +1 hour for testing
            var msftShift = msft.SelectRowKeys(k => k.Key.AddHours(1.0));

            // MSFT data are missing because keys do not match
            var joinLeftWrong = fb.Join(msftShift, JoinKind.Left);

            // This works! Find the value for the nearest smaller key
            // (that is, for the nearest earlier time with value)
            var joinLeft = fb.Join(msftShift, JoinKind.Left, Lookup.ExactOrSmaller);
            joinLeft.Print();
            // [/join-lookup]

            // ------------------------------------------------------------
            // Accessing data and series operations
            // ------------------------------------------------------------

            // [series-get]
            // Get MSFT and FB opening prices and calculate the difference
            var msOpen = joinIn.GetColumn<double>("MsftOpen");
            var msClose = joinIn.GetColumn<double>("MsftClose");
            var msDiff = msClose - msOpen;
            // [/series-get]

            // [series-dropadd]
            // Drop series from a data frame
            joinIn.DropColumn("MsftAdj Close");
            joinIn.DropColumn("FbAdj Close");

            // Add new series to a frame
            joinIn.AddColumn("MsftDiff", msDiff);
            joinIn.Print();
            // [/series-dropadd]

            // [series-rows]
            // Get row and then look at row properties
            var row = joinIn.Rows[new DateTime(2013, 1, 4)];
            var msLo = row.GetAs<double>("MsftLow");
            var msHi = row.GetAs<double>("MsftHigh");

            // Get row for the first available value after
            // the specified key (1 January 2013)
            var firstJan = joinIn.Rows.Get(new DateTime(2013, 1, 1),
              Lookup.ExactOrGreater);

            // Get value for a specified column & row keys
            var diff = joinIn["MsftDiff", new DateTime(2013, 1, 4)];
            // [/series-rows]

            // ------------------------------------------------------------
            // LINQ to data frame
            // ------------------------------------------------------------

            // [linq-select]
            // Project rows into a new series using the Select method
            var diffs = joinIn.Rows.Select(kvp =>
              kvp.Value.GetAs<double>("MsftOpen") -
                kvp.Value.GetAs<double>("FbOpen"));
            // [/linq-select]

            // [linq-where]
            // Filter rows using a specified condition 
            var msftGreaterRows = joinIn.Rows.Where(kvp =>
              kvp.Value.GetAs<double>("MsftOpen") >
                kvp.Value.GetAs<double>("FbOpen"));

            // Transform row collection into a new data frame
            var msftGreaterDf = Frame.FromRows(msftGreaterRows);
            // [/linq-where]

            // [ops-returns]
            // Calculate daily returns in percents
            var returns = msft.Diff(1) / msft * 100.0;

            // Transform all numerical series
            // (round the values to 2 fractional digits)
            var round =
              returns.ColumnApply((Series<DateTime, double> numeric) =>
                numeric.Select(kvp => Math.Round(kvp.Value, 2)));
            // [/ops-returns]
            round.Print();
        }
    }
}
namespace CSharp
{
    static class SeriesSamples
    {
        public static void Samples([CallerFilePath] string file = "")
        {
            var root = Path.GetDirectoryName(file);

            // ------------------------------------------------------------
            // Creating time series
            // ------------------------------------------------------------

            // [create-builder]
            var numNames = new SeriesBuilder<int, string>() {
        { 1, "one" }, { 2, "two" }, { 3, "three" } }.Series;
            numNames.Print();
            // [/create-builder]

            // [create-heterogen]
            // Create series builder and use it via 'dynamic'
            var nameNumsBuild = new SeriesBuilder<string, int>();
            dynamic nameNumsDyn = nameNumsBuild;
            nameNumsDyn.One = 1;
            nameNumsDyn.Two = 2;
            nameNumsDyn.Three = 3;

            // Build series and print it
            var nameNums = nameNumsBuild.Series;
            nameNums.Print();
            // [/create-heterogen]

            // [create-ordinal]
            var rnd = new Random();
            var randNums = Enumerable.Range(0, 100)
              .Select(_ => rnd.NextDouble()).ToOrdinalSeries();
            randNums.Print();
            // [/create-ordinal]

            // [create-kvp]
            var sin =
              (from i in Enumerable.Range(0, 1000)
               let x = i / 100.0
               select KeyValue.Create(x, Math.Sin(x))).ToSeries();
            sin.Print();
            // [/create-kvp]

            // [create-sparse]
            var opts =
              (from i in Enumerable.Range(0, 10)
               let v = OptionalValue.OfNullable(LookupEven(i))
               select KeyValue.Create(i, v)).ToSparseSeries();
            opts.Print();
            // [/create-sparse]

            // [create-csv]
            var frame = Frame.ReadCsv(Path.Combine(root, "../data/stocks/msft.csv"));
            var frameDate = frame.IndexRows<DateTime>("Date").SortRowsByKey();
            var msftOpen = frameDate.GetColumn<double>("Open");
            msftOpen.Print();
            // [/create-csv]

            // ------------------------------------------------------------
            // Lookup and slicing
            // ------------------------------------------------------------

            // [lookup-key]
            // Get value for a specified int and string key
            var tenth = randNums[10];
            var one = nameNums["One"];

            // Get first and last value using index
            var fst = nameNums.GetAt(0);
            var lst = nameNums.GetAt(nameNums.KeyCount - 1);
            // [/lookup-key]

            // [lookup-opt]
            // Get value as OptionalValue<T> and use it
            var opt = opts.TryGet(5);
            if (opt.HasValue) Console.Write(opt.Value);

            // For value types, we can convert to nullable type
            int? value1 = opts.TryGet(5).AsNullable();
            int? value2 = opts.TryGetAt(0).AsNullable();
            // [/lookup-opt]

            // [lookup-ord]
            // Get value exactly at the specified key
            var jan3 = msftOpen
              .Get(new DateTime(2012, 1, 3));

            // Get value at a key or for the nearest previous date
            var beforeJan1 = msftOpen
              .Get(new DateTime(2012, 1, 1), Lookup.ExactOrSmaller);

            // Get value at a key or for the nearest later date
            var afterJan1 = msftOpen
              .Get(new DateTime(2012, 1, 1), Lookup.ExactOrGreater);
            // [/lookup-ord]

            // [lookup-slice]
            // Get a series starting/ending at 
            // the specified date (inclusive)
            var msftStartIncl = msftOpen.StartAt(new DateTime(2012, 1, 1));
            var msftEndIncl = msftOpen.EndAt(new DateTime(2012, 12, 31));

            // Get a series starting/ending after/before 
            // the specified date (exclusive)
            var msftStartExcl = msftOpen.After(new DateTime(2012, 1, 1));
            var msftEndExcl = msftOpen.Before(new DateTime(2012, 12, 31));

            // Get prices for 2012 (both keys are inclusive)
            var msft2012 = msftOpen.Between
              (new DateTime(2012, 1, 1), new DateTime(2012, 12, 31));
            // [/lookup-slice]

            // ------------------------------------------------------------
            // Statistics and calculations
            // ------------------------------------------------------------

            // [calc-stat]
            // Calculate median & mean price
            var msftMed = msft2012.Median();
            var msftAvg = msft2012.Mean();

            // Calculate sum of square differences
            var msftDiff = msft2012 - msftAvg;
            var msftSq = (msftDiff * msftDiff).Sum();
            // [/calc-stat]

            // [calc-diff]
            // Subtract previous day value from current day
            var msftChange = msft2012 - msft2012.Shift(1);

            // Use built-in Diff method to do the same
            var msftChangeAlt = msft2012.Diff(1);

            // Get biggest loss and biggest gain
            var minMsChange = msftChange.Min();
            var maxMsChange = msftChange.Max();
            // [/calc-diff]

            // [calc-custom]
            var wackyStat = msft2012.Observations.Select(kvp =>
              kvp.Value / (kvp.Key - msft2012.FirstKey()).TotalDays).Sum();
            // [/calc-custom]

            // ------------------------------------------------------------
            // Missing data
            // ------------------------------------------------------------

            // [fill-const-drop]
            // Fill missing data with constant
            var fillConst = opts.FillMissing(-1);
            fillConst.Print();

            // Drop keys with no value from the series
            var drop = opts.DropMissing();
            drop.Print();
            // [/fill-const-drop]

            // [fill-dir]
            // Fill with previous available value
            var fillFwd = opts.FillMissing(Direction.Forward);
            fillFwd.Print();

            // Fill with the next available value
            var fillBwd = opts.FillMissing(Direction.Backward);
            fillBwd.Print();
            // [/fill-dir]

            // ------------------------------------------------------------
            // Windows and chunks, grouping
            // ------------------------------------------------------------

            // [aggreg-group]
            // Group random numbers by the first digit & get distribution
            var buckets = randNums
                .GroupBy(kvp => (int)(kvp.Value * 10))
                .Select(kvp => OptionalValue.Create(kvp.Value.KeyCount));
            buckets.Print();
            // [/aggreg-group]

            // [aggreg-win]
            // Average over 25 element floating window
            var monthlyWinMean = msft2012.WindowInto(25, win => win.Mean());

            // Get floating window over 5 elements as series of series
            // and then apply average on each series individually
            var weeklyWinMean = msft2012.Window(5).Select(kvp =>
                kvp.Value.Mean());
            // [/aggreg-win]

            // [aggreg-chunk]
            // Get chunks of size 25 and mean each (disjoint) chunk
            var monthlyChunkMean = msft2012.ChunkInto(25, win => win.Mean());

            // Get series containing individual chunks (as series)
            var weeklyChunkMean = msft2012.Chunk(5).Select(kvp =>
                kvp.Value.Mean());
            // [/aggreg-chunk]

            // [aggreg-pair]
            // For each key, get the previous value and average them
            var twoDayAvgs = msft2012.Pairwise().Select(kvp =>
                (kvp.Value.Item1 + kvp.Value.Item2) / 2.0);
            // [/aggreg-pair]

            // [aggreg-any]
            msft2012.Aggregate
                ( // Get chunks while the month & year of the keys are the same
                  Aggregation.ChunkWhile<DateTime>((k1, k2) =>
                        k1.Month == k2.Month && k2.Year == k1.Year),
                    // For each chunk, return the first key as the key and
                    // either average value or missing value if it was empty
                    chunk => KeyValue.Create
                     (chunk.Data.FirstKey(),
                         chunk.Data.ValueCount > 0 ?
                            OptionalValue.Create(chunk.Data.Mean()) :
                            OptionalValue.Empty<double>()));
            // [/aggreg-any]


            // ------------------------------------------------------------
            // Operations (Select, where)
            // ------------------------------------------------------------

            // [linq-methods]
            var overMean = msft2012
              .Select(kvp => kvp.Value - msftAvg)
              .Where(kvp => kvp.Value > 0.0).KeyCount;
            // [/linq-methods]

            // [linq-query]
            var underMean =
              (from kvp in msft2012
               where kvp.Value - msftAvg < 0.0
               select kvp).KeyCount;
            // [/linq-query]

            Console.WriteLine(overMean);
            Console.WriteLine(underMean);

            // ------------------------------------------------------------
            // Indexing and sampling & resampling
            // ------------------------------------------------------------

            // [index-keys]
            // Turn DateTime keys into DateTimeOffset keys
            var byOffs = msft2012.SelectKeys(kvp =>
              new DateTimeOffset(kvp.Key));

            // Replace keys with ordinal numbers 0 .. KeyCount-1
            var byInt = msft2012.IndexOrdinally();
            // [/index-keys]

            // [index-with]
            // Replace keys with explictly specified new keys
            var byDays = numNames.IndexWith(new[] {
        DateTime.Today,
        DateTime.Today.AddDays(1.0),
        DateTime.Today.AddDays(2.0) });
            // [/index-with]
        }

        private static int? LookupEven(int i)
        {
            return i % 2 == 1 ? null : (int?)i;
        }
    }
}
