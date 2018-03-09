
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionFile = GetFiles("D:/OPENIT/baseeam-web-app/BaseEAM/*.sln").First();
var solution = new Lazy<SolutionParserResult>(() => ParseSolution(solutionFile));

// Define directories.
var buildWebDir = Directory("D:/OPENIT/baseeam-web-app/BaseEAM/BaseEAM.Web/bin") + Directory(configuration);
var distWebDir = Directory("D:/OPENIT/baseeam-build/Build/Web");

var buildWorkflowDir = Directory("D:/OPENIT/BaseEAM_Cloud_Workflow/BaseEAM.Workflow/BaseEAM.WorkflowService/bin") + Directory(configuration);
var distWorkflowDir = Directory("D:/OPENIT/baseeam-build/Build/WorkflowService");

var buildBkgDir = Directory("D:/OPENIT/baseeam-background-service/BaseEAM.BackgroundService/BaseEAM.BackgroundService/bin") + Directory(configuration);
var distBkgDir = Directory("D:/OPENIT/baseeam-build/Build/BackgroundService");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildWebDir);
	CleanDirectory(buildWorkflowDir);
	CleanDirectory(buildBkgDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("D:/OPENIT/baseeam-web-app/BaseEAM/BaseEAM.sln");
	NuGetRestore("D:/OPENIT/BaseEAM_Cloud_Workflow/BaseEAM.Workflow/BaseEAM.Workflow.sln");
	NuGetRestore("D:/OPENIT/baseeam-background-service/BaseEAM.BackgroundService/BaseEAM.BackgroundService.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("D:/OPENIT/baseeam-web-app/BaseEAM/BaseEAM.sln", settings =>
        settings.SetConfiguration(configuration));
		
		// Use MSBuild
      MSBuild("D:/OPENIT/BaseEAM_Cloud_Workflow/BaseEAM.Workflow/BaseEAM.Workflow.sln", settings =>
        settings.SetConfiguration(configuration));
		
		// Use MSBuild
      MSBuild("D:/OPENIT/baseeam-background-service/BaseEAM.BackgroundService/BaseEAM.BackgroundService.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
    }
});

Task("Publish")
    .IsDependentOn("Build")
    .Does(() =>
{
	var webProject = solution.Value
			.Projects
			.Where(p => p.Name.EndsWith(".Web"))
			.First();
			
	DotNetBuild(webProject.Path, settings => settings
		.SetConfiguration(configuration)
		.WithProperty("DeployOnBuild", "true")
		.WithProperty("WebPublishMethod", "FileSystem")
		.WithProperty("DeployTarget", "WebPublish")
		.WithProperty("publishUrl", distWebDir)
		.SetVerbosity(Verbosity.Minimal));
		
	CopyDirectory(buildWorkflowDir, distWorkflowDir);
	
	CopyDirectory(buildBkgDir, distBkgDir);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Publish");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
