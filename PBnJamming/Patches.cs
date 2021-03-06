using System;
using System.Text;
using BepInEx.Configuration;
using BepInEx.Logging;
using FistVR;
using Random = UnityEngine.Random;

namespace PBnJamming
{
	public class Patches : IDisposable
	{
		private readonly ManualLogSource _logger;
		private readonly IFailure _tree;
		private readonly ConfigEntry<bool> _config;

		private bool _extractFlag;
		private bool _lockFlag;

		public Patches(ManualLogSource logger, IFailure tree, ConfigEntry<bool> config)
		{
			_logger = logger;
			_tree = tree;
			_config = config;

			Hook();
		}

		public void Dispose()
		{
			Unhook();
		}

		#region Hooking

		private void Hook()
		{
			On.FistVR.BreakActionWeapon.Awake += BreakActionWeapon_Awake;

			// Fire
			On.FistVR.RollingBlock.Fire += RollingBlock_Fire;
			On.FistVR.FVRFireArmChamber.Fire += FVRFireArmChamber_Fire;

			// Feed
			On.FistVR.ClosedBoltWeapon.BeginChamberingRound += ClosedBoltWeapon_BeginChamberingRound;
			On.FistVR.OpenBoltReceiver.BeginChamberingRound += OpenBoltReceiver_BeginChamberingRound;
			On.FistVR.FVRFireArmChamber.SetRound += FVRFireArmChamber_SetRound; // LeverActionFirearm
			On.FistVR.Handgun.ExtractRound += Handgun_ExtractRound;
			On.FistVR.TubeFedShotgun.ExtractRound += TubeFedShotgun_ExtractRound;

			// Extract
			On.FistVR.ClosedBolt.ImpartFiringImpulse += ClosedBolt_ImpartFiringImpulse;
			On.FistVR.OpenBoltReceiverBolt.ImpartFiringImpulse += OpenBoltReceiverBolt_ImpartFiringImpulse;
			On.FistVR.HandgunSlide.ImpartFiringImpulse += HandgunSlide_ImpartFiringImpulse;
			On.FistVR.TubeFedShotgun.EjectExtractedRound += TubeFedShotgun_EjectExtractedRound;
			On.FistVR.BreakActionWeapon.PopOutRound += BreakActionWeapon_PopOutRound;
			On.FistVR.BreakActionWeapon.PopOutEmpties += BreakActionWeapon_PopOutEmpties;

			// LockOpen
			On.FistVR.Handgun.EngageSlideRelease += Handgun_EngageSlideRelease;
			On.FistVR.ClosedBolt.LockBolt += ClosedBolt_LockBolt;

			// Discharge
			On.FistVR.HandgunSlide.SlideEvent_ArriveAtFore += HandgunSlide_SlideEvent_ArriveAtFore;
			On.FistVR.ClosedBolt.BoltEvent_ArriveAtFore += ClosedBolt_BoltEvent_ArriveAtFore;
			On.FistVR.TubeFedShotgunBolt.BoltEvent_ArriveAtFore += TubeFedShotgunBolt_BoltEvent_ArriveAtFore;
			On.FistVR.OpenBoltReceiverBolt.BoltEvent_BoltCaught += OpenBoltReceiverBolt_BoltEvent_BoltCaught;
		}

		private void Unhook()
		{
			On.FistVR.BreakActionWeapon.Awake -= BreakActionWeapon_Awake;

			// Fire
			On.FistVR.RollingBlock.Fire -= RollingBlock_Fire;
			On.FistVR.FVRFireArmChamber.Fire -= FVRFireArmChamber_Fire;

			// Feed
			On.FistVR.ClosedBoltWeapon.BeginChamberingRound -= ClosedBoltWeapon_BeginChamberingRound;
			On.FistVR.OpenBoltReceiver.BeginChamberingRound -= OpenBoltReceiver_BeginChamberingRound;
			On.FistVR.FVRFireArmChamber.SetRound -= FVRFireArmChamber_SetRound; // LeverActionFirearm
			On.FistVR.Handgun.ExtractRound -= Handgun_ExtractRound;
			On.FistVR.TubeFedShotgun.ExtractRound -= TubeFedShotgun_ExtractRound;

			// Extract
			On.FistVR.ClosedBolt.ImpartFiringImpulse -= ClosedBolt_ImpartFiringImpulse;
			On.FistVR.OpenBoltReceiverBolt.ImpartFiringImpulse -= OpenBoltReceiverBolt_ImpartFiringImpulse;
			On.FistVR.HandgunSlide.ImpartFiringImpulse -= HandgunSlide_ImpartFiringImpulse;
			On.FistVR.TubeFedShotgun.EjectExtractedRound -= TubeFedShotgun_EjectExtractedRound;
			On.FistVR.BreakActionWeapon.PopOutRound -= BreakActionWeapon_PopOutRound;
			On.FistVR.BreakActionWeapon.PopOutEmpties -= BreakActionWeapon_PopOutEmpties;

			// LockOpen
			On.FistVR.Handgun.EngageSlideRelease -= Handgun_EngageSlideRelease;
			On.FistVR.ClosedBolt.LockBolt -= ClosedBolt_LockBolt;

			// Discharge
			On.FistVR.HandgunSlide.SlideEvent_ArriveAtFore -= HandgunSlide_SlideEvent_ArriveAtFore;
			On.FistVR.ClosedBolt.BoltEvent_ArriveAtFore -= ClosedBolt_BoltEvent_ArriveAtFore;
			On.FistVR.TubeFedShotgunBolt.BoltEvent_ArriveAtFore -= TubeFedShotgunBolt_BoltEvent_ArriveAtFore;
			On.FistVR.OpenBoltReceiverBolt.BoltEvent_BoltCaught -= OpenBoltReceiverBolt_BoltEvent_BoltCaught;
		}

