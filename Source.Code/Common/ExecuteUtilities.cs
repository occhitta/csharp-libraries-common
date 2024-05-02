using System.Reflection;

namespace Occhitta.Libraries.Common;

/// <summary>
/// 実行共通関数クラスです。
/// </summary>
public static class ExecuteUtilities {
	#region メンバー変数定義
	/// <summary>
	/// 実行情報
	/// </summary>
	private static Assembly? executeData;
	/// <summary>
	/// 実行位置
	/// </summary>
	private static string? executeFile;
	/// <summary>
	/// 実行階層
	/// </summary>
	private static string? executePath;
	/// <summary>
	/// 実行名称
	/// </summary>
	private static string? executeName;
	#endregion メンバー変数定義

	#region プロパティー定義
	/// <summary>
	/// 実行情報を取得します。
	/// </summary>
	/// <returns>実行情報</returns>
	/// <remarks>アンマネージドコードを経由して実行されるケースは想定外として、<c>Null</c>返却する(Pragma制御)</remarks>
	#pragma warning disable CS8603
	private static Assembly ExecuteData => executeData ??= Assembly.GetEntryAssembly();
	#pragma warning restore CS8603
	/// <summary>
	/// 実行位置を取得します。
	/// </summary>
	/// <returns>実行位置</returns>
	public static string ExecuteFile => executeFile ??= ExecuteData.Location;
	/// <summary>
	/// 実行階層を取得します。
	/// </summary>
	/// <returns>実行階層</returns>
	/// <remarks><see cref="ExecuteFile" />がフルパスではない場合、<c>Null</c>返却となるが<see cref="ExecuteData" />がフルパスである想定なので<c>Pragma</c>(CS8603)にて警告を抑止している</remarks>
	#pragma warning disable CS8603
	public static string ExecutePath => executePath ??= System.IO.Path.GetDirectoryName(ExecuteFile);
	#pragma warning restore CS8603
	/// <summary>
	/// 実行名称を取得します。
	/// </summary>
	/// <returns>実行名称</returns>
	public static string ExecuteName => executeName ??= System.IO.Path.GetFileName(ExecuteFile);
	#endregion プロパティー定義

	#region 公開メソッド定義
	/// <summary>
	/// 完全階層を取得します。
	/// </summary>
	/// <param name="source">相対階層</param>
	/// <returns>完全階層</returns>
	public static string GetFullPath(string source) =>
		System.IO.Path.GetFullPath(System.IO.Path.Combine(ExecutePath, source));
	#endregion 公開メソッド定義
}
