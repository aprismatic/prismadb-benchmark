using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace PrismaBenchmark
{
    static class QueryConstructor
    {
        public static string ConstructInsertQuery(string tableName, List<ArrayList> tuples)
        {
            // build query
            StringBuilder query = new StringBuilder();
            query.Append(String.Format("INSERT INTO {0} (a,b,c,d,e) VALUES ", tableName));
            for (var i = 0; i < tuples.Count(); i++)
            {
                var tuple = tuples[i];
                StringBuilder queryTail = new StringBuilder("(");
                for (var j = 0; j < tuple.Count; j++)
                {
                    var value = tuple[j];
                    if (value.GetType().Equals("".GetType()))
                        value = "\"" + value + "\"";
                    queryTail.Append(value);
                    if (j == tuple.Count - 1)
                        queryTail.Append(")");
                    else
                        queryTail.Append(",");
                }
                query.Append(queryTail);
                if (i == tuples.Count - 1)
                    query.Append(";");
                else
                    query.Append(",");
            }
            return query.ToString();
        }

        public static string ConstructSelectQuery(string operation, int targetA) // select on column a
        {
            return String.Format("SELECT {0} FROM t1 WHERE a={1};", operation, targetA);
        }

        public static string ConstructSelectWithoutQuery(int targetA) // select on column a
        {
            return String.Format("SELECT c FROM t2 WHERE c={0} LIMIT 5;", targetA);
        }

        public static string ConstructSelectJoinQuery(int targetA) // select on column a
        {
            return String.Format("SELECT t1.a, t1.b, t2.a, t2.b FROM t1 INNER JOIN t2 ON t1.a = t2.a WHERE t1.a={0};", targetA);
        }

        public static string ConstructUpdateQuery(int targetA, ArrayList tuple) // set all matches to the same values
        {
            return String.Format("UPDATE t1 SET a={0}, b={1}, c={2}, d=\"{3}\", e=\"{4}\" WHERE a={0};", targetA, tuple[1], tuple[2], tuple[3], tuple[4]);
        }

        public static string ConstructDeleteQuery(int targetA) // select on column a
        {
            return String.Format("DELETE FROM t1 WHERE a={0};", targetA);
        }

        public static string ConstructDecryptQuery(bool check) // select on column a
        {
            return String.Format("PRISMADB DECRYPT t1.a {0};", check? "STATUS" : "");
        }

        public static string ConstructEncryptQuery(bool check, string type) // select on column a
        {
            return String.Format("PRISMADB ENCRYPT t1.a FOR({1}) {0};", check ? "STATUS" : "", type);
        }

        public static string ConstructUpdateKeyQuery(bool check) // select on column a
        {
            return String.Format("PRISMADB UPDATE KEY {0};", check ? "STATUS" : "");
        }
    }
}
