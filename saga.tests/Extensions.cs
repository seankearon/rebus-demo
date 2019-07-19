
using System;
using System.Linq;
using NUnit.Framework;
using Rebus.Sagas;
using Rebus.TestHelpers;
using Rebus.TestHelpers.Events;

namespace saga.tests
{
    public static class SagaSpecificExtensions
    {
        public static OnboardCustomerSagaData SagaDataShould(this SagaFixture<OnboardCustomerSaga> fixture, Predicate<OnboardCustomerSagaData> have, string because = null)
        {
            return fixture.SagaData<OnboardCustomerSaga, OnboardCustomerSagaData>().Should(have, because);
        }

        public static OnboardCustomerSagaData SagaDataIsNotComplete(this OnboardCustomerSagaData data, string because = "the saga data should not be marked as complete")
        {
            Assert.IsFalse(data.Completed, because);
            return data;
        }
    }

    public static class Extensions
    {
        public static FakeBus HasPublished<TMessage>(this FakeBus bus, int count = 1, string because = null, Predicate<TMessage> with = null)
        {
            var events = bus.Events.OfType<MessagePublished<TMessage>>().ToArray();
            Assert.That(events.Count, Is.EqualTo(count), because);
            if (with != null) events.Take(count).ToList().ForEach(x => Assert.IsTrue(with(x.EventMessage), because));
            return bus;
        }

        public static FakeBus HasNotPublished<TMessage>(this FakeBus bus, string because = null)
        {
            var events = bus.Events.OfType<MessagePublished<TMessage>>().ToArray();
            Assert.That(events.Count, Is.EqualTo(0), because);
            return bus;
        }

        public static FakeBus HasNotSent<TMessage>(this FakeBus bus, string because = null)
        {
            var events = bus.Events.OfType<MessageSent<TMessage>>().ToArray();
            Assert.That(events.Count, Is.EqualTo(0), because);
            return bus;
        }

        public static FakeBus HasSent<TMessage>(this FakeBus bus, int count = 1, string because = null, Predicate<TMessage> with = null)
        {
            var events = bus.Events.OfType<MessageSent<TMessage>>().ToArray();
            Assert.That(events.Count, Is.EqualTo(count), because);
            if (with != null) events.Take(count).ToList().ForEach(x => Assert.IsTrue(with(x.CommandMessage), because));
            return bus;
        }

        public static FakeBus HasSentLocal<TMessage>(this FakeBus bus, int count = 1, string because = null, Predicate<TMessage> with = null)
        {
            var events = bus.Events.OfType<MessageSentToSelf<TMessage>>().ToArray();
            Assert.That(events.Count, Is.EqualTo(count), because);
            if (with != null) events.Take(count).ToList().ForEach(x => Assert.IsTrue(with(x.CommandMessage), because));
            return bus;
        }

        public static FakeBus HasNotSentLocal<TMessage>(this FakeBus bus, string because = null)
        {
            var events = bus.Events.OfType<MessageSentToSelf<TMessage>>().ToArray();
            Assert.That(events.Count, Is.EqualTo(0), because);
            return bus;
        }

        public static FakeBus HasDeferredLocal<TMessage>(this FakeBus bus, int count = 1, string because = null, Predicate<TMessage> with = null)
        {
            var events = bus.Events.OfType<MessageDeferredToSelf<TMessage>>();
            Assert.That(events.Count(), Is.EqualTo(count), because);
            if (with != null) events.Take(count).ToList().ForEach(x => Assert.IsTrue(with(x.CommandMessage), because));
            return bus;
        }

        public static TSagaData SagaData<TSaga, TSagaData>(this SagaFixture<TSaga> fixture) where TSagaData : ISagaData, new() where TSaga : Saga
        {
            return fixture.Data.OfType<TSagaData>().Single();
        }

        public static SagaFixture<TSaga> ShouldHaveCompleted<TSaga>(this SagaFixture<TSaga> fixture) where TSaga : Saga
        {
            Assert.IsTrue(fixture.Data == null || !fixture.Data.Any(), "the fixture should have disposed of its data when it is complete");
            return fixture;
        }

        public static SagaFixture<TSaga> ShouldNotHaveCompleted<TSaga>(this SagaFixture<TSaga> fixture) where TSaga : Saga
        {
            Assert.IsFalse(fixture.Data == null || !fixture.Data.Any(), "the fixture should have data when it is not complete");
            return fixture;
        }

        public static TSagaData Should<TSagaData>(this TSagaData data, Predicate<TSagaData> have, string because = null) where TSagaData : ISagaData, new()
        {
            Assert.IsTrue(have(data), because);
            return data;
        }

        public static TSagaData And<TSagaData>(this TSagaData data, Predicate<TSagaData> have, string because = null) where TSagaData : ISagaData, new()
        {
            Assert.IsTrue(have(data), because);
            return data;
        }
        public static TSagaData AndNot<TSagaData>(this TSagaData data, Predicate<TSagaData> have, string because = null) where TSagaData : ISagaData, new()
        {
            Assert.IsFalse(have(data), because);
            return data;
        }
    }
}