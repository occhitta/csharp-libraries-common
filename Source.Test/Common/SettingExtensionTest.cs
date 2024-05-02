using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Occhitta.Libraries.Common;

/// <summary>
/// <see cref="SettingExtension" />検証クラスです。
/// </summary>
public class SettingExtensionTest {
	#region メンバー定数定義
	/// <summary>重複名文言</summary>
	private const string DuplicateMessage = "単一ノードの想定ですが複数ノードが指定されています。\r\n対象ノード:a\r\n検索ノード:c";
	/// <summary>非存在文言</summary>
	private const string NotFoundMessage  = "指定ノードが存在しません。\r\n対象ノード:a\r\n検索ノード:d";
	/// <summary>非定義文言</summary>
	private const string AttributeMessage = "属性名が定義されていません。\r\n対象ノード:a\r\n検索属性名:c";
	#endregion メンバー定数定義

	#region 内部メソッド定義(ToValue)
	/// <summary>
	/// 要素情報を取得します。
	/// </summary>
	/// <param name="sourceCode">要素情報</param>
	/// <returns>要素情報</returns>
	#nullable disable
	private static XmlElement ToValue(XmlDocument sourceCode) =>
		sourceCode.DocumentElement;
	#nullable restore
	/// <summary>
	/// 要素情報を生成します。
	/// </summary>
	/// <param name="sourceCode">要素構文</param>
	/// <returns>要素情報</returns>
	[return:NotNullIfNotNull(nameof(sourceCode))]
	#pragma warning disable CA1859
	private static XmlNode? ToValue(string? sourceCode) {
	#pragma warning restore CA1859
		if (sourceCode == null) {
			return null;
		} else {
			var result = new XmlDocument();
			result.LoadXml(sourceCode);
			return ToValue(result);
		}
	}
	#endregion 内部メソッド定義(ToValue)

	#region 内部メソッド定義(OnError)
	/// <summary>
	/// 異常処理を検証します。
	/// </summary>
	/// <param name="invokeCode">実行処理</param>
	/// <returns>異常文言</returns>
	private static string OnError(Action invokeCode) {
		var resultData = Assert.Throws<SettingException>(() => invokeCode());
		return resultData.Message;
	}
	#endregion 内部メソッド定義(OnError)

	#region 検証メソッド定義(ToRoute)
	/// <summary>
	/// <see cref="SettingExtension.ToRoute(XmlNode)" />を検証します。
	/// </summary>
	/// <param name="sourceCode">検証構文</param>
	/// <param name="searchText">検索経路</param>
	/// <returns>結果情報</returns>
	[TestCase("<a><b><c></c></b></a>", null,     ExpectedResult="a")]
	[TestCase("<a><b><c></c></b></a>", "/a/b",   ExpectedResult="a:b")]
	[TestCase("<a><b><c></c></b></a>", "/a/b/c", ExpectedResult="a:b:c")]
	public string TestToRoute(string sourceCode, string? searchText) {
		var sourceData = ToValue(sourceCode);
		if (searchText == null) {
			return SettingExtension.ToRoute(sourceData);
		} else {
			sourceData = sourceData.SelectSingleNode(searchText);
			Assert.That(sourceData, Is.Not.Null);
			return SettingExtension.ToRoute(sourceData);
		}
	}
	#endregion 検証メソッド定義(ToRoute)

	#region 検証メソッド定義(GetList)
	/// <summary>
	/// 下位一覧取得検証クラスです。
	/// <para>以下のメソッドを検証します。
	///   <para><see cref="SettingExtension.GetList(XmlNode)" /></para>
	///   <para><see cref="SettingExtension.GetList(XmlNode, string)" /></para>
	///   <para><see cref="SettingExtension.GetList{TResult}(XmlNode, string, Func{XmlNode, TResult})" /></para>
	///   <para><see cref="SettingExtension.GetList{TSource, TResult}(XmlNode, string, Func{XmlNode, TSource, TResult}, TSource)" /></para>
	/// </para>
	/// </summary>
	[TestFixture]
	public class GetList {
		#region 内部メソッド定義(ChooseName/ConcatName)
		/// <summary>
		/// 要素名称を取得します。
		/// </summary>
		/// <param name="sourceData">要素情報</param>
		/// <returns>要素名称</returns>
		private static string ChooseName(XmlNode sourceData) =>
			sourceData.Name;
		/// <summary>
		/// 要素名称を連結します。
		/// </summary>
		/// <param name="sourceData">要素情報</param>
		/// <param name="suffixText">末尾情報</param>
		/// <returns>連結情報</returns>
		private static string ConcatName(XmlNode sourceData, string suffixText) =>
			$"{sourceData.Name}{suffixText}";
		#endregion 内部メソッド定義(ChooseName/ConcatName)

