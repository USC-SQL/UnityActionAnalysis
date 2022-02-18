using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Z3;

namespace UnitySymexCrawler
{
    public class DatabaseUtil : IDisposable
    {
        private SqliteConnection connection;
        public DatabaseUtil(string dbFile)
        {
            connection = new SqliteConnection("Data Source=" + dbFile);
            connection.Open();
            InitDB();
        }

        private void InitDB()
        {
            var initCommand = connection.CreateCommand();
            initCommand.CommandText =
                "create table methods (id integer primary key, signature text);\n" +
                "create table smcarguments (argindex integer, value text, symcallid integer, pathid integer);\n" +
                "create table symbolicmethodcalls (symcallid integer, method integer, pathid integer);\n" +
                "create table paths (id integer primary key, method integer, condition text);\n";
            initCommand.ExecuteNonQuery();
        }

        private int GetMethodId(IMethod method)
        {
            string signature = Helpers.GetMethodSignature(method);
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "select id from methods where signature = $signature";
            selectCommand.Parameters.AddWithValue("$signature", signature);
            using (var reader = selectCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "insert into methods (signature) values ($signature); select last_insert_rowid();";
            insertCommand.Parameters.AddWithValue("$signature", signature);
            return Convert.ToInt32(insertCommand.ExecuteScalar());
        }

        private void AddSymbolicMethodCallArgument(int symcallId, int pathId, int argIndex, string value)
        {
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"insert into smcarguments (argindex, value, symcallid, pathid) values ($argIndex, $value, $symcallId, $pathId)";
            insertCommand.Parameters.AddWithValue("$argIndex", argIndex);
            insertCommand.Parameters.AddWithValue("$value", value);
            insertCommand.Parameters.AddWithValue("$symcallId", symcallId);
            insertCommand.Parameters.AddWithValue("$pathId", pathId);
            insertCommand.ExecuteNonQuery();
        }

        private void AddSymbolicMethodCall(int symcallId, int pathId, SymbolicMethodCall smc, SymexState state)
        {
            int methodId = GetMethodId(smc.method);
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"insert into symbolicmethodcalls (symcallid, method, pathid) values ($symcallId, $methodId, $pathId)";
            insertCommand.Parameters.AddWithValue("$symcallId", symcallId);
            insertCommand.Parameters.AddWithValue("$methodId", methodId);
            insertCommand.Parameters.AddWithValue("$pathId", pathId);
            insertCommand.ExecuteNonQuery();
            for (int i = 0, n = smc.args.Count; i < n; ++i)
            {
                Expr arg = smc.args[i];
                AddSymbolicMethodCallArgument(symcallId, pathId, i, JsonSerializer.Serialize(state.SerializeExpr(arg)));
            }
        }

        private void AddPath(SymexState state, int pathId, int methodId)
        {
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"insert into paths (id, method, condition) values ($pathId, $methodId, $condition)";
            insertCommand.Parameters.AddWithValue("$pathId", pathId);
            insertCommand.Parameters.AddWithValue("$methodId", methodId);
            insertCommand.Parameters.AddWithValue("$condition", state.PathConditionString());
            foreach (var p in state.symbolicMethodCalls)
            {
                AddSymbolicMethodCall(p.Key, pathId, p.Value, state);
            }
        }

        public void AddPaths(IMethod method, SymexMachine machine)
        {
            using (var transaction = connection.BeginTransaction())
            {
                int methodId = GetMethodId(method);
                int pathId = 1;
                foreach (SymexState state in machine.States)
                {
                    AddPath(state, pathId, methodId);
                    ++pathId;
                }
                transaction.Commit();
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
