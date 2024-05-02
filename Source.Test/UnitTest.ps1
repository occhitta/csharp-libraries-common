# 基本設定
$OutputDate = Get-Date -Format "yyyyMMdd"
$OutputTime = Get-Date -Format "HHmmss"
$SourcePath = $PSScriptRoot
$OutputPath = "$SourcePath\bin\Reports\$OutputDate";
$OutputFile = "$OutputPath\NUnitTestResult.xml"

# 検証実行
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=$OutputFile
if ($LastExitCode -eq 0) {
  # 検証成功
  reportgenerator -reports:$OutputFile -targetdir:$OutputPath -reporttypes:Html
  if ($LastExitCode -eq 0) {
    # 生成成功
    start "$OutputPath\index.html"
  }
} else {
  # 検証失敗
}
