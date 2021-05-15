﻿using System;
using System.Linq;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

var sqlText = "SELECT TOP 100 * FROM dto.tabUsers WHERE Id > 10 ORDER BY Id DESC";
var result = Parser.Parse(sqlText);
Console.WriteLine(result.BatchCount);
Console.WriteLine(result.Script.Sql);

// var selectStatement = result.Script.Batches[0].Children.OfType<SqlSelectStatement>().First();
// Console.WriteLine(selectStatement.Sql);
//
// Console.WriteLine(selectStatement.SelectSpecification.Sql);
// Console.WriteLine(selectStatement.SelectSpecification.QueryExpression.Sql);
// Console.WriteLine(selectStatement.SelectSpecification.OrderByClause?.Sql);
//
// foreach (var clause in selectStatement.SelectSpecification.Children)
// {
//     Console.WriteLine($"Sql:{clause.Sql}, Type:{clause.GetType().Name}");
// }

Console.WriteLine("-------------------------------");
IterateSqlNode(result.Script);

sqlText = @"
SELECT Id AS UserId, [Name] AS UserName, City AS [From] FROM dto.tabUsers
WHERE Id>10
ORDER BY Id DESC
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY
";
IterateSqlNode(Parser.Parse(sqlText).Script);

Console.WriteLine(GetCountSql(sqlText));

static void IterateSqlNode(SqlCodeObject sqlCodeObject, int indent=0)
{
    if (sqlCodeObject.Children == null) 
        return;
    foreach (var child in sqlCodeObject.Children)
    {
        Console.WriteLine($"{new string(' ', indent)}Sql:{child.Sql}, Type:{child.GetType().Name}");
        IterateSqlNode(child, indent+2);
    }
}

static string GetCountSql(string sql)
{
    var result = Parser.Parse(sql);
    if (result.Script is null)
    {
        throw new ArgumentException("Invalid query", nameof(sql));
    }

    var sqlQuery = result.Script.Batches[0].Children.OfType<SqlSelectStatement>().FirstOrDefault()
        ?.Children.OfType<SqlSelectSpecification>().FirstOrDefault()
        ?.Children.OfType<SqlQuerySpecification>().FirstOrDefault();
    if (sqlQuery is null)
    {
        throw new ArgumentException("Invalid query", nameof(sql));
    }

    return sqlQuery.Sql.Replace(sqlQuery.SelectClause.Sql, "SELECT COUNT(1)");
}
