using System;
using System.Globalization;
using Microsoft.AzureCat.Patterns.CityHash;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.AzureCat.Patterns.DataElasticity.IntegrationTests
{
    /// <summary>
    /// Summary description for CityHashTests
    /// </summary>
    [TestClass]
    public class CityHashTests
    {
        #region methods

        [TestMethod]
        public void SampleIntegerDistributionTest()
        {
            var map = CreateSampleShardMap();
            var results = new[] {0, 0, 0, 0, 0};

            const int testUpTo = 10;

            for (var i = 1; i < testUpTo; i++)
            {
                var hash = (long) CityHasher.CityHash64String(i.ToString(CultureInfo.InvariantCulture));

                var s = 0;
                foreach (var shard in map.Shards)
                {
                    if (hash >= shard.LowDistributionKey && hash < shard.HighDistributionKey)
                    {
                        ++results[s];
                        continue;
                    }
                    ++s;
                }
            }

            for (var i = 0; i < 5; i++)
            {
                Console.WriteLine("Shard {0}: {1}", i + 1, results[i]);
            }
        }

        public void SampleIntegerTest()
        {
            for (var i = 0; i < 1000; i++)
            {
                var s = i.ToString(CultureInfo.InvariantCulture);
                Console.WriteLine(s.PadLeft(5) + ": " + (long) CityHasher.CityHash64String(s));
            }
        }

        [TestMethod]
        public void SampleStringDistributionTest()
        {
            var names = new[] {"jodell", "mnigels", "todell", "aadams", "bjoel", "sjones"};

            foreach (var name in names)
            {
                Console.WriteLine(name.PadLeft(15) + ": " + (long) CityHasher.CityHash64String(name));
            }
        }

        private ShardMap CreateSampleShardMap()
        {
            var map = new ShardMap
            {
                ShardMapID = 1
            };

            var shard =
                new RangeShard
                {
                    LowDistributionKey = -9223372036854775808,
                    HighDistributionKey = -5534023222112865486
                };

            map.Shards.Add(shard);

            shard =
                new RangeShard
                {
                    LowDistributionKey = -5534023222112865485,
                    HighDistributionKey = -1844674407370955164
                };

            map.Shards.Add(shard);

            shard =
                new RangeShard
                {
                    LowDistributionKey = -1844674407370955163,
                    HighDistributionKey = 1844674407370955158
                };

            map.Shards.Add(shard);

            shard =
                new RangeShard
                {
                    LowDistributionKey = 1844674407370955159,
                    HighDistributionKey = 5534023222112865480
                };

            map.Shards.Add(shard);

            shard =
                new RangeShard
                {
                    LowDistributionKey = 5534023222112865481,
                    HighDistributionKey = 9223372036854775807
                };

            map.Shards.Add(shard);

            return map;
        }

        #endregion
    }
}