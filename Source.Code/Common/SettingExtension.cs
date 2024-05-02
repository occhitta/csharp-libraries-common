using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Occhitta.Libraries.Common;

/// <summary>
/// 設定拡張関数クラスです。
/// </summary>
public static class SettingExtension {
	#region 内部メソッド定義(IsMatch)
	/// <summary>
	/// <paramref name="source" />が適合するか判定します。
	/// </summary>
	/// <param name="source">設定情報</param>
	/// <param name="values">判定情報</param>
	/// <returns><paramref name="source" />が<paramref name="values" />に適合した場合、<c>True</c>を返却</returns>
	private static bool IsMatch(string source, params string[] values) {
		foreach (var choose in values) {
			if (String.Equals(source, choose, StringComparison.OrdinalIgnoreCase)) {
				return true;
			}
		}
		return false;
	}
	#endregion 内部メソッド定義(IsMatch)

	#region 内部メソッド定義(ToError)
	/// <summary>
	/// 引数情報にて例外を生成します。
	/// <para>当該メソッドは<paramref name="settingName" />が存在しなかった場合に呼び出すメソッドとなります。</para>
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <returns>設定例外</returns>
	private static SettingException ToError(XmlNode elementData, string settingName) =>
		new("属性名が定義されていません。", ("対象ノード", ToRoute(elementData)), ("検索属性名", settingName));
	/// <summary>
	/// 引数情報にて例外を生成します。
	/// <para>当該メソッドは<paramref name="settingText" />が形式不正である場合に呼び出すメソッドとなります。</para>
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="patternName">形式名称</param>
	/// <param name="settingText">属性情報</param>
	/// <returns>設定例外</returns>
	private static SettingException ToError(XmlNode elementData, string settingName, string patternName, string settingText) =>
		new($"属性値が{patternName}ではありません。", ("対象ノード", ToRoute(elementData)), ("検索属性名", settingName), ("属性値情報", settingText));
	#endregion 内部メソッド定義(ToError)

	#region 公開メソッド定義(ToRoute)
	/// <summary>
	/// 経路内容へ変換します。
	/// </summary>
	/// <param name="source">要素情報</param>
	/// <returns>経路内容</returns>
	public static string ToRoute(this XmlNode source) {
		var parent = source.ParentNode;
		if (parent is XmlElement) {
			return $"{ToRoute(parent)}:{source.Name}";
		} else {
			return source.Name;
		}
	}
	#endregion 公開メソッド定義(ToRoute)

	#region 公開メソッド定義(GetList)
	/// <summary>
	/// 下位一覧を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <returns>下位一覧</returns>
	public static IEnumerable<XmlNode> GetList(this XmlNode elementData) {
		var elementList = elementData.ChildNodes;
		for (var index = 0; index < elementList.Count; index ++) {
			var choose = elementList[index];
			if (choose is XmlElement) {
				yield return choose;
			}
		}
	}
	/// <summary>
	/// 下位一覧を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <returns>下位一覧</returns>
	public static IEnumerable<XmlNode> GetList(this XmlNode elementData, string settingName) {
		foreach (var choose in GetList(elementData)) {
			if (choose.Name == settingName) {
				yield return choose;
			}
		}
	}
	/// <summary>
	/// 下位一覧を取得します。
	/// </summary>
	/// <typeparam name="TResult">変換種別</typeparam>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="convertHook">変換処理</param>
	/// <returns>下位一覧</returns>
	public static TResult[] GetList<TResult>(this XmlNode elementData, string settingName, Func<XmlNode, TResult> convertHook) {
		var result = new List<TResult>();
		foreach (var choose in GetList(elementData, settingName)) {
			result.Add(convertHook(choose));
		}
		return [.. result];
	}
	/// <summary>
	/// 下位一覧を取得します。
	/// </summary>
	/// <typeparam name="TSource">引数種別</typeparam>
	/// <typeparam name="TResult">変換種別</typeparam>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="convertHook">変換処理</param>
	/// <param name="includeData">引数情報</param>
	/// <returns>下位一覧</returns>
	public static TResult[] GetList<TSource, TResult>(this XmlNode elementData, string settingName, Func<XmlNode, TSource, TResult> convertHook, TSource includeData) {
		var result = new List<TResult>();
		foreach (var choose in GetList(elementData, settingName)) {
			result.Add(convertHook(choose, includeData));
		}
		return [.. result];
	}
	#endregion 公開メソッド定義(GetList)