		#region 検証メソッド定義
		/// <summary>
		/// <see cref="SettingExtension.GetList(XmlNode)" />を検証します。
		/// </summary>
		/// <param name="source">検証構文</param>
		/// <returns>名称配列</returns>
		[TestCase("<a><b /><c /><c><d /></c></a>", ExpectedResult=new string[] {"b", "c", "c"})]
		public string[] Pattern1(string sourceCode) {
			var resultList = new List<string>();
			foreach (var chooseData in SettingExtension.GetList(ToValue(sourceCode))) {
				resultList.Add(chooseData.Name);
			}
			return [.. resultList];
		}
		/// <summary>
		/// <see cref="SettingExtension.GetList(XmlNode, string)" />を検証します。
		/// </summary>
		/// <param name="source">検証構文</param>
		/// <param name="search">検索名称</param>
		/// <returns>該当件数</returns>
		[TestCase("<a><b /><c /><c /></a>", "a", ExpectedResult=0)]
		[TestCase("<a><b /><c /><c /></a>", "b", ExpectedResult=1)]
		[TestCase("<a><b /><c /><c /></a>", "c", ExpectedResult=2)]
		public int Pattern2(string sourceCode, string searchName) {
			var result = 0;
			foreach (var choose in SettingExtension.GetList(ToValue(sourceCode), searchName)) {
				Assert.That(choose.Name, Is.EqualTo(searchName));
				result ++;
			}
			return result;
		}
		/// <summary>
		/// <see cref="SettingExtension.GetList{TResult}(XmlNode, string, Func{XmlNode, TResult})" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>変換配列</returns>
		[TestCase("<a><b /><c /><c /></a>", "a", ExpectedResult=new string[0])]
		[TestCase("<a><b /><c /><c /></a>", "b", ExpectedResult=new string[] {"b"})]
		[TestCase("<a><b /><c /><c /></a>", "c", ExpectedResult=new string[] {"c", "c"})]
		public string[] Pattern3(string sourceCode, string searchName) =>
			SettingExtension.GetList(ToValue(sourceCode), searchName, ChooseName);
		/// <summary>
		/// <see cref="SettingExtension.GetList{TSource, TResult}(XmlNode, string, Func{XmlNode, TSource, TResult}, TSource)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="suffixText">末尾情報</param>
		/// <returns>変換配列</returns>
		[TestCase("<a><b /><c /><c /></a>", "a", "!", ExpectedResult=new string[0])]
		[TestCase("<a><b /><c /><c /></a>", "b", "!", ExpectedResult=new string[] {"b!"})]
		[TestCase("<a><b /><c /><c /></a>", "c", "!", ExpectedResult=new string[] {"c!", "c!"})]
		public string[] Pattern4(string sourceCode, string searchName, string suffixText) =>
			SettingExtension.GetList(ToValue(sourceCode), searchName, ConcatName, suffixText);
		#endregion 検証メソッド定義
	}
	#endregion 検証メソッド定義(GetList)

	#region 検証メソッド定義(GetData)
	/// <summary>
	/// 下位情報取得検証クラスです。
	/// <para>以下のメソッドを検証します。
	///   <para><see cref="SettingExtension.GetData(XmlNode, string, out XmlNode)" /></para>
	///   <para><see cref="SettingExtension.GetData(XmlNode, string, XmlNode?)" /></para>
	///   <para><see cref="SettingExtension.GetData(XmlNode, string)" /></para>
	/// </para>
	/// </summary>
	[TestFixture]
	public class GetData {
		/// <summary>
		/// 成功処理を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="expectFlag">想定種別</param>
		/// <param name="expectName">想定名称</param>
		[TestCase("<a><b /><c /><c /></a>", "a", false, null)]
		[TestCase("<a><b /><c /><c /></a>", "b", true,  "b")]
		public void Success1(string sourceCode, string searchName, bool expectFlag, string? expectName) {
			var resultFlag = SettingExtension.GetData(ToValue(sourceCode), searchName, out var cache2);
			Assert.Multiple(() => {
				Assert.That(resultFlag,   Is.EqualTo(expectFlag));
				Assert.That(cache2?.Name, Is.EqualTo(expectName));
			});
		}
		/// <summary>
		/// 失敗処理を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a><b /><c /><c /></a>", "c", ExpectedResult=DuplicateMessage)]
		public string Failure1(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetData(ToValue(sourceCode), searchName, out var actual));

