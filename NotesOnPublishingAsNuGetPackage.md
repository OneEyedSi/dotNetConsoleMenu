Notes on Publishing as a NuGet Package in Visual Studio
=======================================================
Simon Elms, 13 Sep 2020

All the properties once defined in a *.nuspec file are now defined in the Gold.ConsoleMenu.csproj 
file.  The easiest way to edit them is via the Gold.ConsoleMenu project Properties window > Package 
tab.

Once the properties have been updated, set the project to Release mode then in Solution Explorer, 
right-click on the Gold.ConsoleMenu project node and select Pack from the context menu.  This will 
create a *.nupkg file in the bin\Release folder.  Copy the full path of the *.nupkg file from the 
Output window.

Open a Developer Command Prompt window and execute the following to publish the NuGet package to 
NuGet.org: 

    dotnet nuget push {full path to *.nupkg file} --api-key {NuGet.org API key for my account} --source https://api.nuget.org/v3/index.json

It may take a few minutes for the new package to be validated, and to appear in the NuGet.org 
website.