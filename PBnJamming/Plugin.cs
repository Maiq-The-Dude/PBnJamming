using ADepIn;
using Deli;
using DeliFramework = Deli.Deli;
using FistVR;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;

namespace PBnJamming
{
	internal class Plugin : DeliBehaviour
	{
		public IFailure Failure { get; }

		public RootConfigs Configs { get; }

		private bool _extractFlag;
		private bool _lockFlag;

		public enum FailureType
		{
			Fire,
			Feed,
			Extract,
			LockOpen,
			AccDischarge
		}

		public Plugin()
		{
			Configs = new RootConfigs(Config);

			Failure = AddFailure("pbnj.magazine", g => g.Magazine == null ? "" : (g.Magazine.IsIntegrated ? g.Magazine.name : g.Magazine.ObjectWrapper.ItemID))
				.AddFailure("pbnj.roundtype", g => g.RoundType)
				.AddFailure("pbnj.action", g => g.ObjectWrapper.TagFirearmAction)
				.AddFailure("pbnj.era", g => g.ObjectWrapper.TagEra)
				.AddFailure("pbnj.id", g => g.ObjectWrapper.ItemID);

			// Patches
			On.FistVR.BreakActionWeapon.Awake += BreakActionWeapon_Awake;

			// Fire
			On.FistVR.ClosedBoltWeapon.Fire += ClosedBoltWeapon_Fire;
			On.FistVR.OpenBoltReceiver.Fire += OpenBoltReceiver_Fire;
			On.FistVR.Handgun.Fire += Handgun_Fire;
			On.FistVR.TubeFedShotgun.Fire += TubeFedShotgun_Fire;
			On.FistVR.Revolver.Fire += Revolver_Fire;
			On.FistVR.RevolvingShotgun.Fire += RevolvingShotgun_Fire;
			On.FistVR.RollingBlock.Fire += RollingBlock_Fire;
			On.FistVR.BreakActionWeapon.Fire += BreakActionWeapon_Fire;
			On.FistVR.BoltActionRifle.Fire += BoltActionRifle_Fire;
			On.FistVR.LeverActionFirearm.Fire += LeverActionFirearm_Fire;

			// Feed
			On.FistVR.ClosedBoltWeapon.BeginChamberingRound += ClosedBoltWeapon_BeginChamberingRound;
			On.FistVR.OpenBoltReceiver.BeginChamberingRound += OpenBoltReceiver_BeginChamberingRound;
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

			// Slamfire
			On.FistVR.HandgunSlide.SlideEvent_ArriveAtFore += HandgunSlide_SlideEvent_ArriveAtFore;
			On.FistVR.ClosedBolt.BoltEvent_ArriveAtFore += ClosedBolt_BoltEvent_ArriveAtFore;
			On.FistVR.TubeFedShotgunBolt.BoltEvent_ArriveAtFore += TubeFedShotgunBolt_BoltEvent_ArriveAtFore;

			// SeerSlip
			On.FistVR.OpenBoltReceiverBolt.BoltEvent_BoltCaught += OpenBoltReceiverBolt_BoltEvent_BoltCaught;
		}

