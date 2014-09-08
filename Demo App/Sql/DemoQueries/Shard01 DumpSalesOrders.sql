
/****** RUN THIS FROM SHARD 01 (AdvWrkAWSales000001)  ******/

SELECT 'Total Customer Count: ' + CAST(COUNT(1) AS varchar(10))	FROM (SELECT CustomerID FROM [Sales].[SalesOrderHeader] GROUP BY [CustomerID]) AS dv
SELECT 'Sales Order Header Count: ' + CAST(COUNT(1) AS varchar(10))  FROM [Sales].[SalesOrderHeader]
SELECT 'Sales Order Detail Count: ' + CAST(COUNT(1) AS varchar(10))  FROM [Sales].[SalesOrderDetail]
SELECT 'Shopping Cart Item Count: ' + CAST(COUNT(1) AS varchar(10))  FROM [Sales].[ShoppingCartItem]

SELECT *  FROM [Sales].[SalesOrderHeader]
