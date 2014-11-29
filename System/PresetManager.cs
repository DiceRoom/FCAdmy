using System.Collections.Generic;
using System.Windows.Media;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// プリセット
	/// </summary>
	public partial class PresetManager : Singleton<PresetManager>
	{
		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		/// <summary>
		/// 設定されているプリセット名からプリセットデータをロードする
		/// </summary>
		public void LoadPreset()
		{
			try
			{
				string preset_Name = (string)DataManager.GetInstance().GetData(SaveManager.GetInstance().PresetInfoKey);
				string preset_Path = SystemUtility.GetResourcePath() + @"Preset\" + preset_Name + @"\";
				var analayzer = new DL.CXmlAnalyzer();
				analayzer.AnalyzeXmlFile(preset_Path + "Preset.psx");

				//背景画像の取得
				var node = analayzer.GetRootNode().GetChildNodeFromPath("Preset/BackGroundImage")[0];
				mBackgroundImage = ResourceManager.GetInstance().CreateImage(preset_Path + node.GetNodeInfo().mValue + ".adg");

				//キャラクター情報の読み込み
				node = analayzer.GetRootNode().GetChildNodeFromPath("Preset/Characters")[0];
				foreach (var iNode in node.GetChildNode("Character"))
				{
					var info = new SCharacterInfo();
					info.mIndex = int.Parse(iNode.GetChildNode("Index")[0].GetNodeInfo().mValue);
					info.mName = iNode.GetChildNode("Name")[0].GetNodeInfo().mValue;
					info.mIconColorList = new Color[2] { SystemUtility.StringToColor(iNode.GetChildNode("UpBGColor")[0].GetNodeInfo().mValue), SystemUtility.StringToColor(iNode.GetChildNode("DownBGColor")[0].GetNodeInfo().mValue) };
					info.mNameColorList = new Color[2] { SystemUtility.StringToColor(iNode.GetChildNode("FontColor")[0].GetNodeInfo().mValue), SystemUtility.StringToColor(iNode.GetChildNode("ShadowFontColor")[0].GetNodeInfo().mValue) };
					info.mIconImage = ResourceManager.GetInstance().CreateImage(preset_Path + @"\" + string.Format("CIC{0}.adg", info.mIndex));
					
					mCharacterMap.Add(info.mIndex, info);
				}

                mUnknownCharacterIcon = ResourceManager.GetInstance().CreateImage(SystemUtility.GetResourcePath() + @"System\CIC_UKN.adg");
			}
			catch (System.Exception iException)
			{
				SystemUtility.DisplayErrorDialog(string.Format("プリセットソースの読み込みに失敗しました\n[{0}]", iException.Message));
				System.Environment.Exit(1);
			}
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// キャラクター情報の取得
		/// </summary>
		/// <param name="iCharacterIndex">キャラクター番号</param>
		/// <returns>キャラクター情報</returns>
		public SCharacterInfo GetCharacterInfo(int iCharacterIndex)
		{
			if (!mCharacterMap.ContainsKey(iCharacterIndex))
			{
				throw new System.Exception("Not Found Character Index");
			}
			return mCharacterMap[iCharacterIndex];
		}

		/// <summary>
		/// キャラクター情報リストの取得
		/// </summary>
		/// <returns>情報リスト</returns>
		public List<SCharacterInfo> GetCharacterList()
		{
			return new List<SCharacterInfo>(mCharacterMap.Values);
		}

		/// <summary>
		/// キャラクター名からキャラクター番号を取得する
		/// </summary>
		/// <param name="iCharacterName"></param>
		/// <returns></returns>
		public int GetCharacterIndex(string iCharacterName)
		{
			foreach (var iPair in mCharacterMap)
			{
				if (iPair.Value.mName == iCharacterName)
				{
					return iPair.Key;
				}
			}
			return -1;
		}

        /// <summary>
        /// キャラクター未選択時表示するアイコンの取得
        /// </summary>
        /// <returns>アイコンイメージ</returns>
        public ImageSource GetUnknownCharacterIcon()
        {
            return mUnknownCharacterIcon;
        }

		/// <summary>
		/// 背景イメージの取得
		/// </summary>
		/// <returns>背景イメージ</returns>
		public ImageSource GetBackGroundImage()
		{
			return mBackgroundImage;
		}

		/************************************************************************/
		/* 変数宣言                                                             */
		/************************************************************************/

		/// <summary>
		/// キャラクターリスト
		/// </summary>
		private Dictionary<int, SCharacterInfo> mCharacterMap = new Dictionary<int, SCharacterInfo>();

        /// <summary>
        /// キャラクター未選択の時に使用するアイコン
        /// </summary>
        private ImageSource mUnknownCharacterIcon;

		/// <summary>
		/// 背景イメージ
		/// </summary>
		private ImageSource mBackgroundImage;
	}
}
