using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisEquipment : MonoBehaviour, IMonoUpdater
{
	[Serializable]
	public class PlayerModel
	{
		public Mesh m_mesh;

		public Material m_baseMaterial;
	}

	public SkinnedMeshRenderer m_bodyModel;

	public ZNetView m_nViewOverride;

	[Header("Attachment points")]
	public Transform m_leftHand;

	public Transform m_rightHand;

	public Transform m_helmet;

	public Transform m_backShield;

	public Transform m_backMelee;

	public Transform m_backTwohandedMelee;

	public Transform m_backBow;

	public Transform m_backTool;

	public Transform m_backAtgeir;

	public CapsuleCollider[] m_clothColliders = Array.Empty<CapsuleCollider>();

	public PlayerModel[] m_models = Array.Empty<PlayerModel>();

	public bool m_isPlayer;

	public bool m_useAllTrails;

	public bool m_isArmorStand;

	private string m_leftItem = "";

	private string m_rightItem = "";

	private string m_chestItem = "";

	private string m_legItem = "";

	private string m_helmetItem = "";

	private string m_shoulderItem = "";

	private string m_beardItem = "";

	private string m_hairItem = "";

	private string m_utilityItem = "";

	private string m_leftBackItem = "";

	private string m_rightBackItem = "";

	private string m_trinketItem = "";

	private int m_shoulderItemVariant;

	private int m_leftItemVariant;

	private int m_leftBackItemVariant;

	private GameObject m_leftItemInstance;

	private GameObject m_rightItemInstance;

	private GameObject m_helmetItemInstance;

	private List<GameObject> m_chestItemInstances;

	private List<GameObject> m_legItemInstances;

	private List<GameObject> m_shoulderItemInstances;

	private List<GameObject> m_utilityItemInstances;

	private List<GameObject> m_trinketItemInstances;

	private GameObject m_beardItemInstance;

	private GameObject m_hairItemInstance;

	private GameObject m_leftBackItemInstance;

	private GameObject m_rightBackItemInstance;

	private int m_currentLeftItemHash;

	private int m_currentRightItemHash;

	private int m_currentChestItemHash;

	private int m_currentLegItemHash;

	private int m_currentHelmetItemHash;

	private int m_currentShoulderItemHash;

	private int m_currentBeardItemHash;

	private int m_currentHairItemHash;

	private int m_currentUtilityItemHash;

	private int m_currentTrinketItemHash;

	private int m_currentLeftBackItemHash;

	private int m_currentRightBackItemHash;

	private int m_currentShoulderItemVariant;

	private int m_currentLeftItemVariant;

	private int m_currentLeftBackItemVariant;

	private ItemDrop.ItemData.HelmetHairType m_helmetHideHair;

	private ItemDrop.ItemData.HelmetHairType m_helmetHideBeard;

	private Texture m_emptyBodyTexture;

	private Texture m_emptyBodyBumpTexture;

	private Texture m_emptyLegsTexture;

	private Texture m_emptyLegsBumpTexture;

	private int m_modelIndex;

	private Vector3 m_skinColor = Vector3.one;

	private Vector3 m_hairColor = Vector3.one;

	private int m_currentModelIndex;

	private ZNetView m_nview;

	private GameObject m_visual;

	private LODGroup m_lodGroup;

	public static List<IMonoUpdater> Instances { get; } = new List<IMonoUpdater>();


	private void Awake()
	{
		m_nview = (((Object)(object)m_nViewOverride != (Object)null) ? m_nViewOverride : ((Component)this).GetComponent<ZNetView>());
		Transform val = ((Component)this).transform.Find("Visual");
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)this).transform;
		}
		m_visual = ((Component)val).gameObject;
		m_lodGroup = m_visual.GetComponentInChildren<LODGroup>();
		if ((Object)(object)m_bodyModel != (Object)null && ((Renderer)m_bodyModel).material.HasProperty("_ChestTex"))
		{
			m_emptyBodyTexture = ((Renderer)m_bodyModel).material.GetTexture("_ChestTex");
		}
		if ((Object)(object)m_bodyModel != (Object)null && ((Renderer)m_bodyModel).material.HasProperty("_LegsTex"))
		{
			m_emptyLegsTexture = ((Renderer)m_bodyModel).material.GetTexture("_LegsTex");
		}
		if ((Object)(object)m_bodyModel != (Object)null && ((Renderer)m_bodyModel).material.HasProperty("_ChestBumpMap"))
		{
			m_emptyBodyBumpTexture = ((Renderer)m_bodyModel).material.GetTexture("_ChestBumpMap");
		}
		if ((Object)(object)m_bodyModel != (Object)null && ((Renderer)m_bodyModel).material.HasProperty("_LegsBumpMap"))
		{
			m_emptyLegsBumpTexture = ((Renderer)m_bodyModel).material.GetTexture("_LegsBumpMap");
		}
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	private void Start()
	{
		UpdateVisuals();
	}

	public void SetWeaponTrails(bool enabled)
	{
		if (m_useAllTrails)
		{
			MeleeWeaponTrail[] componentsInChildren = ((Component)this).gameObject.GetComponentsInChildren<MeleeWeaponTrail>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Emit = enabled;
			}
		}
		else if (Object.op_Implicit((Object)(object)m_rightItemInstance))
		{
			MeleeWeaponTrail[] componentsInChildren = m_rightItemInstance.GetComponentsInChildren<MeleeWeaponTrail>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Emit = enabled;
			}
		}
	}

	public void SetModel(int index)
	{
		if (m_modelIndex != index && index >= 0 && index < m_models.Length)
		{
			ZLog.Log((object)("Vis equip model set to " + index));
			m_modelIndex = index;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_modelIndex, m_modelIndex);
			}
		}
	}

	public void SetSkinColor(Vector3 color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!(color == m_skinColor))
		{
			m_skinColor = color;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_skinColor, m_skinColor);
			}
		}
	}

	public void SetHairColor(Vector3 color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_hairColor == color))
		{
			m_hairColor = color;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_hairColor, m_hairColor);
			}
		}
	}

	public void SetItem(VisSlot slot, string name, int variant = 0)
	{
		switch (slot)
		{
		case VisSlot.HandLeft:
			SetLeftItem(name, variant);
			break;
		case VisSlot.HandRight:
			SetRightItem(name);
			break;
		case VisSlot.BackLeft:
			SetLeftBackItem(name, variant);
			break;
		case VisSlot.BackRight:
			SetRightBackItem(name);
			break;
		case VisSlot.Chest:
			SetChestItem(name);
			break;
		case VisSlot.Legs:
			SetLegItem(name);
			break;
		case VisSlot.Helmet:
			SetHelmetItem(name);
			break;
		case VisSlot.Shoulder:
			SetShoulderItem(name, variant);
			break;
		case VisSlot.Utility:
			SetUtilityItem(name);
			break;
		case VisSlot.Beard:
			SetBeardItem(name);
			break;
		case VisSlot.Hair:
			SetHairItem(name);
			break;
		default:
			throw new NotImplementedException("Unknown slot: " + slot);
		}
	}

	public void SetLeftItem(string name, int variant)
	{
		if (!(m_leftItem == name) || m_leftItemVariant != variant)
		{
			m_leftItem = name;
			m_leftItemVariant = variant;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_leftItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
				m_nview.GetZDO().Set(ZDOVars.s_leftItemVariant, variant);
			}
		}
	}

	public void SetRightItem(string name)
	{
		if (!(m_rightItem == name))
		{
			m_rightItem = name;
			SetRightItemVisual(name);
		}
	}

	public void SetRightItemVisual(string name)
	{
		if (m_nview.GetZDO() != null && m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_rightItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
		}
	}

	public void SetLeftBackItem(string name, int variant)
	{
		if (!(m_leftBackItem == name) || m_leftBackItemVariant != variant)
		{
			m_leftBackItem = name;
			m_leftBackItemVariant = variant;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_leftBackItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
				m_nview.GetZDO().Set(ZDOVars.s_leftBackItemVariant, variant);
			}
		}
	}

	public void SetRightBackItem(string name)
	{
		if (!(m_rightBackItem == name))
		{
			m_rightBackItem = name;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_rightBackItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
			}
		}
	}

	public void SetChestItem(string name)
	{
		if (!(m_chestItem == name))
		{
			m_chestItem = name;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_chestItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
			}
		}
	}

	public void SetLegItem(string name)
	{
		if (!(m_legItem == name))
		{
			m_legItem = name;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_legItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
			}
		}
	}

	public void SetHelmetItem(string name)
	{
		if (!(m_helmetItem == name))
		{
			m_helmetItem = name;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_helmetItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
			}
		}
	}

	public void SetShoulderItem(string name, int variant)
	{
		if (!(m_shoulderItem == name) || m_shoulderItemVariant != variant)
		{
			m_shoulderItem = name;
			m_shoulderItemVariant = variant;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_shoulderItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
				m_nview.GetZDO().Set(ZDOVars.s_shoulderItemVariant, variant);
			}
		}
	}

	public void SetBeardItem(string name)
	{
		if (!(m_beardItem == name))
		{
			m_beardItem = name;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_beardItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
			}
		}
	}

	public void SetHairItem(string name)
	{
		if (!(m_hairItem == name))
		{
			m_hairItem = name;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_hairItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
			}
		}
	}

	public void SetUtilityItem(string name)
	{
		if (!(m_utilityItem == name))
		{
			m_utilityItem = name;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_utilityItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
			}
		}
	}

	public void SetTrinketItem(string name)
	{
		if (!(m_trinketItem == name))
		{
			m_trinketItem = name;
			if (m_nview.GetZDO() != null && m_nview.IsOwner())
			{
				m_nview.GetZDO().Set(ZDOVars.s_trinketItem, (!string.IsNullOrEmpty(name)) ? StringExtensionMethods.GetStableHashCode(name) : 0);
			}
		}
	}

	public void CustomUpdate(float deltaTime, float time)
	{
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (m_isPlayer && !m_isArmorStand)
		{
			UpdateBaseModel();
			UpdateColors();
		}
		UpdateEquipmentVisuals();
	}

	private void UpdateColors()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		Color val = Utils.Vec3ToColor(m_skinColor);
		Color val2 = Utils.Vec3ToColor(m_hairColor);
		if (m_nview.GetZDO() != null)
		{
			val = Utils.Vec3ToColor(m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.one));
			val2 = Utils.Vec3ToColor(m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.one));
		}
		((Renderer)m_bodyModel).materials[0].SetColor("_SkinColor", val);
		((Renderer)m_bodyModel).materials[1].SetColor("_SkinColor", val2);
		if (Object.op_Implicit((Object)(object)m_beardItemInstance))
		{
			Renderer[] componentsInChildren = m_beardItemInstance.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.SetColor("_SkinColor", val2);
			}
		}
		if (Object.op_Implicit((Object)(object)m_hairItemInstance))
		{
			Renderer[] componentsInChildren = m_hairItemInstance.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.SetColor("_SkinColor", val2);
			}
		}
	}

	private void UpdateBaseModel()
	{
		if (m_models.Length == 0)
		{
			return;
		}
		int num = m_modelIndex;
		if (m_nview.GetZDO() != null)
		{
			num = m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex);
		}
		if (m_currentModelIndex != num || (Object)(object)m_bodyModel.sharedMesh != (Object)(object)m_models[num].m_mesh)
		{
			m_currentModelIndex = num;
			m_bodyModel.sharedMesh = m_models[num].m_mesh;
			((Renderer)m_bodyModel).materials[0].SetTexture("_MainTex", m_models[num].m_baseMaterial.GetTexture("_MainTex"));
			((Renderer)m_bodyModel).materials[0].SetTexture("_SkinBumpMap", m_models[num].m_baseMaterial.GetTexture("_SkinBumpMap"));
			if (m_currentChestItemHash == 0)
			{
				((Renderer)m_bodyModel).materials[0].SetTexture("_ChestTex", m_models[num].m_baseMaterial.GetTexture("_ChestTex"));
				((Renderer)m_bodyModel).materials[0].SetTexture("_ChestBumpMap", m_models[num].m_baseMaterial.GetTexture("_ChestBumpMap"));
			}
			if (m_currentLegItemHash == 0)
			{
				((Renderer)m_bodyModel).materials[0].SetTexture("_LegsTex", m_models[num].m_baseMaterial.GetTexture("_LegsTex"));
				((Renderer)m_bodyModel).materials[0].SetTexture("_LegsBumpMap", m_models[num].m_baseMaterial.GetTexture("_LegsBumpMap"));
			}
			if (m_models[num].m_baseMaterial.HasProperty("_ChestTex"))
			{
				m_emptyBodyTexture = m_models[num].m_baseMaterial.GetTexture("_ChestTex");
			}
			if (m_models[num].m_baseMaterial.HasProperty("_ChestBumpMap"))
			{
				m_emptyBodyBumpTexture = m_models[num].m_baseMaterial.GetTexture("_ChestBumpMap");
			}
			if (m_models[num].m_baseMaterial.HasProperty("_LegsTex"))
			{
				m_emptyLegsTexture = m_models[num].m_baseMaterial.GetTexture("_LegsTex");
			}
			if (m_models[num].m_baseMaterial.HasProperty("_LegsBumpMap"))
			{
				m_emptyLegsBumpTexture = m_models[num].m_baseMaterial.GetTexture("_LegsBumpMap");
			}
		}
	}

	private void UpdateEquipmentVisuals()
	{
		int hash = 0;
		int rightHandEquipped = 0;
		int chestEquipped = 0;
		int legEquipped = 0;
		int hash2 = 0;
		int itemHash = 0;
		int num = 0;
		int hash3 = 0;
		int utilityEquipped = 0;
		int trinketEquipped = 0;
		int leftItem = 0;
		int rightItem = 0;
		int variant = m_shoulderItemVariant;
		int variant2 = m_leftItemVariant;
		int leftVariant = m_leftBackItemVariant;
		ZDO zDO = m_nview.GetZDO();
		if (zDO != null)
		{
			hash = zDO.GetInt(ZDOVars.s_leftItem);
			rightHandEquipped = zDO.GetInt(ZDOVars.s_rightItem);
			chestEquipped = zDO.GetInt(ZDOVars.s_chestItem);
			legEquipped = zDO.GetInt(ZDOVars.s_legItem);
			hash2 = zDO.GetInt(ZDOVars.s_helmetItem);
			hash3 = zDO.GetInt(ZDOVars.s_shoulderItem);
			utilityEquipped = zDO.GetInt(ZDOVars.s_utilityItem);
			trinketEquipped = zDO.GetInt(ZDOVars.s_trinketItem);
			if (m_isPlayer)
			{
				if (!m_isArmorStand)
				{
					itemHash = zDO.GetInt(ZDOVars.s_beardItem);
					num = zDO.GetInt(ZDOVars.s_hairItem);
				}
				leftItem = zDO.GetInt(ZDOVars.s_leftBackItem);
				rightItem = zDO.GetInt(ZDOVars.s_rightBackItem);
				variant = zDO.GetInt(ZDOVars.s_shoulderItemVariant);
				variant2 = zDO.GetInt(ZDOVars.s_leftItemVariant);
				leftVariant = zDO.GetInt(ZDOVars.s_leftBackItemVariant);
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(m_leftItem))
			{
				hash = StringExtensionMethods.GetStableHashCode(m_leftItem);
			}
			if (!string.IsNullOrEmpty(m_rightItem))
			{
				rightHandEquipped = StringExtensionMethods.GetStableHashCode(m_rightItem);
			}
			if (!string.IsNullOrEmpty(m_chestItem))
			{
				chestEquipped = StringExtensionMethods.GetStableHashCode(m_chestItem);
			}
			if (!string.IsNullOrEmpty(m_legItem))
			{
				legEquipped = StringExtensionMethods.GetStableHashCode(m_legItem);
			}
			if (!string.IsNullOrEmpty(m_helmetItem))
			{
				hash2 = StringExtensionMethods.GetStableHashCode(m_helmetItem);
			}
			if (!string.IsNullOrEmpty(m_shoulderItem))
			{
				hash3 = StringExtensionMethods.GetStableHashCode(m_shoulderItem);
			}
			if (!string.IsNullOrEmpty(m_utilityItem))
			{
				utilityEquipped = StringExtensionMethods.GetStableHashCode(m_utilityItem);
			}
			if (!string.IsNullOrEmpty(m_trinketItem))
			{
				trinketEquipped = StringExtensionMethods.GetStableHashCode(m_trinketItem);
			}
			if (m_isPlayer)
			{
				if (!m_isArmorStand)
				{
					if (!string.IsNullOrEmpty(m_beardItem))
					{
						itemHash = StringExtensionMethods.GetStableHashCode(m_beardItem);
					}
					if (!string.IsNullOrEmpty(m_hairItem))
					{
						num = StringExtensionMethods.GetStableHashCode(m_hairItem);
					}
				}
				if (!string.IsNullOrEmpty(m_leftBackItem))
				{
					leftItem = StringExtensionMethods.GetStableHashCode(m_leftBackItem);
				}
				if (!string.IsNullOrEmpty(m_rightBackItem))
				{
					rightItem = StringExtensionMethods.GetStableHashCode(m_rightBackItem);
				}
			}
		}
		bool flag = false;
		flag = SetRightHandEquipped(rightHandEquipped) || flag;
		flag = SetLeftHandEquipped(hash, variant2) || flag;
		flag = SetChestEquipped(chestEquipped) || flag;
		flag = SetLegEquipped(legEquipped) || flag;
		flag = SetHelmetEquipped(hash2, num) || flag;
		flag = SetShoulderEquipped(hash3, variant) || flag;
		flag = SetUtilityEquipped(utilityEquipped) || flag;
		flag = SetTrinketEquipped(trinketEquipped) || flag;
		if (m_isPlayer)
		{
			if (!m_isArmorStand)
			{
				itemHash = GetHairItem(m_helmetHideBeard, itemHash, ItemDrop.ItemData.AccessoryType.Beard);
				flag = SetBeardEquipped(itemHash) || flag;
				num = GetHairItem(m_helmetHideHair, num, ItemDrop.ItemData.AccessoryType.Hair);
				flag = SetHairEquipped(num) || flag;
			}
			flag = SetBackEquipped(leftItem, rightItem, leftVariant) || flag;
		}
		if (flag)
		{
			UpdateLodgroup();
		}
	}

	private int GetHairItem(ItemDrop.ItemData.HelmetHairType type, int itemHash, ItemDrop.ItemData.AccessoryType accessory)
	{
		if (type == ItemDrop.ItemData.HelmetHairType.Hidden)
		{
			return 0;
		}
		if (type == ItemDrop.ItemData.HelmetHairType.Default)
		{
			return itemHash;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
		if (Object.op_Implicit((Object)(object)itemPrefab))
		{
			ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
			if (component != null)
			{
				ItemDrop.ItemData.HelmetHairSettings helmetHairSettings = (accessory switch
				{
					ItemDrop.ItemData.AccessoryType.Hair => component.m_itemData.m_shared.m_helmetHairSettings, 
					ItemDrop.ItemData.AccessoryType.Beard => component.m_itemData.m_shared.m_helmetBeardSettings, 
					_ => throw new Exception("Acecssory type not implemented"), 
				}).FirstOrDefault((ItemDrop.ItemData.HelmetHairSettings x) => x.m_setting == type);
				if (helmetHairSettings != null)
				{
					return StringExtensionMethods.GetStableHashCode(((Object)helmetHairSettings.m_hairPrefab).name);
				}
			}
		}
		return 0;
	}

	private void UpdateLodgroup()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_lodGroup == (Object)null)
		{
			return;
		}
		List<Renderer> list = new List<Renderer>(m_visual.GetComponentsInChildren<Renderer>());
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Renderer val = list[num];
			LODGroup componentInParent = ((Component)val).GetComponentInParent<LODGroup>();
			if (componentInParent != null && (Object)(object)componentInParent != (Object)(object)m_lodGroup)
			{
				LOD[] lODs = componentInParent.GetLODs();
				for (int i = 0; i < lODs.Length; i++)
				{
					if (Array.IndexOf(lODs[i].renderers, val) >= 0)
					{
						list.RemoveAt(num);
						break;
					}
				}
			}
		}
		LOD[] lODs2 = m_lodGroup.GetLODs();
		lODs2[0].renderers = list.ToArray();
		m_lodGroup.SetLODs(lODs2);
	}

	private bool SetRightHandEquipped(int hash)
	{
		if (m_currentRightItemHash == hash)
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)m_rightItemInstance))
		{
			Object.Destroy((Object)(object)m_rightItemInstance);
			m_rightItemInstance = null;
		}
		m_currentRightItemHash = hash;
		if (hash != 0)
		{
			m_rightItemInstance = AttachItem(hash, 0, m_rightHand);
		}
		return true;
	}

	private bool SetLeftHandEquipped(int hash, int variant)
	{
		if (m_currentLeftItemHash == hash && m_currentLeftItemVariant == variant)
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)m_leftItemInstance))
		{
			Object.Destroy((Object)(object)m_leftItemInstance);
			m_leftItemInstance = null;
		}
		m_currentLeftItemHash = hash;
		m_currentLeftItemVariant = variant;
		if (hash != 0)
		{
			m_leftItemInstance = AttachItem(hash, variant, m_leftHand);
		}
		return true;
	}

	private bool SetBackEquipped(int leftItem, int rightItem, int leftVariant)
	{
		if (m_currentLeftBackItemHash == leftItem && m_currentRightBackItemHash == rightItem && m_currentLeftBackItemVariant == leftVariant)
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)m_leftBackItemInstance))
		{
			Object.Destroy((Object)(object)m_leftBackItemInstance);
			m_leftBackItemInstance = null;
		}
		if (Object.op_Implicit((Object)(object)m_rightBackItemInstance))
		{
			Object.Destroy((Object)(object)m_rightBackItemInstance);
			m_rightBackItemInstance = null;
		}
		m_currentLeftBackItemHash = leftItem;
		m_currentRightBackItemHash = rightItem;
		m_currentLeftBackItemVariant = leftVariant;
		if (m_currentLeftBackItemHash != 0)
		{
			m_leftBackItemInstance = AttachBackItem(leftItem, leftVariant, rightHand: false);
		}
		if (m_currentRightBackItemHash != 0)
		{
			m_rightBackItemInstance = AttachBackItem(rightItem, 0, rightHand: true);
		}
		return true;
	}

	private GameObject AttachBackItem(int hash, int variant, bool rightHand)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);
		if ((Object)(object)itemPrefab == (Object)null)
		{
			ZLog.Log((object)("Missing back attach item prefab: " + hash));
			return null;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		switch ((component.m_itemData.m_shared.m_attachOverride != 0) ? component.m_itemData.m_shared.m_attachOverride : component.m_itemData.m_shared.m_itemType)
		{
		case ItemDrop.ItemData.ItemType.Torch:
			if (rightHand)
			{
				return AttachItem(hash, variant, m_backMelee, enableEquipEffects: false, backAttach: true);
			}
			return AttachItem(hash, variant, m_backTool, enableEquipEffects: false, backAttach: true);
		case ItemDrop.ItemData.ItemType.Bow:
			return AttachItem(hash, variant, m_backBow, enableEquipEffects: false, backAttach: true);
		case ItemDrop.ItemData.ItemType.Tool:
			return AttachItem(hash, variant, m_backTool, enableEquipEffects: false, backAttach: true);
		case ItemDrop.ItemData.ItemType.Attach_Atgeir:
			return AttachItem(hash, variant, m_backAtgeir, enableEquipEffects: false, backAttach: true);
		case ItemDrop.ItemData.ItemType.OneHandedWeapon:
			return AttachItem(hash, variant, m_backMelee, enableEquipEffects: false, backAttach: true);
		case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
		case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
			return AttachItem(hash, variant, m_backTwohandedMelee, enableEquipEffects: false, backAttach: true);
		case ItemDrop.ItemData.ItemType.Shield:
			return AttachItem(hash, variant, m_backShield, enableEquipEffects: false, backAttach: true);
		default:
			return null;
		}
	}

	private bool SetChestEquipped(int hash)
	{
		if (m_currentChestItemHash == hash)
		{
			return false;
		}
		m_currentChestItemHash = hash;
		if ((Object)(object)m_bodyModel == (Object)null)
		{
			return true;
		}
		if (m_chestItemInstances != null)
		{
			foreach (GameObject chestItemInstance in m_chestItemInstances)
			{
				if (Object.op_Implicit((Object)(object)m_lodGroup))
				{
					Utils.RemoveFromLodgroup(m_lodGroup, chestItemInstance);
				}
				Object.Destroy((Object)(object)chestItemInstance);
			}
			m_chestItemInstances = null;
			((Renderer)m_bodyModel).material.SetTexture("_ChestTex", m_emptyBodyTexture);
			((Renderer)m_bodyModel).material.SetTexture("_ChestBumpMap", m_emptyBodyBumpTexture);
			((Renderer)m_bodyModel).material.SetTexture("_ChestMetal", (Texture)null);
		}
		if (m_currentChestItemHash == 0)
		{
			return true;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);
		if ((Object)(object)itemPrefab == (Object)null)
		{
			ZLog.Log((object)("Missing chest item " + hash));
			return true;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		if (Object.op_Implicit((Object)(object)component.m_itemData.m_shared.m_armorMaterial))
		{
			((Renderer)m_bodyModel).material.SetTexture("_ChestTex", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_ChestTex"));
			((Renderer)m_bodyModel).material.SetTexture("_ChestBumpMap", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_ChestBumpMap"));
			((Renderer)m_bodyModel).material.SetTexture("_ChestMetal", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_ChestMetal"));
		}
		m_chestItemInstances = AttachArmor(hash);
		return true;
	}

	private bool SetShoulderEquipped(int hash, int variant)
	{
		if (m_currentShoulderItemHash == hash && m_currentShoulderItemVariant == variant)
		{
			return false;
		}
		m_currentShoulderItemHash = hash;
		m_currentShoulderItemVariant = variant;
		if ((Object)(object)m_bodyModel == (Object)null)
		{
			return true;
		}
		if (m_shoulderItemInstances != null)
		{
			foreach (GameObject shoulderItemInstance in m_shoulderItemInstances)
			{
				if (Object.op_Implicit((Object)(object)m_lodGroup))
				{
					Utils.RemoveFromLodgroup(m_lodGroup, shoulderItemInstance);
				}
				Object.Destroy((Object)(object)shoulderItemInstance);
			}
			m_shoulderItemInstances = null;
		}
		if (m_currentShoulderItemHash == 0)
		{
			return true;
		}
		if ((Object)(object)ObjectDB.instance.GetItemPrefab(hash) == (Object)null)
		{
			ZLog.Log((object)("Missing shoulder item " + hash));
			return true;
		}
		m_shoulderItemInstances = AttachArmor(hash, variant);
		return true;
	}

	private bool SetLegEquipped(int hash)
	{
		if (m_currentLegItemHash == hash)
		{
			return false;
		}
		m_currentLegItemHash = hash;
		if ((Object)(object)m_bodyModel == (Object)null)
		{
			return true;
		}
		if (m_legItemInstances != null)
		{
			foreach (GameObject legItemInstance in m_legItemInstances)
			{
				Object.Destroy((Object)(object)legItemInstance);
			}
			m_legItemInstances = null;
			((Renderer)m_bodyModel).material.SetTexture("_LegsTex", m_emptyLegsTexture);
			((Renderer)m_bodyModel).material.SetTexture("_LegsBumpMap", (Texture)null);
			((Renderer)m_bodyModel).material.SetTexture("_LegsMetal", (Texture)null);
		}
		if (m_currentLegItemHash == 0)
		{
			return true;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);
		if ((Object)(object)itemPrefab == (Object)null)
		{
			ZLog.Log((object)("Missing legs item " + hash));
			return true;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		if (Object.op_Implicit((Object)(object)component.m_itemData.m_shared.m_armorMaterial))
		{
			((Renderer)m_bodyModel).material.SetTexture("_LegsTex", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_LegsTex"));
			((Renderer)m_bodyModel).material.SetTexture("_LegsBumpMap", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_LegsBumpMap"));
			((Renderer)m_bodyModel).material.SetTexture("_LegsMetal", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_LegsMetal"));
		}
		m_legItemInstances = AttachArmor(hash);
		return true;
	}

	private bool SetBeardEquipped(int hash)
	{
		if (m_currentBeardItemHash == hash)
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)m_beardItemInstance))
		{
			Object.Destroy((Object)(object)m_beardItemInstance);
			m_beardItemInstance = null;
		}
		m_currentBeardItemHash = hash;
		if (hash != 0)
		{
			m_beardItemInstance = AttachItem(hash, 0, m_helmet);
		}
		return true;
	}

	private bool SetHairEquipped(int hash)
	{
		if (m_currentHairItemHash == hash)
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)m_hairItemInstance))
		{
			Object.Destroy((Object)(object)m_hairItemInstance);
			m_hairItemInstance = null;
		}
		m_currentHairItemHash = hash;
		if (hash != 0)
		{
			m_hairItemInstance = AttachItem(hash, 0, m_helmet);
		}
		return true;
	}

	private bool SetHelmetEquipped(int hash, int hairHash)
	{
		if (m_currentHelmetItemHash == hash)
		{
			return false;
		}
		if (Object.op_Implicit((Object)(object)m_helmetItemInstance))
		{
			Object.Destroy((Object)(object)m_helmetItemInstance);
			m_helmetItemInstance = null;
		}
		m_currentHelmetItemHash = hash;
		HelmetHides(hash, out m_helmetHideHair, out m_helmetHideBeard);
		if (hash != 0)
		{
			m_helmetItemInstance = AttachItem(hash, 0, m_helmet);
		}
		return true;
	}

	private bool SetUtilityEquipped(int hash)
	{
		if (m_currentUtilityItemHash == hash)
		{
			return false;
		}
		if (m_utilityItemInstances != null)
		{
			foreach (GameObject utilityItemInstance in m_utilityItemInstances)
			{
				if (Object.op_Implicit((Object)(object)m_lodGroup))
				{
					Utils.RemoveFromLodgroup(m_lodGroup, utilityItemInstance);
				}
				Object.Destroy((Object)(object)utilityItemInstance);
			}
			m_utilityItemInstances = null;
		}
		m_currentUtilityItemHash = hash;
		if (hash != 0)
		{
			m_utilityItemInstances = AttachArmor(hash);
		}
		return true;
	}

	private bool SetTrinketEquipped(int hash)
	{
		if (m_currentTrinketItemHash == hash)
		{
			return false;
		}
		if (m_trinketItemInstances != null)
		{
			foreach (GameObject trinketItemInstance in m_trinketItemInstances)
			{
				if (Object.op_Implicit((Object)(object)m_lodGroup))
				{
					Utils.RemoveFromLodgroup(m_lodGroup, trinketItemInstance);
				}
				Object.Destroy((Object)(object)trinketItemInstance);
			}
			m_trinketItemInstances = null;
		}
		m_currentTrinketItemHash = hash;
		if (hash != 0)
		{
			m_trinketItemInstances = AttachArmor(hash);
		}
		return true;
	}

	private static void HelmetHides(int itemHash, out ItemDrop.ItemData.HelmetHairType hideHair, out ItemDrop.ItemData.HelmetHairType hideBeard)
	{
		hideHair = ItemDrop.ItemData.HelmetHairType.Default;
		hideBeard = ItemDrop.ItemData.HelmetHairType.Default;
		if (itemHash != 0)
		{
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
			if (!((Object)(object)itemPrefab == (Object)null))
			{
				ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
				hideHair = component.m_itemData.m_shared.m_helmetHideHair;
				hideBeard = component.m_itemData.m_shared.m_helmetHideBeard;
			}
		}
	}

	private List<GameObject> AttachArmor(int itemHash, int variant = -1)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
		if ((Object)(object)itemPrefab == (Object)null)
		{
			ZLog.Log((object)("Missing attach item: " + itemHash + "  ob:" + ((Object)((Component)this).gameObject).name));
			return null;
		}
		List<GameObject> list = new List<GameObject>();
		int childCount = itemPrefab.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = itemPrefab.transform.GetChild(i);
			if (!Utils.CustomStartsWith(((Object)((Component)child).gameObject).name, "attach_"))
			{
				continue;
			}
			string text = ((Object)((Component)child).gameObject).name.Substring(7);
			GameObject val;
			if (text == "skin")
			{
				val = Object.Instantiate<GameObject>(((Component)child).gameObject, ((Component)m_bodyModel).transform.position, ((Component)m_bodyModel).transform.parent.rotation, ((Component)m_bodyModel).transform.parent);
				val.SetActive(true);
				SkinnedMeshRenderer[] componentsInChildren = val.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (SkinnedMeshRenderer obj in componentsInChildren)
				{
					obj.rootBone = m_bodyModel.rootBone;
					obj.bones = m_bodyModel.bones;
				}
				Cloth[] componentsInChildren2 = val.GetComponentsInChildren<Cloth>();
				foreach (Cloth val2 in componentsInChildren2)
				{
					if (m_clothColliders.Length != 0)
					{
						if (val2.capsuleColliders.Length != 0)
						{
							List<CapsuleCollider> list2 = new List<CapsuleCollider>(m_clothColliders);
							list2.AddRange(val2.capsuleColliders);
							val2.capsuleColliders = list2.ToArray();
						}
						else
						{
							val2.capsuleColliders = m_clothColliders;
						}
					}
				}
			}
			else
			{
				Transform val3 = Utils.FindChild(m_visual.transform, text, (IterativeSearchType)0);
				if ((Object)(object)val3 == (Object)null)
				{
					ZLog.LogWarning((object)("Missing joint " + text + " in item " + ((Object)itemPrefab).name));
					continue;
				}
				val = Object.Instantiate<GameObject>(((Component)child).gameObject);
				val.SetActive(true);
				val.transform.SetParent(val3);
				val.transform.localPosition = Vector3.zero;
				val.transform.localRotation = Quaternion.identity;
			}
			if (variant >= 0)
			{
				val.GetComponentInChildren<IEquipmentVisual>()?.Setup(variant);
			}
			CleanupInstance(val);
			EnableEquippedEffects(val);
			list.Add(val);
		}
		return list;
	}

	private GameObject AttachItem(int itemHash, int variant, Transform joint, bool enableEquipEffects = true, bool backAttach = false)
	{
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
		if ((Object)(object)itemPrefab == (Object)null)
		{
			ZLog.Log((object)("Missing attach item: " + itemHash + "  ob:" + ((Object)((Component)this).gameObject).name + "  joint:" + (Object.op_Implicit((Object)(object)joint) ? ((Object)joint).name : "none")));
			return null;
		}
		GameObject val = null;
		Transform val2 = itemPrefab.transform.Find("equipoffset");
		int childCount = itemPrefab.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = itemPrefab.transform.GetChild(i);
			if (backAttach && ((Object)((Component)child).gameObject).name == "attach_back")
			{
				val = ((Component)child).gameObject;
				break;
			}
			if (((Object)((Component)child).gameObject).name == "attach" || (!backAttach && ((Object)((Component)child).gameObject).name == "attach_skin"))
			{
				val = ((Component)child).gameObject;
				break;
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		GameObject val3 = Object.Instantiate<GameObject>(val);
		val3.SetActive(true);
		CleanupInstance(val3);
		if (enableEquipEffects)
		{
			EnableEquippedEffects(val3);
		}
		if (((Object)val).name == "attach_skin")
		{
			val3.transform.SetParent(((Component)m_bodyModel).transform.parent);
			val3.transform.localPosition = Vector3.zero;
			val3.transform.localRotation = Quaternion.identity;
			SkinnedMeshRenderer[] componentsInChildren = val3.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer obj in componentsInChildren)
			{
				obj.rootBone = m_bodyModel.rootBone;
				obj.bones = m_bodyModel.bones;
			}
		}
		else
		{
			val3.transform.SetParent(joint);
			val3.transform.localPosition = Vector3.zero;
			val3.transform.localRotation = Quaternion.identity;
		}
		if ((Object)(object)val2 != (Object)null)
		{
			Transform transform = val3.transform;
			transform.localPosition += val2.position;
			Transform transform2 = val3.transform;
			transform2.localRotation *= val2.rotation;
		}
		val3.GetComponentInChildren<IEquipmentVisual>()?.Setup(variant);
		return val3;
	}

	private static void CleanupInstance(GameObject instance)
	{
		Collider[] componentsInChildren = instance.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	private static void EnableEquippedEffects(GameObject instance)
	{
		Transform val = instance.transform.Find("equiped");
		if (Object.op_Implicit((Object)(object)val))
		{
			((Component)val).gameObject.SetActive(true);
		}
	}

	public int GetModelIndex()
	{
		int result = m_modelIndex;
		if (m_nview.IsValid())
		{
			result = m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex);
		}
		return result;
	}
}
