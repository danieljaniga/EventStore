﻿using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.Core.Tests.ClientAPI.Helpers;
using EventStore.Core.Tests.Helpers;
using EventStore.Core.Tests.Integration;
using NUnit.Framework;

namespace EventStore.Core.Tests.Replication.ReadOnlyReplica {
	[TestFixture]
	[Category("LongRunning")]
	public class connecting_to_read_only_replica : specification_with_cluster {
		protected override MiniClusterNode CreateNode(int index, Endpoints endpoints, EndPoint[] gossipSeeds,
			bool wait = true) {
			var isReadOnly = index == 2;
			var node = new MiniClusterNode(
				PathName, index, endpoints.InternalTcp, endpoints.InternalTcpSec,
				endpoints.ExternalTcp,
				endpoints.ExternalTcpSec, endpoints.HttpEndPoint, skipInitializeStandardUsersCheck: false,
				subsystems: new ISubsystem[] { }, gossipSeeds: gossipSeeds, inMemDb: false,
				readOnlyReplica: isReadOnly);
			if (wait && !isReadOnly)
				WaitIdle();
			return node;
		}

		protected override IEventStoreConnection CreateConnection() {
			var settings = ConnectionSettings.Create()
				.DisableTls()
				.PerformOnAnyNode();
			return EventStoreConnection.Create(settings, _nodes[2].ExternalTcpEndPoint);
		}

		[Test]
		public async Task append_to_stream_should_fail_with_not_supported_exception() {
			const string stream = "append_to_stream_should_fail_with_not_supported_exception";
			await AssertEx.ThrowsAsync<OperationNotSupportedException>(
				() => _conn.AppendToStreamAsync(stream, ExpectedVersion.Any, TestEvent.NewTestEvent()));
		}

		[Test]
		public async Task delete_stream_should_fail_with_not_supported_exception() {
			const string stream = "delete_stream_should_fail_with_not_supported_exception";
			await AssertEx.ThrowsAsync<OperationNotSupportedException>(() =>
				_conn.DeleteStreamAsync(stream, ExpectedVersion.Any));
		}

		[Test]
		public async Task start_transaction_should_fail_with_not_supported_exception() {
			const string stream = "start_transaction_should_fail_with_not_supported_exception";
			await AssertEx.ThrowsAsync<OperationNotSupportedException>(() =>
				_conn.StartTransactionAsync(stream, ExpectedVersion.Any));
		}
	}
}
