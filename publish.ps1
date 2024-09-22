dotnet publish ".\src\Avalonia.Desktop\Avalonia.Desktop.csproj" -p:PublishProfile=Windows

$version = (Get-Item .\publish\BuildLauncher.exe).VersionInfo.FileVersion
$version = $version.Substring(0,$version.Length-2)
$version = $version.Replace('.','')

Compress-Archive -Path ".\publish\BuildLauncher.exe" -DestinationPath ".\publish\buildlauncher_${version}_win-x64.zip"
