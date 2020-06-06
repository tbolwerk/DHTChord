using System;
using System.Timers;
using DHT;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;

namespace DHTTests
{
    public class NodeTests
    {
        private Node _sut;
        private INetworkAdapter _networkAdapter;
        private IFingerTable _mockedFingerTable;
        private IOptions<DhtSettings> _options;
        private IDhtActions _actions;
        private NodeDto _bootstrapNode;
        private ITimeOutScheduler _timeOutScheduler;
        private ISchedule _scheduler;
        private IStabilize _stabilize;
        private ICheckPredecessor _checkPredecessor;

        [SetUp]
        public void Setup()
        {
            _networkAdapter = Substitute.For<INetworkAdapter>();
            _actions = Substitute.For<IDhtActions>();
            _mockedFingerTable = Substitute.For<IFingerTable>();
            _options = Substitute.For<IOptions<DhtSettings>>();
            _timeOutScheduler = Substitute.For<ITimeOutScheduler>();
            _scheduler = Substitute.For<ISchedule>();
            _stabilize = Substitute.For<IStabilize>();
            _checkPredecessor = Substitute.For<ICheckPredecessor>();

            _bootstrapNode = new NodeDto {Id = 0};

            _actions.Notify(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());

            _mockedFingerTable.FingerTableEntries.Returns(new[] {new FingerTableEntry(),});
            var options = new DhtSettings();
            options.StabilizeCallInSeconds = 10;
            options.TimeToLiveInSeconds = 10;
            options.IntervalBetweenPeriodicCallsInSeconds = 10;
            options.MaxRetryAttempts = 10;
            options.FixFingersCallInSeconds = 10;
            options.CheckPredecessorCallInSeconds = 10;
            _options.Value.Returns(options);
        }

        [Test]
        public void FindSuccessorIdIsBetweenSelfAndPredecessor()
        {
            var me = new NodeDto {Id = 5};
            var predecessor = new NodeDto {Id = 0};
            var successor = new NodeDto {Id = 10};

            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor)
            {
                Successor = successor, Predecessor = predecessor, Id = me.Id, BootStrapNode = predecessor
            };

            _sut.FindSuccessor(3, successor, me);
            _actions.Received(Quantity.Exactly(1))
                .FoundSuccessor(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());
        }

        [Test]
        public void FindSuccessorIdIsBetweenSelfAndSuccessor()
        {
            var me = new NodeDto {Id = 5};
            var predecessor = new NodeDto {Id = 0};
            var successor = new NodeDto {Id = 10};

            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor)
            {
                Successor = successor, Predecessor = predecessor, Id = me.Id, BootStrapNode = _sut
            };

