using Core.Event;

namespace Samples_.Event.Events
{
	/// <summary>
	/// This is a sample of game event. Which is mount on __Sample_DefaultEventBus.
	/// You can just make your own game event with the same pattern as this one.
	/// </summary>
    public struct __Sample_SampleEvent : IEvent<__Sample_DefaultEventBus>
    {
        public float EventInfo1;
		public int EventValue1;
    }
}