		#endregion

		private bool Failed(FVRFireArmChamber chamber, FailureType failure)
		{
			var ran = Random.value;
			var mask = _tree[chamber].Unwrap();
			var chance = mask[failure];
			var failed = ran <= chance;

			if (_config.Value && !_lockFlag && !_extractFlag)
			{
				var gun = chamber.Firearm;
				var builder = new StringBuilder().AppendLine()
					.Append("┌─────Failure Roll Report─────").AppendLine()
					.Append("│ ItemID: ").Append(gun.ObjectWrapper == null ? "" : gun.ObjectWrapper.ItemID).AppendLine()
					.Append("│  Era: ").Append(gun.ObjectWrapper == null ? FVRObject.OTagEra.None : gun.ObjectWrapper.TagEra).AppendLine()
					.Append("│  Action: ").Append(gun.ObjectWrapper == null ? FVRObject.OTagFirearmAction.None : gun.ObjectWrapper.TagFirearmAction).AppendLine()
					.Append("│  Magazine: ").Append((gun.Magazine == null || gun.Magazine.ObjectWrapper == null) ? "" : (gun.Magazine.IsIntegrated ? gun.Magazine.FireArm.ObjectWrapper.ItemID : gun.Magazine.ObjectWrapper.ItemID)).AppendLine()
					.Append("│   Round Type: ").Append(gun.RoundType).AppendLine()
					.Append("|   Round Class: ").Append(chamber.m_round == null ? "" : chamber.m_round.RoundClass.ToString()).AppendLine()
					.Append("│ Failure Rolled: ").Append(failure).AppendLine()
					.Append("│  Random: ").Append(ran).AppendLine()
					.Append("│  Chance: ").Append(chance).AppendLine()
					.Append("│  Roll: ").Append(failed).AppendLine()
					.Append("└─────────────────────────────");

				_logger.LogDebug(builder);
			}

			// These base methods are run in Update() - use dumb lock to prevent console spam for now
			PreventConsoleSpam(chamber.Firearm, failure);			

			return failed;
		}

		#region Patches

		private void BreakActionWeapon_Awake(On.FistVR.BreakActionWeapon.orig_Awake orig, BreakActionWeapon self)
		{
			self.RotationInterpSpeed = 1;
			orig(self);
		}

		#region Fire

		private void RollingBlock_Fire(On.FistVR.RollingBlock.orig_Fire orig, RollingBlock self)
		{
			if (Failed(self.Chamber, FailureType.Fire))
			{
				return;
			}

			orig(self);
		}

		private bool FVRFireArmChamber_Fire(On.FistVR.FVRFireArmChamber.orig_Fire orig, FVRFireArmChamber self)
		{
			if (!(self.Firearm is RollingBlock))
			{
				if (Failed(self, FailureType.Fire))
				{
					return false;
				}
			}

			return orig(self);
		}

		#endregion

		#region Feed

		private void ClosedBoltWeapon_BeginChamberingRound(On.FistVR.ClosedBoltWeapon.orig_BeginChamberingRound orig, ClosedBoltWeapon self)
		{
			if (Failed(self.Chamber, FailureType.Feed))
			{
				return;
			}

			orig(self);
		}

		private void OpenBoltReceiver_BeginChamberingRound(On.FistVR.OpenBoltReceiver.orig_BeginChamberingRound orig, OpenBoltReceiver self)
		{
			if (Failed(self.Chamber, FailureType.Feed))
			{
				return;
			}

			orig(self);
		}

		private void Handgun_ExtractRound(On.FistVR.Handgun.orig_ExtractRound orig, Handgun self)
		{
			if (Failed(self.Chamber, FailureType.Feed))
			{
				return;
			}

			orig(self);
		}

		private void TubeFedShotgun_ExtractRound(On.FistVR.TubeFedShotgun.orig_ExtractRound orig, TubeFedShotgun self)
		{
			if (Failed(self.Chamber, FailureType.Feed))
			{
				return;
			}

			orig(self);
		}

