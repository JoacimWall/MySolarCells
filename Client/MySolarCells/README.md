# MySolarCells

Release App IOS
dotnet publish MySolarCells.csproj -f net8.0-ios -c Release -p:RuntimeIdentifier=ios-arm64 -p:BuildIpa=True -o ./artifacts
Release App Android
dotnet publish -f net8.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=com.walltech.mysolarcells.beta.keystore -p:AndroidSigningKeyAlias=com.walltech.mysolarcells.beta -p:AndroidSigningKeyPass=Digital4818 -p:AndroidSigningStorePass=Digital4818

Release App Maccatalyst
dotnet publish MySolarCells.csproj -f net8.0-maccatalyst -c Release -p:RuntimeIdentifiers=maccatalyst-x64;maccatalyst-arm64 -p:BuildIpa=True -o ./artifacts


Data Folder
Ifall installerad via TestFlight på Mac os
/Users/joacimwall/Library/Containers/My Solar Cells/Data/Libary
Debug maccatalyst
/Users/joacimwall/Library/MySolarCells