		/// <summary>
		/// 成功処理を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="extendCode">既定構文</param>
		/// <param name="expectName">想定名称</param>
		/// <param name="expectText">想定内容</param>
		[TestCase("<a><b>1A</b><c>2B</c><c>3C</c></a>", "a", null,        null, null, Reason="「a」タグがないので既定情報を返却(返却値はNULL)")]
		[TestCase("<a><b>1A</b><c>2B</c><c>3C</c></a>", "a", "<a>4D</a>", "a",  "4D", Reason="「a」タグがないので既定情報を返却")]
		[TestCase("<a><b>1A</b><c>2B</c><c>3C</c></a>", "a", "<d>5D</d>", "d",  "5D", Reason="「a」タグがないので既定情報を返却")]
		[TestCase("<a><b>1A</b><c>2B</c><c>3C</c></a>", "b", null,        "b",  "1A", Reason="「b」タグがあるので抽出情報を返却")]
		[TestCase("<a><b>1A</b><c>2B</c><c>3C</c></a>", "b", "<a>4D</a>", "b",  "1A", Reason="「b」タグがあるので抽出情報を返却")]
		[TestCase("<a><b>1A</b><c>2B</c><c>3C</c></a>", "b", "<d>5D</d>", "b",  "1A", Reason="「b」タグがあるので抽出情報を返却")]
		public void Success2(string sourceCode, string searchName, string? extendCode, string? expectName, string? expectText) {
			var resultData = SettingExtension.GetData(ToValue(sourceCode), searchName, ToValue(extendCode));
			Assert.Multiple(() => {
				Assert.That(resultData?.Name,      Is.EqualTo(expectName));
				Assert.That(resultData?.InnerText, Is.EqualTo(expectText));
			});
		}
		/// <summary>
		/// 失敗情報を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a><b>1A</b><c>2B</c><c>3C</c></a>", "c", ExpectedResult=DuplicateMessage, Reason="「c」タグが複数存在するので例外発行")]
		public string Failure2(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetData(ToValue(sourceCode), searchName, null));

		/// <summary>
		/// 成功処理を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>結果情報</returns>
		[TestCase("<a><b>1A</b><c>2B</c>3C<c /></a>", "b", ExpectedResult="1A")]
		public string Success3(string sourceCode, string searchName) {
			var resultData = SettingExtension.GetData(ToValue(sourceCode), searchName);
			Assert.That(resultData.Name, Is.EqualTo(searchName));
			return resultData.InnerText;
		}
		/// <summary>
		/// 失敗処理を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a><b>1A</b><c>2B</c>3C<c /></a>", "d", ExpectedResult=NotFoundMessage)]
		[TestCase("<a><b>1A</b><c>2B</c>3C<c /></a>", "c", ExpectedResult=DuplicateMessage)]
		public string Failure3(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetData(ToValue(sourceCode), searchName));
	}
	#endregion 検証メソッド定義(GetData)

	#region 検証メソッド定義(GetText)
	/// <summary>
	/// 要素情報取得検証クラスです。
	/// </summary>
	[TestFixture]
	public class GetText {
		/// <summary>
		/// <see cref="SettingExtension.GetText(XmlNode)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <returns>結果情報</returns>
		[TestCase("<a />",    ExpectedResult="")]
		[TestCase("<a></a>",  ExpectedResult="")]
		[TestCase("<a>1</a>", ExpectedResult="1")]
		public string Success1(string sourceCode) =>
			SettingExtension.GetText(ToValue(sourceCode));

		/// <summary>
		/// <see cref="SettingExtension.GetText(XmlNode, string, out string)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="expectFlag">想定種別</param>
		/// <param name="expectData">想定結果</param>
		[TestCase("<a b=\"12\" />", "b", true,  "12")]
		[TestCase("<a b=\"12\" />", "c", false, null)]
		public void Success2(string sourceCode, string searchName, bool expectFlag, string? expectData) {
			var resultFlag = SettingExtension.GetText(ToValue(sourceCode), searchName, out var resultData);
			Assert.Multiple(() => {
				Assert.That(resultFlag, Is.EqualTo(expectFlag));
				Assert.That(resultData, Is.EqualTo(expectData));
			});
		}

