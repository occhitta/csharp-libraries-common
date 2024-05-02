namespace Occhitta.Libraries.Common;

/// <summary>
/// <see cref="ExecuteUtilities" />検証クラスです。
/// </summary>
public class ExecuteUtilitiesTest {
	/// <summary>
	/// <see cref="ExecuteUtilities.ExecuteFile" />を検証します。
	/// </summary>
	[Test]
	public void ExecuteFile() {
		var expect = Path.Combine(Environment.CurrentDirectory, "testhost.dll");
		var actual = ExecuteUtilities.ExecuteFile;
		Assert.That(actual, Is.EqualTo(expect));
	}
	/// <summary>
	/// <see cref="ExecuteUtilities.ExecutePath" />を検証します。
	/// </summary>
	[Test]
	public void ExecutePath() {
		var actual = ExecuteUtilities.ExecutePath;
		Assert.That(actual, Is.EqualTo(Environment.CurrentDirectory));
	}
	/// <summary>
	/// <see cref="ExecuteUtilities.ExecuteName" />を検証します。
	/// </summary>
	[Test]
	public void ExecuteName() {
		var actual = ExecuteUtilities.ExecuteName;
		Assert.That(actual, Is.EqualTo("testhost.dll"));
	}
	/// <summary>
	/// <see cref="ExecuteUtilities.GetFullPath(string)" />を検証します。
	/// </summary>
	[Test]
	public void GetFullPath() {
		var actual = ExecuteUtilities.GetFullPath("test.txt");
		Assert.That(actual, Is.EqualTo(Path.Combine(Environment.CurrentDirectory, "test.txt")));
	}
}