		private void OnDestroy()
		{
			// Patches
			On.FistVR.BreakActionWeapon.Awake -= BreakActionWeapon_Awake;

			// Fire
			On.FistVR.ClosedBoltWeapon.Fire -= ClosedBoltWeapon_Fire;
			On.FistVR.OpenBoltReceiver.Fire -= OpenBoltReceiver_Fire;
			On.FistVR.Handgun.Fire -= Handgun_Fire;
			On.FistVR.TubeFedShotgun.Fire -= TubeFedShotgun_Fire;
			On.FistVR.Revolver.Fire -= Revolver_Fire;
			On.FistVR.RevolvingShotgun.Fire -= RevolvingShotgun_Fire;
			On.FistVR.RollingBlock.Fire -= RollingBlock_Fire;
			On.FistVR.BreakActionWeapon.Fire -= BreakActionWeapon_Fire;
			On.FistVR.BoltActionRifle.Fire -= BoltActionRifle_Fire;
			On.FistVR.LeverActionFirearm.Fire -= LeverActionFirearm_Fire;

			// Feed
			On.FistVR.ClosedBoltWeapon.BeginChamberingRound -= ClosedBoltWeapon_BeginChamberingRound;
			On.FistVR.OpenBoltReceiver.BeginChamberingRound -= OpenBoltReceiver_BeginChamberingRound;
			On.FistVR.Handgun.ExtractRound -= Handgun_ExtractRound;
			On.FistVR.TubeFedShotgun.ExtractRound -= TubeFedShotgun_ExtractRound;
			On.FistVR.BreakActionWeapon.PopOutEmpties -= BreakActionWeapon_PopOutEmpties;

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

			// Slamfire
			On.FistVR.HandgunSlide.SlideEvent_ArriveAtFore -= HandgunSlide_SlideEvent_ArriveAtFore;
			On.FistVR.ClosedBolt.BoltEvent_ArriveAtFore -= ClosedBolt_BoltEvent_ArriveAtFore;
			On.FistVR.TubeFedShotgunBolt.BoltEvent_ArriveAtFore -= TubeFedShotgunBolt_BoltEvent_ArriveAtFore;

			// SeerSlip
			On.FistVR.OpenBoltReceiverBolt.BoltEvent_BoltCaught -= OpenBoltReceiverBolt_BoltEvent_BoltCaught;
		}

		private bool Failed(FVRFireArm gun, Mapper<FailureMask, float> type, FailureType failure)
		{
			var ran = Random.Range(0f, 1f);
			var chance = type(Failure[gun]) * Configs.Multiplier.Value;
				
			if (Configs.EnableLogging.Value && !_lockFlag && !_extractFlag)
			{
				var builder = new StringBuilder().AppendLine()
					.Append("┌─────Failure Roll Report─────").AppendLine()
					.Append("│ ItemID: ").Append(gun.ObjectWrapper.ItemID).AppendLine()
					.Append("│  Era: ").Append(gun.ObjectWrapper.TagEra).AppendLine()
					.Append("│  Action: ").Append(gun.ObjectWrapper.TagFirearmAction).AppendLine()
					.Append("│  Round: ").Append(gun.RoundType).AppendLine()
					.Append("│  Magazine: ").Append(gun.Magazine == null ? "" : (gun.Magazine.IsIntegrated ? gun.Magazine.name : gun.Magazine.ObjectWrapper.ItemID)).AppendLine()
					.Append("│ Failure Rolled: ").Append(failure).AppendLine()
					.Append("│  Random: ").Append(ran).AppendLine()
					.Append("│  Chance: ").Append(chance).AppendLine()
					.Append("└─────────────────────────────");

				Logger.LogDebug(builder);
			}

			// These base methods are run in Update() - use dumb lock to prevent spam for now
			_extractFlag = false;
			_lockFlag = false;
			switch (failure)
			{
				case (FailureType.Extract):
					_extractFlag = true;
					break;
				case (FailureType.LockOpen):
					_lockFlag = true;
					break;
				default:
					break;
			}

			return ran <= chance;
		}

		#region Patches
		private void BreakActionWeapon_Awake(On.FistVR.BreakActionWeapon.orig_Awake orig, BreakActionWeapon self)
		{
			self.RotationInterpSpeed = 1;
			orig(self);
		}

		#endregion

		#region Fire	
		private bool ClosedBoltWeapon_Fire(On.FistVR.ClosedBoltWeapon.orig_Fire orig, ClosedBoltWeapon self)
		{
			if (Failed(self, m => m.Fire, FailureType.Fire))
			{
				return false;
			}

			return orig(self);
		}

		private bool OpenBoltReceiver_Fire(On.FistVR.OpenBoltReceiver.orig_Fire orig, OpenBoltReceiver self)
		{
			if (Failed(self, m => m.Fire, FailureType.Fire))
			{
				return false;
			}

			return orig(self);
		}

		private bool Handgun_Fire(On.FistVR.Handgun.orig_Fire orig, Handgun self)
		{
			if (Failed(self, m => m.Fire, FailureType.Fire))
			{
				return false;
			}

			return orig(self);
		}

		private bool TubeFedShotgun_Fire(On.FistVR.TubeFedShotgun.orig_Fire orig, TubeFedShotgun self)
		{
			if (Failed(self, m => m.Fire, FailureType.Fire))
			{
				return false;
			}

			return orig(self);
		}