		private void FVRFireArmChamber_SetRound(On.FistVR.FVRFireArmChamber.orig_SetRound orig, FVRFireArmChamber self, FVRFireArmRound round)
		{
			if ((self.Firearm is LeverActionFirearm ) && round != null)
			{
				if (Failed(self, FailureType.Feed))
				{
					// Add round that will be removed in UpdateLever
					self.Firearm.Magazine.AddRound(round, false, true);
					return;
				}
			}

			orig(self, round);
		}

		#endregion

		#region Extract

		private void ClosedBolt_ImpartFiringImpulse(On.FistVR.ClosedBolt.orig_ImpartFiringImpulse orig, ClosedBolt self)
		{
			if (Failed(self.Weapon.Chamber, FailureType.Extract))
			{
				self.RotationInterpSpeed = 2;
				return;
			}

			orig(self);
		}

		private void OpenBoltReceiverBolt_ImpartFiringImpulse(On.FistVR.OpenBoltReceiverBolt.orig_ImpartFiringImpulse orig, OpenBoltReceiverBolt self)
		{
			if (Failed(self.Receiver.Chamber, FailureType.Extract))
			{
				self.RotationInterpSpeed = 2;
				return;
			}

			orig(self);
		}

		private void HandgunSlide_ImpartFiringImpulse(On.FistVR.HandgunSlide.orig_ImpartFiringImpulse orig, HandgunSlide self)
		{
			if (Failed(self.Handgun.Chamber, FailureType.Extract))
			{
				self.RotationInterpSpeed = 2;
				return;
			}

			orig(self);
		}

		private void TubeFedShotgun_EjectExtractedRound(On.FistVR.TubeFedShotgun.orig_EjectExtractedRound orig, TubeFedShotgun self)
		{
			if (Failed(self.Chamber, FailureType.Extract))
			{
				self.RotationInterpSpeed = 2;
				return;
			}

			orig(self);
		}

		private void BreakActionWeapon_PopOutRound(On.FistVR.BreakActionWeapon.orig_PopOutRound orig, BreakActionWeapon self, FVRFireArmChamber chamber)
		{
			if (Failed(chamber, FailureType.Extract))
			{
				return;
			}

			orig(self, chamber);
		}

		private void BreakActionWeapon_PopOutEmpties(On.FistVR.BreakActionWeapon.orig_PopOutEmpties orig, BreakActionWeapon self)
		{
			if (Failed(self.Barrels[self.m_curBarrel].Chamber, FailureType.Extract))
			{
				return;
			}

			orig(self);
		}

		#endregion

		#region LockOpen

		private void Handgun_EngageSlideRelease(On.FistVR.Handgun.orig_EngageSlideRelease orig, Handgun self)
		{
			if (Failed(self.Chamber, FailureType.LockOpen))
			{
				return;
			}

			orig(self);
		}

		private void ClosedBolt_LockBolt(On.FistVR.ClosedBolt.orig_LockBolt orig, ClosedBolt self)
		{
			if (Failed(self.Weapon.Chamber, FailureType.LockOpen))
			{
				return;
			}

			orig(self);
		}

		#endregion

		#region Discharge

		private void HandgunSlide_SlideEvent_ArriveAtFore(On.FistVR.HandgunSlide.orig_SlideEvent_ArriveAtFore orig, HandgunSlide self)
		{
			if (Failed(self.Handgun.Chamber, FailureType.Discharge))
			{
				self.Handgun.ChamberRound();
				self.Handgun.DropHammer(false);
			}

			orig(self);
		}

		private void ClosedBolt_BoltEvent_ArriveAtFore(On.FistVR.ClosedBolt.orig_BoltEvent_ArriveAtFore orig, ClosedBolt self)
		{
			if (Failed(self.Weapon.Chamber, FailureType.Discharge))
			{
				self.Weapon.ChamberRound();
				self.Weapon.DropHammer();
			}

			orig(self);
		}

		private void TubeFedShotgunBolt_BoltEvent_ArriveAtFore(On.FistVR.TubeFedShotgunBolt.orig_BoltEvent_ArriveAtFore orig, TubeFedShotgunBolt self)
		{
			if (Failed(self.Shotgun.Chamber, FailureType.Discharge))
			{
				self.Shotgun.ChamberRound();
				self.Shotgun.ReleaseHammer();
			}

			orig(self);
		}

		private void OpenBoltReceiverBolt_BoltEvent_BoltCaught(On.FistVR.OpenBoltReceiverBolt.orig_BoltEvent_BoltCaught orig, OpenBoltReceiverBolt self)
		{
			if (Failed(self.Receiver.Chamber, FailureType.Discharge))
			{
				self.Receiver.ReleaseSeer();
			}
			orig(self);
		}

		#endregion

		#endregion

		private void PreventConsoleSpam(FVRFireArm firearm, FailureType failure)
		{
			_extractFlag = _lockFlag = false;

			switch (failure)
			{
				case FailureType.Extract:
					if (firearm is BreakActionWeapon)
					{
						_extractFlag = true;
					}
					break;
				case FailureType.LockOpen:
					_lockFlag = true;
					break;
			}
		}
	}
}
