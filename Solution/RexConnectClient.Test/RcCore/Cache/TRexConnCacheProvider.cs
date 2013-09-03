using System.Collections.Generic;
using NUnit.Framework;
using RexConnectClient.Core.Cache;

namespace RexConnectClient.Test.RcCore.Cache {

	/*================================================================================================*/
	[TestFixture]
	public class TRexConnCacheProvider : TestBase {


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCache() {
			var cp = new RexConnCacheProvider();
			
			IRexConnCache c = cp.GetCache("first", 123);
			Assert.NotNull(c, "Result should be filled.");
			Assert.AreEqual(1, cp.GetCacheCount(), "Incorrect cache count.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCacheSame() {
			var cp = new RexConnCacheProvider();
			IRexConnCache c1 = cp.GetCache("first", 123);
			IRexConnCache c2 = cp.GetCache("first", 123);
			
			Assert.AreEqual(c1, c2, "Cache results should be the same.");
			
			Assert.AreEqual(1, cp.GetCacheCount(), "Incorrect cache count.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCacheDifferent() {
			var cp = new RexConnCacheProvider();
			IRexConnCache c1 = cp.GetCache("first", 123);
			IRexConnCache c2 = cp.GetCache("second", 123);
			IRexConnCache c3 = cp.GetCache("first", 1234);
			
			Assert.AreNotEqual(c1, c2, "Cache 1/2 results should be different.");
			Assert.AreNotEqual(c1, c3, "Cache 1/3 results should be different.");
			Assert.AreNotEqual(c2, c3, "Cache 2/3 results should be different.");
			
			Assert.AreEqual(3, cp.GetCacheCount(), "Incorrect cache count.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCacheKeys() {
			var cp = new RexConnCacheProvider();
			cp.GetCache("first", 1);
			cp.GetCache("second", 2);
			cp.GetCache("third", 3);

			ICollection<string> keys = cp.GetCacheKeys();

			Assert.AreEqual(3, keys.Count, "Incorrect key count.");
			Assert.True(keys.Contains("first:1"), "Missing #1.");
			Assert.True(keys.Contains("second:2"), "Missing #2.");
			Assert.True(keys.Contains("third:3"), "Missing #3.");
		}
		
	}

}