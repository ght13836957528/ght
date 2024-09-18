/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {
	[System.Serializable]
	public class BlendModeMaterials {

		public const string MATERIAL_SUFFIX_MULTIPLY = "-Multiply";
		public const string MATERIAL_SUFFIX_SCREEN = "-Screen";
		public const string MATERIAL_SUFFIX_ADDITIVE = "-Additive";

		[System.Serializable]
		public class ReplacementMaterial {
			public string pageName;
			public Material material;
		}

		[SerializeField, HideInInspector] protected bool requiresBlendModeMaterials = false;
		public bool applyAdditiveMaterial = false;

		public List<ReplacementMaterial> additiveMaterials = new List<ReplacementMaterial>();
		public List<ReplacementMaterial> multiplyMaterials = new List<ReplacementMaterial>();
		public List<ReplacementMaterial> screenMaterials = new List<ReplacementMaterial>();

		public bool RequiresBlendModeMaterials { get { return requiresBlendModeMaterials; } set { requiresBlendModeMaterials = value; } }
	
		
		public BlendMode BlendModeForMaterial (Material material) {
			foreach (ReplacementMaterial pair in multiplyMaterials)
				if (pair.material == material) return BlendMode.Multiply;
			foreach (ReplacementMaterial pair in additiveMaterials)
				if (pair.material == material) return BlendMode.Additive;
			foreach (ReplacementMaterial pair in screenMaterials)
				if (pair.material == material) return BlendMode.Screen;
			return BlendMode.Normal;
		}
		
	#if UNITY_EDITOR
		public void TransferSettingsFrom (BlendModeMaterialsAsset modifierAsset) {
			applyAdditiveMaterial = modifierAsset.applyAdditiveMaterial;
		}
#endif

		public bool UpdateBlendmodeMaterialsRequiredState (SkeletonData skeletonData) {
			requiresBlendModeMaterials = false;

			if (skeletonData == null) throw new ArgumentNullException("skeletonData");

			var skinEntries = new List<Skin.SkinEntry>();
			var slotsItems = skeletonData.Slots.Items;
			for (int slotIndex = 0, slotCount = skeletonData.Slots.Count; slotIndex < slotCount; slotIndex++) {
				var slot = slotsItems[slotIndex];
				if (slot.blendMode == BlendMode.Normal) continue;
				if (!applyAdditiveMaterial && slot.blendMode == BlendMode.Additive) continue;

				skinEntries.Clear();
				foreach (var skin in skeletonData.Skins)
					skin.GetAttachments(slotIndex, skinEntries);

				foreach (var entry in skinEntries) {
					if (entry.Attachment is IHasRendererObject) {
						requiresBlendModeMaterials = true;
						return true;
					}
				}
			}
			return false;
		}

		[System.Serializable]
		public class TemplateMaterials {
			public Material additiveTemplate;
			public Material multiplyTemplate;
			public Material screenTemplate;
		};

		public delegate bool CreateForRegionDelegate (ref List<BlendModeMaterials.ReplacementMaterial> replacementMaterials,
			ref bool anyReplacementMaterialsChanged,
			AtlasRegion originalRegion, Material materialTemplate, string materialSuffix,
			SkeletonDataAsset skeletonDataAsset);

		public static bool CreateAndAssignMaterials (SkeletonDataAsset skeletonDataAsset,
				TemplateMaterials templateMaterials, ref bool anyReplacementMaterialsChanged) {

			return CreateAndAssignMaterials(skeletonDataAsset,
				templateMaterials, ref anyReplacementMaterialsChanged,
				(asset) => { asset.Clear(); }, null, CreateForRegion);
		}

		public static bool CreateAndAssignMaterials (SkeletonDataAsset skeletonDataAsset,
			TemplateMaterials templateMaterials, ref bool anyReplacementMaterialsChanged,
			System.Action<SkeletonDataAsset> clearSkeletonDataAssetFunc,
			System.Action<SkeletonDataAsset> afterAssetModifiedFunc,
			CreateForRegionDelegate createForRegionFunc) {

			bool anyCreationFailed = false;
			BlendModeMaterials blendModeMaterials = skeletonDataAsset.blendModeMaterials;
			bool applyAdditiveMaterial = blendModeMaterials.applyAdditiveMaterial;

			List<Skin.SkinEntry> skinEntries = new List<Skin.SkinEntry>();

			clearSkeletonDataAssetFunc(skeletonDataAsset);
			skeletonDataAsset.isUpgradingBlendModeMaterials = true;
			SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(true);

			SlotData[] slotsItems = skeletonData.Slots.Items;
			for (int slotIndex = 0, slotCount = skeletonData.Slots.Count; slotIndex < slotCount; slotIndex++) {
				SlotData slot = slotsItems[slotIndex];
				if (slot.BlendMode == BlendMode.Normal) continue;
				if (!applyAdditiveMaterial && slot.BlendMode == BlendMode.Additive) continue;

				List<BlendModeMaterials.ReplacementMaterial> replacementMaterials = null;
				Material materialTemplate = null;
				string materialSuffix = null;
				switch (slot.BlendMode) {
				case BlendMode.Multiply:
					replacementMaterials = blendModeMaterials.multiplyMaterials;
					materialTemplate = templateMaterials.multiplyTemplate;
					materialSuffix = MATERIAL_SUFFIX_MULTIPLY;
					break;
				case BlendMode.Screen:
					replacementMaterials = blendModeMaterials.screenMaterials;
					materialTemplate = templateMaterials.screenTemplate;
					materialSuffix = MATERIAL_SUFFIX_SCREEN;
					break;
				case BlendMode.Additive:
					replacementMaterials = blendModeMaterials.additiveMaterials;
					materialTemplate = templateMaterials.additiveTemplate;
					materialSuffix = MATERIAL_SUFFIX_ADDITIVE;
					break;
				}

				skinEntries.Clear();
				foreach (Skin skin in skeletonData.Skins)
					skin.GetAttachments(slotIndex, skinEntries);

				foreach (Skin.SkinEntry entry in skinEntries) {
					var renderableAttachment = entry.Attachment as IHasRendererObject;
					if (renderableAttachment != null) {
						AtlasRegion originalRegion = (AtlasRegion)renderableAttachment.RendererObject;
						if (originalRegion != null) {
							anyCreationFailed |= createForRegionFunc(
								ref replacementMaterials, ref anyReplacementMaterialsChanged,
								originalRegion, materialTemplate, materialSuffix, skeletonDataAsset);
						}
					}
				}
			}
			skeletonDataAsset.isUpgradingBlendModeMaterials = false;
			if (afterAssetModifiedFunc != null) afterAssetModifiedFunc(skeletonDataAsset);
			return !anyCreationFailed;
		}

		protected static bool CreateForRegion (ref List<BlendModeMaterials.ReplacementMaterial> replacementMaterials,
			ref bool anyReplacementMaterialsChanged,
			AtlasRegion originalRegion, Material materialTemplate, string materialSuffix,
			SkeletonDataAsset skeletonDataAsset) {

			bool anyCreationFailed = false;
			bool replacementExists = replacementMaterials.Exists(
				replacement => replacement.pageName == originalRegion.page.name);
			if (!replacementExists) {
				BlendModeMaterials.ReplacementMaterial replacement = CreateReplacementMaterial(originalRegion, materialTemplate, materialSuffix);
				if (replacement != null) {
					replacementMaterials.Add(replacement);
					anyReplacementMaterialsChanged = true;
				} else {
					Debug.LogError(string.Format("Failed creating blend mode Material for SkeletonData asset '{0}'," +
						" atlas page '{1}', template '{2}'.",
						skeletonDataAsset.name, originalRegion.page.name, materialTemplate.name),
						skeletonDataAsset);
					anyCreationFailed = true;
				}
			}
			return anyCreationFailed;
		}

		protected static BlendModeMaterials.ReplacementMaterial CreateReplacementMaterial (
			AtlasRegion originalRegion, Material materialTemplate, string materialSuffix) {

			BlendModeMaterials.ReplacementMaterial newReplacement = new BlendModeMaterials.ReplacementMaterial();
			AtlasPage originalPage = originalRegion.page;
			Material originalMaterial = originalPage.rendererObject as Material;

			newReplacement.pageName = originalPage.name;

			Material blendModeMaterial = new Material(materialTemplate) {
				name = originalMaterial.name + " " + materialTemplate.name,
				mainTexture = originalMaterial.mainTexture
			};
			newReplacement.material = blendModeMaterial;

			if (newReplacement.material)
				return newReplacement;
			else
				return null;
		}

		public void ApplyMaterials (SkeletonData skeletonData) {
			if (skeletonData == null) throw new ArgumentNullException("skeletonData");
			if (!requiresBlendModeMaterials)
				return;

			var skinEntries = new List<Skin.SkinEntry>();
			var slotsItems = skeletonData.Slots.Items;
			for (int slotIndex = 0, slotCount = skeletonData.Slots.Count; slotIndex < slotCount; slotIndex++) {
				var slot = slotsItems[slotIndex];
				if (slot.blendMode == BlendMode.Normal) continue;
				if (!applyAdditiveMaterial && slot.blendMode == BlendMode.Additive) continue;

				List<ReplacementMaterial> replacementMaterials = null;
				switch (slot.blendMode) {
					case BlendMode.Multiply:
						replacementMaterials = multiplyMaterials;
						break;
					case BlendMode.Screen:
						replacementMaterials = screenMaterials;
						break;
					case BlendMode.Additive:
						replacementMaterials = additiveMaterials;
						break;
				}
				if (replacementMaterials == null)
					continue;

				skinEntries.Clear();
				foreach (var skin in skeletonData.Skins)
					skin.GetAttachments(slotIndex, skinEntries);

				foreach (var entry in skinEntries) {
					var renderableAttachment = entry.Attachment as IHasRendererObject;
					if (renderableAttachment != null) {
						renderableAttachment.RendererObject = CloneAtlasRegionWithMaterial(
							(AtlasRegion)renderableAttachment.RendererObject, replacementMaterials);
					}
				}
			}
		}

		protected AtlasRegion CloneAtlasRegionWithMaterial (AtlasRegion originalRegion, List<ReplacementMaterial> replacementMaterials) {
			var newRegion = originalRegion.Clone();
			Material material = null;
			foreach (var replacement in replacementMaterials) {
				if (replacement.pageName == originalRegion.page.name) {
					material = replacement.material;
					break;
				}
			}

			AtlasPage originalPage = originalRegion.page;
			var newPage = originalPage.Clone();
			newPage.rendererObject = material;
			newRegion.page = newPage;
			return newRegion;
		}
	}
}
