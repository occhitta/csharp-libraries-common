using System;
using System.Collections.ObjectModel;

namespace Occhitta.Libraries.Common;

/// <summary>
/// 設定例外情報クラスです。
/// /// </summary>
public class SettingException : Exception {
	/// <summary>
	/// 項目一覧を取得します。
	/// </summary>
	public ReadOnlyCollection<(string, object?)> ElementList {
		get;
	}
	/// <summary>
	/// 設定例外情報を生成します。
	/// </summary>
	/// <param name="reason">例外内容</param>
	public SettingException(string reason) : base(reason) {
		ElementList = new([]);
	}
	/// <summary>
	/// 設定例外情報を生成します。
	/// </summary>
	/// <param name="reason">例外内容</param>
	/// <param name="values">設定配列</param>
	public SettingException(string reason, params (string, object?)[] values) : this($"{reason}{ToText(values)}") {
		ElementList = new(values);
	}
	/// <summary>
	/// 設定例外情報を生成します。
	/// </summary>
	/// <param name="reason">例外内容</param>
	/// <param name="source">例外情報</param>
	public SettingException(string reason, Exception source) : base(reason, source) {
		ElementList = new([]);
	}

	/// <summary>
	/// 設定配列を表現文字列へ変換します。
	/// </summary>
	/// <param name="source">設定配列</param>
	/// <returns>表現文字列</returns>
	private static string ToText((string, object?)[] source) {
		var result = new System.Text.StringBuilder();
		foreach (var choose in source) {
			result.Append(Environment.NewLine);
			result.Append(choose.Item1);
			result.Append(':');
			result.Append(choose.Item2);
		}
		return result.ToString();
	}
}
