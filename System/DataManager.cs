using System;
using System.Collections.Generic;
using System.Linq;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// データ管理マネージャ
	/// </summary>
	public partial class DataManager : Singleton<DataManager>
	{

        /************************************************************************/
        /* アクセサ                                                             */
        /************************************************************************/

        /// <summary>
        /// 指定キーのオブジェクトを取得
        /// </summary>
        /// <param name="iKey">キー名</param>
        /// <returns>データオブジェクト</returns>
        public object GetData(string iKey)
        {
            return (mDataMap.ContainsKey(iKey) ? mDataMap[iKey] : null);
        }

        /// <summary>
        /// オブジェクトの設定
        /// </summary>
        /// <param name="iKey">キー</param>
        /// <param name="iData">データ</param>
        public void SetData(string iKey, object iData)
        {
            mDataMap[iKey] = iData;
        }

        /************************************************************************/
        /* 入出力                                                               */
        /************************************************************************/

        /// <summary>
        /// データの出力
        /// </summary>
        /// <param name="iFileName">ファイルパス</param>
        public void WriteData(string iKey, string iFilePath)
        {
            //オブジェクトなしの場合はエラー
            var write_Object = GetData(iKey);
            if (write_Object == null)
            {
                SystemUtility.DisplayErrorDialog("指定されたデータキーが存在しなかった為出力に失敗しました");
                return;
            }

            //書き出しデータ準備
            var write_Data = new SWriteData[1];
            write_Data[0].mKeyName = iKey;
            write_Data[0].mObject = write_Object;

            //出力
            try
            {
				SystemUtility.WriteSerializationData(iFilePath, write_Data);
            }
            catch (System.Exception iException)
            {
                SystemUtility.DisplayErrorDialog("データ出力に失敗しました\n\n" + iException.Message);
            }
        }

        /// <summary>
        /// 指定したキーリストに存在するデータを全て出力する
        /// </summary>
        /// <param name="iKeyList">キーリスト</param>
		/// <param name="iFileName">ファイルパス</param>
		public void WriteData(string[] iKeyList, string iFilePath)
        {
            var data_List = new List<SWriteData>();
            foreach (var iKey in iKeyList)
            {
                //オブジェクトなしの場合はエラー
                var write_Object = GetData(iKey);
                if (write_Object == null)
                {
                    SystemUtility.DisplayErrorDialog(string.Format("指定されたデータキー[{0}]が存在しなかった為出力に失敗しました", iKey));
                    return;
                }

                //設定
                var write_Data = new SWriteData();
                write_Data.mKeyName = iKey;
                write_Data.mObject = write_Object;
                data_List.Add(write_Data);
            }

            //出力
            try
            {
				SystemUtility.WriteSerializationData(iFilePath, data_List.ToArray());
            }
            catch (System.Exception iException)
            {
                SystemUtility.DisplayErrorDialog("データ出力に失敗しました\n\n" + iException.Message);
            }
        }

        /// <summary>
        /// 指定したキーリストに存在しないデータを全て出力する
        /// </summary>
        /// <param name="iKeyList">キーリスト</param>
		/// <param name="iFileName">ファイルパス</param>
        public void WriteDataNoValidKeys(string[] iKeyList, string iFilePath)
        {
            var data_List = new List<SWriteData>();
            foreach (var iData in mDataMap)
            {
                //キー内に存在しない場合のみ追加
                if (Array.IndexOf(iKeyList, iData.Key) == -1)
                {
                    var write_Data = new SWriteData();
                    write_Data.mKeyName = iData.Key;
                    write_Data.mObject = iData.Value;
                    data_List.Add(write_Data);
                }
            }

            //出力
            try
            {
				SystemUtility.WriteSerializationData(iFilePath, data_List.ToArray());
            }
            catch (System.Exception iException)
            {
                SystemUtility.DisplayErrorDialog("データ出力に失敗しました\n\n" + iException.Message);
            }
        }

        /// <summary>
        /// データの読み込み
        /// </summary>
        /// <param name="iFileName">ファイルパス</param>
        public void ReadData(string iFilePath)
        {
			var get_Data = (SWriteData[])SystemUtility.ReadSerializationData(iFilePath);
            foreach (var iData in get_Data)
            {
                mDataMap[iData.mKeyName] = iData.mObject;
            }
        }

        /************************************************************************/
        /* 内部定義                                                             */
        /************************************************************************/

        /// <summary>
        /// 書き出しデータ
        /// </summary>
        [Serializable]
        struct SWriteData
        {
            /// <summary>
            /// キー名
            /// </summary>
            public string mKeyName;

            /// <summary>
            /// 書き出しオブジェクト
            /// </summary>
            public object mObject;
        };

		/************************************************************************/
		/* 変数宣言                                                             */
		/************************************************************************/

        /// <summary>
        /// データマップ
        /// </summary>
        private Dictionary<string, object> mDataMap = new Dictionary<string, object>();
	}
}
