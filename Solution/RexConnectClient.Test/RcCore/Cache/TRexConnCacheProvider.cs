using System;
using System.Collections.Generic;
using NUnit.Framework;
using RexConnectClient.Core.Cache;
using RexConnectClient.Test.Utils;

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
		
	}

}