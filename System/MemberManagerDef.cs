using System;
using System.Collections.Generic;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// メンバーマネージャ
	/// </summary>
	public partial class MemberManager
	{
		/************************************************************************/
		/* 公開定義                                                             */
		/************************************************************************/

		/// <summary>
		/// メンバーの固定情報
		/// </summary>
		[Serializable]
		public struct SMemberInfo
		{
			/// <summary>
			/// ユーザーID番号
			/// </summary>
			public int mID;

			/// <summary>
			/// メンバー名
			/// </summary>
			public string mName;

			/// <summary>
			/// デフォルトで使用するキャラクターID
			/// </summary>
			public int mDefaultCharacterID;

			/// <summary>
			/// 登録日
			/// </summary>
			public string mResistDate;
		};

		/// <summary>
		/// メンバーリストを取得する時の並び順
		/// </summary>
		public enum ESortKind
		{
			SORT_ASC_ID,		//< メンバーIDを昇順で
			SORT_DESC_ID,		//< メンバーIDを降順で
			SORT_ASC_NAME,		//< メンバー名を昇順で
			SORT_DESC_NAME,		//< メンバー名を降順で
			SORT_ASC_CHARA_ID,	//< デフォルト選択キャラクター番号を昇順で
			SORT_DESC_CHARA_ID,	//< デフォルト選択キャラクター番号を降順で
		};

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		/// <summary>
		/// メンバー番号で並び替えるソートクラス(降順)
		/// </summary>
		private class MemberIDDescSorter : IComparer<SMemberInfo>
		{
			public int Compare(SMemberInfo iMember1, SMemberInfo iMember2)
			{
				return iMember2.mID.CompareTo(iMember1.mID);
			}
		}

		/// <summary>
		/// メンバー名で並び替えるソートクラス(昇順)
		/// </summary>
		private class MemberNameSorter : IComparer<SMemberInfo>
		{
			public int Compare(SMemberInfo iMember1, SMemberInfo iMember2)
			{
				return iMember1.mName.CompareTo(iMember2.mName);
			}
		}

		/// <summary>
		/// メンバー名で並び替えるソートクラス(降順)
		/// </summary>
		private class MemberNameDescSorter : IComparer<SMemberInfo>
		{
			public int Compare(SMemberInfo iMember1, SMemberInfo iMember2)
			{
				return iMember2.mName.CompareTo(iMember1.mName);
			}
		}

		/// <summary>
		/// デフォルトキャラクター番号で並び替えるソートクラス(昇順)
		/// </summary>
		private class MemberCharacterIDSorter : IComparer<SMemberInfo>
		{
			public int Compare(SMemberInfo iMember1, SMemberInfo iMember2)
			{
				return iMember1.mDefaultCharacterID.CompareTo(iMember2.mDefaultCharacterID);
			}
		}

		/// <summary>
		/// デフォルトキャラクター番号で並び替えるソートクラス(降順)
		/// </summary>
		private class MemberCharacterIDDescSorter : IComparer<SMemberInfo>
		{
			public int Compare(SMemberInfo iMember1, SMemberInfo iMember2)
			{
				return iMember2.mDefaultCharacterID.CompareTo(iMember1.mDefaultCharacterID);
			}
		}
	}
}
