# 備忘録
## ファイルの公開方法(NuGet)
### プロジェクトファイル
以下のコードをcsprojに追加
```
<VersionPrefix>1.0.0</VersionPrefix>
```
※「`1.0.0`」は適宜リリース毎に変更する

### プレリリース方法  
ターミナルより以下のコマンドを実行
```powershell:ターミナル
dotnet pack -c Release --include-symbols --version-suffix "alphaYYYYMMDD"
```
※`YYYYMMDD`は例示であり、年月日ではなくてもいい  
※`bin/Release`配下に出力されたファイルをNuGetに公開する

### リリース方法
ターミナルより以下のコマンドを実行
```powershell:ターミナル
dotnet pack -c Release --include-symbols
```
※`bin/Release`配下に出力されたファイルをNuGetに公開する
