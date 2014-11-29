using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// プリセットマネージャ
	/// </summary>
	public partial class PresetManager
	{
		/// <summary>
		/// キャラクター情報
		/// </summary>
		public struct SCharacterInfo
		{
			/// <summary>
			/// キャラクター番号
			/// </summary>
			public int mIndex;

			/// <summary>
			/// キャラクター名
			/// </summary>
			public string mName;

			/// <summary>
			/// アイコンイメージ
			/// </summary>
			public ImageSource mIconImage;

			/// <summary>
			/// アイコンカラーリスト(グラデーションになる -> 0:上の色 1:下の色)
			/// </summary>
			public Color[] mIconColorList;

			/// <summary>
			/// 名前表記カラーリスト(0:本体色 1:影の色)
			/// </summary>
			public Color[] mNameColorList;
		};
	}
}
