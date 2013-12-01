using EventCore.Common.System;
using EventCore.Networking.Event.Modules;

namespace EventCore.Networking.Event
{
    public class Router
    {
        public static IEvent GetEvent()
        {
            if (OperatingSystemDetection.RunningPlatform() == OperatingSystemDetection.Platform.Windows)
            {
                return new SelectEventModule();
            }
            else
            {
                return new EPollEventModule();
            }
        }
    }
}