namespace IronMagnate.Core.Storage;

public sealed record DeliveryResult(double RequestedQuantity, double AcceptedQuantity)
{
    public double UnacceptedQuantity => RequestedQuantity - AcceptedQuantity;
}
