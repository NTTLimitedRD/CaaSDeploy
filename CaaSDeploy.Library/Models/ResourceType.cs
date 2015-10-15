namespace CaasDeploy.Library.Models
{
    public enum ResourceType
    {
        Unknown,
        NetworkDomain,
        Vlan,
        Server,
        FirewallRule,
        PublicIpBlock,
        NatRule,
        Node,
        Pool,
        PoolMember,
        VirtualListener
    }
}