		private void Revolver_Fire(On.FistVR.Revolver.orig_Fire orig, Revolver self)
		{
			if (Failed(self, m => m.Fire, FailureType.Fire))
			{
				return;
			}

			orig(self);
		}

		private void RevolvingShotgun_Fire(On.FistVR.RevolvingShotgun.orig_Fire orig, RevolvingShotgun self)
		{
			if (Failed(self, m => m.Fire, FailureType.Fire))
			{
				return;
			}

			orig(self);
		}

		private void RollingBlock_Fire(On.FistVR.RollingBlock.orig_Fire orig, RollingBlock self)
		{
			if (Failed(self, m => m.Fire, FailureType.Fire))
			{
				return;
			}

			orig(self);
		}

		private bool BreakActionWeapon_Fire(On.FistVR.BreakActionWeapon.orig_Fire orig, BreakActionWeapon self, int b)
		{
			if (Failed(self, m => m.Fire, FailureType.Fire))
			{
				return false;
			}

			return orig(self, b);
		}

		private bool BoltActionRifle_Fire(On.FistVR.BoltActionRifle.orig_Fire orig, BoltActionRifle self)
		{
			if (Failed(self, m => m.Fire, FailureType.Fire))
			{
				return false;
			}

			return orig(self);
		}

		private void LeverActionFirearm_Fire(On.FistVR.LeverActionFirearm.orig_Fire orig, LeverActionFirearm self)
		{
			if (Failed(self, m => m.Fire, FailureType.Fire))
			{
				self.m_isHammerCocked = false;
				self.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
				return;
			}

			orig(self);
		}
		#endregion

		#region Feed
		private void ClosedBoltWeapon_BeginChamberingRound(On.FistVR.ClosedBoltWeapon.orig_BeginChamberingRound orig, ClosedBoltWeapon self)
		{
			if (Failed(self, m => m.Feed, FailureType.Feed))
			{
				return;
			}

			orig(self);
		}

		private void OpenBoltReceiver_BeginChamberingRound(On.FistVR.OpenBoltReceiver.orig_BeginChamberingRound orig, OpenBoltReceiver self)
		{
			if (Failed(self, m => m.Feed, FailureType.Feed))
			{
				return;
			}

			orig(self);
		}

		private void Handgun_ExtractRound(On.FistVR.Handgun.orig_ExtractRound orig, Handgun self)
		{
			if (Failed(self, m => m.Feed, FailureType.Feed))
			{
				return;
			}

			orig(self);
		}

		private void TubeFedShotgun_ExtractRound(On.FistVR.TubeFedShotgun.orig_ExtractRound orig, TubeFedShotgun self)
		{
			if (Failed(self, m => m.Feed, FailureType.Feed))
			{
				return;
			}

			orig(self);
		}
		#endregion

		#region Extract
		private void ClosedBolt_ImpartFiringImpulse(On.FistVR.ClosedBolt.orig_ImpartFiringImpulse orig, ClosedBolt self)
		{
			if (_extractFlag) { return; }
			if (Failed(self.Weapon, m => m.Extract, FailureType.Extract))
			{
				self.RotationInterpSpeed = 2;
				return;
			}

			orig(self);
		}

		private void OpenBoltReceiverBolt_ImpartFiringImpulse(On.FistVR.OpenBoltReceiverBolt.orig_ImpartFiringImpulse orig, OpenBoltReceiverBolt self)
		{
			if (_extractFlag) { return; }
			if (Failed(self.Receiver, m => m.Extract, FailureType.Extract))
			{
				self.RotationInterpSpeed = 2;
				return;
			}

			orig(self);
		}

		private void HandgunSlide_ImpartFiringImpulse(On.FistVR.HandgunSlide.orig_ImpartFiringImpulse orig, HandgunSlide self)
		{
			if (_extractFlag) { return; }
			if (Failed(self.Handgun, m => m.Extract, FailureType.Extract))
			{
				self.RotationInterpSpeed = 2;
				return;
			}

			orig(self);
		}

