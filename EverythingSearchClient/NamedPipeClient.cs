using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace EverythingSearchClient
{
	/// <summary>
	/// Handles named pipe communication with Everything 1.5+
	/// </summary>
	[SupportedOSPlatform("windows")]
	internal class NamedPipeClient : IDisposable
	{
		private NamedPipeClientStream? pipeClient;
		private readonly string instanceName;
		
		public NamedPipeClient(string? instanceName = null)
		{
			this.instanceName = instanceName ?? string.Empty;
		}

		public bool Connect(int timeoutMs = 1000)
		{
			try
			{
				string pipeName = string.IsNullOrEmpty(instanceName) 
					? "Everything IPC" 
					: $"Everything IPC ({instanceName})";

				pipeClient = new NamedPipeClientStream(
					".", 
					pipeName, 
					PipeDirection.InOut,
					PipeOptions.None);

				pipeClient.Connect(timeoutMs);
				return pipeClient.IsConnected;
			}
			catch
			{
				pipeClient?.Dispose();
				pipeClient = null;
				return false;
			}
		}

		public bool IsConnected => pipeClient?.IsConnected ?? false;

		public Version? GetVersion()
		{
			if (!IsConnected || pipeClient == null)
				return null;

			try
			{
				// Request version info
				// Based on Everything3 SDK, version info is retrieved via specific commands
				byte[] request = new byte[8];
				BitConverter.GetBytes((uint)0x00).CopyTo(request, 0); // Command: Get version
				BitConverter.GetBytes((uint)0x00).CopyTo(request, 4); // Reserved

				pipeClient.Write(request, 0, request.Length);
				pipeClient.Flush();

				byte[] response = new byte[16];
				int bytesRead = pipeClient.Read(response, 0, response.Length);
				
				if (bytesRead >= 16)
				{
					int major = BitConverter.ToInt32(response, 0);
					int minor = BitConverter.ToInt32(response, 4);
					int revision = BitConverter.ToInt32(response, 8);
					int build = BitConverter.ToInt32(response, 12);
					return new Version(major, minor, revision, build);
				}
			}
			catch
			{
				// If version retrieval fails, disconnect
				Disconnect();
			}

			return null;
		}

		public bool SendQuery(byte[] queryData, out byte[]? responseData)
		{
			responseData = null;
			
			if (!IsConnected || pipeClient == null)
				return false;

			try
			{
				// Write query data
				pipeClient.Write(queryData, 0, queryData.Length);
				pipeClient.Flush();

				// Read response size (first 4 bytes)
				byte[] sizeBuffer = new byte[4];
				int bytesRead = pipeClient.Read(sizeBuffer, 0, 4);
				if (bytesRead != 4)
					return false;

				int responseSize = BitConverter.ToInt32(sizeBuffer, 0);
				if (responseSize <= 0 || responseSize > 100 * 1024 * 1024) // 100MB limit
					return false;

				// Read response data
				responseData = new byte[responseSize];
				int totalRead = 0;
				while (totalRead < responseSize)
				{
					bytesRead = pipeClient.Read(responseData, totalRead, responseSize - totalRead);
					if (bytesRead <= 0)
						return false;
					totalRead += bytesRead;
				}

				return true;
			}
			catch
			{
				Disconnect();
				return false;
			}
		}

		public void Disconnect()
		{
			pipeClient?.Dispose();
			pipeClient = null;
		}

		public void Dispose()
		{
			Disconnect();
		}
	}
}