		/// <summary>
		/// <see cref="SettingExtension.GetText(XmlNode, string, string?)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="extendData">既定情報</param>
		/// <returns>結果情報</returns>
		[TestCase("<a b=\"12\" />", "b", null, ExpectedResult="12")]
		[TestCase("<a b=\"12\" />", "b", "AB", ExpectedResult="12")]
		[TestCase("<a b=\"12\" />", "c", null, ExpectedResult=null)]
		[TestCase("<a b=\"12\" />", "c", "AB", ExpectedResult="AB")]
		public string? Success3(string sourceCode, string searchName, string? extendData) =>
			SettingExtension.GetText(ToValue(sourceCode), searchName, extendData);

		/// <summary>
		/// <see cref="SettingExtension.GetText(XmlNode, string)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>結果情報</returns>
		[TestCase("<a b=\"c\" />", "b", ExpectedResult="c")]
		public string Success4(string sourceCode, string searchName) =>
			SettingExtension.GetText(ToValue(sourceCode), searchName);
		/// <summary>
		/// <see cref="SettingExtension.GetText(XmlNode, string)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a b=\"c\" />", "c", ExpectedResult=AttributeMessage)]
		public string Failure4(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetText(ToValue(sourceCode), searchName));

		/// <summary>
		/// <see cref="SettingExtension.GetText(XmlAttributeCollection?, string)" />を検証します。
		/// <para>当該検証は上記メソッドのカバレッジ用となります。</para>
		/// </summary>
		/// <remarks>タグ情報の場合は属性がNULLとならない為、第一引数(elementData)に対して属性情報(<see cref="XmlAttribute" />)を渡す事によりNULL分岐を補填する</remarks>
		[Test]
		public void Success5() {
			var sourceData = ToValue("<a b=\"\" />");
			var chooseData = sourceData.Attributes?["b"] ?? throw new Exception();
			Assert.That(SettingExtension.GetText(chooseData, "test", null), Is.Null);
		}
	}
	#endregion 検証メソッド定義(GetText)

	#region 検証メソッド定義(GetFlag)
	/// <summary>
	/// 要素情報取得検証クラスです。
	/// </summary>
	[TestFixture]
	public class GetFlag {
		/// <summary>
		/// <see cref="SettingExtension.GetFlag(XmlNode, string, out bool)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="expectFlag">想定種別</param>
		/// <param name="expectData">想定結果</param>
		[TestCase("<a b=\"1\"     />", "c", false, false)]
		[TestCase("<a b=\"1\"     />", "b", true,  true )]
		[TestCase("<a b=\"true\"  />", "b", true,  true )]
		[TestCase("<a b=\"TRUE\"  />", "b", true,  true )]
		[TestCase("<a b=\"True\"  />", "b", true,  true )]
		[TestCase("<a b=\"on\"    />", "b", true,  true )]
		[TestCase("<a b=\"ON\"    />", "b", true,  true )]
		[TestCase("<a b=\"On\"    />", "b", true,  true )]
		[TestCase("<a b=\"yes\"   />", "b", true,  true )]
		[TestCase("<a b=\"YES\"   />", "b", true,  true )]
		[TestCase("<a b=\"Yes\"   />", "b", true,  true )]
		[TestCase("<a b=\"0\"     />", "b", true,  false)]
		[TestCase("<a b=\"false\" />", "b", true,  false)]
		[TestCase("<a b=\"FALSE\" />", "b", true,  false)]
		[TestCase("<a b=\"FALSE\" />", "b", true,  false)]
		[TestCase("<a b=\"off\"   />", "b", true,  false)]
		[TestCase("<a b=\"OFF\"   />", "b", true,  false)]
		[TestCase("<a b=\"Off\"   />", "b", true,  false)]
		[TestCase("<a b=\"no\"    />", "b", true,  false)]
		[TestCase("<a b=\"NO\"    />", "b", true,  false)]
		[TestCase("<a b=\"No\"    />", "b", true,  false)]
		public void Success1(string sourceCode, string searchName, bool expectFlag, bool expectData) {
			var resultFlag = SettingExtension.GetFlag(ToValue(sourceCode), searchName, out var resultData);
			Assert.Multiple(() => {
				Assert.That(resultFlag, Is.EqualTo(expectFlag));
				Assert.That(resultData, Is.EqualTo(expectData));
			});
		}
		/// <summary>
		/// <see cref="SettingExtension.GetFlag(XmlNode, string, out bool)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a b=\"\"  />", "b", ExpectedResult="属性値が真偽形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:")]
		[TestCase("<a b=\"2\" />", "b", ExpectedResult="属性値が真偽形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:2")]
		public string Failure1(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetFlag(ToValue(sourceCode), searchName, out var resultData));

