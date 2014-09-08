#region usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.AzureCat.Patterns.DataElasticity.Client;
using Microsoft.AzureCat.Patterns.DataElasticity.Models;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

#endregion

namespace Microsoft.AzureCat.Patterns.DataElasticity.Demo.AdventureWorks.Test
{
    internal class TestDataBuilder
    {
        #region fields

        private readonly string _referenceDatabaseConnectionString;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataBuilder"/> class.
        /// </summary>
        /// <param name="referenceDatabaseConnectionString">The reference database connection string.</param>
        public TestDataBuilder(string referenceDatabaseConnectionString)
        {
            _referenceDatabaseConnectionString = referenceDatabaseConnectionString;
        }

        #endregion

        #region methods

        public void AddTestDataInShardSet(string shardSetName, int initialTestCustomerID,
            int numberOfTestCustomers, int numberOfTestOrdersPerCustomer)
        {
            // set up header insert command for test data
            var headerInsertCommand = new SqlCommand("Sales.CreateSalesOrderHeader")
            {
                CommandType = CommandType.StoredProcedure
            };

            var customerIDParameter = headerInsertCommand.Parameters.Add(new SqlParameter("CustomerID", SqlDbType.Int));
            var headerSalesOrderIDParam =
                headerInsertCommand.Parameters.Add(new SqlParameter("SalesOrderID", SqlDbType.Int));

            AddTestSalesOrderParametersTo(headerInsertCommand);

            // set up detail insert command for test data
            var detailInsertCommand = new SqlCommand("Sales.CreateSalesOrderLineItem")
            {
                CommandType = CommandType.StoredProcedure
            };

            var detailSalesOrderIDParam =
                detailInsertCommand.Parameters.Add(new SqlParameter("SalesOrderID", SqlDbType.Int));
            var salesOrderDetailLineNum =
                detailInsertCommand.Parameters.Add(new SqlParameter("SalesOrderLineNum", SqlDbType.Int));

            AddTestSalesDetailParametersTo(detailInsertCommand);

            // set up shopping cart insert command for test data
            var shoppingCartItemInsertCommand =
                new SqlCommand("Sales.CreateShoppingCartItem")
                {
                    CommandType = CommandType.StoredProcedure,
                };

            var shoppingCartCustomerIDParameter = shoppingCartItemInsertCommand.Parameters.Add(new SqlParameter("CustomerID", SqlDbType.Int));
            var shoppingCartLineNumParam =
                shoppingCartItemInsertCommand.Parameters.Add(new SqlParameter("ShoppingCartLineNum", SqlDbType.Int));
            var quantityParam = shoppingCartItemInsertCommand.Parameters.Add(new SqlParameter("Quantity", SqlDbType.Int));
            var productIDParam =
                shoppingCartItemInsertCommand.Parameters.Add(new SqlParameter("ProductID", SqlDbType.Int));


            for (var customerID = initialTestCustomerID; customerID < numberOfTestCustomers + initialTestCustomerID; customerID++)
            {
                // get unique ids for sales orders
                var salesOrderIDList = GetUniqueSalesOrderIDs(numberOfTestOrdersPerCustomer);

                // the database is sharded on customer id
                var shardingKey = customerID.ToString(CultureInfo.InvariantCulture);

                // load the Shardlet object to the the appropriate connection string
                var connection = new ElasticSqlConnection(shardSetName, shardingKey);
                using (connection)
                {
                    connection.Open();

                    foreach (var salesOrderID in salesOrderIDList)
                    {

                        // add sales order header
                        headerInsertCommand.Connection = connection.Current;

                        headerSalesOrderIDParam.Value = salesOrderID;
                        customerIDParameter.Value = customerID;

                        headerInsertCommand.ExecuteNonQuery();

                        // add sales order details
                        detailInsertCommand.Connection = connection.Current;

                        for (var i = 1; i < 5; i++)
                        {
                            detailSalesOrderIDParam.Value = salesOrderID;
                            salesOrderDetailLineNum.Value = i;

                            detailInsertCommand.ExecuteNonQuery();
                        }
                    }

                    // add shopping cart items
                    shoppingCartItemInsertCommand.Connection = connection.Current;

                    for (var i = 1; i < 5; i++)
                    {
                        shoppingCartCustomerIDParameter.Value = customerID;
                        quantityParam.Value = i;
                        productIDParam.Value = i;
                        shoppingCartLineNumParam.Value = i;

                        shoppingCartItemInsertCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Adds test sales orders to database at a specific connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="initialTestCustomerID">The initial test customer identifier.</param>
        /// <param name="numberOfTestCustomers">The number of test customers.</param>
        /// <param name="numberOfTestOrders">The number of test orders per customer.</param>
        public void AddTestSalesOrdersInDatabase(string connectionString, int initialTestCustomerID,
            int numberOfTestCustomers, int numberOfTestOrders)
        {
            // set up insert command for test data
            var headerInsertCommand = new SqlCommand("Sales.CreateSalesOrderHeader")
            {
                CommandType = CommandType.StoredProcedure
            };

            var customerIDParameter = headerInsertCommand.Parameters.Add(new SqlParameter("CustomerID", SqlDbType.Int));
            var salesOrderIDParam = headerInsertCommand.Parameters.Add(new SqlParameter("SalesOrderID", SqlDbType.Int));

            AddTestSalesOrderParametersTo(headerInsertCommand);

            // set up detail insert commands for test data
            var detailInsertCommand = new SqlCommand("Sales.CreateSalesOrderLineItem")
            {
                CommandType = CommandType.StoredProcedure
            };

            var detailSalesOrderIDParam =
                detailInsertCommand.Parameters.Add(new SqlParameter("SalesOrderID", SqlDbType.Int));
            var salesOrderDetailLineNum =
                detailInsertCommand.Parameters.Add(new SqlParameter("SalesOrderLineNum", SqlDbType.Int));

            AddTestSalesDetailParametersTo(detailInsertCommand);

            using (var connection = new ReliableSqlConnection(connectionString))
            {
                connection.Open();
                for (var uniqueCustomerID = initialTestCustomerID;
                    uniqueCustomerID < initialTestCustomerID + numberOfTestCustomers;
                    uniqueCustomerID++)
                {
                    // get unique ids for sales orders
                    var uniqueSalesOrderIDList = GetUniqueSalesOrderIDs(numberOfTestOrders);

                    // create some test data
                    foreach (var salesOrderID in uniqueSalesOrderIDList)
                    {
                        headerInsertCommand.Connection = connection.Current;

                        customerIDParameter.Value = uniqueCustomerID;
                        salesOrderIDParam.Value = salesOrderID;

                        headerInsertCommand.ExecuteNonQuery();

                        detailInsertCommand.Connection = connection.Current;

                        for (var i = 1; i < 5; i++)
                        {
                            detailSalesOrderIDParam.Value = salesOrderID;
                            salesOrderDetailLineNum.Value = i;

                            detailInsertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public void AddTestShoppingCartItemsInDatabase(string connectionString, int initialTestCustomerID,
            int numberOfTestCustomers, int numberOfShoppingCartItems)
        {
            // create some test data
            using (var connection = new ReliableSqlConnection(connectionString))
            {
                connection.Open();

                // set up insert command for test data
                var insertCommand =
                    new SqlCommand("Sales.CreateShoppingCartItem")
                    {
                        CommandType = CommandType.StoredProcedure,
                        Connection = connection.Current
                    };

                var customerIDParameter = insertCommand.Parameters.Add(new SqlParameter("CustomerID", SqlDbType.Int));
                var shoppingCartLineNumParam =
                    insertCommand.Parameters.Add(new SqlParameter("ShoppingCartLineNum", SqlDbType.Int));
                var quantityParam = insertCommand.Parameters.Add(new SqlParameter("Quantity", SqlDbType.Int) { Value = 1 });
                var productIDParam =
                    insertCommand.Parameters.Add(new SqlParameter("ProductID", SqlDbType.Int) { Value = 1 });


                for (var uniqueCustomerID = initialTestCustomerID;
                    uniqueCustomerID < initialTestCustomerID + numberOfTestCustomers;
                    uniqueCustomerID++)
                {
                    for (var shoppingCartLineNum = 1;
                        shoppingCartLineNum <= numberOfShoppingCartItems;
                        shoppingCartLineNum++)
                    {
                        customerIDParameter.Value = uniqueCustomerID;
                        shoppingCartLineNumParam.Value = shoppingCartLineNum;
                        quantityParam.Value = shoppingCartLineNum;
                        productIDParam.Value = shoppingCartLineNum;

                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        public void ResetUniqueSalesOrderIDs()
        {
            using (var masterConnection = new ReliableSqlConnection(_referenceDatabaseConnectionString))
            {
                masterConnection.Open();

                var command = new SqlCommand("TRUNCATE table [Sales].[CustomerSequence]", masterConnection.Current);

                command.ExecuteNonQuery();
            }
        }

        private static void AddTestSalesDetailParametersTo(SqlCommand insertCommand)
        {
            insertCommand.Parameters.Add(new SqlParameter("CarrierTrackingNumber", SqlDbType.NVarChar) { Value = "12345" });
            insertCommand.Parameters.Add(new SqlParameter("OrderQty", SqlDbType.Int) { Value = 1 });
            insertCommand.Parameters.Add(new SqlParameter("ProductID", SqlDbType.Int) { Value = 1 });
            insertCommand.Parameters.Add(new SqlParameter("SpecialOfferID", SqlDbType.Int) { Value = 1 });
            insertCommand.Parameters.Add(new SqlParameter("UnitPrice", SqlDbType.Money) { Value = 10 });
            insertCommand.Parameters.Add(new SqlParameter("UnitPriceDiscount", SqlDbType.Money) { Value = 0 });
        }

        private static void AddTestSalesOrderParametersTo(SqlCommand insertCommand)
        {
            insertCommand.Parameters.Add(new SqlParameter("OrderDate", SqlDbType.Date) { Value = DateTime.Today });
            insertCommand.Parameters.Add(new SqlParameter("DueDate", SqlDbType.Date) { Value = DateTime.Today });
            insertCommand.Parameters.Add(new SqlParameter("ShipDate", SqlDbType.Date) { Value = DateTime.Today });
            insertCommand.Parameters.Add(new SqlParameter("Status", SqlDbType.TinyInt) { Value = 1 });
            insertCommand.Parameters.Add(new SqlParameter("OnlineOrderFlag", SqlDbType.Bit) { Value = 1 });
            insertCommand.Parameters.Add(new SqlParameter("BillToAddressID", SqlDbType.Int) { Value = 1 });
            insertCommand.Parameters.Add(new SqlParameter("ShipToAddressID", SqlDbType.Int) { Value = 1 });
            insertCommand.Parameters.Add(new SqlParameter("ShipMethodID", SqlDbType.Int) { Value = 1 });
            insertCommand.Parameters.Add(new SqlParameter("SubTotal", SqlDbType.Money) { Value = 100 });
            insertCommand.Parameters.Add(new SqlParameter("TaxAmt", SqlDbType.Money) { Value = 10 });
            insertCommand.Parameters.Add(new SqlParameter("Freight", SqlDbType.Money) { Value = 5 });
        }

        private IEnumerable<int> GetUniqueSalesOrderIDs(int numberOfTestShardlets)
        {
            var uniqueIDList = new List<int>(numberOfTestShardlets);
            using (var masterConnection = new ReliableSqlConnection(_referenceDatabaseConnectionString))
            {
                masterConnection.Open();

                var command = new SqlCommand("[Sales].[GetCustomerSequenceIDSet]", masterConnection.Current)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("SetSize", SqlDbType.Int) { Value = numberOfTestShardlets });

                var reader = command.ExecuteReader();

                while (reader.Read())
                    uniqueIDList.Add(reader.GetInt32(0));

                reader.Close();
            }
            return uniqueIDList;
        }

        #endregion
    }
}