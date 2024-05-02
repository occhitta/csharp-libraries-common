using static System.Environment;

namespace Occhitta.Libraries.Common;

/// <summary>
/// <see cref="SettingException" />検証クラスです。
/// </summary>
public class SettingExceptionText {
	/// <summary>
	/// <see cref="SettingException(string)" />を検証します。
	/// </summary>
	[Test]
	public void Pattern1() {
		var resultData = new SettingException("Message");
		Assert.Multiple(() => {
			Assert.That(resultData.ElementList,    Is.Not.Null);
			Assert.That(resultData.ElementList,    Is.Empty);
			Assert.That(resultData.Message,        Is.EqualTo("Message"));
			Assert.That(resultData.InnerException, Is.Null);
		});
	}
	/// <summary>
	/// <see cref="SettingException(string, Exception)" />を検証します。
	/// </summary>
	[Test]
	public void Pattern2() {
		var sourceData = new Exception("Source");
		var resultData = new SettingException("Message", sourceData);
		Assert.Multiple(() => {
			Assert.That(resultData.ElementList,    Is.Not.Null);
			Assert.That(resultData.ElementList,    Is.Empty);
			Assert.That(resultData.Message,        Is.EqualTo("Message"));
			Assert.That(resultData.InnerException, Is.SameAs(sourceData));
		});
	}
	/// <summary>
	/// <see cref="SettingException(string, ValueTuple{string, object?}[])" />を検証します。
	/// </summary>
	[Test]
	public void Pattern3() {
		var expectList = new List<(string, object?)>() { ("Name01", 1), ("Name02", "A") };
		var resultData = new SettingException("Message", [.. expectList]);
		Assert.Multiple(() => {
			Assert.That(resultData.ElementList,    Is.Not.Null);
			Assert.That(resultData.ElementList,    Is.EqualTo(expectList));
			Assert.That(resultData.Message,        Is.EqualTo($"Message{NewLine}Name01:1{NewLine}Name02:A"));
			Assert.That(resultData.InnerException, Is.Null);
		});
	}
}