		/// <summary>
		/// <see cref="SettingExtension.GetFlag(XmlNode, string, bool)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="extendData">既定情報</param>
		/// <returns>結果情報</returns>
		[TestCase("<a b=\"1\" />", "b", true,  ExpectedResult=true )]
		[TestCase("<a b=\"1\" />", "b", false, ExpectedResult=true )]
		[TestCase("<a b=\"0\" />", "b", true,  ExpectedResult=false)]
		[TestCase("<a b=\"0\" />", "b", false, ExpectedResult=false)]
		[TestCase("<a b=\"1\" />", "c", true,  ExpectedResult=true )]
		[TestCase("<a b=\"1\" />", "c", false, ExpectedResult=false)]
		public bool Success2(string sourceCode, string searchName, bool extendData) =>
			SettingExtension.GetFlag(ToValue(sourceCode), searchName, extendData);
		/// <summary>
		/// <see cref="SettingExtension.GetFlag(XmlNode, string, bool)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a b=\"\"  />", "b", ExpectedResult="属性値が真偽形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:")]
		[TestCase("<a b=\"2\" />", "b", ExpectedResult="属性値が真偽形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:2")]
		public string Failure2(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetFlag(ToValue(sourceCode), searchName, true));

		/// <summary>
		/// <see cref="SettingExtension.GetFlag(XmlNode, string, bool?)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="extendData">既定情報</param>
		/// <returns>結果情報</returns>
		[TestCase("<a b=\"1\" />", "b", true,  ExpectedResult=true )]
		[TestCase("<a b=\"1\" />", "b", false, ExpectedResult=true )]
		[TestCase("<a b=\"1\" />", "b", null,  ExpectedResult=true )]
		[TestCase("<a b=\"0\" />", "b", true,  ExpectedResult=false)]
		[TestCase("<a b=\"0\" />", "b", false, ExpectedResult=false)]
		[TestCase("<a b=\"0\" />", "b", null,  ExpectedResult=false)]
		[TestCase("<a b=\"1\" />", "c", true,  ExpectedResult=true )]
		[TestCase("<a b=\"1\" />", "c", false, ExpectedResult=false)]
		[TestCase("<a b=\"1\" />", "c", null,  ExpectedResult=null )]
		public bool? Success3(string sourceCode, string searchName, bool? extendData) =>
			SettingExtension.GetFlag(ToValue(sourceCode), searchName, extendData);
		/// <summary>
		/// <see cref="SettingExtension.GetFlag(XmlNode, string, bool?)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a b=\"\"  />", "b", ExpectedResult="属性値が真偽形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:")]
		[TestCase("<a b=\"2\" />", "b", ExpectedResult="属性値が真偽形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:2")]
		public string Failure3(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetFlag(ToValue(sourceCode), searchName, null));

		/// <summary>
		/// <see cref="SettingExtension.GetFlag(XmlNode, string)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>結果情報</returns>
		[TestCase("<a b=\"1\" />", "b", ExpectedResult=true )]
		[TestCase("<a b=\"0\" />", "b", ExpectedResult=false)]
		public bool Success4(string sourceCode, string searchName) =>
			SettingExtension.GetFlag(ToValue(sourceCode), searchName);
		/// <summary>
		/// <see cref="SettingExtension.GetFlag(XmlNode, string)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a b=\"\"  />", "b", ExpectedResult="属性値が真偽形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:")]
		[TestCase("<a b=\"2\" />", "b", ExpectedResult="属性値が真偽形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:2")]
		[TestCase("<a b=\"1\" />", "c", ExpectedResult="属性名が定義されていません。\r\n対象ノード:a\r\n検索属性名:c")]
		public string Failure4(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetFlag(ToValue(sourceCode), searchName));
	}
	#endregion 検証メソッド定義(GetFlag)

