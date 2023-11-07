# MySolarCells

Release App IOS
dotnet publish MySolarCells.csproj -f net8.0-ios -c Release -p:RuntimeIdentifier=ios-arm64 -p:BuildIpa=True -o ./artifacts

Release App Maccatalyst
dotnet publish MySolarCells.csproj -f net8.0-maccatalyst -c Release -p:RuntimeIdentifiers=maccatalyst-x64;maccatalyst-arm64 -p:BuildIpa=True -o ./artifacts

dotnet publish -f net8.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=com.walltech.mysolarcells.beta.keystore -p:AndroidSigningKeyAlias=com.walltech.mysolarcells.beta -p:AndroidSigningKeyPass=Digital4818 -p:AndroidSigningStorePass=Digital4818