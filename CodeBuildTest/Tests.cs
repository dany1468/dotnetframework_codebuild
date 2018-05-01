using System;
using Npgsql;
using NPoco;
using NUnit.Framework;
using Respawn;

namespace CodeBuildTest
{
	[TestFixture]
	public class Tests
	{
		private NpgsqlConnection _connection;
		private Database _database;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var rootConnString = "Server=pg;Port=5432;User ID=postgres;Password=password;database=postgres";
			var dbConnString = "Server=pg;Port=5432;User ID=postgres;Password=password;database={0}";
			var dbName = DateTime.Now.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString("N");
			using (var connection = new NpgsqlConnection(rootConnString))
			{
				connection.Open();

				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "create database \"" + dbName + "\"";
					cmd.ExecuteNonQuery();
				}
			}
			_connection = new NpgsqlConnection(string.Format(dbConnString, dbName));
			_connection.Open();

			_database = new Database(_connection, DatabaseType.PostgreSQL);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			_connection?.Close();
			_connection?.Dispose();
			_connection = null;
		}

		[Test]
		public void Test1()
		{
			_database.Execute("create table \"foo\" (value int)");

			for (var i = 0; i < 100; i++)
			{
				_database.Execute("INSERT INTO \"foo\" VALUES (@0)", i);
			}

			Assert.That(_database.ExecuteScalar<int>("SELECT COUNT(1) FROM \"foo\""), Is.EqualTo(100));

			var checkpoint = new Checkpoint
			{
				DbAdapter = DbAdapter.Postgres,
				SchemasToInclude = new [] { "public" }
			};
			var result = checkpoint.Reset(_connection);
			result.Wait();

			Assert.That(_database.ExecuteScalar<int>("SELECT COUNT(1) FROM \"foo\""), Is.EqualTo(0));
		}
	}
}
