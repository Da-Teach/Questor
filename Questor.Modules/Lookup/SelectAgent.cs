namespace Questor.Modules.Lookup
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
            DeclineTimer = DateTime.Now;
        }

        public string Name { get; private set; }

        public int Priorit { get; private set; }

        public DateTime DeclineTimer { get; set; }
    }
}