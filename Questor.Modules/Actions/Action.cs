// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using global::Questor.Modules.States;

    public class Action
    {
        public Action()
        {
            Parameters = new Dictionary<string, List<string>>();
        }

        public ActionState State { get; set; }

        public Dictionary<string, List<string>> Parameters { get; private set; }

        public void AddParameter(string parameter, string value)
        {
            if (string.IsNullOrEmpty(parameter) || string.IsNullOrEmpty(value))
                return;

            List<string> values;
            if (!Parameters.TryGetValue(parameter.ToLower(), out values))
                values = new List<string>();

            values.Add(value);
            Parameters[parameter.ToLower()] = values;
        }

        public string GetParameterValue(string parameter)
        {
            List<string> values;
            if (!Parameters.TryGetValue(parameter.ToLower(), out values))
                return null;

            return values.FirstOrDefault();
        }

        public List<string> GetParameterValues(string parameter)
        {
            List<string> values;
            if (!Parameters.TryGetValue(parameter.ToLower(), out values))
                return new List<string>();

            return values;
        }

        public override string ToString()
        {
            var output = State.ToString();

            foreach (var key in Parameters.Keys)
                foreach (var value in Parameters[key])
                    output += string.Format(" [{0}: {1}]", key, value);

            return output;
        }
    }
}