            _sut.FindSuccessor(7, successor, me);
            _actions.Received(Quantity.Exactly(1))
                .FoundSuccessor(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());
        }

        [Test]
        public void FindSuccessorIdIsNotBetweenSelfAndSuccessor()
        {
            var me = new NodeDto {Id = 5};
            var predecessor = new NodeDto {Id = 0};
            var successor = new NodeDto {Id = 10};

            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor)
            {
                Successor = successor, Predecessor = predecessor, Id = me.Id, BootStrapNode = predecessor
            };

            _sut.FindSuccessor(12, successor, me);
            _actions.Received(Quantity.Exactly(1))
                .FindSuccessor(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());
        }

        [Test]
        public void FindSuccessorIdIsNotBetweenSelfAndSuccessorButIsBootstrapNode()
        {
            var me = new NodeDto {Id = 0};
            var predecessor = new NodeDto {Id = 10};
            var successor = new NodeDto {Id = 5};

            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor)
            {
                Successor = successor, Predecessor = predecessor, Id = me.Id, BootStrapNode = me
            };

            _sut.FindSuccessor(12, successor, me);
            _actions.Received(Quantity.Exactly(1))
                .FoundSuccessor(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());
        }

        [Test]
        public void FindSuccessorSelf()
        {
            var me = new NodeDto {Id = 0};
            var successor = new NodeDto {Id = 5};

            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor) {Successor = me, Predecessor = null, Id = me.Id, BootStrapNode = me};

            _sut.FindSuccessor(12, successor, me);
            _actions.Received(Quantity.Exactly(1))
                .FoundSuccessor(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());
        }

        [Test]
        public void StabilizeShouldReturnWhenSuccessorIsItSelf()
        {
            var me = new NodeDto {Id = 0};
            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor) {Successor = me, Predecessor = null, Id = me.Id, BootStrapNode = _sut};
            _stabilize.Stabilize();
            _actions.Received(Quantity.None()).FindSuccessor(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());
            _actions.Received(Quantity.None()).Stabilize(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());
        }
        [Test]
        public void StabilizeShouldFindSuccessorWhenSuccessorIsNull()
        {
            var me = new NodeDto {Id = 0};
            NodeLegacy _sut = new NodeLegacy(_networkAdapter, _mockedFingerTable, _options, _actions, _timeOutScheduler, _scheduler)
            {
                Successor = null, Predecessor = null, Id = me.Id, BootStrapNode = me
            };
            _sut.Stabilize();
            _actions.Received(Quantity.Exactly(1)).FindSuccessor(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());
            _actions.Received(Quantity.None()).Stabilize(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());
        }

        [Test]
        public void JoinBootstrapNode()
        {
            var me = new NodeDto {Id = 0};
            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor) {Successor = me, Predecessor = null, Id = me.Id, BootStrapNode = _sut};
            var joiningNode = new NodeDto {Id = 10};
            _networkAdapter.NotifyHandler += Raise.EventWith(new NotifyEventArgs {NodeDto = joiningNode});
            Assert.AreEqual(joiningNode.Id, _sut?.Successor?.Id);
            Assert.AreEqual(joiningNode, _sut?.Predecessor);
        }


        [Test]
        public void IsAlreadyConnected()
        {
            var successor = new NodeDto {Id = 0};
            var predecessor = new NodeDto {Id = 2};
            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor) {Successor = successor, Predecessor = predecessor, Id = 0};
            var exception = Assert.Throws<Exception>(() => _sut.Create());
            Assert.AreEqual("Already connected", exception.Message);
        }

        [Test]
        public void IsBootstrapNode()
        {
            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor) {Successor = null, Predecessor = null, Id = 0};
            _sut.Create();
            Assert.AreEqual(_sut, _sut.Successor);
            Assert.True(_sut.IsBootStrapNode);
        }

        [Test]
        public void SchedulerConfiguredCorrectly()
        {
            var bootstrap = new NodeDto {Id = 0};
            var predecessor = new NodeDto {Id = 2};
            NodeLegacy _sut =
                new NodeLegacy(_networkAdapter, _mockedFingerTable, _options, _actions, _timeOutScheduler, _scheduler)
                {
                    Successor = bootstrap, BootStrapNode = bootstrap, Predecessor = predecessor, Id = 15
                };
            _scheduler.Received(Quantity.Exactly(1)).Enqueue(Arg.Any<Timer>(), _sut.CheckPredecessor);
            _scheduler.Received(Quantity.Exactly(1)).Enqueue(Arg.Any<Timer>(), _sut.Stabilize);
            _scheduler.Received(Quantity.Exactly(1)).Enqueue(Arg.Any<Timer>(), _sut.FixFingers);
            _scheduler.Received(Quantity.Exactly(1)).Run();
        }

        [Test]
        public void TimeOutSchedulerConfiguredCorrectly()
        {
            var bootstrap = new NodeDto {Id = 0};
            var predecessor = new NodeDto {Id = 2};
            var options = new DhtSettings();
            options.StabilizeCallInSeconds = 10;
            options.TimeToLiveInSeconds = 10;
            options.IntervalBetweenPeriodicCallsInSeconds = 1;
            options.MaxRetryAttempts = 10;
            options.FixFingersCallInSeconds = 10;
            options.CheckPredecessorCallInSeconds = 1;
            _options.Value.Returns(options);
        
            _timeOutScheduler = Substitute.For<ITimeOutScheduler>();
        
            NodeLegacy _sut = new NodeLegacy(_networkAdapter, _mockedFingerTable, _options, _actions, _timeOutScheduler, _scheduler)
            {
                Successor = bootstrap, BootStrapNode = bootstrap, Predecessor = predecessor, Id = 15
            };
        
            _timeOutScheduler.Received(Quantity.Exactly(2)).AddTimeOutTimer(Arg.Any<object>(), Arg.Any<int>(),
                Arg.Any<double>(), Arg.Any<Action>(), Arg.Any<Action>());
        
            const string SUCCESSOR = "successor";
            const string PREDECESSOR = "successor";
        
            _timeOutScheduler.Received(0).StartTimer(Arg.Any<object>());
            _sut.Stabilize();
            _timeOutScheduler.Received(1).StartTimer(SUCCESSOR);
            _sut.CheckPredecessor();
            _timeOutScheduler.Received(1).StartTimer(PREDECESSOR);
        
            _timeOutScheduler.Received(1).StopTimer(SUCCESSOR);
            _timeOutScheduler.Received(1).StopTimer(PREDECESSOR);
        
            _sut.Successor = null;
            _timeOutScheduler.Received(2).StopTimer(SUCCESSOR);
            _sut.Predecessor = null;
            _timeOutScheduler.Received(2).StopTimer(PREDECESSOR);
            _sut.CheckPredecessor();
            _timeOutScheduler.Received(1).StartTimer(PREDECESSOR);
        }

        [Test]
        public void NotifyHandlerEvent()
        {
            var predecessorOfSuccessor = new NodeDto
            {
                Id = 20, IpAddress = "127.0.0.1", Port = 9001, Predecessor = new NodeDto {Id = 10}
            };
            var successor = new NodeDto
            {
                Id = 30,
                IpAddress = "127.0.0.1",
                Port = 9001,
                Predecessor = predecessorOfSuccessor,
                Successor = predecessorOfSuccessor
            };
            var predecessor = new NodeDto {Id = 5};
            var predecessorInBetweenPredecessorAndMe = new NodeDto {Id = 8};

            var me = new NodeDto {Id = 10, Successor = successor, Predecessor = predecessor};

            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor)
            {
                Id = me.Id, Successor = me.Successor, Predecessor = me.Predecessor, BootStrapNode = _bootstrapNode
            };

            _networkAdapter.NotifyHandler +=
                Raise.EventWith(new NotifyEventArgs() {NodeDto = predecessorInBetweenPredecessorAndMe});

            Assert.AreEqual(predecessorInBetweenPredecessorAndMe.Id, _sut.Predecessor.Id);
        }

        [Test]
        public void StabilizeResponseWithOtherPredecessorAsPredecessor()
        {
            var predecessorOfSuccessor = new NodeDto
            {
                Id = 20, IpAddress = "127.0.0.1", Port = 9001, Predecessor = new NodeDto {Id = 10}
            };
            var successor = new NodeDto
            {
                Id = 30,
                IpAddress = "127.0.0.1",
                Port = 9001,
                Predecessor = predecessorOfSuccessor,
                Successor = predecessorOfSuccessor
            };
            var predecessor = new NodeDto {Id = 5};
        
            var me = new NodeDto {Id = 10, Successor = successor, Predecessor = predecessor};
            NodeLegacy _sut = new NodeLegacy(_networkAdapter, _mockedFingerTable, _options, _actions, _timeOutScheduler, _scheduler)
            {
                Id = me.Id, Successor = me.Successor, Predecessor = me.Predecessor, BootStrapNode = _bootstrapNode
            };
        
            _networkAdapter.StabilizeResponseHandler +=
                Raise.EventWith(new StabilizeResponseEventArgs {PredecessorOfSuccessor = predecessorOfSuccessor});
        
            Assert.AreEqual(predecessorOfSuccessor.Id, _sut.Successor.Id);
        }

        [Test]
        public void StabilizeResponseWithMeAsPredecessor()
        {
            var predecessorOfSuccessor = new NodeDto
            {
                Id = 5, IpAddress = "127.0.0.1", Port = 9001, Predecessor = new NodeDto {Id = 10}
            };
            var successor = new NodeDto
            {
                Id = 30,
                IpAddress = "127.0.0.1",
                Port = 9001,
                Predecessor = predecessorOfSuccessor,
                Successor = predecessorOfSuccessor
            };
            var predecessor = new NodeDto {Id = 5};

            var me = new NodeDto {Id = 10, Successor = successor, Predecessor = predecessor};

            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor)
            {
                Id = me.Id, Successor = me.Successor, Predecessor = me.Predecessor, BootStrapNode = _bootstrapNode
            };

            _networkAdapter.StabilizeResponseHandler +=
                Raise.EventWith(new StabilizeResponseEventArgs {PredecessorOfSuccessor = me});

            Assert.AreEqual(successor.Id, _sut.Successor.Id);
        }

        [Test]
        public void JoinSetsSuccessorCorrectly()
        {
            var bootstrap = new NodeDto {Id = 20, IpAddress = "127.0.0.1", Port = 9001};
            _sut = new Node(_networkAdapter, _mockedFingerTable, _actions, _timeOutScheduler, _scheduler,
                _stabilize, _checkPredecessor) {Id = 15, Successor = null, Predecessor = null};
            _sut.Join(bootstrap);
            Assert.AreEqual(bootstrap, _sut.BootStrapNode);
            Assert.AreEqual(null, _sut.Predecessor);
            _actions.Received(Quantity.Exactly(1))
                .FindSuccessor(Arg.Any<NodeDto>(), Arg.Any<uint>(), Arg.Any<NodeDto>());
        }
    }
}