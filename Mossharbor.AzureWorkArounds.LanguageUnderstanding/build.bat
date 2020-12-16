REM msbuild /t:pack /p:NuspecFile=Package.nuspec Mossharbor.AzureWorkArounds.LanguageUnderstanding.csproj
dotnet build
dotnet pack -p:NuspecFile=.\Package.nuspec