#region usings

using System;
using System.Web.UI;
using Microsoft.AzureCat.Patterns.Data.Elasticity;
using Microsoft.AzureCat.Patterns.Data.Elasticity.Models;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Azure.WebConsole.Performance
{
    public partial class TenantDriverTests : Page
    {
        #region methods

        protected void LookupTenants_Click(object sender, EventArgs e)
        {
            var startTime = DateTime.Now.Ticks;
            var count = int.Parse(tenantLookupCount.Text);
            for (var i = 0; i < count; i++)
            {
                var tenant = Tenant.Load(workloadGroupName.Text, "user" + i);
            }
            var endtime = DateTime.Now.Ticks;
            showOperationTime(startTime, endtime);
        }

        protected void MakeRangeMap_Click(object sender, EventArgs e)
        {
            //var startTime = DateTime.Now.Ticks;
            //var driver = ScaleOutTenantManager.GetManager().GetTenantConnectionDriver("UserData");

            //var count = int.Parse(makeRangeSize.Text);
            //var increment = (long.MaxValue/count)*2;
            //var rangeLowValue = Int64.MinValue;
            //var rangeHighValue = rangeLowValue += increment;
            //var counter = 0;
            //while (rangeHighValue != long.MaxValue)
            //{
            //    driver.PublishShard(workloadGroupName.Text, new Shard
            //    {
            //        Catalog = "catalog-" + DateTime.Now.Ticks,
            //        LowDistributionKey = rangeLowValue,
            //        HighDistributionKey = rangeHighValue,
            //        ServerInstanceName = "server",
            //    });

            //    rangeLowValue = rangeHighValue + 1;
            //    if (counter + 1 == count)
            //    {
            //        rangeHighValue = Int64.MaxValue;
            //    }
            //    else
            //    {
            //        rangeHighValue += increment;
            //    }
            //    counter++;
            //}
            //var endtime = DateTime.Now.Ticks;
            //showOperationTime(startTime, endtime);
        }

        protected void MakeTenant_Click(object sender, EventArgs e)
        {
            //var startTime = DateTime.Now.Ticks;
            //var driver = ScaleOutTenantManager.GetManager().GetTenantConnectionDriver("UserData");
            //var count = int.Parse(makeTenantsCount.Text);

            //for (var i = 0; i < count; i++)
            //{
            //    var t = new Tenant
            //    {
            //        ServerInstanceName = "server",
            //        Catalog = "catalog-" + DateTime.Now.Ticks,
            //        TableGroupName = workloadGroupName.Text,
            //        Status = TenantStatus.Active,
            //        DistributionKey = driver.GetDistributionKeyForShardKey("user" + i)
            //    };
            //    driver.PublishTenant(t);
            //}
            //var endtime = DateTime.Now.Ticks;
            //showOperationTime(startTime, endtime);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private void showOperationTime(long start, long end)
        {
            var RunTime = new TimeSpan(end - start);
            lastTestSpeed.Text = RunTime.TotalMilliseconds + " ms";
        }

        #endregion
    }
}