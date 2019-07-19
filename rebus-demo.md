# Rebus Demo

A free and open source .Net "servivce bus".

Service bus can be a bit of an overloaded term.  This is a library that allows you to easily create distributed systems using a message-based architecture.

These types of applications are extremely robust, easy to understand and very business focused.

True "microservices" without all the pain of service discovery.

Abstracts the transport and storage layers - your code is the same if you're running against Azure Service Bus, Rabbit or the file system (configuration is different though).

Runs anywhere - desktop, console, Windows service, web apps, Azure Apps, etc.  

Supports full fat .Net and .Net Core.

Can pay for support, training, consultancy and their Fleet Manager app.

From their wiki:

* common message exchange patterns like point to point, request/reply, publish/subscribe made easy
* advanced coordination of messages, i.e. process managers (or "sagas" in NServiceBus terminology)
* timeout manager that allows message delivery to be deferred to the future
* messages can be transferred using MSMQ, RabbitMQ, Azure Service Bus, SQL Server, and even by using a shared directory in the file system
* subscriptions, sagas and timeouts can be stored in either Microsoft SQL Server, MongoDB, RavenDB, or PostgreSQL
* polymorphic message dispatch that allows messages to compose by implementing multiple interfaces and enables handler reuse
* handler pipeline re-ordering
* MSMQ queue inspector
* and more


# Events and Commands

Talk about the difference between the two.

# Pub/Sub

Sender console publishes an event and the two subscribers respond.

Sender console issues a command the it is received.

Web service can publish and send commands too.  The bus is everywhere!

Dive into the code.

# Transports and Handlers

Running on Azure Service Bus using native subscription handling (topics).

Other transports include queues like Rabbit and MSMQ, DBs like MSSQL, Postgres.

Also in memory and file system.


# Events and Commands

Sender console can send a command.

Show defered commands.

# Process Manager (Sagas)

We're going to model a business process that is under an OLA and which has compensating actions that must be taken upon the OLA becoming breached.

Imaging the onboarding of a new customer where the business want the following to happen during the process:

* An account is to be created for the customer.
* A welcome pack is to be sent to the customer after their account has been created.
* The sales team will set up a call with the customer after their account has been created.
* Other systems are notified after a successful onboarding.
* If OLA is breached then the service desk takes over the process.
* If OLA is breached then other systems will not have been notified about the onboarding.
* If OLA is breached then any placed sales call is cancelled.


The architecture here is one service is responsible for all of this process, but it will use another existing service to send the welcome pack.


# Saga Testing

Very good support for