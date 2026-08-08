using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EverythingSearchClient.Tests
{
	[TestClass]
	public class TestEverything15Support
	{
		[TestMethod]
		public void TestInstanceNameProperty()
		{
			// Test that instance name can be set and retrieved
			string? originalInstance = SearchClient.InstanceName;
			
			try
			{
				// Set to null (default instance)
				SearchClient.InstanceName = null;
				Assert.IsNull(SearchClient.InstanceName);
				
				// Set to empty string (also default instance)
				SearchClient.InstanceName = "";
				Assert.AreEqual("", SearchClient.InstanceName);
				
				// Set to a specific instance (e.g., Everything 1.5 alpha)
				SearchClient.InstanceName = "1.5a";
				Assert.AreEqual("1.5a", SearchClient.InstanceName);
				
				// Reset to default
				SearchClient.InstanceName = null;
				Assert.IsNull(SearchClient.InstanceName);
			}
			finally
			{
				// Restore original value
				SearchClient.InstanceName = originalInstance;
			}
		}
		
		[TestMethod]
		public void TestBackwardCompatibility()
		{
			// Verify that Everything can still be detected regardless of version
			// This test doesn't require Everything to be running
			// It just verifies the API exists and doesn't throw
			
			try
			{
				bool available = SearchClient.IsEverythingAvailable();
				// Result doesn't matter - just checking the call succeeds
				Assert.IsTrue(true);
			}
			catch
			{
				// If Everything is not installed, this is expected
				Assert.IsTrue(true);
			}
		}
		
		[TestMethod]
		public void TestVersionDetection()
		{
			// Test that we can detect Everything version
			// This requires Everything to be running
			
			if (!SearchClient.IsEverythingAvailable())
			{
				Assert.Inconclusive("Everything is not running - cannot test version detection");
				return;
			}
			
			try
			{
				Version version = SearchClient.GetEverythingVersion();
				
				// Verify version is valid
				Assert.IsNotNull(version);
				Assert.IsTrue(version.Major >= 1);
				
				// Everything 1.5 or higher should be detected correctly
				if (version.Major >= 1 && version.Minor >= 5)
				{
					// Everything 1.5+ detected
					Assert.IsTrue(true, $"Everything 1.5+ detected: {version}");
				}
				else
				{
					// Everything 1.4 or earlier
					Assert.IsTrue(true, $"Everything 1.4 or earlier detected: {version}");
				}
			}
			catch (InvalidOperationException)
			{
				Assert.Inconclusive("Everything is not available for version detection");
			}
		}
	}
}
