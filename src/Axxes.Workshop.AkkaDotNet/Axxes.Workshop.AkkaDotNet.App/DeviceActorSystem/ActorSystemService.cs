using Akka.Actor;
using Akka.Configuration;
using Axxes.Workshop.AkkaDotNet.App.Actors;
using Axxes.Workshop.AkkaDotNet.App.Messages;

namespace Axxes.Workshop.AkkaDotNet.App.DeviceActorSystem;

class ActorSystemService : IActorSystemService
{
    private const string ActorSystemName = "DeviceActorSystem";
    private readonly ActorSystem _actorSystem;
    private readonly Dictionary<Guid, IActorRef> _deviceActors = new();

    public ActorSystemService()
    {
        // Read akka.hocon
        var hoconConfig = ConfigurationFactory.ParseString(File.ReadAllText("akka.hocon"));

        // Get the ActorSystem Name
        _actorSystem = ActorSystem.Create(ActorSystemName, hoconConfig);
    }

    public void SendMeasurement(Guid deviceId, MeterReadingReceived message)
    {
        if (!_deviceActors.ContainsKey(deviceId))
            CreateDeviceActor(deviceId);

        _deviceActors[deviceId].Tell(message);
    }

    private void CreateDeviceActor(Guid deviceId)
    {
        var props = DeviceActor.CreateProps(deviceId);
        _deviceActors[deviceId] = _actorSystem.ActorOf(props, $"device-{deviceId}");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await CoordinatedShutdown.Get(_actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
    }
}