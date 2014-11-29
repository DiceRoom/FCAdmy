using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;


namespace FightingCommunityAdministrator
{
	/// <summary>
	/// システムユーティリティ
	/// </summary>
	public partial class SystemUtility
	{
		/************************************************************************/
		/* パス取得系                                                           */
		/************************************************************************/
        
		/// <summary>
		/// ルートパスの取得
		/// </summary>
		/// <returns>ルートパス</returns>
		public static string GetRootPath()
		{
#if DL_DEBUG
			return @"C:\ＦＣあどみぃ！\ＦＣあどみぃ！\";
#else
            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return path.Substring(0, path.LastIndexOf(@"\") + 1);
#endif
        }

		/// <summary>
		/// システムパスのヘッダ取得
		/// </summary>
		/// <returns>システムパスヘッダ</returns>
        public static string GetSystemPath()
		{
			return (GetRootPath() + @"System\");
		}

		/// <summary>
		/// リソースパスのヘッダ取得
		/// </summary>
		/// <returns>リソースパスヘッダ</returns>
        public static string GetResourcePath()
		{
			return (GetRootPath() + @"Resource\");
		}

        /************************************************************************/
        /* シリアライズ系                                                       */
        /************************************************************************/

		/// <summary>
		/// シリアライズデータの書き出し
		/// </summary>
		/// <param name="iFilePath">ファイルパス</param>
		/// <param name="iObject">書き出しオブジェクト</param>
        public static void WriteSerializationData(string iFilePath, object iObject)
		{
			//シリアル化して書き出す
			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			using (System.IO.FileStream fs = new System.IO.FileStream(iFilePath, System.IO.FileMode.Create))
			{
				bf.Serialize(fs, iObject);
			}
		}

		/// <summary>
		/// シリアライズデータの読み込み
		/// </summary>
		/// <param name="iFilaPath">ファイルパス</param>
		/// <returns>読み込みオブジェクト</returns>
        public static object ReadSerializationData(string iFilaPath)
		{
			//シリアル化されているバイナリファイルの読み込み
			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			using (System.IO.FileStream fs = new System.IO.FileStream(iFilaPath, System.IO.FileMode.Open))
			{
				return bf.Deserialize(fs);
			}
		}

        /************************************************************************/
        /* その他便利系                                                         */
        /************************************************************************/

		/// <summary>
		/// エラーメッセージダイアログを表示する
		/// </summary>
		/// <param name="iMessage">メッセージ</param>
        public static void DisplayErrorDialog(string iMessage)
		{
			System.Media.SystemSounds.Hand.Play();
			MessageBox.Show(iMessage, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
		}

        /// <summary>
        /// 文字列からカラーを取得する
        /// </summary>
        /// <param name="iColorName">#AARRGGBBの形で記述</param>
        /// <returns>カラー</returns>
        public static Color StringToColor(string iColorName)
        {
            string color_Str = iColorName.Substring(1);
            var color_list = new int[4];
            foreach (int iIndex in System.Linq.Enumerable.Range(0, 4))
            {
                color_list[iIndex] = Convert.ToInt32(color_Str.Substring(0, 2), 16);
                color_Str = color_Str.Substring(2);
            }

            var color = new Color();
            color.A = (byte)color_list[0];
            color.R = (byte)color_list[1];
            color.G = (byte)color_list[2];
            color.B = (byte)color_list[3];
            return color;
        }

		/// <summary>
		/// 簡易グラデーションブラシの作成
		/// </summary>
		/// <param name="iUpColor">上部カラー</param>
		/// <param name="iDownColor">下部カラー</param>
		/// <returns></returns>
		public static LinearGradientBrush CreateLinearGradientBrash(Color iUpColor, Color iDownColor)
		{
			LinearGradientBrush brush = new LinearGradientBrush();
			brush.StartPoint = new Point(0, 0);
			brush.EndPoint = new Point(0, 1);
			brush.GradientStops.Add(new GradientStop(iUpColor, 0.0));
			brush.GradientStops.Add(new GradientStop(iDownColor, 1.0));
			return brush;
		}

		/// <summary>
		/// ユーザーコントロールのシーン名を取得
		/// </summary>
		/// <param name="iScene">シーン</param>
		/// <returns>シーン名</returns>
		public static string GetSceneName(UserControl iScene)
		{
			var scene_Name = iScene.ToString();
			while (true)
			{
				var pos = scene_Name.IndexOf(".");
				if (pos == -1)
				{
					break;
				}
				scene_Name = scene_Name.Substring(pos + 1);
			}

			//最後の5文字が「～Scene」でない場合はエラー
			if (scene_Name.Length < 7 || scene_Name.Substring(scene_Name.Length - 5) != "Scene")
			{
				throw new System.Exception("Not Scene Class");
			}

			return scene_Name;
		}

		/// <summary>
		/// コントロールのキャプチャBMPを取得する
		/// </summary>
		/// <param name="iControl">コントロール</param>
		/// <returns>ビットマップ</returns>
		public static RenderTargetBitmap GetCaptureControlBitmap(FrameworkElement iControl)
		{
			//親がスクロールバーの場合はスクロール位置をトップにしてやる(出ないと全部出ない)
			var scroll_Check = iControl.Parent as ScrollViewer;
			var scroll_Offset = new Point();
			FlowDirection direction = FlowDirection.LeftToRight;
			int width = (int)iControl.ActualWidth;
			int height = (int)iControl.ActualHeight;
			if (scroll_Check != null)
			{
				scroll_Offset.X = scroll_Check.VerticalOffset;
				scroll_Offset.Y = scroll_Check.HorizontalOffset;
				scroll_Check.ScrollToTop();
				scroll_Check.ScrollToLeftEnd();
				direction = scroll_Check.FlowDirection;
				scroll_Check.FlowDirection = FlowDirection.LeftToRight;
				scroll_Check.UpdateLayout();

				//必要なサイズにする
				if (width < scroll_Check.ExtentWidth)
				{
					width = (int)scroll_Check.ExtentWidth;
				}
				if (height < scroll_Check.ExtentHeight)
				{
					height = (int)scroll_Check.ExtentHeight;
				}
			}


			var ret_Bmp = new RenderTargetBitmap(width,
												 height,
												 96,
												 96,
												 System.Windows.Media.PixelFormats.Pbgra32);
			ret_Bmp.Render(iControl);

			//スクロール位置を戻す
			if (scroll_Check != null)
			{
				scroll_Check.FlowDirection = direction;
				scroll_Check.ScrollToVerticalOffset(scroll_Offset.X);
				scroll_Check.ScrollToHorizontalOffset(scroll_Offset.Y);
				scroll_Check.UpdateLayout();
			}

			return ret_Bmp;
		}

        /// <summary>
        /// コントロールのキャプチャをする
        /// </summary>
        /// <param name="iFilePath">出力ファイルパス</param>
        /// <param name="iControl">コントロール</param>
        public static void OutputCaptureControl(string iFilePath, FrameworkElement iControl)
        {
            try
            {
				OutputImageFile(iFilePath, GetCaptureControlBitmap(iControl));       
            }
            catch (System.Exception iException)
            {
                DisplayErrorDialog(iException.Message);
                throw iException;
            }
        }

		/// <summary>
		/// 指定したビットマップを出力する
		/// </summary>
		/// <param name="iFilePath">ファイルパス</param>
		/// <param name="iBitmap">イメージ</param>
		public static void OutputImageFile(string iFilePath, RenderTargetBitmap iBitmap)
		{
			var enc = new PngBitmapEncoder();
			enc.Frames.Add(BitmapFrame.Create(iBitmap));
			using (System.IO.FileStream stream = System.IO.File.Open(iFilePath, System.IO.FileMode.Create))
			{
				enc.Save(stream);
			}   
		}

        /// <summary>
        /// ライセンスファイルの一括作成
        /// </summary>
        public static void CreateLicenseFiles(double iLimitVersion)
        {
            //ライセンス所有可能者リスト作成
            var path = @"C:\ＦＣあどみぃ！\FightingCommunityAdministrator\Project\License\";
            System.IO.StreamReader reader = (new System.IO.StreamReader(path + @"License.txt", System.Text.Encoding.Default));
            var user_Name_List = new List<string>();
            while (reader.Peek() >= 0)
            {
                var buf = reader.ReadLine().Trim();
                if (buf.Length > 0)
                {
                    user_Name_List.Add(buf);
                }
            }
            reader.Close();

            //書き出し
            var info = new LicenseInfo();
            info.mLicenceData = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
			info.mLimitVersion = iLimitVersion;
            foreach (var iUserName in user_Name_List)
            {
                info.mUserName = iUserName;

                if (!System.IO.Directory.Exists(path + iUserName))
                {
                    System.IO.Directory.CreateDirectory(path + iUserName);
                }
                WriteSerializationData(path + iUserName + @"\FCAdmy.lic", info);
            }
        }

        /// <summary>
        /// ライセンスファイルからユーザー名の取得
        /// </summary>
        /// <param name="iLicenseFilePath">ライセンスファイルパス</param>
        /// <returns>ユーザー名</returns>
        public static string GetLicenseUserName(string iLicenseFilePath)
        {
            try
            {
                var license = (LicenseInfo)ReadSerializationData(iLicenseFilePath);

				//バージョンチェック
				if(license.mLimitVersion <= Version.CurrentVersion)
				{
					throw new System.Exception("Limit Version Over");
				}

                return license.mUserName;
            }
            catch (System.Exception iException)
            {
                DisplayErrorDialog("このバージョンのツールは使用出来ません");
                throw iException;
            }
        }

        /************************************************************************/
        /* 内部定義                                                             */
        /************************************************************************/

        /// <summary>
        /// ライセンス書き出し構造体
        /// </summary>
        [Serializable]
        struct LicenseInfo
        {
            /// <summary>
            /// 使用者名
            /// </summary>
            public string mUserName;

            /// <summary>
            /// 作成日
            /// </summary>
            public string mLicenceData;

			/// <summary>
			/// 使用可能な最大バージョン
			/// </summary>
			public double mLimitVersion;
        };
	}
}
