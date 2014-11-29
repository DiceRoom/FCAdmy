using System;
using System.Collections.Generic;
using System.Linq;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// シングルトン型のデータマネージャ
	/// </summary>
	public partial class MemberManager : Singleton<MemberManager>
	{
		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

        /// <summary>
        /// メンバーデータの読み込み
        /// </summary>
        public void LoadMemberData()
        {
            var get_object = DataManager.GetInstance().GetData(SaveManager.GetInstance().MemberInfoKey);
            if (get_object != null)
            {
                mMemberList = new List<SMemberInfo>((SMemberInfo[])get_object);
            }
        }

		/// <summary>
		/// メンバーの追加
		/// </summary>
		/// <param name="iMember">メンバー情報</param>
		public void AddMember(SMemberInfo iMember)
		{
			//メンバーIDは自動付与する
            int id = -1;
            foreach (var iInfo in mMemberList)
            {
                if (id < iInfo.mID) { id = iInfo.mID; }
            }
            iMember.mID = id + 1;
			iMember.mResistDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
			mMemberList.Add(iMember);
		}

		/// <summary>
		/// メンバーリストの取得
		/// </summary>
		/// <returns>メンバーリスト</returns>
		public List<SMemberInfo> GetMemberList(ESortKind iSortKind = ESortKind.SORT_ASC_ID)
		{
			var ret_List = new List<SMemberInfo>(mMemberList.ToArray());
			if (iSortKind != ESortKind.SORT_ASC_ID)
			{
				switch (iSortKind)
				{
				case ESortKind.SORT_DESC_ID:
					ret_List.Sort(new MemberIDDescSorter());
					break;
				case ESortKind.SORT_ASC_NAME:
					ret_List.Sort(new MemberNameSorter());
					break;
				case ESortKind.SORT_DESC_NAME:
					ret_List.Sort(new MemberNameDescSorter());
					break;
				case ESortKind.SORT_ASC_CHARA_ID:
					ret_List.Sort(new MemberCharacterIDSorter());
					break;
				case ESortKind.SORT_DESC_CHARA_ID:
					ret_List.Sort(new MemberCharacterIDDescSorter());
					break;
				};
			}
			return ret_List;
		}

		/// <summary>
		/// 指定されたIDのメンバーのリストを取得
		/// </summary>
		/// <param name="iIDList">IDリスト</param>
		/// <returns>メンバーリスト</returns>
		public List<SMemberInfo> GetMemberList(List<int> iIDList)
		{
			var ret_List = new List<SMemberInfo>();

			foreach (int iID in iIDList)
			{
				foreach (var iMemberInfo in mMemberList)
				{
					if (iID == iMemberInfo.mID)
					{
						ret_List.Add(iMemberInfo);
						break;
					}
				}
			}

			return ret_List;
		}

		/// <summary>
		/// 指定されたIDを含まないメンバーのリストを取得
		/// </summary>
		/// <param name="iIDList">IDリスト</param>
		/// <returns>メンバーリスト</returns>
		public List<SMemberInfo> GetNoMemberList(List<int> iIDList)
		{
			var ret_List = new List<SMemberInfo>();

			foreach (var iMemberInfo in mMemberList)
			{
				bool add_Flg = true;
				foreach (int iID in iIDList)
				{
					if (iMemberInfo.mID == iID)
					{
						add_Flg = false;
						break;
					}
				}

				if (add_Flg)
				{
					ret_List.Add(iMemberInfo);
				}
			}

			return ret_List;
		}

		/// <summary>
		/// メンバー情報の取得
		/// </summary>
		/// <param name="iID">メンバーID</param>
		/// <returns>メンバー情報</returns>
		public SMemberInfo GetMemberInfo(int iID)
		{
			foreach (var iMemberInfo in mMemberList)
			{
				if (iMemberInfo.mID == iID)
				{
					return iMemberInfo;
				}
			}

            throw new System.Exception();
		}

		/// <summary>
		/// メンバーの情報の上書きを行う
		/// </summary>
		/// <param name="iID">メンバーID</param>
		/// <param name="iMemberInfo">情報</param>
		public void SetMemberInfo(int iID, SMemberInfo iMemberInfo)
		{
			foreach (int iIndex in System.Linq.Enumerable.Range(0, mMemberList.Count))
			{
				if (mMemberList[iIndex].mID == iID)
				{
					iMemberInfo.mID = iID;
					mMemberList[iIndex] = iMemberInfo;
					return;
				}
			}
		}

		/************************************************************************/
		/* 変数宣言                                                             */
		/************************************************************************/

		/// <summary>
		/// メンバーリスト
		/// </summary>
		private List<SMemberInfo> mMemberList = new List<SMemberInfo>();
	}
}
