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
	/// 全体で使うリソースのマネージャ
	/// </summary>
	public partial class ResourceManager : Singleton<ResourceManager>
	{
		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

        /// <summary>
        /// BitmapImageの作成
        /// </summary>
        /// <param name="iFilePath">ファイルパス</param>
        /// <returns>イメージ</returns>
        public BitmapImage CreateImage(string iFilePath)
        {
            //未ロードならロードする
            if (!mImageMap.ContainsKey(iFilePath))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(iFilePath);
                image.EndInit();
                mImageMap.Add(iFilePath, image);
            }

            return mImageMap[iFilePath];
        }

		/************************************************************************/
		/* 変数宣言                                                             */
		/************************************************************************/

        /// <summary>
        /// ロードしたイメージマップ
        /// </summary>
        private Dictionary<string, BitmapImage> mImageMap = new Dictionary<string, BitmapImage>();
	}
}
