# MySolarCells

Release App
dotnet publish MySolarCells.csproj -f net8.0-ios -c Release -p:RuntimeIdentifier=ios-arm64 -p:BuildIpa=True -o ./artifacts

dotnet publish MySolarCells.csproj -f net8.0-android -c Release -p:RuntimeIdentifier=android-arm64 -p:ApplicationId=com.walltech.mysolarcells.beta -p:AndroidSigningKeyAlias=com.walltech.mysolarcells.beta -p:AndroidKeyStore=true -p:AndroidEnableProfiler=true



dotnet publish MySolarCells.csproj -f net8.0-android -c Release -p:RuntimeIdentifier=android-arm64 -p:ApplicationId=ccom.walltech.mysolarcells.beta -p:AndroidSigningKeyPass=Digital4818 -p:AndroidSigningStorePass=Digital4818 -p:AndroidSigningKeyAlias=ccom.walltech.mysolarcells.beta -p:AndroidKeyStore=true -p:AndroidEnableProfiler=true

dotnet publish LiberoClub.csproj -f net8.0-android -c Release -p:RuntimeIdentifier=android-arm64 -p:ApplicationId=com.evry.liberoclub.test -p:AndroidSigningKeyPass=$(Evry_Android_Cert_Passw) -p:AndroidSigningStorePass=$(Evry_Android_Cert_Passw) /p:AndroidSigningKeyStore=$(System.DefaultWorkingDirectory)\\com.evry.liberoclub.test.keystore -p:AndroidSigningKeyAlias=com.evry.liberoclub.test -p:AndroidKeyStore=true -p:AndroidEnableProfiler=true