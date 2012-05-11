
namespace Questor.Modules
{
    using System;
    using System.Xml.Linq;

    public class AgentsList
    {
        public AgentsList()
        {
        }

        public AgentsList(XElement agentList)
        {
            Name = (string)agentList.Attribute("name") ?? "";
            Priorit = (int)agentList.Attribute("priority");
            Decline_timer = DateTime.Now;
        }

        public string Name { get; private set; }
        public int Priorit { get; private set; }
        public DateTime Decline_timer { get; set; }
    }
}