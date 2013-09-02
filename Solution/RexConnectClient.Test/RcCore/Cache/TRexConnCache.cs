using NUnit.Framework;
using RexConnectClient.Core.Cache;
using System.Collections.Generic;
using RexConnectClient.Test.Utils;
using System;

namespace RexConnectClient.Test.RcCore.Cache {

	/*================================================================================================*/
	[TestFixture]
	public class TRexConnCache : TestBase {


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void New() {
			const string host = "local";
			const int port = 1234;

			var c = new RexConnCache(host, port);

			Assert.AreEqual(host, c.HostName, "Incorrect HostName.");
			Assert.AreEqual(port, c.Port, "Incorrect Port.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void AddThenObtainScripts() {
			var c = new RexConnCache("local", 123);
			int n = 0;
			
			var pairs = new Dictionary<int, string>();
			pairs.Add(1, "one");
			pairs.Add(99, "ninety-nine");
			pairs.Add(1234, "one thousand, two hundred and thirty-four");
			pairs.Add(49, "seven x seven");
			
			foreach ( KeyValuePair<int, string> pair in pairs ) {
				c.AddCachedScript(pair.Key, pair.Value);
				Assert.AreEqual(++n, c.GetKeyCount(), "Incorrect count at key "+pair.Key);
			}
			
			foreach ( KeyValuePair<int, string> pair in pairs ) {
				string script = c.GetScriptByKey(pair.Key);
				Assert.AreEqual(pair.Value, script, "Incorrect script for key "+pair.Key);
			}
			
			foreach ( KeyValuePair<int, string> pair in pairs ) {
				int? key = c.FindScriptKey(pair.Value);
				Assert.AreEqual(pair.Key, key, "Incorrect key for script "+pair.Value);
			}
		}
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetScriptByKeyNotFound() {
			var c = new RexConnCache("local", 123);
			TestUtil.CheckThrows<KeyNotFoundException>(true, () => c.GetScriptByKey(1));
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void FindScriptNotFound() {
			var c = new RexConnCache("local", 123);
			Assert.Null(c.FindScriptKey("x"), "Result should be null.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void AddExistingScriptSame() {
			var c = new RexConnCache("local", 123);
			c.AddCachedScript(99, "abcd");
			c.AddCachedScript(99, "abcd");
			Assert.AreEqual(1, c.GetKeyCount(), "Incorrect key count.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void AddExistingScriptDifferent() {
			var c = new RexConnCache("local", 123);
			c.AddCachedScript(99, "abcd");
			TestUtil.CheckThrows<Exception>(true, () => c.AddCachedScript(99, "different"));
		}
		
	}

}