	#region 公開メソッド定義(GetData)
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="settingData">設定情報</param>
	/// <returns><paramref name="settingName" />が定義されている場合、<c>True</c>を返却</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />が複数存在している場合</exception>
	public static bool GetData(this XmlNode elementData, string settingName, [MaybeNullWhen(false)]out XmlNode settingData) {
		settingData = null;
		foreach (var choose in GetList(elementData, settingName)) {
			if (settingData == null) {
				settingData = choose;
			} else {
				throw new SettingException("単一ノードの想定ですが複数ノードが指定されています。", ("対象ノード", ToRoute(elementData)), ("検索ノード", settingName));
			}
		}
		return settingData != null;
	}
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="defaultData">既定情報</param>
	/// <returns>設定情報</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />が複数存在している場合</exception>
	[return: NotNullIfNotNull(nameof(defaultData))]
	public static XmlNode? GetData(this XmlNode elementData, string settingName, XmlNode? defaultData) =>
		GetData(elementData, settingName, out var settingData)? settingData: defaultData;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <returns>設定情報</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />が複数存在している場合</exception>
	/// <exception cref="SettingException"><paramref name="settingName" />が定義されていない場合</exception>
	public static XmlNode GetData(this XmlNode elementData, string settingName) =>
		GetData(elementData, settingName, out var settingData)? settingData: throw new SettingException("指定ノードが存在しません。", ("対象ノード", ToRoute(elementData)), ("検索ノード", settingName));
	#endregion 公開メソッド定義(GetData)

	#region 公開メソッド定義(GetText)
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <returns>設定情報</returns>
	public static string GetText(this XmlNode elementData) =>
		elementData.InnerText;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <returns>設定情報</returns>
	private static string? GetText(XmlAttribute? elementData) =>
		elementData?.Value;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementList">要素一覧</param>
	/// <param name="settingName">設定名称</param>
	/// <returns>設定情報</returns>
	private static string? GetText(XmlAttributeCollection? elementList, string settingName) =>
		GetText(elementList?[settingName]);
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="settingData">設定情報</param>
	/// <returns><paramref name="settingName" />が定義されている場合、<c>True</c>を返却</returns>
	public static bool GetText(this XmlNode elementData, string settingName, [MaybeNullWhen(false)]out string settingData) =>
		(settingData = GetText(elementData.Attributes, settingName)) != null;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="defaultData">既定情報</param>
	/// <returns>設定情報</returns>
	[return: NotNullIfNotNull(nameof(defaultData))]
	public static string? GetText(this XmlNode elementData, string settingName, string? defaultData) =>
		GetText(elementData, settingName, out var settingData)? settingData : defaultData;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <returns>設定情報</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />が定義されていない場合</exception>
	public static string GetText(this XmlNode elementData, string settingName) =>
		GetText(elementData, settingName, out var settingData)? settingData : throw ToError(elementData, settingName);
	#endregion 公開メソッド定義(GetText)

	#region 公開メソッド定義(GetFlag)
	/// <summary>
	/// 設定情報を解析します。
	/// </summary>
	/// <param name="settingText">設定情報</param>
	/// <param name="settingData">解析情報</param>
	/// <returns>解析に成功した場合、<c>True</c>を返却</returns>
	private static bool GetFlag(string settingText, out bool settingData) {
		if (IsMatch(settingText, "1", "True", "Yes", "On")) {
			settingData = true;
			return true;
		} else if (IsMatch(settingText, "0", "False", "No", "Off")) {
			settingData = false;
			return true;
		} else {
			settingData = default;
			return false;
		}
	}
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="settingData">設定情報</param>
	/// <returns><paramref name="settingName" />が定義されている場合、<c>True</c>を返却</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />の値が想定ではない場合</exception>
	public static bool GetFlag(this XmlNode elementData, string settingName, out bool settingData) {
		if (GetText(elementData, settingName, out var settingText) == false) {
			settingData = default;
			return false;
		} else if (GetFlag(settingText, out settingData) == false) {
			throw ToError(elementData, settingName, "真偽形式", settingText);
		} else {
			return true;
		}
	}
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="defaultData">既定情報</param>
	/// <returns>設定情報</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />の値が想定ではない場合</exception>
	public static bool GetFlag(this XmlNode elementData, string settingName, bool defaultData) =>
		GetFlag(elementData, settingName, out var settingData)? settingData : defaultData;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="defaultData">既定情報</param>
	/// <returns>設定情報</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />の値が想定ではない場合</exception>
	public static bool? GetFlag(this XmlNode elementData, string settingName, bool? defaultData) =>
		GetFlag(elementData, settingName, out var settingData)? settingData : defaultData;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <returns>設定情報</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />の値が想定ではない場合</exception>
	/// <exception cref="SettingException"><paramref name="settingName" />が定義されていない場合</exception>
	public static bool GetFlag(this XmlNode elementData, string settingName) =>
		GetFlag(elementData, settingName, out var settingData)? settingData : throw ToError(elementData, settingName);
	#endregion 公開メソッド定義(GetFlag)

