using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DHT;
using NSubstitute;
using NUnit.Framework;

namespace DHTTests
{
    public class DhtRelayServiceAdapterTests
    {
        private IDhtRelayServiceAdapter _sut;
        private IRelay _mockedRelay;

        [SetUp]
        public void Setup()
        {
        }
        
        
        [Test]
        public void NotifyThrowsExceptionWhenItIsUnableToConnect()
        {
            var successorNode = new NodeDto {Id = 10, IpAddress = "127.0.0.1", Port = 9001};
            var predecessorOfSuccessor = new NodeDto {Id = 0, IpAddress = "127.0.0.1", Port = 9001};
            _mockedRelay = Substitute.For<IRelay>();
            var successorBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new DhtProtocolCommandDto
            {
                Command = DhtCommand.NOTIFY,
                NodeDto = predecessorOfSuccessor
            }));
            _mockedRelay.receive().Returns(successorBytes);
            _mockedRelay.connected.Returns(false);
            _sut = new DhtRelayServiceAdapter(_mockedRelay);

            // Act
            var error = Assert.ThrowsAsync<Exception>(async () => await _sut.Notify(successorNode));
            // Assert
            Assert.True(error.Message == "Cannot notify, no connection");
        } 
        [Test]
        public async Task NotifyReturnsPredecessorOfNode()
        {
            var successorNode = new NodeDto {Id = 10, IpAddress = "127.0.0.1", Port = 9001};
            var expected = new NodeDto {Id = 0, IpAddress = "127.0.0.1", Port = 9001};
            _mockedRelay = Substitute.For<IRelay>();
            var successorBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new DhtProtocolCommandDto
            {
                Command = DhtCommand.NOTIFY,
                NodeDto = expected
            }));
            _mockedRelay.receive().Returns(successorBytes);
            _mockedRelay.connected.Returns(true);
            _sut = new DhtRelayServiceAdapter(_mockedRelay);

            // Act
            var actual = await _sut.Notify(successorNode);
            // Assert
            Assert.AreEqual(expected.Id,actual.Id);
        }

        [Test]
        public void GetSuccessorThrowsErrorWhenIsUnableToConnect()
        {
            var successorNode = new NodeDto {Id = 10, IpAddress = "127.0.0.1", Port = 9001};
            _mockedRelay = Substitute.For<IRelay>();
            var successorBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new DhtProtocolCommandDto
            {
                Command = DhtCommand.FIND_SUCCESSOR,
                NodeDto = new NodeDto {Id = 0, IpAddress = "127.0.0.1", Port = 9001}
            }));
            _mockedRelay.receive().Returns(successorBytes);
            _mockedRelay.connected.Returns(false);
            _sut = new DhtRelayServiceAdapter(_mockedRelay);

            // Act
            var error = Assert.ThrowsAsync<Exception>(async () => await _sut.GetSuccessor(successorNode));
            // Assert
            Assert.True(error.Message == "Cannot connect with successor");
        }

        [Test]
        public async Task GetSuccessorReturnsSuccessor()
        {
            var autoResetEvent = new AutoResetEvent(false);
            var predecessorNode = new NodeDto {Id = 10, IpAddress = "127.0.0.1", Port = 9001};
            _mockedRelay = Substitute.For<IRelay>();
            var successorBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new DhtProtocolCommandDto
            {
                Command = DhtCommand.FIND_SUCCESSOR,
                NodeDto = new NodeDto {Id = 0, IpAddress = "127.0.0.1", Port = 9001}
            }));
            _mockedRelay.receive().Returns(successorBytes);
            _mockedRelay.connected.Returns(true);
            _sut = new DhtRelayServiceAdapter(_mockedRelay);
            _sut.NotifyHandler += (sender, args) =>
            {
                autoResetEvent.Set();
                NotifyEventArgs eventArgs = (NotifyEventArgs) args;
                var node = eventArgs.NodeDto;
            };

            // Assert
            var wasSignaled = autoResetEvent.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.True(wasSignaled);

            var successor = await _sut.GetSuccessor(predecessorNode);
            Assert.True(successor.Id == 0);
        }

        [Test]
        public async Task GetSuccessorCallsConnectingWithRightParameters()
        {
            var successorNode = new NodeDto {Id = 10, IpAddress = "127.0.0.1", Port = 9001};
            _mockedRelay = Substitute.For<IRelay>();
            var successorBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new DhtProtocolCommandDto
            {
                Command = DhtCommand.FIND_SUCCESSOR,
                NodeDto = new NodeDto {Id = 0, IpAddress = "127.0.0.1", Port = 9001}
            }));
            _mockedRelay.receive().Returns(successorBytes);
            _mockedRelay.connected.Returns(true);
            _sut = new DhtRelayServiceAdapter(_mockedRelay);

            // Act
            await _sut.GetSuccessor(successorNode);

            // Assert
            _mockedRelay.Received().connect(successorNode.IpAddress, successorNode.Port);
        }

        [Test]
        public void VerifyNotificationNotifiesHandler()
        {
            var autoResetEvent = new AutoResetEvent(false);

            _mockedRelay = Substitute.For<IRelay>();
            var notify =
                Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new DhtProtocolCommandDto {Command = DhtCommand.NOTIFY}));
            _mockedRelay.receive().Returns(notify);
            _mockedRelay.connected.Returns(true);
            _sut = new DhtRelayServiceAdapter(_mockedRelay);
            _sut.NotifyHandler += (sender, args) =>
            {
                autoResetEvent.Set();
                NotifyEventArgs eventArgs = (NotifyEventArgs) args;
                var node = eventArgs.NodeDto;
            };

            // Assert
            var wasSignaled = autoResetEvent.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.True(wasSignaled);
        }

        [Test]
        public void VerifyNotificationReturnsEventArgsWithNode()
        {
            var autoResetEvent = new AutoResetEvent(false);
            NodeDto node = null;
            _mockedRelay = Substitute.For<IRelay>();
            var notify = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new DhtProtocolCommandDto
                {Command = DhtCommand.NOTIFY, NodeDto = new NodeDto {Id = 0, IpAddress = "127.0.0.1", Port = 9001}}));
            _mockedRelay.receive().Returns(notify);
            _mockedRelay.connected.Returns(true);
            _sut = new DhtRelayServiceAdapter(_mockedRelay);

            _sut.NotifyHandler += (sender, args) =>
            {
                autoResetEvent.Set();
                NotifyEventArgs eventArgs = (NotifyEventArgs) args;
                node = eventArgs.NodeDto;
            };
            var wasSignaled = autoResetEvent.WaitOne(timeout: TimeSpan.FromSeconds(1));

            // Assert
            Assert.True(wasSignaled && node.Id == 0 && node.IpAddress.Equals("127.0.0.1") && node.Port == 9001);
        }
    }
}