	#region 検証メソッド定義(GetInt4)
	/// <summary>
	/// 要素情報取得検証クラスです。
	/// </summary>
	[TestFixture]
	public class GetInt4 {
		/// <summary>
		/// <see cref="SettingExtension.GetInt4(XmlNode, string, out int)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="expectFlag">想定種別</param>
		/// <param name="expectData">想定結果</param>
		[TestCase("<a b=\"-1\" />", "b", true,  -1)]
		[TestCase("<a b=\"+0\" />", "b", true,   0)]
		[TestCase("<a b=\"+1\" />", "b", true,   1)]
		[TestCase("<a b=\"+0\" />", "c", false,  0)]
		[TestCase("<a b=\"+0\" />", "c", false,  0)]
		[TestCase("<a b=\"+0\" />", "c", false,  0)]
		public void Success1(string sourceCode, string searchName, bool expectFlag, int expectData) {
			var resultFlag = SettingExtension.GetInt4(ToValue(sourceCode), searchName, out var resultData);
			Assert.Multiple(() => {
				Assert.That(resultFlag, Is.EqualTo(expectFlag));
				Assert.That(resultData, Is.EqualTo(expectData));
			});
		}
		/// <summary>
		/// <see cref="SettingExtension.GetInt4(XmlNode, string, out int)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a b=\"\"  />", "b", ExpectedResult="属性値が整数形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:")]
		[TestCase("<a b=\"c\" />", "b", ExpectedResult="属性値が整数形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:c")]
		public string Failure1(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetInt4(ToValue(sourceCode), searchName, out var resultData));

		/// <summary>
		/// <see cref="SettingExtension.GetInt4(XmlNode, string, int)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="extendData">既定情報</param>
		/// <returns>結果情報</returns>
		[TestCase("<a b=\"-1\" />", "b",  0, ExpectedResult=-1)]
		[TestCase("<a b=\"+0\" />", "b",  0, ExpectedResult= 0)]
		[TestCase("<a b=\"+1\" />", "b",  0, ExpectedResult= 1)]
		[TestCase("<a b=\"+0\" />", "c", -1, ExpectedResult=-1)]
		[TestCase("<a b=\"+0\" />", "c",  0, ExpectedResult= 0)]
		[TestCase("<a b=\"+0\" />", "c",  1, ExpectedResult= 1)]
		public int Success2(string sourceCode, string searchName, int extendData) =>
			SettingExtension.GetInt4(ToValue(sourceCode), searchName, extendData);
		/// <summary>
		/// <see cref="SettingExtension.GetInt4(XmlNode, string, int)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a b=\"\"  />", "b", ExpectedResult="属性値が整数形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:")]
		[TestCase("<a b=\"c\" />", "b", ExpectedResult="属性値が整数形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:c")]
		public string Failure2(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetInt4(ToValue(sourceCode), searchName, 0));

		/// <summary>
		/// <see cref="SettingExtension.GetInt4(XmlNode, string, int?)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="extendData">既定情報</param>
		/// <returns>結果情報</returns>
		[TestCase("<a b=\"-1\" />", "b", null, ExpectedResult=  -1)]
		[TestCase("<a b=\"+0\" />", "b", null, ExpectedResult=   0)]
		[TestCase("<a b=\"+1\" />", "b", null, ExpectedResult=   1)]
		[TestCase("<a b=\"+0\" />", "c",   -1, ExpectedResult=  -1)]
		[TestCase("<a b=\"+0\" />", "c",    0, ExpectedResult=   0)]
		[TestCase("<a b=\"+0\" />", "c",    1, ExpectedResult=   1)]
		[TestCase("<a b=\"+0\" />", "c", null, ExpectedResult=null)]
		public int? Success3(string sourceCode, string searchName, int? extendData) =>
			SettingExtension.GetInt4(ToValue(sourceCode), searchName, extendData);
		/// <summary>
		/// <see cref="SettingExtension.GetInt4(XmlNode, string, int?)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a b=\"\"  />", "b", ExpectedResult="属性値が整数形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:")]
		[TestCase("<a b=\"c\" />", "b", ExpectedResult="属性値が整数形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:c")]
		public string Failure3(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetInt4(ToValue(sourceCode), searchName, null));

		/// <summary>
		/// <see cref="SettingExtension.GetInt4(XmlNode, string)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>結果情報</returns>
		[TestCase("<a b=\"-1\" />", "b", ExpectedResult=-1)]
		[TestCase("<a b=\"+0\" />", "b", ExpectedResult= 0)]
		[TestCase("<a b=\"+1\" />", "b", ExpectedResult= 1)]
		public int Success4(string sourceCode, string searchName) =>
			SettingExtension.GetInt4(ToValue(sourceCode), searchName);
		/// <summary>
		/// <see cref="SettingExtension.GetInt4(XmlNode, string)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCase("<a b=\"\"  />", "b", ExpectedResult="属性値が整数形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:")]
		[TestCase("<a b=\"c\" />", "b", ExpectedResult="属性値が整数形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:c")]
		[TestCase("<a b=\"1\" />", "c", ExpectedResult="属性名が定義されていません。\r\n対象ノード:a\r\n検索属性名:c")]
		public string Failure4(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetInt4(ToValue(sourceCode), searchName));
	}
	#endregion 検証メソッド定義(GetInt4)