	#region 公開メソッド定義(GetInt4)
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="settingData">設定情報</param>
	/// <returns><paramref name="settingName" />が定義されている場合、<c>True</c>を返却</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />の値が想定ではない場合</exception>
	public static bool GetInt4(this XmlNode elementData, string settingName, out int settingData) {
		if (GetText(elementData, settingName, out var settingText) == false) {
			settingData = default;
			return false;
		} else if (Int32.TryParse(settingText, out settingData) == false) {
			throw ToError(elementData, settingName, "整数形式", settingText);
		} else {
			return true;
		}
	}
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="defaultData">既定情報</param>
	/// <returns>設定情報</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />の値が想定ではない場合</exception>
	public static int GetInt4(this XmlNode elementData, string settingName, int defaultData) =>
		GetInt4(elementData, settingName, out var settingData)? settingData : defaultData;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="defaultData">既定情報</param>
	/// <returns>設定情報</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />の値が想定ではない場合</exception>
	public static int? GetInt4(this XmlNode elementData, string settingName, int? defaultData) =>
		GetInt4(elementData, settingName, out var settingData)? settingData : defaultData;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <returns>設定情報</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />の値が想定ではない場合</exception>
	/// <exception cref="SettingException"><paramref name="settingName" />が定義されていない場合</exception>
	public static int GetInt4(this XmlNode elementData, string settingName) =>
		GetInt4(elementData, settingName, out var settingData)? settingData : throw ToError(elementData, settingName);
	#endregion 公開メソッド定義(GetInt4)

	#region 公開メソッド定義(GetSpan)
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="patternText">設定書式</param>
	/// <param name="settingData">設定情報</param>
	/// <returns><paramref name="settingName" />が定義されている場合、<c>True</c>を返却</returns>
	public static bool GetSpan(this XmlNode elementData, string settingName, string patternText, out TimeSpan settingData) {
		if (GetText(elementData, settingName, out var settingText) == false) {
			settingData = default;
			return false;
		} else if (TimeSpan.TryParseExact(settingText, patternText, null, out settingData) == false) {
			throw ToError(elementData, settingName, "期間形式", settingText);
		} else {
			return true;
		}
	}
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="patternText">設定書式</param>
	/// <param name="defaultData">既定情報</param>
	/// <returns>設定情報</returns>
	public static TimeSpan GetSpan(this XmlNode elementData, string settingName, string patternText, TimeSpan defaultData) =>
		GetSpan(elementData, settingName, patternText, out var settingData)? settingData : defaultData;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="patternText">設定書式</param>
	/// <param name="defaultData">既定情報</param>
	/// <returns>設定情報</returns>
	public static TimeSpan? GetSpan(this XmlNode elementData, string settingName, string patternText, TimeSpan? defaultData) =>
		GetSpan(elementData, settingName, patternText, out var settingData)? settingData : defaultData;
	/// <summary>
	/// 設定情報を取得します。
	/// </summary>
	/// <param name="elementData">要素情報</param>
	/// <param name="settingName">設定名称</param>
	/// <param name="patternText">設定書式</param>
	/// <returns>設定情報</returns>
	/// <exception cref="SettingException"><paramref name="settingName" />が定義されていない場合</exception>
	public static TimeSpan GetSpan(this XmlNode elementData, string settingName, string patternText) =>
		GetSpan(elementData, settingName, patternText, out var settingData)? settingData : throw ToError(elementData, settingName);
	#endregion 公開メソッド定義(GetSpan)
}
