namespace DD.CBU.CaasDeploy.Library.Models
{
    /// <summary>
    /// The type of a deployment resource.
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// The unknown resource type
        /// </summary>
        Unknown,

        /// <summary>
        /// The network domain resource type
        /// </summary>
        NetworkDomain,

        /// <summary>
        /// The VLAN resource type
        /// </summary>
        Vlan,

        /// <summary>
        /// The server resource type
        /// </summary>
        Server,

        /// <summary>
        /// The firewall rule resource type
        /// </summary>
        FirewallRule,

        /// <summary>
        /// The public IP block resource type
        /// </summary>
        PublicIpBlock,

        /// <summary>
        /// The NAT rule resource type
        /// </summary>
        NatRule,

        /// <summary>
        /// The node resource type
        /// </summary>
        Node,

        /// <summary>
        /// The pool resource type
        /// </summary>
        Pool,

        /// <summary>
        /// The pool member resource type
        /// </summary>
        PoolMember,

        /// <summary>
        /// The virtual listener resource type
        /// </summary>
        VirtualListener
    }
}