		private void TubeFedShotgun_EjectExtractedRound(On.FistVR.TubeFedShotgun.orig_EjectExtractedRound orig, TubeFedShotgun self)
		{
			if (_extractFlag) { return; }
			if (Failed(self, m => m.Extract, FailureType.Extract))
			{
				self.RotationInterpSpeed = 2;
				return;
			}

			orig(self);
		}

		private void BreakActionWeapon_PopOutRound(On.FistVR.BreakActionWeapon.orig_PopOutRound orig, BreakActionWeapon self, FVRFireArmChamber chamber)
		{
			if (_extractFlag) { return; }
			if (Failed(self, m => m.Extract, FailureType.Extract))
			{
				return;
			}

			orig(self, chamber);
		}

		private void BreakActionWeapon_PopOutEmpties(On.FistVR.BreakActionWeapon.orig_PopOutEmpties orig, BreakActionWeapon self)
		{
			if (_extractFlag) { return; }
			if (Failed(self, m => m.Extract, FailureType.Extract))
			{
				return;
			}

			orig(self);
		}
		#endregion

		#region LockOpen
		private void Handgun_EngageSlideRelease(On.FistVR.Handgun.orig_EngageSlideRelease orig, Handgun self)
		{
			if (Failed(self, m => m.LockOpen, FailureType.LockOpen))
			{
				return;
			}

			orig(self);
		}

		private void ClosedBolt_LockBolt(On.FistVR.ClosedBolt.orig_LockBolt orig, ClosedBolt self)
		{
			if (Failed(self.Weapon, m => m.LockOpen, FailureType.LockOpen))
			{
				return;
			}

			orig(self);
		}
		#endregion

		#region AccDischarge
		private void HandgunSlide_SlideEvent_ArriveAtFore(On.FistVR.HandgunSlide.orig_SlideEvent_ArriveAtFore orig, HandgunSlide self)
		{
			if (Failed(self.Handgun, m => m.AccDischarge, FailureType.AccDischarge))
			{
				self.Handgun.ChamberRound();
				self.Handgun.DropHammer(false);
			}

			orig(self);
		}

		private void ClosedBolt_BoltEvent_ArriveAtFore(On.FistVR.ClosedBolt.orig_BoltEvent_ArriveAtFore orig, ClosedBolt self)
		{
			if (Failed(self.Weapon, m => m.AccDischarge, FailureType.AccDischarge))
			{
				self.Weapon.ChamberRound();
				self.Weapon.DropHammer();
			}

			orig(self);
		}

		private void TubeFedShotgunBolt_BoltEvent_ArriveAtFore(On.FistVR.TubeFedShotgunBolt.orig_BoltEvent_ArriveAtFore orig, TubeFedShotgunBolt self)
		{
			if (Failed(self.Shotgun, m => m.AccDischarge, FailureType.AccDischarge))
			{
				self.Shotgun.ChamberRound();
				self.Shotgun.ReleaseHammer();
			}

			orig(self);
		}

		private void OpenBoltReceiverBolt_BoltEvent_BoltCaught(On.FistVR.OpenBoltReceiverBolt.orig_BoltEvent_BoltCaught orig, OpenBoltReceiverBolt self)
		{
			if (Failed(self.Receiver, m => m.AccDischarge, FailureType.AccDischarge))
			{
				self.Receiver.ReleaseSeer();
			}
			orig(self);
		}
		#endregion

		public static IFailure AddFailure<TKey>(string name, Mapper<FVRFireArm, TKey> keyFromGun)
		{
			if (Module.Kernel.Get<IAssetReader<Option<Dictionary<TKey, FailureMask>>>>().IsNone)
			{
				Module.Kernel.BindJson<Dictionary<TKey, FailureMask>>();
			}
			
			var dict = new Dictionary<TKey, FailureMask>();
			var failure = new DictFailure<TKey>(dict, keyFromGun);
			var loader = new DictLoader<TKey>(dict);

			DeliFramework.AddAssetLoader(name, loader);

			return failure;
		}
	}

	internal static class ExtPlugin
	{
		public static IFailure AddFailure<TKey>(this IFailure inner, string name, Mapper<FVRFireArm, TKey> keyFromGun)
		{
			var weighted = Plugin.AddFailure<TKey>(name, keyFromGun);
			return new WeightedSumFailure(weighted, inner);
		}
	}
}
