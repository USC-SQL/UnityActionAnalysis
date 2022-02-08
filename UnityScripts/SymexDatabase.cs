using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.Z3;
using Microsoft.Data.Sqlite;

namespace UnitySymexCrawler
{
    public class SymexDatabase : IDisposable
    {
        private SqliteConnection connection;

        private HashSet<string> symexMethods;

        public SymexDatabase(string dbFile)
        {
            connection = new SqliteConnection(dbFile);
            connection.Open();
            ReadDB();
        }

        private string GetMethodSignature(int methodId)
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "select signature from methods where id = $methodId";
            selectCommand.Parameters.AddWithValue("$methodId", methodId);
            using (var reader = selectCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetString(0);
                }
                else
                {
                    return null;
                }
            }
        }

        private void ReadDB()
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "select distinct method from paths";

            List<int> methodIds = new List<int>();
            using (var reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    methodIds.Add(reader.GetInt32(0));
                }
            }

            symexMethods = new HashSet<string>(methodIds.Select(id => GetMethodSignature(id)));
        }

        private static string GetMethodSignature(MethodInfo m)
        {
            string args = string.Join(";", m.GetParameters().Select(p => p.ParameterType.FullName));
            var assembly = m.DeclaringType.Assembly;
            return m.DeclaringType.FullName + (assembly != null ? "," + assembly.GetName().Name : "") + ":" + m.Name + "(" + args + ")";
        }

        public bool IsSymexMethod(MethodInfo m)
        {
            return symexMethods.Contains(GetMethodSignature(m));
        }

        private int GetMethodId(MethodInfo m)
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "select id from methods where signature = $signature";
            selectCommand.Parameters.AddWithValue("$signature", GetMethodSignature(m));
            return Convert.ToInt32(selectCommand.ExecuteScalar());
        }
        public List<SymexPath> GetSymexPaths(MethodInfo m)
        {
            int methodId = GetMethodId(m);
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "select id, condition from paths where method = $methodId";
            selectCommand.Parameters.AddWithValue("$methodId", methodId);

            List<SymexPath> paths = new List<SymexPath>();
            using (var reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    paths.Add(new SymexPath(reader.GetInt32(0), reader.GetString(1), this));
                }
            }

            return paths;
        }

        public SymbolicMethodCall GetSymbolicMethodCall(int symcallId, SymexPath path)
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "select method from symbolicmethodcalls where symcallid = $symcallId and pathid = $pathId";
            selectCommand.Parameters.AddWithValue("$symcallId", symcallId);
            selectCommand.Parameters.AddWithValue("$pathId", path.pathId);
            int methodId = Convert.ToInt32(selectCommand.ExecuteScalar());
            string signature = GetMethodSignature(methodId);
            return new SymbolicMethodCall(symcallId, path, SymexHelpers.GetMethodFromSignature(signature));
        }

        public SymbolicMethodCallArgument GetSymbolicMethodCallArgument(int argIndex, SymbolicMethodCall smc)
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText =
                "select value from smcarguments where symcallid = $symcallId and pathid = $pathid and argindex = $argIndex";
            selectCommand.Parameters.AddWithValue("$symcallId", smc.symcallId);
            selectCommand.Parameters.AddWithValue("$pathid", smc.path.pathId);
            selectCommand.Parameters.AddWithValue("$argIndex", argIndex);
            using (var reader = selectCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    string value = reader.GetString(0);
                    return new SymbolicMethodCallArgument(
                        SymexValue.Parse(value), argIndex, smc.symcallId, smc.path.pathId);
                }
                else
                {
                    throw new ArgumentException("no such symbolic method call");
                }
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}