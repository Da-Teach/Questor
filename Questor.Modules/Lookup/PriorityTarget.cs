// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Lookup
{
    using global::Questor.Modules.Caching;

    public class PriorityTarget
    {
        private EntityCache _entity;

        public long EntityID { get; set; }

        public Priority Priority { get; set; }

        public EntityCache Entity
        {
            get { return _entity ?? (_entity = Cache.Instance.EntityById(EntityID)); }
        }

        public void ClearCache()
        {
            _entity = null;
        }
    }
}