	#region 検証メソッド定義(GetSpan)
	/// <summary>
	/// 要素情報取得検証クラスです。
	/// </summary>
	[TestFixture]
	public class GetSpan {
		#region メンバー定数定義
		/// <summary>標準書式</summary>
		private const string FormatText = "hh\\:mm\\:ss";
		/// <summary>要素情報</summary>
		private const string ValueText0 = "<a b=\"00:00:00\" />";
		/// <summary>要素情報</summary>
		private const string ValueText1 = "<a b=\"00:00:01\" />";
		/// <summary>要素情報</summary>
		private static readonly TimeSpan ValueData0 = TimeSpan.FromSeconds(0);
		/// <summary>要素情報</summary>
		private static readonly TimeSpan ValueData1 = TimeSpan.FromSeconds(1);
		/// <summary>要素情報</summary>
		private static readonly TimeSpan? ValueDataN = null;
		#endregion メンバー定数定義

		#region 内部メソッド定義(Success/Failure)
		/// <summary>
		/// 検証情報を生成します。
		/// </summary>
		/// <param name="pattern">検証種別</param>
		/// <returns>検証集合</returns>
		private static IEnumerable<TestCaseData> Success(int pattern) {
			switch (pattern) {
			case 1:
				yield return new TestCaseData(ValueText0, "b", FormatText, true,  ValueData0);
				yield return new TestCaseData(ValueText1, "b", FormatText, true,  ValueData1);
				yield return new TestCaseData(ValueText1, "c", FormatText, false, TimeSpan.Zero);
				break;
			case 2:
				// 属性定義あり
				yield return new TestCaseData(ValueText0, "b", FormatText, ValueData0) { ExpectedResult = ValueData0 };
				yield return new TestCaseData(ValueText0, "b", FormatText, ValueData1) { ExpectedResult = ValueData0 };
				// 属性定義あり
				yield return new TestCaseData(ValueText1, "b", FormatText, ValueData0) { ExpectedResult = ValueData1 };
				yield return new TestCaseData(ValueText1, "b", FormatText, ValueData1) { ExpectedResult = ValueData1 };
				// 属性定義なし
				yield return new TestCaseData(ValueText0, "c", FormatText, ValueData0) { ExpectedResult = ValueData0 };
				yield return new TestCaseData(ValueText0, "c", FormatText, ValueData1) { ExpectedResult = ValueData1 };
				break;
			case 3:
				// 属性定義あり
				yield return new TestCaseData(ValueText0, "b", FormatText, ValueData0) { ExpectedResult = ValueData0 };
				yield return new TestCaseData(ValueText0, "b", FormatText, ValueData1) { ExpectedResult = ValueData0 };
				yield return new TestCaseData(ValueText0, "b", FormatText, ValueDataN) { ExpectedResult = ValueData0 };
				// 属性定義あり
				yield return new TestCaseData(ValueText1, "b", FormatText, ValueData0) { ExpectedResult = ValueData1 };
				yield return new TestCaseData(ValueText1, "b", FormatText, ValueData1) { ExpectedResult = ValueData1 };
				yield return new TestCaseData(ValueText1, "b", FormatText, ValueDataN) { ExpectedResult = ValueData1 };
				// 属性定義なし
				yield return new TestCaseData(ValueText0, "c", FormatText, ValueData0) { ExpectedResult = ValueData0 };
				yield return new TestCaseData(ValueText0, "c", FormatText, ValueData1) { ExpectedResult = ValueData1 };
				yield return new TestCaseData(ValueText0, "c", FormatText, ValueDataN) { ExpectedResult = ValueDataN };
				break;
			case 4:
				yield return new TestCaseData(ValueText0, "b", FormatText) { ExpectedResult = ValueData0 };
				yield return new TestCaseData(ValueText1, "b", FormatText) { ExpectedResult = ValueData1 };
				break;
			}
		}
		/// <summary>
		/// 検証情報を生成します。
		/// </summary>
		/// <returns>検証集合</returns>
		private static IEnumerable<TestCaseData> Failure() {
			yield return new TestCaseData("<a b=\"\"             />", "b") { ExpectedResult = "属性値が期間形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:" };
			yield return new TestCaseData("<a b=\"00:00:00.000\" />", "b") { ExpectedResult = "属性値が期間形式ではありません。\r\n対象ノード:a\r\n検索属性名:b\r\n属性値情報:00:00:00.000" };
		}
		#endregion 内部メソッド定義(Success/Failure)

