/* Written by Noob536 */
using System;

namespace GoToBM
{
    using DirectEve;
    using global::Questor.Modules;

    static class Program
    {
        private static DirectEve _directEve;
        private static Traveler _traveler;
        private static DirectBookmark _bookmark;
        private static DateTime _lastPulse;
        private static bool _done = false;
        private static string _BM;
        private static bool _started = false;

        [STAThread]
        static void Main(string[] args)
        {
            Logging.Log("GoToBM: Started");
            if (args.Length == 0 || args[0].Length < 1)
            {
                Logging.Log("GoToBM: You need to supply a bookmark name");
                Logging.Log("GoToBM: Ended");
                return;
            }
            _BM = args[0];
            _BM = _BM.ToLower();

            _directEve = new DirectEve();
            Cache.Instance.DirectEve = _directEve;
            _directEve.OnFrame += OnFrame;
            _traveler = new Traveler();

            while (!_done)
            {
                System.Threading.Thread.Sleep(50);
            }
            Logging.Log("GoToBM: Exiting");
            return;
        }

        static void OnFrame(object sender, EventArgs e)
        {
            if (DateTime.Now.Subtract(_lastPulse).TotalMilliseconds < 1500)
                return;
            _lastPulse = DateTime.Now;

            // New frame, invalidate old cache
            Cache.Instance.InvalidateCache();

            if (Cache.Instance.InWarp)
                return;

            if (!_started)
            {
                _started = true;
                if (!Cache.Instance.DirectEve.Session.IsReady)
                {

                    Logging.Log("GoToBM: Not in game, exiting");
                    return;
                }
                Logging.Log("GoToBM: Attempting to find bookmark [" + _BM + "]");
                foreach (var bookmark in Cache.Instance.DirectEve.Bookmarks)
                {
                    if (bookmark.Title.ToLower().Equals(_BM))
                    {
                        _bookmark = bookmark;
                        break;
                    }
                    if (_bookmark == null && bookmark.Title.ToLower().Contains(_BM))
                    {
                        _bookmark = bookmark;
                    }
                }
                if (_bookmark == null)
                {
                    Logging.Log("GoToBM: Bookmark not found");
                    _done = true;
                    return;
                }
                _traveler.Destination = new BookmarkDestination(_bookmark);
            }
            _traveler.ProcessState();
            if (_traveler.State == TravelerState.AtDestination)
            {
                _done = true;
                Logging.Log("GoToBM: At destination");
            }
            else if (_traveler.State == TravelerState.Error)
            {
                Logging.Log("GoToBM: Traveler error");
                _done = true;
            }
        }
    }
}