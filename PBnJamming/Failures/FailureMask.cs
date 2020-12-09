using Valve.Newtonsoft.Json;

namespace PBnJamming
{
	public readonly struct FailureMask
	{
		public static FailureMask Never { get; } = new FailureMask(-1, -1, -1, -1, -1);
		public static FailureMask Always { get; } = new FailureMask(1, 1, 1, 1, 1);

		public readonly float Fire;
		public readonly float Feed;
		public readonly float Extract;
		public readonly float LockOpen;
		public readonly float AccDischarge;

		[JsonConstructor]
		public FailureMask(float fire, float feed, float extract, float lockOpen, float accDischarge)
		{
			Fire = fire;
			Feed = feed;
			Extract = extract;
			LockOpen = lockOpen;
			AccDischarge = accDischarge;
		}

		public static FailureMask operator +(FailureMask a, FailureMask b)
		{
			return new FailureMask(
				a.Fire + b.Fire,
				a.Feed + b.Feed,
				a.Extract + b.Extract,
				a.LockOpen + b.LockOpen,
				a.AccDischarge + b.AccDischarge
			);
		}

		public static FailureMask operator *(FailureMask a, FailureMask b)
		{
			return new FailureMask(
				a.Fire * b.Fire,
				a.Feed * b.Feed,
				a.Extract * b.Extract,
				a.LockOpen * b.LockOpen,
				a.AccDischarge * b.AccDischarge
			);
		}
	}
}
