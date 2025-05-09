namespace FAManagementStudio.Common;

public class MessageBase(object sender, object target)
{
    public object Sender { get; private set; } = sender;
    public object Target { get; private set; } = target;
}
