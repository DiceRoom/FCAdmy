using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// プリセット
	/// </summary>
    public partial class SaveManager : Singleton<SaveManager>
	{
		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SaveManager()
		{
			mTimer.Tick += _AutoSave;
			mTimer.Interval = new TimeSpan(0, 0, 0, 0 , 800);
		}

        /// <summary>
        /// プロジェクトの保存
        /// </summary>
        public void SaveProject()
        {
			mTimer.Start();
			mSaveWindow = new WaitSaveWindow();
			mSaveWindow.ShowDialog();
        }

		/// <summary>
		/// オートセーブ
		/// </summary>
		public void AutoSaveProject()
		{
			if (SystemManager.GetInstance().GetSystemInfo().mAutoSaveFlg)
			{
				SaveProject();
			}
		}

        /// <summary>
        /// コンテンツデータの保存
        /// </summary>
        public void SaveContentData()
        {
            DataManager.GetInstance().WriteDataNoValidKeys(new string[] { mMemberInfoKey, mBattleInfoKey, mPresetInfoKey, mSystemInfoKey }, mContentDataName);
        }

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! オートセーブの実行
		private void _AutoSave(object iSender, EventArgs iArgs)
		{
			mTimer.Stop();

			try
			{
				DataManager.GetInstance().SetData(mMemberInfoKey, MemberManager.GetInstance().GetMemberList().ToArray());
				DataManager.GetInstance().SetData(mBattleInfoKey, BattleManager.GetInstance().GetBattleList().ToArray());
                DataManager.GetInstance().SetData(mSystemInfoKey, SystemManager.GetInstance().GetSystemInfo());
				DataManager.GetInstance().WriteData(new string[] { mMemberInfoKey, mBattleInfoKey, mPresetInfoKey, mSystemInfoKey }, mProjectFilePath);
			}
			catch (System.Exception){}

			mSaveWindow.Close();
			mSaveWindow = null;
		}

        /************************************************************************/
        /* アクセサ                                                             */
        /************************************************************************/

        /// <summary>
        /// プロジェクトファイルアクセサ
        /// </summary>
        public string ProjectFilePath { get { return mProjectFilePath; } set { mProjectFilePath = value; } }

        /// <summary>
        /// コンテントファイルアクセサ
        /// </summary>
        public string ContentFilePath { get { return mContentDataName; } set { mContentDataName = value; } }

        /// <summary>
        /// メンバー情報キーのアクセサ
        /// </summary>
        public string MemberInfoKey { get { return mMemberInfoKey; } }

        /// <summary>
        /// 大会情報キーのアクセサ
        /// </summary>
        public string BattleInfoKey { get { return mBattleInfoKey; } }

        /// <summary>
        /// プリセット情報キーのアクセサ
        /// </summary>
        public string PresetInfoKey { get { return mPresetInfoKey; } }
        
        /// <summary>
        /// システム情報キーのアクセサ
        /// </summary>
        public string SystemInfoKey { get { return mSystemInfoKey; } }

        /************************************************************************/
        /* 変数定義                                                             */
        /************************************************************************/

        /// <summary>
        /// プロジェクトファイル名
        /// </summary>
		private string mProjectFilePath;

        /// <summary>
        /// コンテンツファイル名
        /// </summary>
		private string mContentDataName = SystemUtility.GetSystemPath() + "FCAdmy.ctb";

		/// <summary>
		/// セーブウィンドウ
		/// </summary>
		private WaitSaveWindow mSaveWindow;

		/// <summary>
		/// セーブタイマー
		/// </summary>
		private System.Windows.Threading.DispatcherTimer mTimer = new System.Windows.Threading.DispatcherTimer();

        /// <summary>
        /// メンバー情報キー
        /// </summary>
        private string mMemberInfoKey = "__MEMBER_KEY__";

        /// <summary>
        /// 大会情報キー
        /// </summary>
        private string mBattleInfoKey = "__BATTLE_KEY__";

        /// <summary>
        /// プリセット情報キー
        /// </summary>
        private string mPresetInfoKey = "__PRESET_KEY__";

        /// <summary>
        /// システム情報キー
        /// </summary>
        private string mSystemInfoKey = "__SYSTEM_KEY__";
	}
}
