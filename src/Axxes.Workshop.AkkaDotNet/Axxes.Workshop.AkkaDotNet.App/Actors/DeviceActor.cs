using Akka.Actor;
using Akka.Event;
using Axxes.Workshop.AkkaDotNet.App.Messages;

namespace Axxes.Workshop.AkkaDotNet.App.Actors;

class DeviceActor : ReceiveActor
{
    private readonly Guid _deviceId;

    public DeviceActor(Guid deviceId)
    {
        _deviceId = deviceId;
        CreateChildActors();
        Receive<MeterReadingReceived>(HandleMeterReadingReceived);
        Receive<NormalizedMeterReading>(HandleNormalizedMeterReading);

        Context.GetLogger().Info($"DeviceActor created for ID {_deviceId}");
    }

    private void CreateChildActors()
    {
        var persistenceProps = ReadingPersistenceActor.CreateProps(_deviceId);
        var persistenceActor = Context.ActorOf(persistenceProps, "value-persistence");
        var normalizationProps = ValueNormalizationActor.CreateProps(persistenceActor);
        Context.ActorOf(normalizationProps, "value-normalization");
    }

    private void HandleMeterReadingReceived(MeterReadingReceived msg)
    {
        Context.Child("value-normalization").Forward(msg);
    }

    private void HandleNormalizedMeterReading(NormalizedMeterReading msg)
    {
        Context.Child("value-persistence").Tell(msg);
    }

    public static Props CreateProps(Guid deviceId)
    {
        return Props.Create<DeviceActor>(deviceId);
    }
}