
/*** RUN FROM POINT SHARD AdvWrkAWSales_HighVolume ***/

SELECT 'Total Customer Count: ' + CAST(COUNT(1) AS varchar(10))	FROM (SELECT CustomerID FROM [Sales].[SalesOrderHeader] GROUP BY [CustomerID]) AS dv
SELECT 'Sales Order Header Count: ' + CAST(COUNT(*) AS varchar)  FROM [Sales].[SalesOrderHeader]
SELECT 'Sales Order Detail Count: ' + CAST(COUNT(*) AS varchar)  FROM [Sales].[SalesOrderDetail]
SELECT 'Shopping Cart Item Count: ' + CAST(COUNT(*) AS varchar)  FROM [Sales].[ShoppingCartItem]

SELECT *  FROM [Sales].[SalesOrderHeader]
