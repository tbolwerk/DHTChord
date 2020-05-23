using System.Threading.Tasks;
using DHT;
using NSubstitute;
using NSubstitute.Core.Arguments;
using NSubstitute.Extensions;
using NUnit.Framework;

namespace DHTTests
{
    public class NodeTests
    {
        private Node _sut;
        private IDhtRelayServiceAdapter _mockedRelayServiceAdapter;
        private IFingerTable _mockedFingerTable;

        [SetUp]
        public void Setup()
        {
            _mockedRelayServiceAdapter = Substitute.For<IDhtRelayServiceAdapter>();
            _mockedFingerTable = Substitute.For<IFingerTable>();
        }


        [Test]
        public void RaiseNotifyEvent()
        {
            var predecessorOfSuccessor = new NodeDto
                {Id = 20, IpAddress = "127.0.0.1", Port = 9001, Predecessor = new NodeDto {Id = 10}};
            var successor = new NodeDto
            {
                Id = 30, IpAddress = "127.0.0.1", Port = 9001, Predecessor = predecessorOfSuccessor,
                Successor = predecessorOfSuccessor
            };
            var predecessor = new NodeDto {Id = 5};
            var predecessorInBetweenPredecessorAndMe = new NodeDto {Id = 8};

            var me = new NodeDto {Id = 10, Successor = successor, Predecessor = predecessor};
            
         
            
            _sut = new Node(_mockedRelayServiceAdapter, _mockedFingerTable)
                {Id = me.Id, Successor = me.Successor, Predecessor = me.Predecessor};

      
            _mockedRelayServiceAdapter.NotifyHandler +=
                Raise.EventWith(new object(), new NotifyEventArgs {NodeDto = predecessorInBetweenPredecessorAndMe});
            Assert.AreEqual(predecessorInBetweenPredecessorAndMe.Id,_sut.Predecessor.Id);
        }

        [Test]
        public async Task StabilizeSetsPredecessorCorrectly()
        {
            var predecessorOfSuccessor = new NodeDto
                {Id = 20, IpAddress = "127.0.0.1", Port = 9001, Predecessor = new NodeDto {Id = 10}};
            var successor = new NodeDto
            {
                Id = 30, IpAddress = "127.0.0.1", Port = 9001, Predecessor = predecessorOfSuccessor,
                Successor = predecessorOfSuccessor
            };
            var predecessor = new NodeDto {Id = 5};

            var me = new NodeDto {Id = 10, Successor = successor, Predecessor = predecessor};

            _mockedRelayServiceAdapter.GetSuccessor(predecessorOfSuccessor).Returns(predecessorOfSuccessor);
            _mockedRelayServiceAdapter.Notify(Arg.Any<NodeDto>()).Returns(me);
            _sut = new Node(_mockedRelayServiceAdapter, _mockedFingerTable)
                {Id = me.Id, Successor = me.Successor, Predecessor = me.Predecessor};
            await _sut.Stabilize();
            Assert.AreEqual(predecessorOfSuccessor.Id, _sut.Successor.Id);
        }

        [Test]
        public async Task StabilizeGetsFalseNotificationBack()
        {
            var predecessorOfSuccessor = new NodeDto
                {Id = 20, IpAddress = "127.0.0.1", Port = 9001, Predecessor = new NodeDto {Id = 10}};
            var successor = new NodeDto
            {
                Id = 30, IpAddress = "127.0.0.1", Port = 9001, Predecessor = predecessorOfSuccessor,
                Successor = predecessorOfSuccessor
            };
            var predecessor = new NodeDto {Id = 5};

            var me = new NodeDto {Id = 10, Successor = successor, Predecessor = predecessor};

            _mockedRelayServiceAdapter.GetSuccessor(predecessorOfSuccessor).Returns(predecessorOfSuccessor);
            _mockedRelayServiceAdapter.Notify(Arg.Any<NodeDto>()).Returns(predecessor);
            _sut = new Node(_mockedRelayServiceAdapter, _mockedFingerTable)
                {Id = me.Id, Successor = me.Successor, Predecessor = me.Predecessor};
            await _sut.Stabilize();
            Assert.AreEqual(predecessorOfSuccessor.Id, _sut.Successor.Id);
        }

        [Test]
        public async Task JoinSetsSuccessorCorrectly()
        {
            var bootstrap = new NodeDto {Id = 20, IpAddress = "127.0.0.1", Port = 9001};
            _mockedRelayServiceAdapter.GetSuccessor(bootstrap).Returns(bootstrap);
            _sut = new Node(_mockedRelayServiceAdapter, _mockedFingerTable) {Id = 10};
            await _sut.Join(bootstrap);
            Assert.AreEqual(bootstrap, _sut.Successor);
            Assert.AreEqual(null, _sut.Predecessor);
        }

        [Test]
        public async Task JoinCallsClosestPrecedingNode()
        {
            var bootstrap = new NodeDto {Id = 0, IpAddress = "127.0.0.1", Port = 9001};

            _mockedRelayServiceAdapter.GetSuccessor(bootstrap).Returns(bootstrap);
            _sut = new Node(_mockedRelayServiceAdapter, _mockedFingerTable) {Id = 10};
            await _sut.Join(bootstrap);
            _mockedFingerTable.Received(1).ClosestPrecedingNode(bootstrap.Id);
        }

        [Test]
        public async Task ClosestPrecedingNodeReturnsNull()
        {
            var bootstrap = new NodeDto {Id = 0, IpAddress = "127.0.0.1", Port = 9001};

            _mockedRelayServiceAdapter.GetSuccessor(bootstrap).Returns(bootstrap);

            _sut = new Node(_mockedRelayServiceAdapter, _mockedFingerTable) {Id = 10};
            await _sut.Join(bootstrap);
            Assert.True(_sut.Successor.Id == bootstrap.Id);
        }

        [Test]
        public async Task ClosestPrecedingNodeReturnsCorrectNode()
        {
            var bootstrap = new NodeDto {Id = 0, IpAddress = "127.0.0.1", Port = 9001};
            var bootstrapSuccessor = new NodeDto {Id = 30, IpAddress = "127.0.0.1", Port = 9001};

            _mockedRelayServiceAdapter.GetSuccessor(bootstrap).Returns(bootstrap);
            _mockedFingerTable.ClosestPrecedingNode(0).Returns(bootstrapSuccessor);

            _sut = new Node(_mockedRelayServiceAdapter, _mockedFingerTable) {Id = 10};
            await _sut.Join(bootstrap);
            Assert.True(_sut.Successor.Id == bootstrapSuccessor.Id);
        }
    }
}