		#region 検証メソッド定義(パターン１)
		/// <summary>
		/// <see cref="SettingExtension.GetSpan(XmlNode, string, string, out TimeSpan)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="formatText">変換書式</param>
		/// <param name="expectFlag">想定種別</param>
		/// <param name="expectData">想定結果</param>
		#pragma warning disable IDE0300
		[TestCaseSource(nameof(Success), new object[] { 1 })]
		#pragma warning restore IDE0300
		public void Success1(string sourceCode, string searchName, string formatText, bool expectFlag, TimeSpan expectData) {
			var resultFlag = SettingExtension.GetSpan(ToValue(sourceCode), searchName, formatText, out var resultData);
			Assert.Multiple(() => {
				Assert.That(resultFlag, Is.EqualTo(expectFlag));
				Assert.That(resultData, Is.EqualTo(expectData));
			});
		}
		/// <summary>
		/// <see cref="SettingExtension.GetSpan(XmlNode, string, string, out TimeSpan)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCaseSource(nameof(Failure))]
		public string Failure1(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetSpan(ToValue(sourceCode), searchName, FormatText, out var resultData));
		#endregion 検証メソッド定義(パターン１)

		#region 検証メソッド定義(パターン２)
		/// <summary>
		/// <see cref="SettingExtension.GetSpan(XmlNode, string, string, TimeSpan)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="extendData">既定情報</param>
		/// <returns>結果情報</returns>
		#pragma warning disable IDE0300
		[TestCaseSource(nameof(Success), new object[] { 2 })]
		#pragma warning restore IDE0300
		public TimeSpan Success2(string sourceCode, string searchName, string formatText, TimeSpan extendData) =>
			SettingExtension.GetSpan(ToValue(sourceCode), searchName, formatText, extendData);
		/// <summary>
		/// <see cref="SettingExtension.GetSpan(XmlNode, string, string, TimeSpan)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCaseSource(nameof(Failure))]
		public string Failure2(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetSpan(ToValue(sourceCode), searchName, FormatText, TimeSpan.Zero));
		#endregion 検証メソッド定義(パターン２)

		#region 検証メソッド定義(パターン３)
		/// <summary>
		/// <see cref="SettingExtension.GetSpan(XmlNode, string, string, TimeSpan?)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="extendData">既定情報</param>
		/// <returns>結果情報</returns>
		#pragma warning disable IDE0300
		[TestCaseSource(nameof(Success), new object[] { 3 })]
		#pragma warning restore IDE0300
		public TimeSpan? Success3(string sourceCode, string searchName, string formatText, TimeSpan? extendData) =>
			SettingExtension.GetSpan(ToValue(sourceCode), searchName, formatText, extendData);
		/// <summary>
		/// <see cref="SettingExtension.GetSpan(XmlNode, string, string, TimeSpan?)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCaseSource(nameof(Failure))]
		public string Failure3(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetSpan(ToValue(sourceCode), searchName, FormatText, null));
		#endregion 検証メソッド定義(パターン３)

		#region 検証メソッド定義(パターン４)
		/// <summary>
		/// <see cref="SettingExtension.GetSpan(XmlNode, string, string)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <param name="formatText">変換書式</param>
		/// <returns>結果情報</returns>
		#pragma warning disable IDE0300
		[TestCaseSource(nameof(Success), new object[] { 4 })]
		#pragma warning restore IDE0300
		public TimeSpan Success4(string sourceCode, string searchName, string formatText) =>
			SettingExtension.GetSpan(ToValue(sourceCode), searchName, formatText);
		/// <summary>
		/// <see cref="SettingExtension.GetSpan(XmlNode, string, string)" />を検証します。
		/// </summary>
		/// <param name="sourceCode">検証構文</param>
		/// <param name="searchName">検索名称</param>
		/// <returns>失敗文言</returns>
		[TestCaseSource(nameof(Failure))]
		[TestCase(ValueText0, "c", ExpectedResult="属性名が定義されていません。\r\n対象ノード:a\r\n検索属性名:c")]
		public string Failure4(string sourceCode, string searchName) =>
			OnError(() => SettingExtension.GetSpan(ToValue(sourceCode), searchName, FormatText));
		#endregion 検証メソッド定義(パターン４)
	}
	#endregion 検証メソッド定義(GetSpan)
}
